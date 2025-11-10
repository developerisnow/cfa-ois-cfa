# Reposcan Artifacts · OIS‑CFA

This folder aggregates repository snapshots and system blueprint outputs for agents and reviewers. All changes live on branch `agents` and are pushed only to remote `alex`.

## Contents

- RepoMix
  - `reposcan/RepoMix/repomix.md` — full Markdown snapshot for LLMs
  - `reposcan/RepoMix/repomix.json` — JSON variant
- CodeToPrompt
  - `reposcan/CodeToPrompt/code2prompt.md` — alternative snapshot with full tree
- EG (Yek)
  - `reposcan/EG/yek.txt` — ultra‑fast plain snapshot with tree header
- Shotgun (SDD)
  - JSON: `reposcan/Shotgun/ois-cfa.shtgn.reposcan.json`
  - C4 view: `reposcan/Shotgun/ois-cfa.c4.md` (Mermaid)
  - ERD: `reposcan/Shotgun/ois-cfa.er.md` (Mermaid)
- Versions
  - `reposcan/versions.txt` — toolchain versions

## How To Re‑generate

- RepoMix
  - `npx repomix@latest --style markdown -o reposcan/RepoMix/repomix.md`
  - `npx repomix@latest --style json -o reposcan/RepoMix/repomix.json`
- CodeToPrompt
  - `code2prompt . -O reposcan/CodeToPrompt/code2prompt.md -F markdown --full-directory-tree -q`
- Yek
  - `yek --output-dir reposcan/EG --output-name yek.txt -t .`
- Shotgun JSON
  - Use template and mapping rules in `repositories/ai/SDD-shotgun-pro` and enrich `reposcan/Shotgun/ois-cfa.shtgn.reposcan.json`

## Ubuntu · Docker Compose (Dev)

Prereqs
- Docker Engine 24+, Compose plugin
- Ports free: 5432, 9092, 2181, 8080, 9000, 9001

Steps
- Clone repo and checkout `agents` branch: `git checkout agents`
- Bring up infra:
  - `docker compose up -d postgres zookeeper kafka keycloak minio`
- Configure env (examples in `docker-compose.yml` and service `appsettings.json`)
- Run services locally (auto‑migrate on startup):
  - Registry: `dotnet run --project services/registry`
  - Compliance: `dotnet run --project services/compliance`
  - Issuance: `dotnet run --project services/issuance`
  - Settlement: `dotnet run --project services/settlement`
  - Identity (stub): `dotnet run --project services/identity`
  - API Gateway: `dotnet run --project apps/api-gateway`
- Health checks
  - Gateway: `GET http://localhost:5xxx/health` (port per launchSettings)
  - Services: each exposes `/health`; Issuance also `/metrics` (Prometheus)

Notes
- Services apply EF migrations at startup.
- Kafka and Keycloak are optional for basic flows; enable when testing events/oidc.
- For Fabric dev, see `ops/fabric` and Helm charts in `ops/infra/helm`.

## Cross‑References

- Shotgun JSON keys: contexts, containers, components, domain_glossary, deployment_topology, data_schema, api_endpoints, external_services, sources.
- Contracts: see `packages/contracts/*` for OpenAPI/AsyncAPI and JSON schemas.
- Helm/devops: `ops/infra/helm/*`, `ops/fabric`, `docker-compose.yml`.

