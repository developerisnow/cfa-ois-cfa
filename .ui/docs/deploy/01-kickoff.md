# 01 · Kickoff

Objectives
- Deploy OIS‑CFA on WDS with non‑default ports
- Run infra (Postgres, Kafka/ZK, Keycloak, MinIO) and core services (identity, registry, issuance, settlement, compliance, api‑gateway)
- Ensure observability hooks and backup routines

Scope
- Single host (WDS) Docker Compose deployment
- No data migration from legacy
- TLS/hostname optional (documented), can run behind reverse proxy

Assumptions
- Ubuntu 22.04+ with Docker Engine + Compose plugin permitted
- SSH access with sudo
- Outbound network to pull images

Roles
- DevOps/SRE: lead deployment, operate, document
- Backend: assist with configs and EF migrations
- Security: validate secrets/TLS

Inputs
- Repo: `ois-cfa` (branch `agents`)
- Contracts: `packages/contracts/*`
- Docs: `.ui/reposcan/Runbooks` and this `.ui/docs/deploy`

Risks
- Port conflicts on active server
- Insufficient CPU/RAM/disk
- Kafka/Keycloak start order and health
- Secret handling

Decision Log
- Use env‑driven non‑default host ports
- Phase rollout infra → identity → data services → api‑gateway
