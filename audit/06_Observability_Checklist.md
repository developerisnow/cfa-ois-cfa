# Observability Checklist — SLI/SLO/SLI

Дата: 2025-11-11 (UTC)

SLI/SLO (пример)

- Latency p95 (HTTP/gRPC):
  - SLI: p95 < 300ms (gateway), < 500ms (сервис)
  - SLO: 99% запросов в пределах SLI за 7 дней
- Error rate (5xx):
  - SLI: доля 5xx < 1%
  - SLO: 99% интервалов 5-мин без превышения
- Saturation:
  - CPU > 80% (5 мин) — предупреждение, > 90% — критика
  - Memory > 85% — предупреждение

Алерты (PrometheusRule)

- Высокий error rate (5xx)
- Высокая latency p95
- PodRestarts spikes
- CPU/Memory saturation
- HPA MaxedOut

Трассировка (OpenTelemetry)

- Экспорт OTLP (4317/4318)
- Инструментация .NET: HTTP/gRPC, MassTransit, EF Core
- Корреляция trace_id в логах (Loki/ELK)

Метрики/скрейп

- ServiceMonitor для всех сервисов (путь /metrics)
- PrometheusRule с alert rules
- Grafana dashboards: API Gateway, Сервисы, PostgreSQL/Redis/RabbitMQ

Артефакты

- См. `audit/09_Artifacts/observability/prometheus-rules.sample.yaml`
- См. `audit/09_Artifacts/observability/otel-collector.sample.yaml`
- См. `audit/09_Artifacts/observability/grafana-dashboards-notes.md`

