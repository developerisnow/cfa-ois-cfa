---
created: 2025-11-19 16:25
updated: 2025-11-19 16:25
type: skill
sphere: project
topic: context-composer
author: Codex CLI (co-76ca)
agentID: codex-cli-co-76ca
partAgentID: [co-76ca]
version: 0.1.0
tags: [skill, context, repomix, code2prompt, mcp]
---

# Skill: `context-composer-repomix-code2prompt` для ois-cfa

Назначение skill’а:
- Научить агента (Claude, Codex, Gemini), как **быстро и воспроизводимо собрать полезный контекст** по проекту `ois-cfa`.
- Использовать для этого **repomix** и **code2prompt** (через CLI или MCP), чтобы:
  - в 1–2 команды собрать осмысленный срез репозитория под конкретную задачу;
  - сохранять результат в `.txt` composer‑файлы для дальнейшего глубокого анализа (Gemini, GPT‑5, Claude, Oracle‑мост и т.п.).

Skill НЕ делает саму инженерию за человека — он даёт **готовые контекстные «пресеты»** и команды для их получения.

## Общие правила использования

Когда пользователь просит:
- «собери контекст для архитектуры/домена/деплоя/фронта/потоков NX»,
- «сделай composer‑файл для глубокой аналитики по X»,

агент должен:
1. Уточнить **цель** (что именно будет делать deep‑модель: ревью, рефакторинг, подготовка плана миграции, дебаг деплоя и т.п.).
2. Выбрать подходящий **кейс из пяти ниже** и следовать его инструкциям.
3. При необходимости адаптировать команды под текущую ветку/каталог, но **не менять суть**: включаем те же группы файлов.
4. Явно указать пользователю:
   - какие команды стоит выполнить в терминале;
   - какое имя у итогового `.txt` файла и где он лежит;
   - какие вопросы лучше всего задавать на этом контексте.

> Рабочий корень для CLI‑примеров ниже:  
> `/Users/user/__Repositories/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa`

---

## Кейс 1 — Архитектура и high‑level контекст

**Цель:** дать deep‑модели целостное понимание системы: домен ЦФА, архитектура, контекст, WBS, правила, high‑level roadmap и основные артефакты аудита.

**Основные директории/файлы:**
- `docs/context/` — `PROJECT-CONTEXT.md`, `FRONTEND-CONTEXT.md`, `RULES-SUMMARY.md`, `WBS-OIS.md`, `PROMPTS-MAP.md`.
- `docs/architecture/` — high‑level архитектура, C4, API/AsyncAPI/DFD/ontology/security/threat/uml.
- `audit/00_Executive_Summary.md`, `audit/01_Findings.md`, `audit/02_Recommendations.md`.

**Инструмент по умолчанию:** `repomix` (xml или markdown, с компрессией при больших объёмах).

**Рекомендуемый composer‑файл:**
- Путь: `memory-bank/composers/ois-cfa.architecture-context.repomix.txt` (можно создать каталог `composers/`, если его ещё нет).

**Пример команд:**

```bash
cd /Users/user/__Repositories/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa

# 1) High-level архитектурный контекст (docs/context + docs/architecture + ключевые части аудита)
repomix \
  --include "docs/context/**,docs/architecture/**,audit/00_Executive_Summary.md,audit/01_Findings.md,audit/02_Recommendations.md" \
  --style markdown \
  --compress \
  --output ../../../../memory-bank/composers/ois-cfa.architecture-context.repomix.txt
```

**Когда использовать этот кейс:**
- стартовая «подложка» для любого нового агента/модели;
- подготовка плана миграции/доработок;
- поиск архитектурных/доменных несоответствий между кодом и спецификациями.

---

## Кейс 2 — Домейн, контракты и цепочка (backend core)

**Цель:** дать модели плотный контекст по доменной логике, on‑chain компонентам и контрактам:
- доменные сущности и их связи;
- Hyperledger chaincode по issuance/registry;
- OpenAPI/AsyncAPI/JSON Schemas из `packages/contracts`.

**Основные директории/файлы:**
- `chaincode/issuance/**`, `chaincode/registry/**`.
- `packages/contracts/**` (особенно `openapi-*.yaml` и `schemas/*.json`).
- `packages/domain/**` (включая `domain.Tests`).

**Инструменты:**
- Для «сырой» упаковки + компрессии: `repomix`.
- Для генерации промпта под конкретный тип задачи (например, ревью доменной модели): `code2prompt`.

**Рекомендуемые composer‑файлы:**
- `memory-bank/composers/ois-cfa.domain-and-contracts.repomix.txt`
- `memory-bank/composers/ois-cfa.domain-and-contracts.code2prompt.md`

**Пример команд:**

```bash
cd /Users/user/__Repositories/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa

# 1) Домейн + контракты через repomix (сжатый контекст)
repomix \
  --include "chaincode/**,packages/contracts/**,packages/domain/**" \
  --style markdown \
  --compress \
  --output ../../../../memory-bank/composers/ois-cfa.domain-and-contracts.repomix.txt

# 2) Тот же срез через code2prompt (для кастомного шаблона ревью)
code2prompt \
  --include "chaincode/**,packages/contracts/**,packages/domain/**" \
  --exclude "*/bin/**,*/obj/**" \
  --tokens format \
  --output-file ../../../../memory-bank/composers/ois-cfa.domain-and-contracts.code2prompt.md \
  .
```

**Когда использовать этот кейс:**
- ревью соответствия доменной модели юридическим/бизнес‑правилам;
- поиск расхождений между chaincode, backend‑логикой и OpenAPI/JSON‑схемами;
- подготовка задачи на рефакторинг доменной модели, контрактов или тестов.

---

## Кейс 3 — Фронтенды и shared‑ui (порталы Issuer/Investor/Backoffice)

**Цель:** собрать единый фронтовый контекст, чтобы:
- понять, как порталы используют API/SDK;
- увидеть общие компоненты и UX‑паттерны (`shared-ui`);
- анализировать auth/role flows на фронте.

**Основные директории/файлы:**
- `apps/backoffice/**`
- `apps/portal-investor/**`
- `apps/portal-issuer/**`
- `apps/shared-ui/**`

**Инструмент по умолчанию:** `code2prompt` (markdown + `--full-directory-tree` для обзорной картины).

**Рекомендуемый composer‑файл:**
- `memory-bank/composers/ois-cfa.frontends-and-shared-ui.code2prompt.md`

**Пример команд:**

```bash
cd /Users/user/__Repositories/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa

code2prompt \
  --include "apps/backoffice/**,apps/portal-investor/**,apps/portal-issuer/**,apps/shared-ui/**" \
  --exclude "*/.next/**,*/node_modules/**,*/.turbo/**" \
  --full-directory-tree \
  --tokens format \
  --output-file ../../../../memory-bank/composers/ois-cfa.frontends-and-shared-ui.code2prompt.md \
  .
```

**Когда использовать этот кейс:**
- дизайн/ревью UI/UX сценариев (issuer/investor/backoffice);
- поиск фронтовых багов/несоответствий доменной модели;
- планирование рефакторинга общих компонентов или темизации.

---

## Кейс 4 — DevOps, deploy и эксплуатация

**Цель:** дать модели полный эксплуатационный контекст:
- docker‑compose, k8s/helm, keycloak bootstrap;
- runbooks и quickstart‑доки для окружений (`uk1`, `cfa1` и др.);
- CI/CD артефакты и скрипты.

**Основные директории/файлы:**
- `docker-compose*.yml`
- `docs/deploy/**`, `docs/dlt/**`, `docs/ops/**`
- `audit/09_Artifacts/**` (ci/helm/k8s/observability)
- `infra/**` (если есть), `scripts/**` связанные с деплоем/бэкапом/healthcheck.

**Инструмент по умолчанию:** `repomix` (markdown + включение git‑логов/диффов при необходимости).

**Рекомендуемый composer‑файл:**
- `memory-bank/composers/ois-cfa.devops-and-deploy.repomix.txt`

**Пример команд:**

```bash
cd /Users/user/__Repositories/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa

repomix \
  --include "docker-compose*.yml,docs/deploy/**,docs/dlt/**,docs/ops/**,audit/09_Artifacts/**,infra/**,scripts/**" \
  --style markdown \
  --include-logs \
  --include-logs-count 20 \
  --compress \
  --output ../../../../memory-bank/composers/ois-cfa.devops-and-deploy.repomix.txt
```

**Когда использовать этот кейс:**
- подготовка/обновление runbook’ов для новых окружений;
- разбор инцидентов деплоя/CI/CD;
- оптимизация observability (Prometheus/Grafana/OTel).

---

## Кейс 5 — End‑to‑end потоки NX‑задач (issuance/registry и др.)

**Цель:** собрать связанный контекст для NX‑тасков (особенно NX‑01..NX‑06):
- спецификации задач, отчёты, артефакты тестов и диагностики;
- карту C4/репо‑скан;
- связанные части кода/контрактов, nếu нужно.

**Основные источники:**
- Внутри текущего репо `ois-cfa`:
  - `docs/context/`, `docs/architecture/`, `docs/ops/**` (для отсылки).
- В соседнем рабочем треке `wt__ois-cfa__NX01` (репо‑папка в монорепо; при необходимости явно подсказать путь пользователю).
- Отчёты/ежедневники, перечисленные в `20251119-1109-gem3-brainstorm-merge-uk1-for-NX05-06.md`.

**Инструменты:**
- Для компактного snapshot’а по NX‑артефактам: `repomix` (по конкретным путям).
- Для гибкого промпта вокруг конкретной NX‑таски: `code2prompt` с `--include` по списку файлов.

**Рекомендуемые composer‑файлы (пример):**
- `memory-bank/composers/ois-cfa.nx-flows.snapshot.repomix.txt`
- `memory-bank/composers/ois-cfa.nx05-06.merge-uk1-scenario.code2prompt.md`

**Пример команд (для запуска из корня монорепо, если нужен кросс‑реповый контекст):**

```bash
# 1) Snapshot по NX‑артефактам (репорты + артефакты + контекст)
cd /Users/user/__Repositories/prj_Cifra-rwa-exachange-assets

repomix \
  --include "repositories/customer-gitlab/wt__ois-cfa__NX01/tasks/**,repositories/customer-gitlab/wt__ois-cfa__NX01/artifacts/**,repositories/customer-gitlab/wt__ois-cfa__NX01/docs/context/**" \
  --style markdown \
  --compress \
  --output memory-bank/composers/ois-cfa.nx-flows.snapshot.repomix.txt

# 2) Точный контекст для NX‑05/NX‑06 merge‑сценария (code2prompt)
code2prompt \
  --include "repositories/customer-gitlab/wt__ois-cfa__NX01/tasks/NX-05-issuer-dashboard-and-reports.md,repositories/customer-gitlab/wt__ois-cfa__NX01/tasks/NX-06-issuer-payout-schedule-spec-and-ui.md,repositories/customer-gitlab/wt__ois-cfa__NX01/artifacts/issuance-endpoints-coverage-report.md,repositories/customer-gitlab/wt__ois-cfa__NX01/artifacts/registry-flow-report.md" \
  --tokens format \
  --output-file memory-bank/composers/ois-cfa.nx05-06.merge-uk1-scenario.code2prompt.md \
  .
```

> Если агент работает **только внутри `ois-cfa` субмодуля**, он должен:
> - явно подсказать пользователю перейти в корень монорепо для запуска кросс‑реповых команд;  
> - либо ограничиться теми NX‑артефактами, которые уже смонтированы/симлинкнуты в текущем репо.

**Когда использовать этот кейс:**
- глубокий анализ прогресса по NX‑таскам;
- подготовка плана слияния веток/окружений (`uk1`, `cfa1`, future hosts);
- поиск inconsistency между спеками NX и фактическим кодом/деплоем.

---

## Как это связано с MCP и deep‑моделями

Агент, обладающий этим skill’ом, должен понимать следующий паттерн:

1. **Сбор контекста (CLI/MCP‑уровень):**
   - либо запускаем команды выше вручную в терминале;
   - либо просим агента использовать MCP‑серверы:
     - `repomix` (`npx repomix --mcp` — уже сконфигурирован в `.mcp.json` на уровне проекта);
     - `code2prompt` через MCP‑обёртку `LLMs-code2prompt-mcp`.
2. **Сохранение результата:**
   - итоговые `.txt`/`.md` файлы падают в `memory-bank/composers/` или другой согласованный каталог;
   - имена файлов отражают проект (`ois-cfa`), кейс и, при необходимости, дату/ветку.
3. **Глубокий анализ:**
   - эти composer‑файлы загружаются в deep‑thinking модель (Gemini, GPT‑5, Claude, Oracle‑мост) как **основной контекст**;
   - поверх них формулируется уже задача: ревью, план миграции, incident post‑mortem, рефакторинг и т.п.

Агент может:
- предлагать пользователю, **какой из 5 кейсов** выбрать под текущую задачу;
- объяснять, **какими командами собрать контекст**;
- напоминать, что **composer‑файлы переиспользуемы** — их лучше не перегенерировать без необходимости, а хранить как стабильные артефакты.

