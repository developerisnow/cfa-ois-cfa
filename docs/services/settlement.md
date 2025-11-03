# Settlement Service

Сервис для batch payouts и reconciliation выплат по ЦФА.

## Endpoints

### POST /v1/settlement/run
Запуск settlement для указанной даты.

**Query Parameters:**
- `date` (optional, YYYY-MM-DD) - Дата для settlement. По умолчанию - сегодня.

**Response (202):**
```json
{
  "batchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "runDate": "2025-01-15",
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 500000.00,
  "status": "completed",
  "itemCount": 10,
  "createdAt": "2025-01-15T10:00:00Z"
}
```

**Pipeline:**
1. (a) Find due issuances & holders from registry
2. (b) Build batch with deterministic itemIds; set idem_key
3. (c) Call bank-nominal: payout batch (idempotent); get bank_refs
4. (d) Mark on ledger via registry (record payout tx)
5. (e) Emit `ois.payout.executed`; write reconciliation log

### GET /v1/reports/payouts
Получение отчёта по выплатам за период.

**Query Parameters:**
- `from` (optional, YYYY-MM-DD) - Начальная дата. По умолчанию - 30 дней назад.
- `to` (optional, YYYY-MM-DD) - Конечная дата. По умолчанию - сегодня.

**Response (200):**
```json
{
  "from": "2025-01-01",
  "to": "2025-01-31",
  "totalBatches": 5,
  "totalAmount": 2500000.00,
  "totalItems": 50,
  "completedItems": 48,
  "failedItems": 2,
  "batches": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "runDate": "2025-01-15",
      "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "totalAmount": 500000.00,
      "status": "completed",
      "itemCount": 10,
      "completedCount": 10,
      "failedCount": 0,
      "createdAt": "2025-01-15T10:00:00Z"
    }
  ]
}
```

## Database Schema

```sql
CREATE TABLE payouts_batch (
    id UUID PRIMARY KEY,
    run_date DATE NOT NULL,
    issuance_id UUID,
    total_amount NUMERIC(20,8) NOT NULL,
    status VARCHAR(50) NOT NULL,
    idem_key VARCHAR(255) UNIQUE,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE payouts_item (
    id UUID PRIMARY KEY,
    batch_id UUID NOT NULL REFERENCES payouts_batch(id) ON DELETE CASCADE,
    investor_id UUID NOT NULL,
    amount NUMERIC(20,8) NOT NULL,
    bank_ref VARCHAR(255),
    dlt_tx_hash VARCHAR(64),
    status VARCHAR(50) NOT NULL,
    failure_reason TEXT,
    created_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE reconciliation_log (
    id UUID PRIMARY KEY,
    batch_id UUID NOT NULL,
    payload_json JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL
);
```

## Events

### ois.payout.executed
Публикуется после успешного выполнения batch payout.

**Payload:**
```json
{
  "batchId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "executedAt": "2025-01-15T10:00:05Z",
  "totalAmount": 500000.00,
  "itemCount": 10,
  "items": [
    {
      "itemId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "amount": 50000.00,
      "status": "completed",
      "bankRef": "PAY-...",
      "dltTxHash": "abc123...",
      "failureReason": null
    }
  ]
}
```

## Idempotency

Settlement защищён от дублирования через `idem_key = "settlement-{runDate:yyyy-MM-dd}"`:
- Если batch с таким ключом уже существует → возвращается существующий (202)
- Повторный запуск с той же датой → idempotent, no duplicates

## Sequence Diagram

```
Client → Settlement Service
  → (a) Query Registry for holdings by issuance
  → (b) Query Issuance for schedule
  → (c) Calculate due amounts per holder
  → (d) Create batch with deterministic item IDs
  → (e) Call Bank Nominal: POST /nominal/payouts/batch
  → (f) Update items with bank_refs
  → (g) Mark on ledger (via Registry)
  → (h) Emit ois.payout.executed
  → (i) Write reconciliation log
```

## Error Mapping

- No issuances due → `InvalidOperationException` → 400 Bad Request
- Bank payout failed → `InvalidOperationException` → 400 Bad Request
- Invalid date range → 400 Bad Request
- Duplicate idempotency key → возврат существующего batch (202)

## Integration Services

### Registry Client
- `GetHoldingsByIssuanceAsync()` - Получение держателей по issuance

### Issuance Client
- `GetIssuanceAsync()` - Получение информации о выпуске (включая schedule)

### Bank Nominal Client
- `ExecuteBatchPayoutAsync()` - Выполнение batch payout (идемпотентно)

## Deterministic ID Generation

Item IDs генерируются детерминированно на основе:
- `batchId`
- `investorId`
- `issuanceId`

Формула: `SHA256(batchId:investorId:issuanceId).Take(16)` → Guid

Это гарантирует, что при повторном запуске для той же даты items будут иметь те же IDs.

## Performance

- K6 load test: `/v1/reports/payouts` → p95 < 300ms @ 100 RPS
- Target: 100 RPS, error rate < 1%

## Observability

- **Logging**: Serilog JSON формат
- **Tracing**: OpenTelemetry (консольный экспортер)
- **Metrics**: Prometheus (через OTel)

## Reconciliation

После выполнения batch payout записывается reconciliation log:
- `payload_json` содержит batchRef и статусы всех items
- Используется для сверки с банковскими выписками

