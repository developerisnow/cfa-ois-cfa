# Compliance Service

Сервис для KYC проверок, оценки квалификации инвесторов и управления жалобами.

## Endpoints

### POST /v1/compliance/kyc/check
Проверка KYC статуса для инвестора.

**Request:**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response (200):**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "pass",
  "checkedAt": "2025-01-15T10:00:00Z",
  "reason": null
}
```

**Status values:** `pass`, `fail`, `pending`, `review`

**Pipeline:**
1. Check watchlists (stub)
2. If match → set status to `fail`, emit `ois.compliance.flagged`
3. If pending → set to `pass` (demo)
4. Return result

### POST /v1/compliance/qualification/evaluate
Оценка квалификации инвестора для операции.

**Request:**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 50000.00
}
```

**Response (200):**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "tier": "qualified",
  "limit": 60000.00,
  "used": 10000.00,
  "allowed": true,
  "reason": null,
  "evaluatedAt": "2025-01-15T10:00:00Z"
}
```

**Tier values:** `unqualified`, `qualified`, `professional`

**Policy Engine:**
- Config-driven limits (appsettings.json)
- Default limits:
  - `unqualified`: no limit (not allowed)
  - `qualified`: 60,000 руб
  - `professional`: unlimited
- Evaluation: deterministic based on investor ID (demo)

**If limit exceeded:**
- `allowed: false`
- Emit `ois.compliance.flagged` with reason `qualification_exceeded`

### GET /v1/compliance/investors/{id}/status
Получение полного статуса compliance для инвестора.

**Response (200):**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "kyc": "pass",
  "qualificationTier": "qualified",
  "qualificationLimit": 60000.00,
  "qualificationUsed": 10000.00,
  "updatedAt": "2025-01-15T10:00:00Z"
}
```

### POST /v1/complaints
Создание жалобы.

**Headers (optional):**
- `Idempotency-Key` - UUID для предотвращения дубликатов

**Request:**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "category": "service",
  "text": "Slow response time on orders"
}
```

**Response (201):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "category": "service",
  "text": "Slow response time on orders",
  "status": "open",
  "slaDue": "2025-01-22T10:00:00Z",
  "createdAt": "2025-01-15T10:00:00Z",
  "resolvedAt": null
}
```

**Category values:** `fraud`, `service`, `technical`, `other`

**SLA:** 7 days from creation

### GET /v1/complaints/{id}
Получение информации о жалобе.

## Database Schema

```sql
CREATE TABLE investors_compliance (
    investor_id UUID PRIMARY KEY,
    kyc VARCHAR(50) NOT NULL,
    qualification_tier VARCHAR(50) NOT NULL,
    qual_limit NUMERIC(20,8),
    qual_used NUMERIC(20,8),
    updated_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE complaints (
    id UUID PRIMARY KEY,
    investor_id UUID,
    category VARCHAR(50) NOT NULL,
    text TEXT NOT NULL,
    status VARCHAR(50) NOT NULL,
    sla_due TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL,
    resolved_at TIMESTAMPTZ,
    idem_key VARCHAR(255) UNIQUE
);
```

## Events

### ois.compliance.flagged
Публикуется при обнаружении проблем compliance.

**Payload:**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "reason": "qualification_exceeded",
  "severity": "high",
  "flaggedAt": "2025-01-15T10:00:00Z",
  "details": {
    "used": 50000,
    "limit": 60000,
    "requested": 20000
  }
}
```

**Reason values:** `kyc_fail`, `qualification_exceeded`, `watchlist_match`, `manual_review`

**Severity values:** `low`, `medium`, `high`, `critical`

## Integration with Registry

Registry service вызывает compliance перед обработкой заказа:

1. `CheckKycAsync(investorId)` → если `status != "pass"` → order fails
2. `CheckQualificationAsync(investorId, amount)` → если `allowed = false` → order fails

**Error messages:**
- KYC: `"KYC check failed for investor {id}"`
- Qualification: `"Qualification check failed for investor {id}: limit exceeded or not qualified"`

## Watchlists Service (Stub)

- Deterministic demo: ~10% match rate based on investor ID
- If matched → KYC status set to `fail`, event emitted
- Real implementation would integrate with external watchlist providers

## Qualification Policy

**Config-driven** (appsettings.json):
```json
{
  "Qualification": {
    "Limits": {
      "unqualified": null,
      "qualified": 60000,
      "professional": null
    }
  }
}
```

**Evaluation logic** (demo):
- Deterministic based on investor ID last byte
- < 100: unqualified
- 100-199: qualified
- >= 200: professional

## Idempotency

- Complaints support optional `Idempotency-Key` header
- If duplicate key → returns existing complaint (201)

## Observability

- **Logging**: Serilog JSON формат
- **Tracing**: OpenTelemetry (консольный экспортер)
- **Metrics**: Prometheus (через OTel)

## Error Handling

- Invalid investor ID → 400 Bad Request
- Compliance check failures → propagated to registry service
- Watchlist match → automatic KYC fail + event

