# OIS‑CFA Deployment (VPS) · Single Runbook

This document replaces prior split files. It covers: kickoff, inventory/preflight, ports, compose usage, operations, troubleshooting, and DoD.

## Kickoff & Scope
- Stack: PostgreSQL, Kafka/ZK, Keycloak, Minio, .NET services (identity, registry, issuance, compliance, settlement), API Gateway. Fabric mocked for now.
- Non‑default ports; bind to 127.0.0.1 by default. See `.ai/deploy/.ai/deploy/.env.deployment.example`.
- Change control: iterative commits to `agents`; tag archives as needed (e.g., `zip/agents-md`).

## Inventory & Preflight

## Scripts
- .ai/deploy/scripts/check_resources.sh
- .ai/deploy/scripts/check_ports.sh
- .ai/deploy/scripts/gen_env.sh
- .ai/deploy/scripts/bootstrap_deploy.sh
- .ai/deploy/scripts/preflight.sh
- Run on eywa1 to snapshot resources and ports (manual quick checks):
  - `uname -a && lsb_release -a` — OS
  - `lscpu && free -h && df -hT` — CPU/Mem/Disk
  - `docker --version && docker compose version` — toolchain
  - `ss -tulpen | grep -E ":(58080|58090|561..|55432|59092|58181|5900[01])\b" || true` — conflicts
- Saved examples: `.ai/docs/logs/resources.md`, `.ai/docs/logs/ports.md`.

## Ports Plan (defaults)
Gateway 58080, Keycloak 58090, Identity 56100, Registry 56110, Issuance 56120, Compliance 56130, Settlement 56140, Postgres 55432, Kafka 59092, ZK 58181, Minio 59000/59001.

## Environment
- Copy and edit: `cp .ai/deploy/.ai/deploy/.env.deployment.example .ai/deploy/.ai/deploy/.env.deployment`
- Use with compose via `--env-file .ai/deploy/.ai/deploy/.env.deployment`.

## Deploy with Unified Compose (in-repo)
- Build: `docker compose -f .ai/deploy/docker/docker-compose.yml --env-file .ai/deploy/.ai/deploy/.env.deployment build`
- Up:    `docker compose -f .ai/deploy/docker/docker-compose.yml --env-file .ai/deploy/.ai/deploy/.env.deployment up -d`
- Logs:  `docker compose -f .ai/deploy/docker/docker-compose.yml logs -f <service>`

## Verify
- Each service: `GET /health` (ports from env)
- Gateway `/` → Swagger; Identity `/.well-known/openid-configuration`; Issuance `/metrics`

## Operations
- Status: `docker compose ... ps`; Logs: `docker compose ... logs -f <service>`
- Backups: Postgres via `pg_dump`; Minio via `mc` client
- Monitoring: Prometheus scrape at Issuance `/metrics`; OTel via `OTEL_EXPORTER_OTLP_ENDPOINT`

## Troubleshooting
- Port conflicts → adjust `.ai/deploy/.ai/deploy/.env.deployment` and re‑up
- DB migration failures → check connection string and privileges
- Kafka/ZK ordering → ensure ZK ready before Kafka
- Keycloak startup → DB readiness and admin credentials

## DoD
- Infra up and healthy on planned ports
- All services healthy; smoke checks pass
- Docs + compose committed in `agents`
