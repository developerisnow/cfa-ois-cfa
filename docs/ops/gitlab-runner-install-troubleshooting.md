# GitLab Runner Install: Troubleshooting

**Дата:** 2025-01-27  
**Команда:** `make gitlab-runner-install`

---

## 🔍 ПРОБЛЕМА: Несоответствие placeholder'ов

### Обнаружено

В `Makefile` (строка 272) использовался:
```bash
sed "s/__REPLACE_WITH_RUNNER_TOKEN__/$$RUNNER_TOKEN/g"
```

Но в `configmap.yaml` используется:
```toml
token = "__REPLACE_WITH_GLRT_TOKEN__"
```

**Результат:** Токен не заменялся при установке!

---

## ✅ ИСПРАВЛЕНИЕ

Makefile теперь заменяет **оба** placeholder'а:
```bash
sed -e "s/__REPLACE_WITH_GLRT_TOKEN__/$$RUNNER_TOKEN/g" \
    -e "s/__REPLACE_WITH_RUNNER_TOKEN__/$$RUNNER_TOKEN/g" \
    ops/infra/k8s/gitlab-runner/configmap.yaml
```

---

## 📋 КАК РАБОТАЕТ КОМАНДА

### Выполнение `make gitlab-runner-install`

1. **Проверка RUNNER_TOKEN**
   - Проверяется наличие переменной `RUNNER_TOKEN`
   - Если отсутствует → ошибка с инструкцией

2. **Проверка kubectl**
   - Проверяется наличие `kubectl` в PATH
   - Если отсутствует → ошибка

3. **Проверка KUBECONFIG**
   - Если `KUBECONFIG` не задан → используется `ops/infra/timeweb/kubeconfig.yaml`
   - Проверяется подключение к кластеру
   - Если не подключен → ошибка с инструкцией

4. **Применение манифестов** (в порядке):
   - `namespace.yaml` → создание namespace `gitlab-runner`
   - `rbac.yaml` → ServiceAccount, Role, RoleBinding
   - `configmap.yaml` → **с заменой токена** через `sed`
   - `deployment.yaml` → Deployment с 2 replicas
   - `service.yaml` → Service для метрик

5. **Ожидание pods**
   - Ожидание готовности pods (timeout 120s)

---

## 🔧 ЧТО ПЕРЕДАЕТСЯ

### Переменные окружения

1. **RUNNER_TOKEN** (обязательно)
   - Формат: `glrt-...` (Authentication Token) или `GR...` (Registration Token)
   - Используется для замены в `configmap.yaml`
   - Пример: `export RUNNER_TOKEN="glrt-abc123..."`

2. **KUBECONFIG** (опционально)
   - Путь к kubeconfig файлу
   - Если не задан → используется `ops/infra/timeweb/kubeconfig.yaml`
   - Пример: `export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"`

### Порядок применения манифестов

```bash
1. kubectl apply -f namespace.yaml
2. kubectl apply -f rbac.yaml
3. sed ... configmap.yaml | kubectl apply -f -
4. kubectl apply -f deployment.yaml
5. kubectl apply -f service.yaml
```

---

## 🚀 ИСПОЛЬЗОВАНИЕ

### Базовое использование

```bash
export RUNNER_TOKEN="glrt-ваш-токен"
make gitlab-runner-install
```

### С явным KUBECONFIG

```bash
export RUNNER_TOKEN="glrt-ваш-токен"
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
make gitlab-runner-install
```

### Получение токена

```bash
# Показать инструкции
make gitlab-runner-get-token

# Затем установить
export RUNNER_TOKEN="ваш-токен"
make gitlab-runner-install
```

---

## ❌ ЧАСТЫЕ ОШИБКИ

### 1. "Error: RUNNER_TOKEN not set"

**Причина:** Переменная `RUNNER_TOKEN` не задана

**Решение:**
```bash
export RUNNER_TOKEN="glrt-ваш-токен"
make gitlab-runner-install
```

---

### 2. "Error: kubectl cannot connect to cluster"

**Причина:** KUBECONFIG не настроен или неверный

**Решение:**
```bash
# Настроить kubeconfig
make setup-kubeconfig

# Или вручную
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
kubectl get nodes  # Проверить подключение
```

---

### 3. Токен не заменяется в ConfigMap

**Причина:** Несоответствие placeholder'ов (исправлено)

**Решение:** Используйте обновленную версию Makefile

**Проверка:**
```bash
# Проверить токен в ConfigMap
kubectl get configmap -n gitlab-runner gitlab-runner-config -o jsonpath='{.data.config\.toml}' | grep token
```

---

### 4. Pods не запускаются

**Причина:** Неверный токен или проблемы с сетью

**Решение:**
```bash
# Проверить логи
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50

# Проверить токен
kubectl get configmap -n gitlab-runner gitlab-runner-config -o yaml | grep token
```

---

## ✅ ПРОВЕРКА УСТАНОВКИ

### 1. Проверить pods

```bash
kubectl get pods -n gitlab-runner
```

Ожидается:
```
NAME                             READY   STATUS    RESTARTS   AGE
gitlab-runner-xxx-xxx            1/1     Running   0          1m
gitlab-runner-yyy-yyy            1/1     Running   0          1m
```

### 2. Проверить ConfigMap

```bash
kubectl get configmap -n gitlab-runner gitlab-runner-config -o yaml | grep -A 5 token
```

Ожидается: токен должен быть заменен (не placeholder)

### 3. Проверить логи

```bash
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20
```

Ожидается: строки "Checking for jobs..." без ошибок 403

### 4. Проверить в GitLab UI

1. Откройте: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
2. Раздел: **Runners**
3. Должен появиться runner **ois-cfa-runner** со статусом **Online**

---

## 📚 ДОПОЛНИТЕЛЬНЫЕ РЕСУРСЫ

- [Инструкция для Lens](docs/ops/gitlab-runner-lens-install.md)
- [Полная документация GitLab Runner](docs/ops/gitlab-runner.md)
- [Отчет о соответствии](docs/ops/gitlab-runner-compliance-report.md)

---

**Статус:** ✅ Проблема исправлена, команда работает корректно

