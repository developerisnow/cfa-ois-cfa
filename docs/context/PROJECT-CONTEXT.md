# PROJECT-CONTEXT (OIS-CFA)

Generated: 2025-01-27  
Last Updated: 2025-11-18 (NX-01 spec validation re-check on infra.defis.deploy)

## Executive Summary

Репозиторий содержит mono‑repo ОИС для ЦФА с backend‑сервисами на .NET 8/9, фронт‑порталами (Next.js), спецификациями OpenAPI/AsyncAPI и инфраструктурой (K8s/Helm/GitLab CI). Спецификация‑first артефакты присутствуют в `packages/contracts`. Основные bounded contexts: issuance, registry, settlement, compliance, identity, а также шлюз к Fabric (`fabric-gateway`) и API‑gateway.

Текущий статус по признакам в репозитории:
- Спеки: OpenAPI для gateway/issuance/registry/settlement, AsyncAPI событий, JSON Schema доменных сущностей.
- Сервисы: исходники для всех ключевых сервисов, telemetry (OTel/Prometheus) и health‑пробы у большинства.
- События: Kafka/MassTransit, outbox‑паттерн реализован (issuance, registry, compliance) и consumer в settlement.
- Инфра/CI: Helm/K8s манифесты и GitLab CI присутствуют; есть заметки и гайты по GitLab Runner/ArgoCD.
- Артефакты: собраны отчеты (Keycloak, frontend, build), но тестовые отчеты/coverage не обнаружены.

## Rules Digest (.cursor/rules)

См. также: `docs/context/RULES-SUMMARY.md` для полной выжимки.

- Нулевая галлюцинация и spec‑first/test‑first подход.
- .NET 8/9, C# 12+, DDD/CQRS, EF Core 8, MassTransit, Kafka/RabbitMQ, Redis, Keycloak.
- Наличие OTEL, HealthChecks, Prometheus — обязательно для всех сервисов.
- Любые изменения сопровождаются тестами и командами запуска.

## Architecture Snapshot

Сервисы (backend, `services/`):
- compliance — KYC/аудит, публикует `ois.kyc.updated`, `ois.audit.logged`, `ois.compliance.flagged` (см. `services/compliance/Services/ComplianceService.cs`).
- identity — заглушка OIDC/Keycloak интеграций (см. `services/identity/Program.cs`).
- issuance — выпуск ЦФА, Kafka outbox (`services/issuance/Background/OutboxPublisher.cs`).
- registry — заказы/кошельки/транзакции, события `ois.order.*`, `ois.registry.transferred` (см. `services/registry/Services/RegistryService.cs`).
- settlement — расчет/консьюмеры событий (например, `services/settlement/Consumers/OrderPaidEventConsumer.cs`).
- fabric-gateway — HTTP‑шлюз к Fabric с resilient HttpClient.

Приложения (frontend, `apps/`):
- api-gateway (ASP.NET Core), backoffice, broker-portal, portal‑investor, portal‑issuer, shared‑ui.

Подробный контекст по фронтенду см. в `docs/context/FRONTEND-CONTEXT.md`.

Опорные документы (`docs/architecture/*`): C4‑снимок, последовательности ЕСИА/OIDC, модель данных, дизайн сети Fabric, NFR targets.

<!-- BEGIN: PROJECT-CONTEXT:API-EVENT-MATRIX -->

## API/Event Matrix

Источники: `packages/contracts/openapi-*.yaml`, `packages/contracts/asyncapi.yaml`.  
Валидация выполнена: 2025-01-27 (NX-01, initial). Перепроверено на ветке `infra.defis.deploy`: 2025-11-18 (NX-01 v2). Отчёты: `artifacts/spec-lint-openapi.txt`, `artifacts/spec-validate-asyncapi.txt`, `artifacts/spec-validate-jsonschema.txt`.

### REST API Matrix (Gateway → Services)

**Gateway Implementation:** YARP (Yet Another Reverse Proxy)  
**Configuration:** `apps/api-gateway/appsettings.json`  
**Last Updated:** 2025-01-27 (NX-02)

| Gateway Endpoint | Method | YARP Route | Target Service | Service Endpoint | Status | Notes |
|-----------------|--------|------------|----------------|------------------|--------|-------|
| `/health` | GET | Direct | Gateway | `/health` | ✅ | Gateway health probe |
| `/issuances` | POST | `issuances` | Issuance | `/v1/issuances` | ✅ | YARP transform: `/issuances` → `/v1/issuances` |
| `/issuances/{id}` | GET | `issuances` | Issuance | `/v1/issuances/{id}` | ✅ | `services/issuance/Program.cs:216` |
| `/issuances/{id}/publish` | POST | `issuances` | Issuance | `/v1/issuances/{id}/publish` | ✅ | `services/issuance/Program.cs:228` |
| `/issuances/{id}/close` | POST | `issuances` | Issuance | `/v1/issuances/{id}/close` | ✅ | `services/issuance/Program.cs:253` |
| `/v1/issuances/{id}/redeem` | POST | `redeem` | Registry | `/v1/issuances/{id}/redeem` | ✅ | `services/registry/Program.cs:261` |
| `/v1/orders` | POST | `orders` | Registry | `/v1/orders` | ✅ | `services/registry/Program.cs:212` |
| `/orders/{id}` | GET | `orders` | Registry | `/v1/orders/{id}` | ⚠️ | OpenAPI: `/orders/{id}`, но YARP ожидает `/v1/orders/{id}` |
| `/v1/wallets/{investorId}` | GET | `wallets` | Registry | `/v1/wallets/{investorId}` | ✅ | `services/registry/Program.cs:249` |
| `/v1/settlement/run` | POST | `settlement` | Settlement | `/v1/settlement/run` | ✅ | `services/settlement/Program.cs:182` |
| `/v1/reports/payouts` | GET | `reports` | Settlement | `/v1/reports/payouts` | ✅ | `services/settlement/Program.cs:205` |
| `/v1/compliance/kyc/check` | POST | `compliance` | Compliance | `/v1/compliance/kyc/check` | ✅ | `services/compliance/Program.cs:192` |
| `/v1/compliance/qualification/evaluate` | POST | `compliance` | Compliance | `/v1/compliance/qualification/evaluate` | ✅ | `services/compliance/Program.cs:204` |
| `/v1/compliance/investors/{id}/status` | GET | `compliance` | Compliance | `/v1/compliance/investors/{id}/status` | ✅ | `services/compliance/Program.cs:216` |
| `/v1/complaints` | POST | `complaints` | Compliance | `/v1/complaints` | ✅ | `services/compliance/Program.cs:230` |

**YARP Clusters:**
- `issuance` → `http://issuance-service:8080`
- `registry` → `http://registry-service:8080`
- `settlement` → `http://settlement-service:8080`
- `compliance` → `http://compliance-service:8080`
- `identity` → `http://identity-service:8080`

**SPEC DIFF**: 
- Gateway OpenAPI определяет `/issuances` без `/v1`, но YARP корректно трансформирует в `/v1/issuances` для сервиса.
- Gateway OpenAPI определяет `/orders/{id}` без `/v1`, но YARP маршрут `orders` ожидает `/v1/orders/{**catch-all}`. Требуется либо обновить OpenAPI на `/v1/orders/{id}`, либо добавить дополнительный маршрут.

**Health & Metrics:**
- Все основные сервисы имеют `/health` endpoint (✅)
- Issuance, Registry, Settlement, Compliance имеют `/metrics` (Prometheus) (✅)
- Gateway имеет `/health`, но не имеет `/metrics` (⚠️ рекомендуется добавить)

### Events Matrix (AsyncAPI ↔ Code)

| Topic | AsyncAPI | Producer (Service) | Consumer | Status | Notes |
|-------|----------|---------------------|-----------|--------|------|
| `ois.issuance.published` | ✅ | Issuance (`IssuanceService.cs:121`) | — | ✅ | Outbox: `services/issuance/Background/OutboxPublisher.cs:67` |
| `ois.issuance.closed` | ✅ | Issuance (`IssuanceService.cs:177`) | — | ✅ | Outbox: `services/issuance/Background/OutboxPublisher.cs:71` |
| `ois.order.placed` | ✅ | — | — | ⚠️ | В AsyncAPI, но нет в коде (только в тестах: `registry.Tests/OutboxPublishTests.cs:37`) |
| `ois.order.created` | ✅ | Registry (`RegistryService.cs:83`) | — | ✅ | Outbox: `services/registry/Background/OutboxPublisher.cs:67` |
| `ois.order.reserved` | ✅ | Registry (`RegistryService.cs:112`) | — | ✅ | Outbox: `services/registry/Background/OutboxPublisher.cs:71` |
| `ois.order.paid` | ✅ | Registry (`RegistryService.cs:268`) | Settlement (`OrderPaidEventConsumer.cs`, `OrderPaidConsumer.cs`) | ✅ | MassTransit: `services/settlement/Program.cs:69` |
| `ois.order.confirmed` | ✅ | — | — | ⚠️ | В AsyncAPI, но нет продьюсера в коде |
| `ois.registry.transferred` | ✅ | Registry (`RegistryService.cs:278`) | — | ✅ | Outbox: `services/registry/Background/OutboxPublisher.cs:79` |
| `ois.payout.executed` | ✅ | Settlement (`SettlementService.cs:175`) | — | ✅ | Outbox: `services/settlement/Background/OutboxPublisher.cs:85` |
| `ois.payout.scheduled` | ✅ | — | — | ⚠️ | В AsyncAPI, но нет продьюсера в коде |
| `ois.audit.logged` | ✅ | Compliance (`ComplianceService.cs:64`) | — | ✅ | Outbox: `services/compliance/Background/OutboxPublisher.cs:75` |
| `ois.transfer.completed` | ✅ | — | — | ⚠️ | В AsyncAPI, но нет продьюсера в коде |
| `ois.compliance.flagged` | ✅ | Compliance (`ComplianceService.cs:150,226`) | — | ✅ | Outbox: `services/compliance/Background/OutboxPublisher.cs:67` |
| `ois.kyc.updated` | ✅ | Compliance (`ComplianceService.cs:55`) | — | ✅ | Outbox: `services/compliance/Background/OutboxPublisher.cs:71` |

**SPEC DIFF**: Топики `ois.order.placed`, `ois.order.confirmed`, `ois.payout.scheduled`, `ois.transfer.completed` объявлены в AsyncAPI, но не имеют продьюсеров в коде. Требуется либо реализация, либо удаление из AsyncAPI.

<!-- END: PROJECT-CONTEXT:API-EVENT-MATRIX -->

## Quality Summary (artifacts/*)

Найдено:
- Build/Frontend/Keycloak отчёты: `artifacts/*`. Диагностики и гайды по GitLab Runner/Keycloak.
- **Спецификации валидация (NX-01, initial 2025-01-27; re-check 2025-11-18 на infra.defis.deploy)**:
  - `artifacts/spec-lint-openapi.txt` — Spectral lint: ✅ No errors (9 OpenAPI файлов)
  - `artifacts/spec-validate-asyncapi.txt` — AsyncAPI CLI: ✅ Valid
  - `artifacts/spec-validate-jsonschema.txt` — AJV: ⚠️ Предупреждения о форматах `uuid`/`decimal` (не критично)

Не найдено/Требуется:
- Unit/Integration/E2E отчёты (JUnit/Allure), coverage отчёты.

<!-- BEGIN: PROJECT-CONTEXT:API-EVENT-GAPS -->

## Gap List (spec/code/tests)

### Спеки (✅ Обновлено NX-01)

**Решено:**
- ✅ Spectral lint для OpenAPI выполнен: 9 файлов, ошибок нет (`artifacts/spec-lint-openapi.txt`)
- ✅ AsyncAPI CLI валидация выполнена: синтаксис корректен (`artifacts/spec-validate-asyncapi.txt`)
- ✅ JSON Schema валидация выполнена: предупреждения о форматах `uuid`/`decimal` (не критично, AJV игнорирует неизвестные форматы)

**Осталось:**
- ⚠️ JSON Schemas: форматы `uuid` и `decimal` не поддерживаются AJV по умолчанию. Рекомендация: использовать `pattern` для UUID или добавить кастомные форматы.
- ⚠️ AsyncAPI: топики `ois.order.placed`, `ois.order.confirmed`, `ois.payout.scheduled`, `ois.transfer.completed` объявлены, но нет продьюсеров в коде.

### Код

**API Gateway маршрутизация:**
- ⚠️ Gateway использует `/issuances` без `/v1`, сервисы — с `/v1`. Требуется согласование или проксирование через YARP (`apps/api-gateway`).
- ✅ Основные endpoints соответствуют спецификациям (см. API Matrix выше).

**События:**
- ⚠️ Топики в AsyncAPI без продьюсеров: `ois.order.placed` (только в тестах), `ois.order.confirmed`, `ois.payout.scheduled`, `ois.transfer.completed`.
- ✅ Реализованные топики соответствуют AsyncAPI схемам.

**Сервисы:**
- ⚠️ Identity — минимальная заглушка; требуется проработка Keycloak интеграции и защищённых политик на всех эндпойнтах.
- ✅ Health/metrics: все основные сервисы имеют `/health` и Prometheus метрики (`/metrics`).
- ⚠️ Gateway: имеет `/health`, но не имеет `/metrics` для мониторинга проксируемых запросов.

**API Gateway маршрутизация (✅ Обновлено NX-02):**
- ✅ YARP настроен корректно: маршруты определены в `apps/api-gateway/appsettings.json`
- ✅ Трансформация путей работает: `/issuances` → `/v1/issuances` для Issuance сервиса
- ⚠️ Несоответствие OpenAPI: `/orders/{id}` в OpenAPI, но YARP ожидает `/v1/orders/{id}`
- ✅ Health check цель: `make check-health` проверяет все сервисы параллельно

**Issuance Service (✅ Обновлено NX-03):**
- ✅ Endpoints соответствуют OpenAPI: POST/GET `/v1/issuances`, POST `/publish`, POST `/close`
- ✅ DTO и валидаторы соответствуют схемам OpenAPI
- ✅ Аутентификация: JWT Bearer + политики `role:issuer` / `role:any-auth`
- ✅ События: `ois.issuance.published` и `ois.issuance.closed` публикуются через outbox
- ⚠️ SPEC DIFF: События содержат `dltTxHash`, которого нет в AsyncAPI (рекомендуется добавить)
- ✅ Тесты: Unit и Integration тесты созданы в `services/issuance/issuance.Tests/`
- **Отчёт:** `artifacts/issuance-endpoints-coverage-report.md`

### Тесты/CI

- ⚠️ Нет артефактов нагрузочного теста (k6) и e2e сценариев выпуска/покупки/погашения.
- ⚠️ Не видно отчетов покрытия; нет Pact/контрактных тестов между gateway и сервисами.

<!-- END: PROJECT-CONTEXT:API-EVENT-GAPS -->

## References

- Спеки: `packages/contracts/*` (OpenAPI/AsyncAPI/JSON Schemas).
- Сервисы: `services/*` (Program.cs, Services/*, DTOs/*, Migrations/*).
- Приложения: `apps/*`.
- Инфра/CI: `ops/*`, `.gitlab-ci.yml`, Helm/K8s в `ops/infra/*`.
- Правила/промпты: `.cursor/rules/*`, `.cursor/promts/*`.
- Gateway Routing: `artifacts/gateway-routing-report.md` (NX-02, 2025-01-27).
- Issuance Coverage: `artifacts/issuance-endpoints-coverage-report.md` (NX-03, 2025-01-27).
