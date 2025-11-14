# Helm Charts для OIS-CFA

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Обзор

Helm charts для всех сервисов и приложений проекта OIS-CFA.

### Структура

```
ops/infra/helm/
├── api-gateway/          # API Gateway
├── identity/             # Identity Service
├── issuance/             # Issuance Service
├── registry/             # Registry Service
├── settlement/           # Settlement Service
├── compliance/           # Compliance Service
├── bank-nominal/         # Bank Nominal Integration
├── portal-issuer/        # Portal Issuer (Next.js)
├── portal-investor/      # Portal Investor (Next.js)
├── broker-portal/        # Broker Portal (Next.js)
└── backoffice/           # Backoffice (Next.js)
```

### Компоненты Chart

Каждый chart включает:
- `Chart.yaml` — метаданные chart
- `values.yaml` — значения по умолчанию
- `values-dev.yaml` — значения для dev
- `values-staging.yaml` — значения для staging
- `values-prod.yaml` — значения для prod
- `templates/` — Kubernetes манифесты:
  - `deployment.yaml`
  - `service.yaml`
  - `ingress.yaml`
  - `servicemonitor.yaml`
  - `serviceaccount.yaml`
  - `hpa.yaml` (если autoscaling включен)
  - `_helpers.tpl`

---

## Использование

### Установка через Helm

```bash
# Dev
helm install api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  --create-namespace \
  -f ops/infra/helm/api-gateway/values-dev.yaml

# Staging
helm install api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  --create-namespace \
  -f ops/infra/helm/api-gateway/values-staging.yaml

# Production
helm install api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  --create-namespace \
  -f ops/infra/helm/api-gateway/values-prod.yaml
```

### Обновление

```bash
helm upgrade api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  -f ops/infra/helm/api-gateway/values-prod.yaml
```

### Удаление

```bash
helm uninstall api-gateway --namespace ois-cfa
```

---

## Через GitOps

### ArgoCD

Charts автоматически развёртываются через ArgoCD Applications (см. `ops/gitops/argocd/apps/`).

### GitLab Agent

Манифесты генерируются из charts и синхронизируются через GitLab Agent.

---

## Переменные окружений

### Общие

| Переменная | Описание | По умолчанию |
|-----------|----------|--------------|
| `replicaCount` | Количество реплик | 2 |
| `image.repository` | Docker image repository | - |
| `image.tag` | Docker image tag | latest |
| `service.type` | Service type | ClusterIP |
| `ingress.enabled` | Включить Ingress | true |

### Секреты

Секреты управляются через:
- **Sealed Secrets** (dev/staging)
- **Vault** (production)

Настройка в `values.yaml`:
```yaml
secrets:
  enabled: true
  type: "sealed-secrets" # or "vault"
  name: "api-gateway-secrets"
```

---

## ServiceMonitor

ServiceMonitor для Prometheus включается через:
```yaml
serviceMonitor:
  enabled: true
  interval: 30s
  path: /metrics
```

---

## Ingress и CertManager

Ingress настроен с автоматической генерацией TLS сертификатов через CertManager:

```yaml
ingress:
  enabled: true
  className: "nginx"
  annotations:
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
  tls:
    - secretName: api-gateway-tls
      hosts:
        - api.ois-cfa.example.com
```

**Требования:**
- CertManager установлен в кластере
- ClusterIssuer `letsencrypt-prod` настроен

---

## Health Checks

Все сервисы имеют health checks:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
```

---

## Autoscaling

HPA (Horizontal Pod Autoscaler) настраивается через:

```yaml
autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 80
```

---

## Создание нового Chart

### Использование скрипта

```bash
# Создать chart для сервиса
./scripts/create-helm-chart.sh identity service

# Создать chart для приложения
./scripts/create-helm-chart.sh portal-issuer app
```

### Вручную

1. Скопировать `api-gateway` chart
2. Заменить `api-gateway` на имя нового сервиса
3. Обновить `values.yaml` с специфичными настройками
4. Обновить `values-*.yaml` для окружений

---

## Troubleshooting

### Pod не запускается

```bash
# Проверить логи
kubectl logs -n ois-cfa deployment/api-gateway

# Проверить события
kubectl describe pod -n ois-cfa -l app.kubernetes.io/name=api-gateway
```

### Ingress не работает

```bash
# Проверить Ingress
kubectl get ingress -n ois-cfa

# Проверить CertManager
kubectl get certificate -n ois-cfa
kubectl describe certificate api-gateway-tls -n ois-cfa
```

### ServiceMonitor не работает

```bash
# Проверить ServiceMonitor
kubectl get servicemonitor -n ois-cfa

# Проверить Prometheus targets
# В Prometheus UI: Status → Targets
```

---

## Ссылки

- [Helm Documentation](https://helm.sh/docs/)
- [CertManager](https://cert-manager.io/)
- [Prometheus Operator](https://prometheus-operator.dev/)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

