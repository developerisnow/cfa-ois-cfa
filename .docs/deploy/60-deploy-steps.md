# Deploy Steps (WDS)

0) Prep
- `git checkout agents` (remote `alex`)
- `cp .docs/deploy/.env.deployment.example .env.deployment && edit`
- `./.docs/deploy/bin/check_resources.sh` and `./.docs/deploy/bin/check_ports.sh`

1) Infra
- `docker compose -f docker-compose.yml -f .docs/deploy/compose/infra.override.yml --env-file .env.deployment pull`
- `docker compose -f docker-compose.yml -f .docs/deploy/compose/infra.override.yml --env-file .env.deployment up -d`
- Verify: Postgres, Kafka, Zookeeper, Keycloak, Minio

2) Apps
- Option A: run locally (dev): `dotnet run --project services/<svc>` (ports from appsettings)
- Option B: containerized (if enabled):
  - `docker compose -f docker-compose.yml -f .docs/deploy/compose/apps.override.yml --env-file .env.deployment up -d`

3) Verify
- Health: `GET /health` for each service
- Gateway `/` redirects to swagger
- Identity OIDC `/.well-known/openid-configuration`
- Issuance `/metrics` (Prometheus)

4) Persist outputs
- Save curls and screenshots to `./_out/verify-*.md`

Rollback
- `docker compose ... down` for added services
- Remove created volumes only if safe
