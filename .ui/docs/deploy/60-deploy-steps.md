# Deploy Steps (WDS)

0) Prep
- `git checkout agents` (remote `alex`)
- `cp .ui/docs/deploy/.env.deployment.example .env.deployment && edit`
- `./.ui/docs/deploy/bin/check_resources.sh` and `./.ui/docs/deploy/bin/check_ports.sh`

1) Infra
- `docker compose -f docker-compose.yml -f .ui/docs/deploy/compose/infra.override.yml --env-file .env.deployment pull`
- `docker compose -f docker-compose.yml -f .ui/docs/deploy/compose/infra.override.yml --env-file .env.deployment up -d`
- Verify: Postgres, Kafka, Zookeeper, Keycloak, Minio

2) Apps
- Option A: run locally (dev): `dotnet run --project services/<svc>` (ports from appsettings)
- Option B: containerized (if enabled):
  - `docker compose -f docker-compose.yml -f .ui/docs/deploy/compose/apps.override.yml --env-file .env.deployment up -d`

3) External deploy pack (single Compose)
- Alternative: use standalone compose project at `/home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed`
- Steps there:
  - `cp /home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed/.env.example .env` (or edit existing)
  - `docker compose -f /home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed/docker-compose.yml build`
  - `docker compose -f /home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed/docker-compose.yml up -d`
  - Adjust absolute repo paths if running on another host

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
