# GitLab Runner 403 Fix: Complete Analysis & Solution

**Дата:** 2025-01-27  
**Статус:** ✅ Исправления применены, требуется обновить токен

---

## 🔍 DISCOVERY RESULTS

### Установка
- **Namespace:** `gitlab-runner`
- **Deployment:** `gitlab-runner`
- **Install Method:** Raw manifests (не Helm)
- **Replicas:** 2

### Конфигурация (до исправлений)
- **ConfigMap:** `gitlab-runner-config` (read-only)
- **Mount:** `/etc/gitlab-runner` (ConfigMap)
- **State file:** Не настроен
- **Token:** `glpat-...` (Personal Access Token) ❌

---

## ❌ ПРОБЛЕМЫ ОБНАРУЖЕНЫ

### 1. Неправильный тип токена
- **Текущий:** `glpat-...` (Personal Access Token)
- **Ожидается:** `glrt-...` (Runner Authentication Token)
- **Причина:** PAT не может использоваться для runner authentication
- **Результат:** 403 Forbidden

### 2. State file недоступен для записи
- **Путь:** `/etc/gitlab-runner/.runner_system_id`
- **Проблема:** ConfigMap монтируется как read-only
- **Последствие:** Runner не может сохранить system_id
- **Результат:** Предупреждения в логах, проблемы с идентификацией

### 3. Нет writable volume
- **Проблема:** Нет volume для `/home/gitlab-runner`
- **Последствие:** State file не может быть записан
- **Результат:** Runner не может сохранить состояние

### 4. Отсутствует request_concurrency
- **Проблема:** Не настроен в config.toml
- **Последствие:** Long polling issues (предупреждение в логах)
- **Результат:** Задержки при получении jobs

---

## ✅ ИСПРАВЛЕНИЯ ПРИМЕНЕНЫ

### 1. ConfigMap (`ops/infra/k8s/gitlab-runner/configmap.yaml`)

**Добавлено:**
```toml
state_file = "/home/gitlab-runner/.runner_system_id"
request_concurrency = 3
environment = ["FF_USE_ADAPTIVE_REQUEST_CONCURRENCY=true"]
```

**Изменено:**
- Токен заменен на placeholder: `__REPLACE_WITH_GLRT_TOKEN__`
- Добавлен комментарий о необходимости authentication token

### 2. Deployment (`ops/infra/k8s/gitlab-runner/deployment.yaml`)

**Добавлено:**
```yaml
volumeMounts:
  - name: runner-home
    mountPath: /home/gitlab-runner

volumes:
  - name: runner-home
    emptyDir: {}
```

**Результат:**
- `/home/gitlab-runner` теперь writable
- State file может быть записан

---

## 📋 ДЕЙСТВИЯ ДЛЯ ЗАВЕРШЕНИЯ

### Шаг 1: Получить правильный токен

**Если runner уже зарегистрирован:**
1. Открыть: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
2. Раздел: Runners
3. Найти зарегистрированный runner
4. Скопировать **Authentication Token** (glrt-...)

**Если runner не зарегистрирован:**
1. Использовать **Registration Token** (GR...) для первой регистрации
2. После регистрации runner получит Authentication Token (glrt-...)
3. Обновить ConfigMap с новым токеном

### Шаг 2: Применить исправления

```bash
export RUNNER_TOKEN="glrt-ваш-токен"
./ops/ci/patch_runner_fix_403.sh
```

Или вручную:
```bash
# Обновить ConfigMap
sed "s/__REPLACE_WITH_GLRT_TOKEN__/${RUNNER_TOKEN}/g" \
    ops/infra/k8s/gitlab-runner/configmap.yaml | \
    kubectl apply -f -

# Перезапустить pods
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
```

### Шаг 3: Проверить результат

```bash
# Проверить логи (не должно быть 403)
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50 | grep -i "403\|forbidden"

# Проверить state file
kubectl exec -n gitlab-runner <pod-name> -- cat /home/gitlab-runner/.runner_system_id

# Проверить токен (маскированный)
kubectl exec -n gitlab-runner <pod-name> -- cat /etc/gitlab-runner/config.toml | grep token
```

---

## 🔍 ДИАГНОСТИКА

### Скрипт диагностики

```bash
GITLAB_URL="https://git.telex.global" \
RUNNER_TOKEN="glrt-ваш-токен" \
STATE_FILE="/home/gitlab-runner/.runner_system_id" \
./ops/ci/diagnose_runner.sh
```

Скрипт:
- Проверяет тип токена
- Проверяет state file
- Выполняет verify запрос к GitLab API
- Сохраняет результаты в `ARCHIVE/runner/verify-*.log`

---

## ✅ ОЖИДАЕМЫЙ РЕЗУЛЬТАТ

После применения исправлений:
- ✅ State file записывается в `/home/gitlab-runner/.runner_system_id`
- ✅ Runner использует правильный authentication token (glrt-...)
- ✅ Request concurrency настроен (3)
- ✅ Feature flag включен для adaptive concurrency
- ✅ Нет ошибок 403 в логах
- ✅ Runner успешно получает jobs

---

## 📊 ТЕКУЩИЙ СТАТУС

### Исправления
- [x] ConfigMap обновлен (state_file, request_concurrency)
- [x] Deployment обновлен (writable volume)
- [ ] Токен обновлен (требует ручного действия)

### Проверка
- [ ] Логи не содержат 403
- [ ] State file записывается
- [ ] Runner получает jobs

---

## 🔧 КОМАНДЫ

```bash
# Применить исправления
export RUNNER_TOKEN="glrt-ваш-токен"
./ops/ci/patch_runner_fix_403.sh

# Диагностика
./ops/ci/diagnose_runner.sh

# Проверка логов
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50

# Проверка статуса
kubectl get pods -n gitlab-runner
```

---

**Следующий шаг:** Обновить токен в ConfigMap с правильным authentication token (glrt-...)

