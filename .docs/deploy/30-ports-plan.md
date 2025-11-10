# Ports Plan (Non‑Default)

Principles
- Avoid standard ports to prevent conflicts
- Prefer binding to `127.0.0.1` when public exposure is not needed
- Centralize in `.env.deployment`

Proposed mapping (override as needed after `bin/check_ports.sh`):
- Gateway: 58080
- Keycloak: 58090
- Registry: 56110
- Issuance: 56120
- Compliance: 56130
- Settlement: 56140
- Identity: 56100
- Fabric‑Gateway (adapter): 56150
- Postgres: 55432 (or internal only)
- Kafka: 59092
- Zookeeper: 58181
- Minio: 59000 (API), 59001 (Console)

Generate `.env.deployment` with these defaults via `bin/gen_env.sh`.
