---
created: 2025-11-11 15:20
updated: 2025-11-11 15:20
type: runbook
sphere: [devops]
topic: [deploy, docker-compose, vps]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [compose, linux, dotnet, keycloak]
---

# OIS‑CFA · Deploy на VPS (Docker Compose) — Обзор

Цель: поднять полный dev‑контур на VPS с Docker Compose: инфраструктура, .NET‑сервисы, API‑шлюз, Keycloak, и (опционально) веб‑клиенты.

Ключевые принципы
- [ ] Используем non‑standard порты, чтобы не конфликтовать с окружением
- [ ] Сборка выполняется поэтапно (низкая RAM) — «infra → services → gateway → web»
- [ ] Миграции БД включаем флагом `MIGRATE_ON_STARTUP=true` (по умолчанию off)
- [ ] Логи читаем через `docker logs`, готовность через `/health`

Состав контура (порты по умолчанию)
- API Gateway: `55000`
- Identity: `55001`
- Issuance: `55005`
- Registry: `55006`
- Settlement: `55007`
- Compliance: `55008`
- PostgreSQL: `55432`
- Kafka: `59092`, ZooKeeper: `52181`
- Keycloak: `58080`
- Minio: `59000` (S3), `59001` (Console)

Структура документации
- 01 — Подготовка VPS и Docker
- 02 — Настройка `.env` и Compose
- 03 — Инфраструктура
- 04 — .NET‑сервисы
- 05 — API‑шлюз
- 06 — Keycloak (realm/clients)
- 07 — Веб‑клиенты (Next.js)
- 08 — Smoke‑тесты
- 09 — Траблшутинг

