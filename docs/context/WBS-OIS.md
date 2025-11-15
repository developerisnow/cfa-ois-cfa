# WBS-OIS (Work Breakdown Structure)

Generated: 2025-11-13

## Tracks
- Spec & Contracts
- Backend Services (Issuance, Registry, Settlement, Compliance, Identity)
- API Gateway & Frontends
- Observability & Security
- CI/CD & Infra

## Milestones
- M1: Spec linted + API/Event Matrix (NX-01)
- M2: Gateway health/metrics/routing verified (NX-02)
- M3: Issuance + Registry core paths green with tests (NX-03, NX-04)
- M4: Identity/Keycloak baseline + policies (planned)
- M5: CI quality gates + artifacts (planned)

## Next Tasks
- NX-01: Spec validation + API/Event Matrix — tasks/NX-01-spec-validate-and-matrix.md
- NX-02: API Gateway routing + health/metrics — tasks/NX-02-gateway-routing-and-health.md
- NX-03: Issuance endpoints alignment + tests — tasks/NX-03-issuance-endpoints-coverage.md
- NX-04: Registry order flow (create→reserve→paid) + events — tasks/NX-04-registry-orders-flow.md
- NX-05: Identity/Keycloak integration baseline — planned
- NX-06: CI quality gates (Spectral/AJV/tests/coverage artifacts) — planned

## Notes
- Вся реализация — spec-first. При расхождениях в спеке → сперва YAML‑патч и ревью, затем реализация.
- Для событий Kafka — синхронизировать AsyncAPI топики и payload c фактическими DTO/схемами.

