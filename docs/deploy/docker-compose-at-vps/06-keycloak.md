---
created: 2025-11-11 15:23
updated: 2025-11-11 15:23
type: runbook
sphere: [devops]
topic: [keycloak, oidc]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [keycloak, oidc]
---

# 06 — Keycloak (realm/clients)

Параметры
- [ ] URL (внутри compose сети): `http://keycloak:8080`
- [ ] URL (на хосте): `http://localhost:58080`
- [ ] Админ: `admin/admin123`
- [ ] Realm: `ois`

Бутстрап realm и клиентов (issuer, investor, backoffice)
- [ ] ```bash
  cd /opt/ois-cfa
  chmod +x ops/keycloak/bootstrap-realm.sh
  docker exec ois-keycloak bash -lc "bash -s" < ops/keycloak/bootstrap-realm.sh
  ```
- [ ] Скрипт создаёт клиентов с redirect URIs по публичным URL (редактируем переменные в начале при необходимости)
- [ ] Демо‑пользователи: `investor/Passw0rd!`, `issuer/Passw0rd!`, `backoffice/Passw0rd!`

Внешний доступ
- [ ] Если 58080 недоступен снаружи — это, вероятно, фаервол провайдера
- [ ] Временное решение: SSH‑туннель (см. 07 и 09)

