# Исправление GitLab Runner: Токен в ConfigMap

**Дата:** 2025-01-27  
**Проблема:** GitLab Runner не получал токен из ConfigMap  
**Решение:** Исправлен deployment для правильного монтирования config.toml

---

## 🔍 ПРОБЛЕМА

GitLab Runner получал 403 Forbidden, потому что токен не передавался в pod правильно. Пользователь вручную добавил раннер в поде, и он появился в GitLab, что подтвердило, что токен правильный, но проблема в конфигурации.

---

## ✅ РЕШЕНИЕ

### 1. Токен добавлен в ConfigMap

Токен `GR1348941HYErDk_6wh8UsSenSgsU` добавлен в `ops/infra/k8s/gitlab-runner/configmap.yaml`:

```yaml
[[runners]]
  name = "ois-cfa-runner"
  url = "https://git.telex.global"
  token = "GR1348941HYErDk_6wh8UsSenSgsU"
  executor = "kubernetes"
```

### 2. Исправлен Deployment

Deployment обновлен для правильного монтирования ConfigMap:

```yaml
volumes:
  - name: config
    configMap:
      name: gitlab-runner-config
      items:
        - key: config.toml
          path: config.toml
```

Это гарантирует, что файл `config.toml` правильно монтируется в `/etc/gitlab-runner/config.toml`.

---

## 🔧 КОМАНДЫ ДЛЯ ПРИМЕНЕНИЯ

```bash
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# 1. Применить ConfigMap с токеном
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml

# 2. Применить исправленный deployment
kubectl apply -f ops/infra/k8s/gitlab-runner/deployment.yaml

# 3. Перезапустить pods (если нужно)
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner

# 4. Проверить статус
kubectl get pods -n gitlab-runner
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50
```

---

## ✅ ПРОВЕРКА

1. **Проверить, что pods запущены:**
   ```bash
   kubectl get pods -n gitlab-runner
   # Ожидается: 2/2 Running
   ```

2. **Проверить логи:**
   ```bash
   kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50
   # Должно быть: "Checking for jobs..." без ошибок 403
   ```

3. **Проверить в GitLab UI:**
   - Settings → CI/CD → Runners
   - Runner должен быть "Online" и "Active"

4. **Проверить файл config.toml в pod:**
   ```bash
   POD_NAME=$(kubectl get pods -n gitlab-runner -l app=gitlab-runner -o jsonpath='{.items[0].metadata.name}')
   kubectl exec -n gitlab-runner $POD_NAME -- cat /etc/gitlab-runner/config.toml | grep token
   # Должен показать: token = "GR1348941HYErDk_6wh8UsSenSgsU"
   ```

---

## 📝 ИЗМЕНЕНИЯ В КОДЕ

### `ops/infra/k8s/gitlab-runner/configmap.yaml`
- ✅ Токен добавлен: `token = "GR1348941HYErDk_6wh8UsSenSgsU"`

### `ops/infra/k8s/gitlab-runner/deployment.yaml`
- ✅ Добавлен `items` в volume для правильного монтирования `config.toml`

---

## 🎯 РЕЗУЛЬТАТ

После применения изменений:
- ✅ ConfigMap содержит правильный токен
- ✅ Deployment правильно монтирует config.toml
- ✅ GitLab Runner должен успешно регистрироваться
- ✅ Jobs должны запускаться без ошибок 403

---

## 🔄 ОБНОВЛЕНИЕ ТОКЕНА В БУДУЩЕМ

Если токен истечет или будет отозван:

1. **Получить новый токен из GitLab UI:**
   - Settings → CI/CD → Runners
   - Reset registration token

2. **Обновить ConfigMap:**
   ```bash
   # Отредактировать ops/infra/k8s/gitlab-runner/configmap.yaml
   # Заменить токен на новый
   
   # Применить
   kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml
   
   # Перезапустить pods
   kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
   ```

**Или использовать Makefile:**
```bash
export RUNNER_TOKEN="новый-токен"
make gitlab-runner-update-token
```

---

**Статус:** ✅ Исправлено и готово к использованию

