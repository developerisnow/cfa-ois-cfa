# NX-02 — API Gateway маршрутизация + health/metrics

Цель: Проверить/задокументировать маршрутизацию API Gateway к сервисам, обеспечить единообразные `/health` и `/metrics` во всех сервисах, обновить документацию.

Входы
- Gateway/OpenAPI: `packages/contracts/openapi-gateway.yaml`
- Код gateway: `apps/api-gateway/*`
- Сервисы: `services/*`

Шаги
1. Инвентаризировать эндпойнты из `openapi-gateway.yaml` и сопоставить сервисам (issuance/registry/settlement/compliance).
2. Проверить наличие health‑проб (`/health`) и метрик (`/metrics` Prometheus) в каждом сервисе. Для отсутствующих — добавить.
3. Убедиться, что gateway проксирует/маршрутизирует запросы корректно (YARP/ReverseProxy или контроллеры). Если используется YARP — описать маршруты.
4. Обновить `docs/context/PROJECT-CONTEXT.md` (Architecture Snapshot и API/Event Matrix) с фактической схемой маршрутизации.
5. Добавить Makefile цели: `make check-health` — параллельные GET запросы ко всем сервисам `/health` и сбор статуса.

Подсказки/кодовые точки
- Примеры telemetry/health уже есть в `services/issuance`, `services/compliance`.
- Проверить `Program.cs` в каждом сервисе на `MapHealthChecks`/Prometheus endpoints.

Команды
- `dotnet build` (сборка gateway и сервисов)
- Локальные проверки health: `curl -sf http://localhost:PORT/health`

Файлы/артефакты
- `Makefile` — цель `check-health`.
- Обновлённый `docs/context/PROJECT-CONTEXT.md` с таблицей маршрутов.
- `artifacts/gateway-routing-report.md` — краткий отчёт о сопоставлении эндпойнтов.

Критерии приёмки
- Все сервисы отвечают 200 на `/health`; метрики доступны как минимум в Issuance/Compliance/Registry/Settlement.
- Gateway корректно маршрутизирует как минимум эндпойнты Issuance и Registry, подтверждено ручной проверкой (curl) или интеграционным тестом.
- Обновлённая документация.

