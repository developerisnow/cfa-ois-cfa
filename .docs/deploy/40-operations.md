# 40 · Operations (Backups, Monitoring, Health)

Backups
- `postgres-backup` sidecar writes to `./backups`
- Test restore quarterly

Health
- All services expose `/health`; gateway `/health`

Metrics
- Add Prometheus scrape (see services that expose `/metrics`)
- Dashboards in Grafana per service

Logs
- Structured logs in containers; consider shipping to ELK or Loki

SLOs (targets)
- API p95 < 300ms, availability 99.9%
