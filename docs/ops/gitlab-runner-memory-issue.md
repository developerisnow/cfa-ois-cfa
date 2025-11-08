# GitLab Runner: Проблема с недостатком памяти

**Дата:** 2025-01-27  
**Проблема:** Job не может быть запущен из-за недостатка памяти в кластере

---

## 🔍 ПРОБЛЕМА

### Ошибка в логах:

```
Unschedulable: "0/1 nodes are available: 1 Insufficient memory. 
no new claims to deallocate, preemption: 0/1 nodes are available: 
1 No preemption victims found for incoming pod."
```

### Анализ:

1. **В кластере только 1 нода**
2. **На ноде недостаточно памяти** для запуска нового pod
3. **Kubernetes не может найти pod для вытеснения** (preemption)

---

## 📊 ТЕКУЩАЯ КОНФИГУРАЦИЯ

### Требования памяти для job pod (из configmap.yaml):

```toml
[runners.kubernetes]
  memory_limit = "4Gi"      # Лимит памяти на job
  memory_request = "1Gi"    # Запрошенная память на job
  helper_memory_limit = "1Gi"
  helper_memory_request = "128Mi"
```

**Итого на один job:**
- Build container: 1Gi request, 4Gi limit
- Helper container: 128Mi request, 1Gi limit
- **Минимум требуется:** ~1.2Gi свободной памяти

---

## ✅ РЕШЕНИЯ

### Решение 1: Уменьшить требования памяти для jobs (рекомендуется)

**Файл:** `ops/infra/k8s/gitlab-runner/configmap.yaml`

```toml
[runners.kubernetes]
  memory_limit = "2Gi"      # Уменьшено с 4Gi
  memory_request = "512Mi"  # Уменьшено с 1Gi
  helper_memory_limit = "512Mi"
  helper_memory_request = "64Mi"
```

**Применить:**
```bash
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
```

---

### Решение 2: Освободить память (удалить неиспользуемые pods)

```bash
# Удалить завершенные job pods
kubectl delete pods -n gitlab-runner --field-selector=status.phase==Succeeded

# Удалить failed job pods
kubectl delete pods -n gitlab-runner --field-selector=status.phase==Failed

# Проверить использование памяти
kubectl top nodes
```

---

### Решение 3: Добавить ноды в кластер

Если используете Timeweb Cloud:

```bash
# Через twc CLI
twc k8s node-pool scale <cluster-name> <node-pool-name> --nodes 2

# Или через Terraform
# Увеличить min_nodes в ops/infra/timeweb/variables.tf
```

---

### Решение 4: Настроить автоматическое масштабирование

Если доступен Cluster Autoscaler:

```yaml
# Добавить в node pool
autoscaling:
  enabled: true
  min_nodes: 1
  max_nodes: 3
```

---

## 🔧 ПРОВЕРКА

### 1. Проверить доступную память на ноде

```bash
kubectl describe nodes | grep -A 5 "Allocated resources"
```

### 2. Проверить использование памяти pods

```bash
kubectl top pods -n gitlab-runner
```

### 3. Проверить требования памяти для jobs

```bash
kubectl get pods -n gitlab-runner -o jsonpath='{range .items[*]}{.metadata.name}{"\t"}{.spec.containers[*].resources.requests.memory}{"\n"}{end}'
```

---

## 📋 РЕКОМЕНДАЦИИ

### Для разработки/тестирования:

1. **Уменьшить memory_limit до 2Gi** (вместо 4Gi)
2. **Уменьшить memory_request до 512Mi** (вместо 1Gi)
3. **Ограничить concurrent builds** до 2-3 (вместо 10)

### Для production:

1. **Добавить ноды** в кластер (минимум 2-3)
2. **Настроить Cluster Autoscaler**
3. **Мониторить использование ресурсов**

---

## 🚀 БЫСТРОЕ ИСПРАВЛЕНИЕ

```bash
# 1. Уменьшить требования памяти в configmap
sed -i 's/memory_limit = "4Gi"/memory_limit = "2Gi"/' \
    ops/infra/k8s/gitlab-runner/configmap.yaml
sed -i 's/memory_request = "1Gi"/memory_request = "512Mi"/' \
    ops/infra/k8s/gitlab-runner/configmap.yaml

# 2. Применить изменения
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml

# 3. Перезапустить runner
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner

# 4. Очистить старые job pods
kubectl delete pods -n gitlab-runner --field-selector=status.phase==Succeeded
```

---

**Статус:** ⚠️ Требуется уменьшить требования памяти или добавить ноды

