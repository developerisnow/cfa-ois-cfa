# GitLab CI/CD Pipeline для OIS-CFA

**Версия:** 1.0  
**Дата:** 2025-01-27  
**Владелец:** DevOps/SRE

---

## Содержание

1. [Обзор](#обзор)
2. [Стадии Pipeline](#стадии-pipeline)
3. [Build Jobs](#build-jobs)
4. [Test Jobs](#test-jobs)
5. [Deploy Jobs](#deploy-jobs)
6. [Environments](#environments)
7. [Артефакты](#артефакты)
8. [Переменные](#переменные)
9. [Troubleshooting](#troubleshooting)

---

## Обзор

GitLab CI/CD pipeline для автоматизации сборки, тестирования и развёртывания OIS-CFA.

### Стадии

1. **infra** — управление инфраструктурой (Terraform)
2. **build** — сборка Docker образов
3. **test** — тестирование (unit, pact, e2e, k6)
4. **deploy** — развёртывание в окружения

### Файл конфигурации

`.gitlab-ci.yml` в корне репозитория

---

## Стадии Pipeline

### 1. INFRA

Управление инфраструктурой через Terraform.

**Jobs:**
- `terraform:plan` — планирование изменений
- `terraform:apply` — применение изменений (manual)

**Триггеры:**
- Web UI (ручной запуск)
- Ветки `infra/*`

**Артефакты:**
- `tfplan` — план Terraform

### 2. BUILD

Сборка Docker образов для всех сервисов и приложений.

**Backend Services:**
- `build:api-gateway`
- `build:identity`
- `build:issuance`
- `build:registry`
- `build:settlement`
- `build:compliance`
- `build:bank-nominal`

**Frontend Apps:**
- `build:portal-issuer`
- `build:portal-investor`
- `build:backoffice`

**Особенности:**
- Multi-platform build (linux/amd64, linux/arm64)
- Registry cache для ускорения сборки
- Тег `latest` только для default branch

**Образы сохраняются в:**
- `$CI_REGISTRY_IMAGE/<service-name>:$CI_COMMIT_SHA`
- `$CI_REGISTRY_IMAGE/<service-name>:latest` (только для main)

### 3. TEST

Тестирование на разных уровнях.

**Jobs:**
- `test:unit` — unit тесты (.NET/xUnit)
- `test:pact` — contract testing (Pact)
- `test:e2e` — end-to-end тесты (Playwright)
- `test:k6:smoke` — нагрузочное тестирование (k6)
- `validate:specs` — валидация OpenAPI/AsyncAPI спецификаций

**Артефакты:**
- JUnit XML отчёты
- Coverage отчёты
- Pact файлы
- Playwright HTML отчёты
- k6 summary JSON

### 4. DEPLOY

Развёртывание в окружения.

**Environments:**
- `dev` — feature branches, merge requests
- `staging` — main branch
- `production` — tags v* (manual)

**Варианты развёртывания:**
- **Option A:** ArgoCD (staging, production)
- **Option B:** GitLab Agent (dev, pull-based GitOps)

---

## Build Jobs

### Backend Services

Все backend сервисы используют шаблон `.docker_build_template`:

```yaml
build:api-gateway:
  variables:
    IMAGE_NAME: api-gateway
    DOCKERFILE: apps/api-gateway/Dockerfile
    BUILD_CONTEXT: .
```

**Особенности:**
- Docker Buildx для multi-platform
- Registry cache
- Параллельная сборка всех сервисов

### Frontend Apps

Frontend приложения используют шаблон `.build_frontend_template`:

```yaml
build:portal-issuer:
  variables:
    IMAGE_NAME: portal-issuer
    APP_PATH: apps/portal-issuer
```

**Особенности:**
- Node.js build перед Docker
- Single platform (linux/amd64)

---

## Test Jobs

### Unit Tests

```yaml
test:unit:
  script:
    - dotnet test --logger "junit;LogFileName=junit.xml"
```

**Артефакты:**
- `junit.xml` — JUnit отчёт
- `coverage/` — coverage отчёты

**Coverage:**
- Формат: Cobertura
- Threshold: настраивается в проекте

### Pact Tests

```yaml
test:pact:
  script:
    - cd tests/contracts/pact-consumer
    - npm ci && npm test
```

**Артефакты:**
- `pacts/` — Pact файлы

**Publish:**
- Публикация в Pact Broker (если настроен)
- Только для default branch

### E2E Tests

```yaml
test:e2e:
  image: mcr.microsoft.com/playwright:v1.47.2-jammy
  script:
    - cd tests/e2e
    - npm ci
    - npx playwright install --with-deps
    - npm test
```

**Артефакты:**
- `playwright-report/` — HTML отчёт
- `test-results/junit.xml` — JUnit отчёт

**Триггеры:**
- Default branch
- Feature branches
- Merge requests

### K6 Smoke Tests

```yaml
test:k6:smoke:
  variables:
    K6_SMOKE_VUS: "10"
    K6_SMOKE_DURATION: "30s"
  script:
    - k6 run --summary-export=summary.json tests/k6/gateway-critical-paths.js
```

**Артефакты:**
- `summary.json` — summary отчёт
- `k6-results.json` — детальные результаты

**Thresholds:**
- Настраиваются в k6 скриптах

---

## Deploy Jobs

### Option A: ArgoCD

```yaml
deploy:staging:
  script:
    - argocd app sync ois-staging --grpc-web
    - argocd app wait ois-staging --grpc-web
```

**Требования:**
- ArgoCD установлен в кластере
- `ARGOCD_ADMIN_PASSWORD` переменная настроена
- `ARGOCD_SERVER` указывает на ArgoCD server

**Используется для:**
- `staging` (main branch)
- `production` (tags v*, manual)

### Option B: GitLab Agent

```yaml
deploy:dev:
  script:
    - echo "GitLab Agent syncs manifests automatically"
    - kubectl get deployments -n $KUBERNETES_NAMESPACE
```

**Требования:**
- GitLab Agent установлен в кластере
- Манифесты в `ops/gitops/gitlab-agent/manifests/`

**Используется для:**
- `dev` (feature branches, merge requests)

**Pull-based GitOps:**
- Агент автоматически синхронизирует изменения
- CI только обновляет манифесты в Git

---

## Environments

### DEV

**Триггеры:**
- Feature branches (`feature/*`)
- Merge requests

**Deploy:**
- GitLab Agent (pull-based)
- Автоматический

**URL:**
- `https://dev.ois-cfa.example.com`

### STAGING

**Триггеры:**
- Main branch

**Deploy:**
- ArgoCD
- Автоматический

**URL:**
- `https://staging.ois-cfa.example.com`

### PRODUCTION

**Триггеры:**
- Tags `v*.*.*` (например, `v1.0.0`)

**Deploy:**
- ArgoCD
- Manual (требует подтверждения)

**URL:**
- `https://ois-cfa.example.com`

---

## Артефакты

### JUnit Reports

```yaml
artifacts:
  reports:
    junit: junit.xml
```

**Используется для:**
- Unit tests
- E2E tests

**Отображение:**
- GitLab UI → Test Reports

### Coverage Reports

```yaml
artifacts:
  paths:
    - coverage/
```

**Формат:**
- Cobertura XML
- HTML

**Отображение:**
- GitLab UI → Coverage

### Pact Files

```yaml
artifacts:
  paths:
    - tests/contracts/pact-consumer/pacts/
```

**Использование:**
- Публикация в Pact Broker
- Contract verification

### Playwright Reports

```yaml
artifacts:
  paths:
    - tests/e2e/playwright-report/
```

**Формат:**
- HTML отчёт
- Скриншоты и видео

**Отображение:**
- Скачивание артефактов

### K6 Reports

```yaml
artifacts:
  paths:
    - summary.json
    - k6-results.json
```

**Формат:**
- JSON summary
- Детальные результаты

---

## Переменные

### Обязательные

| Переменная | Описание | Где настроить |
|-----------|----------|---------------|
| `ARGOCD_ADMIN_PASSWORD` | ArgoCD admin password | GitLab CI/CD Variables |
| `PACT_BROKER_URL` | Pact Broker URL (опционально) | GitLab CI/CD Variables |
| `PACT_BROKER_TOKEN` | Pact Broker token (опционально) | GitLab CI/CD Variables |

### Terraform Backend

| Переменная | Описание |
|-----------|----------|
| `TF_HTTP_ADDRESS` | GitLab Terraform state address |
| `TF_HTTP_USERNAME` | GitLab username |
| `TF_HTTP_PASSWORD` | GitLab token |

### Настройка в GitLab

1. Settings → CI/CD → Variables
2. Добавить переменные
3. Установить флаги:
   - **Masked** — для секретов
   - **Protected** — только для protected branches/tags

---

## Troubleshooting

### Build fails: "docker buildx not found"

**Решение:**
- Убедитесь, что используется `docker:24-dind` image
- Проверьте, что `docker buildx create` выполняется

### Test fails: "No tests found"

**Решение:**
- Проверьте структуру тестов
- Убедитесь, что тесты не пропущены (skip)

### Deploy fails: "ArgoCD not accessible"

**Решение:**
- Проверьте `ARGOCD_SERVER` переменную
- Убедитесь, что ArgoCD доступен из CI runner
- Проверьте `ARGOCD_ADMIN_PASSWORD`

### GitLab Agent not syncing

**Решение:**
- Проверьте статус агента: `kubectl get pods -n gitlab-agent`
- Проверьте конфигурацию: `.gitlab/agents/ois-cfa-agent/config.yaml`
- Проверьте логи агента

### Images not in registry

**Решение:**
- Проверьте права доступа к registry
- Убедитесь, что `CI_REGISTRY_USER` и `CI_REGISTRY_PASSWORD` установлены
- Проверьте, что registry доступен

---

## Примеры использования

### Запуск pipeline вручную

1. GitLab UI → CI/CD → Pipelines
2. Run pipeline
3. Выбрать branch
4. Запустить

### Deploy в production

1. Создать tag: `git tag v1.0.0 && git push origin v1.0.0`
2. Pipeline запустится автоматически
3. Подтвердить manual job `deploy:prod`

### Просмотр артефактов

1. GitLab UI → CI/CD → Pipelines
2. Выбрать pipeline
3. Выбрать job
4. Download artifacts

---

## Ссылки

- [GitLab CI/CD Documentation](https://docs.gitlab.com/ee/ci/)
- [Docker Buildx](https://docs.docker.com/buildx/)
- [ArgoCD CLI](https://argo-cd.readthedocs.io/en/stable/user-guide/commands/argocd/)
- [GitLab Agent](https://docs.gitlab.com/ee/user/clusters/agent/)

---

**Примечание:** Все даты в формате Europe/Moscow (UTC+3).

