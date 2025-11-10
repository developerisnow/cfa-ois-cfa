# Compose Profiles & Overrides

We keep upstream `docker-compose.yml` intact. We add overrides in `.docs/deploy/compose/` and env in `.env.deployment`.

Run examples
- Infra only: `docker compose -f docker-compose.yml -f .docs/deploy/compose/infra.override.yml --env-file .env.deployment up -d`
- Apps (if containerized): `-f .docs/deploy/compose/apps.override.yml`

Notes
- Non‑default ports come from `.env.deployment`
- If apps are not containerized, use systemd templates in `.docs/deploy/systemd/`
