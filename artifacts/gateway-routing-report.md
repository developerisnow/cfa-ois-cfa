# API Gateway Routing Report

**Generated:** 2025-01-27  
**Task:** NX-02 Gateway Routing and Health

## Executive Summary

API Gateway использует **YARP (Yet Another Reverse Proxy)** для маршрутизации запросов к backend сервисам. Все основные сервисы имеют `/health` и `/metrics` endpoints. Маршрутизация настроена в `apps/api-gateway/appsettings.json`.

## YARP Routing Configuration

### Routes (Gateway → Services)

| Gateway Path | Cluster | Service | Service Address | Transform | Status |
|--------------|---------|---------|----------------|-----------|--------|
| `/issuances/{**catch-all}` | `issuance` | Issuance | `http://issuance-service:8080` | `/v1/issuances/{**catch-all}` | ✅ |
| `/v1/orders/{**catch-all}` | `registry` | Registry | `http://registry-service:8080` | `/v1/orders/{**catch-all}` | ✅ |
| `/v1/wallets/{**catch-all}` | `registry` | Registry | `http://registry-service:8080` | `/v1/wallets/{**catch-all}` | ✅ |
| `/v1/issuances/{**catch-all}/redeem` | `registry` | Registry | `http://registry-service:8080` | `/v1/issuances/{**catch-all}/redeem` | ✅ |
| `/v1/settlement/{**catch-all}` | `settlement` | Settlement | `http://settlement-service:8080` | `/v1/settlement/{**catch-all}` | ✅ |
| `/v1/reports/{**catch-all}` | `settlement` | Settlement | `http://settlement-service:8080` | `/v1/reports/{**catch-all}` | ✅ |
| `/v1/compliance/{**catch-all}` | `compliance` | Compliance | `http://compliance-service:8080` | `/v1/compliance/{**catch-all}` | ✅ |
| `/v1/complaints/{**catch-all}` | `compliance` | Compliance | `http://compliance-service:8080` | `/v1/complaints/{**catch-all}` | ✅ |
| `/identity/{**catch-all}` | `identity` | Identity | `http://identity-service:8080` | `/identity/{**catch-all}` | ✅ |

### Endpoint Mapping (OpenAPI Gateway → YARP Routes)

| OpenAPI Endpoint | Method | YARP Route | Service Endpoint | Verified |
|------------------|--------|------------|-----------------|----------|
| `/health` | GET | Direct (Gateway) | `/health` | ✅ |
| `/issuances` | POST | `issuances` | `/v1/issuances` | ✅ |
| `/issuances/{id}` | GET | `issuances` | `/v1/issuances/{id}` | ✅ |
| `/issuances/{id}/publish` | POST | `issuances` | `/v1/issuances/{id}/publish` | ✅ |
| `/issuances/{id}/close` | POST | `issuances` | `/v1/issuances/{id}/close` | ✅ |
| `/v1/issuances/{id}/redeem` | POST | `redeem` | `/v1/issuances/{id}/redeem` | ✅ |
| `/v1/orders` | POST | `orders` | `/v1/orders` | ✅ |
| `/orders/{id}` | GET | `orders` | `/v1/orders/{id}` | ⚠️ |
| `/v1/wallets/{investorId}` | GET | `wallets` | `/v1/wallets/{investorId}` | ✅ |
| `/v1/settlement/run` | POST | `settlement` | `/v1/settlement/run` | ✅ |
| `/v1/reports/payouts` | GET | `reports` | `/v1/reports/payouts` | ✅ |
| `/v1/compliance/kyc/check` | POST | `compliance` | `/v1/compliance/kyc/check` | ✅ |
| `/v1/compliance/qualification/evaluate` | POST | `compliance` | `/v1/compliance/qualification/evaluate` | ✅ |
| `/v1/compliance/investors/{id}/status` | GET | `compliance` | `/v1/compliance/investors/{id}/status` | ✅ |
| `/v1/complaints` | POST | `complaints` | `/v1/complaints` | ✅ |

**⚠️ SPEC DIFF**: Gateway OpenAPI определяет `/orders/{id}` без префикса `/v1`, но YARP маршрут `orders` ожидает `/v1/orders/{**catch-all}`. Требуется либо обновить OpenAPI, либо добавить дополнительный маршрут.

## Health & Metrics Status

### Services Health Endpoints

| Service | Health Endpoint | Metrics Endpoint | Status | Notes |
|---------|----------------|------------------|--------|-------|
| API Gateway | `/health` | — | ✅ | `apps/api-gateway/Program.cs:99` |
| Identity | `/health` | — | ✅ | `services/identity/Program.cs:25` |
| Issuance | `/health` | `/metrics` | ✅ | `services/issuance/Program.cs:139-140` |
| Registry | `/health` | `/metrics` | ✅ | `services/registry/Program.cs:167-168` |
| Settlement | `/health` | `/metrics` | ✅ | `services/settlement/Program.cs:136-137` |
| Compliance | `/health` | `/metrics` | ✅ | `services/compliance/Program.cs:145-146` |
| Bank Nominal | `/health` | — | ✅ | `services/integrations/bank-nominal/Program.cs:19` |
| ESIA Adapter | `/health` | — | ✅ | (README mentions it) |

### Health Checks Implementation

Все основные сервисы используют:
- `AddHealthChecks()` с `AddDbContextCheck<T>()` для проверки БД
- `MapHealthChecks("/health")` для endpoint
- `MapPrometheusScrapingEndpoint("/metrics")` для метрик (Issuance, Registry, Settlement, Compliance)

## Local Development Ports

| Service | HTTP Port | HTTPS Port | Notes |
|---------|-----------|------------|-------|
| API Gateway | 53985 | 53977 | `apps/api-gateway/Properties/launchSettings.json` |
| Identity | 53987 | 53981 | `services/identity/Properties/launchSettings.json` |
| Issuance | 53983 | 53978 | `services/issuance/Properties/launchSettings.json` |
| Registry | 53988 | 53980 | `services/registry/Properties/launchSettings.json` |
| Settlement | — | — | (check docker-compose or appsettings) |
| Compliance | 53986 | 53979 | `services/compliance/Properties/launchSettings.json` |

**Note**: В docker-compose используются другие порты (5000-5008). См. `docker-compose.yml`.

## Recommendations

1. **Согласовать префиксы**: Gateway OpenAPI использует `/issuances` без `/v1`, но сервисы ожидают `/v1/issuances`. YARP трансформирует корректно, но OpenAPI спецификация должна отражать фактический путь Gateway.

2. **Добавить маршрут для `/orders/{id}`**: Либо обновить OpenAPI на `/v1/orders/{id}`, либо добавить дополнительный YARP маршрут.

3. **Метрики для Gateway**: Рассмотреть добавление Prometheus метрик в Gateway для мониторинга проксируемых запросов.

4. **Health check для всех интеграций**: ESIA Adapter и EDO Connector должны иметь явные health endpoints.

## Verification Commands

```bash
# Check all services health
make check-health

# Check individual service
curl -sf http://localhost:5000/health && echo "Gateway: OK" || echo "Gateway: FAILED"

# Check metrics
curl -sf http://localhost:5002/metrics | head -20

# Test routing through Gateway
curl -X POST http://localhost:5000/issuances \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"assetId":"...","issuerId":"..."}'
```

## References

- YARP Configuration: `apps/api-gateway/appsettings.json`
- Gateway Code: `apps/api-gateway/Program.cs`
- OpenAPI Spec: `packages/contracts/openapi-gateway.yaml`
- Service Health: `services/*/Program.cs` (MapHealthChecks)

