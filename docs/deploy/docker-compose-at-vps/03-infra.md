---
created: 2025-11-11 15:21
updated: 2025-11-11 15:21
type: runbook
sphere: [devops]
topic: [infra, postgres, kafka, keycloak, minio]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [compose, infra]
---

# 03 — Инфраструктура (Postgres, Kafka/ZK, Keycloak, Minio)

Запуск инфраструктуры
- [ ] ```bash
  cd /opt/ois-cfa
  docker compose -f docker-compose.yml -f docker-compose.override.yml -f docker-compose.kafka.override.yml up -d
  ```
- [ ] Проверить контейнеры: 
  - `docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"`

Health/порты (локально на сервере)
- [ ] Postgres: `docker exec -it ois-postgres pg_isready -U ois`
- [ ] Keycloak: порт `58080` (админ admin/admin123), URL: `http://localhost:58080`
- [ ] Minio: `http://localhost:59001` (minioadmin/minioadmin)

Примечание по Kafka
- [ ] В dev используем образ `confluentinc/cp-kafka:7.5.0` (через override)

