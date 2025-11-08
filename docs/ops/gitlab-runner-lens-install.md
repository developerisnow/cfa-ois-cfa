    # Установка GitLab Runner через Lens

**Дата:** 2025-01-27  
**Инструмент:** Lens IDE  
**Цель:** Создать GitLab Runner pod в Kubernetes кластере через графический интерфейс

---

## 📋 ПРЕДВАРИТЕЛЬНЫЕ ТРЕБОВАНИЯ

1. **Lens установлен и подключен к кластеру**
   - Откройте Lens
   - Подключите кластер (Timeweb Kubernetes)
   - Проверьте подключение: `kubectl get nodes`

2. **Получен Runner Token**
   - Откройте: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
   - Раздел: Runners
   - Скопируйте **Authentication Token** (glrt-...) или **Registration Token** (GR...)

3. **Файлы манифестов готовы**
   - `ops/infra/k8s/gitlab-runner/namespace.yaml`
   - `ops/infra/k8s/gitlab-runner/rbac.yaml`
   - `ops/infra/k8s/gitlab-runner/configmap.yaml`
   - `ops/infra/k8s/gitlab-runner/deployment.yaml`
   - `ops/infra/k8s/gitlab-runner/service.yaml`

---

## 🚀 ПОШАГОВАЯ ИНСТРУКЦИЯ

### Шаг 1: Создать Namespace

1. В Lens откройте **Workloads** → **Namespaces**
2. Нажмите **+** (Create) → **Create from YAML**
3. Скопируйте содержимое `ops/infra/k8s/gitlab-runner/namespace.yaml`:

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: gitlab-runner
  labels:
    name: gitlab-runner
    app.kubernetes.io/name: gitlab-runner
    app.kubernetes.io/component: ci-cd
```

4. Нажмите **Create**
5. Проверьте: Namespace `gitlab-runner` должен появиться в списке

---

### Шаг 2: Создать RBAC (ServiceAccount, Role, RoleBinding)

1. В Lens откройте **Workloads** → **Namespaces** → выберите `gitlab-runner`
2. Перейдите в **Config** → **Service Accounts**
3. Нажмите **+** (Create) → **Create from YAML**
4. Скопируйте содержимое `ops/infra/k8s/gitlab-runner/rbac.yaml`:

```yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: gitlab-runner
  namespace: gitlab-runner
---
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
---
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

5. Нажмите **Create**
6. Проверьте:
   - **Config** → **Service Accounts**: должен появиться `gitlab-runner`
   - **Config** → **Roles**: должен появиться `gitlab-runner`
   - **Config** → **Role Bindings**: должен появиться `gitlab-runner`

---

### Шаг 3: Создать ConfigMap

1. В Lens откройте **Workloads** → **Namespaces** → выберите `gitlab-runner`
2. Перейдите в **Config** → **Config Maps**
3. Нажмите **+** (Create) → **Create from YAML**
4. Откройте файл `ops/infra/k8s/gitlab-runner/configmap.yaml`
5. **ВАЖНО:** Замените `__REPLACE_WITH_GLRT_TOKEN__` на ваш токен:

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: gitlab-runner-config
  namespace: gitlab-runner
data:
  config.toml: |
    concurrent = 10
    check_interval = 3
    log_level = "info"
    state_file = "/home/gitlab-runner/.runner_system_id"

    [[runners]]
      name = "ois-cfa-runner"
      url = "https://git.telex.global"
      token = "glrt-ВАШ-ТОКЕН-ЗДЕСЬ"  # ← Замените на ваш токен
      executor = "kubernetes"
      request_concurrency = 3
      environment = ["FF_USE_ADAPTIVE_REQUEST_CONCURRENCY=true"]
      [runners.kubernetes]
        namespace = "gitlab-runner"
        image = "alpine:latest"
        privileged = true
        cpu_limit = "2"
        memory_limit = "4Gi"
        cpu_request = "500m"
        memory_request = "1Gi"
        # ... остальная конфигурация
```

6. Нажмите **Create**
7. Проверьте: **Config** → **Config Maps** → должен появиться `gitlab-runner-config`

---

### Шаг 4: Создать Deployment

1. В Lens откройте **Workloads** → **Namespaces** → выберите `gitlab-runner`
2. Перейдите в **Workloads** → **Deployments**
3. Нажмите **+** (Create) → **Create from YAML**
4. Скопируйте содержимое `ops/infra/k8s/gitlab-runner/deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: gitlab-runner
  namespace: gitlab-runner
  labels:
    app: gitlab-runner
spec:
  replicas: 2
  selector:
    matchLabels:
      app: gitlab-runner
  template:
    metadata:
      labels:
        app: gitlab-runner
    spec:
      serviceAccountName: gitlab-runner
      containers:
        - name: gitlab-runner
          image: gitlab/gitlab-runner:latest
          imagePullPolicy: IfNotPresent
          args:
            - run
            - --config=/etc/gitlab-runner/config.toml
          env:
            - name: CI_SERVER_URL
              value: "https://git.telex.global"
            - name: RUNNER_EXECUTOR
              value: "kubernetes"
            - name: RUNNER_REQUESTED_CONCURRENT_BUILDS
              value: "10"
            - name: RUNNER_OUTPUT_LIMIT
              value: "4096"
          volumeMounts:
            - name: config
              mountPath: /etc/gitlab-runner
            - name: runner-home
              mountPath: /home/gitlab-runner
          resources:
            requests:
              cpu: 100m
              memory: 128Mi
            limits:
              cpu: 500m
              memory: 512Mi
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
      volumes:
        - name: config
          configMap:
            name: gitlab-runner-config
            items:
              - key: config.toml
                path: config.toml
        - name: runner-home
          emptyDir: {}
```

5. Нажмите **Create**
6. Проверьте: **Workloads** → **Deployments** → должен появиться `gitlab-runner`
7. Дождитесь готовности: статус должен стать **Running** (2/2 pods)

---

### Шаг 5: Создать Service (опционально, для метрик)

1. В Lens откройте **Workloads** → **Namespaces** → выберите `gitlab-runner`
2. Перейдите в **Network** → **Services**
3. Нажмите **+** (Create) → **Create from YAML**
4. Скопируйте содержимое `ops/infra/k8s/gitlab-runner/service.yaml`:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: gitlab-runner
  namespace: gitlab-runner
  labels:
    app: gitlab-runner
spec:
  type: ClusterIP
  ports:
    - name: metrics
      port: 9252
      targetPort: 9252
      protocol: TCP
  selector:
    app: gitlab-runner
```

5. Нажмите **Create**
6. Проверьте: **Network** → **Services** → должен появиться `gitlab-runner`

---

## ✅ ПРОВЕРКА УСТАНОВКИ

### В Lens:

1. **Workloads** → **Deployments** → `gitlab-runner`
   - Статус: **Running** (2/2 pods)
   - Все pods в статусе **Running**

2. **Workloads** → **Pods** → выберите pod `gitlab-runner-*`
   - Статус: **Running**
   - Логи: нажмите на pod → **Logs** → должны быть строки "Checking for jobs..."

3. **Config** → **Config Maps** → `gitlab-runner-config`
   - Должен содержать `config.toml` с правильным токеном

### В GitLab UI:

1. Откройте: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
2. Раздел: **Runners**
3. Должен появиться runner с именем **ois-cfa-runner**
4. Статус: **Online** (зеленый индикатор)

---

## 🔧 УСТРАНЕНИЕ ПРОБЛЕМ

### Pod не запускается

1. Проверьте логи:
   - В Lens: выберите pod → **Logs**
   - Ищите ошибки: `403 Forbidden`, `token invalid`

2. Проверьте ConfigMap:
   - Токен должен быть правильного формата (glrt-... или GR...)
   - URL должен быть `https://git.telex.global`

3. Проверьте RBAC:
   - ServiceAccount должен быть `gitlab-runner`
   - Role должен иметь права на создание pods

### Runner не регистрируется в GitLab

1. Проверьте токен:
   - Если используете Registration Token (GR...), runner должен зарегистрироваться автоматически
   - Если используете Authentication Token (glrt-...), runner уже должен быть зарегистрирован

2. Проверьте сеть:
   - Pod должен иметь доступ к `https://git.telex.global`
   - Проверьте: `kubectl exec -n gitlab-runner <pod-name> -- wget -O- https://git.telex.global`

3. Проверьте логи:
   - Ищите ошибки подключения или авторизации

---

## 📚 ДОПОЛНИТЕЛЬНЫЕ РЕСУРСЫ

- [Официальная документация GitLab Runner](https://docs.gitlab.com/runner/)
- [GitLab Runner для Kubernetes](https://docs.gitlab.com/runner/install/kubernetes/)
- [Lens Documentation](https://k8slens.dev/)

---

## 🎯 БЫСТРАЯ УСТАНОВКА (через Makefile)

Если предпочитаете командную строку:

```bash
export RUNNER_TOKEN="glrt-ваш-токен"
make gitlab-runner-install
```

---

**Статус:** ✅ Инструкция готова к использованию

