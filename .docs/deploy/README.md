# Deployment Docs · OIS‑CFA (agents)

This directory contains the end‑to‑end plan and runbooks to deploy OIS‑CFA on WDS using Docker Compose with non‑default ports. All changes live on `agents` and push only to `alex`.

Sections:
- 01‑Kickoff: goals, scope, roles, prerequisites
- 02‑Plan: phased rollout and checkpoints
- 03‑DoD: definition of done per phase
- 04‑Deliverables: artifacts to produce
- 10‑Environment: WDS inventory, capacity, constraints
- 11‑Ports: non‑default port map + selection
- 12‑Preflight: checks to validate readiness (+ script)
- 20‑Runbook‑WDS: step‑by‑step deployment
- 21‑Compose‑Override: example override with env‑based ports
- 30‑Troubleshooting
- 31‑Rollback
- 40‑Operations: backups, monitoring, health, SLOs

Cross‑references:
- Reposcan index: reposcan/README.md:1
- APIs: reposcan/Shotgun/ois-cfa-apis.md:1
- C4: reposcan/Shotgun/ois-cfa-c4.md:1
- ER: reposcan/Shotgun/ois-cfa-er.md:1
- Dependencies: reposcan/Shotgun/dependencies.md:1

Iteration: every meaningful change is committed separately (see CHANGELOG).
