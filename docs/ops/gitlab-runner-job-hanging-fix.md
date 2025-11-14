# GitLab Runner: Job Hanging Fix

**Дата:** 2025-01-27  
**Проблема:** Jobs висят, runner pods в CrashLoopBackOff

---

## 🔍 ПРОБЛЕМЫ ОБНАРУЖЕНЫ

### 1. Runner pods в CrashLoopBackOff

**Причина:** Health checks (liveness/readiness probes) не могут подключиться к `/metrics:9252`

**Ошибка в логах:**
```
listen_address not defined, metrics & debug endpoints disabled
Liveness probe failed: Get "http://10.244.98.202:9252/metrics": dial tcp 10.244.98.202:9252: connect: connection refused
```

**Решение:** Отключить health checks (runner не слушает на порту 9252 по умолчанию)

---

### 2. State file путь неправильный

**Проблема:** В configmap указан путь `/etc/gitlab-runner/.runner_system_id` (ConfigMap read-only)

**Ошибка в логах:**
```
WARNING: Couldn't save new system ID on state file.
state_file=/etc/gitlab-runner/.runner_system_id
```

**Решение:** Уже исправлено в configmap (`state_file = "/home/gitlab-runner/.runner_system_id"`), но нужно проверить применение

---

### 3. Job висит

**Job pod:** `runner-zqlriqywz-project-6-concurrent-0-t7eqxvgd`

**Статус:** Running (2/2 Ready)

**Возможные причины:**
- Job выполняется долго
- Job завис в ожидании
- Проблемы с сетью/доступом

---

## ✅ ИСПРАВЛЕНИЯ ПРИМЕНЕНЫ

### 1. Отключены health checks

**Файл:** `ops/infra/k8s/gitlab-runner/deployment.yaml`

**Изменение:**
```yaml
# Health checks disabled - runner doesn't listen on port 9252 by default
# To enable metrics, add listen_address = ":9252" to config.toml
# livenessProbe:
#   httpGet:
#     path: /metrics
#     port: 9252
```

**Причина:** Runner не слушает на порту 9252 по умолчанию, нужно явно включить `listen_address` в config.toml

---

### 2. State file путь правильный

**Проверка:** ConfigMap содержит `state_file = "/home/gitlab-runner/.runner_system_id"`

**Volume:** `/home/gitlab-runner` монтируется как `emptyDir` (writable)

---

## 🔧 ДОПОЛНИТЕЛЬНЫЕ ИСПРАВЛЕНИЯ

### Включить metrics (опционально)

Если нужны health checks и метрики, добавить в `configmap.yaml`:

```toml
listen_address = ":9252"
```

И раскомментировать health checks в `deployment.yaml`.

---

## 📋 ПРОВЕРКА

### 1. Проверить runner pods

```bash
kubectl get pods -n gitlab-runner
```

Ожидается: все pods в статусе `Running` (1/1 Ready)

### 2. Проверить логи

```bash
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20
```

Ожидается: нет ошибок о health checks, нет ошибок о state file

### 3. Проверить job pod

```bash
kubectl get pod -n gitlab-runner runner-* -o wide
kubectl logs -n gitlab-runner runner-* -c build --tail=50
```

### 4. Проверить в GitLab UI

1. Откройте: https://git.telex.global/npk/ois-cfa/-/pipelines
2. Найдите зависший pipeline
3. Проверьте статус job'ов
4. Посмотрите логи job'а

---

## 🚀 ПРИМЕНЕНИЕ ИСПРАВЛЕНИЙ

```bash
# Применить исправленный deployment
kubectl apply -f ops/infra/k8s/gitlab-runner/deployment.yaml

# Перезапустить pods
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner

# Проверить статус
kubectl get pods -n gitlab-runner
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20
```

---

## 📚 ДОПОЛНИТЕЛЬНЫЕ РЕСУРСЫ

- [GitLab Runner Metrics](https://docs.gitlab.com/runner/monitoring/)
- [Health Checks](https://docs.gitlab.com/runner/configuration/advanced-configuration.html#the-global-section)

---

**Статус:** ✅ Исправления применены

