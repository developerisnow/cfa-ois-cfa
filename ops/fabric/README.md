# Hyperledger Fabric Dev Network

Локальная dev-сеть для разработки и тестирования ОИС ЦФА.

## Быстрый старт

```bash
# 1. Запустить сеть
./dev-up.sh

# 2. Создать канал
./scripts/create-channel.sh

# 3. Установить chaincode
./scripts/install-chaincode.sh

# 4. Утвердить chaincode
./scripts/approve-chaincode.sh

# 5. Проверить здоровье
./scripts/health-check.sh
```

## Команды

### Запуск сети
```bash
./dev-up.sh
```
Генерирует crypto-material, genesis block, channel transaction и запускает все контейнеры.

### Остановка
```bash
./dev-down.sh
```

### Полный сброс
```bash
./dev-reset.sh
```
**Внимание**: Удаляет все данные, включая crypto-material и volumes!

## Структура

```
ops/fabric/
├── docker-compose.yml          # Compose конфигурация
├── configtx.yaml               # Channel конфигурация
├── crypto-config.yaml          # Crypto material конфигурация
├── dev-up.sh                   # Запуск сети
├── dev-down.sh                 # Остановка сети
├── dev-reset.sh                # Сброс сети
├── scripts/
│   ├── create-channel.sh       # Создание канала
│   ├── install-chaincode.sh    # Установка chaincode
│   ├── approve-chaincode.sh    # Утверждение chaincode
│   ├── invoke-example.sh       # Примеры вызовов
│   ├── health-check.sh         # Проверка здоровья
│   └── chaincode-api.sh        # HTTP API для chaincode (dev helper)
└── channel-artifacts/          # Генерируется автоматически
    ├── genesis.block
    └── cfa-main.tx
```

## Пути к Crypto-Material

### Orderer
- **MSP**: `crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/msp`
- **TLS CA**: `crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls/ca.crt`
- **Orderer TLS Cert**: `crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls/server.crt`
- **Orderer TLS Key**: `crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls/server.key`

### Peer0
- **MSP**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/msp`
- **TLS CA**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt`
- **Peer TLS Cert**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.crt`
- **Peer TLS Key**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.key`

### Peer1
- **MSP**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/msp`
- **TLS CA**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls/ca.crt`
- **Peer TLS Cert**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls/server.crt`
- **Peer TLS Key**: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls/server.key`

### Admin User
- **MSP**: `crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp`
- **Sign Cert**: `crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp/signcerts/Admin@ois-dev.example.com-cert.pem`
- **Private Key**: `crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp/keystore/` (файл с ключом)

## Endpoints

- **Orderer**: `localhost:7050`
- **Peer0**: `localhost:7051`
- **Peer1**: `localhost:8051`
- **CA**: `localhost:7054`
- **CouchDB0**: `localhost:5984` (Fauxton: http://localhost:5984/_utils)
- **CouchDB1**: `localhost:5985` (Fauxton: http://localhost:5985/_utils)

## Конфигурация для сервисов

```json
{
  "Ledger": {
    "UseMock": false,
    "ChaincodeEndpoint": "http://localhost:8080"
  },
  "Fabric": {
    "PeerEndpoint": "http://localhost:7051",
    "ChannelName": "cfa-main",
    "MspId": "OisDevMSP",
    "TlsCertPath": "../../ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.crt",
    "TlsKeyPath": "../../ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.key",
    "TlsRootCertPath": "../../ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt"
  }
}
```

## Проверка работы

### Проверить контейнеры
```bash
docker ps | grep -E "orderer|peer|ca|couchdb"
```

### Проверить каналы
```bash
docker exec peer0.ois-dev.example.com peer channel list
```

### Проверить установленный chaincode
```bash
docker exec peer0.ois-dev.example.com peer lifecycle chaincode queryinstalled
```

### Пример invoke
```bash
./scripts/invoke-example.sh
```

## Troubleshooting

См. `docs/dlt/dev-network.md` для распространенных ошибок и решений.

## Дополнительная документация

- `docs/dlt/dev-network.md` - Полная документация
- `docs/architecture/13-HLF-Network-Design.md` - Архитектура сети

