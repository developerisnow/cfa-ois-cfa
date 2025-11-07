# Принудительная перерегистрация GitLab Runner

**Дата:** 2025-01-27  
**Проблема:** Runner получает 403 Forbidden, даже с новым registration token  
**Причина:** Runner уже зарегистрирован и использует старый authentication token

---

## 🔍 ДИАГНОСТИКА

### Симптомы:
- ✅ Registration token обновлен в ConfigMap
- ✅ ConfigMap правильно монтируется в pod
- ❌ Runner все еще получает 403 Forbidden
- ❌ Runner ID: `HYErDk_6w` (старый runner)

### Причина:
Runner уже был зарегистрирован ранее и использует **сохраненный authentication token**, который недействителен. Runner **игнорирует** registration token из ConfigMap, потому что он уже зарегистрирован.

---

## 🔧 РЕШЕНИЕ: Принудительная перерегистрация

### Вариант 1: Удалить runner из GitLab UI (РЕКОМЕНДУЕТСЯ)

1. **Открыть GitLab UI:**
   - https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
   - Раздел: Runners

2. **Найти и удалить runner:**
   - Найти runner с ID `HYErDk_6w` (или похожим)
   - Нажать "Remove runner" или "Delete"

3. **Перезапустить pods:**
   ```bash
   kubectl delete pods -n gitlab-runner -l app=gitlab-runner
   ```

4. **Проверить регистрацию:**
   ```bash
   kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50
   # Должно быть: "Runner registered successfully" или подобное
   ```

5. **Проверить в GitLab UI:**
   - Новый runner должен появиться
   - Статус: "Online" и "Active"

---

### Вариант 2: Очистить сохраненную конфигурацию в pod

Если runner сохраняет конфигурацию в файл (например, `.runner_system_id`):

1. **Найти файлы конфигурации:**
   ```bash
   kubectl exec -n gitlab-runner <pod-name> -- ls -la /etc/gitlab-runner/
   ```

2. **Удалить сохраненную конфигурацию:**
   ```bash
   kubectl exec -n gitlab-runner <pod-name> -- rm -f /etc/gitlab-runner/.runner_system_id
   kubectl exec -n gitlab-runner <pod-name> -- rm -f /etc/gitlab-runner/.runner_*
   ```

3. **Перезапустить pod:**
   ```bash
   kubectl delete pod <pod-name> -n gitlab-runner
   ```

**Проблема:** ConfigMap монтируется как read-only, поэтому файлы могут не сохраняться между перезапусками.

---

### Вариант 3: Использовать PersistentVolume для конфигурации

Если нужно сохранять конфигурацию runner'а между перезапусками:

1. **Создать PersistentVolumeClaim:**
   ```yaml
   apiVersion: v1
   kind: PersistentVolumeClaim
   metadata:
     name: gitlab-runner-config
     namespace: gitlab-runner
   spec:
     accessModes:
       - ReadWriteOnce
     resources:
       requests:
         storage: 1Gi
   ```

2. **Обновить deployment для использования PVC:**
   ```yaml
   volumes:
     - name: config
       configMap:
         name: gitlab-runner-config
     - name: runner-state
       persistentVolumeClaim:
         claimName: gitlab-runner-config
   volumeMounts:
     - name: config
       mountPath: /etc/gitlab-runner
     - name: runner-state
       mountPath: /etc/gitlab-runner/.runner_state
   ```

Но это усложняет конфигурацию. Лучше использовать Вариант 1.

---

### Вариант 4: Использовать initContainer для очистки

Добавить initContainer, который очищает старую конфигурацию:

```yaml
initContainers:
  - name: clear-runner-state
    image: busybox
    command:
      - sh
      - -c
      - |
        # Очистить старую конфигурацию, если есть
        rm -f /runner-state/.runner_system_id || true
        rm -f /runner-state/.runner_* || true
    volumeMounts:
      - name: runner-state
        mountPath: /runner-state
```

Но это тоже усложняет конфигурацию.

---

## ✅ РЕКОМЕНДУЕМОЕ РЕШЕНИЕ

**Использовать Вариант 1: Удалить runner из GitLab UI**

Это самый простой и надежный способ:

1. Удалить старый runner из GitLab UI
2. Перезапустить pods
3. Runner автоматически перерегистрируется с новым registration token

---

## 🔍 ПРОВЕРКА УСПЕХА

После применения решения:

1. **Проверить логи:**
   ```bash
   kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50
   # Должно быть: "Checking for jobs..." БЕЗ ошибок 403
   ```

2. **Проверить в GitLab UI:**
   - Settings → CI/CD → Runners
   - Новый runner должен быть "Online" и "Active"
   - Runner ID должен быть другим (не `HYErDk_6w`)

3. **Запустить тестовый job:**
   - Создать простой job в `.gitlab-ci.yml`
   - Проверить, что job запускается на runner'е

---

## 📝 КОМАНДЫ ДЛЯ БЫСТРОГО ИСПРАВЛЕНИЯ

```bash
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# 1. Удалить все pods (они пересоздадутся)
kubectl delete pods -n gitlab-runner -l app=gitlab-runner

# 2. Проверить логи через 30 секунд
sleep 30
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50

# 3. Если все еще 403, нужно удалить runner из GitLab UI вручную
```

---

## ⚠️ ВАЖНО

- Registration token используется только для **первой регистрации**
- После регистрации GitLab Runner получает **authentication token**
- Authentication token сохраняется и используется для всех последующих запросов
- Если authentication token недействителен → 403 Forbidden
- Обновление registration token в ConfigMap **не поможет**, если runner уже зарегистрирован

---

**Следующий шаг:** Удалить runner с ID `HYErDk_6w` из GitLab UI и перезапустить pods.

