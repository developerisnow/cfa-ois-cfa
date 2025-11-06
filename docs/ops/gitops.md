# GitOps для OIS-CFA: ArgoCD vs GitLab Agent

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Содержание

1. [Обзор](#обзор)
2. [Сравнение вариантов](#сравнение-вариантов)
3. [Рекомендация](#рекомендация)
4. [ArgoCD Setup](#argocd-setup)
5. [GitLab Agent Setup](#gitlab-agent-setup)
6. [Архитектура GitOps](#архитектура-gitops)
7. [Безопасность](#безопасность)
8. [Troubleshooting](#troubleshooting)

---

## Обзор

GitOps — это методология управления инфраструктурой и приложениями через декларативные манифесты, хранящиеся в Git. Для проекта OIS-CFA рассматриваются два варианта:

- **Option A: ArgoCD** — независимый GitOps инструмент с богатым функционалом
- **Option B: GitLab Agent for Kubernetes** — нативная интеграция GitLab с Kubernetes

### Требования проекта

- **Многоуровневая синхронизация:** system → platform → business
- **SSO интеграция:** Keycloak для аутентификации
- **RBAC:** роли с разными правами (admin, developer, auditor)
- **Аудит:** полное логирование всех операций
- **Соответствие:** ГОСТ 57580.x, СТО БР

---

## Сравнение вариантов

### ArgoCD

#### Плюсы ✅

1. **Богатый функционал:**
   - App-of-Apps паттерн из коробки
   - Множество источников (Git, Helm, Kustomize, S3)
   - Sync policies и автоматизация
   - Health checks и status reporting
   - Rollback и history

2. **UI и визуализация:**
   - Веб-интерфейс с детальным статусом
   - Визуализация зависимостей
   - Diff view (желаемое vs фактическое состояние)

3. **Зрелость:**
   - Широко используется в production
   - Большое сообщество
   - Множество интеграций

4. **SSO и RBAC:**
   - Поддержка OIDC/OAuth2 (Keycloak)
   - Гибкая настройка RBAC
   - Project-based изоляция

5. **Независимость:**
   - Не привязан к конкретному Git-провайдеру
   - Работает с любым Git репозиторием

#### Минусы ❌

1. **Дополнительный компонент:**
   - Требует установки и поддержки
   - Дополнительные ресурсы (CPU, память)

2. **Сложность настройки:**
   - Требует настройки SSO, RBAC
   - Необходимо управлять сертификатами

3. **Отдельный UI:**
   - Еще один интерфейс для изучения
   - Не интегрирован с GitLab UI

### GitLab Agent for Kubernetes

#### Плюсы ✅

1. **Нативная интеграция:**
   - Полная интеграция с GitLab UI
   - Единый интерфейс для CI/CD и GitOps
   - Pull-based синхронизация (безопаснее)

2. **Простота:**
   - Меньше компонентов для управления
   - Автоматическая регистрация агента
   - Управление через GitLab UI

3. **Безопасность:**
   - Pull-based модель (агент запрашивает изменения)
   - Нет необходимости открывать порты в кластере
   - Интеграция с GitLab RBAC

4. **CI/CD интеграция:**
   - Единый workflow для CI/CD и GitOps
   - Использование GitLab переменных и секретов

5. **Меньше ресурсов:**
   - Легковесный агент (kas)
   - Меньше overhead

#### Минусы ❌

1. **Ограниченный функционал:**
   - Меньше возможностей по сравнению с ArgoCD
   - Нет встроенного UI для визуализации
   - Ограниченная поддержка источников

2. **Привязка к GitLab:**
   - Работает только с GitLab
   - Нет возможности использовать другие Git-провайдеры

3. **Меньше зрелости:**
   - Относительно новый функционал
   - Меньше примеров и документации

4. **SSO:**
   - Зависит от GitLab SSO
   - Меньше гибкости в настройке RBAC

---

## Рекомендация

### Для проекта OIS-CFA: **ArgoCD**

**Обоснование:**

1. **Регуляторные требования:**
   - Требуется детальный аудит и логирование
   - ArgoCD предоставляет полную историю изменений
   - Гибкая настройка RBAC для соответствия ГОСТ 57580.x

2. **Многоуровневая архитектура:**
   - App-of-Apps паттерн идеально подходит для system/platform/business
   - Сложные зависимости между компонентами
   - ArgoCD лучше справляется с такими сценариями

3. **Независимость:**
   - Не привязан к GitLab (можно мигрировать на другой Git-провайдер)
   - Работает с любыми источниками (Git, Helm, S3)

4. **Зрелость и надежность:**
   - Проверен в production на многих проектах
   - Большое сообщество и поддержка

5. **Визуализация:**
   - UI помогает в отладке и мониторинге
   - Важно для compliance и аудита

### Когда использовать GitLab Agent:

- Если проект полностью на GitLab и не планируется миграция
- Если нужна максимальная простота и минимум компонентов
- Если команда уже использует GitLab CI/CD и хочет единый интерфейс

---

## ArgoCD Setup

### Структура

```
ops/gitops/argocd/
├── bootstrap/              # Bootstrap манифесты
│   ├── namespace.yaml
│   ├── argocd-install.yaml
│   └── app-of-apps.yaml   # Root application
├── apps/                  # Application definitions
│   ├── system/            # System applications
│   │   ├── argocd.yaml
│   │   ├── monitoring.yaml
│   │   └── ingress.yaml
│   ├── platform/          # Platform applications
│   │   ├── keycloak.yaml
│   │   ├── vault.yaml
│   │   └── postgresql.yaml
│   └── business/          # Business applications
│       ├── api-gateway.yaml
│       ├── services.yaml
│       └── frontend.yaml
├── config/                # ArgoCD configuration
│   ├── rbac.yaml         # RBAC policies
│   ├── sso-keycloak.yaml # SSO configuration
│   └── projects.yaml     # Project definitions
└── helm/                  # Helm values for ArgoCD
    └── values.yaml
```

### Установка

```bash
# 1. Создать namespace
kubectl create namespace argocd

# 2. Установить ArgoCD
helm repo add argo https://argoproj.github.io/argo-helm
helm install argocd argo/argo-cd \
  --namespace argocd \
  --values ops/gitops/argocd/helm/values.yaml \
  --wait

# 3. Получить пароль admin
kubectl -n argocd get secret argocd-initial-admin-secret -o jsonpath="{.data.password}" | base64 -d

# 4. Настроить SSO (Keycloak)
kubectl apply -f ops/gitops/argocd/config/sso-keycloak.yaml

# 5. Настроить RBAC
kubectl apply -f ops/gitops/argocd/config/rbac.yaml

# 6. Применить app-of-apps
kubectl apply -f ops/gitops/argocd/bootstrap/app-of-apps.yaml
```

### App-of-Apps паттерн

```yaml
# Root application (app-of-apps)
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: root
  namespace: argocd
spec:
  project: default
  source:
    repoURL: https://gitlab.com/ois-cfa/ois-cfa.git
    targetRevision: main
    path: ops/gitops/argocd/apps
  destination:
    server: https://kubernetes.default.svc
    namespace: argocd
  syncPolicy:
    automated:
      prune: true
      selfHeal: true
    syncOptions:
      - CreateNamespace=true
```

### Sync порядок

1. **System** (базовая инфраструктура)
   - ArgoCD, Ingress, Monitoring
2. **Platform** (платформенные сервисы)
   - Keycloak, Vault, PostgreSQL
3. **Business** (бизнес-приложения)
   - API Gateway, Services, Frontend

---

## GitLab Agent Setup

### Структура

```
ops/gitops/gitlab-agent/
├── agent-config.yaml      # Конфигурация агента
├── manifests/             # Манифесты для синхронизации
│   ├── system/           # System manifests
│   ├── platform/         # Platform manifests
│   └── business/         # Business manifests
└── README.md
```

### Регистрация агента

1. **В GitLab UI:**
   - Infrastructure → Kubernetes clusters
   - Add Kubernetes cluster → GitLab Agent
   - Создать новый агент (например, `ois-cfa-agent`)

2. **Установка агента:**

```bash
# Получить токен регистрации из GitLab UI
AGENT_TOKEN="your-agent-token"

# Установить через Helm
helm repo add gitlab https://charts.gitlab.io
helm install gitlab-agent gitlab/gitlab-agent \
  --namespace gitlab-agent \
  --create-namespace \
  --set config.token=${AGENT_TOKEN} \
  --set config.kasAddress=wss://gitlab.com/-/kubernetes-agent/
```

3. **Подключить репозиторий манифестов:**

В GitLab UI:
- Infrastructure → Kubernetes clusters → ваш кластер
- Connected agents → ваш агент
- Add manifest repository
- Указать путь: `ops/gitops/gitlab-agent/manifests`

### Sync правила

```yaml
# .gitlab/agents/ois-cfa-agent/config.yaml
gitops:
  manifest_projects:
    - id: ois-cfa/ois-cfa
      default_namespace: default
      paths:
        - glob: 'ops/gitops/gitlab-agent/manifests/system/**'
          sync_policy:
            order: 1
        - glob: 'ops/gitops/gitlab-agent/manifests/platform/**'
          sync_policy:
            order: 2
            depends_on:
              - 'ops/gitops/gitlab-agent/manifests/system/**'
        - glob: 'ops/gitops/gitlab-agent/manifests/business/**'
          sync_policy:
            order: 3
            depends_on:
              - 'ops/gitops/gitlab-agent/manifests/platform/**'
```

---

## Архитектура GitOps

### Многоуровневая синхронизация

```
┌─────────────────────────────────────────────────┐
│              Git Repository                     │
│  (ops/gitops/argocd/apps или manifests/)       │
└─────────────────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────┐
│         GitOps Controller                        │
│  (ArgoCD Application Controller или GitLab KAS) │
└─────────────────────────────────────────────────┘
                    │
        ┌───────────┼───────────┐
        ▼           ▼           ▼
   ┌────────┐  ┌────────┐  ┌────────┐
   │ System │  │Platform │  │Business │
   └────────┘  └────────┘  └────────┘
        │           │           │
        └───────────┼───────────┘
                    ▼
        ┌───────────────────────┐
        │   Kubernetes Cluster  │
        └───────────────────────┘
```

### Порядок синхронизации

1. **System** (базовая инфраструктура)
   - Namespaces
   - Ingress Controller
   - Monitoring (Prometheus, Grafana)
   - ArgoCD (если используется)

2. **Platform** (платформенные сервисы)
   - Keycloak (SSO)
   - Vault (secrets)
   - PostgreSQL
   - Kafka
   - Redis

3. **Business** (бизнес-приложения)
   - API Gateway
   - Backend Services (Identity, Issuance, Registry, Settlement)
   - Frontend (Portal Issuer, Portal Investor, Backoffice)
   - Hyperledger Fabric (Peers, Orderers, CA)

---

## Безопасность

### ArgoCD

1. **SSO (Keycloak):**
   - OIDC интеграция
   - Группы и роли
   - Session management

2. **RBAC:**
   - Project-based изоляция
   - Роли: admin, developer, auditor
   - Политики доступа

3. **Аудит:**
   - Логирование всех операций
   - История изменений
   - Интеграция с SIEM

### GitLab Agent

1. **Аутентификация:**
   - GitLab токены
   - Интеграция с GitLab RBAC

2. **Безопасность сети:**
   - Pull-based модель
   - Нет открытых портов в кластере

3. **Аудит:**
   - Логи в GitLab
   - Audit logs в Kubernetes

---

## Troubleshooting

### ArgoCD

**Приложение не синхронизируется:**
```bash
# Проверить статус
kubectl get applications -n argocd

# Логи
kubectl logs -n argocd -l app.kubernetes.io/name=argocd-application-controller

# Принудительная синхронизация
argocd app sync <app-name>
```

**SSO не работает:**
```bash
# Проверить конфигурацию
kubectl get configmap argocd-cm -n argocd -o yaml

# Проверить подключение к Keycloak
curl -k https://keycloak.example.com/realms/argocd/.well-known/openid-configuration
```

### GitLab Agent

**Агент не подключается:**
```bash
# Проверить статус
kubectl get pods -n gitlab-agent

# Логи
kubectl logs -n gitlab-agent -l app=gitlab-agent

# Проверить токен
kubectl get secret -n gitlab-agent gitlab-agent-token
```

**Манифесты не применяются:**
- Проверить путь в конфигурации агента
- Проверить права доступа к репозиторию
- Проверить формат манифестов (YAML)

---

## Ссылки

- [ArgoCD Documentation](https://argo-cd.readthedocs.io/)
- [GitLab Kubernetes Agent](https://docs.gitlab.com/ee/user/clusters/agent/)
- [GitOps Principles](https://www.gitops.tech/)
- [App-of-Apps Pattern](https://argo-cd.readthedocs.io/en/stable/operator-manual/cluster-bootstrapping/)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

