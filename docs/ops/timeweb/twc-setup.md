# Настройка Timeweb Cloud CLI (twc)

Руководство по установке и настройке `twc` CLI для работы с Kubernetes кластерами Timeweb Cloud.

## Что такое twc?

`twc` (Timeweb Cloud CLI) — официальный командный интерфейс для управления ресурсами Timeweb Cloud, включая:
- Kubernetes кластеры
- Базы данных
- Cloud Servers
- Load Balancers
- Firewall правила
- И другие ресурсы

## Установка

### Автоматическая установка

```bash
# Используя скрипт проекта
./tools/timeweb/install.sh

# Или через Makefile
make twc-install
```

### Ручная установка

```bash
# Установка через pip
pip install --user twc-cli

# Добавление в PATH
export PATH="${HOME}/.local/bin:${PATH}"

# Проверка установки
twc --version
```

## Получение API токена

1. Войдите в [личный кабинет Timeweb Cloud](https://timeweb.cloud)
2. Перейдите в раздел **API** → **Токены доступа**
3. Создайте новый токен с правами:
   - `k8s:read`
   - `k8s:write`
   - `vpc:read` (опционально)
   - `firewall:read` (опционально)
4. Скопируйте токен (он показывается только один раз!)

## Настройка токена

### Способ 1: Переменная окружения (рекомендуется)

```bash
export TWC_TOKEN='your-token-here'
```

Для постоянного использования добавьте в `~/.bashrc` или `~/.zshrc`:
```bash
echo 'export TWC_TOKEN="your-token-here"' >> ~/.bashrc
source ~/.bashrc
```

### Способ 2: Конфигурация twc

```bash
twc config set token 'your-token-here'
```

Токен будет сохранён в `~/.config/twc/config.yaml`.

### Способ 3: Через terraform.tfvars

```bash
# Создать файл конфигурации
cp ops/infra/timeweb/terraform.tfvars.example ops/infra/timeweb/terraform.tfvars

# Отредактировать и указать токен
# twc_token = "your-token-here"
```

⚠️ **Внимание:** `terraform.tfvars` добавлен в `.gitignore` и не должен коммититься в репозиторий.

## Проверка настройки

```bash
# Проверка через Makefile
make twc-verify

# Или вручную
twc k8s cluster list
```

Если команда выполняется успешно и показывает список кластеров (или пустой список), настройка выполнена правильно.

## Основные команды

### Управление кластерами

```bash
# Список кластеров
twc k8s cluster list

# Детали кластера
twc k8s cluster show <cluster-id>

# Создание кластера (через Terraform рекомендуется)
twc k8s cluster create --help

# Удаление кластера
twc k8s cluster remove <cluster-id>
```

### Управление node groups

```bash
# Список групп узлов
twc k8s group list --cluster-id <cluster-id>

# Детали группы узлов
twc k8s group show <group-id> --cluster-id <cluster-id>
```

### Получение kubeconfig

```bash
# Способ 1: Через скрипт проекта (рекомендуется)
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# Способ 2: Через Makefile
make twc-kubeconfig

# Способ 3: Напрямую через twc
CLUSTER_ID=$(twc k8s cluster list --format json | jq -r '.[] | select(.name == "ois-cfa-k8s") | .id')
twc k8s cluster kubeconfig "${CLUSTER_ID}" > kubeconfig.yaml
```

### Просмотр доступных опций

```bash
# Доступные версии Kubernetes
twc k8s list-k8s-versions

# Доступные пресеты (конфигурации узлов)
twc k8s list-presets

# Доступные сетевые драйверы
twc k8s list-network-drivers
```

## Автоматическая настройка кластера

Используйте скрипт для автоматической настройки доступа к кластеру:

```bash
./ops/scripts/setup-twc-cluster.sh [cluster-name]
```

Скрипт:
1. Проверяет установку `twc`
2. Настраивает токен (из переменной окружения или terraform.tfvars)
3. Находит кластер по имени
4. Экспортирует kubeconfig
5. Проверяет подключение к кластеру

## Использование kubeconfig

После экспорта kubeconfig:

```bash
# Установить как текущий контекст
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# Проверить подключение
kubectl cluster-info
kubectl get nodes
kubectl get namespaces
```

## Troubleshooting

### Ошибка: "twc: command not found"

```bash
# Установить twc
make twc-install

# Или добавить в PATH
export PATH="${HOME}/.local/bin:${PATH}"
```

### Ошибка: "TWC_TOKEN is not set"

```bash
# Установить токен
export TWC_TOKEN='your-token-here'

# Или настроить через twc
twc config set token 'your-token-here'
```

### Ошибка: "Invalid token" или "Authentication failed"

1. Проверьте правильность токена
2. Убедитесь, что токен имеет необходимые права (k8s:read, k8s:write)
3. Проверьте, не истёк ли токен

### Ошибка: "Cluster not found"

```bash
# Проверить список кластеров
twc k8s cluster list

# Если кластер не существует, создать через Terraform
cd ops/infra/timeweb
terraform init
terraform plan
terraform apply
```

### Ошибка при экспорте kubeconfig

```bash
# Попробовать альтернативные команды
twc k8s cluster kubeconfig <cluster-id>
twc k8s kubeconfig <cluster-id>
twc k8s cluster get-kubeconfig <cluster-id>
```

## Безопасность

⚠️ **Важные рекомендации:**

1. **Не коммитьте токены в Git**
   - `terraform.tfvars` уже в `.gitignore`
   - Используйте переменные окружения или GitLab CI/CD Variables

2. **Ограничьте права токена**
   - Создавайте токены только с необходимыми правами
   - Для CI/CD используйте отдельные токены с минимальными правами

3. **Регулярно ротируйте токены**
   - Обновляйте токены каждые 90 дней
   - Немедленно отзывайте скомпрометированные токены

4. **Защищайте kubeconfig**
   - Устанавливайте права `600` на kubeconfig файлы
   - Не передавайте kubeconfig по незащищённым каналам

## Дополнительные ресурсы

- [Timeweb Cloud Kubernetes Documentation](https://timeweb.cloud/docs/k8s)
- [Timeweb Cloud CLI Documentation](https://timeweb.cloud/docs/cli)
- [Terraform Provider для Timeweb Cloud](../../ops/infra/timeweb/README.md)

