# NX-08 — Backoffice: журнал аудита (Audit Log UI)

Цель: предоставить операторам backoffice удобный UI для просмотра журнала аудита (audit log), который уже формируется backend-сервисами, с возможностью фильтрации и поиска.

Контекст
- Общий контекст: `docs/context/PROJECT-CONTEXT.md`.
- Правила/качество: `docs/context/RULES-SUMMARY.md`.
- Backend события аудита: `packages/contracts/schemas/AuditEvent.json`, `packages/contracts/asyncapi.yaml`, `docs/services/compliance.md`.
- Frontend контекст backoffice: `docs/context/FRONTEND-CONTEXT.md`.

Скоуп
- В портале backoffice реализовать страницу `/audit`, отображающую аудит-события.
- Минимальные возможности:
  - фильтрация по типу события, пользователю, дате;
  - постраничная загрузка;
  - отображение ключевых полей события.

Шаги
1. **Разобрать модель AuditEvent и API-доступ**
   - Изучить `packages/contracts/schemas/AuditEvent.json` для структуры события.
   - Проверить, есть ли REST-эндпойнт для получения списка audit-событий в `packages/contracts/openapi-*`.
   - При отсутствии — предложить SPEC DIFF (например, `GET /v1/audit`) и сохранить в `tasks/` отдельным файлом.

2. **API-клиент для журнала аудита**
   - Добавить соответствующий метод в TS SDK (`packages/sdks/ts`), если его ещё нет.
   - Реализовать функцию запроса журнала в `apps/backoffice` с поддержкой фильтров и пагинации.

3. **UI `/audit`**
   - Таблица с полями: Timestamp, UserId/Subject, Action (eventType), Target (resource/aggregate), Result/Status.
   - Фильтры по диапазону дат, типу события, идентификатору пользователя.
   - Отображение подробной информации по клику (модальное окно или отдельный блок) с payload события.

4. **Тесты и артефакты**
   - Добавить компонентные тесты для таблицы и фильтров.
   - Е2Е-сценарий (если есть): логин в backoffice → переход на `/audit` → изменение фильтров.
   - Сохранить `artifacts/backoffice-audit-log.md` с описанием эндпойнтов, примерами payload и скриншотами.

Команды
- Frontend dev: `cd apps/backoffice && npm install && npm run dev`.
- Backend dev/test: команды для сборки и тестов compliance/registry/settlement по `BACKEND-BUILD-AND-TEST.md`.

Критерии приёмки
- Страница `/audit` отображает список событий аудита и позволяет фильтровать их по ключевым параметрам.
- Структура выводимых данных соответствует `AuditEvent.json`.
- API-вызовы согласованы со спецификациями или задокументированы SPEC DIFF.
- Создан артефакт `artifacts/backoffice-audit-log.md`.

