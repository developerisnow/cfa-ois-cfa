# Bank Nominal Service

Mock адаптер для номинального счёта и банковских операций.

## Endpoints

- `POST /nominal/accounts` - Открыть номинальный счёт
- `GET /nominal/accounts/{id}` - Get account
- `POST /nominal/transfer` - Перевод средств (с идемпотентностью)

## TODO

- [ ] Implement .NET 9 service
- [ ] Idempotency key handling
- [ ] Mock account storage
- [ ] Integration tests

