---
created: 2025-11-11
updated: 2025-11-18-1619
author: AlexA
co-authrors: gpt5
type: diagram
tags: c4
previous:
next:  [reposcan]
related: []
---

# Project map

## 1. C1–C4 диаграммы Mermaid

### 1.1 C1 — System Context OIS‑CFA и внешние акторы

Основано на PROJECT-CONTEXT, архитектурных openapi‑/asyncapi‑доках и описании интеграций с ESIA, банком, EDO и Fabric.

```mermaid
graph LR
  %% People
  subgraph Users
    issuer[Person: Issuer]
    investor[Person: Investor]
    broker[Person: Broker]
    backoffice[Person: Backoffice / Compliance]
  end

  %% Core system
  ois[System: OIS-CFA Platform]

  %% External systems
  subgraph External_Systems
    esia[[System: ESIA OIDC]]
    bank[[System: Bank Nominal Core]]
    edo[[System: EDO Provider]]
    fabric[[System: Fabric DLT Network]]
    regulator[[System: Regulator / CBR]]
  end

  %% People → System
  issuer -->|Создание/управление выпусками, отчётность| ois
  investor -->|Инвестиции, портфель, история операций| ois
  broker -->|Брокерские операции, маркетплейс| ois
  backoffice -->|KYC/AML, квалификация, аудит| ois

  %% System → External systems
  ois -->|OIDC login, профиль| esia
  ois -->|Платежи, резервирование, списания| bank
  ois -->|Юр. документы, подписание| edo
  ois -->|Реестр активов, сделки на DLT| fabric
  ois -->|Регуляторная отчётность, выгрузки| regulator
```

---

### 1.2 C2 — Containers фронты, gateway, сервисы, инфраструктура

Контейнеры взяты из дерева `apps/*`, `services/*`, `ops/infra/*`, `packages/contracts/*` и API/Event‑матрицы в PROJECT-CONTEXT.

```mermaid
graph TB
  %% Frontend portals
  subgraph Frontends
    fe_issuer[Container: Portal Issuer Next.js\napps/portal-issuer]
    fe_investor[Container: Portal Investor Next.js\napps/portal-investor]
    fe_backoffice[Container: Backoffice Next.js\napps/backoffice]
    fe_broker[Container: Broker Portal Next.js\napps/broker-portal]
  end

  %% Edge layer
  subgraph Edge
    api_gw[Container: API Gateway .NET + YARP\napps/api-gateway]
    keycloak[Container: Keycloak IdP]
  end

  %% Domain services
  subgraph Domain_Services
    svc_issuance[Container: Issuance Service .NET\nservices/issuance]
    svc_registry[Container: Registry Service .NET\nservices/registry]
    svc_settlement[Container: Settlement Service .NET\nservices/settlement]
    svc_compliance[Container: Compliance Service .NET\nservices/compliance]
    svc_identity[Container: Identity Service .NET\nservices/identity]
    svc_fabric_gw[Container: Fabric Gateway .NET\nservices/fabric-gateway]
  end

  %% Integrations
  subgraph Integrations
    svc_bank[Container: Bank Nominal Adapter\nservices/integrations/bank-nominal]
    svc_esia[Container: ESIA Adapter\nservices/integrations/esia-adapter]
    svc_edo[Container: EDO Connector\nservices/integrations/edo-connector]
  end

  %% Data & infra
  subgraph Data_and_Infra
    pg[Container: PostgreSQL cluster\nper service schema]
    kafka[Container: Kafka broker\nevents from AsyncAPI]
    minio[Container: MinIO / Object storage]
    fabric[Container: Fabric network\norderer + peers + chaincode]
    otel[Container: OTel Collector / Prometheus / Grafana]
  end

  %% Users → Frontends
  issuer[Issuer] --> fe_issuer
  investor[Investor] --> fe_investor
  backoffice[Backoffice] --> fe_backoffice
  broker[Broker] --> fe_broker

  %% Frontends → Gateway
  fe_issuer -->|HTTP + OIS TS SDK| api_gw
  fe_investor --> api_gw
  fe_backoffice --> api_gw
  fe_broker --> api_gw

  %% Gateway → Keycloak
  api_gw -->|OIDC / JWT validation| keycloak

  %% Gateway → Domain services REST
  api_gw --> svc_issuance
  api_gw --> svc_registry
  api_gw --> svc_settlement
  api_gw --> svc_compliance
  api_gw --> svc_identity

  %% Services → DB
  svc_issuance --> pg
  svc_registry --> pg
  svc_settlement --> pg
  svc_compliance --> pg
  svc_identity --> pg

  %% Services → Kafka outbox
  svc_issuance -->|Outbox events| kafka
  svc_registry -->|Outbox events| kafka
  svc_settlement -->|Consume/payments| kafka
  svc_compliance -->|Audit/kyc events| kafka

  %% DLT
  svc_issuance --> svc_fabric_gw
  svc_registry --> svc_fabric_gw
  svc_settlement --> svc_fabric_gw
  svc_fabric_gw --> fabric

  %% External integrations
  svc_registry --> svc_bank
  svc_compliance --> svc_esia
  svc_compliance --> svc_edo

  svc_bank --> bank_ext[[External: Bank Core]]
  svc_esia --> esia_ext[[External: ESIA]]
  svc_edo --> edo_ext[[External: EDO]]

  %% Storage & observability
  svc_compliance --> minio
  api_gw --> otel
  svc_issuance --> otel
  svc_registry --> otel
  svc_settlement --> otel
  svc_compliance --> otel
```

---

### 1.3 C3 — Components: Issuance & Registry NX‑03 / NX‑04

Эти C3-диаграммы фокусируются на доменах, важнейших для NX‑03 issuance endpoints + tests и NX‑04 registry order flow.

#### 1.3.1 C3 — Issuance Service `services/issuance`

```mermaid
graph TB
  subgraph Issuance_Service["Issuance Service services/issuance"]
    is_api[Component: HTTP API\nProgram.cs + Minimal APIs]
    is_app[Component: IssuanceApplicationService\nIssuanceService.cs]
    is_domain[Component: Domain Model\nIssuanceId / IssuanceStatus / ValueObjects\npackages/domain]
    is_dbctx[Component: IssuanceDbContext\nIssuanceDbContext.cs]
    is_outbox_svc[Component: OutboxService\nIOutboxService + impl]
    is_outbox_bg[Component: OutboxPublisher\nBackgroundService]
    is_ledger_port[Component: ILedgerIssuance port]
    is_ledger_adp[Component: LedgerIssuanceAdapter\nHTTP → Fabric-Gateway]
    is_metrics[Component: Metrics\nInfrastructure/Metrics.cs]
  end

  gw[Container: API Gateway] --> is_api

  %% Inside service
  is_api -->|Create/Get/Publish/Close issuance| is_app
  is_app --> is_domain
  is_app --> is_dbctx
  is_app --> is_outbox_svc
  is_app --> is_ledger_port

  is_ledger_port --> is_ledger_adp
  is_ledger_adp --> fabric_gw[Container: Fabric Gateway Service]

  is_dbctx --> pg_is[PostgreSQL DB\nschema: issuance]
  is_outbox_svc --> is_dbctx
  is_outbox_bg --> is_dbctx
  is_outbox_bg --> kafka_is[Kafka topics\nois.issuance.published / closed]

  %% Cross-service consumers
  kafka_is --> reg_svc[Container: Registry Service]
  kafka_is --> set_svc[Container: Settlement Service]

  %% Observability
  is_api --> is_metrics
  is_app --> is_metrics
```

Основные соответствия C3↔NX:

* NX‑03 использует `is_api`, `is_app`, `is_dbctx`, `is_outbox_*`, `is_ledger_*` + их тесты `services/issuance/issuance.Tests`.

---

#### 1.3.2 C3 — Registry Service `services/registry`

```mermaid
graph TB
  subgraph Registry_Service["Registry Service services/registry"]
    reg_api[Component: HTTP API\nProgram.cs orders, wallets, redeem]
    reg_app[Component: RegistryApplicationService\nRegistryService.cs]
    reg_domain[Component: Domain Model\nWallet/Holding/Order/Transaction entities]
    reg_dbctx[Component: RegistryDbContext]
    reg_outbox_svc[Component: OutboxService]
    reg_outbox_bg[Component: OutboxPublisher]
    reg_bank_port[Component: IBankNominalService port]
    reg_bank_client[Component: BankNominalClient\nHTTP → bank adapter]
    reg_comp_port[Component: IComplianceService port]
    reg_comp_client[Component: ComplianceClient\nHTTP → compliance]
    reg_ledger_port[Component: ILedgerRegistry port]
    reg_ledger_adp[Component: LedgerRegistryAdapter\nHTTP → Fabric-Gateway]
  end

  gw[Container: API Gateway] --> reg_api

  %% Inside service
  reg_api -->|/v1/orders, /v1/wallets, /v1/issuances/id/redeem| reg_app
  reg_app --> reg_domain
  reg_app --> reg_dbctx
  reg_app --> reg_outbox_svc
  reg_app --> reg_bank_port
  reg_app --> reg_comp_port
  reg_app --> reg_ledger_port

  reg_bank_port --> reg_bank_client
  reg_comp_port --> reg_comp_client
  reg_ledger_port --> reg_ledger_adp

  reg_bank_client --> bank_int[Container: Bank Nominal Integration]
  reg_comp_client --> comp_svc[Container: Compliance Service]
  reg_ledger_adp --> fabric_gw[Container: Fabric Gateway Service]

  reg_dbctx --> pg_reg[PostgreSQL DB\nschema: registry]
  reg_outbox_svc --> reg_dbctx
  reg_outbox_bg --> reg_dbctx
  reg_outbox_bg --> kafka_reg[Kafka topics\norders/*, registry.*]

  kafka_reg --> set_svc[Container: Settlement Service]

```

Соответствие C3↔NX:

* NX‑04 работает ровно по этому разрезу: `reg_api` маршруты /orders, /redeem, `reg_app` стейт‑машина заказа create→reserve→paid, зависимости на банк/комплаенс/Fabric и события в Kafka.

---

### 1.4 C4 — Code/Modules: `services/issuance`

Этот C4‑уровень деталирует `services/issuance` на классы/модули и показывает, как они реализуют компоненты C3. Основано на `IssuanceDbContext`, DTO‑ах, Metrics, Dockerfile и тестах.

```mermaid
graph TB
  %% Composition root
  subgraph CompositionRoot["Composition Root"]
    program["Program.cs\nDI, Minimal APIs, Auth, Metrics\nservices/issuance/Program.cs"]
  end

  %% API layer
  subgraph ApiLayer["API Layer"]
    dtoCreate["CreateIssuanceRequest\nDTO"]
    dtoResponse["IssuanceResponse\nDTO"]
    validators["FluentValidators\nCreateIssuanceRequestValidator, ... "]
  end

  %% Application layer
  subgraph AppLayer["Application Layer"]
    iIssuance["IIssuanceService"]
    issuanceSvc["IssuanceService"]
    iOutbox["IOutboxService"]
    outboxSvc["OutboxService"]
    iLedger["ILedgerIssuance"]
    ledgerAdapter["LedgerIssuanceAdapter"]
  end

  %% Domain + Persistence
  subgraph DomainAndPersistence["Domain + Persistence"]
    domainValue["OIS.Domain\nIssuanceId, IssuanceStatus,\nSecurity, value objects"]
    dbCtx["IssuanceDbContext"]
    entIssuance["IssuanceEntity\ntable: issuances"]
    entOutbox["OutboxMessage\ntable: outbox_messages"]
  end

  %% Infrastructure
  subgraph Infrastructure["Infrastructure"]
    metrics["Metrics\nrequest_duration_ms,\nrequest_errors_total"]
    outboxBg["OutboxPublisher\nBackgroundService"]
    efCore["EF Core + Npgsql provider"]
    massTransit["MassTransit Kafka rider"]
    docker["Dockerfile\nnet9.0 aspnet + sdk"]
  end

  %% External dependencies
  httpFabric["HttpClient\n→ Fabric-Gateway API"]
  postgresDb["PostgreSQL DB\nissuance schema"]
  kafka["Kafka broker\nAsyncAPI topics"]

  %% Wiring
  program --> dtoCreate
  program --> dtoResponse
  program --> validators
  program --> iIssuance

  iIssuance --> issuanceSvc
  issuanceSvc --> domainValue
  issuanceSvc --> dbCtx
  issuanceSvc --> iOutbox
  issuanceSvc --> iLedger

  iOutbox --> outboxSvc
  iLedger --> ledgerAdapter

  dbCtx --> entIssuance
  dbCtx --> entOutbox
  dbCtx --> efCore

  outboxSvc --> dbCtx
  outboxBg --> dbCtx
  outboxBg --> massTransit

  ledgerAdapter --> httpFabric

  dbCtx --> postgresDb
  massTransit --> kafka

  program --> metrics
  metrics --> outboxBg
  docker --> program
```

Связь C3→C4:

* C3‑компонент **HTTP API** = `Program.cs` + DTO/validators.
* **IssuanceApplicationService** = `IssuanceService` + интерфейс `IIssuanceService`.
* **Persistence** = `IssuanceDbContext` + `IssuanceEntity`/`OutboxMessage` + EF Core.
* **Ledger adapter** = `ILedgerIssuance` + `LedgerIssuanceAdapter` + HttpClient.
* **Outbox** = `IOutboxService` + `OutboxService` + `OutboxPublisher` + MassTransit/Kafka.
