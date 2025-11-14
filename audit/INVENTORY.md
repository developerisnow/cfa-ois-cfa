# INVENTORY — Версии и ресурсы

Дата: 2025-11-11 (UTC)

Версии (UNKNOWN = требуется подтверждение)

- Kubernetes: UNKNOWN
- Ingress Class: вероятно nginx (по манифестам), версия UNKNOWN
- ArgoCD: образ использован latest (рекомендовано зафиксировать)
- GitLab Runner: gitlab/gitlab-runner:latest (рекомендовано зафиксировать)
- .NET SDK: 9.0 (по `.gitlab-ci.yml` тестов)

Namespaces (по манифестам в репо)

- ois-cfa (бизнес-сервисы)
- fabric-network (Fabric компоненты)
- argocd (система)
- gitlab-runner (runner)
- tools (debug toolbox)

Deployments/Чарты (по ops/infra/helm и ArgoCD apps)

- api-gateway
- identity
- issuance
- registry
- settlement
- compliance
- bank-nominal
- fabric-peer / fabric-orderer / fabric-ca

CRD (количество): UNKNOWN (см. команды сбора данных)

Ingress/Domains (по значениям в репо)

- api.cfa.capital, api.staging.ois-cfa.example.com, api.dev.ois-cfa.example.com (пример)

Регистр контейнеров

- registry.gitlab.com/ois-cfa/ois-cfa (по values/references)

Команды для заполнения недостающих полей

```bash
kubectl version --short
kubectl get nodes -o wide
kubectl get ns
for ns in dev stage prod ois-cfa fabric-network; do
  kubectl -n $ns get deploy,sts,po,svc,ing,hpa,pdb || true
  kubectl -n $ns get networkpolicy || true
done
kubectl get sc
kubectl get ingressclass
kubectl get crd | wc -l
```

