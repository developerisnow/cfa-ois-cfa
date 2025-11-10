#!/usr/bin/env bash
set -euo pipefail

TARGET_DIR=${1:-/home/user/__Repositories/yury-customer/deploy/ois-cfa-deployed}
REPO_DIR=${2:-/home/user/__Repositories/yury-customer/prj_Cifra-rwa-exachange-assets/repositories/customer-gitlab/ois-cfa}

mkdir -p "$TARGET_DIR"/logs
cd "$TARGET_DIR"

# Copy unified compose template
cp -f "$REPO_DIR/.docs/deploy/templates/docker-compose.unified.template.yml" ./docker-compose.yml

# Create .env with ports and secrets if absent
if [ ! -f .env ]; then
  echo "OIS_CFA_REPO_DIR=$REPO_DIR" > .env
  cat "$REPO_DIR/.docs/deploy/22-env.ports.example" >> .env
  echo "" >> .env
  if [ -f "$REPO_DIR/.docs/deploy/23-env.secrets.example" ]; then
    # Evaluate openssl substitutions only if openssl available
    if command -v openssl >/dev/null 2>&1; then
      eval "$(sed -n '1,999p' "$REPO_DIR/.docs/deploy/23-env.secrets.example" | sed 's/^/export /')" >/dev/null 2>&1 || true
      sed -e "s#\$(openssl rand -hex 16 2>/dev/null || echo ois_dev_password)#$(openssl rand -hex 16 2>/dev/null || echo ois_dev_password)#" \
          -e "s#\$(openssl rand -hex 12 2>/dev/null || echo admin)#$(openssl rand -hex 12 2>/dev/null || echo admin)#" \
          "$REPO_DIR/.docs/deploy/23-env.secrets.example" >> .env
    else
      cat "$REPO_DIR/.docs/deploy/23-env.secrets.example" >> .env
    fi
  fi
fi

cat > README.md <<'MD'
# ois-cfa-deployed

- Unified Docker Compose stack for infra + services
- Place repo dir in .env: `OIS_CFA_REPO_DIR=/path/to/ois-cfa`
- Non-default ports configured via .env

Usage
```bash
# Preflight (ports + docker availability)
ss -ltn | awk '{print $4}' | grep -E ':(55432|52181|59092|58080|59000|59001|6504|6505|6506|6507|6508|6580)$' || true

docker compose --env-file .env config >/dev/null

# Bring up infra, then services
# docker compose --env-file .env up -d postgres zookeeper kafka keycloak minio
# docker compose --env-file .env up -d identity registry issuance settlement compliance api-gateway
```
MD

printf "Bootstrapped deploy project at %s\n" "$TARGET_DIR"
