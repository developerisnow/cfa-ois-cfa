# Frontend Applications - Startup Guide

## URLs

### Development Servers
- **Portal Issuer**: http://localhost:3001
- **Portal Investor**: http://localhost:3002
- **Backoffice**: http://localhost:3003

### Backend Services
- **API Gateway**: http://localhost:5000
- **Keycloak**: http://localhost:8080

---

## Quick Start

### 1. Install Dependencies

```bash
# Install all frontend dependencies
cd apps/portal-issuer && npm install
cd ../portal-investor && npm install
cd ../backoffice && npm install

# Install E2E test dependencies
cd ../../tests/e2e && npm install

# Generate SDKs (optional, if needed)
cd ../../packages/sdks/ts && npm install && npm run generate
```

### 2. Start Backend Services

```bash
docker-compose up -d
```

### 3. Setup Keycloak

См. `KEYCLOAK-SETUP.md` для настройки realm, clients и users.

**Quick setup:**
1. Go to http://localhost:8080 (admin/admin)
2. Create realm `ois-dev`
3. Create clients: `portal-issuer`, `portal-investor`, `backoffice`
4. Create roles: `issuer`, `investor`, `admin`
5. Create test users с соответствующими ролями

### 4. Start Frontend Apps

**Terminal 1:**
```bash
cd apps/portal-issuer
npm run dev
```

**Terminal 2:**
```bash
cd apps/portal-investor
npm run dev
```

**Terminal 3:**
```bash
cd apps/backoffice
npm run dev
```

---

## Test Credentials (Keycloak)

После создания в Keycloak:

- **Issuer**: `issuer@test.com` / `password123` (role: issuer)
- **Investor**: `investor@test.com` / `password123` (role: investor)
- **Admin**: `admin@test.com` / `password123` (roles: admin, backoffice)

---

## E2E Testing

```bash
# Run tests
make e2e

# Run with UI
make e2e-ui

# View report
cd tests/e2e && npx playwright show-report
```

---

## Troubleshooting

1. **Keycloak connection errors**: Проверьте, что Keycloak запущен и realm настроен
2. **CORS errors**: Убедитесь, что API Gateway запущен и CORS настроен
3. **SDK errors**: Запустите `npm run generate` в `packages/sdks/ts`

