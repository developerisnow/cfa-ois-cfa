# GitOps для OIS-CFA

GitOps конфигурация для автоматического развёртывания инфраструктуры и приложений.

## Варианты

- **ArgoCD** (рекомендуется) — независимый GitOps инструмент
- **GitLab Agent** — нативная интеграция GitLab с Kubernetes

## Документация

Полная документация: [docs/ops/gitops.md](../../docs/ops/gitops.md)

## Быстрый старт

### ArgoCD

```bash
# 1. Установить ArgoCD
make argocd:install

# 2. Получить пароль admin
make argocd:password

# 3. Забутстрапить app-of-apps
make argocd:bootstrap

# 4. Проверить статус
make argocd:status
```

### GitLab Agent

```bash
# 1. Получить токен агента из GitLab UI
# Infrastructure → Kubernetes clusters → Add cluster → GitLab Agent

# 2. Установить агент
export AGENT_TOKEN="your-agent-token"
make gitlab-agent:install

# 3. Проверить статус
make gitlab-agent:status
```

## Структура

```
ops/gitops/
├── argocd/              # ArgoCD конфигурация
│   ├── bootstrap/      # Bootstrap манифесты
│   ├── apps/            # Application definitions
│   │   ├── system/      # System applications
│   │   ├── platform/    # Platform applications
│   │   └── business/    # Business applications
│   ├── config/          # ArgoCD configuration (RBAC, SSO)
│   └── helm/            # Helm values
└── gitlab-agent/        # GitLab Agent конфигурация
    ├── agent-config.yaml
    └── manifests/       # Manifests для синхронизации
        ├── system/
        ├── platform/
        └── business/
```

## Порядок синхронизации

1. **System** — базовая инфраструктура (Ingress, Monitoring)
2. **Platform** — платформенные сервисы (Keycloak, Vault, PostgreSQL)
3. **Business** — бизнес-приложения (API Gateway, Services, Frontend)

