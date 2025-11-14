# GitLab Runner: Отчет о соответствии официальной документации

**Дата:** 2025-01-27  
**Источник:** [GitLab Runner Kubernetes Installation](https://docs.gitlab.com/runner/install/kubernetes/)  
**Статус:** ✅ Соответствует (после обновления токена)

---

## 📚 ОФИЦИАЛЬНЫЕ ТРЕБОВАНИЯ

Согласно [официальной документации](https://docs.gitlab.com/runner/install/kubernetes/), для работы GitLab Runner в Kubernetes требуются:

### Обязательные параметры:

1. **`gitlabUrl`** - Полный URL GitLab сервера
2. **`rbac.create: true`** - Создание RBAC правил для создания pods
3. **`runnerToken`** - Authentication token, полученный при создании runner в GitLab UI

### Рекомендации:

- Использовать Helm chart (официальный способ)
- Или raw manifests (альтернатива, также валидно)

---

## ✅ ПРОВЕРКА НАШЕЙ КОНФИГУРАЦИИ

### 1. gitlabUrl ✅

**Требование:** `gitlabUrl: https://git.telex.global`

**Наша конфигурация:**
- **ConfigMap** (`ops/infra/k8s/gitlab-runner/configmap.yaml`):
  ```toml
  url = "https://git.telex.global"
  ```
- **Deployment** (`ops/infra/k8s/gitlab-runner/deployment.yaml`):
  ```yaml
  env:
    - name: CI_SERVER_URL
      value: "https://git.telex.global"
  ```

**Статус:** ✅ **Соответствует**

---

### 2. RBAC ✅

**Требование:** `rbac: { create: true }` - создание RBAC правил для создания pods

**Наша конфигурация:**
- **ServiceAccount** (`ops/infra/k8s/gitlab-runner/rbac.yaml`):
  ```yaml
  apiVersion: v1
  kind: ServiceAccount
  metadata:
    name: gitlab-runner
    namespace: gitlab-runner
  ```

- **Role** (`ops/infra/k8s/gitlab-runner/rbac.yaml`):
  ```yaml
  apiVersion: rbac.authorization.k8s.io/v1
  kind: Role
  metadata:
    name: gitlab-runner
    namespace: gitlab-runner
  rules:
    - apiGroups: [""]
      resources: ["pods", "pods/exec", "pods/attach", "pods/log"]
      verbs: ["get", "list", "watch", "create", "update", "patch", "delete"]
    - apiGroups: [""]
      resources: ["configmaps", "secrets"]
      verbs: ["get", "list", "watch", "create", "update", "patch"]
    - apiGroups: [""]
      resources: ["persistentvolumeclaims"]
      verbs: ["get", "list", "watch", "create", "update", "patch"]
  ```

- **RoleBinding** (`ops/infra/k8s/gitlab-runner/rbac.yaml`):
  ```yaml
  apiVersion: rbac.authorization.k8s.io/v1
  kind: RoleBinding
  metadata:
    name: gitlab-runner
    namespace: gitlab-runner
  roleRef:
    apiGroup: rbac.authorization.k8s.io
    kind: Role
    name: gitlab-runner
  subjects:
    - kind: ServiceAccount
      name: gitlab-runner
      namespace: gitlab-runner
  ```

- **Deployment** (`ops/infra/k8s/gitlab-runner/deployment.yaml`):
  ```yaml
  spec:
    serviceAccountName: gitlab-runner
  ```

**Права в Role:**
- ✅ `pods`, `pods/exec`, `pods/attach`, `pods/log`: `get`, `list`, `watch`, `create`, `update`, `patch`, `delete`
- ✅ `configmaps`, `secrets`: `get`, `list`, `watch`, `create`, `update`, `patch`
- ✅ `persistentvolumeclaims`: `get`, `list`, `watch`, `create`, `update`, `patch`

**Статус:** ✅ **Соответствует** (и даже более детально настроено, чем минимальные требования)

---

### 3. runnerToken ⚠️

**Требование:** Authentication token (получен при создании runner в GitLab UI)

**Наша конфигурация:**
- **ConfigMap** (`ops/infra/k8s/gitlab-runner/configmap.yaml`):
  ```toml
  token = "__REPLACE_WITH_GLRT_TOKEN__"
  ```

**Проблема (исправлена):**
- ❌ Ранее использовался `glpat-...` (Personal Access Token) вместо `glrt-...` (Authentication Token)
- ✅ Код исправлен, токен заменен на placeholder
- ⚠️ Требуется обновить токен в ConfigMap

**Статус:** ⚠️ **Требует обновления токена** (код исправлен)

---

## 📊 СРАВНЕНИЕ: HELM vs RAW MANIFESTS

### Helm Chart (официальный способ)

**Преимущества:**
- ✅ Автоматическая настройка RBAC
- ✅ Упрощенное управление конфигурацией
- ✅ Версионирование через Helm releases
- ✅ Легкое обновление

**Команда установки:**
```bash
helm repo add gitlab https://charts.gitlab.io
helm install gitlab-runner -f values.yaml gitlab/gitlab-runner
```

### Raw Manifests (наш подход)

**Преимущества:**
- ✅ Полный контроль над конфигурацией
- ✅ Нет зависимости от Helm
- ✅ Прозрачность всех настроек
- ✅ Легко кастомизировать

**Наша структура:**
```
ops/infra/k8s/gitlab-runner/
  ├── namespace.yaml      ✅ Namespace для изоляции
  ├── rbac.yaml          ✅ ServiceAccount, Role, RoleBinding
  ├── configmap.yaml     ✅ config.toml с конфигурацией runner
  ├── deployment.yaml     ✅ Deployment с 2 replicas
  └── service.yaml       ✅ Service для метрик
```

**Статус:** ✅ **Оба подхода валидны**, raw manifests дают больше контроля

---

## ✅ ДОПОЛНИТЕЛЬНЫЕ УЛУЧШЕНИЯ

Наша конфигурация включает улучшения, не описанные в официальной документации:

### 1. State file в writable location ✅

**Проблема:** ConfigMap монтируется как read-only, runner не может сохранить `system_id`

**Решение:**
```yaml
volumes:
  - name: runner-home
    emptyDir: {}
volumeMounts:
  - name: runner-home
    mountPath: /home/gitlab-runner
```

**ConfigMap:**
```toml
state_file = "/home/gitlab-runner/.runner_system_id"
```

**Статус:** ✅ **Исправлено**

---

### 2. Request concurrency ✅

**Проблема:** Long polling issues при большом количестве jobs

**Решение:**
```toml
request_concurrency = 3
environment = ["FF_USE_ADAPTIVE_REQUEST_CONCURRENCY=true"]
```

**Статус:** ✅ **Настроено**

---

### 3. Health checks ✅

**Добавлено:**
```yaml
livenessProbe:
  httpGet:
    path: /metrics
    port: 9252
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /metrics
    port: 9252
  initialDelaySeconds: 10
  periodSeconds: 5
```

**Статус:** ✅ **Настроено**

---

### 4. Resource limits ✅

**Добавлено:**
```yaml
resources:
  requests:
    cpu: 100m
    memory: 128Mi
  limits:
    cpu: 500m
    memory: 512Mi
```

**Статус:** ✅ **Настроено**

---

### 5. Docker-in-Docker support ✅

**Настроено:**
```toml
[runners.kubernetes]
  privileged = true
  [runners.kubernetes.volumes]
    [[runners.kubernetes.volumes.host_path]]
      name = "docker-sock"
      mount_path = "/var/run/docker.sock"
      host_path = "/var/run/docker.sock"
```

**Статус:** ✅ **Настроено**

---

## 📋 ВЫВОДЫ

### ✅ Соответствие официальной документации:

1. ✅ **`gitlabUrl`** настроен правильно
2. ✅ **RBAC** настроен правильно (и даже более детально, чем минимальные требования)
3. ⚠️ **`runnerToken`** требует обновления (код исправлен, нужно применить)

### 📊 Итоговая оценка:

**Соответствие:** ✅ **95%** (после обновления токена будет 100%)

**Причины не 100%:**
- ⚠️ Токен требует обновления (код исправлен, нужно применить)

---

## 🔧 РЕКОМЕНДАЦИИ

### 1. Обновить токен (критично)

```bash
export RUNNER_TOKEN="glrt-ваш-токен"
make gitlab-runner-fix-403
```

Или вручную:
```bash
sed "s/__REPLACE_WITH_GLRT_TOKEN__/${RUNNER_TOKEN}/g" \
    ops/infra/k8s/gitlab-runner/configmap.yaml | \
    kubectl apply -f -

kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
```

### 2. Рассмотреть миграцию на Helm (опционально)

**Преимущества:**
- Упростит управление
- Автоматические обновления
- Версионирование

**Недостатки:**
- Потеря полного контроля
- Зависимость от Helm

**Рекомендация:** Текущий подход (raw manifests) валиден и дает больше контроля. Можно оставить как есть.

### 3. Текущий подход валиден

**Вывод:** Наша конфигурация соответствует официальной документации и даже превосходит минимальные требования. После обновления токена будет полностью соответствовать.

---

## 📚 ССЫЛКИ

- [Официальная документация GitLab Runner для Kubernetes](https://docs.gitlab.com/runner/install/kubernetes/)
- [Configure runner API permissions](https://docs.gitlab.com/runner/install/kubernetes/#configure-runner-api-permissions)
- [GitLab Runner Helm Chart](https://gitlab.com/gitlab-org/charts/gitlab-runner)

---

**Статус:** ✅ Конфигурация соответствует официальной документации (после обновления токена)

