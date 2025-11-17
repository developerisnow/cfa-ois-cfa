# NX-01 — Валидация спецификаций и API/Event Matrix

Цель: Провести линтинг и базовую валидацию OpenAPI/AsyncAPI/JSON Schemas, собрать проверенную матрицу API ↔ сервисы и событий ↔ продьюсер/консьюмер, зафиксировать результаты.

Входы
- OpenAPI/AsyncAPI/JSON Schema: `packages/contracts/*`
- Код сервисов: `services/*`
- Текущий контекст: `docs/context/PROJECT-CONTEXT.md`

Шаги
1. OpenAPI lint (Spectral) всех `packages/contracts/openapi-*.yaml`.
2. AsyncAPI CLI — синтаксис и базовая проверка `packages/contracts/asyncapi.yaml`.
3. AJV — валидация JSON Schemas (`packages/contracts/schemas/*.json`) на корректность и ссылки `$ref`.
4. Сверка топиков из AsyncAPI с фактическими продьюсерами/консьюмерами в коде (`services/*`).
5. Сверка основных endpoint из `openapi-gateway.yaml` с соответствующими сервисами (issuance/registry/...): наличие обработчиков/DTO.
6. Сформировать раздел "API/Event Matrix" в `docs/context/PROJECT-CONTEXT.md` (обновить/уточнить), вынести несовпадения в Gap List.

Команды (варианты)
- Spectral (Node): `npx -y @stoplight/spectral-cli@6 lint packages/contracts/openapi-*.yaml`
- Spectral (Docker): `docker run --rm -v "$PWD:/work" stoplight/spectral:6 lint /work/packages/contracts/openapi-*.yaml`
- AsyncAPI (Docker): `docker run --rm -v "$PWD:/work" asyncapi/cli:2.10.0 validate /work/packages/contracts/asyncapi.yaml`
- AJV (Node): `npx -y ajv-cli@5 compile -s packages/contracts/schemas/*.json`

Файлы/артефакты
- `artifacts/spec-lint-openapi.txt` — вывод Spectral.
- `artifacts/spec-validate-asyncapi.txt` — вывод AsyncAPI CLI.
- `artifacts/spec-validate-jsonschema.txt` — вывод AJV.
- Обновлённый `docs/context/PROJECT-CONTEXT.md` — разделы API/Event Matrix и Gap List.

Критерии приёмки
- Все указанные команды выполнены, отчёты сохранены в `artifacts/` и приложены к MR.
- Матрица API/Event отражает текущий код и спецификации; все расхождения перечислены как SPEC DIFF/TODO с точными путями.
- Нет блокирующих ошибок линтинга/валидации; предупреждения задокументированы.

