# Backend Context Map (OIS-CFA)

Date: 2025-11-11

This document indexes backend contracts and implementation, validates API/event specs, and maps endpoints to handlers, entities, and topics.

## Validations Summary

- OpenAPI (Spectral): passed (custom minimal ruleset)
  - Command: `ops/scripts/validate-specs.sh` (uses local Node v20 toolchain)
- AsyncAPI (asyncapi CLI): valid with warnings only
- JSON Schemas (AJV compile): all schemas compile (formats disabled)

Artifacts and commands can be re-run via: `ops/scripts/validate-specs.sh`.

## Contracts Indexed

- OpenAPI specs: `packages/contracts/openapi-*.yaml`
- AsyncAPI spec: `packages/contracts/asyncapi.yaml`
- JSON Schemas: `packages/contracts/schemas/*.json`

## Endpoint ↔ Handler ↔ Entity ↔ Topic

### Issuance Service

- POST `/v1/issuances`
  - Handler: `services/issuance/Program.cs:87` → `IIssuanceService.CreateAsync` → `services/issuance/Services/IssuanceService.cs:26`
  - DTOs: `CreateIssuanceRequest` → `IssuanceResponse`
  - Entity: `IssuanceEntity` (`services/issuance/IssuanceDbContext.cs`)
  - Topics: —
- GET `/v1/issuances/{id}`
  - Handler: `services/issuance/Program.cs:98` → `IIssuanceService.GetByIdAsync`
  - DTOs: `IssuanceResponse`
  - Entity: `IssuanceEntity`
  - Topics: —
- POST `/v1/issuances/{id}/publish`
  - Handler: `services/issuance/Program.cs:109` → `IIssuanceService.PublishAsync` → `services/issuance/Services/IssuanceService.cs:63`
  - DTOs: `IssuanceResponse`
  - Entity: `IssuanceEntity`
  - Topics: `ois.issuance.published`
    - AsyncAPI: channel `ois.issuance.published`, message `IssuancePublished` (payload `IssuancePublishedPayload`)
- POST `/v1/issuances/{id}/close`
  - Handler: `services/issuance/Program.cs:130` → `IIssuanceService.CloseAsync` → `services/issuance/Services/IssuanceService.cs:96`
  - DTOs: `IssuanceResponse`
  - Entity: `IssuanceEntity`
  - Topics: `ois.issuance.closed`
    - AsyncAPI: channel `ois.issuance.closed`, message `IssuanceClosed` (payload `IssuanceClosedPayload`)

### Registry Service

- POST `/v1/orders`
  - Handler: `services/registry/Program.cs:81` → `IRegistryService.PlaceOrderAsync` → `services/registry/Services/RegistryService.cs:23`
  - DTOs: `CreateOrderRequest` → `OrderResponse`
  - Entities: `OrderEntity`, `WalletEntity`, `HoldingEntity`, `TransactionEntity` (`services/registry/RegistryDbContext.cs`)
  - Topics: `ois.registry.transferred`
    - AsyncAPI: channel `ois.registry.transferred`, message `RegistryTransferred` (payload `RegistryTransferredPayload`)
- GET `/v1/orders/{id}`
  - Handler: `services/registry/Program.cs:104` → `IRegistryService.GetOrderAsync`
  - DTOs: `OrderResponse`
  - Entity: `OrderEntity`
  - Topics: —
- GET `/v1/wallets/{investorId}`
  - Handler: `services/registry/Program.cs:115` → `IRegistryService.GetWalletAsync`
  - DTOs: `WalletResponse`
  - Entities: `WalletEntity`, `HoldingEntity`
  - Topics: —
- POST `/v1/issuances/{id}/redeem`
  - Handler: `services/registry/Program.cs:126` → `IRegistryService.RedeemAsync` → `services/registry/Services/RegistryService.cs:188`
  - DTOs: `RedeemRequest` → `RedeemResponse`
  - Entities: `TransactionEntity`, `HoldingEntity`
  - Topics: —

### Settlement Service

- POST `/v1/settlement/run`
  - Handler: `services/settlement/Program.cs:74` → `ISettlementService.RunSettlementAsync` → `services/settlement/Services/SettlementService.cs:24`
  - DTOs: `SettlementResponse`
  - Entities: `PayoutBatchEntity`, `PayoutItemEntity`, `ReconciliationLogEntity` (`services/settlement/SettlementDbContext.cs`)
  - Topics: `ois.payout.executed`
    - AsyncAPI: channel `ois.payout.executed`, message `PayoutExecuted` (payload `PayoutExecutedPayload`)
- GET `/v1/reports/payouts`
  - Handler: `services/settlement/Program.cs:95` → `ISettlementService.GetPayoutsReportAsync`
  - DTOs: `PayoutsReportResponse`
  - Entities: — (read-only aggregation)
  - Topics: —

### Compliance Service

- POST `/v1/compliance/kyc/check`
  - Handler: `services/compliance/Program.cs:71` → `IComplianceService.CheckKycAsync` → `services/compliance/Services/ComplianceService.cs:32`
  - DTOs: `KycCheckRequest` → `KycResult`
  - Entity: `InvestorComplianceEntity` (`services/compliance/ComplianceDbContext.cs`)
  - Topics: `ois.compliance.flagged` (when watchlist match)
    - AsyncAPI: channel `ois.compliance.flagged`, message `ComplianceFlagged` (payload `ComplianceFlaggedPayload`)
- POST `/v1/compliance/qualification/evaluate`
  - Handler: `services/compliance/Program.cs:82` → `IComplianceService.EvaluateQualificationAsync`
  - DTOs: `QualificationEvaluateRequest` → `QualificationResult`
  - Entity: `InvestorComplianceEntity`
  - Topics: `ois.compliance.flagged` (when limit exceeded)
- GET `/v1/compliance/investors/{id}/status`
  - Handler: `services/compliance/Program.cs:93` → `IComplianceService.GetInvestorStatusAsync`
  - DTOs: `InvestorStatusResponse`
  - Entity: `InvestorComplianceEntity`
  - Topics: —
- POST `/v1/complaints`
  - Handler: `services/compliance/Program.cs:106` → `IComplianceService.CreateComplaintAsync`
  - DTOs: `CreateComplaintRequest` → `ComplaintResponse`
  - Entity: `ComplaintEntity`
  - Topics: —
- GET `/v1/complaints/{id}`
  - Handler: `services/compliance/Program.cs:124` → `IComplianceService.GetComplaintAsync`
  - DTOs: `ComplaintResponse`
  - Entity: `ComplaintEntity`
  - Topics: —

### Identity (Mock)

- GET `/.well-known/openid-configuration`
  - Handler: `services/identity/Program.cs:27`
- GET `/userinfo`
  - Handler: `services/identity/Program.cs:37`
- GET `/users`
  - Handler: `services/identity/Program.cs:45`
- GET `/users/{id}`
  - Handler: `services/identity/Program.cs:46`

### Integrations (Bank Nominal, Mock)

- POST `/nominal/reserve`
  - Handler: `services/integrations/bank-nominal/Program.cs:21`
  - DTOs: `ReserveRequest` → `ReserveResponse`
- POST `/nominal/payouts/batch`
  - Handler: `services/integrations/bank-nominal/Program.cs:45`
  - DTOs: `BatchPayoutRequest` → `BatchPayoutResponse`

## Topics ↔ Message Schemas (AsyncAPI)

- `ois.issuance.published` → `IssuancePublished` → `IssuancePublishedPayload`
- `ois.issuance.closed` → `IssuanceClosed` → `IssuanceClosedPayload`
- `ois.registry.transferred` → `RegistryTransferred` → `RegistryTransferredPayload`
- `ois.payout.executed` → `PayoutExecuted` → `PayoutExecutedPayload`
- `ois.compliance.flagged` → `ComplianceFlagged` → `ComplianceFlaggedPayload`

## Notes

- OpenAPI gateway spec adjusted to fix duplicate path and missing component refs; cross-file `$ref` is used to re-use service schemas.
- AsyncAPI kept at 2.6.0 syntax; warnings remain (messageId, id/tags); functional validity is green.
- JSON Schemas normalized to draft-07 semantics for `exclusiveMinimum`.

