# 30 · Troubleshooting

Port conflicts
- Run preflight; change conflicting values in `.env.ports`

Docker permissions
- Ensure user is in docker group; relogin or `newgrp docker`

Images slow to pull
- Use local registry mirror / pre‑pull images

Container crash loops
- `docker compose logs -f <service>` and verify env vars

EF migrations
- If migration fails, ensure DB reachable and credentials consistent

Kafka not reachable
- Confirm ZK is up and Kafka listener uses container port 9092 with advertised listener set to host mapping

Keycloak DB errors
- Keycloak uses Postgres DSN; verify `KC_DB_URL` points to Postgres service and DB user exists
