# Быстрый старт: debug:deploy job

## Проблема: "No runners available"

Если job `debug:deploy` не запускается из-за отсутствия раннеров, выполните следующие шаги.

## Шаг 1: Проверить статус раннера

```bash
# Проверить статус раннера в кластере
make check-runner-status

# Или вручную
make gitlab-runner-status
```

**Ожидаемый результат:**
- Раннер pods в статусе "Running"
- Раннер виден в GitLab UI как "Online"

## Шаг 2: Проверить раннер в GitLab UI

1. Откройте: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
2. Раздел: **Runners**
3. Проверьте:
   - ✅ Раннер отображается
   - ✅ Статус: **Online** (зелёный индикатор)
   - ✅ Теги: **пустые** (или совпадают с job)

**Если раннер не онлайн:**
- Проверьте логи: `make gitlab-runner-logs`
- Перезапустите: `make gitlab-runner-restart`

**Если раннер имеет теги:**
- Уберите теги в GitLab UI (Settings → CI/CD → Runners → Edit runner)
- Или добавьте теги в job (см. ниже)

## Шаг 3: Настроить KUBECONFIG

Job требует доступ к Kubernetes кластеру. Настройте переменную `KUBECONFIG`:

**Вариант A: Через GitLab CI/CD Variables (рекомендуется)**

1. GitLab UI → Settings → CI/CD → Variables
2. Добавить переменную:
   - Key: `KUBECONFIG`
   - Value: содержимое файла `ops/infra/timeweb/kubeconfig.yaml`
   - Type: **Variable** (или **File** если путь)
   - Protected: ✅ (для production)
   - Masked: ❌ (kubeconfig слишком большой)

**Вариант B: Через GitLab Secure Files**

1. GitLab UI → Settings → CI/CD → Secure Files
2. Загрузить файл `ops/infra/timeweb/kubeconfig.yaml`
3. Job автоматически найдёт файл

**Вариант C: Локальная проверка**

```bash
# Экспортировать kubeconfig
make setup-kubeconfig

# Проверить подключение
kubectl get nodes
```

## Шаг 4: Запустить debug:deploy

1. GitLab UI → CI/CD → Pipelines
2. Запустить pipeline (или открыть существующий)
3. Найти job `debug:deploy`
4. Нажать **Play** (manual job)

## Если раннер требует теги

Если раннер настроен с тегами и не может запустить job без тегов:

**Вариант A: Убрать теги с раннера (рекомендуется)**
- GitLab UI → Settings → CI/CD → Runners → Edit
- Убрать все теги
- Сохранить

**Вариант B: Добавить теги в job**

В `.gitlab-ci.yml` добавить:
```yaml
debug:deploy:
  tags:
    - kubernetes  # или теги вашего раннера
```

## Troubleshooting

### Ошибка: "This job is stuck because the project doesn't have any runners online"

**Причины:**
1. Раннер не зарегистрирован
2. Раннер не онлайн
3. Раннер имеет теги, а job не указал теги
4. Раннер не может запустить Docker контейнеры

**Решение:**
```bash
# 1. Проверить статус
make check-runner-status

# 2. Проверить логи
make gitlab-runner-logs

# 3. Перезапустить раннер
make gitlab-runner-restart

# 4. Проверить в GitLab UI
# Settings → CI/CD → Runners
```

### Ошибка: "KUBECONFIG not found"

**Решение:**
1. Настроить переменную `KUBECONFIG` в GitLab CI/CD Variables
2. Или загрузить kubeconfig как Secure File
3. Или убедиться, что файл `ops/infra/timeweb/kubeconfig.yaml` существует в репозитории

### Ошибка: "Cannot connect to cluster"

**Решение:**
```bash
# Проверить kubeconfig локально
export KUBECONFIG="ops/infra/timeweb/kubeconfig.yaml"
kubectl cluster-info
kubectl get nodes

# Если не работает, обновить kubeconfig
make setup-kubeconfig
```

## Минимальная конфигурация для debug:deploy

Job `debug:deploy` требует:
- ✅ Раннер онлайн (любой executor: docker, kubernetes)
- ✅ KUBECONFIG настроен (переменная или файл)
- ✅ Раннер может запускать Docker контейнеры
- ✅ Раннер не требует теги (или job указал теги)

## Проверка готовности

```bash
# 1. Проверить раннер
make check-runner-status

# 2. Проверить kubeconfig
make check-kubeconfig

# 3. Проверить подключение
export KUBECONFIG="ops/infra/timeweb/kubeconfig.yaml"
kubectl get nodes

# 4. Запустить debug deploy локально (тест)
kubectl apply -f ops/debug/namespace.yaml
kubectl apply -f ops/debug/serviceaccount.yaml
kubectl apply -f ops/debug/configmap-scripts.yaml
kubectl apply -f ops/debug/debug-pod.yaml
```

## После успешного deploy

```bash
# Проверить pod
kubectl get pods -n tools

# Подключиться к debug pod
kubectl exec -it -n tools debug-toolbox -- /bin/bash

# Запустить скрипты
kubectl exec -n tools debug-toolbox -- /scripts/logs-collect.sh
```

