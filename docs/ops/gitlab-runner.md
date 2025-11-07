# GitLab Runner для OIS-CFA

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Быстрый старт

### 0. Настроить kubeconfig (обязательно!)

Перед установкой runner необходимо настроить подключение к Kubernetes кластеру:

**Вариант 1: Автоматическая настройка (рекомендуется)**

```bash
# 1. Установить токен Timeweb Cloud
export TWC_TOKEN='your-timeweb-token'

# 2. Запустить скрипт настройки
make setup-kubeconfig

# Скрипт автоматически:
# - Проверит наличие kubectl и twc CLI
# - Экспортирует kubeconfig из Timeweb Cloud
# - Настроит переменную KUBECONFIG
# - Проверит подключение к кластеру
```

**Вариант 2: Ручная настройка**

```bash
# 1. Установить токен
export TWC_TOKEN='your-timeweb-token'

# 2. Экспортировать kubeconfig
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# 3. Установить переменную KUBECONFIG
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# 4. Проверить подключение
kubectl get nodes
```

**Вариант 3: Если уже есть kubeconfig файл**

```bash
# Указать путь к существующему файлу
export KUBECONFIG="/absolute/path/to/your/kubeconfig.yaml"

# Проверить подключение
kubectl get nodes
```

**Проверка настройки:**

```bash
make check-kubeconfig
```

**Если kubeconfig не настроен:**
- См. документацию: `docs/ops/timeweb/kubeconfig.md`
- Или используйте существующий kubeconfig файл

### 1. Получить Runner Registration Token

**Вариант A: Через GitLab UI**
1. Открыть проект: https://git.telex.global/npk/ois-cfa
2. Settings → CI/CD → Runners
3. Expand секцию "Runners"
4. Скопировать **Registration token**

**Вариант B: Показать инструкции**

```bash
make gitlab-runner-get-token
```

### 2. Установить Runner

**Вариант A: Через Makefile**

```bash
export RUNNER_TOKEN="your-runner-token-here"
make gitlab-runner-install
```

**Вариант B: Через скрипт**

```bash
export RUNNER_TOKEN="your-runner-token-here"
./ops/scripts/gitlab-runner-install.sh
```

**Вариант C: Автоматически получить токен и установить**

```bash
export GITLAB_TOKEN="glpat-gMALQnHdaBtQ8CtlZ1oPVW86MQp1OjYH.01.0w0tmvenu"
./ops/scripts/gitlab-runner-install.sh
```

### 3. Проверить статус

```bash
make gitlab-runner-status
```

Или в GitLab UI:
- Settings → CI/CD → Runners
- Должен появиться runner "ois-cfa-runner" со статусом **Online**

---

## Управление

### Просмотр логов

```bash
make gitlab-runner-logs
```

### Перезапуск

```bash
make gitlab-runner-restart
```

### Масштабирование

```bash
make gitlab-runner-scale REPLICAS=5
```

### Удаление

```bash
make gitlab-runner-uninstall
```

---

## Конфигурация

### Параметры Runner

**Файл:** `ops/infra/k8s/gitlab-runner/configmap.yaml`

- **Concurrent builds:** 10
- **Executor:** Kubernetes
- **Tags:** `kubernetes`, `docker`, `kubectl`, `dotnet`, `node`
- **Resources per job:**
  - CPU: 500m request, 2 limit
  - Memory: 1Gi request, 4Gi limit

### Поддержка Docker-in-Docker

Runner настроен для работы с Docker-in-Docker:
- `privileged = true` в конфигурации
- Монтирование Docker socket (опционально)

### Кэширование

Настроено кэширование в S3 (требует настройки):
- Bucket: `__REPLACE_WITH_S3_BUCKET__`
- Location: `us-east-1`

---

## Использование в CI/CD

### Теги Runner

В `.gitlab-ci.yml` можно указать теги:

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

1. Проверить теги в `.gitlab-ci.yml`
2. Проверить ресурсы кластера:
```bash
kubectl describe nodes
kubectl top nodes
```

3. Проверить права ServiceAccount:
```bash
kubectl auth can-i create pods --as=system:serviceaccount:gitlab-runner:gitlab-runner -n gitlab-runner
```

---

## Документация

Полная документация: `ops/infra/k8s/gitlab-runner/README.md`

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

