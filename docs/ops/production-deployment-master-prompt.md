# 🎯 МАСТЕР-ПРОМПТ: Выкатка ОИС ЦФА в Production

**Версия:** 2.0  
**Дата:** 2025-01-27  
**Статус:** Активный  
**Владелец:** Technical Lead / DevOps

---

## 📋 КОНТЕКСТ И ЦЕЛЬ

**Цель:** Выкатить всю систему ОИС ЦФА в production на Kubernetes кластер Timeweb Cloud с доступом через домен `https://cfa.capital/`.

**Текущее состояние:**
- ✅ GitLab: https://git.telex.global
- ✅ Kubernetes кластер: Timeweb Cloud (1 worker node)
- ✅ Домен: https://cfa.capital/
- ✅ Ingress NGINX: установлен
- ✅ GitLab Agent: установлен и работает
- ✅ GitLab Runner: установлен, но были проблемы с ресурсами (исправлено)
- ⚠️ Build jobs: не запускаются в ветке `infra` (нужно проверить правила)
- ⚠️ Нет развернутых приложений в кластере

---

## 🏗️ АРХИТЕКТУРА СИСТЕМЫ

### Компоненты для выкатки

#### 1. **Platform Services** (инфраструктурные зависимости)
- PostgreSQL (база данных)
- Redis (кэш, сессии)
- Kafka (event streaming)
- Keycloak (SSO/OIDC)
- Vault (secrets management, опционально)

#### 2. **DLT Infrastructure** (Hyperledger Fabric)
- Fabric CA (Certificate Authority)
- Fabric Orderer (консенсус, Raft)
- Fabric Peer (валидация транзакций)
- Chaincode (issuance, registry)

#### 3. **Backend Services** (.NET 9)
- `api-gateway` - точка входа, маршрутизация
- `identity` - аутентификация, авторизация
- `issuance` - выпуск ЦФА
- `registry` - реестр ЦФА
- `settlement` - расчеты, выплаты
- `compliance` - комплаенс, KYC
- `fabric-gateway` - интеграция с Fabric
- `bank-nominal` - интеграция с банком (mock)

#### 4. **Frontend Applications** (Next.js 15)
- `portal-issuer` - портал эмитента
- `portal-investor` - портал инвестора
- `backoffice` - административный портал
- `broker-portal` - портал брокера (опционально)

---

## 📊 ПОРЯДОК ВЫКАТКИ (Dependency Graph)

```
┌─────────────────────────────────────────────────────────┐
│ ФАЗА 0: Инфраструктура (Platform Services)               │
├─────────────────────────────────────────────────────────┤
│ 1. PostgreSQL                                            │
│ 2. Redis                                                 │
│ 3. Kafka + Zookeeper                                     │
│ 4. Keycloak (зависит от PostgreSQL)                     │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ ФАЗА 1: DLT Infrastructure (Hyperledger Fabric)       │
├─────────────────────────────────────────────────────────┤
│ 1. Fabric CA                                             │
│ 2. Fabric Orderer (Raft)                                 │
│ 3. Fabric Peer                                           │
│ 4. Chaincode (issuance, registry)                       │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ ФАЗА 2: Backend Services (микросервисы)                  │
├─────────────────────────────────────────────────────────┤
│ 1. Identity Service (зависит от Keycloak, PostgreSQL)   │
│ 2. Fabric Gateway (зависит от Fabric Peer)              │
│ 3. Registry Service (зависит от Fabric, PostgreSQL)     │
│ 4. Issuance Service (зависит от Registry, PostgreSQL)  │
│ 5. Settlement Service (зависит от Registry, Kafka)      │
│ 6. Compliance Service (зависит от Identity, PostgreSQL)  │
│ 7. Bank Nominal (mock, независим)                       │
│ 8. API Gateway (зависит от всех сервисов)                │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│ ФАЗА 3: Frontend Applications (Next.js)                  │
├─────────────────────────────────────────────────────────┤
│ 1. Portal Issuer (зависит от API Gateway)                │
│ 2. Portal Investor (зависит от API Gateway)            │
│ 3. Backoffice (зависит от API Gateway)                   │
└─────────────────────────────────────────────────────────┘
```

---

## 🚀 ПЛАН ВЫКАТКИ (пошагово)

### **ЭТАП 0: Подготовка и диагностика**

#### 0.1. Проверить текущее состояние
```bash
# Проверить кластер
kubectl cluster-info
kubectl get nodes
kubectl get namespaces

# Проверить GitLab Runner
kubectl get pods -n gitlab-runner
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50

# Проверить GitLab Agent
kubectl get pods -n gitlab-agent
kubectl logs -n gitlab-agent -l app=gitlab-agent --tail=50

# Проверить Ingress
kubectl get ingress -A
```

#### 0.2. Исправить правила CI/CD для ветки `infra`
**Проблема:** Build jobs не запускаются в ветке `infra`  
**Решение:** Обновить правила в `.gitlab-ci.yml`:

```yaml
# Текущее правило (запускается для всех веток):
rules:
  - if: '$CI_COMMIT_BRANCH || $CI_COMMIT_TAG'

# Добавить явное правило для ветки infra:
rules:
  - if: '$CI_COMMIT_BRANCH || $CI_COMMIT_TAG'
  - if: '$CI_COMMIT_BRANCH == "infra"'
```

#### 0.3. Проверить наличие Dockerfile для всех компонентов
```bash
# Backend Services
ls -la apps/api-gateway/Dockerfile
ls -la services/*/Dockerfile

# Frontend Apps
ls -la apps/*/Dockerfile

# Chaincode (если нужен Dockerfile для сборки)
ls -la chaincode/*/Dockerfile
```

#### 0.4. Проверить наличие Helm Charts
```bash
ls -la ops/infra/helm/*/
```

---

### **ЭТАП 1: Platform Services (PostgreSQL, Redis, Kafka, Keycloak)**

**Цель:** Развернуть базовую инфраструктуру

#### 1.1. PostgreSQL
```bash
# Использовать готовый Helm chart или создать манифесты
helm repo add bitnami https://charts.bitnami.com/bitnami
helm install postgresql bitnami/postgresql \
  --namespace ois-cfa \
  --create-namespace \
  --set auth.postgresPassword=ois_prod_password \
  --set auth.database=ois \
  --set persistence.size=20Gi

# Проверить
kubectl get pods -n ois-cfa -l app.kubernetes.io/name=postgresql
```

#### 1.2. Redis
```bash
helm install redis bitnami/redis \
  --namespace ois-cfa \
  --set auth.password=ois_redis_password \
  --set persistence.size=10Gi

# Проверить
kubectl get pods -n ois-cfa -l app.kubernetes.io/name=redis
```

#### 1.3. Kafka + Zookeeper
```bash
helm repo add confluentinc https://confluentinc.github.io/cp-helm-charts/
helm install kafka confluentinc/cp-helm-charts \
  --namespace ois-cfa \
  --set cp-kafka.persistence.size=20Gi

# Или использовать Strimzi operator (рекомендуется для production)
```

#### 1.4. Keycloak
```bash
helm repo add codecentric https://codecentric.github.io/helm-charts
helm install keycloak codecentric/keycloak \
  --namespace ois-cfa \
  --set postgresql.enabled=false \
  --set externalDatabase.host=postgresql.ois-cfa.svc.cluster.local \
  --set externalDatabase.database=keycloak \
  --set externalDatabase.user=postgres \
  --set externalDatabase.password=ois_prod_password

# Проверить
kubectl get pods -n ois-cfa -l app=keycloak
```

**Критерий успеха:** Все platform services в статусе `Running`

---

### **ЭТАП 2: DLT Infrastructure (Hyperledger Fabric)**

**Цель:** Развернуть Hyperledger Fabric сеть

#### 2.1. Fabric CA
```bash
helm install fabric-ca ops/infra/helm/fabric-ca \
  --namespace fabric-network \
  --create-namespace \
  -f ops/infra/helm/fabric-ca/values-prod.yaml

# Проверить
kubectl get pods -n fabric-network -l app=fabric-ca
```

#### 2.2. Fabric Orderer (Raft)
```bash
helm install fabric-orderer ops/infra/helm/fabric-orderer \
  --namespace fabric-network \
  -f ops/infra/helm/fabric-orderer/values-prod.yaml

# Проверить
kubectl get pods -n fabric-network -l app=fabric-orderer
```

#### 2.3. Fabric Peer
```bash
helm install fabric-peer ops/infra/helm/fabric-peer \
  --namespace fabric-network \
  -f ops/infra/helm/fabric-peer/values-prod.yaml

# Проверить
kubectl get pods -n fabric-network -l app=fabric-peer
```

#### 2.4. Chaincode (issuance, registry)
```bash
# Собрать chaincode образы
cd chaincode/issuance
docker build -t $CI_REGISTRY_IMAGE/chaincode-issuance:$CI_COMMIT_SHA .
docker push $CI_REGISTRY_IMAGE/chaincode-issuance:$CI_COMMIT_SHA

cd ../registry
docker build -t $CI_REGISTRY_IMAGE/chaincode-registry:$CI_COMMIT_SHA .
docker push $CI_REGISTRY_IMAGE/chaincode-registry:$CI_COMMIT_SHA

# Установить через Helm
helm install chaincode-issuance ops/infra/helm/chaincode-lifecycle \
  --namespace fabric-network \
  --set chaincode.name=issuance \
  --set chaincode.image=$CI_REGISTRY_IMAGE/chaincode-issuance:$CI_COMMIT_SHA

helm install chaincode-registry ops/infra/helm/chaincode-lifecycle \
  --namespace fabric-network \
  --set chaincode.name=registry \
  --set chaincode.image=$CI_REGISTRY_IMAGE/chaincode-registry:$CI_COMMIT_SHA
```

**Критерий успеха:** Fabric сеть работает, chaincode установлен и запущен

---

### **ЭТАП 3: Backend Services**

**Цель:** Развернуть микросервисы в правильном порядке

#### 3.1. Identity Service
```bash
# Собрать образ (через CI/CD или вручную)
# Build job должен создать: $CI_REGISTRY_IMAGE/identity:$CI_COMMIT_SHA

# Выкатить через Helm
helm install identity ops/infra/helm/identity \
  --namespace ois-cfa \
  --create-namespace \
  --set image.repository=$CI_REGISTRY_IMAGE/identity \
  --set image.tag=$CI_COMMIT_SHA \
  --set postgresql.host=postgresql.ois-cfa.svc.cluster.local \
  --set keycloak.url=http://keycloak.ois-cfa.svc.cluster.local

# Проверить
kubectl get pods -n ois-cfa -l app=identity
kubectl logs -n ois-cfa -l app=identity --tail=50
```

#### 3.2. Fabric Gateway
```bash
helm install fabric-gateway ops/infra/helm/fabric-gateway \
  --namespace ois-cfa \
  --set fabric.peer.url=fabric-peer.fabric-network.svc.cluster.local:7051

# Проверить
kubectl get pods -n ois-cfa -l app=fabric-gateway
```

#### 3.3. Registry Service
```bash
helm install registry ops/infra/helm/registry \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/registry \
  --set image.tag=$CI_COMMIT_SHA \
  --set fabric.gateway.url=fabric-gateway.ois-cfa.svc.cluster.local \
  --set postgresql.host=postgresql.ois-cfa.svc.cluster.local

# Проверить
kubectl get pods -n ois-cfa -l app=registry
```

#### 3.4. Issuance Service
```bash
helm install issuance ops/infra/helm/issuance \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/issuance \
  --set image.tag=$CI_COMMIT_SHA \
  --set registry.url=http://registry.ois-cfa.svc.cluster.local \
  --set postgresql.host=postgresql.ois-cfa.svc.cluster.local

# Проверить
kubectl get pods -n ois-cfa -l app=issuance
```

#### 3.5. Settlement Service
```bash
helm install settlement ops/infra/helm/settlement \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/settlement \
  --set image.tag=$CI_COMMIT_SHA \
  --set registry.url=http://registry.ois-cfa.svc.cluster.local \
  --set kafka.brokers=kafka.ois-cfa.svc.cluster.local:9092

# Проверить
kubectl get pods -n ois-cfa -l app=settlement
```

#### 3.6. Compliance Service
```bash
helm install compliance ops/infra/helm/compliance \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/compliance \
  --set image.tag=$CI_COMMIT_SHA \
  --set identity.url=http://identity.ois-cfa.svc.cluster.local \
  --set postgresql.host=postgresql.ois-cfa.svc.cluster.local

# Проверить
kubectl get pods -n ois-cfa -l app=compliance
```

#### 3.7. Bank Nominal (mock)
```bash
helm install bank-nominal ops/infra/helm/bank-nominal \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/bank-nominal \
  --set image.tag=$CI_COMMIT_SHA

# Проверить
kubectl get pods -n ois-cfa -l app=bank-nominal
```

#### 3.8. API Gateway (последний, зависит от всех)
```bash
helm install api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  --create-namespace \
  -f ops/infra/helm/api-gateway/values-prod.yaml \
  --set image.repository=$CI_REGISTRY_IMAGE/api-gateway \
  --set image.tag=$CI_COMMIT_SHA \
  --set ingress.hosts[0].host=api.cfa.capital \
  --set services.identity.url=http://identity.ois-cfa.svc.cluster.local \
  --set services.issuance.url=http://issuance.ois-cfa.svc.cluster.local \
  --set services.registry.url=http://registry.ois-cfa.svc.cluster.local \
  --set services.settlement.url=http://settlement.ois-cfa.svc.cluster.local \
  --set services.compliance.url=http://compliance.ois-cfa.svc.cluster.local

# Проверить
kubectl get pods -n ois-cfa -l app=api-gateway
kubectl get ingress -n ois-cfa
curl https://api.cfa.capital/health
```

**Критерий успеха:** Все backend services в статусе `Running`, API Gateway доступен через Ingress

---

### **ЭТАП 4: Frontend Applications**

**Цель:** Развернуть Next.js приложения

#### 4.1. Portal Issuer
```bash
# Build job должен создать: $CI_REGISTRY_IMAGE/portal-issuer:$CI_COMMIT_SHA

helm install portal-issuer ops/infra/helm/portal-issuer \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/portal-issuer \
  --set image.tag=$CI_COMMIT_SHA \
  --set ingress.hosts[0].host=issuer.cfa.capital \
  --set env.NEXT_PUBLIC_API_URL=https://api.cfa.capital

# Проверить
kubectl get pods -n ois-cfa -l app=portal-issuer
curl https://issuer.cfa.capital
```

#### 4.2. Portal Investor
```bash
helm install portal-investor ops/infra/helm/portal-investor \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/portal-investor \
  --set image.tag=$CI_COMMIT_SHA \
  --set ingress.hosts[0].host=investor.cfa.capital \
  --set env.NEXT_PUBLIC_API_URL=https://api.cfa.capital

# Проверить
kubectl get pods -n ois-cfa -l app=portal-investor
curl https://investor.cfa.capital
```

#### 4.3. Backoffice
```bash
helm install backoffice ops/infra/helm/backoffice \
  --namespace ois-cfa \
  --set image.repository=$CI_REGISTRY_IMAGE/backoffice \
  --set image.tag=$CI_COMMIT_SHA \
  --set ingress.hosts[0].host=admin.cfa.capital \
  --set env.NEXT_PUBLIC_API_URL=https://api.cfa.capital

# Проверить
kubectl get pods -n ois-cfa -l app=backoffice
curl https://admin.cfa.capital
```

**Критерий успеха:** Все frontend приложения доступны через Ingress

---

## 🔧 АВТОМАТИЗАЦИЯ ЧЕРЕЗ CI/CD

### Обновить `.gitlab-ci.yml` для автоматической выкатки

#### Добавить deploy jobs для каждого компонента:

```yaml
# Deploy Platform Services
deploy:postgresql:
  stage: deploy
  image: bitnami/helm:latest
  script:
    - helm upgrade --install postgresql bitnami/postgresql ...
  rules:
    - if: '$CI_COMMIT_BRANCH == "infra"'
    - if: '$CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH'

# Deploy Backend Services
deploy:identity:
  stage: deploy
  image: bitnami/helm:latest
  dependencies:
    - build:identity
  script:
    - helm upgrade --install identity ops/infra/helm/identity ...
  rules:
    - if: '$CI_COMMIT_BRANCH == "infra"'
    - if: '$CI_COMMIT_BRANCH == $CI_DEFAULT_BRANCH'

# И так далее для каждого компонента...
```

---

## 📋 ЧЕКЛИСТ ГОТОВНОСТИ

### Инфраструктура
- [ ] Kubernetes кластер работает
- [ ] Ingress NGINX установлен
- [ ] GitLab Agent работает
- [ ] GitLab Runner работает
- [ ] Build jobs запускаются в ветке `infra`

### Platform Services
- [ ] PostgreSQL развернут и доступен
- [ ] Redis развернут и доступен
- [ ] Kafka развернут и доступен
- [ ] Keycloak развернут и доступен

### DLT Infrastructure
- [ ] Fabric CA работает
- [ ] Fabric Orderer работает
- [ ] Fabric Peer работает
- [ ] Chaincode установлен и запущен

### Backend Services
- [ ] Identity Service работает
- [ ] Fabric Gateway работает
- [ ] Registry Service работает
- [ ] Issuance Service работает
- [ ] Settlement Service работает
- [ ] Compliance Service работает
- [ ] Bank Nominal работает
- [ ] API Gateway работает и доступен через Ingress

### Frontend Applications
- [ ] Portal Issuer доступен
- [ ] Portal Investor доступен
- [ ] Backoffice доступен

### CI/CD
- [ ] Build jobs работают
- [ ] Deploy jobs работают
- [ ] GitOps синхронизация работает

---

## 🚨 КРИТИЧЕСКИЕ ПРОБЛЕМЫ И РЕШЕНИЯ

### Проблема 1: Build jobs не запускаются в ветке `infra`
**Решение:** Обновить правила в `.gitlab-ci.yml`:
```yaml
rules:
  - if: '$CI_COMMIT_BRANCH || $CI_COMMIT_TAG'
  - if: '$CI_COMMIT_BRANCH == "infra"'
```

### Проблема 2: Недостаточно ресурсов в кластере
**Решение:** 
- Оптимизировать resource requests/limits
- Использовать один worker node для MVP
- Планировать масштабирование для production

### Проблема 3: Отсутствуют Helm Charts
**Решение:** Создать базовые Helm charts для всех компонентов

### Проблема 4: Отсутствуют Dockerfile
**Решение:** Создать Dockerfile для всех компонентов

---

## 📅 TIMELINE (примерный)

- **День 1:** Исправить CI/CD правила, проверить build jobs
- **День 2:** Выкатить Platform Services (PostgreSQL, Redis, Kafka, Keycloak)
- **День 3:** Выкатить DLT Infrastructure (Fabric CA, Orderer, Peer, Chaincode)
- **День 4:** Выкатить Backend Services (Identity, Registry, Issuance, Settlement, Compliance, API Gateway)
- **День 5:** Выкатить Frontend Applications (Portal Issuer, Portal Investor, Backoffice)
- **День 6:** Тестирование, исправление проблем
- **День 7+:** Постепенная оптимизация и масштабирование

---

## 📚 ДОПОЛНИТЕЛЬНЫЕ РЕСУРСЫ

- [Production Deployment Master Plan](./production-deployment-master-plan.md)
- [GitOps Setup](./gitops.md)
- [Helm Charts](./helm.md)
- [GitLab CI/CD](./gitlab-ci.md)
- [Architecture Overview](../architecture/10-HighLevel-Architecture.md)

---

## ⚠️ РИСКИ И ОГРАНИЧЕНИЯ

1. **Один worker node** - нет HA, нужен минимум 3 узла для production
2. **Ресурсы** - ограниченные CPU/memory на одном узле
3. **DNS** - может быть не настроен, использовать IP временно
4. **Secrets** - использовать Vault или Kubernetes Secrets
5. **Backup** - настроить автоматические бэкапы для PostgreSQL и Fabric

---

**Следующий шаг:** Начать с ЭТАПА 0 - подготовка и диагностика

