---
created: 2025-11-18 13:40
updated: 2025-11-18 13:40
type: skill
sphere: project
topic: daily-report
author: Alex (co-3c63)
agentID: co-019a915c-3c63-7311-b21c-af448053d646
partAgentID: [co-3c63]
version: 0.1.0
tags: [skill, reporting, daily-report, enterprise]
---

# Skill: `daily-report` для ois-cfa

Назначение:
- Помочь разработчику быстро подготовить ежедневный отчёт в формате, который ожидает заказчик
  (\"кровавый enterprise\" стиль со структурой daily report).
- Формат вдохновлён файлами:
  - `ARCHIVE/examples-daily-reports/Daily_Report_—_YYYY_MM_DD_—_Developer_Name_.md`
  - `ARCHIVE/examples-daily-reports/Daily-Report-2025-11-15-Ivan-Petrov.md`

## Структура отчёта

Рекомендуемый формат Markdown:

```md
# Daily Report — YYYY-MM-DD — Developer_Name

## Context / Project
- Project: OIS-CFA
- Repo: `./ois-cfa`
- Branch: <текущая ветка> (например, infra.defis.deploy или feature/NX-03-issuance-tests)

## Worklog (What I did)
- [HH:MM–HH:MM] Краткое описание блока работы (файл(ы), сервис(ы), таск NX-0X или внешний тикет).
- [HH:MM–HH:MM] ...

## Deliverables / Results
- [x] Что именно готово (PR/MR, коммит, обновлённый doc, новый тест).
- [ ] Что в процессе (с ссылкой на ветку/таск).

## Issues / Risks / Blockers
- Кратко и по сути: что мешало, какие риски вижу, что надо от заказчика/команды.

## Plan for next day
- 1–3 конкретных пункта, привязанных к NX-таскам или тикетам.
```

## Как использовать skill

Когда пользователь просит:
- \"сформируй daily report\",
- \"сделай отчёт за сегодня\" и т.п.,

агент должен:
1. Уточнить дату и имя разработчика (если не очевидно).
2. Спросить, на какие NX-таски/MR/коммиты опираться (или сам собрать краткий список из контекста, если уже есть логи).
3. Сформировать отчёт строго по структуре выше:
   - не превращать его в эссе; максимум конкретики и ссылок.
4. При необходимости добавить ссылки на ветки/PR (короткими относительными путями).

Важно:
- Не писать лишней философии; daily-report — это фактология для менеджеров/заказчика.
- Если часть работы — исследование/аудит, выписать, какие выводы и артефакты появились (docs, diagrams, планы).

