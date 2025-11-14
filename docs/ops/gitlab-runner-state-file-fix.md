# GitLab Runner: State File Fix

**Дата:** 2025-01-27  
**Проблема:** Runner не может сохранить system_id в state file

---

## 🔍 ПРОБЛЕМА

### Ошибка в логах:

```
WARNING: Couldn't save new system ID on state file. In order to reliably identify this runner in jobs with a known identifier,
please ensure there is a text file at the location specified in `state_file` with the contents of `system_id`. Example: echo "r_BmJdiXfGe0Lc" > "/etc/gitlab-runner/.runner_system_id"
  state_file=/etc/gitlab-runner/.runner_system_id system_id=r_BmJdiXfGe0Lc
```

### Анализ проблемы:

1. **В configmap.yaml указан путь:** `/home/gitlab-runner/.runner_system_id`
2. **Runner пытается сохранить в:** `/etc/gitlab-runner/.runner_system_id`
3. **Причина:** ConfigMap не обновлен в кластере или pods не перезапущены после обновления

---

## ✅ РЕШЕНИЕ

### 1. Проверить ConfigMap в кластере

```bash
kubectl get configmap -n gitlab-runner gitlab-runner-config -o jsonpath='{.data.config\.toml}' | grep state_file
```

Должно быть:
```toml
state_file = "/home/gitlab-runner/.runner_system_id"
```

### 2. Применить исправленный ConfigMap

```bash
kubectl apply -f ops/infra/k8s/gitlab-runner/configmap.yaml
```

### 3. Перезапустить pods

```bash
kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
```

### 4. Проверить результат

```bash
# Проверить config в pod
POD=$(kubectl get pods -n gitlab-runner -l app=gitlab-runner -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n gitlab-runner $POD -- cat /etc/gitlab-runner/config.toml | grep state_file

# Проверить логи (не должно быть WARNING о state file)
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20 | grep -i "state_file\|system_id"
```

---

## 📋 КОНФИГУРАЦИЯ

### ConfigMap (`ops/infra/k8s/gitlab-runner/configmap.yaml`)

```toml
state_file = "/home/gitlab-runner/.runner_system_id"
```

### Deployment (`ops/infra/k8s/gitlab-runner/deployment.yaml`)

```yaml
volumeMounts:
  - name: runner-home
    mountPath: /home/gitlab-runner

volumes:
  - name: runner-home
    emptyDir: {}
```

**Важно:** 
- `/home/gitlab-runner` монтируется как `emptyDir` (writable)
- `/etc/gitlab-runner` монтируется как ConfigMap (read-only)

---

## 🔧 ПРОВЕРКА

### После применения исправлений:

1. **Config в pod должен содержать:**
   ```toml
   state_file = "/home/gitlab-runner/.runner_system_id"
   ```

2. **Логи не должны содержать:**
   ```
   WARNING: Couldn't save new system ID on state file
   state_file=/etc/gitlab-runner/.runner_system_id
   ```

3. **Файл должен быть создан:**
   ```bash
   kubectl exec -n gitlab-runner <pod-name> -- cat /home/gitlab-runner/.runner_system_id
   ```

---

## 📚 ДОПОЛНИТЕЛЬНАЯ ИНФОРМАЦИЯ

### Почему это важно:

- **System ID** используется для идентификации runner'а в jobs
- Без сохранения system_id runner может создавать новый ID при каждом перезапуске
- Это может привести к проблемам с отслеживанием и идентификацией runner'а

### Альтернативное решение (если проблема сохраняется):

Если runner все еще пытается использовать `/etc/gitlab-runner/.runner_system_id`, можно создать initContainer для создания файла:

```yaml
initContainers:
  - name: init-state-file
    image: alpine:latest
    command: ['sh', '-c', 'mkdir -p /home/gitlab-runner && touch /home/gitlab-runner/.runner_system_id']
    volumeMounts:
      - name: runner-home
        mountPath: /home/gitlab-runner
```

Но это не должно быть необходимо, если ConfigMap применен правильно.

---

**Статус:** ✅ Решение применено

