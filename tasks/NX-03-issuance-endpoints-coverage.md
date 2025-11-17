# NX-03 — Issuance: выравнивание эндпойнтов с OpenAPI + тесты

Цель: Синхронизировать реализацию сервиса Issuance с `openapi-issuance.yaml` и `openapi-gateway.yaml`, добавить юнит/интеграционные тесты, проверить публикацию событий из outbox.

Входы
- Спеки: `packages/contracts/openapi-issuance.yaml`, `packages/contracts/openapi-gateway.yaml`
- Код: `services/issuance/*`

Шаги
1. Сопоставить операции `/issuances` (create/get/publish/close) между спеками и текущими контроллерами/сервисами.
2. Для отсутствующих DTO/валидаторов — добавить в `services/issuance/DTOs`, `Validators` согласно схемам в `packages/contracts/schemas/*`.
3. Проверить/настроить аутентификацию/политики для операций (Bearer/Keycloak) — согласовать со спецификацией SecuritySchemes.
4. Убедиться, что события `ois.issuance.published` и `ois.issuance.closed` публикуются через outbox и соответствуют AsyncAPI payload.
5. Добавить тесты: `services/issuance/issuance.Tests` — юнит на сервис, интеграционный на эндпойнты (WebApplicationFactory).
6. Обновить документацию при расхождениях (SPEC DIFF → MR на YAML перед кодом).

Команды
- `dotnet test services/issuance/issuance.Tests -v minimal`
- Проверка событий: временный consumer или логи outbox publisher (unit)

Файлы/артефакты
- Код и тесты в `services/issuance/*`
- `artifacts/issuance-test-report.txt` — результаты тестов

Критерии приёмки
- Эндпойнты Issuance соответствуют OpenAPI; тесты зелёные.
- События соответствуют AsyncAPI (имена топиков и payload).

