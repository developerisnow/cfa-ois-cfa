# Получение и использование kubeconfig для Timeweb Cloud Kubernetes

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Содержание

1. [Быстрый старт](#быстрый-старт)
2. [Установка twc CLI](#установка-twc-cli)
3. [Настройка аутентификации](#настройка-аутентификации)
4. [Получение kubeconfig](#получение-kubeconfig)
5. [Проверка подключения](#проверка-подключения)
6. [Хранение kubeconfig](#хранение-kubeconfig)
7. [Troubleshooting](#troubleshooting)

---

## Быстрый старт

```bash
# 1. Установить twc CLI
./tools/timeweb/install.sh

# 2. Настроить токен
export TWC_TOKEN='your-token-here'

# 3. Экспортировать kubeconfig
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# 4. Использовать kubeconfig
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
kubectl get nodes
```

---

## Установка twc CLI

### Автоматическая установка

```bash
./tools/timeweb/install.sh
```

Скрипт:
- Проверяет наличие Python 3 и pip
- Устанавливает `twc-cli` через pip
- Проверяет установку

### Ручная установка

```bash
# Установка через pip
pip install --user twc-cli

# Добавить в PATH (если установлен в ~/.local/bin)
export PATH="${HOME}/.local/bin:${PATH}"

# Проверка
twc --version
```

### Требования

- Python 3.8+
- pip
- Доступ к интернету для установки пакетов

---

## Настройка аутентификации

### Вариант 1: Переменная окружения (рекомендуется)

```bash
export TWC_TOKEN='your-timeweb-cloud-api-token'
```

Токен можно получить в [личном кабинете Timeweb Cloud](https://timeweb.cloud):
- Раздел: API → Токены доступа
- Права: `k8s:read`, `k8s:write`

### Вариант 2: Конфигурация twc

```bash
twc config set token 'your-timeweb-cloud-api-token'
```

### Проверка конфигурации

```bash
# Список кластеров
twc k8s cluster list

# Если видите список кластеров - конфигурация работает
```

---

## Получение kubeconfig

### Способ 1: Через скрипт (рекомендуется)

```bash
# Базовое использование
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# С указанием выходного файла
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s /path/to/kubeconfig.yaml
```

Скрипт автоматически:
- Проверяет наличие twc CLI
- Находит кластер по имени
- Экспортирует kubeconfig
- Устанавливает правильные права доступа (600)
- Проверяет подключение (если установлен kubectl)

### Способ 2: Через twc CLI напрямую

```bash
# 1. Получить список кластеров и найти ID
twc k8s cluster list

# 2. Получить ID конкретного кластера
CLUSTER_ID=$(twc k8s cluster list --format json | \
  jq -r '.[] | select(.name == "ois-cfa-k8s") | .id')

# 3. Экспортировать kubeconfig
twc k8s cluster get-kubeconfig "${CLUSTER_ID}" > kubeconfig.yaml

# 4. Установить права доступа
chmod 600 kubeconfig.yaml
```

### Способ 3: Через Terraform

Если кластер создан через Terraform:

```bash
cd ops/infra/timeweb

# Экспорт kubeconfig
terraform output -raw kubeconfig > kubeconfig.yaml
chmod 600 kubeconfig.yaml

# Или через Makefile
make tf:kubeconfig
```

---

## Проверка подключения

### Базовая проверка

```bash
# Установить kubeconfig
export KUBECONFIG="$(pwd)/kubeconfig.yaml"

# Проверить подключение
kubectl cluster-info

# Список узлов
kubectl get nodes

# Список namespace
kubectl get namespaces
```

### Расширенная проверка

```bash
# Информация о кластере
kubectl cluster-info dump

# Проверка версии Kubernetes
kubectl version

# Проверка компонентов
kubectl get componentstatuses

# Проверка подов в kube-system
kubectl get pods -n kube-system
```

### Если кластер ещё создаётся

Если кластер только что создан, может потребоваться время для инициализации:

```bash
# Проверка статуса через twc
twc k8s cluster get <cluster-id>

# Ожидание готовности (пример)
while ! kubectl get nodes 2>/dev/null; do
  echo "Waiting for cluster to be ready..."
  sleep 10
done
```

---

## Хранение kubeconfig

### ⚠️ Важно: Безопасность

**НЕ коммитьте kubeconfig в Git!**

Файл `kubeconfig.yaml` уже добавлен в `.gitignore`:
- `ops/infra/timeweb/.gitignore` содержит правила для `kubeconfig*`

### Варианты хранения

#### 1. GitLab Secure Files (для ручных проверок)

**Использование:**
- Ручные проверки и отладка
- Временное хранение для CI/CD

**Настройка:**
1. Перейдите в GitLab: Settings → CI/CD → Secure Files
2. Загрузите `kubeconfig.yaml`
3. Используйте в CI/CD через переменную `KUBECONFIG_FILE`

**Пример в `.gitlab-ci.yml`:**
```yaml
deploy:
  script:
    - kubectl --kubeconfig=$KUBECONFIG_FILE get nodes
```

#### 2. GitLab Kubernetes Agent (рекомендуется для production)

**Использование:**
- Production окружения
- Автоматическое управление доступом
- Интеграция с GitLab CI/CD

**Настройка:** См. Task 19 (GitLab Agent setup)

#### 3. Локальное хранение (только для разработки)

```bash
# Сохранить в ~/.kube/config
mkdir -p ~/.kube

# Добавить как отдельный контекст
KUBECONFIG="${HOME}/.kube/config-ois-cfa:${HOME}/.kube/config" \
  kubectl config view --flatten > "${HOME}/.kube/config-merged"

# Использовать конкретный контекст
kubectl config use-context ois-cfa-k8s
```

#### 4. Vault / Secret Manager (для production)

```bash
# Сохранить в Vault
vault kv put secret/ois-cfa/kubeconfig \
  content="$(cat kubeconfig.yaml)"

# Получить из Vault
vault kv get -field=content secret/ois-cfa/kubeconfig > kubeconfig.yaml
```

---

## Troubleshooting

### Ошибка: "twc: command not found"

**Решение:**
```bash
# Установить twc CLI
./tools/timeweb/install.sh

# Или добавить в PATH
export PATH="${HOME}/.local/bin:${PATH}"
```

### Ошибка: "TWC_TOKEN environment variable is not set"

**Решение:**
```bash
# Установить переменную окружения
export TWC_TOKEN='your-token-here'

# Или настроить через twc config
twc config set token 'your-token-here'
```

### Ошибка: "Cluster 'ois-cfa-k8s' not found"

**Решение:**
```bash
# Проверить список кластеров
twc k8s cluster list

# Использовать правильное имя кластера
./tools/timeweb/kubeconfig-export.sh <correct-cluster-name>
```

### Ошибка: "Unable to connect to the server"

**Возможные причины:**
1. Кластер ещё создаётся (подождите 10-20 минут)
2. Firewall блокирует доступ (проверьте правила)
3. Неправильный kubeconfig

**Решение:**
```bash
# Проверить статус кластера
twc k8s cluster get <cluster-id>

# Проверить firewall правила
twc firewall list

# Переэкспортировать kubeconfig
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s
```

### Ошибка: "The connection to the server ... was refused"

**Решение:**
```bash
# Проверить, что кластер готов
twc k8s cluster get <cluster-id> | grep status

# Проверить доступность API server
curl -k https://<api-server-url>

# Проверить firewall (должен быть открыт порт 6443)
```

### kubeconfig истёк или недействителен

**Решение:**
```bash
# Переэкспортировать kubeconfig
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# Или через twc
twc k8s cluster get-kubeconfig <cluster-id> > kubeconfig.yaml
```

---

## Примеры использования

### Полный workflow

```bash
# 1. Установка
./tools/timeweb/install.sh

# 2. Настройка
export TWC_TOKEN='your-token'

# 3. Проверка
twc k8s cluster list

# 4. Экспорт kubeconfig
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# 5. Использование
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
kubectl get nodes
kubectl get pods --all-namespaces
```

### Использование в CI/CD

```yaml
# .gitlab-ci.yml
deploy:
  before_script:
    - pip install --user twc-cli
    - export PATH="${HOME}/.local/bin:${PATH}"
    - export TWC_TOKEN="${TWC_TOKEN}"
    - ./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s
    - export KUBECONFIG="$(pwd)/kubeconfig.yaml"
  script:
    - kubectl get nodes
    - kubectl apply -f k8s/
```

---

## Ссылки

- [Timeweb Cloud CLI Documentation](https://timeweb.cloud/docs/cli)
- [Kubernetes kubeconfig](https://kubernetes.io/docs/concepts/configuration/organize-cluster-access-kubeconfig/)
- [GitLab Secure Files](https://docs.gitlab.com/ee/ci/secure_files/)
- [GitLab Kubernetes Agent](https://docs.gitlab.com/ee/user/clusters/agent/)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

