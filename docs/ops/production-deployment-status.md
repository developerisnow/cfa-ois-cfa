# Статус выкатки в Production

**Дата:** 2025-01-27  
**Статус:** ✅ Тестовый pod выкачен и работает

---

## ✅ ВЫПОЛНЕНО

### 1. Тестовый pod выкачен
- **Namespace:** `ois-cfa`
- **Deployment:** `test-nginx`
- **Service:** `test-nginx` (ClusterIP)
- **Ingress:** `test-nginx` для домена `cfa.capital`
- **Статус:** ✅ Running (1/1 Ready)

### 2. Доступ работает
- **Node IP:** `217.25.93.83`
- **Доступ через Ingress:** `http://cfa.capital` (если DNS настроен)
- **Доступ по IP:** `http://217.25.93.83 -H 'Host: cfa.capital'`
- **Проверка:** ✅ curl возвращает nginx welcome page

### 3. Инфраструктура
- ✅ Kubernetes кластер работает (1 worker node)
- ✅ Ingress NGINX установлен и работает
- ✅ GitLab Agent установлен (2 pod'а Running)
- ⚠️ GitLab Runner требует обновления токена

---

## ⚠️ ТРЕБУЕТ ВНИМАНИЯ

### GitLab Runner (БЛОКЕР для CI/CD)

**Проблема:** Runner получает 403 Forbidden, jobs не запускаются

**Решение:**

1. **Получить новый Runner Registration Token:**
   - Открыть: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd
   - Раздел: Runners
   - Если токен отозван → "Reset registration token"
   - Скопировать новый токен

2. **Обновить токен в кластере:**
   ```bash
   export RUNNER_TOKEN="новый-токен-из-gitlab"
   make gitlab-runner-update-token
   kubectl delete pods -n gitlab-runner -l app=gitlab-runner
   ```

3. **Проверить статус:**
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

## 📋 СЛЕДУЮЩИЕ ШАГИ

### Фаза 1: Исправить Runner (КРИТИЧНО)
- [ ] Получить Runner Registration Token из GitLab UI
- [ ] Обновить токен в кластере
- [ ] Проверить, что Runner "Online" в GitLab UI
- [ ] Запустить тестовый job в GitLab CI

### Фаза 2: Настроить GitOps
- [ ] Создать `.gitlab/agents/ois-cfa-agent/config.yaml`
- [ ] Настроить пути к манифестам
- [ ] Проверить синхронизацию через GitLab Agent

### Фаза 3: Выкатить API Gateway
- [ ] Обновить `values-prod.yaml` с правильным доменом
- [ ] Выкатить через Helm или GitOps
- [ ] Проверить доступ по `https://api.cfa.capital`

### Фаза 4: Настроить DNS
- [ ] Настроить A-запись для `cfa.capital` → `217.25.93.83`
- [ ] Настроить A-запись для `api.cfa.capital` → `217.25.93.83`
- [ ] Проверить доступ по домену

---

## 🔧 КОМАНДЫ ДЛЯ ПРОВЕРКИ

```bash
# Проверить тестовый pod
export KUBECONFIG="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
kubectl get pods -n ois-cfa
kubectl get svc -n ois-cfa
kubectl get ingress -n ois-cfa

# Проверить доступ
NODE_IP="217.25.93.83"
curl -H "Host: cfa.capital" "http://${NODE_IP}"

# Проверить Runner
make gitlab-runner-status

# Проверить GitLab Agent
kubectl get pods -n gitlab-agent
```

---

## 📊 ТЕКУЩЕЕ СОСТОЯНИЕ КЛАСТЕРА

```
Namespaces:
- ois-cfa (тестовый pod)
- gitlab-agent (GitLab Agent)
- gitlab-runner (GitLab Runner - требует исправления)
- ingress-nginx (Ingress Controller)

Pods:
- test-nginx: Running (1/1)
- gitlab-agent: Running (2/2)
- gitlab-runner: Running, но получает 403 (требует токен)
```

---

## 🎯 КРИТЕРИИ УСПЕХА

- [x] Тестовый pod выкачен и работает
- [x] Ingress настроен для домена
- [x] Доступ по IP работает
- [ ] GitLab Runner работает (требует токен)
- [ ] DNS настроен (опционально)
- [ ] API Gateway выкачен
- [ ] CI/CD pipeline работает

---

**Следующий шаг:** Исправить GitLab Runner токен для запуска CI/CD jobs

