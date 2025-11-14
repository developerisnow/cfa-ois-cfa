---
created: 2025-11-11 15:24
updated: 2025-11-11 15:24
type: runbook
sphere: [devops]
topic: [troubleshooting]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [troubleshooting, logs]
---

# 09 — Траблшутинг

Типовые проверки
- [ ] `docker compose -f ... ps` — статусы
- [ ] `docker logs -f <name>` — логи
- [ ] `ss -ltnp` — порты слушаются на хосте
- [ ] `curl -i http://localhost:<port>/health` — готовность

Нехватка памяти при сборке
- [ ] Добавить swap (см. 01)
- [ ] Собирайте по одному сервису: `docker compose ... build <service>`

Проблемы миграций БД при старте
- [ ] Запускать без миграций: `MIGRATE_ON_STARTUP=false docker compose ... up -d <service>`
- [ ] Для разового применения — на время старта: `MIGRATE_ON_STARTUP=true ...`

Gateway не стартует, ошибка YARP
- [ ] Проверить `appsettings.json` — маршрут `redeem` должен быть `"/v1/issuances/{id}/redeem"`

Keycloak недоступен снаружи
- [ ] Проверить провайдерский фаервол (58080 TCP). На самом сервере UFW может быть выключен, но у провайдера порт может быть закрыт.
- [ ] Временно использовать SSH‑туннель (см. 07); пример: `ssh -N -L 15808:localhost:58080 cfa1-mux`

Next.js фронты не собираются
- [ ] Требуют сборки `shared-ui` и SDK: сделать корневой install и сборку пакетов (workspaces)
- [ ] Минимальный путь — собирать issuer/investor; backoffice перенести на следующий этап

Очистка образов/кэша
- [ ] `docker system df`, `docker image prune -f`, `docker builder prune -f`
