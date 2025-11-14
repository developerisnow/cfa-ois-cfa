---
created: 2025-11-11 15:23
updated: 2025-11-11 15:23
type: runbook
sphere: [devops]
topic: [nextjs, web]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [nextjs, docker]
---

# 07 — Веб‑клиенты (Next.js)

Сервисы
- [ ] Portal Issuer (порт по умолчанию 53001)
- [ ] Portal Investor (порт по умолчанию 53002)
- [ ] Backoffice (порт по умолчанию 53003)

Зависимости monorepo
- [ ] Некоторые приложения импортируют `shared-ui` и `@ois/api-client`
- [ ] Для корректной сборки нужен «корневой» install и сборка пакетов (или простая альтернатива ниже)

Альтернатива (минимальный путь на dev)
- [ ] Сборка Portal Issuer/Investor из своих папок (Dockerfiles добавлены)
- [ ] Backoffice можно отложить (ошибки резолва модулей при отсутствии сборки shared‑ui/sdk)

Запуск (issuer + investor)
- [ ] ```bash
  cd /opt/ois-cfa
  C="-f docker-compose.yml -f docker-compose.override.yml -f docker-compose.kafka.override.yml -f docker-compose.services.yml -f docker-compose.apps.yml"
  docker compose $C build portal-issuer portal-investor
  docker compose $C up -d portal-issuer portal-investor
  for p in 53001 53002; do curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:${p}/; done
  ```

Переменные окружения для фронтов
- [ ] `API_PUBLIC_URL=http://<host-ip>:55000`
- [ ] `KEYCLOAK_PUBLIC_URL=http://<host-ip>:58080` (или локальный туннель)
- [ ] `KEYCLOAK_REALM=ois`
- [ ] `NEXTAUTH_URL` для каждого фронта на свой URL (см. docker-compose.apps.yml)

Логин через Keycloak
- [ ] Убедиться, что клиенты в Keycloak созданы (см. 06)
- [ ] Проверить redirect URIs и web origins под ваши адреса

- SSH‑туннели (если внешний фаервол закрыт)
- [ ] Порты должны быть ≤ 65535. Пример корректного туннеля:
  ```bash
  ssh -N \
    -L 15500:localhost:55000 \
    -L 15501:localhost:55001 \
    -L 15506:localhost:55006 \
    -L 15808:localhost:58080 \
    -L 15301:localhost:53001 \
    -L 15302:localhost:53002 \
    cfa1-mux
  ```
- [ ] Открыть в браузере: 
  - Gateway: `http://localhost:15500/health`
  - Issuer app: `http://localhost:15301/`
  - Investor app: `http://localhost:15302/`
  - Keycloak: `http://localhost:15808/`
