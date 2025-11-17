# NX-07 — Backoffice: полный KYC-флоу и реестр пользователей

Цель: реализовать в backoffice полноценный KYC-workflow (просмотр заявок, approve/reject, статусы) и базовый реестр пользователей, опираясь на существующие backend-сервисы Compliance/Identity.

Контекст
- Общий контекст: `docs/context/PROJECT-CONTEXT.md`.
- Frontend контекст: `docs/context/FRONTEND-CONTEXT.md` (раздел про Backoffice).
- Backend контекст: `docs/services/compliance.md`, `docs/services/registry.md`, `packages/contracts/openapi-compliance.yaml` (если есть).
- Регуляторные ожидания по реестру: `docs/legal/07-Реестр-пользователей.md`.

Скоуп
- В портале backoffice (`apps/backoffice`) страница `/kyc` должна позволять:
  - видеть список KYC-заявок со статусами;
  - открывать детали заявки;
  - выносить решение approve/reject с комментарием.
- Добавить простую страницу реестра пользователей (таблица с фильтрами/поиском) на основе данных backend.

Шаги
1. **Уточнить backend-контракты KYC и пользователей**
   - Изучить спецификации в `packages/contracts` и документацию `docs/services/compliance.md`, `docs/services/registry.md`.
   - Проверить, какие эндпойнты уже есть (например, получение KYC-статуса, списка документов, принятие решения).
   - При отсутствии необходимых рест-эндпойнтов — подготовить SPEC DIFF (отдельный `.md` файл в `tasks/`).

2. **API-клиент для KYC и реестра пользователей**
   - В TS SDK (`packages/sdks/ts`) добавить/проверить методы для:
     - получения списка KYC-заявок;
     - принятия решения (approve/reject);
     - получения списка пользователей с основными полями (Id, Email, Role, Status, CreatedAt).
   - Прокинуть эти методы в `apps/backoffice` через слой API-клиента.

3. **UI `/kyc`**
   - Список заявок (таблица) с колонками: InvestorId, статус, дата создания, дата решения.
   - Возможность кликнуть по заявке и открыть детали (поля инвестора, документы, флаги AML, если доступны).
   - Кнопки "Approve"/"Reject" с подтверждением и optional-комментарием.
   - Отображение результата (toast/notification) и обновление списка.

4. **UI "Реестр пользователей"**
   - Создать страницу (например, `/users`) в `apps/backoffice`.
   - Таблица с полями: UserId, Email, Role(s), Status, CreatedAt.
   - Фильтры по роли/статусу, поиск по Email.
   - Ссылка из основного меню backoffice на `/users`.

5. **Тесты и артефакты**
   - Добавить компонентные тесты для таблиц/форм KYC и списка пользователей.
   - При наличии е2е — добавить сценарий: логин backoffice → `/kyc` (approve заявки) → `/users` (проверить обновлённый статус).
   - Сохранить `artifacts/backoffice-kyc-and-users.md` с описанием использованных эндпойнтов и скриншотами/результатами.

Команды
- Frontend dev:
  - `cd apps/backoffice`
  - `npm install`
  - `npm run dev`
- Backend dev/test: `dotnet build`, `dotnet test` по сервисам compliance/identity/registry согласно `BACKEND-BUILD-AND-TEST.md`.

Критерии приёмки
- Страница `/kyc` позволяет просматривать заявки и выносить решение, эти действия отражаются в backend.
- Страница `/users` отображает корректный список пользователей и позволяет фильтровать/искать.
- Все вызовы API соответствуют спецификациям; для отсутствующих контрактов есть SPEC DIFF.
- Создан артефакт `artifacts/backoffice-kyc-and-users.md`.

