# 20 · WDS Deployment Runbook (Docker Compose)

This runbook deploys OIS‑CFA on WDS with non‑default ports.

0) Prepare
- Clone repo and switch branch:
```bash
git clone git@git.telex.global:npk/ois-cfa.git && cd ois-cfa
git remote add alex git@github.com:developerisnow/cfa-ois-cfa.git || true
git fetch alex && git checkout agents
```
- Copy `.ui/docs/deploy/22-env.ports.example` to `.env.ports` and adjust
- Optionally copy `.ui/docs/deploy/21-compose-override.example.yml` to `compose.override.yml`

1) Preflight
```bash
bash .ui/docs/deploy/13-preflight.sh | tee .ui/docs/deploy/logs/preflight-$(date +%F-%H%M).md
```
Resolve any conflicts before proceeding.

2) Infra Bring‑up
```bash
docker network create ois-network || true
set -e
export COMPOSE_PROJECT_NAME=ois
export COMPOSE_FILE=docker-compose.yml:compose.override.yml
set -o pipefail
docker compose --env-file .env.ports up -d postgres zookeeper kafka keycloak minio | tee .ui/docs/deploy/logs/infra-up-$(date +%F-%H%M).md
docker compose ps
```
Health:
- Postgres: healthy
- ZK/Kafka: listening on configured host ports
- Keycloak: http://<host>:${HOST_KEYCLOAK_PORT}
- MinIO: http://<host>:${HOST_MINIO_CONSOLE_PORT}

3) App Services (sequential)
Build/run each, verify, then proceed to next.
```bash
# Identity
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build identity
curl -sf http://localhost:${SERVICE_IDENTITY_PORT}/health || true

# Registry
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build registry
curl -sf http://localhost:${SERVICE_REGISTRY_PORT}/health || true

# Issuance
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build issuance
curl -sf http://localhost:${SERVICE_ISSUANCE_PORT}/health || true

# Settlement
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build settlement
curl -sf http://localhost:${SERVICE_SETTLEMENT_PORT}/health || true

# Compliance
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build compliance
curl -sf http://localhost:${SERVICE_COMPLIANCE_PORT}/health || true
```

4) API Gateway
```bash
docker compose -f .ui/reposcan/Runbooks/docker-compose.services.example.yml --env-file .env.ports up -d --build api-gateway
curl -sf http://localhost:${SERVICE_APIGW_PORT}/health
```

5) Smoke Tests
- Registry order GET: `curl http://localhost:${SERVICE_REGISTRY_PORT}/v1/orders/<uuid>`
- Issuance list: `curl http://localhost:${SERVICE_ISSUANCE_PORT}/v1/issuances`
- Gateway health: `curl http://localhost:${SERVICE_APIGW_PORT}/health`

6) Persist and Document
- Record results under `.ui/docs/deploy/logs/`
- Update `.ui/docs/deploy/CHANGELOG.md`

Notes

## Bootstrap external deploy (eywa1)
Use the script to scaffold a deploy folder outside the repo (no services started):

```bash
bash .ui/docs/deploy/50-bootstrap-deploy.sh \
  /home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed \
  /home/user/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa
# Then inspect .env and run preflight/compose when approved
```

- For TLS and hostnames, place a reverse proxy in front of these ports or configure Keycloak accordingly.
