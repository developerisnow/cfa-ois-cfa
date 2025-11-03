# Мэппинг ГОСТ Р 57580.1-2017 → меры в системе

> Контроль соответствия требованиям информационной безопасности

_Last updated: 2025-11-01_

| № | Контроль | Мера | Доказательство | Статус |
|---|---|---|---|---|
| 1 | Управление политикой ИБ | Политика ИБ, обновляемая через ADR | docs/architecture/security/Policy-IB.md | ✅ |
| 2 | Организация ИБ | RACI матрица, роли в Keycloak | CODEOWNERS, Keycloak roles | ✅ |
| 3 | Классификация активов | Инвентарь ЦФА, персональных данных | ER диаграммы, DFD | ✅ |
| 4 | Контроль доступа (AC) | RBAC через Keycloak, middleware | apps/*/src/middleware.ts | ✅ |
| 5 | Криптография | mTLS между gateway и services (dev: self-signed) | docker-compose.yml, cert-manager notes | ⏳ |
| 6 | Безопасность коммуникаций | HTTPS, security headers, rate limiting | apps/api-gateway/Program.cs | ✅ |
| 7 | Контроль доступа к сети | Firewall rules, сетевые зоны | ops/infra/Network-Zones.drawio | ⏳ |
| 8 | Мониторинг и логирование | OpenTelemetry, Serilog JSON, Prometheus | services/*/Program.cs | ✅ |
| 9 | Управление уязвимостями | OWASP Dependency Check, SAST в CI | .github/workflows/security-scan.yml | ✅ |
| 10 | Обработка инцидентов | Playbook в docs/architecture/security/SoC-Playbooks.md | SoC-Playbooks.md | ⏳ |
| 11 | Аудит ИБ | Audit log в БД, журнал событий | services/*/AuditEvent.cs | ✅ |
| 12 | Резервное копирование | Nightly backups, restore процедуры | ops/scripts/backup.sh | ✅ |
| 13 | Восстановление после сбоев | DRP документация, RTO/RPO целевые | docs/architecture/security/DRP.md | ⏳ |
| 14 | Контроль изменений | Git workflow, CODEOWNERS, PR review | .github/workflows/*.yml | ✅ |
| 15 | Обработка ПДн | Маскирование в логах, согласие пользователя | Serilog enrichment, consent flow | ⏳ |
| 16 | Управление рисками | STRIDE модель угроз | docs/architecture/threat/STRIDE-Context.md | ✅ |
| 17 | Контроль поставщиков | СТО БР 1.4 checklist | docs/architecture/security/STO-BR-Checklist.md | ✅ |
| 18 | Независимая оценка | Пентест план, регулярные аудиты | docs/security/Plan-Pentest.md | ⏳ |
| 19 | Безопасность приложений | Input validation, ProblemDetails, SAST | FluentValidation, RFC7807 | ✅ |
| 20 | Контроль сервисов | Health checks, graceful shutdown | services/*/Program.cs | ✅ |
| 21 | Управление емкостью | Мониторинг через Prometheus/Grafana | ops/infra/grafana-dashboards.json | ✅ |
| 22 | Изоляция данных | Схемы БД по сервисам, приватные каналы HLF | services/*/DbContext.cs | ✅ |
| 23 | Контроль копий | Retention policy для бэкапов | ops/scripts/backup.sh (7 days) | ✅ |
| 24 | Управление событиями | Kafka events, audit trail | packages/contracts/asyncapi.yaml | ✅ |
| 25 | Контроль доступа к источникам | Keycloak OIDC, token validation | apps/*/src/lib/auth.ts | ✅ |
| 26 | Целостность данных | DLT ledger, hash verification | chaincode/**/*.go | ✅ |
| 27 | Контроль цепочки поставок | Dependency audit в CI | .github/workflows/security-scan.yml | ✅ |
| 28 | Управление доступом приложений | API Gateway с rate limiting | apps/api-gateway/Program.cs | ✅ |
| 29 | Контроль тестов | Test coverage, security tests | tests/**/*.cs | ✅ |
| 30 | Управление жизненным циклом | Версионирование API, миграции БД | packages/contracts/openapi-*.yaml | ✅ |
| 31 | События ИБ | Логирование failed auth, rate limit | Serilog, middleware | ✅ |
| 32 | Обучение персонала | CONTRIBUTING.md, SECURITY.md | ops/CONTRIBUTING.md | ✅ |
| 33 | Контроль привилегий | Минимальные права в БД, service accounts | docker-compose.yml env vars | ✅ |
| 34 | Защита от вредоносного ПО | Container scanning, базовые образы | Dockerfiles | ✅ |
| 35 | Контроль переносимости | Версионирование схем, миграции | EF Core migrations | ✅ |
| 36 | Управление конфигурацией | .env.example, secrets в vault | .env.example files | ✅ |
| 37 | Обработка ошибок | ProblemDetails (RFC7807), logging | services/*/Program.cs | ✅ |
| 38 | Контроль времени | NTP синхронизация (infra), timestamps | AuditEvent.ts | ✅ |
| 39 | Управление инцидентами | Playbook, escalation procedure | docs/architecture/security/SoC-Playbooks.md | ⏳ |
| 40 | Контроль совместимости | API versioning, backward compatibility | OpenAPI specs | ✅ |

**Условные обозначения:**
- ✅ Реализовано
- ⏳ В планах / частично
- ❌ Не реализовано
