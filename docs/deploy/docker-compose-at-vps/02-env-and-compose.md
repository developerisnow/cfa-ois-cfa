---
created: 2025-11-11 15:21
updated: 2025-11-11 15:21
type: runbook
sphere: [devops]
topic: [env, compose]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [compose, env]
---

# 02 — Настройка `.env` и Compose файлов

Репозиторий и путь
- [ ] Код расположен в `/opt/ois-cfa`
- [ ] Файлы Compose:
  - `docker-compose.yml` (инфраструктура)
  - `docker-compose.override.yml` (порты/переменные из `.env`)
  - `docker-compose.kafka.override.yml` (Kafka образ для dev)
  - `docker-compose.services.yml` (.NET сервисы + API gateway)
  - `docker-compose.apps.yml` (опционально: фронтенды Next.js)

Переменные окружения (`.env`)
- [ ] Открыть `./ois-cfa/.env` и проверить:
  - [ ] Порты сервисов: `GATEWAY_HOST_PORT=55000`, `IDENTITY_HOST_PORT=55001`, `ISSUANCE_HOST_PORT=55005`, `REGISTRY_HOST_PORT=55006`, `SETTLEMENT_HOST_PORT=55007`, `COMPLIANCE_HOST_PORT=55008`
  - [ ] Инфра: `POSTGRES_HOST_PORT=55432`, `KAFKA_HOST_PORT=59092`, `ZOOKEEPER_HOST_PORT=52181`, `KEYCLOAK_HOST_PORT=58080`, `MINIO_HOST_PORT=59000`, `MINIO_CONSOLE_PORT=59001`
  - [ ] Соединения: `SERVICE_DB_CONN=Host=postgres;Port=5432;Database=ois;Username=ois;Password=ois_dev_password`
  - [ ] Kafka bootstrap: `KAFKA_BOOTSTRAP=kafka:9092`
  - [ ] (Опционально для фронтов) `API_PUBLIC_URL`, `KEYCLOAK_PUBLIC_URL`, `KEYCLOAK_REALM`, `ISSUER_HOST_PORT`, `INVESTOR_HOST_PORT`, `BACKOFFICE_HOST_PORT`

Git/синхронизация кода на VPS
- [ ] Если нужно обновить код из локали: 
  ```bash
  rsync -az --delete --exclude '.git' --exclude 'node_modules' ././ois-cfa/ cfa1:/opt/ois-cfa/
  ```

