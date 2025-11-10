# 11 · Ports Strategy (Non‑default)

Principles
- Avoid common defaults to reduce collisions on active WDS
- Keep contiguous ranges for clarity
- All mappings configurable via env file

Recommended host ports (override as needed):
- Postgres: 55432
- Zookeeper: 52181
- Kafka: 59092
- Keycloak: 58080
- MinIO API: 59000
- MinIO Console: 59001
- Identity: 6504
- Issuance: 6505
- Registry: 6506
- Settlement: 6507
- Compliance: 6508
- API Gateway: 6580

Validation
- Use `ss -ltnp | grep :<port>` to ensure port is free
- Preflight script checks all configured ports

Change control
- Update `.env.ports` and re‑create containers with `docker compose up -d`
