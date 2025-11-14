Context Map (Frontend ↔ API Contracts)

Overview
- Monorepo structure: apps/* (portals), services/*, packages/contracts/* (OpenAPI/AsyncAPI/JSON Schemas), packages/sdks/ts (TS SDK), packages/types/ts (JSON Schema → TS types).
- Roles via Keycloak: investor, issuer, broker, backoffice (plus admin). Guards applied with NextAuth middleware per app.

Auth/Guards
- Investor app guard: apps/portal-investor/src/middleware.ts
  - Matcher: `/portfolio/*`, `/orders/*`, `/history/*` → token must contain role `investor`.
- Issuer app guard: apps/portal-issuer/src/middleware.ts
  - Matcher: `/dashboard/*`, `/issuances/*`, `/reports/*`, `/payouts/*` → token must contain role `issuer`.
- Backoffice app guard: apps/backoffice/src/middleware.ts
  - Matcher: `/kyc/*`, `/qualification/*`, `/payouts/*`, `/audit/*` → token must contain `backoffice` or `admin`.

Contracts Sources
- OpenAPI (Gateway): `packages/contracts/openapi-gateway.yaml` (market, orders, investors, reports, audit, kyc, compliance, settlement, etc.)
- OpenAPI (Services): issuance, identity, settlement, compliance under `packages/contracts/openapi-*.yaml`.
- AsyncAPI (events): `packages/contracts/asyncapi.yaml` (ois.payout.* etc.).
- JSON Schemas (domain): `packages/contracts/schemas/*.json` (CFA, Issuance, Order, Payout, AuditEvent, Wallet, TxHistoryItem, IssuerReportRow, etc.).

SDK/Types
- SDK (TS Axios client): `packages/sdks/ts` (OisApiClient). Request headers include `x-request-id`, `traceparent`, `x-client-app`; retries on 429/5xx.
- Types generated from JSON Schemas: `packages/types/ts` with generator (json-schema-to-typescript) producing `src/generated/*.d.ts` and `dist/*`.

Pages → Endpoints → Models
- Investor portal (`apps/portal-investor`)
  - `/catalog` (src/app/catalog/page.tsx)
    - GET `/v1/market/issuances` (query: status, sort, limit, offset)
    - Model: MarketIssuancesResponse → MarketIssuanceCard (SDK type)
  - `/issuances/[id]` (src/app/issuances/[id]/page.tsx)
    - GET `/v1/market/issuances/{id}` → MarketIssuanceCard
    - POST `/v1/orders` (header: Idempotency-Key) → OrderResponse
    - Models: MarketIssuanceCard, CreateOrderRequest, OrderResponse
  - `/history` (src/app/history/page.tsx)
    - GET `/v1/investors/{id}/transactions` → TransactionHistoryResponse (items: TxHistoryItem)
    - GET `/v1/investors/{id}/payouts` → PayoutHistoryResponse (items: PayoutItem)
    - CSV export local
  - `/portfolio` (src/app/portfolio/page.tsx)
    - GET `/v1/wallets/{investorId}` → WalletResponse (holdings[])

- Issuer portal (`apps/portal-issuer`)
  - `/reports` (src/app/reports/page.tsx)
    - GET `/v1/reports/issuances` → IssuerIssuancesReportResponse (items: IssuerReportRow)
    - GET `/v1/reports/payouts` → IssuerPayoutsReportResponse (items: period aggregates)
    - CSV/XLSX exports local
  - `/payouts/schedule` (src/app/payouts/schedule/page.tsx)
    - Note: CRUD endpoints for schedule absent in Gateway OpenAPI. UI stub provided. See Spec Diff in docs/frontend/MVP-impl.md.

- Backoffice portal (`apps/backoffice`)
  - `/kyc` (src/app/kyc/page.tsx)
    - POST `/v1/kyc/{investorId}/decision` → KycDecisionResponse
    - GET `/v1/kyc/{investorId}/documents` → KycDocumentsResponse
    - GET `/v1/compliance/investors/{id}/status` → InvestorStatusResponse
    - Local upload via multipart/form-data
  - `/audit` (src/app/audit/page.tsx)
    - GET `/v1/audit` (filters: actor, action, entity, from, to, limit, offset) → AuditEventsResponse (items: AuditEvent)
    - CSV export local

Events (AsyncAPI)
- `packages/contracts/asyncapi.yaml` (topics include: `ois.payout.executed`, `ois.payout.scheduled`, etc.)
  - Used for backoffice/audit visibility; UIs currently poll via REST (`/v1/audit`). Future: subscribe to events stream.

Validation & Tooling
- CI job `validate:specs` runs:
  - Spectral: `packages/contracts/openapi-*.yaml`
  - AsyncAPI CLI: `packages/contracts/asyncapi.yaml`
  - Ajv compile: `packages/contracts/schemas/*.json` (draft-07)
- Local generation:
  - SDK: `packages/sdks/ts` (currently maintained; add generator if required)
  - Types: `cd packages/types/ts && npm run generate && npm run build`

Appendix: Model Pointers (JSON Schemas)
- Issuance: `packages/contracts/schemas/Issuance.json`
- Order: `packages/contracts/schemas/Order.json`
- PayoutItem: `packages/contracts/schemas/PayoutItem.json`
- AuditEvent: `packages/contracts/schemas/AuditEvent.json`
- Wallet: `packages/contracts/schemas/Wallet.json`
- TxHistoryItem: `packages/contracts/schemas/TxHistoryItem.json`
- IssuerReportRow: `packages/contracts/schemas/IssuerReportRow.json`

