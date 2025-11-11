---
created: 2025-11-11 15:24
updated: 2025-11-11 15:24
type: runbook
sphere: [devops]
topic: [smoke, curl]
author: alex-a
agentID: co-3a68
partAgentID: [co-3a68]
version: 1.0.0
tags: [testing]
---

# 08 — Smoke‑тесты (через Gateway)

Health
- [ ] `curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55000/health` → 200
- [ ] `curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55001/health` → 200
- [ ] `curl -sS -o /dev/null -w "%{http_code}\n" http://localhost:55006/health` → 200

Прокси маршруты (без данных ожидаемо 404)
- [ ] `curl -i http://localhost:55000/v1/orders/$(uuidgen)` → 404
- [ ] `curl -i http://localhost:55000/v1/wallets/$(uuidgen)` → 404

Создание выпуска (как пример, после сидирования)
- [ ] ```bash
  cat > /tmp/issuance.json <<JSON
  {
    "assetId": "$(uuidgen)",
    "issuerId": "$(uuidgen)",
    "totalAmount": 1000000,
    "nominal": 1000,
    "issueDate": "2025-01-01",
    "maturityDate": "2026-01-01",
    "scheduleJson": {"coupons": []}
  }
  JSON
  curl -sS -H "Content-Type: application/json" -d @/tmp/issuance.json -i http://localhost:55000/v1/issuances
  ```

Примечание
- [ ] Для полного сквозного сценария потребуется сид‑данные и/или dev‑упрощения для compliance/bank‑nominal

