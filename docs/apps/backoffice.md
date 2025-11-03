# Backoffice

Next.js 15 административный портал.

## Development

```bash
npm install
npm run dev
```

App runs on http://localhost:3003

## Pages

- `/kyc` - Управление KYC (approve/reject)
- `/qualification` - Управление квалификацией (tier/limits)
- `/payouts` - Управление выплатами (run/monitor)
- `/audit` - Аудит лог

## Auth

- Keycloak OIDC
- RBAC: требует роль `admin` или `backoffice`

