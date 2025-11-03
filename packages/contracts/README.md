# Contracts Package

API контракты (OpenAPI/AsyncAPI/JSON Schemas) для ОИС ЦФА.

## Структура

```
packages/contracts/
├── openapi-gateway.yaml          # Gateway API
├── openapi-identity.yaml          # Identity Service
├── openapi-integrations-esia.yaml # ESIA Adapter
├── openapi-integrations-bank.yaml # Bank Nominal
├── openapi-integrations-edo.yaml  # EDO Connector
├── asyncapi.yaml                  # Kafka Events
└── schemas/
    ├── CFA.json
    ├── Issuance.json
    ├── Order.json
    ├── Payout.json
    └── AuditEvent.json
```

## Валидация

```bash
# OpenAPI
spectral lint openapi-*.yaml

# AsyncAPI
asyncapi validate asyncapi.yaml

# JSON Schemas
ajv validate -s schemas/CFA.json -d <data>
```

## Генерация SDK

См. `/Makefile` target `generate-sdks`.

