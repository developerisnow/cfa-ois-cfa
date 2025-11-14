# Debug Toolbox для OIS-CFA

Набор инструментов для отладки и диагностики Kubernetes кластера.

## Компоненты

- **debug-pod** — Pod с утилитами для отладки
- **Скрипты** — автоматизированный сбор логов и диагностика
- **GitLab интеграция** — загрузка артефактов в CI/CD

## Установка

### 1. Создать namespace и RBAC

```bash
kubectl apply -f ops/debug/namespace.yaml
kubectl apply -f ops/debug/serviceaccount.yaml
```

### 2. Создать секрет для GitLab (опционально)

```bash
# Скопировать пример
cp ops/debug/secret-gitlab-token.yaml.example ops/debug/secret-gitlab-token.yaml

# Заполнить токен
# Редактировать ops/debug/secret-gitlab-token.yaml

# Применить
kubectl apply -f ops/debug/secret-gitlab-token.yaml
```

### 3. Создать ConfigMap со скриптами

```bash
kubectl apply -f ops/debug/configmap-scripts.yaml
```

### 4. Запустить debug pod

```bash
kubectl apply -f ops/debug/debug-pod.yaml
```

Или через Makefile:

```bash
make debug-deploy
```

## Использование

### Доступ к debug pod

```bash
# Через Makefile
make debug-exec

# Или напрямую
kubectl exec -it -n tools debug-toolbox -- /bin/bash
```

### Запуск скриптов

```bash
# Сбор логов
kubectl exec -n tools debug-toolbox -- /scripts/logs-collect.sh fabric-network ois-cfa

# Дамп событий
kubectl exec -n tools debug-toolbox -- /scripts/events-dump.sh

# Статус ArgoCD
kubectl exec -n tools debug-toolbox -- /scripts/argo-status.sh

# Статус GitLab Agent
kubectl exec -n tools debug-toolbox -- /scripts/agent-status.sh
```

## Скрипты

### logs-collect.sh

Собирает логи всех pods в указанных namespace'ах.

**Использование:**
```bash
/scripts/logs-collect.sh [namespace1] [namespace2] ...
```

**По умолчанию:** `fabric-network ois-cfa argocd keycloak monitoring`

**Артефакты:**
- Логи каждого pod
- Описание pods
- YAML манифесты
- События namespace
- Сводка ресурсов

### events-dump.sh

Выгружает все события Kubernetes, отсортированные по времени.

**Использование:**
```bash
/scripts/events-dump.sh [output-file]
```

**Артефакты:**
- Все события кластера
- Сводка по типам событий
- Последние 100 предупреждений

### argo-status.sh

Проверяет статус всех ArgoCD Applications.

**Использование:**
```bash
/scripts/argo-status.sh
```

**Артефакты:**
- Статус всех Applications
- Детальная информация по каждому
- Сводка (Synced/Healthy)

### agent-status.sh

Проверяет статус GitLab Kubernetes Agent.

**Использование:**
```bash
/scripts/agent-status.sh
```

**Артефакты:**
- Статус agent pods
- Логи агента
- Конфигурация

## GitLab CI интеграция

Скрипты автоматически определяют GitLab CI окружение и сохраняют артефакты:

```yaml
debug:collect-logs:
  stage: test
  script:
    - kubectl exec -n tools debug-toolbox -- /scripts/logs-collect.sh
    - kubectl cp tools/debug-toolbox:/tmp/artifacts ./artifacts
  artifacts:
    when: always
    paths:
      - artifacts/
    expire_in: 1 week
```

## Утилиты в pod

- **kubectl** — управление Kubernetes
- **jq** — обработка JSON
- **curl** — HTTP запросы
- **glab** — GitLab CLI
- **yq** — обработка YAML
- **helm** — управление Helm charts
- **k9s** — опционально, TUI для Kubernetes

## Troubleshooting

### Pod не запускается

```bash
# Проверить статус
kubectl get pod -n tools debug-toolbox

# Проверить события
kubectl describe pod -n tools debug-toolbox

# Проверить логи
kubectl logs -n tools debug-toolbox
```

### Нет доступа к ресурсам

```bash
# Проверить RBAC
kubectl get clusterrolebinding debug-toolbox
kubectl describe clusterrole debug-toolbox
```

### GitLab CLI не работает

```bash
# Проверить секрет
kubectl get secret -n tools gitlab-readonly-token

# Проверить переменные окружения
kubectl exec -n tools debug-toolbox -- env | grep GITLAB
```

## Безопасность

- ServiceAccount с минимальными правами (read-only)
- GitLab token только read-only
- Secrets не коммитятся в Git
- Pod удаляется после использования

