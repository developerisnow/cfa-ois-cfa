# RULES-SUMMARY — Выжимка из .cursor/rules

## 1. Общие принципы (global.mdc, composer.mdc)
- План сначала, код потом; небольшие, обратимые диффы (<200 строк) с явным планом.
- Нулевая галлюцинация: только факты из репозитория и официальных документов.
- Spec-first и test-first: сначала OpenAPI/AsyncAPI/JSON Schemas и их валидация, затем реализация и тесты.
- Обязательны юнит, интеграционные и e2e-тесты для новых/изменённых функций.
- Всегда указывать команды для сборки/тестов и минимальный manual-check.

## 2. Backend (.NET, DDD/CQRS) — dotnet-ddd.mdc, BACKEND-OIS.md
- Архитектура слоёв: Domain, Application, Infrastructure, API.
- Domain: сущности/агрегаты, Value Objects (immutable), доменные события.
- Application: команды/запросы (CQRS, MediatR-подход), FluentValidation, политики авторизации.
- Infrastructure: EF Core 8 (без lazy loading), контексты, конфигурации, outbox, MassTransit (Kafka/RabbitMQ).
- API: контроллеры/минимальные API строго по OpenAPI; версия API; валидация входа.
- События: MassTransit + Kafka, саги, idempotent-консьюмеры, outbox/inbox паттерны.
- Observability: OpenTelemetry (traces, metrics), Prometheus `/metrics`, HealthChecks `/health`.

## 3. Observability & Security — observability.mdc, security.mdc
- Добавлять/проверять OTEL: ресурс (service.name, environment), экспортер OTLP.
- HealthChecks с проверкой зависимностей (БД, брокер, Redis) и готовностью.
- Политики ретраев/таймаутов/сircuit-breaker (Polly) для внешних вызовов.
- OWASP ASVS: входные данные валидируются, полезные нагрузки внешних систем проверяются.
- Секреты — только из окружения/секрет-менеджера; PII-safe логирование, маскирование токенов.

## 4. Testing Discipline — testing.mdc
- Любое изменение сопровождается изменением/добавлением тестов (xUnit + FluentAssertions).
- Для багов: сначала падающий тест, потом фикс.
- Критичные пути ≥90% покрытия, адаптеры ≥70%.
- Рекомендованный запуск: `dotnet test -m:1` + генерация отчётов покрытия.

## 5. Project Rules — develop-promt.mdc, main-promt.mdc
- Цель: MVP ОИС по вертикалям выпуск→покупка→выплаты→погашение с регуляторным комплектом.
- Все артефакты должны быть валидны (lint/build/test), с указанием источников и дат.
- Для любых допущений — фиксировать в `ARCHIVE/assumptions.md`.
- Документы: структурированы, с оглавлением, версионностью и матрицей соответствия НПА.

## 6. Frontend (из MVP-impl и промптов)
- Next.js 15 для порталов (`portal-investor`, `portal-issuer`, `backoffice`).
- SDK на TypeScript из OpenAPI (`packages/sdks/ts`), единый `OisApiClient` с заголовками `x-request-id`, `traceparent`, `x-client-app`.
- Auth: Keycloak OIDC через NextAuth, роли: investor, issuer, backoffice/admin.
- E2E: Playwright-journeys для ключевых пользовательских сценариев.

