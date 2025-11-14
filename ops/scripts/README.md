# Operations Scripts

Скрипты для операций и диагностики Kubernetes кластера.

## k8s-healthcheck.sh

Комплексная проверка здоровья Kubernetes кластера с генерацией HTML-отчёта.

### Использование

```bash
# Локально
./ops/scripts/k8s-healthcheck.sh

# Через Makefile
make k8s-healthcheck

# Из debug toolbox pod
make k8s-healthcheck-debug
```

### Проверки

1. **Nodes** — количество и статус узлов, версии, ресурсы
2. **Ingress Controller** — статус pods и LoadBalancer
3. **Cert-Manager** — статус pods, сертификатов и issuers
4. **DNS** — CoreDNS pods и разрешение DNS
5. **Critical Namespaces** — наличие системных namespace'ов
6. **System Pods** — статус системных pods
7. **API Server** — проверка доступности API сервера

### Отчёты

Скрипт генерирует:
- **HTML-отчёт** (`healthcheck-report-*.html`) — визуальный отчёт с результатами проверок
- **JSON-отчёт** (`healthcheck-*.json`) — структурированные данные для автоматизации

### Action Items

При обнаружении проблем скрипт формирует список действий с командами для исправления.

### GitLab CI

Интегрирован в GitLab CI как manual job `k8s:healthcheck`. Отчёты сохраняются в artifacts и доступны в UI.

## setup-twc-cluster.sh

Автоматическая настройка доступа к Kubernetes кластеру в Timeweb Cloud.

### Использование

```bash
# Настроить доступ к кластеру ois-cfa-k8s
./ops/scripts/setup-twc-cluster.sh

# Или указать имя кластера
./ops/scripts/setup-twc-cluster.sh my-cluster-name
```

### Что делает скрипт

1. Проверяет установку `twc` CLI
2. Находит токен (из переменной окружения или terraform.tfvars)
3. Проверяет аутентификацию с Timeweb Cloud
4. Находит кластер по имени
5. Показывает детали кластера и node groups
6. Экспортирует kubeconfig
7. Проверяет подключение к кластеру

### Требования

- Установленный `twc` CLI (см. `tools/timeweb/install.sh`)
- Токен Timeweb Cloud API (см. `docs/ops/timeweb/twc-setup.md`)
- `kubectl` (опционально, для проверки подключения)
- `jq` (опционально, для парсинга JSON)

### Пример вывода

```
=== Timeweb Cloud Cluster Setup ===
Cluster name: ois-cfa-k8s

=== Verifying twc configuration ===
✓ Authentication successful

=== Available Kubernetes Clusters ===
ID          Name           Status    Version
12345       ois-cfa-k8s   active    1.28

=== Finding cluster: ois-cfa-k8s ===
✓ Found cluster ID: 12345

=== Cluster Details ===
...

=== Exporting Kubeconfig ===
✓ Kubeconfig exported to: ops/infra/timeweb/kubeconfig.yaml

=== Verifying Kubeconfig ===
✓ Successfully connected to cluster
```

### Troubleshooting

**Ошибка: "TWC_TOKEN is not set"**
- Установите токен: `export TWC_TOKEN='your-token'`
- Или создайте `terraform.tfvars` с токеном

**Ошибка: "Cluster not found"**
- Проверьте список кластеров: `twc k8s list`
- Создайте кластер через Terraform: `cd ops/infra/timeweb && terraform apply`

**Ошибка: "Failed to authenticate"**
- Проверьте правильность токена
- Убедитесь, что токен имеет права `k8s:read` и `k8s:write`

## Другие скрипты

- `backup.sh` — резервное копирование
- `create-helm-chart.sh` — создание Helm charts
- `test-restore.sh` — тестирование восстановления
