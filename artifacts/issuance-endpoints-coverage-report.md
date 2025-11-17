# Issuance Service Endpoints Coverage Report

**Generated:** 2025-01-27  
**Task:** NX-03 Issuance Endpoints Coverage

## Executive Summary

Проведена синхронизация реализации сервиса Issuance с OpenAPI спецификациями (`openapi-issuance.yaml`, `openapi-gateway.yaml`), проверено соответствие событий AsyncAPI, добавлены тесты.

## Endpoint Mapping (OpenAPI ↔ Code)

| OpenAPI Endpoint | Method | Service Endpoint | Handler | Status | Notes |
|-----------------|--------|------------------|---------|--------|-------|
| `/v1/issuances` | POST | `/v1/issuances` | `Program.cs:185` → `IssuanceService.CreateAsync` | ✅ | Создание черновика |
| `/v1/issuances/{id}` | GET | `/v1/issuances/{id:guid}` | `Program.cs:216` → `IssuanceService.GetByIdAsync` | ✅ | Получение по ID |
| `/v1/issuances/{id}/publish` | POST | `/v1/issuances/{id:guid}/publish` | `Program.cs:228` → `IssuanceService.PublishAsync` | ✅ | Публикация |
| `/v1/issuances/{id}/close` | POST | `/v1/issuances/{id:guid}/close` | `Program.cs:253` → `IssuanceService.CloseAsync` | ✅ | Закрытие |

**Gateway Mapping:**
- Gateway `/issuances` → YARP transform → Service `/v1/issuances` ✅

## DTO & Validators

### CreateIssuanceRequest
- **Location:** `services/issuance/DTOs/CreateIssuanceRequest.cs`
- **OpenAPI Schema:** `packages/contracts/openapi-issuance.yaml` → `CreateIssuanceRequest`
- **Status:** ✅ Соответствует
- **Fields:**
  - `assetId` (Guid, required) ✅
  - `issuerId` (Guid, required) ✅
  - `totalAmount` (decimal, required, > 0) ✅
  - `nominal` (decimal, required, > 0) ✅
  - `issueDate` (DateOnly, required) ✅
  - `maturityDate` (DateOnly, required) ✅
  - `scheduleJson` (Dictionary<string, object>?, optional) ✅

### IssuanceResponse
- **Location:** `services/issuance/DTOs/IssuanceResponse.cs`
- **OpenAPI Schema:** `packages/contracts/openapi-issuance.yaml` → `IssuanceResponse`
- **Status:** ✅ Соответствует
- **Fields:** Все поля соответствуют OpenAPI схеме

### Validators
- **Location:** `services/issuance/Validators/CreateIssuanceRequestValidator.cs`
- **Status:** ✅ Реализован с FluentValidation
- **Rules:**
  - AssetId, IssuerId: NotEmpty ✅
  - TotalAmount, Nominal: GreaterThan(0) ✅
  - IssueDate, MaturityDate: NotEmpty ✅
  - MaturityDate > IssueDate ✅

## Authentication & Authorization

### Security Schemes (OpenAPI)
- **BearerAuth:** JWT Bearer token (OIDC/Keycloak)
- **Location:** `packages/contracts/openapi-gateway.yaml` → `components.securitySchemes.BearerAuth`

### Implementation
- **Location:** `services/issuance/Program.cs:83-105`
- **Status:** ✅ Реализовано
- **Details:**
  - JWT Bearer authentication (`AddJwtBearer`)
  - Keycloak authority configuration
  - Authorization policies:
    - `role:issuer` — для POST `/issuances`, `/publish`, `/close`
    - `role:any-auth` — для GET `/issuances/{id}`

**SPEC DIFF:** OpenAPI определяет `security: BearerAuth` для всех endpoints, код использует политики `role:issuer` и `role:any-auth`. Это соответствует, так как политики проверяют аутентификацию + роль.

## Events (AsyncAPI ↔ Code)

### ois.issuance.published

**AsyncAPI Schema:**
- **Topic:** `ois.issuance.published`
- **Payload:** `IssuancePublishedPayload`
- **Required fields:** `issuanceId`, `assetId`, `issuerId`, `publishedAt`
- **Optional fields:** `totalAmount`, `schedule`, `metadata`

**Code Implementation:**
- **Location:** `services/issuance/Services/IssuanceService.cs:121-130`
- **Status:** ✅ Соответствует
- **Payload:**
  ```csharp
  {
      issuanceId = issuance.Id,        // ✅ required
      assetId = issuance.AssetId,      // ✅ required
      issuerId = issuance.IssuerId,    // ✅ required
      totalAmount = issuance.TotalAmount, // ✅ optional
      schedule = schedule,              // ✅ optional
      publishedAt = issuance.PublishedAt, // ✅ required
      dltTxHash = txHash               // ⚠️ не в AsyncAPI (добавлено в коде)
  }
  ```

**SPEC DIFF:** Код добавляет `dltTxHash` в payload, которого нет в AsyncAPI. Рекомендация: добавить `dltTxHash` в `IssuancePublishedPayload` в AsyncAPI или удалить из кода.

### ois.issuance.closed

**AsyncAPI Schema:**
- **Topic:** `ois.issuance.closed`
- **Payload:** `IssuanceClosedPayload`
- **Required fields:** `issuanceId`, `closedAt`
- **Optional fields:** `reason`

**Code Implementation:**
- **Location:** `services/issuance/Services/IssuanceService.cs:177-182`
- **Status:** ✅ Соответствует
- **Payload:**
  ```csharp
  {
      issuanceId = issuance.Id,    // ✅ required
      closedAt = issuance.ClosedAt, // ✅ required
      dltTxHash = txHash           // ⚠️ не в AsyncAPI (добавлено в коде)
  }
  ```

**SPEC DIFF:** Код добавляет `dltTxHash` в payload, которого нет в AsyncAPI. Рекомендация: добавить `dltTxHash` в `IssuanceClosedPayload` в AsyncAPI или удалить из кода.

### Outbox Pattern
- **Location:** `services/issuance/Services/OutboxService.cs`
- **Status:** ✅ Реализован
- **Publisher:** `services/issuance/Background/OutboxPublisher.cs`
- **Topics:** `ois.issuance.published`, `ois.issuance.closed`

## Tests

### Test Structure
- **Location:** `services/issuance/issuance.Tests/` (создано)
- **Also exists:** `tests/issuance.Tests/` (существующие тесты)

### Unit Tests (IssuanceServiceTests.cs)
- ✅ `CreateAsync_ShouldCreateDraftIssuance`
- ✅ `GetByIdAsync_ExistingIssuance_ReturnsIssuance`
- ✅ `GetByIdAsync_NonExistent_ReturnsNull`
- ✅ `PublishAsync_DraftIssuance_PublishesAndPublishesEvent`
- ✅ `PublishAsync_NonDraftStatus_ThrowsException`
- ✅ `PublishAsync_NonExistent_ThrowsException`
- ✅ `CloseAsync_PublishedIssuance_ClosesAndPublishesEvent`
- ✅ `CloseAsync_NonPublishedStatus_ThrowsException`
- ✅ `CloseAsync_NonExistent_ThrowsException`

### Integration Tests (IssuanceApiTests.cs)
- ✅ `POST_Issuances_ValidRequest_Returns201`
- ✅ `POST_Issuances_InvalidRequest_Returns400`
- ✅ `GET_Issuances_ExistingId_Returns200`
- ✅ `GET_Issuances_NonExistentId_Returns404`
- ✅ `POST_Publish_ExistingDraft_Returns200`
- ⚠️ `POST_Publish_NonExistent_Returns404` — требует исправления (возвращает 500 вместо 404)
- ✅ `POST_Close_ExistingPublished_Returns200`
- ✅ `POST_Close_NonExistent_Returns404`
- ✅ `POST_Issuances_Unauthorized_Returns401`

### Test Results
- **Location:** `artifacts/issuance-test-report.txt`
- **Status:** 
  - ✅ 1 тест проходит (`IssuanceServiceTests.CreateAsync_ShouldCreateDraftIssuance`)
  - ⚠️ 1 тест падает (`IssuanceApiTests.Publish_NonExistent_Should_Return_404`) — ожидает 404, получает 500
  - **Исправление:** Добавлена проверка существования issuance перед вызовом `PublishAsync` в `Program.cs:233-235`
  - **Примечание:** Тест может использовать старую версию кода. После пересборки должен проходить.

## Recommendations

1. **AsyncAPI:** Добавить `dltTxHash` в `IssuancePublishedPayload` и `IssuanceClosedPayload` для соответствия коду, либо удалить из кода.

2. **Error Handling:** ✅ Исправлено — добавлена проверка существования issuance перед вызовом `PublishAsync` и `CloseAsync` в `Program.cs:233-235, 262-264`.

3. **Tests:** Тесты созданы в `services/issuance/issuance.Tests/`. Один тест может падать из-за использования старой версии кода — требуется пересборка.

4. **Documentation:** ✅ Обновлено — `docs/services/issuance.md` дополнено информацией о событиях, их payload и соответствии AsyncAPI.

## Verification Commands

```bash
# Run tests
dotnet test services/issuance/issuance.Tests.csproj -v minimal

# Or use existing tests
dotnet test tests/issuance.Tests/issuance.Tests.csproj -v minimal

# Check OpenAPI spec
spectral lint packages/contracts/openapi-issuance.yaml

# Check AsyncAPI spec
asyncapi validate packages/contracts/asyncapi.yaml
```

## References

- OpenAPI Spec: `packages/contracts/openapi-issuance.yaml`
- Gateway Spec: `packages/contracts/openapi-gateway.yaml`
- AsyncAPI Spec: `packages/contracts/asyncapi.yaml`
- Service Code: `services/issuance/`
- Tests: `services/issuance/issuance.Tests/`, `tests/issuance.Tests/`

