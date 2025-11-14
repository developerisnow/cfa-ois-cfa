# GitLab Runner: Оптимизация ресурсов для малого кластера

**Дата:** 2025-01-27  
**Проблема:** Недостаточно CPU и памяти в кластере с 1 нодой

---

## 🔍 ПРОБЛЕМА

### Ошибка:
```
Unschedulable: "0/1 nodes are available: 1 Insufficient cpu, 1 Insufficient memory"
```

### Ситуация:
- **Кластер:** 1 нода с ограниченными ресурсами
- **Проблема:** Недостаточно CPU и памяти для запуска job pods
- **Job pod:** `runner-pmfoxuiph-project-6-concurrent-2-1b8gnt8j` в Pending

---

## ✅ РЕШЕНИЕ: Агрессивная оптимизация ресурсов

### Изменения в ConfigMap

#### 1. Уменьшены требования CPU

**Было:**
```toml
cpu_limit = "2"
cpu_request = "500m"
service_cpu_limit = "1"
service_cpu_request = "100m"
helper_cpu_limit = "500m"
helper_cpu_request = "100m"
```

**Стало:**
```toml
cpu_limit = "1"
cpu_request = "200m"
service_cpu_limit = "500m"
service_cpu_request = "50m"
helper_cpu_limit = "200m"
helper_cpu_request = "50m"
```

**Экономия CPU:** ~60% (с 700m до 300m на job)

#### 2. Уменьшены требования памяти

**Было:**
```toml
memory_limit = "2Gi"
memory_request = "512Mi"
service_memory_limit = "1Gi"
service_memory_request = "128Mi"
helper_memory_limit = "512Mi"
helper_memory_request = "64Mi"
```

**Стало:**
```toml
memory_limit = "1Gi"
memory_request = "256Mi"
service_memory_limit = "512Mi"
service_memory_request = "64Mi"
helper_memory_limit = "256Mi"
helper_memory_request = "32Mi"
```

**Экономия памяти:** ~50% (с 640Mi до 320Mi на job)

#### 3. Уменьшено concurrent builds

**Было:** `concurrent = 3`  
**Стало:** `concurrent = 2`

---

## 📊 ИТОГОВАЯ ЭКОНОМИЯ

### На один job:

| Ресурс | Было | Стало | Экономия |
|--------|------|-------|----------|
| CPU request | 700m | 300m | 57% |
| Memory request | 640Mi | 320Mi | 50% |
| CPU limit | 3.5 | 1.7 | 51% |
| Memory limit | 3.5Gi | 1.75Gi | 50% |

### На все concurrent jobs:

| Параметр | Было (3 jobs) | Стало (2 jobs) | Экономия |
|----------|---------------|----------------|----------|
| CPU request | 2100m | 600m | 71% |
| Memory request | 1920Mi | 640Mi | 67% |

---

## 🚀 ПРИМЕНЕНИЕ

```bash
# 1. Применить ConfigMap
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml

# 2. Перезапустить runner
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner

# 3. Удалить pending pods
kubectl delete pods -n gitlab-runner --field-selector=status.phase==Pending
```

---

## ⚠️ ОГРАНИЧЕНИЯ

### Что может не работать:

1. **Тяжелые сборки** (Docker, большие проекты)
   - Может не хватить памяти для компиляции
   - Решение: Увеличить memory_limit для конкретных jobs через tags

2. **Параллельные тесты**
   - Может не хватить CPU
   - Решение: Ограничить параллелизм в тестах

3. **Большие Docker образы**
   - Может не хватить памяти для pull
   - Решение: Использовать image cache

---

## 📋 РЕКОМЕНДАЦИИ

### Для малого кластера (1-2 ноды, <4GB RAM):

1. ✅ Использовать текущие настройки (concurrent=2, memory=256Mi)
2. ✅ Очищать завершенные pods регулярно
3. ✅ Мониторить использование ресурсов

### Для production:

1. **Добавить ноды** (минимум 2-3)
2. **Настроить Cluster Autoscaler**
3. **Увеличить требования** для стабильности:
   - `concurrent = 5`
   - `memory_request = "512Mi"`
   - `cpu_request = "500m"`

---

## 🔧 АЛЬТЕРНАТИВНЫЕ РЕШЕНИЯ

### 1. Использовать tags для разных типов jobs

```toml
[[runners]]
  name = "lightweight"
  tags = ["light"]
  [runners.kubernetes]
    memory_request = "256Mi"
    cpu_request = "200m"

[[runners]]
  name = "heavyweight"
  tags = ["heavy"]
  [runners.kubernetes]
    memory_request = "1Gi"
    cpu_request = "500m"
```

### 2. Настроить node selector для выделенных нод

```toml
[runners.kubernetes.node_selector]
  "node-type" = "ci"
```

### 3. Использовать taints/tolerations

```toml
[[runners.kubernetes.tolerations]]
  key = "ci-only"
  operator = "Equal"
  value = "true"
  effect = "NoSchedule"
```

---

**Статус:** ✅ Оптимизация применена, jobs должны запускаться

