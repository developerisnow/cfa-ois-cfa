# Findings — Наблюдения и доказательства

Дата проверки: 2025-11-11 (UTC)

Легенда: наблюдение → почему важно → риск → доказательство (файл/фрагмент/вывод)

## CI/CD

| Наблюдение | Почему важно | Риск | Доказательство |
|---|---|---|---|
| Используется Docker-in-Docker (privileged) для сборок | DinD требует привилегий и/или docker.sock, повышает поверхность атаки | Высокий | `.gitlab-ci.yml` использует `docker:24-dind` и сервис `docker:24-dind`; runner настроен privileged и монтирует docker.sock: ops/infra/k8s/gitlab-runner/configmap.yaml |
| Публикуется тег `latest` (default branch и в Helm values) | Немутабельные теги усложняют откаты и воспроизводимость | Высокий | `.gitlab-ci.yml` задаёт `IMAGE_TAG_LATEST: latest` и пушит latest; helm values используют `tag: "latest"`: ops/infra/helm/api-gateway/values.yaml |
| Отсутствует Trivy/SAST/SBOM по умолчанию | Уязвимости и лицензии не контролируются на CI | Средний | В `.gitlab-ci.yml` нет jobs со сканированием образов/кода и SBOM |
| Deploy с образом `argoproj/argocd:latest` | Неприменены pinned версии инструментов CD | Средний | `.gitlab-ci.yml` deploy шаблон использует `argoproj/argocd:latest` |
| Dev deployment через GitLab Agent CRD HelmChart с `image.tag: latest` | Неруководимый дрейф образов в dev | Средний | ops/gitops/gitlab-agent/manifests/business/api-gateway.yaml (valuesContent tag latest) |
| Review Apps оформлены частично (по правилам веток), без TTL/stop | Накопление мусора, расходы | Низкий | `.gitlab-ci.yml` нет стандартного `environment:on_stop` для MR review; dev stop частично есть для feature branch |

## Kubernetes

| Наблюдение | Почему важно | Риск | Доказательство |
|---|---|---|---|
| NetworkPolicy отсутствует для бизнес-сервисов | Zero-trust по сети по умолчанию | Высокий | В чарте api-gateway нет NetworkPolicy; только для fabric-* существуют (egress allow-all): ops/infra/helm/fabric-*/templates/networkpolicy.yaml |
| readOnlyRootFilesystem=false по умолчанию | Сниженная защита контейнера | Средний | ops/infra/helm/api-gateway/values.yaml: securityContext.readOnlyRootFilesystem: false |
| Пробы и ресурсы заданы (частично) | Хорошая практика, но требуется унификация | Низкий | deployment шаблоны и values содержат liveness/readiness, requests/limits |
| Откат Helm/ArgoCD не оформлен как процедура | MTTR ↑ при сбое | Средний | В `.gitlab-ci.yml` нет rollback job; ArgoCD sync без rollback |

## Секреты и комплаенс

| Наблюдение | Почему важно | Риск | Доказательство |
|---|---|---|---|
| Система секретов не подтверждена | Риск утечки/ручной drift | Средний | В values указаны варианты (sealed-secrets/vault), но нет фактических манифестов SealedSecret/ExternalSecret (поиск пуст) |
| CronJob rotation с образом `vault:latest` | latest в security-чувствительном процессе | Средний | ops/infra/helm/fabric-peer/templates/secrets-rotation.yaml |

## Наблюдаемость

| Наблюдение | Почему важно | Риск | Доказательство |
|---|---|---|---|
| otel-collector присутствует, но экспорт только logging/prometheus | Нет полноценного экспорта в трассировочную систему | Низкий | ops/infra/otel-collector-config.yaml |
| prometheus.yml со static_configs | Хрупко; лучше ServiceMonitor | Средний | ops/infra/prometheus.yml |

## Производительность, ёмкость, стоимость

| Наблюдение | Почему важно | Риск | Доказательство |
|---|---|---|---|
| Requests/limits заданы не везде и разнятся | Неоптимальные затраты и SLO | Средний | values.*.yaml в чартах различаются, не везде включён autoscaling |

## DORA метрики (предварительно)

- Частота релизов, Lead Time, MTTR, Change Failure Rate — UNKNOWN (нет логов последних 10 пайплайнов). Рекомендуется выгрузка GitLab metrics и расчёт по pipeline events.

Примечания к доказательствам (файлы в репо)

- .gitlab-ci.yml (корень): стадии build/test/deploy с docker:dind; deploy через ArgoCD/Agent.
- ops/infra/k8s/gitlab-runner/configmap.yaml: privileged=true, docker.sock hostPath.
- ops/gitops/gitlab-agent/manifests/business/api-gateway.yaml: HelmChart с image.tag: latest.
- ops/infra/helm/api-gateway/values.yaml: tag: "latest", readOnlyRootFilesystem: false.
- ops/infra/otel-collector-config.yaml, ops/infra/prometheus.yml: базовые конфиги observability.

