# Hyperledger Fabric Dev Network

**Версия:** 1.0  
**Дата:** 2024-12-19  
**Окружение:** Development

---

## Обзор

Данная документация описывает настройку и использование локальной dev-сети Hyperledger Fabric для разработки и тестирования ОИС ЦФА.

### Компоненты сети

- **Orderer**: 1 узел (Raft для dev, single-node)
- **Peers**: 2 узла (peer0, peer1)
- **CA**: 1 Certificate Authority
- **CouchDB**: 2 экземпляра (для каждого peer)
- **Channel**: `cfa-main`
- **Organization**: `ois-dev` (OisDevMSP)

---

## Быстрый старт

### 1. Подготовка окружения

**Требования:**
- Docker и Docker Compose
- Bash (или Git Bash на Windows)
- `jq` (для парсинга JSON в скриптах)

**Установка на Windows:**
```powershell
# Установить Docker Desktop
# Установить Git Bash или WSL2
```

### 2. Запуск сети

```bash
cd ops/fabric
chmod +x *.sh scripts/*.sh
./dev-up.sh
```

Этот скрипт:
1. Генерирует crypto-material
2. Создает genesis block
3. Создает channel транзакцию
4. Запускает все контейнеры
5. Проверяет готовность сети

### 3. Создание канала

```bash
./scripts/create-channel.sh
```

### 4. Установка chaincode

```bash
./scripts/install-chaincode.sh
./scripts/approve-chaincode.sh
```

### 4. Проверка здоровья

```bash
./scripts/health-check.sh
```

---

## Структура артефактов

### Crypto Material

```
ops/fabric/crypto-config/
├── ordererOrganizations/
│   └── example.com/
│       ├── orderers/orderer.example.com/
│       │   ├── msp/
│       │   └── tls/
│       └── msp/
└── peerOrganizations/
    └── ois-dev.example.com/
        ├── peers/
        │   ├── peer0.ois-dev.example.com/
        │   │   ├── msp/
        │   │   └── tls/
        │   └── peer1.ois-dev.example.com/
        │       ├── msp/
        │       └── tls/
        ├── users/
        │   └── Admin@ois-dev.example.com/
        │       └── msp/
        └── ca/
```

**Пути к сертификатам:**
- Orderer TLS CA: `crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls/ca.crt`
- Peer0 TLS CA: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt`
- Peer1 TLS CA: `crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls/ca.crt`
- Admin MSP: `crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp`

### Channel Artifacts

```
ops/fabric/channel-artifacts/
├── genesis.block          # Genesis block для orderer
└── cfa-main.tx           # Channel creation transaction
```

---

## Endpoints сети

### Orderer
- **Address**: `localhost:7050`
- **Container**: `orderer.example.com`
- **TLS**: Enabled

### Peers
- **Peer0**: `localhost:7051` (container: `peer0.ois-dev.example.com`)
- **Peer1**: `localhost:8051` (container: `peer1.ois-dev.example.com`)
- **Events Peer0**: `localhost:7053`
- **Events Peer1**: `localhost:8053`
- **TLS**: Enabled

### CA
- **Address**: `localhost:7054`
- **Container**: `ca.ois-dev.example.com`
- **Admin**: `admin:adminpw`

### CouchDB
- **CouchDB0** (для Peer0): `localhost:5984`
- **CouchDB1** (для Peer1): `localhost:5985`
- **Credentials**: `admin:adminpw`

---

## Chaincode

### Установленные chaincode

1. **issuance** (v1.0)
   - Methods: `Issue`, `Close`, `Get`
   - Channel: `cfa-main`

2. **registry** (v1.0)
   - Methods: `Transfer`, `Redeem`, `GetHistory`
   - Channel: `cfa-main`

### Примеры вызовов

#### Issue
```bash
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
  -e CORE_PEER_TLS_ENABLED=true \
  -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
  -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
  -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
  peer0.ois-dev.example.com \
  peer chaincode invoke \
  -o orderer.example.com:7050 \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
  -C cfa-main -n issuance \
  --peerAddresses peer0.ois-dev.example.com:7051 \
  --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
  --peerAddresses peer1.ois-dev.example.com:8051 \
  --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
  -c '{"function":"Issue","Args":["issuance-id","asset-id","issuer-id","1000000","1000","2024-01-01","2025-01-01","{}"]}'
```

#### Transfer
```bash
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
  -e CORE_PEER_TLS_ENABLED=true \
  -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
  -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
  -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
  peer0.ois-dev.example.com \
  peer chaincode invoke \
  -o orderer.example.com:7050 \
  --tls --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
  -C cfa-main -n registry \
  --peerAddresses peer0.ois-dev.example.com:7051 \
  --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
  --peerAddresses peer1.ois-dev.example.com:8051 \
  --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
  -c '{"function":"Transfer","Args":["","holder-id","issuance-id","10000"]}'
```

#### Query
```bash
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
  -e CORE_PEER_TLS_ENABLED=true \
  -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
  -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
  -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
  peer0.ois-dev.example.com \
  peer chaincode query \
  -C cfa-main -n issuance \
  -c '{"function":"Get","Args":["issuance-id"]}'
```

---

## Конфигурация сервисов

### Environment Variables

Для подключения сервисов к Fabric сети:

```bash
# .env или appsettings.json
Ledger__UseMock=false
Ledger__ChaincodeEndpoint=http://localhost:8080

Fabric__PeerEndpoint=http://localhost:7051
Fabric__ChannelName=cfa-main
Fabric__MspId=OisDevMSP
Fabric__TlsCertPath=ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.crt
Fabric__TlsKeyPath=ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/server.key
Fabric__TlsRootCertPath=ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt
```

### Gateway Service

Для dev-окружения можно использовать упрощенный HTTP Gateway, который вызывает chaincode через docker exec.

**В production**: использовать Fabric Gateway SDK (gRPC) или Fabric SDK для .NET.

---

## Управление сетью

### Остановка

```bash
./dev-down.sh
```

### Сброс (удаление всех данных)

```bash
./dev-reset.sh
```

**Внимание**: Это удалит все данные, включая crypto-material и channel artifacts!

### Логи

```bash
# Логи orderer
docker logs orderer.example.com

# Логи peer0
docker logs peer0.ois-dev.example.com

# Логи peer1
docker logs peer1.ois-dev.example.com

# Логи CA
docker logs ca.ois-dev.example.com

# Следить за логами в реальном времени
docker logs -f peer0.ois-dev.example.com
```

---

## Распространенные ошибки

### 1. Orderer не запускается

**Симптомы:**
```
Error: failed to create deliver client: context deadline exceeded
```

**Решение:**
- Проверьте, что genesis.block существует: `ls ops/fabric/channel-artifacts/genesis.block`
- Проверьте права доступа к crypto-config
- Проверьте логи: `docker logs orderer.example.com`

### 2. Peer не может присоединиться к каналу

**Симптомы:**
```
Error: proposal failed (err: rpc error: code = Unavailable desc = connection closed)
```

**Решение:**
- Убедитесь, что orderer запущен: `docker ps | grep orderer`
- Проверьте TLS сертификаты: `ls ops/fabric/crypto-config/.../tls/`
- Проверьте, что channel block скопирован в peer

### 3. Chaincode не устанавливается

**Симптомы:**
```
Error: chaincode package not found
```

**Решение:**
- Убедитесь, что chaincode скомпилирован (Go модули)
- Проверьте путь к chaincode в скрипте
- Убедитесь, что tar.gz файл создан

### 4. CouchDB недоступен

**Симптомы:**
```
Error: failed to connect to CouchDB
```

**Решение:**
- Проверьте, что CouchDB запущен: `docker ps | grep couchdb`
- Проверьте credentials: `admin:adminpw`
- Проверьте порты: `curl http://localhost:5984/_up`

### 5. TLS ошибки

**Симптомы:**
```
Error: x509: certificate signed by unknown authority
```

**Решение:**
- Убедитесь, что все TLS сертификаты сгенерированы
- Проверьте пути к сертификатам в docker-compose.yml
- Перегенерируйте crypto-material: `./dev-reset.sh && ./dev-up.sh`

---

## Мониторинг

### CouchDB Fauxton UI

- **Peer0 CouchDB**: http://localhost:5984/_utils
- **Peer1 CouchDB**: http://localhost:5985/_utils
- **Credentials**: `admin:adminpw`

### Проверка блоков

```bash
# Получить последний блок
docker exec peer0.ois-dev.example.com \
  peer channel getinfo -c cfa-main
```

### Проверка установленного chaincode

```bash
docker exec peer0.ois-dev.example.com \
  peer lifecycle chaincode queryinstalled
```

---

## Интеграция с сервисами

### Issuance Service

```csharp
// appsettings.json
{
  "Ledger": {
    "UseMock": false,
    "ChaincodeEndpoint": "http://localhost:8080"
  }
}
```

### Registry Service

Аналогично, настройка через `Ledger:ChaincodeEndpoint`.

### Retry Policy

Адаптеры используют Polly с exponential backoff:
- **Retry Count**: 3
- **Backoff**: 2s, 4s, 8s
- **Circuit Breaker**: 5 failures → 30s break

---

## Тестирование

### E2E тест через Playwright

```typescript
// tests/e2e/ledger.spec.ts
test('Publish issuance → Transfer → Redeem', async ({ page }) => {
  // 1. Publish issuance
  // 2. Verify txHash returned
  // 3. Query ledger state
  // 4. Transfer
  // 5. Redeem
  // 6. Verify final state
});
```

### Unit тесты chaincode

```bash
cd chaincode/issuance
go test -v
```

---

## Команды для быстрого доступа

```bash
# Полный цикл запуска
cd ops/fabric
./dev-up.sh
./scripts/create-channel.sh
./scripts/install-chaincode.sh
./scripts/approve-chaincode.sh
./scripts/health-check.sh

# Пример вызова
./scripts/invoke-example.sh

# Остановка
./dev-down.sh

# Сброс
./dev-reset.sh
```

---

## Пути к crypto-material (для reference)

### Orderer
- **MSP**: `ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/msp`
- **TLS**: `ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls`
- **CA Cert**: `ops/fabric/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/tls/ca.crt`

### Peer0
- **MSP**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/msp`
- **TLS**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls`
- **CA Cert**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt`

### Peer1
- **MSP**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/msp`
- **TLS**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls`
- **CA Cert**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer1.ois-dev.example.com/tls/ca.crt`

### Admin User
- **MSP**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp`
- **Sign Cert**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp/signcerts/Admin@ois-dev.example.com-cert.pem`
- **Private Key**: `ops/fabric/crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp/keystore/`

---

## Следующие шаги

1. **Production сеть**: Настроить multi-org, multi-orderer конфигурацию
2. **Fabric Gateway SDK**: Интегрировать официальный SDK вместо HTTP адаптера
3. **Мониторинг**: Настроить Prometheus/Grafana для метрик
4. **Security**: Настроить HSM для production ключей
5. **Backup**: Настроить автоматическое резервное копирование ledger state

---

**Автор:** OIS Development Team  
**Последнее обновление:** 2024-12-19

