# Registry Chaincode

Hyperledger Fabric chaincode для операций с ЦФА (Go).

## Methods

- `Transfer(from?, to, amount)` - Передача прав
- `Redeem(holder, amount)` - Погашение
- `GetHistory(assetId)` - История операций

## TODO

- [ ] Implement Go chaincode
- [ ] Unit tests (shim.MockStub)
- [ ] Integration with HLF network

