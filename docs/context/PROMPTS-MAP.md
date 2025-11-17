# PROMPTS-MAP — Карта мастер-промтов

## 1. MASTER-CONTEXT-OIS (.cursor/promts/MASTER-CONTEXT-OIS.md)
- Роль: Codex-Architect-OIS (principal architect & delivery lead).
- Цель: собрать единый контекст ОИС (спеки, код, артефакты, задачи) и сформировать план работ + новые задачи для Cursor-агентов.
- Основные выходы:
  - `docs/context/PROJECT-CONTEXT.md` — данный файл и связанные контекстные материалы.
  - `docs/context/WBS-OIS.md` — иерархия работ (WBS) по фронту/бэку/инфре.
  - `tasks/NX-**` — конкретные задачи для следующего этапа.

## 2. BACKEND-OIS (.cursor/promts/BACKEND-OIS.md)
- Роль: Codex-Backend-OIS (principal backend engineer).
- Фокус:
  - SPEC-FIRST & TEST-FIRST backend-реализация по OpenAPI/AsyncAPI/JSON Schemas.
  - Вертикали MVP: Investor buy flow, issuer payouts/redemption, compliance KYC/audit.
  - Чёткие acceptance criteria для backend сервисов и e2e-потоков.

## 3. BACKEND-BUILD-AND-TEST (.cursor/promts/BACKEND-BUILD-AND-TEST.md)
- Роль: Codex-Backend-Build-OIS (principal .NET engineer & CI sherpa).
- Фокус:
  - Стандартизированные команды сборки/тестов/миграций (`dotnet build/test`, Makefile, скрипты в `tools/`).
- Ожидаемые артефакты:
  - JUnit/TRX отчёты тестов, coverage-отчёты, smoke-health скрипты.

## 4. CURSOR RULES (main-promt.mdc, develop-promt.mdc)
- Роль: Codex-CTO-OIS / старший архитектор.
- Фокус:
  - Регуляторный и архитектурный контекст (259-ФЗ, 746-П, 5625-У, ГОСТ 57580, СТО БР).
  - Требование к документам (legal/architecture/security/testing) и чек-листам.

## 5. Использование этой карты
- При постановке задач ссылаться на конкретные промпты:
  - Backend-реализация: «см. BACKEND-OIS».
  - Сборка и тесты: «см. BACKEND-BUILD-AND-TEST».
  - Архитектурные/регуляторные требования: «см. main-promt.mdc и develop-promt.mdc».
- Это позволяет разработчику и ИИ-агенту быстро понять контекст и ожидаемый уровень качества.

