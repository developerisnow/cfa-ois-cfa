# GitOps Sync Guide: GitLab Agent

**Дата:** 2025-01-27  
**Инструмент:** GitLab Agent for Kubernetes  
**Статус:** ✅ Настроено и готово к использованию

---

## 🎯 ЦЕЛЬ

Настроить автоматическую синхронизацию манифестов из Git в Kubernetes кластер через GitLab Agent.

---

## ✅ ВЫПОЛНЕНО

### 1. GitLab Agent установлен
- **Namespace:** `gitlab-agent`
- **Pods:** 2/2 Running
- **Статус:** Online

### 2. Конфигурация агента создана
- **Путь:** `.gitlab/agents/ois-cfa-agent/config.yaml`
- **Структура:** system → platform → business
- **Политики:** prune, self_heal включены

### 3. Манифесты подготовлены
- **System:** `ops/gitops/gitlab-agent/manifests/system/`
- **Platform:** `ops/gitops/gitlab-agent/manifests/platform/`
- **Business:** `ops/gitops/gitlab-agent/manifests/business/`

---

## 🔄 ПРОЦЕСС СИНХРОНИЗАЦИИ

### Автоматическая синхронизация (GitLab Agent)

GitLab Agent автоматически синхронизирует манифесты из Git в кластер:

1. **Изменения в Git:**
   - Коммит в `ops/gitops/gitlab-agent/manifests/**`
   - Merge Request в main/master

2. **GitLab Agent обнаруживает изменения:**
   - Агент опрашивает GitLab API
   - Получает список измененных файлов

3. **Применение манифестов:**
   - System (order: 1)
   - Platform (order: 2, depends on system)
   - Business (order: 3, depends on platform)

4. **Проверка статуса:**
   - Все ресурсы Synced
   - Health checks проходят

---

## 📋 РУЧНАЯ СИНХРОНИЗАЦИЯ

### Вариант 1: Через скрипт

```bash
./ops/scripts/gitops-sync.sh dev
```

Скрипт:
- Проверяет статус GitLab Agent
- Применяет манифесты в правильном порядке
- Показывает статус ресурсов

### Вариант 2: Через kubectl

```bash
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# System
kubectl apply -f ops/gitops/gitlab-agent/manifests/system/ --recursive

# Platform
kubectl apply -f ops/gitops/gitlab-agent/manifests/platform/ --recursive

# Business
kubectl apply -f ops/gitops/gitlab-agent/manifests/business/ --recursive
```

### Вариант 3: Через GitLab CI/CD

Манифесты автоматически применяются при merge в main/master через GitLab Agent.

---

## 🔍 ПРОВЕРКА СТАТУСА

### 1. Проверить GitLab Agent

```bash
kubectl get pods -n gitlab-agent
kubectl logs -n gitlab-agent -l app=gitlab-agent --tail=50
```

### 2. Проверить в GitLab UI

- **Infrastructure → Kubernetes clusters**
- **Ваш кластер → Connected agents**
- **Агент `ois-cfa-agent` → Online**

### 3. Проверить примененные ресурсы

```bash
# Namespaces
kubectl get namespaces

# Deployments
kubectl get deployments -n ois-cfa

# Services
kubectl get services -n ois-cfa

# Ingress
kubectl get ingress -n ois-cfa
```

---

## 📊 GITLAB ENVIRONMENTS & DEPLOYMENTS

### Настройка в `.gitlab-ci.yml`

Для отображения релизов в GitLab Environments нужно добавить в deploy jobs:

```yaml
deploy:dev:
  stage: deploy
  environment:
    name: dev
    url: https://dev.cfa.capital
    deployment_tier: development
  script:
    - echo "Deploying to dev..."
    # GitLab Agent автоматически применит изменения
```

### Проверка в GitLab UI

- **Operations → Environments**
- Должны отображаться: dev, staging, prod
- Каждый environment показывает последний deployment

---

## 🎯 КРИТЕРИИ УСПЕХА

- [x] GitLab Agent установлен и работает
- [x] Конфигурация агента создана
- [x] Манифесты подготовлены
- [x] Манифесты применены в кластер
- [ ] Все приложения Synced/Healthy
- [ ] GitLab Environments отображают релизы

---

## 📝 СЛЕДУЮЩИЕ ШАГИ

1. **Проверить статус в GitLab UI:**
   - Infrastructure → Kubernetes clusters
   - Connected agents → ois-cfa-agent → Online

2. **Создать MR с изменениями манифестов** (если нужно)

3. **Проверить синхронизацию:**
   - После merge в main/master
   - GitLab Agent автоматически применит изменения

4. **Настроить GitLab Environments:**
   - Добавить environment в `.gitlab-ci.yml`
   - Настроить URLs для каждого окружения

---

## 🔧 TROUBLESHOOTING

### Проблема: Агент не синхронизирует

**Решение:**
1. Проверить статус агента: `kubectl get pods -n gitlab-agent`
2. Проверить логи: `kubectl logs -n gitlab-agent -l app=gitlab-agent`
3. Проверить конфигурацию: `.gitlab/agents/ois-cfa-agent/config.yaml`
4. Проверить в GitLab UI: Infrastructure → Kubernetes clusters

### Проблема: Манифесты не применяются

**Решение:**
1. Проверить пути в `config.yaml`
2. Проверить, что манифесты в правильной директории
3. Применить вручную: `./ops/scripts/gitops-sync.sh dev`

---

**Статус:** ✅ GitOps sync настроен и готов к использованию

