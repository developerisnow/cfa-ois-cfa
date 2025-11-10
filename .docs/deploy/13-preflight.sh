#!/usr/bin/env bash
set -euo pipefail

RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; NC='\033[0m'

info() { echo -e "${YELLOW}==>${NC} $*"; }
ok()   { echo -e "${GREEN}[OK]${NC} $*"; }
fail() { echo -e "${RED}[FAIL]${NC} $*"; }

ENV_FILE=".env.ports"
if [[ -f "$ENV_FILE" ]]; then
  set -a; source "$ENV_FILE"; set +a
else
  info "No $ENV_FILE found; using defaults"
fi

HOST_POSTGRES_PORT=${HOST_POSTGRES_PORT:-55432}
HOST_ZOOKEEPER_PORT=${HOST_ZOOKEEPER_PORT:-52181}
HOST_KAFKA_PORT=${HOST_KAFKA_PORT:-59092}
HOST_KEYCLOAK_PORT=${HOST_KEYCLOAK_PORT:-58080}
HOST_MINIO_API_PORT=${HOST_MINIO_API_PORT:-59000}
HOST_MINIO_CONSOLE_PORT=${HOST_MINIO_CONSOLE_PORT:-59001}
SERVICE_IDENTITY_PORT=${SERVICE_IDENTITY_PORT:-6504}
SERVICE_ISSUANCE_PORT=${SERVICE_ISSUANCE_PORT:-6505}
SERVICE_REGISTRY_PORT=${SERVICE_REGISTRY_PORT:-6506}
SERVICE_SETTLEMENT_PORT=${SERVICE_SETTLEMENT_PORT:-6507}
SERVICE_COMPLIANCE_PORT=${SERVICE_COMPLIANCE_PORT:-6508}
SERVICE_APIGW_PORT=${SERVICE_APIGW_PORT:-6580}

HEADER() {
  echo; echo "# $1"; echo
}

HEADER "Host Summary"
uname -a || true
printf "CPU: %s cores\n" "$(nproc || echo '?')"
free -h || true
df -h / || true

HEADER "Docker/Compose"
if command -v docker >/dev/null 2>&1; then ok "Docker installed: $(docker --version)"; else fail "Docker missing"; exit 1; fi
if docker compose version >/dev/null 2>&1; then ok "Compose plugin: $(docker compose version)"; else fail "Compose plugin missing"; exit 1; fi

HEADER "Port Availability"
check_port() {
  local p=$1
  if ss -ltn 2>/dev/null | awk '{print $4}' | grep -q ":$p$"; then
    fail "Port $p is in use"
    return 1
  else
    ok "Port $p is free"
    return 0
  fi
}
FAIL=0
for p in \
  $HOST_POSTGRES_PORT $HOST_ZOOKEEPER_PORT $HOST_KAFKA_PORT \
  $HOST_KEYCLOAK_PORT $HOST_MINIO_API_PORT $HOST_MINIO_CONSOLE_PORT \
  $SERVICE_IDENTITY_PORT $SERVICE_ISSUANCE_PORT $SERVICE_REGISTRY_PORT \
  $SERVICE_SETTLEMENT_PORT $SERVICE_COMPLIANCE_PORT $SERVICE_APIGW_PORT; do
  check_port "$p" || FAIL=1
done

HEADER "Network"
for host in download.docker.com quay.io docker.io ghcr.io; do
  if ping -c1 -W1 "$host" >/dev/null 2>&1; then ok "Ping $host"; else info "Cannot ping $host (may be blocked)"; fi
done

if [[ $FAIL -eq 1 ]]; then
  echo; fail "Preflight FAILED due to port conflicts."; exit 2
fi

echo; ok "Preflight PASSED"
