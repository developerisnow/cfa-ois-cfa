# NX-04 — Registry: поток заказа (create→reserve→paid) + события

Цель: Завершить и покрыть тестами ключевой поток заказа в Registry, обеспечить публикацию событий (`ois.order.*`, `ois.registry.transferred`) и потребление в Settlement.

Входы
- Спеки: `packages/contracts/openapi-gateway.yaml`, `packages/contracts/schemas/Order.json`, `Wallet.json`, др.
- Код: `services/registry/*`, `services/settlement/Consumers/*`

Шаги
1. Сопоставить `RegistryService` методам OpenAPI: создание заказа, резервирование, пометка как оплаченный, отмена, получение.
2. Проверить outbox‑вставки в `RegistryService` — топики/поля соответствуют AsyncAPI и JSON Schemas.
3. Реализовать/уточнить консьюмер в Settlement для события `ois.order.paid` (см. `Consumers/OrderPaidEventConsumer.cs`).
4. Добавить интеграционные тесты: успешный happy‑path create→reserve→paid (с заглушкой bank/ledger), проверка изменений кошелька/холдинга.
5. Уточнить метрики/логи (время операций, корреляция X‑Request‑ID) и health.

Команды
- `dotnet test services/registry/registry.Tests -v minimal`
- Локальная проверка: curl эндпойнтов gateway/registry (см. NX‑02)

Файлы/артефакты
- Код и тесты в `services/registry/*`, `services/settlement/*`
- `artifacts/registry-flow-report.md` — краткий отчёт и логи запуска теста

Критерии приёмки
- Поток заказа проходит e2e (без реальных внешних интеграций); тесты зелёные.
- Публикация событий соответствует AsyncAPI; консьюмер Settlement корректно обрабатывает `ois.order.paid`.

