# Operations Runbook

Lifecycle
- Start/Stop: compose commands per profile
- Status: `docker compose ps`, service‑specific health endpoints
- Logs: `docker compose logs -f <svc>` or systemd `journalctl -u <unit> -f`

Backups
- Postgres: use `ops/scripts/backup.sh` or `pg_dump`
- Minio: `mc` client; periodic snapshotting

Monitoring
- Prometheus scrape: Issuance `/metrics` (add more when available)
- OTel: configure OTLP endpoint via `OTEL_EXPORTER_OTLP_ENDPOINT`

Security
- Keycloak admin password rotation
- Limit public bind; prefer 127.0.0.1 + reverse proxy (nginx) if needed
