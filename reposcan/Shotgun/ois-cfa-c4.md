# OIS-CFA · C4 Views (Mermaid)

This file provides context and container-level C4-style views using Mermaid. Diagrams reflect the `agents` branch state and reference core services and externals.

## Context Diagram

```mermaid
flowchart LR
  actorInvestor[Investor]
  actorIssuer[Issuer]
  actorBroker[Broker]

  subgraph OIS[OIS‑CFA Platform]
    API[API Gateway]
    REG[Registry Service]
    ISS[Issuance Service]
    SETT[Settlement Service]
    COMP[Compliance Service]
    IDP[Identity Service]
    BP[Broker Portal]
    IP[Issuer Portal]
    INV[Investor Portal]
    BO[Backoffice]
  end

  ESIA[ESIA (OIDC)]
  EDO[EDO (Diadoc/SBIS/1C)]
  BANK[Bank Nominal API]
  subgraph DLT[DLT Network (Hyperledger Fabric)]
    ORDERER[Orderer]
    PEER[Peer(s) + CouchDB]
    CA[Fabric CA]
  end

  actorInvestor --> INV --> API
  actorIssuer   --> IP  --> API
  actorBroker   --> BP  --> API
  BO --> API

  API --> REG
  API --> ISS
  API --> SETT
  API --> COMP
  API --> IDP

  IDP --- ESIA
  COMP --- EDO
  SETT --- BANK

  REG --> PEER
  ISS --> PEER
  PEER --> ORDERER
  CA --> PEER
```

## Container Diagram

```mermaid
flowchart TB
  subgraph Frontends
    INV[portal‑investor (Next.js)]
    IP[portal‑issuer (Next.js)]
    BP[broker‑portal (Next.js)]
    BO[backoffice (Next.js)]
  end

  API[api‑gateway (.NET + YARP)]

  subgraph Services
    IDP[identity (.NET)]
    REG[registry (.NET + EF Core)]
    ISS[issuance (.NET + Kafka)]
    SETT[settlement (.NET)]
    COMP[compliance (.NET)]
    FABGW[fabric‑gateway adapter]
  end

  subgraph Data & Messaging
    PG[(PostgreSQL)]
    KAFKA[(Kafka)]
    ZK[(Zookeeper)]
    OTEL[OpenTelemetry]
    PROM[Prometheus]
    GRAF[Grafana]
    MINIO[(MinIO)]
  end

  subgraph Fabric
    ORDERER[Orderer]
    PEER[Peer(s) + CouchDB]
    CA[Fabric CA]
  end

  INV --> API
  IP  --> API
  BP  --> API
  BO  --> API

  API --> IDP & REG & ISS & SETT & COMP
  REG --> PG
  COMP --> PG
  ISS --> PG
  SETT --> PG

  ISS --> KAFKA
  REG --> FABGW --> PEER --> ORDERER
  CA --> PEER

  API -.metrics.-> PROM
  Services -.traces.-> OTEL
  PROM -.dashboards.-> GRAF
  MINIO -.artifacts/docs.-> Services
```
