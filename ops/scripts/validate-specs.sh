#!/usr/bin/env bash
set -euo pipefail

# Validate OpenAPI, AsyncAPI, and JSON Schemas without requiring global installs.
# It bootstraps a local Node.js (v20) toolchain in .tools if needed.

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
TOOLS_DIR="$ROOT_DIR/.tools"
NODE_VERSION="v20.17.0"
NODE_DIR="$TOOLS_DIR/node-${NODE_VERSION}-linux-x64"
NODE_BIN="$NODE_DIR/bin"

mkdir -p "$TOOLS_DIR"

need_node_install() {
  if [ -x "$NODE_BIN/node" ]; then
    # Check version >= 18
    local v
    v=$($NODE_BIN/node -v | sed 's/v//; s/\..*//')
    if [ "$v" -ge 18 ]; then
      return 1 # no install needed
    fi
  fi
  return 0
}

if need_node_install; then
  echo "Bootstrapping Node.js v20 locally under $NODE_DIR ..."
  OS="linux"
  ARCH="x64"
  URL="https://nodejs.org/dist/${NODE_VERSION}/node-${NODE_VERSION}-${OS}-${ARCH}.tar.xz"
  TMP_TAR="$TOOLS_DIR/node-${NODE_VERSION}-${OS}-${ARCH}.tar.xz"

  rm -rf "$NODE_DIR"
  curl -fsSL "$URL" -o "$TMP_TAR"
  tar -xJf "$TMP_TAR" -C "$TOOLS_DIR"
  rm -f "$TMP_TAR"
fi

export PATH="$NODE_BIN:$PATH"

echo "Using Node: $(node -v)" >&2
echo "Using npm:  $(npm -v)" >&2

cd "$ROOT_DIR"

echo "\n[1/3] Spectral: lint OpenAPI specs (custom minimal ruleset)" >&2
npx -y @stoplight/spectral-cli@6 lint -r .spectral.yaml packages/contracts/openapi-*.yaml

echo "\n[2/3] AsyncAPI: validate asyncapi.yaml" >&2
npx -y @asyncapi/cli@2 validate packages/contracts/asyncapi.yaml

echo "\n[3/3] AJV: compile JSON Schemas" >&2
AJV_OK=true
for schema in packages/contracts/schemas/*.json; do
  echo "Compiling $schema" >&2
  if ! npx -y ajv-cli@5 compile --validate-formats=false --strict=false -s "$schema"; then
    AJV_OK=false
    break
  fi
done

if [ "$AJV_OK" != true ]; then
  echo "AJV schema compilation failed" >&2
  exit 1
fi

echo "\nAll spec validations passed." >&2
