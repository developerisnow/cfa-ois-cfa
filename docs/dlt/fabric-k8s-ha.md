# Hyperledger Fabric HA в Kubernetes

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** Blockchain Architect / DevOps

---

## Содержание

1. [Обзор](#обзор)
2. [Архитектура HA](#архитектура-ha)
3. [Развёртывание](#развёртывание)
4. [Chaincode Lifecycle](#chaincode-lifecycle)
5. [Secrets Management](#secrets-management)
6. [Observability](#observability)
7. [Тестирование](#тестирование)
8. [Troubleshooting](#troubleshooting)

---

## Обзор

Высокодоступная конфигурация Hyperledger Fabric в Kubernetes для проекта OIS-CFA.

### Компоненты

- **Orderer** — Raft консенсус (3/5 узлов)
- **Peer** — 2+ узла на организацию
- **CA** — Certificate Authority
- **CouchDB** — State database с PV/PVC
- **Chaincode** — issuance, registry

### Требования

- Kubernetes 1.24+
- Helm 3.8+
- StorageClass для PV
- Prometheus Operator (для ServiceMonitor)
- Vault или Sealed Secrets

---

## Архитектура HA

### Orderer (Raft)

**Конфигурация:**
- **Dev:** 3 узла (минимально для Raft)
- **Prod:** 5 узлов (лучшая отказоустойчивость)

**StatefulSet:**
- Уникальный PVC на каждый узел
- Стабильные имена: `fabric-orderer-0`, `fabric-orderer-1`, etc.
- Headless service для peer-to-peer связи

**Raft консенсус:**
- Leader election автоматически
- Терпимость к отказу: (N-1)/2 узлов
- Для 5 узлов: до 2 отказов

### Peer

**Конфигурация:**
- **Dev:** 2 узла
- **Prod:** 3+ узла

**Deployment:**
- Pod anti-affinity для распределения по нодам
- Общий CouchDB или отдельный на реплику

**Gossip:**
- Автоматическая синхронизация между пирами
- Leader election для организации

### CouchDB

**Storage:**
- PVC на каждый peer (если `createPerReplica: true`)
- Или общий PVC (ReadWriteMany) для всех реплик
- Минимум 20Gi на peer

**HA:**
- CouchDB в sidecar контейнере
- Репликация через Gossip protocol

---

## Развёртывание

### 1. Подготовка namespace

```bash
kubectl create namespace fabric-network
```

### 2. Создание секретов

#### Sealed Secrets (dev/staging)

```bash
# Создать MSP secret
kubectl create secret generic fabric-peer-msp \
  --from-file=msp=./ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/msp \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/fabric-peer-msp.yaml

# Создать TLS secret
kubectl create secret generic fabric-peer-tls \
  --from-file=tls=./ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/fabric-peer-tls.yaml
```

#### Vault (production)

```bash
# Сохранить MSP в Vault
vault kv put secret/fabric/ois-dev/msp \
  @ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/msp

# Сохранить TLS в Vault
vault kv put secret/fabric/ois-dev/tls \
  @ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls
```

### 3. Установка Orderer

```bash
# Dev (3 узла)
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --set replicaCount=3 \
  --set secrets.type=sealed-secrets

# Prod (5 узлов)
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  --set replicaCount=5 \
  --set secrets.type=vault
```

### 4. Установка Peer

```bash
# Dev (2 узла)
helm install fabric-peer ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  --set replicaCount=2 \
  --set couchdb.storage.createPerReplica=false \
  --set secrets.type=sealed-secrets

# Prod (3+ узла)
helm install fabric-peer ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  --set replicaCount=3 \
  --set couchdb.storage.createPerReplica=true \
  --set secrets.type=vault
```

### 5. Установка CA

```bash
helm install fabric-ca ops/infra/helm/fabric-ca \
  --namespace fabric-network \
  --set secrets.type=sealed-secrets
```

### 6. Создание канала

```bash
# Создать genesis block
kubectl exec -n fabric-network fabric-orderer-0 -- \
  configtxgen -profile CFAChannel -outputBlock /tmp/cfa-main-genesis.block \
  -channelID cfa-main

# Создать канал
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer channel create -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -c cfa-main -f /tmp/channel.tx --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
```

---

## Chaincode Lifecycle

### 1. Build и Package

```bash
# Build chaincode images
cd chaincode/issuance
docker build -t registry.gitlab.com/ois-cfa/ois-cfa/fabric-chaincode/issuance:1.0 .
docker push registry.gitlab.com/ois-cfa/ois-cfa/fabric-chaincode/issuance:1.0
```

### 2. Install

```bash
# Установить chaincode lifecycle chart
helm install chaincode-lifecycle ops/infra/helm/chaincode-lifecycle \
  --namespace fabric-network \
  --set chaincode[0].packageId="" \
  --set imageRegistry=registry.gitlab.com/ois-cfa/ois-cfa

# Запустить install job
kubectl create job --from=cronjob/chaincode-install-issuance-1.0 \
  chaincode-install-issuance-1.0-manual -n fabric-network
```

Или через Helm:

```bash
helm template chaincode-lifecycle ops/infra/helm/chaincode-lifecycle \
  | kubectl apply -f -
```

### 3. Approve

```bash
# После install, получить package ID
PACKAGE_ID=$(kubectl logs -n fabric-network job/chaincode-install-issuance-1.0-peer0 | grep "Package ID" | awk '{print $3}')

# Обновить values с package ID
helm upgrade chaincode-lifecycle ops/infra/helm/chaincode-lifecycle \
  --namespace fabric-network \
  --set chaincode[0].packageId=$PACKAGE_ID

# Запустить approve job
kubectl create job --from=cronjob/chaincode-approve-issuance-1.0 \
  chaincode-approve-issuance-1.0-manual -n fabric-network
```

### 4. Commit

```bash
# После approve на всех пирах
kubectl create job --from=cronjob/chaincode-commit-issuance-1.0 \
  chaincode-commit-issuance-1.0-manual -n fabric-network
```

### 5. Invoke

```bash
# Invoke через peer
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode invoke \
  -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -C cfa-main \
  -n issuance \
  -c '{"function":"Issue","Args":["asset1","1000"]}' \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
```

---

## Secrets Management

### Sealed Secrets (dev/staging)

```bash
# Установить Sealed Secrets controller
kubectl apply -f https://github.com/bitnami-labs/sealed-secrets/releases/download/v0.24.0/controller.yaml

# Создать sealed secret
kubectl create secret generic fabric-peer-msp \
  --from-file=msp=./msp.tar.gz \
  --dry-run=client -o yaml | kubeseal -o yaml > sealed-secrets/fabric-peer-msp.yaml

# Применить
kubectl apply -f sealed-secrets/fabric-peer-msp.yaml
```

### Vault (production)

```bash
# Настроить Vault integration
helm install fabric-peer ops/infra/helm/fabric-peer \
  --set secrets.type=vault \
  --set secrets.rotation.enabled=true \
  --set secrets.rotation.vaultAddr=https://vault.example.com \
  --set secrets.rotation.vaultRole=fabric-peer-rotation
```

### Ротация секретов

**Автоматическая (CronJob):**
```yaml
secrets:
  rotation:
    enabled: true
    schedule: "0 2 * * 0"  # Weekly on Sunday at 2 AM
```

**Ручная:**
```bash
# Обновить секреты в Vault
vault kv put secret/fabric/ois-dev/msp @new-msp.json

# Перезапустить pods
kubectl rollout restart deployment/fabric-peer -n fabric-network
```

---

## Observability

### ServiceMonitor

ServiceMonitors автоматически создаются при установке charts:

```yaml
serviceMonitor:
  enabled: true
  interval: 30s
  path: /metrics
```

### Grafana Dashboards

Импортировать дашборды из `ops/infra/grafana-dashboards-fabric.json`:

1. **Fabric Peer Metrics:**
   - Peer Status
   - Block Height
   - Transaction Rate
   - Chaincode Invocations
   - Gossip Messages
   - CouchDB Operations

2. **Fabric Orderer Metrics:**
   - Orderer Status
   - Block Creation Rate
   - Raft Leader
   - Raft Term
   - Transaction Rate
   - Batch Size

3. **Fabric Chaincode Metrics:**
   - Chaincode Invocations (per chaincode)
   - Chaincode Latency
   - Chaincode Errors

### Prometheus Queries

```promql
# Peer block height
fabric_peer_block_height{channel="cfa-main"}

# Transaction rate
rate(fabric_peer_transactions_total[5m])

# Chaincode invocations
rate(fabric_peer_chaincode_invocations_total{chaincode="issuance"}[5m])

# Raft leader
fabric_orderer_raft_leader
```

---

## Тестирование

### 1. Проверка канала

```bash
# Список каналов
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer channel list

# Информация о канале
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer channel getinfo -c cfa-main
```

### 2. Invoke chaincode

```bash
# Issue asset
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode invoke \
  -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -C cfa-main \
  -n issuance \
  -c '{"function":"Issue","Args":["ASSET001","1000000","2025-12-31"]}' \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt

# Query chaincode
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode query \
  -C cfa-main \
  -n issuance \
  -c '{"function":"GetAsset","Args":["ASSET001"]}'
```

### 3. E2E тест (issue→buy→redeem)

```bash
# 1. Issue
INVOKE_RESULT=$(kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode invoke \
  -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -C cfa-main \
  -n issuance \
  -c '{"function":"Issue","Args":["ASSET001","1000000"]}' \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt)

TX_HASH=$(echo $INVOKE_RESULT | grep -oP 'txid: \K[^ ]+')
echo "Transaction Hash: $TX_HASH"

# 2. Buy (registry chaincode)
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode invoke \
  -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -C cfa-main \
  -n registry \
  -c '{"function":"Transfer","Args":["INVESTOR001","10000"]}' \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt

# 3. Redeem
kubectl exec -n fabric-network fabric-peer-0 -- \
  peer chaincode invoke \
  -o fabric-orderer.fabric-network.svc.cluster.local:7050 \
  -C cfa-main \
  -n registry \
  -c '{"function":"Redeem","Args":["INVESTOR001","5000"]}' \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
```

---

## Troubleshooting

### Peer не запускается

```bash
# Проверить логи
kubectl logs -n fabric-network deployment/fabric-peer

# Проверить секреты
kubectl get secrets -n fabric-network | grep fabric-peer

# Проверить PVC
kubectl get pvc -n fabric-network
```

### Orderer не выбирает leader

```bash
# Проверить Raft статус
kubectl exec -n fabric-network fabric-orderer-0 -- \
  orderer cluster status

# Проверить логи
kubectl logs -n fabric-network statefulset/fabric-orderer
```

### Chaincode не устанавливается

```bash
# Проверить job логи
kubectl logs -n fabric-network job/chaincode-install-issuance-1.0-peer0

# Проверить доступность образа
kubectl run test --image=registry.gitlab.com/ois-cfa/ois-cfa/fabric-chaincode/issuance:1.0 --rm -it --restart=Never -- /bin/sh
```

### CouchDB не работает

```bash
# Проверить CouchDB pod
kubectl get pods -n fabric-network | grep couchdb

# Проверить PVC
kubectl get pvc -n fabric-network | grep couchdb

# Проверить логи
kubectl logs -n fabric-network deployment/fabric-peer -c couchdb
```

---

## Ссылки

- [Hyperledger Fabric Documentation](https://hyperledger-fabric.readthedocs.io/)
- [Fabric on Kubernetes](https://hyperledger-fabric.readthedocs.io/en/latest/deploy_chaincode.html)
- [Raft Consensus](https://hyperledger-fabric.readthedocs.io/en/latest/raft_configuration.html)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

