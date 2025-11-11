# Executive Summary — CI/CD и Kubernetes Аудит (ois-cfa)

Дата проверки: 2025-11-11 (UTC)
Область: GitLab CE CI/CD → Kubernetes (timeweb.cloud), GitOps (ArgoCD + GitLab Agent), Helm Charts, Observability, Security, Release Management.

TL;DR

- Состояние: смешанная модель GitOps (ArgoCD для stage/prod, GitLab Agent CRD HelmChart для dev) + Docker-in-Docker (privileged) для сборок.
- Ключевые риски: privileged runner + docker.sock, метка образов latest, неполная сет. сегментация (NetworkPolicy отсутствует в бизнес-чартах), ограниченная политика Pod Security, ручной откат, нестрогое сканирование образов.
- Быстрые победы (≤48ч): убрать DinD → Kaniko; убрать latest; добавить deny-all NetworkPolicy; включить PodSecurity (restricted); включить Trivy с fail-on=HIGH,CRITICAL; включить HPA; закрепить версии ArgoCD/kubectl; добавить шаг rollback.

Границы и допущения

- Версии (UNKNOWN): Kubernetes, IngressClass, ArgoCD, GitLab Runner. В репозитории присутствуют: docker:24-dind, gitlab/gitlab-runner:latest, argoproj/argocd:latest, .NET SDK 9.0. Просим подтвердить точные версии кластера и ingress-класса.
- Секреты: в чартах упоминается sealed-secrets/vault как опции, фактическая система управления секретами не подтверждена (UNKNOWN).
- Наблюдаемость: присутствуют конфиги otel-collector и prometheus.yml, использование в кластере не подтверждено (UNKNOWN).
- Данные кластера и последние пайплайны не предоставлены на момент отчета — выводы по кластеру помечены как предположительные и требуют валидации командами из чек-листа.

Ключевые наблюдения (High-level)

- CI: используется Docker-in-Docker (privileged) в .gitlab-ci.yml для сборки образов, публикация latest для default branch. Отсутствуют Trivy/SAST по умолчанию, нет SBOM.
- CD: ArgoCD для staging/prod с образом latest и grpc-web; dev — GitLab Agent HelmChart CRD с image.tag: latest. Review-приложения частично эмулируются через правила веток, но изоляция/TTL не оформлены как Review Apps.
- K8s: чарты для сервисов содержат probes и ресурсы, но readOnlyRootFilesystem: false по умолчанию, NetworkPolicy только для fabric-компонентов (egress: allow-all), для бизнес-сервисов — отсутствует.
- Security: privileged runner + docker.sock, множество latest-тегов, неописанная система секретов, Pod Security (PSA/PodSecurityPolicy) не зафиксирована в манифестах.
- Observability: есть otel-collector config и prometheus.yml со static_configs; рекомендуется перейти на ServiceMonitor + PrometheusRule, объединить трассировки .NET/MassTransit/EFCore через OTLP.

Цветовая оценка рисков (влияние × вероятность)

- Красный: DinD/privileged runner (docker.sock), latest в проде, отсутствие deny-all NetworkPolicy, отсутствие гарантированного rollback-процесса.
- Оранжевый: PodSecurity недостаточно строгая (readOnlyRootFilesystem=false), egress: allow-all, закрепление версий инструментов (argocd/kubectl), статический скрейп метрик.
- Желтый: Неоформленные Review Apps/TTL, отсутствие SBOM и лицензий, Trivy не в fail mode.

Quick Wins (5–7, ≤48 часов)

1) Сборки Kaniko (замена DinD) — снизить привилегии раннера и убрать docker.sock; добавить кэш registry; теги = SHA.
2) Изъять latest: заменить image.tag в Helm values на $CI_COMMIT_SHA и семвер-теги из релизов; запрет latest через политика.
3) Deny-all NetworkPolicy + точечные allow для ingress/egress сервисов (DB, Redis, RabbitMQ, egress DNS/CRL).
4) PodSecurity: runAsNonRoot=true, readOnlyRootFilesystem=true, drop ALL, seccompProfile: RuntimeDefault; automountServiceAccountToken=false.
5) Trivy: сканирование Dockerfile/образов, fail-on=CRITICAL,HIGH, отчет как artifact; SBOM (syft/trivy) как artifact.
6) HPA включить по CPU и, опционально, кастомной метрике latency p95; настроить requests/limits.
7) Rollback job: argocd app rollback/helm rollback в pipeline; сохранить diff (helm diff) как артефакт.

Предлагаемая целевая схема (Mermaid)

```mermaid
flowchart LR
  A[Commit/MR] --> B[Build Image (Kaniko)]
  B --> C[Test & Scan]
  C --> D[Helm Lint/Package]
  D --> E{Branch?}
  E -- MR --> F[Review App]
  E -- main --> G[Deploy Dev]
  G --> H[Promote Stage]
  H --> I[Promote Prod (Canary/Blue-Green)]
  I --> J[Post-Deploy Checks]
  J -->|Fail| R[Rollback (Helm)]
```

Следующие шаги

- Заполнить UNKNOWN поля командами из чек-листов (k8s версии, ingress-класс, CRD, namespaces, DORA метрики GitLab). Ответить на 3 уточняющих вопроса ниже.
- Утвердить Quick Wins и приступить к внедрению по дорожной карте (4–8 недель).

Уточняющие вопросы (прицельные)

1) Какой реестр контейнеров используется (внутренний GitLab Registry или внешний), есть ли секрет imagePullSecrets в кластере?
2) Какая фактическая стратегия управления секретами: SealedSecrets, ExternalSecrets (ESO) или Vault (и где именно подключено: чарт/оператор)?
3) Подтвердите ingress-класс (nginx/traefik/alb) и домены dev/stage/prod (+ review apps). Есть ли cert-manager?
4) Тип runner: только Kubernetes executor? Можно ли отказаться от privileged/hostPath в пользу Kaniko?

---

Уровень уверенности: высокий по CI-анализу (по .gitlab-ci.yml), средний по K8s (без kubeconfig), средний по Observability/Security (по артефактам в репозитории).

