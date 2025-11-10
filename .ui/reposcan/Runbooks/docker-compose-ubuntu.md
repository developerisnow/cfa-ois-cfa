# Runbook · Ubuntu Server Deployment (Docker Compose)

This runbook covers spinning up core infrastructure and services of OIS‑CFA on a vanilla Ubuntu host using Docker Compose. It targets the `agents` branch artifacts and avoids changes to `main`.

## 1) Prerequisites
- Ubuntu 22.04+
- User in `sudo` group
- Network egress to pull images from Docker registries

Install Docker and Compose plugin:
```bash
sudo apt-get update -y
sudo apt-get install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo $VERSION_CODENAME) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
sudo apt-get update -y
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker $USER
newgrp docker
docker version && docker compose version
```

## 2) Clone and checkout
```bash
git clone git@git.telex.global:npk/ois-cfa.git
cd ois-cfa
git remote add alex git@github.com:developerisnow/cfa-ois-cfa.git || true
git fetch alex
git checkout agents
```

## 3) Bring up infrastructure
The root `docker-compose.yml` includes Postgres, Kafka+ZK, Keycloak, and MinIO.
```bash
docker network create ois-network || truedocker compose up -d
docker compose ps
```

Health checks:
- Postgres: container `ois-postgres` healthy
- Keycloak: http://<host>:8080 (admin/admin by default; change in production)
- MinIO: http://<host>:9001 console (minioadmin/minioadmin)

## 4) Run services (containers)
An example compose for services is provided at `reposcan/Runbooks/docker-compose.services.example.yml`.

```bash
docker compose -f reposcan/Runbooks/docker-compose.services.example.yml up -d --build
```

Ports (default):
- api-gateway: 5080
- identity: 5004
- issuance: 5005
- registry: 5006
- settlement: 5007
- compliance: 5008

## 5) Configure identity (optional)
If using Keycloak directly, import realm/clients as needed. If using the stub `identity` service, endpoints are exposed at port 5004.

## 6) Verify APIs
Use health endpoints or the OpenAPI specs in `packages/contracts`.
```bash
curl http://localhost:5080/health
curl http://localhost:5006/v1/orders/<id>
```

## 7) Observability (optional)
- Integrate OpenTelemetry, Prometheus, Grafana according to ops manifests under `ops/`.

## 8) Data & backups
- Postgres data volume: `postgres_data` (see docker-compose.yml)
- Automated backup container provided (`postgres-backup`) writing to `./backups/`

## 9) Production considerations
- Replace default passwords and rotate secrets
- TLS termination and proper hostnames
- Persist volumes on separate disks
- CPU/memory sizing and autoscaling policies
- Restrict Kafka access and place behind network policies
