#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "=== Resetting Fabric network ==="

# Stop network
echo "[1/3] Stopping network..."
docker-compose down

# Remove volumes
echo "[2/3] Removing volumes..."
docker volume rm fabric-network_orderer.example.com 2>/dev/null || true
docker volume rm fabric-network_peer0.ois-dev.example.com 2>/dev/null || true
docker volume rm fabric-network_peer1.ois-dev.example.com 2>/dev/null || true
docker volume rm fabric-network_couchdb0 2>/dev/null || true
docker volume rm fabric-network_couchdb1 2>/dev/null || true
docker volume rm fabric-network_ca.ois-dev.example.com 2>/dev/null || true

# Remove crypto material and artifacts
echo "[3/3] Removing crypto material and artifacts..."
rm -rf crypto-config
rm -rf channel-artifacts

echo "Network reset complete."

