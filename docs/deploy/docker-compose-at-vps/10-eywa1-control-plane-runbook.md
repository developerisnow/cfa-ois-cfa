---
created: 2025-11-19 18:30
updated: 2025-11-19 18:30
type: runbook
sphere: [devops]
topic: [deploy, control-plane, vps]
author: alex-a
agentID: co-76ca
partAgentID: [co-76ca]
version: 0.1.0
tags: [eywa1, tmux, ssh, cloudflare, docker-compose]
---

# OIS‑CFA · Eywa1 Control Plane — Multi‑VPS Runbook

Цель: превратить `eywa1` в управляющий узел (Control Plane) для однотипного деплоя OIS‑CFA на VPS (`cfa1`, `fin2`, `germ1`, …), не ломая рабочий UK1.

## Scope
- Workspace: `eywa1`, директория проекта `~/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets`.
- Код: рабочий `worktree` `repositories/customer-gitlab/wt__ois-cfa__NX01` (ветка `tasks/NX-01-spec-validate-and-matrix` / `infra.defis.deploy`).
- Целевые узлы: `cfa1`, `fin2`, `germ1`, … (UK1 — только как референс, без изменений).
- DNS: Cloudflare для `*.cfa.llmneighbors.com`, `*.cfa{1,2,3}.llmneighbors.com`.

## TL;DR
- Eywa1 становится единым Control Plane: все операции — через SSH + `tmux` на целевых узлах, логи сохраняются в сессии `p-cfa`.
- На каждом VPS создаётся пользователь `user` с sudo, базовый стек (Docker, Node 20, tmux, nginx, postfix) и директория `/srv/cfa`.
- Новый деплой стандартизирован через два скрипта в `wt__ois-cfa__NX01`: `ops/scripts/deploy/provision-node.sh` (bootstrap) и `ops/scripts/deploy/deploy-node.sh` (деплой стека).
- UK1 остаётся каноническим демо‑стендом (см. `docs/deploy/20251113-cloudflare-ingress.md` и `docker-compose-at-vps/*`); на нём ничего не меняем без отдельного плана.
- Для NX‑05/06 рабочим окружением для кода считаем `wt__ois-cfa__NX01`, а для проверок UI/flows — новые VPS, развернутые по этому runbook’у.

## Architecture (High‑Level)

```mermaid
flowchart LR
  subgraph Eywa1["eywa1 · Control Plane"]
    Agent[CLI agent / user]
    Repo[prj_Cifra-rwa-exachange-assets\nwt__ois-cfa__NX01]
    CF[Cloudflare CLI/API]
    SSHKeys[SSH keys]
  end

  subgraph Nodes["Target VPS nodes"]
    subgraph Node1["cfa1"]
      User1[user (sudo)]
      Tmux1[tmux session\np-cfa]
      Stack1[OIS-CFA stack\nDocker + PM2]
    end
    subgraph Node2["fin2"]
      User2[user (sudo)]
      Tmux2[tmux session\np-cfa]
      Stack2[OIS-CFA stack\nDocker + PM2]
    end
  end

  Agent --> Repo
  Agent --> CF
  Agent --> SSHKeys

  SSHKeys --> User1
  SSHKeys --> User2

  Agent -->|provision-node.sh| Node1
  Agent -->|deploy-node.sh| Node1
  Agent -->|provision-node.sh| Node2
  Agent -->|deploy-node.sh| Node2
```

## Phases Overview

1. **Phase 0 — Rules & Safety**: фиксируем запреты и инварианты (UK1 не трогаем, только `user`, только `/srv/cfa`).
2. **Phase 1 — Prepare Eywa1**: убеждаемся, что есть SSH‑ключи, Cloudflare CLI, актуальный `wt__ois-cfa__NX01`.
3. **Phase 2 — Provision Node**: подготавливаем VPS (user, SSH, базовые пакеты, tmux, `/srv/cfa`, сессия `p-cfa`).
4. **Phase 3 — Deploy OIS‑CFA Stack**: клонируем `ois-cfa`, включаем docker‑compose, подключаем фронты через PM2 и nginx.
5. **Phase 4 — Verify & Handover**: health‑чеки, smoke‑тесты, фиксация итогового состояния в memory‑bank.

---

## Phase 0 — Rules & Safety

Why  
- Минимизировать риск сломать единственный рабочий стенд и превратить деплой в «необратимый эксперимент».

What  
- Описать инварианты, которые агент/инженер не имеет права нарушать.

How  
- Перед запуском любых скриптов проговорить/прописать ограничения.

Result  
- Любые действия по bootstrap/deploy выполняются только в рамках согласованных узлов и веток.

### Invariants
- **UK1 не трогаем** — используем только как эталон (см. `docs/deploy/20251113-cloudflare-ingress.md` и `docker-compose-at-vps/*`).
- **Только пользователь `user`** для приложений и деплоя (никаких сервисов от `root`).
- **Единый путь проекта**: `/srv/cfa` на всех узлах.
- **Все длительные команды** — только внутри `tmux`‑сессии `p-cfa` на целевом VPS.
- **Только зафиксированные ветки**: для деплоя используем `infra.defis.deploy` (или явно согласованную ветку), код для NX‑05/06 — из `wt__ois-cfa__NX01`.

---

## Phase 1 — Prepare Eywa1 (Control Plane)

Why  
- Без корректного состояния на `eywa1` невозможен безопасный bootstrap узлов.

What  
- Проверяем наличие SSH‑ключей, Cloudflare‑кредов, актуальной копии репо и утилит.

How  
- Ручной чеклист + точечные команды.

Result  
- `eywa1` готов отдавать команды на `cfa1`/`fin2` и управлять DNS.

### Checklist
- [ ] SSH‑ключ `~/.ssh/id_rsa` (или другой) привязан к `root`/`user` на целевых VPS (`ssh cfa1`, `ssh fin2` работает без пароля).
- [ ] В `~/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets` актуален `git pull` для монорепо.
- [ ] В `repositories/customer-gitlab/wt__ois-cfa__NX01` подтянуты последние изменения ветки `infra.defis.deploy` / `tasks/NX-01-spec-...`.
- [ ] На `eywa1` установлен Cloudflare CLI или подготовлен `curl` + `.env` с `CLOUDFLARE_API_TOKEN` (см. `docs/deploy/20251113-cloudflare-ingress.md`).
- [ ] Установлены базовые утилиты: `jq`, `tmux`, `git`, `docker` (на `eywa1` только если нужно).

---

## Phase 2 — Provision Node (Bootstrap VPS)

Why  
- Нужно стандартизировать базовый образ сервера, чтобы все последующие шаги не превращались в «каждый VPS по‑своему».

What  
- Скрипт `ops/scripts/deploy/provision-node.sh` выполняет bootstrap: пользователь `user`, пакеты, Docker, Node, tmux, `p-cfa`.

How  
- Запускаем скрипт с `eywa1`, который через SSH конфигурирует целевой VPS.

Result  
- На `cfa1`/`fin2` есть готовый «пустой, но правильный» хост для деплоя OIS‑CFA.

### Minimal usage

```bash
cd ~/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/wt__ois-cfa__NX01
ops/scripts/deploy/provision-node.sh cfa1
ops/scripts/deploy/provision-node.sh fin2
```

### What it standardizes
- Создаёт (если нет) пользователя `user` с группой `sudo`.
- Настраивает базовые пакеты: `curl`, `git`, `tmux`, `jq`, `ufw`, `docker`, `docker-compose-plugin`, `nginx`, `postfix`.
- Устанавливает Node.js 20 для `user` (через `nvm` или системный пакет, зависит от окружения).
- Создаёт `/srv/cfa`, назначает владельца `user:user`.
- Добавляет в `~user/.tmux.conf` параметр `set-option -g history-limit 1000000`.
- Создаёт (если нет) `tmux`‑сессию `p-cfa`, рабочий каталог `/srv/cfa`.

---

## Phase 3 — Deploy OIS‑CFA Stack

Why  
- Поддерживать один сценарий деплоя для всех узлов, чтобы не плодить «уникальные» стенды.

What  
- Скрипт `ops/scripts/deploy/deploy-node.sh` клонирует `ois-cfa`, подтягивает ветку и запускает docker‑compose + фронты.

How  
- Из `eywa1` вызываем скрипт для выбранного узла; скрипт отправляет команды в `tmux`‑сессию `p-cfa` на VPS.

Result  
- На целевом VPS развёрнут стек OIS‑CFA, готовый к smoke‑тестам NX‑05/06.

### Minimal usage

```bash
cd ~/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/wt__ois-cfa__NX01
OIS_CFA_BRANCH=infra.defis.deploy ops/scripts/deploy/deploy-node.sh cfa1
OIS_CFA_BRANCH=infra.defis.deploy ops/scripts/deploy/deploy-node.sh fin2
```

### Notes
- По умолчанию используется репозиторий `git@git.telex.global:npk/ois-cfa.git` и ветка `infra.defis.deploy` (можно переопределить переменными окружения).
- Скрипт **не заполняет секреты** — `.env`‑файлы должны быть подготовлены отдельно (см. `docs/deploy/docker-compose-at-vps/02-env-and-compose.md`).
- Для фронтендов (Next.js) скрипт создаёт базовый scaffold команд в `tmux`, но конкретные `pm2`‑процессы и `env.local` лучше выровнять по UK1 (см. сессии `co-3dd7` и `20251113-cloudflare-ingress.md`).

---

## Phase 4 — Verify & Handover

Why  
- Без чётких health‑чеков легко получить «оно где‑то крутится, но не факт, что работает».

What  
- Проверяем `/health` и ключевые UI‑флоу, фиксируем результаты и риски.

How  
- Команды curl, базовый UI walkthrough, при возможности — Playwright e2e.

Result  
- Описанный, воспроизводимый стенд с понятным статусом готовности.

### Basic health
- [ ] `curl http://<host>:55000/health` (gateway) → `200 OK`.
- [ ] `curl http://<host>:55005/health` (issuance), `55006` (registry), `55007` (settlement), `55008` (compliance).
- [ ] Keycloak доступен по HTTP (до настройки ingress/TLS).

### Smoke‑flows (минимум)
- [ ] Issuer портал открывается (локальный порт или домен через nginx/Cloudflare).
- [ ] Базовый issuance‑flow (создание простого выпуска без сложного payout schedule).
- [ ] Просмотр отчётности `/reports` (после NX‑05 implementation).

### Memory‑bank / Logging
- [ ] Создан файл в `memory-bank/Scrum/<date>/<timestamp>-<host>-bootstrap.session.md` с кратким логом команд и ссылками на tmux‑сессии.

---

## Definition of Done (DoD)

- [ ] На `eywa1` зафиксирован этот runbook и скрипты в `ops/scripts/deploy`.
- [ ] Минимум один новый узел (`cfa1` или `fin2`) успешно прошёл Phases 2–4.
- [ ] Состояние UK1 **не изменено** в процессе работ.
- [ ] Для каждого узла есть:
  - [ ] DNS‑записи в Cloudflare (`*.cfa{N}.llmneighbors.com`) с документированными IP.
  - [ ] Описание портов/URL и health‑команд в таблице (отдельный раздел или memory‑bank).
- [ ] Для NX‑05/06 зафиксировано, на каких стендах проводится тестирование (uk1 vs cfa1/fin2).

---

## Agent Kickoff Prompt (Codex/Claude/Gemini)

Ниже — заготовка промпта для CLI‑агента, заточенная под этот runbook и `wt__ois-cfa__NX01`. Её можно адаптировать под конкретную задачу (например, «подними cfa1» или «подготовь fin2 для NX‑05/06»).

```text
You are a DevOps/Backend engineer working on the OIS-CFA project.

Workspace:
- You are running on host "eywa1".
- Local repo: ~/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets
- Main backend worktree: repositories/customer-gitlab/wt__ois-cfa__NX01 (branch tasks/NX-01-spec-validate-and-matrix / infra.defis.deploy)

Goal:
- Provision and deploy the OIS-CFA stack to a target VPS (e.g. cfa1 or fin2)
  using the "Eywa1 Control Plane" approach described in:
  - docs/deploy/docker-compose-at-vps/00-overview.md
  - docs/deploy/docker-compose-at-vps/10-eywa1-control-plane-runbook.md

Hard rules:
- DO NOT touch or modify the existing UK1 environment (prod demo).
- Use only the "user" account with sudo on target nodes (no root-only services).
- Use /srv/cfa as the project root on all nodes.
- Run all long-running commands inside tmux session "p-cfa" on the target node.
- For code changes and NX-05/06 work, treat wt__ois-cfa__NX01 as the primary tree.

Phases:
1) Phase 1 – validate Eywa1: SSH keys, Cloudflare token, local repo state.
2) Phase 2 – run ops/scripts/deploy/provision-node.sh <host> to bootstrap the node.
3) Phase 3 – run ops/scripts/deploy/deploy-node.sh <host> with the correct branch.
4) Phase 4 – run health checks (/health, Keycloak, portals) and record results in memory-bank.

Output expectations:
- Follow Why → What → How → Result structure.
- Use numbered steps and small tables where helpful.
- Explicitly call out any deviations from the runbook (SPEC DIFF-like).
```

