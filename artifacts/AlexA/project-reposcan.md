
## 2. RepoScan JSON map (единый артефакт)

Ниже — конкретный пример `snapshots/reposcan/ois-cfa/infra.defis.deploy.reposcan.json`, совместимый с T/B/L‑моделью и NX‑схемой из aggregated‑talks, но заполненный **по текущему дереву `ois-cfa`**.

### 2.1 Короткая T/B/L‑таблица для ориентира

| Level  | В `ois-cfa` сейчас                                        | Примеры путей                                                                |
| ------ | --------------------------------------------------------- | ---------------------------------------------------------------------------- |
| Trunk  | Контракты, контекст, WBS, ключевые ops/runbook            | `docs/context/*`, `packages/contracts/*`, `packages/domain`, `docs/deploy/*` |
| Branch | Каркас сервисов/фронтов/infra, ветка `infra.defis.deploy` | `services/*`, `apps/*`, `ops/infra/*`, `ops/gitops/*`                        |
| Leaf   | Тесты, UI‑страницы, скрипты, NX‑таски                     | `tests/**`, `apps/*/src/**`, `ops/scripts/*`, `tasks/NX-*.md`                |

(Эта таблица синхронизирована с описанием в aggregated‑talks и AGENTS/trees‑leaves‑docs.)

эти C4‑диаграммы показывают **одну и ту же систему на четырёх масштабах**: от пользователей и внешних систем (C1), через контейнеры (C2), внутрь конкретных сервисов Issuance/Registry (C3) и до классов внутри `services/issuance` (C4). Они опираются на текущий snapshot `infra.defis.deploy`: реальные Program.cs, DbContext’ы, OpenAPI/AsyncAPI, фронтовый context‑map и WBS‑OIS, а не на старые диаграммы из `docs/architecture/c4` или reposcan 11‑го числа.

По сравнению с прежним 2025‑11‑11 репосканом это описание **синхронно с кодом и NX‑каноном**: домены ровно те же, что в WBS (issuance/registry/settlement/compliance/identity), Edge‑слой — текущий `apps/api-gateway` с YARP‑маршрутами из API/Event‑матрицы, сервисы — фактические .NET‑микросервисы с outbox→Kafka и адаптерами к Fabric/банку/ЕСИА. Старый JSON описывал “планируемую” структуру; новый `infra.defis.deploy.reposcan.json` — это конкретная карта дерева, доменных групп, тестов и NX‑тасков, плюс явная T/B/L‑классификация по тем правилам, которые вы с Алексеем обсуждали в aggregated‑talks.

Практически:

* для **планирования NX‑тасков** планировщик или человек берёт `logical.nx_index[NX-*]`, смотрит связанные `nodes` и `anchors` и сразу видит, куда лезть (например, NX‑03 → `services/issuance` + `tests/issuance.Tests` + `openapi-issuance.yaml`; NX‑04 → `services/registry` + AsyncAPI и e2e‑journeys инвестора);
* для **ревью архитектуры** можно идти “сверху вниз”: C1/C2 для внешних интеграций и границ системы, C3 для двух ключевых доменов, C4 для точечного аудита реализации (EF‑конфиг, outbox, ledger‑adapter);
* для **онбординга разработчиков** JSON‑карта и диаграммы дают быстрый маршрут: “всё по issuance” или “всё по registry” вытаскивается из `logical.domains[*]`, а T/B/L и anchors подсказывают, какие файлы трогать можно, какие — только через NX‑таски и review. Это именно тот “map of the territory”, которого не хватало в прошлой итерации.
