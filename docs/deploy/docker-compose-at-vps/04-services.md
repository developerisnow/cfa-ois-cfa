---
created: 2025-11-11 15:22
updated: 2025-11-11 15:22
type: runbook
sphere: [devops]
topic: [services, dotnet]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [dotnet, compose]
---

# 04 — .NET‑сервисы (поэтапный запуск)

Общие правила
- [ ] На малых VPS собирать по одному сервису (RAM 2 ГБ)
- [ ] Миграции БД — через флаг `MIGRATE_ON_STARTUP=true` (по умолчанию не применяются)
- [ ] Проверка готовности: `/health` на соответствующем порту

Identity Service
- [ ] ```bash
  cd /opt/ois-cfa
  C="-f docker-compose.yml -f docker-compose.override.yml -f docker-compose.kafka.override.yml -f docker-compose.services.yml"
  docker compose $C build identity-service && docker compose $C up -d identity-service
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55001/health
  ```

Registry Service
- [ ] ```bash
  docker compose $C build --no-cache registry-service
  MIGRATE_ON_STARTUP=false docker compose $C up -d registry-service
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55006/health
  ```

Issuance Service (dev‑правки учтены)
- [ ] Примечание: в dev отключены Prometheus‑экспортер и scraping endpoint, авто‑валидация временно выключена
- [ ] ```bash
  docker compose $C build --no-cache issuance-service
  MIGRATE_ON_STARTUP=false docker compose $C up -d issuance-service
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55005/health
  ```

Settlement Service
- [ ] ```bash
  docker compose $C build settlement-service
  MIGRATE_ON_STARTUP=false docker compose $C up -d settlement-service
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55007/health
  ```

Compliance Service
- [ ] ```bash
  docker compose $C build compliance-service
  MIGRATE_ON_STARTUP=false docker compose $C up -d compliance-service
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55008/health
  ```

Логи и статус
- [ ] `docker compose $C ps`
- [ ] `docker logs -f <service>`

