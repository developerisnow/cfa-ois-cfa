# Timeweb Cloud CLI Tools

Утилиты для работы с Timeweb Cloud через командную строку.

## Быстрый старт

```bash
# 1. Установить twc CLI
./tools/timeweb/install.sh

# 2. Настроить токен
export TWC_TOKEN='your-token-here'

# 3. Проверить конфигурацию
make twc:verify

# 4. Экспортировать kubeconfig
make twc:kubeconfig
```

## Скрипты

### install.sh

Устанавливает Timeweb Cloud CLI (`twc-cli`) через pip.

**Использование:**
```bash
./tools/timeweb/install.sh
```

**Требования:**
- Python 3.8+
- pip

### kubeconfig-export.sh

Экспортирует kubeconfig для указанного кластера Kubernetes.

**Использование:**
```bash
# Базовое использование (кластер ois-cfa-k8s)
./tools/timeweb/kubeconfig-export.sh

# С указанием имени кластера
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# С указанием имени кластера и выходного файла
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s /path/to/kubeconfig.yaml
```

**Требования:**
- Установленный `twc` CLI (см. `install.sh`)
- Переменная окружения `TWC_TOKEN`
- `jq` (для парсинга JSON, опционально)

## Makefile цели

```bash
# Установка twc CLI
make twc:install

# Список кластеров
make twc:cluster-list

# Экспорт kubeconfig
make twc:kubeconfig

# Проверка конфигурации
make twc:verify
```

## Документация

- [Полная документация по kubeconfig](../../docs/ops/timeweb/kubeconfig.md)
- [Terraform и Timeweb Cloud](../../docs/ops/timeweb/terraform.md)

## Troubleshooting

### twc: command not found

```bash
# Установить twc CLI
make twc:install

# Или добавить в PATH
export PATH="${HOME}/.local/bin:${PATH}"
```

### TWC_TOKEN not set

```bash
export TWC_TOKEN='your-token-here'
```

### Cluster not found

```bash
# Проверить список кластеров
make twc:cluster-list
```

