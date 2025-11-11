---
created: 2025-11-11 15:22
updated: 2025-11-11 15:22
type: runbook
sphere: [devops]
topic: [gateway, yarp]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [yarp, reverse-proxy]
---

# 05 — API Gateway (YARP)

Сборка и запуск
- [ ] ```bash
  cd /opt/ois-cfa
  C="-f docker-compose.yml -f docker-compose.override.yml -f docker-compose.kafka.override.yml -f docker-compose.services.yml"
  docker compose $C build api-gateway && docker compose $C up -d api-gateway
  curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55000/health
  ```

Примечания по маршрутам
- [ ] Маршруты читаются из `apps/api-gateway/appsettings.json` (секция `ReverseProxy`)
- [ ] Исправлено правило redeem: `"/v1/issuances/{id}/redeem"` (catch‑all в середине запрещён)

Проверки
- [ ] `/health` → 200
- [ ] Запросы на `/v1/orders/{id}`, `/v1/wallets/{investorId}` возвращают 404 (NotFound), если нет данных — это нормальная реакция

