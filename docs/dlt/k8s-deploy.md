# Kubernetes Deployment Guide для Hyperledger Fabric

**Версия:** 1.0  
**Дата:** 2024-12-19  
**Окружение:** Production-ready

---

## Обзор

Данный документ описывает процесс развертывания и управления Hyperledger Fabric сетью в Kubernetes кластере.

### Компоненты

- **Orderer**: 3-5 узлов (Raft консенсус)
- **Peer**: 2+ узла на организацию
- **CA**: 1-2 узла на организацию
- **CouchDB**: 1 экземпляр на peer

---

## Предварительные требования

### Инфраструктура

- Kubernetes кластер версии 1.24+
- Helm 3.8+
- kubectl настроен
- Доступ к container registry
- Vault или Sealed Secrets для управления секретами

### Storage

- StorageClass для persistent volumes
- Минимальные требования:
  - Orderer: 10-50Gi на узел
  - Peer: 50-100Gi на узел
  - CouchDB: 20-50Gi на peer
  - CA: 10-20Gi на узел

---

## Быстрый старт

### 1. Создание namespace

```bash
kubectl apply -f ops/infra/k8s/namespace.yaml
```

### 2. Создание StorageClass

```bash
kubectl apply -f ops/infra/k8s/storageclass.yaml
```

### 3. Подготовка секретов

#### Dev (Sealed Secrets)

```bash
# Создать sealed secrets из crypto-material
kubectl create secret generic fabric-orderer-msp \
  --from-file=ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/msp \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/orderer-msp.yaml

kubectl create secret generic fabric-orderer-tls \
  --from-file=ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/orderer-tls.yaml

# Аналогично для peer и CA
```

#### Prod (Vault)

```bash
# Загрузить секреты в Vault
vault kv put secret/fabric/orderer/msp @ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/msp
vault kv put secret/fabric/orderer/tls @ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls
```

### 4. Установка Orderer

```bash
# Dev
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-orderer/values.yaml

# Prod
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-orderer/values.yaml \
  --values ops/infra/helm/fabric-orderer/values.prod.yaml
```

### 5. Установка Peer

```bash
# Dev
helm install fabric-peer ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-peer/values.yaml

# Prod
helm install fabric-peer ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-peer/values.yaml \
  --values ops/infra/helm/fabric-peer/values.prod.yaml
```

### 6. Установка CA

```bash
# Dev
helm install fabric-ca ops/infra/helm/fabric-ca \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-ca/values.yaml

# Prod
helm install fabric-ca ops/infra/helm/fabric-ca \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-ca/values.yaml \
  --values ops/infra/helm/fabric-ca/values.prod.yaml
```

---

## Порядок развертывания каналов

### 1. Создание genesis block

```bash
# На локальной машине или в job
configtxgen -profile Genesis -channelID system-channel -outputBlock genesis.block
```

### 2. Создание channel transaction

```bash
configtxgen -profile CfaMainChannel -channelID cfa-main -outputCreateChannelTx cfa-main.tx
```

### 3. Создание канала

```bash
# Создать channel через peer CLI в pod
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  peer channel create \
    -o fabric-orderer:7050 \
    -c cfa-main \
    -f /etc/hyperledger/fabric/channel-artifacts/cfa-main.tx \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
```

### 4. Присоединение peers к каналу

```bash
# Для каждого peer
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  peer channel join \
    -b /etc/hyperledger/fabric/cfa-main.block
```

---

## Добавление организаций

### 1. Обновление конфигурации

```bash
# Обновить configtx.yaml с новой организацией
# Сгенерировать новые crypto-material
cryptogen generate --config=crypto-config.yaml
```

### 2. Обновление channel configuration

```bash
# Получить текущую конфигурацию
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  peer channel fetch config config_block.pb -c cfa-main

# Извлечь конфигурацию, добавить новую организацию, обновить
# Подписать обновление
# Отправить обновление
```

### 3. Установка peer для новой организации

```bash
helm install fabric-peer-neworg ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  --set organization.name=neworg \
  --set organization.mspId=NewOrgMSP \
  --values ops/infra/helm/fabric-peer/values.yaml
```

---

## Ротация сертификатов

### 1. Генерация новых сертификатов

```bash
# Использовать CA для генерации новых сертификатов
fabric-ca-client enroll -u https://admin:adminpw@ca.ois-dev.example.com:7054
```

### 2. Обновление секретов

```bash
# Dev (Sealed Secrets)
kubectl create secret generic fabric-peer-tls-new \
  --from-file=ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/peer-tls-new.yaml

kubectl apply -f sealed-secrets/peer-tls-new.yaml
```

### 3. Rolling update

```bash
# Обновить deployment с новыми секретами
kubectl set env deployment/fabric-peer TLS_SECRET_NAME=fabric-peer-tls-new -n fabric-network
kubectl rollout restart deployment/fabric-peer -n fabric-network
```

### 4. Проверка

```bash
# Убедиться, что все pods работают
kubectl get pods -n fabric-network
kubectl logs -f fabric-peer-0 -n fabric-network
```

---

## Disaster Recovery (DR)

### 1. Backup

#### Регулярные бэкапы

```bash
# Backup crypto-material
kubectl get secret fabric-orderer-msp -n fabric-network -o yaml > backup/orderer-msp-$(date +%Y%m%d).yaml

# Backup persistent volumes
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  tar czf /tmp/peer-data-backup.tar.gz /var/hyperledger/production

kubectl cp fabric-peer-0:/tmp/peer-data-backup.tar.gz backup/peer-data-$(date +%Y%m%d).tar.gz -n fabric-network
```

#### Автоматизация бэкапов

```bash
# Создать CronJob для бэкапов
kubectl apply -f ops/infra/k8s/backup-cronjob.yaml
```

### 2. Восстановление

#### Восстановление из бэкапа

```bash
# 1. Восстановить секреты
kubectl apply -f backup/orderer-msp-YYYYMMDD.yaml

# 2. Восстановить данные
kubectl cp backup/peer-data-YYYYMMDD.tar.gz fabric-peer-0:/tmp/peer-data-backup.tar.gz -n fabric-network
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  tar xzf /tmp/peer-data-backup.tar.gz -C /

# 3. Перезапустить pods
kubectl rollout restart deployment/fabric-peer -n fabric-network
```

#### Восстановление в новом кластере

```bash
# 1. Создать namespace и storage
kubectl apply -f ops/infra/k8s/namespace.yaml
kubectl apply -f ops/infra/k8s/storageclass.yaml

# 2. Восстановить секреты
kubectl apply -f backup/*.yaml

# 3. Установить Helm charts
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --values ops/infra/helm/fabric-orderer/values.yaml

# 4. Восстановить данные в PVC
# (зависит от storage backend)

# 5. Перезапустить все компоненты
```

### 3. DR тестирование

```bash
# Регулярные DR-учения (ежемесячно)
# 1. Создать тестовый кластер
# 2. Восстановить из бэкапа
# 3. Проверить работоспособность
# 4. Документировать результаты
```

---

## Chaincode Deployment

### 1. Сборка и публикация

```bash
# Использовать chaincode-build chart
helm install chaincode-build ops/infra/helm/chaincode-build \
  --namespace fabric-network \
  --values ops/infra/helm/chaincode-build/values.yaml \
  --set chaincode[0].version=1.1
```

### 2. Установка

```bash
# Job установки запустится автоматически после сборки
kubectl get jobs -n fabric-network | grep chaincode-install
```

### 3. Утверждение

```bash
# Job утверждения запустится автоматически
kubectl get jobs -n fabric-network | grep chaincode-approve
```

### 4. Коммит

```bash
# Job коммита запустится автоматически
kubectl get jobs -n fabric-network | grep chaincode-commit
```

### 5. Rollback

```bash
# Если нужно откатить версию
helm upgrade chaincode-build ops/infra/helm/chaincode-build \
  --namespace fabric-network \
  --set rollback.enabled=true \
  --set rollback.chaincode=issuance \
  --set rollback.previousVersion=1.0 \
  --set rollback.strategy=immediate
```

---

## Мониторинг

### ServiceMonitors

ServiceMonitors автоматически создаются для всех компонентов. Убедитесь, что Prometheus Operator установлен:

```bash
kubectl get servicemonitors -n fabric-network
```

### Дашборды Grafana

Импортировать дашборды из `ops/infra/grafana-dashboards.json`:

```bash
# Через Grafana UI или API
curl -X POST http://grafana:3000/api/dashboards/db \
  -H "Content-Type: application/json" \
  -d @ops/infra/grafana-dashboards.json
```

---

## Troubleshooting

### Pods не запускаются

```bash
# Проверить логи
kubectl logs -f fabric-orderer-0 -n fabric-network

# Проверить события
kubectl describe pod fabric-orderer-0 -n fabric-network

# Проверить секреты
kubectl get secrets -n fabric-network
```

### PVC не создаются

```bash
# Проверить StorageClass
kubectl get storageclass

# Проверить PVC
kubectl get pvc -n fabric-network
kubectl describe pvc fabric-orderer-data -n fabric-network
```

### Network policies блокируют трафик

```bash
# Временно отключить network policies
helm upgrade fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --set networkPolicy.enabled=false
```

---

## Проверка готовности

### Health checks

```bash
# Проверить health всех компонентов
kubectl get pods -n fabric-network
kubectl exec -it fabric-peer-0 -n fabric-network -- peer node status
```

### ServiceMonitors

```bash
# Проверить, что ServiceMonitors активны
kubectl get servicemonitors -n fabric-network
```

### Тестирование chaincode

```bash
# Invoke через peer CLI
kubectl exec -it fabric-peer-0 -n fabric-network -- \
  peer chaincode invoke \
    -o fabric-orderer:7050 \
    -C cfa-main \
    -n issuance \
    -c '{"function":"Get","Args":["test"]}'
```

---

## Команды для быстрого доступа

```bash
# Установка всех компонентов (dev)
helm install fabric-orderer ops/infra/helm/fabric-orderer --namespace fabric-network
helm install fabric-peer ops/infra/helm/fabric-peer --namespace fabric-network
helm install fabric-ca ops/infra/helm/fabric-ca --namespace fabric-network

# Обновление
helm upgrade fabric-orderer ops/infra/helm/fabric-orderer --namespace fabric-network
helm upgrade fabric-peer ops/infra/helm/fabric-peer --namespace fabric-network

# Удаление
helm uninstall fabric-orderer --namespace fabric-network
helm uninstall fabric-peer --namespace fabric-network
helm uninstall fabric-ca --namespace fabric-network

# Проверка манифестов
helm template fabric-orderer ops/infra/helm/fabric-orderer --values ops/infra/helm/fabric-orderer/values.yaml
```

---

## Дополнительная документация

- **Архитектура**: `docs/architecture/13-HLF-Network-Design.md`
- **Dev Network**: `docs/dlt/dev-network.md`
- **Helm Charts**: `ops/infra/helm/`

---

**Статус**: ✅ COMPLETE  
**Дата**: 2024-12-19

