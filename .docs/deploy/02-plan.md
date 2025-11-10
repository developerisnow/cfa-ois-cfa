# 02 · Deployment Plan

Phases
1) Preflight
   - Capacity check (CPU/RAM/disk)
   - Docker/Compose presence
   - Port scan and selection
2) Infra bring‑up (Compose root)
   - postgres, zookeeper, kafka, keycloak, minio
3) Identity
   - identity service (or Keycloak realm import if applicable)
4) Data services
   - registry, issuance, settlement, compliance
5) API gateway
   - proxy to services, health verification
6) Hardening & Ops
   - backups, metrics endpoints, log routing

Checkpoints
- After each phase run health checks and record outputs in `.docs/deploy/logs/`

Rollout Order (piece‑by‑piece)
- postgres → zookeeper → kafka → keycloak → minio → identity → registry → issuance → settlement → compliance → api‑gateway
