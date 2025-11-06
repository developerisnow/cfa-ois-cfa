# GitLab Kubernetes Agent для OIS-CFA

Настройка GitLab Agent for Kubernetes для GitOps синхронизации.

## Регистрация агента

1. **В GitLab UI:**
   - Infrastructure → Kubernetes clusters
   - Add Kubernetes cluster → GitLab Agent
   - Создать агент: `ois-cfa-agent`

2. **Получить токен регистрации** из GitLab UI

3. **Установить агент:**

```bash
# Создать namespace
kubectl create namespace gitlab-agent

# Установить через Helm
helm repo add gitlab https://charts.gitlab.io
helm install gitlab-agent gitlab/gitlab-agent \
  --namespace gitlab-agent \
  --create-namespace \
  --set config.token=${AGENT_TOKEN} \
  --set config.kasAddress=wss://gitlab.com/-/kubernetes-agent/
```

## Конфигурация

Конфигурация агента должна быть размещена в:
`.gitlab/agents/ois-cfa-agent/config.yaml`

**Важно:** Создайте эту структуру в корне репозитория:

```bash
mkdir -p .gitlab/agents/ois-cfa-agent
cp ops/gitops/gitlab-agent/agent-config.yaml .gitlab/agents/ois-cfa-agent/config.yaml
```

GitLab Agent автоматически обнаружит конфигурацию в этой директории.

## Структура манифестов

```
ops/gitops/gitlab-agent/manifests/
├── system/      # System infrastructure (order: 1)
├── platform/    # Platform services (order: 2)
└── business/    # Business applications (order: 3)
```

## Проверка статуса

```bash
# Проверить статус агента
kubectl get pods -n gitlab-agent

# Логи агента
kubectl logs -n gitlab-agent -l app=gitlab-agent

# В GitLab UI
# Infrastructure → Kubernetes clusters → ваш кластер → Connected agents
```

## Документация

См. [docs/ops/gitops.md](../../../docs/ops/gitops.md) для полной документации.

