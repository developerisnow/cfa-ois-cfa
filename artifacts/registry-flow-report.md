# Registry Order Flow Report

**Generated:** 2025-11-18  
**Task:** NX-04 Registry: create→reserve→paid flow + events (infra.defis.deploy, .NET 9)

## 1. REST Endpoints (OpenAPI ↔ Code)

**Spec:** `packages/contracts/openapi-registry.yaml`  
**Service:** `services/registry`

| OpenAPI Endpoint                | Method | Service Endpoint                      | Handler                                  | Status | Notes                                      |
|---------------------------------|--------|----------------------------------------|------------------------------------------|--------|--------------------------------------------|
| `/v1/orders`                   | POST   | `/v1/orders`                           | `Program.cs` → `RegistryService.PlaceOrderAsync` | ✅    | Idempotency-Key header required, maps to `IdemKey` |
| `/v1/orders/{id}`              | GET    | `/v1/orders/{id:guid}`                 | `RegistryService.GetOrderAsync`         | ✅    | Returns `OrderResponse` or 404             |
| `/v1/orders/{id}/cancel`       | POST   | `/v1/orders/{id:guid}/cancel`          | `RegistryService.CancelOrderAsync`      | ✅    | Cancels if not `paid/cancelled`           |
| `/v1/orders/{id}/mark-paid`    | POST   | `/v1/orders/{id:guid}/mark-paid`       | `RegistryService.MarkPaidAsync`         | ✅    | Marks `reserved` order as `paid`         |
| `/v1/wallets/{investorId}`     | GET    | `/v1/wallets/{investorId:guid}`        | `RegistryService.GetWalletAsync`        | ✅    | Returns wallet + holdings or 404           |
| `/v1/issuances/{id}/redeem`    | POST   | `/v1/issuances/{id:guid}/redeem`       | `RegistryService.RedeemAsync`           | ✅    | Redeem from holdings, writes tx           |

## 2. Core Flow: create → reserve → paid

**Service:** `RegistryService`  
**DB:** `RegistryDbContext` (`orders`, `wallets`, `holdings`, `tx`, `outbox_messages`)

### 2.1. PlaceOrderAsync

- Idempotency:
  - По заголовку `Idempotency-Key` (GUID) ищем order по `IdemKey`. Если найден → возвращаем существующий `OrderResponse`.
- Validation:
  - `IComplianceService.CheckKycAsync(investorId)`
  - `IComplianceService.CheckQualificationAsync(investorId, amount)`
- Create:
  - `OrderEntity` со статусом `"created"`, полями `InvestorId`, `IssuanceId`, `Amount`, `IdemKey`, `CreatedAt`/`UpdatedAt`.
- Events (Outbox):
  - `ois.order.created` (OrderCreatedPayload)
  - `ois.order.placed` (OrderPlacedPayload) — новое событие, отражающее факт приёма заказа.
- Reserve funds:
  - `IBankNominalService.ReserveFundsAsync(investorId, amount, idempotencyKey)`
  - Статус заказа → `"reserved"`.
- Event:
  - `ois.order.reserved` (OrderReservedPayload) c `bankTransferId`.

### 2.2. MarkPaidAsync

- Preconditions:
  - 404 если заказа нет.
  - Idempotent: если `Status == "paid"`, возвращаем текущий `OrderResponse`.
  - Если `Status != "reserved"` → InvalidOperationException (BadRequest на API уровне).
- Ledger transfer:
  - `_ledger.TransferAsync(null, order.InvestorId.ToString(), order.IssuanceId, order.Amount, ct)` → `txHash`.
  - При такте ошибки: оставляем статус `"reserved"`, заполняем `FailureReason`, исключение пробрасываем.
- Update:
  - `Status = "paid"`, `DltTxHash = txHash`, `ConfirmedAt = UtcNow`, `UpdatedAt = UtcNow`.
  - Получаем/создаём wallet (`GetOrCreateWalletAsync`), обновляем holding (`UpdateHoldingAsync`).
  - Пишем `TransactionEntity` (`WriteTransactionAsync`) с `type = "transfer"`, `DltTxHash = txHash`.
- Events (Outbox):
  - `ois.order.paid` (OrderPaidPayload) — факт оплаты.
  - `ois.order.confirmed` (OrderConfirmedPayload) — подтверждение с `dltTxHash` и `walletId`.
  - `ois.registry.transferred` (RegistryTransferredPayload) — факт перевода в реестре.

## 3. Events Mapping (AsyncAPI ↔ Code)

**Spec:** `packages/contracts/asyncapi.yaml`  
**Contracts:** `packages/domain/IntegrationEvents.cs`  
**Outbox publisher:** `services/registry/Background/OutboxPublisher.cs`

| Topic                  | Spec Payload           | Producer Code                                          | Outbox Publisher                           | Status |
|------------------------|------------------------|--------------------------------------------------------|--------------------------------------------|--------|
| `ois.order.created`    | OrderCreatedPayload    | `RegistryService.PlaceOrderAsync`                     | `OutboxPublisher` → `OrderCreated`         | ✅     |
| `ois.order.placed`     | OrderPlacedPayload     | `RegistryService.PlaceOrderAsync`                     | `OutboxPublisher` → `OrderPlaced`          | ✅     |
| `ois.order.reserved`   | OrderReservedPayload   | `RegistryService.PlaceOrderAsync` (после ReserveFunds)| `OutboxPublisher` → `OrderReserved`        | ✅     |
| `ois.order.paid`       | OrderPaidPayload       | `RegistryService.MarkPaidAsync`                       | `OutboxPublisher` → `OrderPaid`            | ✅     |
| `ois.order.confirmed`  | OrderConfirmedPayload  | `RegistryService.MarkPaidAsync`                       | `OutboxPublisher` → `OrderConfirmed`       | ✅     |
| `ois.registry.transferred` | RegistryTransferredPayload | `RegistryService.MarkPaidAsync`                   | `OutboxPublisher` → `RegistryTransferred`  | ✅     |

## 4. Tests

**Project:** `services/registry/registry.Tests/registry.Tests.csproj` (net9.0)

Запуск:

```bash
$HOME/.dotnet/dotnet test services/registry/registry.Tests/registry.Tests.csproj -v minimal
```

Результат:

- ✅ Всего 9 тестов пройдено, 0 упало, 0 пропущено.

Ключевые тесты для NX‑04:

- `OrderFlowTests.PlaceOrder_IsIdempotent_And_Reserved`:
  - Проверяет:
    - idempotency по `IdemKey` (r1.Id == r2.Id),
    - конечный статус `"reserved"`,
    - наличие `ois.order.created`, `ois.order.placed`, `ois.order.reserved` в `OutboxMessages`.
- `OrderFlowTests.MarkPaid_Moves_To_Paid_And_Writes_Tx`:
  - Проверяет:
    - статус `"paid"` и наличие `DltTxHash`,
    - запись в `Transactions` с `Status = "confirmed"`,
    - наличие `ois.order.paid`, `ois.order.confirmed`, `ois.registry.transferred` в outbox.
- `OrderFlowTests.MarkPaid_On_Ledger_Error_Stays_Reserved_And_Allows_Retry`:
  - Проверяет реакцию на ошибку леджера: заказ остаётся в `"reserved"`, Retry после починки леджера переводит в `"paid"`.

## 5. Settlement (OrderPaid consumption)

**Consumer:** `services/settlement/Consumers/OrderPaidEventConsumer.cs` / `OrderPaidConsumer.cs`  
**Spec:** `ois.order.paid` → `OrderPaidPayload`

- Консьюмеры в Settlement получают `OrderPaid` event и запускают логику расчёта/выплаты (подробности в `services/settlement/`), остаются совместимыми с обновлённым `OrderPaid` contract (поле `txHash` используется как идентификатор DLT‑транзакции).
- Дополнительно можно покрыть end‑to‑end сценарий (NX‑04+Settlement) через E2E/contract tests, но это выходит за рамки текущего NX‑04 re-check.

## 6. Notes

- После NX‑04 SPEC DIFF по `ois.order.placed` и `ois.order.confirmed` закрыт: продьюсеры реализованы, outbox → Kafka wiring есть.
- Открытые AsyncAPI gaps:
  - `ois.payout.scheduled` и `ois.transfer.completed` всё ещё не имеют продьюсеров — оставлены как TODO для последующих задач (см. Gap List в PROJECT-CONTEXT).

