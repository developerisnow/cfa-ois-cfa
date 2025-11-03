# СХЕМА DLT-СЕТИ
## Hyperledger Fabric - Архитектура и конфигурация

**Версия:** {{VERSION}}  
**Дата:** {{DATE}}  
**Оператор:** {{COMPANY_NAME}} (ОГРН: {{OGRN}}, ИНН: {{INN}})

---

## 1. ОБЩИЕ ПОЛОЖЕНИЯ

### 1.1. Назначение

Настоящий документ описывает архитектуру и конфигурацию распределенной сети Hyperledger Fabric для оператора информационной системы цифровых финансовых активов (ОИС ЦФА). Сеть обеспечивает безопасное хранение и обработку операций с ЦФА.

### 1.2. Технологический стек

**Hyperledger Fabric:** 2.5+  
**Консенсус:** Raft  
**Криптография:** ECDSA, SHA-256  
**Сеть:** mTLS, TLS 1.3  
**Хранение:** CouchDB  
**Мониторинг:** Prometheus, Grafana  

---

## 2. АРХИТЕКТУРА СЕТИ

### 2.1. Общая схема

```
┌─────────────────────────────────────────────────────────────┐
│                    ОИС ЦФА DLT СЕТЬ                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │   Orderer   │  │   Orderer   │  │   Orderer   │        │
│  │   Node 1    │  │   Node 2    │  │   Node 3    │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
│           │              │              │                 │
│           └──────────────┼──────────────┘                 │
│                          │                                │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │    Peer     │  │    Peer     │  │    Peer     │        │
│  │   Node 1    │  │   Node 2    │  │   Node 3    │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
│           │              │              │                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │     CA      │  │     CA      │  │     CA      │        │
│  │   (Root)    │  │  (Org1)     │  │  (Org2)     │        │
│  └─────────────┘  └─────────────┘  └─────────────┘        │
└─────────────────────────────────────────────────────────────┘
```

### 2.2. Компоненты сети

**Orderer Service:**
- 3 узла для обеспечения отказоустойчивости
- Консенсус Raft для высокой производительности
- Географически распределенное размещение
- Автоматическое переключение при сбоях

**Peer Nodes:**
- 3 узла для обеспечения отказоустойчивости
- Полная репликация данных
- Поддержка chaincode
- Интеграция с внешними системами

**Certificate Authority (CA):**
- Root CA для управления сертификатами
- Организационные CA для участников
- Автоматическая ротация сертификатов
- Интеграция с HSM

---

## 3. ОРГАНИЗАЦИОННАЯ СТРУКТУРА

### 3.1. Участники сети

**Организация 1: ОИС ЦФА (Оператор)**
- Роль: Оператор информационной системы
- Ответственность: Управление сетью, выпуск ЦФА
- Узлы: 2 Peer, 1 Orderer
- Права: Полные права на все операции

**Организация 2: Банк-партнер**
- Роль: Финансовый партнер
- Ответственность: Обеспечение ликвидности, расчеты
- Узлы: 1 Peer, 1 Orderer
- Права: Чтение, валидация операций

**Организация 3: Регулятор (ЦБ РФ)**
- Роль: Надзорный орган
- Ответственность: Мониторинг, аудит
- Узлы: 1 Peer, 1 Orderer
- Права: Только чтение, аудит

### 3.2. Роли и права

**Администратор сети:**
- Управление конфигурацией
- Добавление/удаление участников
- Обновление chaincode
- Мониторинг сети

**Оператор:**
- Выпуск ЦФА
- Управление пользователями
- Обработка операций
- Формирование отчетов

**Аудитор:**
- Просмотр всех операций
- Формирование отчетов
- Мониторинг соответствия
- Выявление нарушений

---

## 4. КАНАЛЫ И ПРИВАТНОСТЬ

### 4.1. Структура каналов

**Основной канал (main-channel):**
- Все участники сети
- Общие операции
- Публичные данные
- Системные транзакции

**Приватный канал (private-channel):**
- Только ОИС ЦФА и Банк-партнер
- Финансовые операции
- Конфиденциальные данные
- Расчеты и платежи

**Аудиторский канал (audit-channel):**
- Все участники
- Аудиторские данные
- Отчеты и логи
- Соответствие требованиям

### 4.2. Приватные коллекции

**Коллекция 1: Пользовательские данные**
- Персональные данные
- Документы KYC
- Финансовая информация
- Доступ: только ОИС ЦФА

**Коллекция 2: Операционные данные**
- Детали операций
- Внутренние процессы
- Техническая информация
- Доступ: ОИС ЦФА, Банк-партнер

**Коллекция 3: Аудиторские данные**
- Логи операций
- Отчеты о нарушениях
- Статистика
- Доступ: все участники

---

## 5. CHAINCODE (СМАРТ-КОНТРАКТЫ)

### 5.1. Issuance Chaincode

**Функции:**
- `issueCFA(issuer, cfaId, amount, metadata)` - Выпуск ЦФА
- `getCFABalance(holder, cfaId)` - Получение баланса
- `getCFAInfo(cfaId)` - Информация о ЦФА
- `listCFAs(issuer)` - Список ЦФА эмитента

**Логика:**
- Проверка прав эмитента
- Валидация параметров
- Обновление балансов
- Логирование операции

### 5.2. Transfer Chaincode

**Функции:**
- `transferCFA(from, to, cfaId, amount, metadata)` - Перевод ЦФА
- `getTransferHistory(holder, cfaId)` - История переводов
- `validateTransfer(from, to, cfaId, amount)` - Валидация перевода

**Логика:**
- Проверка баланса отправителя
- Проверка прав получателя
- Выполнение перевода
- Обновление состояния

### 5.3. Registry Chaincode

**Функции:**
- `registerUser(userId, userData)` - Регистрация пользователя
- `updateUser(userId, userData)` - Обновление данных
- `getUser(userId)` - Получение данных пользователя
- `listUsers(filter)` - Список пользователей

**Логика:**
- Валидация данных пользователя
- Проверка уникальности
- Обновление реестра
- Аудит изменений

### 5.4. Audit Chaincode

**Функции:**
- `logOperation(operationId, operationData)` - Логирование операции
- `getAuditLog(filter)` - Получение логов
- `generateReport(period, type)` - Формирование отчета
- `validateCompliance(operationId)` - Проверка соответствия

**Логика:**
- Неизменяемое логирование
- Индексация по параметрам
- Формирование отчетов
- Проверка соответствия

---

## 6. КОНФИГУРАЦИЯ СЕТИ

### 6.1. Orderer Configuration

**configtx.yaml:**
```yaml
Organizations:
  - &OrdererOrg
    Name: OrdererOrg
    ID: OrdererMSP
    MSPDir: crypto-config/ordererOrganizations/example.com/msp
    Policies:
      Readers:
        Type: Signature
        Rule: "OR('OrdererMSP.member')"
      Writers:
        Type: Signature
        Rule: "OR('OrdererMSP.member')"
      Admins:
        Type: Signature
        Rule: "OR('OrdererMSP.admin')"

Orderer: &OrdererDefaults
  OrdererType: etcdraft
  EtcdRaft:
    Consenters:
      - Host: orderer1.example.com
        Port: 7050
        ClientTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer1.example.com/tls/server.crt
        ServerTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer1.example.com/tls/server.crt
      - Host: orderer2.example.com
        Port: 7050
        ClientTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer2.example.com/tls/server.crt
        ServerTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer2.example.com/tls/server.crt
      - Host: orderer3.example.com
        Port: 7050
        ClientTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer3.example.com/tls/server.crt
        ServerTLSCert: crypto-config/ordererOrganizations/example.com/orderers/orderer3.example.com/tls/server.crt
  BatchTimeout: 2s
  BatchSize:
    MaxMessageCount: 500
    AbsoluteMaxBytes: 10 MB
    PreferredMaxBytes: 2 MB
  MaxChannels: 0
  Capabilities:
    V2_0: true
```

### 6.2. Peer Configuration

**core.yaml:**
```yaml
peer:
  id: peer0.org1.example.com
  networkId: dev
  listenAddress: 0.0.0.0:7051
  chaincodeListenAddress: 0.0.0.0:7052
  address: peer0.org1.example.com:7051
  addressAutoDetect: false
  gomaxprocs: -1
  keepalive:
    minInterval: 60s
    client:
      interval: 60s
      timeout: 20s
    deliveryClient:
      interval: 60s
      timeout: 20s
  gossip:
    bootstrap: 127.0.0.1:7051
    useLeaderElection: true
    orgLeader: false
    membershipTrackerInterval: 5s
    maxBlockCountToStore: 100
    maxPropagationBurstLatency: 10ms
    maxPropagationBurstSize: 10
    propagateIterations: 1
    propagatePeerNum: 3
    pullInterval: 4s
    pullPeerNum: 3
    requestStateInfoInterval: 4s
    publishStateInfoInterval: 4s
    stateInfoRetentionInterval: 0s
    publishCertPeriod: 10s
    skipBlockVerification: false
    dialTimeout: 3s
    connTimeout: 2s
    recvBuffSize: 20
    sendBuffSize: 200
    digestWaitTime: 1s
    requestWaitTime: 1s
    responseWaitTime: 2s
    aliveTimeInterval: 5s
    aliveExpirationTimeout: 25s
    reconnectInterval: 25s
    externalEndpoint: ""
    election:
      startupGracePeriod: 15s
      membershipSampleInterval: 1s
      leaderAliveThreshold: 10s
      leaderElectionDuration: 5s
    pvtData:
      pullRetryThreshold: 60s
      transientstoreMaxBlockRetention: 1000
      pushAckTimeout: 3s
      btlPullMargin: 10
      reconcileBatchSize: 10
      reconcileSleepInterval: 1m
      reconciliationEnabled: true
      skipPullingInvalidTransactionsDuringCommit: false
    state:
      enabled: true
      checkInterval: 10s
      responseTimeout: 3s
      batchSize: 10
      blockBufferSize: 100
      maxRetries: 3
  events:
    address: 0.0.0.0:7053
    buffersize: 100
    timeout: 10ms
    timewindow: 15m
    keepalive:
      minInterval: 60s
    sendTimeout: 60s
  fileSystemPath: /var/hyperledger/production
  BCCSP:
    Default: SW
    SW:
      Hash: SHA2
      Security: 256
      FileKeyStore:
        KeyStore:
  mspConfigPath: msp
  localMspId: Org1MSP
  client:
    connTimeout: 3s
  deliveryclient:
    reconnectTotalTimeThreshold: 3600s
    connTimeout: 3s
    reConnectBackoffThreshold: 3600s
  localMspType: bccsp
  profile:
    enabled: false
    address: 0.0.0.0:6060
  adminService:
    enabled: false
  handlers:
    authFilters:
      - name: DefaultAuth
      - name: ExpirationCheck
    decorators:
      - name: DefaultDecorator
    endorsers:
      escc:
        name: DefaultEndorsement
        library:
    validators:
      vscc:
        name: DefaultValidation
        library:
  validatorPoolSize: 0
  discovery:
    enabled: true
    authCacheEnabled: true
    authCacheMaxSize: 1000
    authCachePurgeRetentionRatio: 0.75
  limits:
    concurrency:
      qscc: 500
      cscc: 500
      lscc: 500
      escc: 500
      vscc: 500
      qscc: 500
    concurrency:
      qscc: 500
      cscc: 500
      lscc: 500
      escc: 500
      vscc: 500
      qscc: 500
```

### 6.3. CA Configuration

**fabric-ca-server-config.yaml:**
```yaml
version: 1.4.0
port: 7054
debug: false
crlsizelimit: 512000
tls:
  enabled: true
  certfile: tls-cert.pem
  keyfile: tls-key.pem
ca:
  name: ca-org1
  keyfile: ca-key.pem
  certfile: ca-cert.pem
  chainfile: ca-chain.pem
db:
  type: sqlite3
  datasource: fabric-ca-server.db
  tls:
    enabled: false
    certfiles:
    client:
      certfile:
      keyfile:
registry:
  maxenrollments: -1
  identities:
    - name: admin
      pass: adminpw
      type: client
      affiliation: ""
      attrs:
        hf.Registrar.Roles: "*"
        hf.Registrar.DelegateRoles: "*"
        hf.Revoker: true
        hf.IntermediateCA: true
        hf.GenCRL: true
        hf.Registrar.Attributes: "*"
        hf.AffiliationMgr: true
affiliations:
  org1:
    - department1
    - department2
  org2:
    - department1
ldap:
  enabled: false
  url: ldap://<adminDN>:<adminPassword>@<host>:<port>/<base>
  userfilter: (uid=%s)
  attribute:
    names: ['uid','member']
    converters:
      - name:
        value:
    maps:
      groups:
        - name:
          value:
operations:
  listenAddress: 127.0.0.1:9443
  tls:
    enabled: false
    certfile:
    keyfile:
metrics:
  provider: prometheus
  statsd:
    network: udp
    address: 127.0.0.1:8125
    writeInterval: 10s
    prefix: server
```

---

## 7. БЕЗОПАСНОСТЬ

### 7.1. Криптография

**Алгоритмы:**
- ECDSA P-256 для подписей
- SHA-256 для хеширования
- AES-256 для шифрования
- RSA-2048 для сертификатов

**Управление ключами:**
- HSM для корневых ключей
- Автоматическая ротация
- Хранение в Vault
- Аудит доступа

### 7.2. Сетевая безопасность

**mTLS:**
- Взаимная аутентификация
- Шифрование трафика
- Проверка сертификатов
- Отзыв скомпрометированных

**Firewall:**
- Разрешение только необходимых портов
- Блокировка внешнего доступа
- Мониторинг трафика
- Логирование соединений

### 7.3. Контроль доступа

**RBAC:**
- Роли и права
- Принцип минимальных привилегий
- Регулярный пересмотр
- Аудит доступа

**ABAC:**
- Атрибуты пользователей
- Контекстные правила
- Динамическое управление
- Гибкая политика

---

## 8. МОНИТОРИНГ И ОБСЛУЖИВАНИЕ

### 8.1. Мониторинг сети

**Метрики:**
- Производительность узлов
- Использование ресурсов
- Количество транзакций
- Время обработки

**Алерты:**
- Отказ узла
- Высокая нагрузка
- Ошибки обработки
- Нарушения безопасности

### 8.2. Логирование

**События:**
- Создание блоков
- Обработка транзакций
- Ошибки системы
- Административные действия

**Хранение:**
- Централизованное логирование
- Ротация логов
- Архивирование
- Поиск и анализ

---

## 9. РАЗВЕРТЫВАНИЕ И МАСШТАБИРОВАНИЕ

### 9.1. Развертывание

**Docker Compose:**
```yaml
version: '3.8'
services:
  orderer1:
    image: hyperledger/fabric-orderer:2.5
    container_name: orderer1
    environment:
      - FABRIC_LOGGING_SPEC=INFO
      - ORDERER_GENERAL_LISTENADDRESS=0.0.0.0
      - ORDERER_GENERAL_GENESISPROFILE=TwoOrgsOrdererGenesis
      - ORDERER_GENERAL_LOCALMSPID=OrdererMSP
      - ORDERER_GENERAL_LOCALMSPDIR=/var/hyperledger/orderer/msp
      - ORDERER_GENERAL_TLS_ENABLED=true
      - ORDERER_GENERAL_TLS_PRIVATEKEY=/var/hyperledger/orderer/tls/server.key
      - ORDERER_GENERAL_TLS_CERTIFICATE=/var/hyperledger/orderer/tls/server.crt
      - ORDERER_GENERAL_TLS_ROOTCAS=[/var/hyperledger/orderer/tls/ca.crt]
      - ORDERER_GENERAL_CLUSTER_CLIENTCERTIFICATE=/var/hyperledger/orderer/tls/server.crt
      - ORDERER_GENERAL_CLUSTER_CLIENTPRIVATEKEY=/var/hyperledger/orderer/tls/server.key
      - ORDERER_GENERAL_CLUSTER_ROOTCAS=[/var/hyperledger/orderer/tls/ca.crt]
    working_dir: /opt/gopath/src/github.com/hyperledger/fabric
    command: orderer
    volumes:
      - ./channel-artifacts/genesis.block:/var/hyperledger/orderer/orderer.genesis.block
      - ./crypto-config/ordererOrganizations/example.com/orderers/orderer1.example.com/msp:/var/hyperledger/orderer/msp
      - ./crypto-config/ordererOrganizations/example.com/orderers/orderer1.example.com/tls/:/var/hyperledger/orderer/tls
      - orderer1.example.com:/var/hyperledger/production/orderer
    ports:
      - 7050:7050
    networks:
      - fabric-network

  peer0.org1.example.com:
    image: hyperledger/fabric-peer:2.5
    container_name: peer0.org1.example.com
    environment:
      - CORE_VM_ENDPOINT=unix:///host/var/run/docker.sock
      - CORE_VM_DOCKER_HOSTCONFIG_NETWORKMODE=fabric-network
      - FABRIC_LOGGING_SPEC=INFO
      - CORE_PEER_TLS_ENABLED=true
      - CORE_PEER_GOSSIP_USELEADERELECTION=true
      - CORE_PEER_GOSSIP_ORGLEADER=false
      - CORE_PEER_PROFILE_ENABLED=true
      - CORE_PEER_TLS_CERT_FILE=/etc/hyperledger/fabric/tls/server.crt
      - CORE_PEER_TLS_KEY_FILE=/etc/hyperledger/fabric/tls/server.key
      - CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt
      - CORE_PEER_ID=peer0.org1.example.com
      - CORE_PEER_ADDRESS=peer0.org1.example.com:7051
      - CORE_PEER_LISTENADDRESS=0.0.0.0:7051
      - CORE_PEER_CHAINCODEADDRESS=peer0.org1.example.com:7052
      - CORE_PEER_CHAINCODELISTENADDRESS=0.0.0.0:7052
      - CORE_PEER_GOSSIP_BOOTSTRAP=peer0.org1.example.com:7051
      - CORE_PEER_GOSSIP_EXTERNALENDPOINT=peer0.org1.example.com:7051
      - CORE_PEER_LOCALMSPID=Org1MSP
    volumes:
      - /var/run/:/host/var/run/
      - ./crypto-config/peerOrganizations/org1.example.com/peers/peer0.org1.example.com/msp:/etc/hyperledger/fabric/msp
      - ./crypto-config/peerOrganizations/org1.example.com/peers/peer0.org1.example.com/tls:/etc/hyperledger/fabric/tls
      - peer0.org1.example.com:/var/hyperledger/production
    working_dir: /opt/gopath/src/github.com/hyperledger/fabric/peer
    command: peer node start
    ports:
      - 7051:7051
    depends_on:
      - orderer1
    networks:
      - fabric-network

  ca.org1.example.com:
    image: hyperledger/fabric-ca:1.5
    container_name: ca.org1.example.com
    environment:
      - FABRIC_CA_HOME=/etc/hyperledger/fabric-ca-server
      - FABRIC_CA_SERVER_CA_NAME=ca-org1
      - FABRIC_CA_SERVER_TLS_ENABLED=true
      - FABRIC_CA_SERVER_PORT=7054
    ports:
      - 7054:7054
    command: sh -c 'fabric-ca-server start -b admin:adminpw -d'
    volumes:
      - ./crypto-config/peerOrganizations/org1.example.com/ca/:/etc/hyperledger/fabric-ca-server-config
      - ca.org1.example.com:/etc/hyperledger/fabric-ca-server
    networks:
      - fabric-network

networks:
  fabric-network:
    driver: bridge
volumes:
  orderer1.example.com:
  peer0.org1.example.com:
  ca.org1.example.com:
```

### 9.2. Kubernetes развертывание

**orderer-deployment.yaml:**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: orderer1
  namespace: fabric
spec:
  replicas: 1
  selector:
    matchLabels:
      app: orderer1
  template:
    metadata:
      labels:
        app: orderer1
    spec:
      containers:
      - name: orderer1
        image: hyperledger/fabric-orderer:2.5
        ports:
        - containerPort: 7050
        env:
        - name: FABRIC_LOGGING_SPEC
          value: "INFO"
        - name: ORDERER_GENERAL_LISTENADDRESS
          value: "0.0.0.0"
        - name: ORDERER_GENERAL_GENESISPROFILE
          value: "TwoOrgsOrdererGenesis"
        - name: ORDERER_GENERAL_LOCALMSPID
          value: "OrdererMSP"
        - name: ORDERER_GENERAL_LOCALMSPDIR
          value: "/var/hyperledger/orderer/msp"
        - name: ORDERER_GENERAL_TLS_ENABLED
          value: "true"
        volumeMounts:
        - name: orderer-msp
          mountPath: /var/hyperledger/orderer/msp
        - name: orderer-tls
          mountPath: /var/hyperledger/orderer/tls
        - name: orderer-genesis
          mountPath: /var/hyperledger/orderer/orderer.genesis.block
        - name: orderer-data
          mountPath: /var/hyperledger/production/orderer
      volumes:
      - name: orderer-msp
        secret:
          secretName: orderer-msp
      - name: orderer-tls
        secret:
          secretName: orderer-tls
      - name: orderer-genesis
        configMap:
          name: orderer-genesis
      - name: orderer-data
        persistentVolumeClaim:
          claimName: orderer-data
```

---

## 10. ТЕСТИРОВАНИЕ И ВАЛИДАЦИЯ

### 10.1. Функциональное тестирование

**Тест-кейсы:**
- Выпуск ЦФА
- Перевод ЦФА
- Регистрация пользователей
- Формирование отчетов

**Автоматизация:**
- Unit тесты для chaincode
- Интеграционные тесты
- E2E тесты
- Нагрузочное тестирование

### 10.2. Тестирование безопасности

**Пентестинг:**
- Тестирование уязвимостей
- Анализ конфигурации
- Тестирование доступа
- Анализ трафика

**Аудит:**
- Проверка соответствия
- Анализ логов
- Валидация политик
- Тестирование восстановления

---

## 11. ОБСЛУЖИВАНИЕ И ОБНОВЛЕНИЯ

### 11.1. Обслуживание

**Ежедневно:**
- Проверка состояния узлов
- Анализ логов
- Мониторинг производительности
- Проверка резервных копий

**Еженедельно:**
- Обновление статистики
- Очистка логов
- Проверка безопасности
- Тестирование восстановления

**Ежемесячно:**
- Полный аудит системы
- Обновление сертификатов
- Тестирование производительности
- Планирование обновлений

### 11.2. Обновления

**Планирование:**
- Анализ изменений
- Оценка рисков
- Планирование времени
- Подготовка отката

**Выполнение:**
- Создание backup
- Обновление узлов
- Тестирование функциональности
- Валидация производительности

**Откат:**
- Остановка обновления
- Восстановление из backup
- Валидация восстановления
- Анализ причин

---

## 12. ДОКУМЕНТАЦИЯ И ПОДДЕРЖКА

### 12.1. Техническая документация

**Архитектурная документация:**
- Схемы сети
- Конфигурационные файлы
- Процедуры развертывания
- Руководства по обслуживанию

**Операционная документация:**
- Процедуры мониторинга
- Процедуры восстановления
- Процедуры обновления
- Контакты поддержки

### 12.2. Обучение персонала

**Техническая команда:**
- Архитектура Hyperledger Fabric
- Управление сетью
- Разработка chaincode
- Мониторинг и обслуживание

**Операционная команда:**
- Использование системы
- Обработка инцидентов
- Формирование отчетов
- Соблюдение процедур

---

**Дата создания:** {{DATE}}  
**Автор:** {{AUTHOR}}  
**Версия:** {{VERSION}}  
**Статус:** Утверждено  
**Следующий пересмотр:** {{NEXT_REVIEW_DATE}}

---

## ПРИЛОЖЕНИЯ

### Приложение A: Схемы сети
### Приложение B: Конфигурационные файлы
### Приложение C: Скрипты развертывания
### Приложение D: Процедуры тестирования
### Приложение E: История изменений
