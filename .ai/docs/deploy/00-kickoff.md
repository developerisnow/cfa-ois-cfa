# Kickoff · Plan, Deliverables, DoD

Goals
- Stand up infra services and .NET apps on WDS with non‑default ports.
- Provide reproducible scripts, configs, and runbooks.

Deliverables
- Preflight outputs: hardware inventory, open ports snapshot
- Configs: `.env.deployment`, compose override(s), systemd templates (optional)
- Deployment logs in `./_out/` and markdown backups of each phase
- Verified health endpoints and smoke checks

Definition of Done (DoD)
- Postgres, Kafka/Zookeeper, Keycloak, Minio running and reachable on planned ports
- Services up: registry, issuance, compliance, settlement, identity, api‑gateway
- Health: each `/health` returns 200; Identity OIDC discovery responds; Gateway `/` → swagger
- Metrics: issuance exposes `/metrics`
- Repos updated: docs and overrides committed on `agents` to `alex`

Milestones
1) Preflight & Ports (inventory, conflicts, `.env.deployment`)
2) Infra up via Compose (DB, Kafka, Keycloak, Minio)
3) Apps up (api‑gateway, registry, issuance, compliance, settlement, identity)
4) Verification & smoke tests
5) Post‑deploy runbook and monitoring

Change control
- Iterative commits per sub‑task on `agents` (remote: alex)
- If a step fails, record details in `./_out/phase-*.md` and propose remediation
