# Issuance Chaincode

Hyperledger Fabric chaincode для выпуска ЦФА (Go).

## Methods

- `Issue(assetId, total, scheduleJSON)` - Выпустить ЦФА
- `Close()` - Закрыть выпуск
- `Get()` - Получить информацию о выпуске

## TODO

- [ ] Implement Go chaincode
- [ ] Unit tests (shim.MockStub)
- [ ] Integration with HLF network

