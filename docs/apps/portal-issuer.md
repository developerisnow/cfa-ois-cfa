# Portal Issuer

Next.js 15 портал для эмитентов ЦФА.

## Environment Variables

См. `.env.example`:
- `NEXT_PUBLIC_API_BASE_URL` - API Gateway URL (default: http://localhost:5000)
- `NEXT_PUBLIC_KEYCLOAK_URL` - Keycloak server URL
- `NEXT_PUBLIC_KEYCLOAK_REALM` - Keycloak realm (default: ois-dev)
- `NEXT_PUBLIC_KEYCLOAK_CLIENT_ID` - Client ID (default: portal-issuer)
- `NEXTAUTH_SECRET` - NextAuth secret

## Development

```bash
npm install
npm run dev
```

App runs on http://localhost:3001

## Build

```bash
npm run build
npm start
```

## Pages

- `/dashboard` - Dashboard с метриками
- `/issuances` - Список выпусков
- `/issuances/create` - Создать выпуск
- `/issuances/[id]` - Детали выпуска (publish/close)
- `/reports` - Отчёты

## Auth

Интеграция с Keycloak через NextAuth:
- OIDC flow
- RBAC: требует роль `issuer`
- Защита роутов через middleware

## QA Notes

- Формы валидируются через Zod
- Ошибки отображаются через Sonner toasts
- API ошибки маппятся в RFC7807 ProblemDetails

