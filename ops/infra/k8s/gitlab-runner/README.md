# GitLab Runner для OIS-CFA

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Обзор

GitLab Runner развернут в Kubernetes кластере для выполнения CI/CD jobs из `.gitlab-ci.yml`.

**Executor:** Kubernetes  
**Concurrent builds:** 10  
**Tags:** `kubernetes`, `docker`, `kubectl`, `dotnet`, `node`

---

## Требования

### Kubernetes кластер

- Kubernetes 1.24+
- Доступ к Docker socket (для Docker-in-Docker)
- Минимум 2 CPU и 4GB RAM на нодах
- RBAC включен

### GitLab

- GitLab 18.5+ (git.telex.global)
- Runner registration token

---

## Установка

### 0. Настроить kubeconfig (обязательно!)

**Перед установкой runner необходимо настроить подключение к Kubernetes кластеру.**

```bash
# Проверить текущий kubeconfig
kubectl cluster-info

# Если ошибка "connection refused" или "no configuration", настроить:
export TWC_TOKEN='your-timeweb-token'
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# Проверить подключение
kubectl get nodes
```

**Документация:** `docs/ops/timeweb/kubeconfig.md`

### 1. Получить Runner Registration Token

1. В GitLab UI:
   - Settings → CI/CD → Runners
   - Expand "Runners" section
   - Скопировать **Registration token** (или создать новый runner)

2. Или использовать групповой/instance runner token (если настроен)

### 2. Обновить конфигурацию

```bash
# Заменить токен в configmap
RUNNER_TOKEN="your-runner-registration-token"

# Обновить configmap
sed -i "s/__REPLACE_WITH_RUNNER_TOKEN__/$RUNNER_TOKEN/g" \
  ops/infra/k8s/gitlab-runner/configmap.yaml
```

### 3. Развернуть Runner

```bash
# Применить манифесты
kubectl apply -f ops/infra/k8s/gitlab-runner/namespace.yaml
kubectl apply -f ops/infra/k8s/gitlab-runner/rbac.yaml
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml
kubectl apply -f ops/infra/k8s/gitlab-runner/deployment.yaml
kubectl apply -f ops/infra/k8s/gitlab-runner/service.yaml

# Проверить статус
kubectl get pods -n gitlab-runner
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50
```

### 4. Проверить регистрацию

В GitLab UI:
- Settings → CI/CD → Runners
- Должен появиться runner с именем "ois-cfa-runner"
- Статус: **Online** (зеленый индикатор)

---

## Конфигурация

### Параметры Runner

**Файл:** `ops/infra/k8s/gitlab-runner/configmap.yaml`

Основные параметры:
- `concurrent = 10` — максимум одновременных jobs
- `executor = "kubernetes"` — использование Kubernetes executor
- `privileged = true` — требуется для Docker-in-Docker
- `cpu_limit = "2"` — лимит CPU на job
- `memory_limit = "4Gi"` — лимит памяти на job

### Поддержка Docker-in-Docker

Runner настроен для работы с Docker-in-Docker через:
- `privileged = true` в конфигурации
- Монтирование `/var/run/docker.sock` (опционально, для ускорения)

### Кэширование (S3)

Для ускорения сборки настроено кэширование в S3:
- Bucket: `__REPLACE_WITH_S3_BUCKET__`
- Location: `us-east-1`
- Shared: `true` (общий кэш для всех runners)

**Настройка S3:**
1. Создать S3 bucket
2. Обновить `BucketName` в configmap
3. Добавить credentials через Secret (см. ниже)

---

## Secrets

### S3 Credentials (опционально)

```bash
# Создать secret для S3
kubectl create secret generic gitlab-runner-s3 \
  --from-literal=access-key="YOUR_ACCESS_KEY" \
  --from-literal=secret-key="YOUR_SECRET_KEY" \
  -n gitlab-runner

# Обновить configmap для использования secret
# Добавить в deployment:
# envFrom:
#   - secretRef:
#       name: gitlab-runner-s3
```

### Kubernetes Credentials

Для jobs, требующих доступ к Kubernetes (kubectl):
- ServiceAccount `gitlab-runner` имеет права в namespace `gitlab-runner`
- Для доступа к другим namespace — создать дополнительные Role/RoleBinding

---

## Использование в CI/CD

### Теги Runner

В `.gitlab-ci.yml` можно указать теги для выбора конкретного runner:

```yaml
build:api-gateway:
  tags:
    - kubernetes
    - docker
  script:
    - docker build ...
```

### Примеры Jobs

**Docker Build:**
```yaml
build:service:
  image: docker:24-dind
  services:
    - docker:24-dind
  tags:
    - kubernetes
    - docker
  script:
    - docker build -t $CI_REGISTRY_IMAGE/service:$CI_COMMIT_SHA .
```

**Kubernetes Deploy:**
```yaml
deploy:staging:
  image: bitnami/kubectl:1.30
  tags:
    - kubernetes
    - kubectl
  script:
    - kubectl apply -f manifests/
```

**Dotnet Test:**
```yaml
test:unit:
  image: mcr.microsoft.com/dotnet/sdk:9.0
  tags:
    - kubernetes
    - dotnet
  script:
    - dotnet test
```

---

## Мониторинг

### Метрики Prometheus

Runner экспортирует метрики на порту `9252`:
- `http://gitlab-runner.gitlab-runner.svc.cluster.local:9252/metrics`

**ServiceMonitor для Prometheus:**
```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: gitlab-runner
  namespace: gitlab-runner
spec:
  selector:
    matchLabels:
      app: gitlab-runner
  endpoints:
    - port: metrics
      path: /metrics
```

### Логи

```bash
# Логи всех runner pods
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=100 -f

# Логи конкретного pod
kubectl logs -n gitlab-runner <pod-name> -f
```

### Статус в GitLab UI

- Settings → CI/CD → Runners
- Показаны: статус, последний контакт, количество jobs

---

## Масштабирование

### Увеличить количество replicas

```bash
kubectl scale deployment gitlab-runner -n gitlab-runner --replicas=5
```

Или обновить `deployment.yaml`:
```yaml
spec:
  replicas: 5
```

### Horizontal Pod Autoscaler

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: gitlab-runner
  namespace: gitlab-runner
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: gitlab-runner
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
```

---

## Troubleshooting

### Runner не регистрируется

1. Проверить токен:
```bash
kubectl get configmap gitlab-runner-config -n gitlab-runner -o yaml | grep token
```

2. Проверить логи:
```bash
kubectl logs -n gitlab-runner -l app=gitlab-runner | grep -i error
```

3. Проверить доступность GitLab:
```bash
kubectl exec -n gitlab-runner <pod-name> -- curl -I https://git.telex.global
```

### Jobs не запускаются

1. Проверить теги в `.gitlab-ci.yml`:
```yaml
tags:
  - kubernetes  # Должен совпадать с RUNNER_TAG_LIST
```

2. Проверить ресурсы:
```bash
kubectl describe nodes
kubectl top nodes
```

3. Проверить права ServiceAccount:
```bash
kubectl auth can-i create pods --as=system:serviceaccount:gitlab-runner:gitlab-runner -n gitlab-runner
```

### Docker-in-Docker не работает

1. Проверить `privileged = true` в configmap
2. Проверить права на создание privileged pods:
```bash
kubectl auth can-i create pods --as=system:serviceaccount:gitlab-runner:gitlab-runner --subresource=* -n gitlab-runner
```

3. Проверить политики Pod Security:
```bash
kubectl get namespace gitlab-runner -o jsonpath='{.metadata.annotations.pod-security\.kubernetes\.io/enforce}'
```

---

## Обновление

### Обновить образ Runner

```bash
# Обновить image в deployment.yaml
# gitlab/gitlab-runner:latest → gitlab/gitlab-runner:v16.10.0

kubectl set image deployment/gitlab-runner \
  gitlab-runner=gitlab/gitlab-runner:v16.10.0 \
  -n gitlab-runner
```

### Обновить конфигурацию

```bash
# Изменить configmap
kubectl edit configmap gitlab-runner-config -n gitlab-runner

# Перезапустить pods для применения изменений
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
```

---

## Безопасность

### Рекомендации

1. ✅ Использовать отдельный namespace для runner
2. ✅ Ограничить права ServiceAccount минимально необходимыми
3. ✅ Использовать secrets для токенов и credentials
4. ⚠️ `privileged = true` требуется для Docker-in-Docker, но снижает безопасность
5. ✅ Регулярно обновлять образ runner
6. ✅ Мониторить использование ресурсов

### Pod Security Standards

Для соответствия Pod Security Standards можно использовать:
- `restricted` — максимальная безопасность (может не работать с Docker-in-Docker)
- `baseline` — компромисс между безопасностью и функциональностью
- `privileged` — минимальная безопасность (текущая настройка)

---

## Ссылки

- [GitLab Runner Documentation](https://docs.gitlab.com/runner/)
- [Kubernetes Executor](https://docs.gitlab.com/runner/executors/kubernetes.html)
- [Docker-in-Docker](https://docs.gitlab.com/ee/ci/docker/using_docker_build.html#docker-in-docker)
- [GitLab Runner Helm Chart](https://docs.gitlab.com/runner/install/kubernetes.html)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

