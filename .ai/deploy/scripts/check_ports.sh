#!/usr/bin/env bash
set -Eeuo pipefail
OUT_DIR="$(dirname "$0")/../_out"; mkdir -p "$OUT_DIR"
PORTS=(58080 58090 56100 56110 56120 56130 56140 56150 55432 59092 58181 59000 59001)
CMD="ss -tulpen || netstat -tulpen"
{
  echo "# Ports Snapshot ($(date -Is))";
  eval $CMD 2>/dev/null || true
  for p in "${PORTS[@]}"; do
    if ss -tulpen 2>/dev/null | grep -q ":$p\b"; then echo "CONFLICT $p"; else echo "OK $p"; fi
  done
} | tee "$OUT_DIR/ports.md"
