# Roadmap — Приоритизированный план (4–8 недель)

Дата: 2025-11-11 (UTC)

MoSCoW

- Must: убрать DinD (Kaniko), убрать latest, ввести NetworkPolicy deny-all, включить PodSecurity (restricted), добавить Trivy+SBOM, добавить rollback job.
- Should: включить HPA, закрепить версии инструментов, Review Apps с TTL, ServiceMonitor/PrometheusRule, автоматизация promote.
- Could: Canary/Blue-Green, VPA, Cluster Autoscaler, SLO дашборды и алерты p95.
- Won’t (сейчас): сервис-меш, полная Zero Trust PKI — позже.

Impact/Effort (кратко)

- Kaniko вместо DinD — Высокий/Средний
- Удалить latest — Высокий/Низкий
- NetworkPolicy deny-all — Высокий/Средний
- PodSecurity restricted — Высокий/Низкий
- Trivy+SBOM — Средний/Низкий
- Rollback job — Средний/Низкий
- HPA — Средний/Низкий
- Pin инструментов — Средний/Низкий

План по неделям

Недели 1–2
- CI: внедрить Kaniko, убрать privileged/docker.sock в runner профиле.
- CI: ввести теги образов = $CI_COMMIT_SHA, убрать latest из Helm values.
- Security: PSA (restricted), securityContext ужесточить, automountServiceAccountToken=false в чартах.
- Scan: добавить Trivy+SBOM в pipeline.

Недели 3–4
- K8s: NetworkPolicy (deny-all + allow), rollout по namespace `ois-cfa`.
- CD: rollback job (ArgoCD/Helm), pin versions argocd/kubectl.
- Perf: включить HPA для ключевых сервисов (api-gateway, registry, issuance, settlement).

Недели 5–6
- Observability: ServiceMonitor/PrometheusRule, базовые алерты (5xx, latency p95, saturation), OTLP трассировки (.NET, MassTransit, EF Core).
- Environments: Review Apps (TTL 24–48ч), auto-clean.

Недели 7–8
- Release strategies: Canary для прод-выкатки api-gateway.
- Capacity: ревизия requests/limits, правайтсайзинг, оценка стоимости и автоскейлинг кластера.

Контрольные точки

- DoD каждой фазы: тестовые отчёты/артефакты, дифф чарта, успешный откат, графики метрик и алертов.

