# Runbooks — Инструкции

Дата: 2025-11-11 (UTC)

1) Rollback Helm/ArgoCD

Helm (если релиз управляется Helm напрямую):
```bash
helm history <release> -n <ns>
helm rollback <release> <REVISION> -n <ns>
```

ArgoCD (GitOps-подход):
```bash
argocd login $ARGOCD_SERVER --username $ARGOCD_USER --password $ARGOCD_PASSWORD --grpc-web --insecure
argocd app history ois-prod
argocd app rollback ois-prod --revision <REVISION> --grpc-web
```

Проверка после отката:
```bash
kubectl -n <ns> get deploy,po
kubectl -n <ns> rollout status deploy/<name>
```

2) Инцидент 5xx (повышенная ошибка)

- Проверить NGINX Ingress/ALB ошибки и backend ошибки.
- Сравнить релиз/diff (helm diff/argocd app diff).
- Проверить метрики p95/CPU/Mem, рестарты pod.
- При необходимости: scale down canary, откат.

3) Утечка секрета

- Немедленно ротация секретов (ESO/Vault/SealedSecrets), отозвать ключи.
- Пересоздать поды/деплойменты.
- Проверить логи доступов.

4) Degraded latency

- Проверить HPA срабатывания, узлы (pressure), кэш/БД.
- Временное масштабирование, анализ трассировки (OTLP спаны .NET/MassTransit/EF Core).

Команды валидации/отката

```bash
# Валидация после деплоя
kubectl -n <ns> get deploy,po,svc,ingress
kubectl -n <ns> describe deploy <name>

# Дифф релиза Helm
helm diff upgrade <release> <chart> -n <ns> -f values.yaml

# Быстрый откат ArgoCD
argocd app rollback <app> --revision <REV>
```

