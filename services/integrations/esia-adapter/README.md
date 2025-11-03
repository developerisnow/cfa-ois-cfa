# ESIA Adapter Service

Mock адаптер для ЕСИА (OIDC/OAuth2).

## Endpoints

- `GET /health` - Health check
- `GET /oidc/authorize` - OIDC authorization (redirect to mock)
- `POST /oidc/callback` - OIDC callback handler
- `GET /profile` - Get ESIA profile (mock)

## TODO

- [ ] Implement .NET 9 service with OIDC endpoints
- [ ] Mock ESIA profile data
- [ ] Integration tests

