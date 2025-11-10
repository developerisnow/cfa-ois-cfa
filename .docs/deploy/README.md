# OIS‑CFA Deployment · WDS Runbook

This deploy pack documents how we deploy the ois‑cfa ecosystem to WDS (active server). All changes are committed iteratively on branch `agents` and pushed only to remote `alex`.

- Scope: Postgres, Kafka/Zookeeper, Keycloak, Minio, .NET services (registry, compliance, issuance, settlement, identity), API Gateway, Fabric (future).
- Non‑default ports: avoids conflicts on busy server. See Ports Plan.
- Safety: preflight checks for resources and port conflicts; reversible steps; clear rollback.

Quick nav:
- Kickoff: `00-kickoff.md`
- Inventory & Preflight: `20-inventory.md`, `40-pre-flight-checks.md`
- Ports: `30-ports-plan.md`
- Compose profiles: `50-compose-profiles.md`
- Deploy steps: `60-deploy-steps.md`
- Ops runbook: `70-operations-runbook.md`
- Troubleshooting: `80-troubleshooting.md`
- DoD: `90-DoD.md`

Artifacts generated elsewhere:
- Reposcan: `reposcan/README.md` with snapshot, C4 and ER diagrams for system context.
