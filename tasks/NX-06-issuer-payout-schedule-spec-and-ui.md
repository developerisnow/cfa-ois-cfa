# NX-06 — Issuer: расписание выплат (SPEC DIFF + UI)

Цель: подготовить и согласовать минимальную спецификацию CRUD по расписанию выплат для выпусков ЦФА и реализовать базовый UI в портале эмитента, не выходя за рамки spec-first подхода.

Контекст
- Общий контекст: `docs/context/PROJECT-CONTEXT.md`.
- Frontend контекст: `docs/context/FRONTEND-CONTEXT.md` (раздел про Portal Issuer).
- План MVP фронтенда: `docs/frontend/MVP-impl.md` (Issuer /reports + payout schedule Spec Diff).

Скоуп
- На уровне спеки: предложить минимальный набор эндпойнтов и схем для управления расписанием выплат по выпуску.
- На уровне UI: добавить в портал эмитента (страницу деталей выпуска или отдельную страницу) отображение расписания выплат в read-only виде, опираясь на доступные поля backend (например, `Issuance.scheduleJson` или аналогичные).

Шаги
1. **Анализ текущих спецификаций и кода**
   - Проверить `packages/contracts/openapi-gateway.yaml` и `openapi-issuance.yaml` на наличие любых эндпойнтов, связанных с payout schedule.
   - Проверить domain/DTO в backend (services/issuance, services/settlement, services/registry) на наличие полей, связанных с распорядком выплат.

2. **Подготовка SPEC DIFF для payout schedule**
   - Если в спецификациях нет CRUD для расписания выплат, подготовить YAML-патч (SPEC DIFF) с предложением добавить:
     - `POST /v1/issuances/{id}/payouts/schedule`
     - `GET /v1/issuances/{id}/payouts/schedule`
     - `PATCH /v1/issuances/{id}/payouts/schedule/{itemId}`
     - `DELETE /v1/issuances/{id}/payouts/schedule/{itemId}`
   - Описать схему `PayoutScheduleItem` (id, date, amount, status) и возможные статусы.
   - Сохранить SPEC DIFF как отдельный файл, например `tasks/NX-06-payout-schedule-SPEC-DIFF.md`.

3. **Read-only UI расписания (до реализации backend)**
   - В `apps/portal-issuer` расширить страницу деталей выпуска `/issuances/[id]` или создать вложенную секцию/таб.
   - Отобразить расписание выплат в виде таблицы (дата, сумма, статус), используя:
     - Либо реальные поля из backend (если уже есть).
     - Либо временный read-only маппинг из существующих данных (например, JSON-поле), с чёткой пометкой TODO.
   - Не реализовывать создание/редактирование/отмену выплат до согласования SPEC DIFF.

4. **Тесты и артефакты**
   - Добавить/обновить компонентные тесты для UI-блока расписания.
   - Если есть Playwright-сценарий для Issuer, добавить шаги проверки отображения расписания.
   - Сохранить SPEC DIFF и краткий отчёт в `artifacts/issuer-payout-schedule.md` с:
     - Описанием предложенных эндпойнтов.
     - Скриншотами или описанием UI.

Команды
- Frontend dev: `cd apps/portal-issuer && npm install && npm run dev`.
- Для генерации/проверки OpenAPI (при необходимости): команды из `ops/scripts/validate-specs.sh` и `BACKEND-BUILD-AND-TEST.md`.

Критерии приёмки
- Подготовлен SPEC DIFF для payout schedule, согласованный с архитектурными правилами (spec-first).
- В портале эмитента на странице выпуска видно read-only расписание выплат (на основе доступных данных).
- Созданы/обновлены тесты; есть артефакт `artifacts/issuer-payout-schedule.md`.

