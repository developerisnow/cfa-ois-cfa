#!/usr/bin/env bash
set -Eeuo pipefail
OUT_DIR="$(dirname "$0")/../_out"; mkdir -p "$OUT_DIR"
{
  echo "# Resources Snapshot ($(date -Is))";
  echo "## System"; uname -a; lsb_release -a 2>/dev/null || true
  echo "## CPU"; lscpu || true
  echo "## Memory"; free -h || true
  echo "## Disk"; df -hT || true
  echo "## Docker"; docker --version 2>&1 || true; docker compose version 2>&1 || true
} | tee "$OUT_DIR/resources.md"
