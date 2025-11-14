# Terraform для Timeweb Cloud Kubernetes

Документация по настройке и использованию Terraform для развёртывания Kubernetes кластера в Timeweb Cloud для проекта OIS-CFA.

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Содержание

1. [Обзор](#обзор)
2. [Предварительные требования](#предварительные-требования)
3. [Настройка](#настройка)
4. [Использование](#использование)
5. [GitLab State Backend](#gitlab-state-backend)
6. [Конфигурация](#конфигурация)
7. [FAQ](#faq)
8. [Ограничения](#ограничения)
9. [Troubleshooting](#troubleshooting)

---

## Обзор

Terraform конфигурация для автоматизации создания и управления инфраструктурой Kubernetes в Timeweb Cloud:

- **Kubernetes Cluster** (`twc_k8s_cluster`)
- **Node Group** (`twc_k8s_node_group`)
- **VPC** (опционально, для изоляции сети)
- **Firewall** (правила безопасности)

### Расположение файлов

```
ops/infra/timeweb/
├── main.tf              # Основные ресурсы
├── providers.tf        # Провайдеры Terraform
├── variables.tf        # Переменные
├── outputs.tf          # Выходные значения
├── backend.tf          # Backend конфигурация
├── terraform.tfvars.example  # Пример конфигурации
├── .gitignore          # Игнорируемые файлы
└── README.md           # Краткая инструкция
```

---

## Предварительные требования

1. **Terraform** >= 1.5.0
   ```bash
   terraform version
   ```

2. **Timeweb Cloud API Token**
   - Получить в [личном кабинете Timeweb Cloud](https://timeweb.cloud)
   - Раздел: API → Токены доступа
   - Права: `k8s:read`, `k8s:write`, `vpc:read`, `vpc:write`, `firewall:read`, `firewall:write`

3. **GitLab** (для managed state, опционально)
   - Проект с включённым Terraform State
   - Personal Access Token или Project Access Token с правами `api`

---

## Настройка

### 1. Клонирование и подготовка

```bash
cd ops/infra/timeweb
cp terraform.tfvars.example terraform.tfvars
```

### 2. Редактирование `terraform.tfvars`

Откройте `terraform.tfvars` и заполните значения:

```hcl
twc_token = "your-timeweb-cloud-api-token"

cluster_name     = "ois-cfa-k8s"
cluster_location = "ru-1"  # ru-1, ru-2, ru-3
cluster_version  = "1.28"

node_group_name      = "ois-cfa-nodes"
node_group_preset_id = 1
node_count           = 3
node_disk_size       = 50

vpc_enabled     = true
firewall_enabled = true
```

### 3. Выбор региона (location)

Доступные регионы Timeweb Cloud:

- **ru-1** — Москва (по умолчанию)
- **ru-2** — Санкт-Петербург
- **ru-3** — Казань

Выбор зависит от:
- Близости к пользователям
- Требований к задержке (latency)
- Доступности зон

### 4. Выбор пресета (preset_id)

Пресеты определяют конфигурацию узлов (CPU, RAM). Проверьте актуальные пресеты в [документации Timeweb Cloud](https://timeweb.cloud/docs/k8s) или через API.

Примеры (могут измениться):
- `1` — минимальный (2 vCPU, 4 GB RAM)
- `2` — стандартный (4 vCPU, 8 GB RAM)
- `3` — производительный (8 vCPU, 16 GB RAM)

**Рекомендации для OIS-CFA:**
- Dev/Test: preset_id = 1-2, node_count = 2-3
- Production: preset_id = 2-3, node_count = 3-5 (минимум для HA)

---

## Использование

### Базовые команды

```bash
# Инициализация
terraform init

# Проверка конфигурации
terraform validate

# План изменений
terraform plan

# Применение
terraform apply

# Уничтожение инфраструктуры
terraform destroy
```

### Использование Makefile

```bash
# Инициализация
make tf:init

# План
make tf:plan

# Применение
make tf:apply

# Уничтожение
make tf:destroy

# Валидация
make tf:validate
```

### Получение kubeconfig

```bash
# Сохранить kubeconfig в файл
terraform output -raw kubeconfig > kubeconfig.yaml

# Использовать kubeconfig
export KUBECONFIG=$(pwd)/kubeconfig.yaml
kubectl get nodes
```

### Просмотр outputs

```bash
terraform output
terraform output api_server_url
terraform output cluster_id
```

---

## GitLab State Backend

Для хранения состояния Terraform в GitLab (managed state) необходимо настроить backend.

### 1. Создание GitLab переменных

В настройках проекта GitLab (Settings → CI/CD → Variables) добавьте:

| Переменная | Значение | Masked | Protected |
|-----------|----------|--------|-----------|
| `TWC_TOKEN` | Timeweb Cloud API token | ✅ | ✅ |
| `TF_HTTP_ADDRESS` | `https://gitlab.com/api/v4/projects/{PROJECT_ID}/terraform/state/{STATE_NAME}` | ❌ | ✅ |
| `TF_HTTP_LOCK_ADDRESS` | `https://gitlab.com/api/v4/projects/{PROJECT_ID}/terraform/state/{STATE_NAME}/lock` | ❌ | ✅ |
| `TF_HTTP_UNLOCK_ADDRESS` | `https://gitlab.com/api/v4/projects/{PROJECT_ID}/terraform/state/{STATE_NAME}/unlock` | ❌ | ✅ |
| `TF_HTTP_USERNAME` | GitLab username или `gitlab-ci-token` | ❌ | ✅ |
| `TF_HTTP_PASSWORD` | Personal/Project Access Token с правами `api` | ✅ | ✅ |

**Где найти PROJECT_ID:**
- В настройках проекта GitLab: Settings → General → Project ID

**STATE_NAME:**
- Уникальное имя для state (например, `ois-cfa-timeweb-prod`)

### 2. Настройка backend

#### Вариант A: Через переменные окружения

```bash
export TF_HTTP_ADDRESS="https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod"
export TF_HTTP_LOCK_ADDRESS="https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/lock"
export TF_HTTP_UNLOCK_ADDRESS="https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/unlock"
export TF_HTTP_USERNAME="your-username"
export TF_HTTP_PASSWORD="your-token"

terraform init
```

#### Вариант B: Через -backend-config

```bash
terraform init \
  -backend-config="address=https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod" \
  -backend-config="lock_address=https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/lock" \
  -backend-config="unlock_address=https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/unlock" \
  -backend-config="username=your-username" \
  -backend-config="password=your-token"
```

#### Вариант C: Файл backend.hcl (gitignored)

Создайте `backend.hcl` (добавлен в .gitignore):

```hcl
address        = "https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod"
lock_address   = "https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/lock"
unlock_address = "https://gitlab.com/api/v4/projects/123456/terraform/state/ois-cfa-timeweb-prod/unlock"
username       = "your-username"
password       = "your-token"
```

Инициализация:
```bash
terraform init -backend-config=backend.hcl
```

### 3. Использование в GitLab CI/CD

В `.gitlab-ci.yml`:

```yaml
variables:
  TF_HTTP_ADDRESS: "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/terraform/state/ois-cfa-timeweb-prod"
  TF_HTTP_LOCK_ADDRESS: "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/terraform/state/ois-cfa-timeweb-prod/lock"
  TF_HTTP_UNLOCK_ADDRESS: "${CI_API_V4_URL}/projects/${CI_PROJECT_ID}/terraform/state/ois-cfa-timeweb-prod/unlock"
  TF_HTTP_USERNAME: "gitlab-ci-token"
  TF_HTTP_PASSWORD: "${CI_JOB_TOKEN}"

terraform:
  stage: deploy
  script:
    - cd ops/infra/timeweb
    - terraform init
    - terraform plan
    - terraform apply -auto-approve
```

---

## Конфигурация

### Основные переменные

| Переменная | Описание | По умолчанию |
|-----------|----------|--------------|
| `twc_token` | Timeweb Cloud API token | - |
| `cluster_name` | Имя кластера | `ois-cfa-k8s` |
| `cluster_location` | Регион (ru-1, ru-2, ru-3) | `ru-1` |
| `cluster_version` | Версия Kubernetes | `1.28` |
| `node_group_name` | Имя группы узлов | `ois-cfa-nodes` |
| `node_group_preset_id` | ID пресета | `1` |
| `node_count` | Количество узлов | `3` |
| `node_disk_size` | Размер диска (GB) | `50` |
| `vpc_enabled` | Включить VPC | `true` |
| `firewall_enabled` | Включить firewall | `true` |

### Firewall правила

По умолчанию разрешены:
- **6443** — Kubernetes API server
- **30000-32767** — NodePort диапазон
- **22** — SSH (⚠️ ограничить в production)
- **80, 443** — HTTP/HTTPS для ingress

**Рекомендации для production:**
- Ограничить SSH (22) только IP адресами администраторов
- Ограничить API server (6443) только IP адресами CI/CD и админов
- Использовать Network Policies в Kubernetes для дополнительной изоляции

---

## FAQ

### Как узнать доступные пресеты?

1. Через [документацию Timeweb Cloud](https://timeweb.cloud/docs/k8s)
2. Через API:
   ```bash
   curl -H "Authorization: Bearer $TWC_TOKEN" \
     https://api.timeweb.cloud/api/v1/k8s/presets
   ```

### Можно ли изменить размер кластера после создания?

Да, измените `node_count` в `terraform.tfvars` и выполните `terraform apply`. Terraform обновит количество узлов.

### Как обновить версию Kubernetes?

Измените `cluster_version` в `terraform.tfvars` и выполните `terraform apply`. Проверьте доступные версии в документации Timeweb Cloud.

### Как добавить дополнительную группу узлов?

Добавьте ещё один ресурс `twc_k8s_node_group` в `main.tf`:

```hcl
resource "twc_k8s_node_group" "workers" {
  cluster_id = twc_k8s_cluster.main.id
  name       = "ois-cfa-workers"
  preset_id  = 2
  node_count = 5
  disk_size  = 100
}
```

### Как использовать несколько окружений (dev/staging/prod)?

1. Используйте разные `terraform.tfvars` файлы:
   - `terraform.tfvars.dev`
   - `terraform.tfvars.staging`
   - `terraform.tfvars.prod`

2. Используйте workspaces:
   ```bash
   terraform workspace new dev
   terraform workspace new staging
   terraform workspace new prod
   terraform workspace select dev
   ```

3. Используйте разные state файлы в GitLab (разные STATE_NAME).

### Как экспортировать kubeconfig для CI/CD?

В GitLab CI/CD:

```yaml
script:
  - cd ops/infra/timeweb
  - terraform output -raw kubeconfig > kubeconfig.yaml
  - kubectl --kubeconfig=kubeconfig.yaml get nodes
```

Или используйте переменную окружения:
```yaml
script:
  - export KUBECONFIG=$(terraform output -raw kubeconfig)
  - kubectl get nodes
```

---

## Ограничения

### Timeweb Cloud

1. **Минимальное количество узлов:** обычно 2-3 (зависит от пресета)
2. **Максимальное количество узлов:** зависит от тарифа и квот
3. **Версии Kubernetes:** только поддерживаемые Timeweb Cloud (проверьте документацию)
4. **Регионы:** ru-1, ru-2, ru-3 (на момент написания)

### Terraform Provider

1. **Версия провайдера:** используйте `~> 1.6` (совместимость с версиями 1.6.x)
2. **State locking:** обязателен при использовании GitLab backend

### Проект OIS-CFA

1. **Безопасность:** firewall правила должны быть ограничены в production
2. **Стоимость:** учитывайте стоимость узлов при выборе пресета и количества
3. **Backup:** настройте backup для state файла и конфигураций

---

## Troubleshooting

### Ошибка: "Invalid token"

- Проверьте правильность `twc_token` в `terraform.tfvars`
- Убедитесь, что токен имеет необходимые права (k8s, vpc, firewall)

### Ошибка: "State locked"

- Другой процесс использует state
- Проверьте блокировки в GitLab: Settings → CI/CD → Terraform state
- Принудительно разблокируйте: `terraform force-unlock <LOCK_ID>`

### Ошибка: "Preset not found"

- Проверьте актуальные пресеты через API или документацию
- Убедитесь, что `node_group_preset_id` соответствует доступным пресетам

### Ошибка: "Region not available"

- Проверьте доступность региона в вашем аккаунте Timeweb Cloud
- Используйте `ru-1` (Москва) как наиболее доступный

### Кластер создаётся слишком долго

- Создание кластера может занять 10-20 минут
- Проверьте статус в личном кабинете Timeweb Cloud
- Проверьте логи: `terraform apply` с `-debug` флагом

### Не могу подключиться к кластеру

1. Проверьте kubeconfig:
   ```bash
   terraform output -raw kubeconfig > kubeconfig.yaml
   kubectl --kubeconfig=kubeconfig.yaml get nodes
   ```

2. Проверьте firewall правила (должен быть открыт порт 6443)

3. Проверьте доступность API server:
   ```bash
   curl -k $(terraform output -raw api_server_url)
   ```

---

## Timeweb Cloud CLI (twc)

Для работы с кластерами Kubernetes через командную строку используется официальный CLI инструмент `twc`.

### Установка

```bash
# Установка через pip
./tools/timeweb/install.sh

# Или вручную
pip install --user twc-cli
export PATH="${HOME}/.local/bin:${PATH}"
```

### Настройка

```bash
# Вариант 1: Через переменную окружения
export TWC_TOKEN='your-timeweb-cloud-api-token'

# Вариант 2: Через конфигурацию twc
twc config set token 'your-timeweb-cloud-api-token'
```

### Проверка конфигурации

```bash
# Список кластеров
twc k8s cluster list

# Детали кластера
twc k8s cluster get <cluster-id>

# Список node groups
twc k8s node-group list --cluster-id <cluster-id>
```

### Получение kubeconfig

#### Способ 1: Через скрипт (рекомендуется)

```bash
# Экспорт kubeconfig для кластера
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s

# Или с указанием выходного файла
./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s /path/to/kubeconfig.yaml
```

#### Способ 2: Через twc CLI напрямую

```bash
# Получить ID кластера
CLUSTER_ID=$(twc k8s cluster list --format json | jq -r '.[] | select(.name == "ois-cfa-k8s") | .id')

# Экспортировать kubeconfig
twc k8s cluster get-kubeconfig "${CLUSTER_ID}" > kubeconfig.yaml
chmod 600 kubeconfig.yaml
```

#### Способ 3: Через Terraform

```bash
cd ops/infra/timeweb
terraform output -raw kubeconfig > kubeconfig.yaml
chmod 600 kubeconfig.yaml
```

### Использование kubeconfig

```bash
# Установить kubeconfig как текущий
export KUBECONFIG="$(pwd)/kubeconfig.yaml"

# Или добавить в ~/.kube/config
kubectl --kubeconfig=kubeconfig.yaml get nodes

# Проверка подключения
kubectl cluster-info
kubectl get nodes
kubectl get namespaces
```

### Хранение kubeconfig

#### GitLab Secure Files (для ручных проверок)

1. Перейдите в Settings → CI/CD → Secure Files
2. Загрузите `kubeconfig.yaml` как Secure File
3. Используйте в CI/CD через переменную `KUBECONFIG_FILE`

#### GitLab Agent (рекомендуется для production)

См. Task 19 для настройки GitLab Kubernetes Agent.

#### Локальное хранение (только для разработки)

```bash
# Сохранить в ~/.kube/config
mkdir -p ~/.kube
cp kubeconfig.yaml ~/.kube/config-ois-cfa
export KUBECONFIG="${HOME}/.kube/config-ois-cfa:${HOME}/.kube/config"
kubectl config use-context ois-cfa-k8s
```

**⚠️ Внимание:** Не коммитьте kubeconfig в Git. Файл уже добавлен в `.gitignore`.

### MCP Server (экспериментально)

Timeweb Cloud предоставляет экспериментальный MCP (Model Context Protocol) сервер для интеграции с AI-ассистентами.

**Статус:** Экспериментальный, не рекомендуется для production.

**Ссылка:** [Timeweb Cloud MCP Server](https://github.com/timeweb-cloud/mcp-server) (если доступен)

**Текущий подход:** Используем CLI (`twc`) и Terraform как основной способ управления инфраструктурой.

---

## Ссылки

- [Timeweb Cloud Kubernetes Documentation](https://timeweb.cloud/docs/k8s)
- [Timeweb Cloud Terraform Provider](https://registry.terraform.io/providers/timeweb-cloud/timeweb-cloud/latest/docs)
- [Timeweb Cloud CLI Documentation](https://timeweb.cloud/docs/cli)
- [GitLab Terraform State](https://docs.gitlab.com/ee/user/infrastructure/terraform_state.html)
- [Terraform HTTP Backend](https://developer.hashicorp.com/terraform/language/settings/backends/http)

---

## История изменений

| Версия | Дата | Изменения |
|--------|------|-----------|
| 1.1 | 2025-01-27 | Добавлен раздел про Timeweb Cloud CLI и kubeconfig |
| 1.0 | 2025-01-27 | Первоначальная версия |

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

