# Быстрый старт: Выкатка в Production

**Версия:** 1.0  
**Дата:** 2025-01-27

---

## 🎯 Цель

Выкатить тестовый pod в production с доступом по домену `cfa.capital`.

---

## ✅ ТЕКУЩИЙ СТАТУС

- ✅ **Тестовый pod выкачен и работает**
- ✅ **Ingress настроен для домена `cfa.capital`**
- ✅ **Доступ по IP работает:** `http://217.25.93.83 -H 'Host: cfa.capital'`
- ⚠️ **GitLab Runner требует обновления токена** (блокер для CI/CD)

---

## 🚀 БЫСТРЫЙ СТАРТ

### Шаг 1: Исправить GitLab Runner (КРИТИЧНО)

**Проблема:** Runner получает 403 Forbidden, jobs не запускаются

**Решение:**

1. **Получить Runner Registration Token:**
   - Открыть: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
   - Раздел: Runners
   - Если токен отозван → "Reset registration token"
   - Скопировать новый токен

2. **Обновить токен:**
   ```bash
   export RUNNER_TOKEN="новый-токен-из-gitlab"
   make gitlab-runner-update-token
   kubectl delete pods -n gitlab-runner -l app=gitlab-runner
   ```

3. **Проверить:**
   ```bash
   make gitlab-runner-status
   # В GitLab UI: Settings → CI/CD → Runners → должен быть "Online"
   ```

**Или использовать мастер-скрипт:**
```bash
export RUNNER_TOKEN="токен-из-gitlab"
./ops/scripts/fix-runner-and-deploy.sh
```

---

### Шаг 2: Проверить тестовый pod

```bash
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"

# Проверить статус
kubectl get pods -n ois-cfa
kubectl get svc -n ois-cfa
kubectl get ingress -n ois-cfa

# Проверить доступ
curl -H "Host: cfa.capital" "http://217.25.93.83"
```

**Ожидаемый результат:** nginx welcome page

---

### Шаг 3: Настроить DNS (опционально)

Если DNS не настроен, используйте IP напрямую:

```bash
# Временный доступ по IP
curl -H "Host: cfa.capital" "http://217.25.93.83"
```

**Для постоянного доступа:**
- Настроить A-запись: `cfa.capital` → `217.25.93.83`
- После настройки DNS: `http://cfa.capital` будет работать напрямую

---

## 📋 СЛЕДУЮЩИЕ ШАГИ

### 1. Настроить GitOps (GitLab Agent)

```bash
# Создать конфигурацию агента
mkdir -p .gitlab/agents/ois-cfa-agent
cp ops/gitops/gitlab-agent/agent-config.yaml .gitlab/agents/ois-cfa-agent/config.yaml

# Проверить статус агента
kubectl get pods -n gitlab-agent
kubectl logs -n gitlab-agent -l app=gitlab-agent
```

### 2. Выкатить API Gateway

```bash
# Обновить values-prod.yaml (уже обновлён с доменом api.cfa.capital)
helm install api-gateway ops/infra/helm/api-gateway \
  --namespace ois-cfa \
  --create-namespace \
  -f ops/infra/helm/api-gateway/values-prod.yaml

# Проверить
kubectl get pods -n ois-cfa
kubectl get ingress -n ois-cfa
```

### 3. Настроить CI/CD

После исправления Runner:
- Jobs будут автоматически запускаться
- Build jobs соберут образы
- Deploy jobs выкатят через GitOps

---

## 🔧 ПОЛЕЗНЫЕ КОМАНДЫ

```bash
# Проверить кластер
kubectl get nodes
kubectl get namespaces
kubectl get pods -A

# Проверить тестовый pod
kubectl get pods -n ois-cfa
kubectl logs -n ois-cfa -l app=test-nginx

# Проверить Ingress
kubectl get ingress -n ois-cfa
kubectl describe ingress -n ois-cfa test-nginx

# Проверить Runner
make gitlab-runner-status
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50

# Проверить GitLab Agent
kubectl get pods -n gitlab-agent
kubectl logs -n gitlab-agent -l app=gitlab-agent --tail=50
```

---

## 📊 ТЕКУЩЕЕ СОСТОЯНИЕ

```
✅ Namespace: ois-cfa
✅ Deployment: test-nginx (1/1 Ready)
✅ Service: test-nginx (ClusterIP)
✅ Ingress: test-nginx (cfa.capital)
✅ Node IP: 217.25.93.83
✅ Доступ работает: curl -H "Host: cfa.capital" "http://217.25.93.83"

⚠️ GitLab Runner: требует обновления токена
✅ GitLab Agent: работает (2/2 Running)
✅ Ingress NGINX: работает
```

---

## 🎯 КРИТЕРИИ УСПЕХА

- [x] Тестовый pod выкачен
- [x] Ingress настроен
- [x] Доступ по IP работает
- [ ] GitLab Runner работает (требует токен)
- [ ] DNS настроен (опционально)
- [ ] API Gateway выкачен
- [ ] CI/CD pipeline работает

---

## 📚 ДОПОЛНИТЕЛЬНАЯ ДОКУМЕНТАЦИЯ

- [Мастер-план выкатки](./production-deployment-master-plan.md)
- [Статус выкатки](./production-deployment-status.md)
- [GitLab Runner Troubleshooting](./gitlab-runner-troubleshooting.md)
- [GitOps Setup](./gitops.md)

---

**Следующий шаг:** Исправить GitLab Runner токен для запуска CI/CD jobs

