#!/usr/bin/env bash
set -euo pipefail

# Simple EF Core migrations runner for all services
# Usage:
#   ./tools/migrate_all.sh             # run migrations
#   TESTCONTAINERS=1 ./tools/migrate_all.sh  # skip with a note (for CI/Testcontainers mode)
# Optional env:
#   DOTNET_ROOT (defaults to .tools/dotnet9)
#   CONNECTION_STRING (overrides ConnectionStrings:DefaultConnection)

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT_DIR"

export DOTNET_ROOT="${DOTNET_ROOT:-$ROOT_DIR/.tools/dotnet9}"
export PATH="$DOTNET_ROOT:$PATH"

echo "[migrate_all] Using DOTNET_ROOT=$DOTNET_ROOT"

if [[ "${TESTCONTAINERS:-}" =~ ^(1|true|TRUE|yes|YES)$ ]]; then
  echo "[migrate_all] skipped by flag (TESTCONTAINERS=$TESTCONTAINERS)"
  exit 0
fi

# Ensure local dotnet-ef tool
if [[ ! -f .config/dotnet-tools.json ]] || ! rg -q '"dotnet-ef"' .config/dotnet-tools.json 2>/dev/null; then
  echo "[migrate_all] Initializing local dotnet tool manifest and installing dotnet-ef"
  dotnet new tool-manifest >/dev/null
  dotnet tool install dotnet-ef --version 9.* >/dev/null
fi
dotnet tool restore >/dev/null

services=(
  services/issuance/issuance.csproj
  services/registry/registry.csproj
  services/settlement/settlement.csproj
  services/compliance/compliance.csproj
)

for proj in "${services[@]}"; do
  if [[ ! -f "$proj" ]]; then
    echo "[migrate_all] WARN: project not found: $proj"
    continue
  fi

  svc_dir="$(dirname "$proj")"
  svc_name="$(basename "$svc_dir")"
  echo "[migrate_all] Updating database for $svc_name ($proj)"

  # Allow overriding connection string via env
  if [[ -n "${CONNECTION_STRING:-}" ]]; then
    echo "[migrate_all] Using overridden connection string for $svc_name"
    ConnectionStrings__DefaultConnection="$CONNECTION_STRING" dotnet ef database update \
      --project "$proj" --startup-project "$proj" --no-build
  else
    dotnet ef database update --project "$proj" --startup-project "$proj" --no-build
  fi
done

echo "[migrate_all] Done."

