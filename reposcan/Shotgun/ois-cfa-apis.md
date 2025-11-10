# OIS-CFA · API Surface (from OpenAPI/AsyncAPI)

Canonical contracts live in `packages/contracts`. This summary lists key REST endpoints per service and event channels. See the contracts for full schemas, types, and responses.

## API Gateway
- Spec: `packages/contracts/openapi-gateway.yaml:1`
- Highlights:
  - `GET /health` — gateway health
  - Reverse proxy routes to service APIs with rate limiting and OIDC integration

## Identity Service
- Spec: `packages/contracts/openapi-identity.yaml:1`
- Endpoints:
  - `GET /.well-known/openid-configuration`
  - `GET /userinfo`
  - `GET /users`
  - `POST /users`

## Registry Service
- Spec: `packages/contracts/openapi-registry.yaml:1`
- Endpoints:
  - `POST /v1/orders` — Idempotency-Key header required
  - `GET /v1/orders/{id}` — fetch order by id
  - `GET /v1/wallets/{investorId}` — investor wallet
  - `POST /v1/issuances/{id}/redeem` — redeem issuance for investor
  - `GET /v1/holdings/{investorId}` — investor holdings

## Issuance Service
- Spec: `packages/contracts/openapi-issuance.yaml:1`
- Endpoints:
  - `GET /v1/issuances` — list issuances
  - `POST /v1/issuances` — create issuance
  - `GET /v1/issuances/{id}` — get issuance
  - `POST /v1/issuances/{id}/publish` — publish
  - `POST /v1/issuances/{id}/close` — close

## Compliance Service
- Spec: `packages/contracts/openapi-compliance.yaml:1`
- Endpoints:
  - `POST /v1/compliance/kyc/check` — KYC check
  - `POST /v1/compliance/qualification/evaluate` — qualification tier
  - `POST /v1/complaints` — register complaint
  - `GET /v1/complaints/{id}` — complaint status

## Settlement Service
- Spec: `packages/contracts/openapi-settlement.yaml:1`
- Endpoints:
  - `POST /v1/settlement/run?date=YYYY-MM-DD`
  - `GET /v1/reports/payouts?from=YYYY-MM-DD&to=YYYY-MM-DD`

## Integrations — Bank Nominal (stub)
- Spec: `packages/contracts/openapi-integrations-bank.yaml:1`
- Endpoints:
  - `POST /nominal/reserve` — reserve funds
  - `POST /nominal/payouts/batch` — execute payout batch

## Integrations — ESIA (OIDC)
- Spec: `packages/contracts/openapi-integrations-esia.yaml:1`

## Integrations — EDO
- Spec: `packages/contracts/openapi-integrations-edo.yaml:1`

## Events (AsyncAPI)
- Spec: `packages/contracts/asyncapi.yaml:1`
- Channels (publish):
  - `ois.issuance.published`, `ois.issuance.closed`
  - `ois.order.placed`, `ois.order.confirmed`
  - `ois.payout.scheduled`, `ois.payout.executed`
  - `ois.audit.logged`, `ois.transfer.completed`

## cURL Examples

```bash
# Place order (Registry)
curl -X POST http://localhost:5006/v1/orders \
  -H 'Authorization: Bearer <token>' \
  -H 'Idempotency-Key: <uuid>' \
  -H 'Content-Type: application/json' \
  -d '{"investorId":"<uuid>","issuanceId":"<uuid>","amount":100.0}'

# Publish issuance (Issuance)
curl -X POST http://localhost:5005/v1/issuances/<id>/publish \
  -H 'Authorization: Bearer <token>'

# Run settlement (Settlement)
curl -X POST 'http://localhost:5007/v1/settlement/run?date=2025-01-15' \
  -H 'Authorization: Bearer <token>'
```
