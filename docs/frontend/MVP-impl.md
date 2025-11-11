MVP Frontend Implementation Plan (OIS)

Scope
- Apps: `apps/portal-investor`, `apps/portal-issuer`, `apps/backoffice`
- SDK: `packages/sdks/ts` (OpenAPI-based, with observability headers + retry)
- Contracts: `packages/contracts/*` (OpenAPI/AsyncAPI/JSON Schemas)

Routes
- Investor
  - `/catalog` → GET `/v1/market/issuances` (list, filters, pagination)
  - `/issuances/[id]` → GET `/v1/market/issuances/{id}`; POST `/v1/orders` (Idempotency-Key)
  - `/history` → GET `/v1/investors/{id}/transactions`, `/v1/investors/{id}/payouts` (CSV export)
- Issuer
  - `/reports` → GET `/v1/reports/issuances`, `/v1/reports/payouts` (CSV/XLSX export)
  - Note: payouts schedule CRUD endpoints are not present in Gateway spec. See Spec Diff.
- Backoffice
  - `/kyc` → POST `/v1/kyc/{investorId}/decision`; GET `/v1/kyc/{investorId}/documents`; GET `/v1/compliance/investors/{id}/status`
  - `/audit` → GET `/v1/audit` (filters/search/export)

AuthN/AuthZ
- Keycloak via NextAuth in apps. Middleware enforces roles:
  - Investor: `investor`
  - Issuer: `issuer`
  - Backoffice: `backoffice` or `admin`
- E2E uses Playwright to stub `GET /api/auth/session` for app journeys.

API Client and Observability
- `OisApiClient` adds headers per request: `x-request-id`, `traceparent`, `x-client-app`.
- Basic retry/backoff on 429/5xx/network errors (3 attempts, exponential with jitter).
- Web-vitals initialized in providers; emits CustomEvent `web-vitals` and logs to console.
- ErrorBoundary wraps app content to surface UI errors with accessible fallback.

Acceptance Readiness
- Investor: catalog → detail → buy → history implemented end-to-end with SDK; CSV export for history.
- Issuer: reports implemented with CSV/XLSX exports.
- Backoffice: KYC approve/reject and Audit views implemented; audit entries visible after decision (when backend returns event).

Spec Diff (Minimal)
- Missing in `packages/contracts/openapi-gateway.yaml` for Issuer Payout Schedule CRUD:
  - POST `/v1/issuances/{id}/payouts/schedule` (create/update schedule)
  - GET `/v1/issuances/{id}/payouts/schedule` (fetch schedule)
  - PATCH `/v1/issuances/{id}/payouts/schedule/{itemId}` (status change: planned→executing→done)
  - DELETE `/v1/issuances/{id}/payouts/schedule/{itemId}` (cancel)
  - Suggest schemas: `PayoutScheduleItem { id, date, amount, status }` with statuses: planned|executing|done|cancelled.
  - Until added, the issuer UI shows read-only schedule preview via `Issuance.scheduleJson`.

Testing
- Unit/component: existing pages use predictable SDK contracts; focus added later for hooks/utils.
- E2E (Playwright):
  - Investor: `tests/e2e/tests/investor-journey.spec.ts`
  - Issuer: `tests/e2e/tests/issuer-journey.spec.ts`
  - Backoffice: `tests/e2e/tests/backoffice-journey.spec.ts`
- Lighthouse: target ≥85 on key routes; pending CI add for LHCI.

Notes
- No ad-hoc endpoints used beyond documented Gateway APIs.
- i18n and analytics beyond MVP kept minimal.

