# GitLab Runner: Image Pull Error Fix

**Дата:** 2025-01-27  
**Проблема:** Ошибка при pull образа `bitnami/kubectl:1.30`

---

## 🔍 ПРОБЛЕМА

### Ошибка:
```
ERROR: Job failed: prepare environment: waiting for pod running: 
pulling image "bitnami/kubectl:1.30" for container build: 
image pull failed: Back-off pulling image "bitnami/kubectl:1.30": 
ErrImagePull: rpc error: code = NotFound desc = 
failed to pull and unpack image "docker.io/bitnami/kubectl:1.30": 
failed to resolve reference "docker.io/bitnami/kubectl:1.30": 
docker.io/bitnami/kubectl:1.30: not found
```

### Причина:
- Образ `bitnami/kubectl:1.30` не существует в Docker Hub
- Версия 1.30 может быть недоступна или тег неправильный

---

## ✅ РЕШЕНИЕ

### Замена на `bitnami/kubectl:latest`

**Файл:** `.gitlab-ci.yml`

**Было:**
```yaml
image: bitnami/kubectl:1.30
```

**Стало:**
```yaml
image: bitnami/kubectl:latest
```

---

## 🔧 АЛЬТЕРНАТИВНЫЕ РЕШЕНИЯ

### Вариант 1: Использовать конкретную версию (если нужна)

Проверить доступные теги:
```bash
curl -s https://hub.docker.com/v2/repositories/bitnami/kubectl/tags/ | jq -r '.results[].name' | head -10
```

Использовать существующий тег, например:
```yaml
image: bitnami/kubectl:1.29
# или
image: bitnami/kubectl:1.28
```

### Вариант 2: Использовать официальный образ kubectl

```yaml
image: bitnami/kubectl:latest
# или
image: alpine/k8s:1.30.0
```

### Вариант 3: Использовать образ с kubectl установленным

```yaml
image: alpine:latest
before_script:
  - apk add --no-cache kubectl
```

---

## 📋 ПРОВЕРКА

### 1. Проверить синтаксис YAML

```bash
python3 -c "import yaml; yaml.safe_load(open('.gitlab-ci.yml'))"
```

### 2. Проверить замену

```bash
grep "bitnami/kubectl" .gitlab-ci.yml
```

Должно быть: `bitnami/kubectl:latest` (или другой существующий тег)

### 3. Запустить job

После замены новый job должен успешно pull образ.

---

## 🚀 ПРИМЕНЕНИЕ

Изменения уже применены в `.gitlab-ci.yml`:
- Все `bitnami/kubectl:1.30` заменены на `bitnami/kubectl:latest`

---

## 📚 ДОПОЛНИТЕЛЬНАЯ ИНФОРМАЦИЯ

### Почему `latest` может быть проблемой:

- `latest` может обновляться и ломать совместимость
- Для production лучше использовать конкретную версию

### Рекомендация для production:

1. Проверить доступные теги
2. Использовать конкретную версию (например, `1.29`)
3. Зафиксировать в `.gitlab-ci.yml`

---

**Статус:** ✅ Исправление применено

