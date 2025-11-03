# Registry Service

Сервис управления заказами, кошельками и операциями с ЦФА через реестр.

## Endpoints

### POST /v1/orders
Размещение заказа на покупку ЦФА.

**Headers:**
- `Idempotency-Key` (required, UUID) - Ключ идемпотентности

**Request:**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100000.00
}
```

**Response (202):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100000.00,
  "status": "confirmed",
  "walletId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "dltTxHash": "abc123...",
  "createdAt": "2025-01-01T10:00:00Z",
  "confirmedAt": "2025-01-01T10:00:05Z"
}
```

**Pipeline:**
1. Проверка идемпотентности (по `Idempotency-Key`)
2. KYC/Qualification проверка (stub - временно всегда OK)
3. Резервирование средств через bank-nominal (идемпотентно)
4. Вызов ledger `registry.Transfer(to=investor, issuanceId, amount)`
5. Создание/обновление wallet и holding
6. Запись транзакции и публикация события `ois.registry.transferred`

### GET /v1/orders/{id}
Получение информации о заказе.

**Response (200):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "confirmed",
  ...
}
```

### GET /v1/wallets/{investorId}
Получение портфеля инвестора.

**Response (200):**
```json
{
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "balance": 50000.00,
  "blocked": 10000.00,
  "holdings": [
    {
      "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 100.00,
      "updatedAt": "2025-01-01T10:00:00Z"
    }
  ]
}
```

### POST /v1/issuances/{id}/redeem
Погашение выпуска.

**Request:**
```json
{
  "amount": 50.00
}
```

**Response (200):**
```json
{
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "redeemedAmount": 50.00,
  "dltTxHash": "def456...",
  "redeemedAt": "2025-01-01T12:00:00Z"
}
```

## Database Schema

```sql
CREATE TABLE wallets (
    id UUID PRIMARY KEY,
    owner_type VARCHAR(50) NOT NULL,
    owner_id UUID NOT NULL,
    balance NUMERIC(20,8) NOT NULL,
    blocked NUMERIC(20,8) NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    UNIQUE(owner_type, owner_id)
);

CREATE TABLE holdings (
    id UUID PRIMARY KEY,
    investor_id UUID NOT NULL,
    issuance_id UUID NOT NULL,
    quantity NUMERIC(20,8) NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    UNIQUE(investor_id, issuance_id)
);

CREATE TABLE orders (
    id UUID PRIMARY KEY,
    investor_id UUID NOT NULL,
    issuance_id UUID NOT NULL,
    amount NUMERIC(20,8) NOT NULL,
    status VARCHAR(50) NOT NULL,
    idem_key VARCHAR(255) UNIQUE,
    wallet_id UUID,
    dlt_tx_hash VARCHAR(64),
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    confirmed_at TIMESTAMPTZ,
    failure_reason TEXT
);

CREATE TABLE tx (
    id UUID PRIMARY KEY,
    type VARCHAR(50) NOT NULL,
    from_wallet_id UUID,
    to_wallet_id UUID,
    issuance_id UUID,
    amount NUMERIC(20,8) NOT NULL,
    dlt_tx_hash VARCHAR(64),
    status VARCHAR(50) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL,
    confirmed_at TIMESTAMPTZ
);
```

## Events

### ois.registry.transferred
Публикуется после успешного перевода на ledger.

**Payload:**
```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "issuanceId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "investorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "amount": 100000.00,
  "txHash": "abc123...",
  "walletId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "transferredAt": "2025-01-01T10:00:05Z"
}
```

## Idempotency

Заказы защищены от дублирования через заголовок `Idempotency-Key`:
- Если заказ с таким ключом уже существует → возвращается существующий (202)
- Ключ хранится в поле `orders.idem_key` с уникальным индексом
- Второй идентичный запрос с тем же ключом → 202, без двойного списания

## Ledger Integration

Сервис интегрирован с Hyperledger Fabric через chaincode `registry`:

### Chaincode Methods

- `Transfer(from?, to, issuanceId, amount)` - Перевод ЦФА
  - `from` = null → issue (первичная эмиссия инвестору)
  - `from` != null → transfer (между держателями)
  
- `Redeem(holderId, issuanceId, amount)` - Погашение
- `GetHistory(issuanceId)` - История операций

### Ledger Adapter

- Mock mode (по умолчанию): генерирует mock txHash
- Real mode: подключение к HLF (TODO)

**Configuration:**
```json
{
  "Ledger": {
    "UseMock": true,
    "ChaincodeEndpoint": ""
  }
}
```

## Error Mapping

- KYC/Qualification failed → `InvalidOperationException` → 400 Bad Request
- Bank reserve failed → `InvalidOperationException` → 400 Bad Request
- Ledger error → `InvalidOperationException` → 400 Bad Request
- Duplicate idempotency key → возврат существующего заказа (202)
- Order not found → 404 Not Found
- Wallet not found → 404 Not Found

## Integration Services

### Compliance Service (Stub)
- `CheckKycAsync()` - Проверка KYC (временно всегда OK)
- `CheckQualificationAsync()` - Проверка квалификации (временно всегда OK)

### Bank Nominal Service
- `ReserveFundsAsync()` - Резервирование средств
- Поддержка идемпотентности через `Idempotency-Key`

## Observability

- **Logging**: Serilog JSON формат
- **Tracing**: OpenTelemetry (консольный экспортер)
- **Metrics**: Prometheus (через OTel)

## Transaction Flow

1. **Place Order:**
   ```
   Client → Registry → Compliance (KYC/Qual) → Bank (Reserve) → Ledger (Transfer) → DB → Outbox
   ```

2. **Logs:**
   ```
   [INFO] Funds reserved: transferId={id}, investor={id}, amount={amount}
   [INFO] Ledger Transfer successful: orderId={id}, txHash={hash}, duration={ms}ms
   [INFO] Order {orderId} confirmed with txHash {txHash}
   ```

