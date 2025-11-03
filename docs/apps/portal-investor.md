# Portal Investor

Next.js 15 портал для инвесторов.

## Environment Variables

См. `.env.example`:
- `NEXT_PUBLIC_API_BASE_URL` - API Gateway URL
- `NEXT_PUBLIC_KEYCLOAK_URL` - Keycloak server URL
- `NEXT_PUBLIC_KEYCLOAK_REALM` - Keycloak realm
- `NEXT_PUBLIC_KEYCLOAK_CLIENT_ID` - Client ID (default: portal-investor)

## Development

```bash
npm install
npm run dev
```

App runs on http://localhost:3002

## Pages

- `/portfolio` - Портфель (wallet, holdings)
- `/orders/new` - Разместить заказ на покупку
- `/history` - История операций

## Auth

- Keycloak OIDC
- RBAC: требует роль `investor`

