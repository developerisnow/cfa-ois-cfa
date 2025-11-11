# Security Checklist

Дата: 2025-11-11 (UTC)

Контейнеры/Образы

- Trivy: сканирование Dockerfile и образов; HIGH/CRITICAL → fail
- SBOM (CycloneDX) — артефакт пайплайна
- Запрет latest в проде; pinned версии инструментов

Kubernetes

- PSA: restricted (namespace)
- Pod securityContext: runAsNonRoot, readOnlyRootFilesystem=true, drop ALL, seccompProfile: RuntimeDefault
- NetworkPolicy: deny-all, DNS egress, allow только нужные ingress/egress
- ServiceAccount: automountServiceAccountToken=false; минимально необходимые роли (RBAC)

Секреты

- Управление секретами: SealedSecrets/ExternalSecrets/Vault (без plain secrets)
- Ротация и аудит секретов; исключить секреты из env/plain

CI/CD

- Без privileged/DinD; runner без hostPath docker.sock
- Контроль окружения: protect переменных, маскирование, ограничение на protected branches/tags
- Rollback процедура: задокументирована и проверена

Наблюдаемость/Логи

- Корреляция trace_id в логах
- Алерты по 5xx/latency/usage с runbooks

