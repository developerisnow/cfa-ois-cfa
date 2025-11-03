# Assumptions

## Technical Assumptions

1. **.NET Version**: Using .NET 9.0 (latest stable)
   - Source: Microsoft .NET releases
   - Date: 2025-01

2. **PostgreSQL**: Version 16 (alpine)
   - Source: Docker Hub official image
   - Rationale: Latest stable, small image size

3. **Kafka**: Apache Kafka 3.7 (latest)
   - Source: Apache Kafka releases
   - Rationale: Stable, production-ready

4. **Keycloak**: Version 25.0 (latest)
   - Source: Quay.io Keycloak images
   - Rationale: OIDC/OAuth2 support, dev-friendly

5. **Next.js**: Version 15 (latest)
   - Source: Next.js releases
   - Rationale: App Router, React 19 support

6. **Hyperledger Fabric**: Version 2.2+ (mentioned in docs)
   - Source: Architecture docs
   - Note: Not included in MVP docker-compose (external dependency)

## Business Assumptions

1. **MVP Scope**: 
   - Happy path only for issue→buy→payout→redeem flow
   - Edge cases marked as TODO with failing tests

2. **Mock Integrations**: 
   - ESIA: Mock OIDC profile
   - Bank: Mock nominal account + transfer with idempotency
   - EDO: Mock UKEP signature

3. **Dev Environment**:
   - Single-node Kafka (no replication in dev)
   - PostgreSQL without encryption (dev only)
   - Self-signed certificates acceptable

## Security Assumptions

1. **Secrets**: Managed via environment variables (dev) or Vault (prod)
2. **mTLS**: Not required in dev, required in prod
3. **RBAC**: Basic role-based access in MVP

## Missing Data / TODO

1. **HLF Network**: Hyperledger Fabric network topology not yet implemented
   - Need: Chaincode deployment scripts
   - Need: Network configuration (orderer/peer setup)

2. **Seed Data**: Demo data script not yet created
   - Need: Sample issuers, investors, assets, issuances

3. **E2E Test**: Playwright scenarios not yet created
   - Need: Full lifecycle test (issue→buy→payout→redeem)

