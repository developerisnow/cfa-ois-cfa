# PROJECT-CONTEXT (OIS-CFA)

Generated: 2025-11-13

## Executive Summary

Репозиторий содержит mono‑repo ОИС для ЦФА с backend‑сервисами на .NET 8/9, фронт‑порталами (Next.js), спецификациями OpenAPI/AsyncAPI и инфраструктурой (K8s/Helm/GitLab CI). Спецификация‑first артефакты присутствуют в `packages/contracts`. Основные bounded contexts: issuance, registry, settlement, compliance, identity, а также шлюз к Fabric (`fabric-gateway`) и API‑gateway.

Текущий статус по признакам в репозитории:
- Спеки: OpenAPI для gateway/issuance/registry/settlement, AsyncAPI событий, JSON Schema доменных сущностей.
- Сервисы: исходники для всех ключевых сервисов, telemetry (OTel/Prometheus) и health‑пробы у большинства.
- События: Kafka/MassTransit, outbox‑паттерн реализован (issuance, registry, compliance) и consumer в settlement.
- Инфра/CI: Helm/K8s манифесты и GitLab CI присутствуют; есть заметки и гайты по GitLab Runner/ArgoCD.
- Артефакты: собраны отчеты (Keycloak, frontend, build), но тестовые отчеты/coverage не обнаружены.

## Rules Digest (.cursor/rules)

- Нулевая галлюцинация: только факты из репозитория; при нехватке — SPEC DIFF/TODO с точным путём.
- Spec‑first: REST/Events только при наличии OpenAPI/AsyncAPI. Нет спеки — сначала патч спек, затем код.
- Проверяемость: артефакты валидные (lint/build/test), с воспроизводимыми командами.
- Безопасность: секреты вне кода (Vault/ENV), PII‑safe логи; RBAC.
- Код/качество: .NET 8/9, C# 12+, DDD/CQRS, MassTransit, Kafka/RabbitMQ, EF Core 8, Redis, Keycloak.
- Обозревательность: OpenTelemetry traces/metrics, Prometheus `/metrics`, health `/health`.
- Процессы: маленькие диффы, план перед кодом, тесты обязательны; Conventional Commits; CI gates.

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

Опорные документы (`docs/architecture/*`): C4‑снимок, последовательности ЕСИА/OIDC, модель данных, дизайн сети Fabric, NFR targets.

## API/Event Matrix

Источники: `packages/contracts/openapi-*.yaml`, `packages/contracts/asyncapi.yaml`.

REST (основное из openapi-gateway.yaml):
- /health → все сервисы (health probes).
- /issuances [POST, GET, /{id}/publish, /{id}/close] → Issuance Service (`services/issuance`).
- /v1/orders [POST ...] → Registry Service (`services/registry`).
Примечание: Полный перечень см. `packages/contracts/openapi-gateway.yaml` и доменные файлы OpenAPI для сервисов.

Events (AsyncAPI → реализации в коде):
- ois.issuance.published, ois.issuance.closed → публикация: Issuance (см. OutboxPublisher).
- ois.order.created, ois.order.reserved, ois.order.paid → публикация: Registry; потребление: Settlement (консьюмеры).
- ois.registry.transferred → публикация: Registry.
- ois.kyc.updated, ois.audit.logged, ois.compliance.flagged → публикация: Compliance.

Требует верификации соответствия с AsyncAPI: топики/схемы полезной нагрузки ↔ `packages/contracts/schemas/*.json`.

## Quality Summary (artifacts/*)

Найдено:
- Build/Frontend/Keycloak отчёты: `artifacts/*`. Диагностики и гайды по GitLab Runner/Keycloak.

Не найдено/Требуется:
- Unit/Integration/E2E отчёты (JUnit/Allure), coverage отчёты.
- Линты OpenAPI/AsyncAPI/AJV‑валидация JSON Schema как артефакты.

## Gap List (spec/code/tests)

Спеки
- Нет зафиксированных результатов Spectral для OpenAPI и AsyncAPI CLI для событий.
- Проверка соответствия JSON Schema ↔ события/DTO не зафиксирована (AJV/Набор проверок).

Код
- Identity — минимальная заглушка; требуется проработка Keycloak интеграции и защищённых политик на всех эндпойнтах.
- Сверка API Gateway маршрутов и внутренних сервисов (YARP/ReverseProxy, если используется) не задокументирована.
- Полнота health/metrics для всех сервисов требует проверки.

Тесты/CI
- Нет артефактов нагрузочного теста (k6) и e2e сценариев выпуска/покупки/погашения.
- Не видно отчетов покрытия; нет Pact/контрактных тестов между gateway и сервисами.

## References

- Спеки: `packages/contracts/*` (OpenAPI/AsyncAPI/JSON Schemas).
- Сервисы: `services/*` (Program.cs, Services/*, DTOs/*, Migrations/*).
- Приложения: `apps/*`.
- Инфра/CI: `ops/*`, `.gitlab-ci.yml`, Helm/K8s в `ops/infra/*`.
- Правила/промпты: `.cursor/rules/*`, `.cursor/promts/*`.

