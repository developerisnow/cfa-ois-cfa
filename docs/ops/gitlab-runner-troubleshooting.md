# Troubleshooting GitLab Runner для debug jobs

## Проблема: Job не запускается ("No runners available")

### Причина 1: Раннер не зарегистрирован или не активен

**Проверка:**
1. Откройте GitLab UI → Settings → CI/CD → Runners
2. Проверьте, что раннер отображается и имеет статус "Online" (зелёный индикатор)

**Решение:**
```bash
# Проверить статус раннера в кластере
make gitlab-runner-status

# Если раннер не запущен, перезапустить
make gitlab-runner-restart

# Проверить логи
make gitlab-runner-logs
```

### Причина 2: Раннер требует теги, но job не указал теги

**Проверка:**
- В GitLab UI → Settings → CI/CD → Runners проверьте теги раннера
- Если раннер имеет теги (например, `docker`, `kubernetes`), job должен их указать

**Решение:**

**Вариант A: Убрать теги с раннера (рекомендуется для debug jobs)**
1. GitLab UI → Settings → CI/CD → Runners
2. Нажмите на раннер
3. Уберите все теги (или оставьте пустым)
4. Сохраните

**Вариант B: Добавить теги в job (если раннер требует теги)**
```yaml
debug:deploy:
  tags:
    - docker  # или теги вашего раннера
```

### Причина 3: Раннер не может запустить Docker контейнеры

**Проверка:**
- Раннер должен быть настроен с executor `docker` или `kubernetes`

**Решение:**
- Для debug jobs достаточно executor `docker` или `kubernetes`
- Проверьте конфигурацию раннера в `ops/infra/k8s/gitlab-runner/configmap.yaml`

### Причина 4: Раннер не имеет доступа к kubeconfig

**Проверка:**
- Debug jobs требуют доступ к Kubernetes кластеру
- KUBECONFIG должен быть настроен как переменная CI/CD или в раннере

**Решение:**

**Вариант A: Настроить KUBECONFIG как переменную CI/CD (рекомендуется)**
1. GitLab UI → Settings → CI/CD → Variables
2. Добавить переменную:
   - Key: `KUBECONFIG`
   - Value: содержимое kubeconfig файла (или путь к файлу)
   - Type: File (если путь) или Variable (если содержимое)

**Вариант B: Использовать GitLab Secure Files**
1. GitLab UI → Settings → CI/CD → Secure Files
2. Загрузить `kubeconfig.yaml`
3. В job использовать:
   ```yaml
   before_script:
     - export KUBECONFIG="${CI_PROJECT_DIR}/kubeconfig.yaml"
   ```

**Вариант C: Монтировать kubeconfig в раннер (для Kubernetes executor)**
- Настроить volume mount в конфигурации раннера

## Быстрое решение для debug:deploy

### Шаг 1: Проверить раннер

```bash
# Локально проверить статус раннера
make gitlab-runner-status

# Проверить логи
make gitlab-runner-logs
```

### Шаг 2: Убедиться, что раннер может запускать job'ы

В GitLab UI:
1. Settings → CI/CD → Runners
2. Убедитесь, что раннер:
   - Имеет статус "Online" (зелёный)
   - Не имеет тегов (или теги совпадают с job)
   - Executor: `docker` или `kubernetes`

### Шаг 3: Настроить KUBECONFIG

**В GitLab UI:**
1. Settings → CI/CD → Variables
2. Добавить переменную:
   - Key: `KUBECONFIG`
   - Value: путь к kubeconfig или содержимое файла
   - Type: File (если путь) или Variable

**Или использовать Secure Files:**
1. Settings → CI/CD → Secure Files
2. Загрузить `kubeconfig.yaml`
3. Job автоматически найдёт файл

### Шаг 4: Запустить debug:deploy

1. GitLab UI → CI/CD → Pipelines
2. Запустить pipeline вручную
3. Найти job `debug:deploy`
4. Нажать "Play" (manual job)

## Минимальная конфигурация раннера для debug jobs

Debug jobs требуют минимальную конфигурацию:

```yaml
# configmap.yaml для GitLab Runner
concurrent = 4
check_interval = 0

[[runners]]
  name = "gitlab-runner"
  url = "https://git.telex.global/"
  token = "__REPLACE_WITH_RUNNER_TOKEN__"
  executor = "kubernetes"
  
  [runners.kubernetes]
    namespace = "gitlab-runner"
    image = "bitnami/kubectl:1.30"
    
  # Без тегов - может запускать любые job'ы
  # tags = []
```

## Проверка готовности

```bash
# 1. Проверить раннер
make gitlab-runner-status

# 2. Проверить kubeconfig
make check-kubeconfig

# 3. Проверить подключение к кластеру
kubectl get nodes

# 4. Запустить debug deploy локально (для теста)
export KUBECONFIG="ops/infra/timeweb/kubeconfig.yaml"
kubectl apply -f ops/debug/namespace.yaml
kubectl apply -f ops/debug/serviceaccount.yaml
kubectl apply -f ops/debug/configmap-scripts.yaml
kubectl apply -f ops/debug/debug-pod.yaml
```

## Частые ошибки

### "This job is stuck because the project doesn't have any runners online"

**Решение:**
- Проверьте, что раннер зарегистрирован и онлайн
- Убедитесь, что раннер не имеет тегов (или job указал правильные теги)
- Проверьте, что раннер может запускать Docker контейнеры

### "kubectl: command not found"

**Решение:**
- Job использует образ `bitnami/kubectl:1.30`, kubectl должен быть доступен
- Если ошибка, проверьте, что раннер может загружать Docker образы

### "Cannot connect to cluster"

**Решение:**
- Настройте переменную `KUBECONFIG` в GitLab CI/CD Variables
- Или загрузите kubeconfig как Secure File
- Проверьте, что kubeconfig валиден: `kubectl cluster-info`

### "Permission denied" при применении манифестов

**Решение:**
- Проверьте RBAC права ServiceAccount раннера
- Убедитесь, что раннер имеет права на создание ресурсов в namespace `tools`

