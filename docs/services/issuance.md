# Issuance Service

Сервис управления выпусками цифровых финансовых активов (ЦФА).

## Endpoints

### POST /v1/issuances
Создание черновика выпуска.

**Request:**
```json
{
  "assetId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 1000000.00,
  "nominal": 1000.00,
  "issueDate": "2025-01-01",
  "maturityDate": "2026-01-01",
  "scheduleJson": {
    "items": [
      {
        "date": "2025-06-01",
        "amount": 50000.00
      }
    ]
  }
}
```

**Response (201):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 1000000.00,
  "nominal": 1000.00,
  "issueDate": "2025-01-01",
  "maturityDate": "2026-01-01",
  "status": "draft",
  "scheduleJson": { ... },
  "createdAt": "2025-01-01T10:00:00Z",
  "updatedAt": "2025-01-01T10:00:00Z"
}
```

### GET /v1/issuances/{id}
Получение информации о выпуске.

**Response (200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 1000000.00,
  "nominal": 1000.00,
  "issueDate": "2025-01-01",
  "maturityDate": "2026-01-01",
  "status": "published",
  "publishedAt": "2025-01-02T10:00:00Z",
  ...
}
```

### POST /v1/issuances/{id}/publish
Публикация выпуска (перевод из draft в published).

**Response (200):** IssuanceResponse со статусом "published"

**Errors:**
- 400: Issuance not in draft status
- 404: Issuance not found

### POST /v1/issuances/{id}/close
Закрытие выпуска (перевод из published в closed).

**Response (200):** IssuanceResponse со статусом "closed"

**Errors:**
- 400: Issuance not in published status
- 404: Issuance not found

## Status Transitions

```
Draft → Published → Closed → Redeemed
```

- **Draft**: Черновик выпуска
- **Published**: Опубликован, доступен для покупки
- **Closed**: Закрыт для новых покупок
- **Redeemed**: Погашен

## Database Schema

```sql
CREATE TABLE issuances (
    id UUID PRIMARY KEY,
    asset_id UUID NOT NULL,
    issuer_id UUID NOT NULL,
    total_amount NUMERIC(20,8) NOT NULL,
    nominal NUMERIC(20,8) NOT NULL,
    issue_date DATE NOT NULL,
    maturity_date DATE NOT NULL,
    status VARCHAR(50) NOT NULL,
    schedule_json JSONB,
    dlt_tx_hash VARCHAR(64),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    published_at TIMESTAMPTZ,
    closed_at TIMESTAMPTZ
);

CREATE INDEX ix_issuances_asset_id ON issuances(asset_id);
CREATE INDEX ix_issuances_issuer_id ON issuances(issuer_id);
CREATE INDEX ix_issuances_status ON issuances(status);
```

## Events

### ois.issuance.published
Публикуется при переходе в статус Published.

**Payload:**
```json
{
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "assetId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 1000000.00,
  "schedule": { ... },
  "publishedAt": "2025-01-02T10:00:00Z"
}
```

### ois.issuance.closed
Публикуется при переходе в статус Closed.

**Payload:**
```json
{
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "closedAt": "2025-12-31T10:00:00Z"
}
```

## Ledger Integration

Сервис интегрирован с Hyperledger Fabric через chaincode `issuance`.

### Chaincode Methods

- `Issue(id, assetId, issuerId, totalAmount, nominal, issueDate, maturityDate, scheduleJSON)` - Создание выпуска на ledger
- `Close(id)` - Закрытие выпуска (изменение статуса на closed)
- `Get(id)` - Получение информации о выпуске

### Ledger Adapter

Реализован `ILedgerIssuance` адаптер с поддержкой mock-режима:

**Mock Mode** (по умолчанию):
- Активируется, если `Ledger:ChaincodeEndpoint` не указан или `Ledger:UseMock=true`
- Генерирует mock transaction hash
- Симулирует задержку сети
- Используется для разработки без подключения к HLF

**Real Mode**:
- Подключение к Hyperledger Fabric через указанный endpoint
- Вызовы chaincode методов через HTTP/gRPC (TODO: реализовать Fabric SDK)

### Configuration

```json
{
  "Ledger": {
    "UseMock": true,
    "ChaincodeEndpoint": "http://localhost:7051"  // Optional: HLF peer endpoint
  }
}
```

### Error Mapping

- Ledger недоступен → `InvalidOperationException` → 400 Bad Request (ProblemDetails)
- Issuance не найден на ledger → `InvalidOperationException` → 400 Bad Request
- Invalid status transition → `InvalidOperationException` → 400 Bad Request

### Transaction Hash & Logging

При каждой операции на ledger:
- Transaction hash сохраняется в `dlt_tx_hash` поле
- Логируются txHash и duration операции
- Формат лога: `Ledger {Operation} successful for {IssuanceId}: txHash={TxHash}, duration={Duration}ms`

**Пример логов:**
```
[INFO] Ledger Issue successful for 3fa85f64-5717-4562-b3fc-2c963f66afa6: txHash=abc123..., duration=52ms
[INFO] Published issuance 3fa85f64-5717-4562-b3fc-2c963f66afa6 with txHash abc123...
```

## Outbox Pattern

События публикуются через паттерн Outbox:
1. Событие сохраняется в таблицу `outbox_messages` в рамках той же транзакции
2. Отдельный процесс (пока не реализован) обрабатывает outbox и публикует в Kafka

**Schema:**
```sql
CREATE TABLE outbox_messages (
    id UUID PRIMARY KEY,
    topic VARCHAR(255) NOT NULL,
    payload JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    processed_at TIMESTAMPTZ
);
```

## Validation

- `totalAmount` и `nominal` должны быть > 0
- `maturityDate` должна быть после `issueDate`
- Все обязательные поля должны быть заполнены

## Observability

- **Logging**: Serilog (JSON формат)
- **Tracing**: OpenTelemetry (консольный экспортер)
- **Metrics**: Prometheus метрики (через OTel)

## Health Check

`GET /health` - проверка доступности сервиса и подключения к БД.

