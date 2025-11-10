# 03 · Definition of Done

Infra
- All infra containers healthy
- Non‑default ports selected and documented

Services
- Identity, Registry, Issuance, Settlement, Compliance reachable on host ports
- EF migrations applied without errors

Gateway
- `/health` returns 200
- Routing to services verified

Ops
- Backup directory mounted and writeable
- Metrics endpoints exposed (where available)

Docs
- Final runbook validated and updated
- Troubleshooting and rollback sections completed
