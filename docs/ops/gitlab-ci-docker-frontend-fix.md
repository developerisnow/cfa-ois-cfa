# Исправление: Docker CLI отсутствует в Frontend Build Jobs

**Дата:** 2025-01-27  
**Проблема:** `docker: not found` в frontend build jobs  
**Статус:** ✅ Исправлено

---

## Проблема

### Ошибка

```
/scripts-6-358/step_script: eval: line 187: docker: not found
```

### Причина

Шаблон `.build_frontend_template` использовал образ `node:22-alpine`, который не содержит Docker CLI. При этом в `before_script` и `script` выполнялись команды:
- `docker login`
- `docker buildx create`
- `docker buildx build`

---

## Решение

### Изменения

1. **Образ изменён** с `node:22-alpine` на `docker:24-dind`
2. **Добавлен service** `docker:24-dind` для Docker-in-Docker
3. **Добавлена установка Node.js** в `before_script`:
   ```yaml
   - apk add --no-cache nodejs npm
   ```

### Обновлённый шаблон

```yaml
.build_frontend_template: &build_frontend_template
  stage: build
  image: docker:24-dind
  services:
    - docker:24-dind
  before_script:
    - apk add --no-cache nodejs npm
    - docker login -u $CI_REGISTRY_USER -p $CI_REGISTRY_PASSWORD $CI_REGISTRY
  script:
    - |
      cd $APP_PATH
      npm ci
      npm run build
      docker buildx create --use --name builder || true
      docker buildx build \
        --platform linux/amd64 \
        --push \
        --tag $REGISTRY/$IMAGE_NAME:$IMAGE_TAG \
        --tag $REGISTRY/$IMAGE_NAME:$IMAGE_TAG_LATEST \
        --file Dockerfile \
        .
```

---

## Затронутые Jobs

Все frontend build jobs используют этот шаблон:
- `build:portal-issuer`
- `build:portal-investor`
- `build:backoffice`

---

## Проверка

✅ YAML синтаксис валиден  
✅ Docker CLI доступен в образе `docker:24-dind`  
✅ Node.js устанавливается через `apk`  
✅ Docker-in-Docker service настроен

---

## Альтернативные решения (не использованы)

### Вариант 1: Установка Docker CLI в node:22-alpine

```yaml
before_script:
  - apk add --no-cache docker-cli docker-buildx
```

**Недостаток:** Требует настройки Docker daemon и может быть сложнее.

### Вариант 2: Использование образа с Node.js и Docker

```yaml
image: node:22-alpine
services:
  - docker:24-dind
before_script:
  - apk add --no-cache docker-cli docker-buildx
  - export DOCKER_HOST=tcp://docker:2375
```

**Недостаток:** Дополнительная настройка DOCKER_HOST.

### Выбранное решение

Использование `docker:24-dind` с установкой Node.js — наиболее простое и надёжное решение, согласованное с backend build jobs.

---

## Следующие шаги

1. ✅ Исправление применено
2. ⏳ Запустить pipeline для проверки
3. ⏳ Убедиться, что все frontend build jobs проходят успешно

---

## Ссылки

- [GitLab CI Docker-in-Docker](https://docs.gitlab.com/ee/ci/docker/using_docker_build.html#use-docker-in-docker-executor)
- [Docker Alpine Package Manager](https://pkgs.alpinelinux.org/packages)

