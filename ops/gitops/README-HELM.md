# Helm Charts для GitOps

## Создание новых Charts

Используйте скрипт для создания нового chart на основе шаблона:

```bash
./ops/scripts/create-helm-chart.sh <chart-name>
```

Пример:
```bash
./ops/scripts/create-helm-chart.sh identity
./ops/scripts/create-helm-chart.sh portal-issuer
```

## Структура Chart

Каждый chart должен содержать:
- `Chart.yaml` — метаданные
- `values.yaml` — значения по умолчанию
- `values-dev.yaml` — для dev окружения
- `values-staging.yaml` — для staging
- `values-prod.yaml` — для production
- `templates/` — Kubernetes манифесты

## Интеграция с GitOps

### ArgoCD

Добавьте Application в `ops/gitops/argocd/apps/business/`:

```yaml
apiVersion: argoproj.io/v1alpha1
kind: Application
metadata:
  name: <service-name>
  namespace: argocd
spec:
  project: business
  source:
    repoURL: https://gitlab.com/ois-cfa/ois-cfa.git
    targetRevision: main
    path: ops/infra/helm/<service-name>
    helm:
      valueFiles:
        - values.yaml
        - values-${ARGOCD_ENV}.yaml
  destination:
    server: https://kubernetes.default.svc
    namespace: ois-cfa
```

### GitLab Agent

Добавьте HelmChart в `ops/gitops/gitlab-agent/manifests/business/`:

```yaml
apiVersion: helm.cattle.io/v1
kind: HelmChart
metadata:
  name: <service-name>
  namespace: ois-cfa
spec:
  chart: <service-name>
  repo: https://gitlab.com/ois-cfa/ois-cfa.git
  targetNamespace: ois-cfa
```

## Проверка

```bash
# Локальная проверка
helm template <service-name> ops/infra/helm/<service-name> -f ops/infra/helm/<service-name>/values-dev.yaml

# Установка в кластер
helm install <service-name> ops/infra/helm/<service-name> \
  --namespace ois-cfa \
  -f ops/infra/helm/<service-name>/values-dev.yaml
```

