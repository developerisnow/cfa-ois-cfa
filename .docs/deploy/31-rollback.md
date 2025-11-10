# 31 · Rollback

Stop services
```bash
docker compose -f reposcan/Runbooks/docker-compose.services.example.yml down
```

Stop infra (optional)
```bash
docker compose down
```

Restore from backups
- Postgres: restore from `./backups` if using the backup sidecar
- Recreate services after data restore

Port reversion
- Adjust `.env.ports` to previous working set and re‑apply
