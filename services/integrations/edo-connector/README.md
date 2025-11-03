# EDO Connector Service

Mock адаптер для ЭДО (подписание документов UKEP).

## Endpoints

- `POST /documents` - Отправить документ на подпись
- `GET /documents/{id}` - Get document
- `GET /documents/{id}/status` - Статус документа
- `POST /documents/{id}/sign` - Подписать документ (mock UKEP)

## TODO

- [ ] Implement .NET 9 service
- [ ] Mock UKEP signature
- [ ] Document storage
- [ ] Integration tests

