#!/usr/bin/env bash
set -Eeuo pipefail
SRC="$(dirname "$0")/../.env.deployment.example"
DST="$(git rev-parse --show-toplevel)/.env.deployment"
[ -f "$DST" ] && { echo "Refusing to overwrite existing $DST"; exit 1; }
cp "$SRC" "$DST"
echo "Created $DST. Review and adjust." 
