#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CHAINCODE_DIR="$(cd "$SCRIPT_DIR/../../chaincode" && pwd)"

CHANNEL_NAME=cfa-main
CC_VERSION=1.0
CC_SEQUENCE=1

echo "=== Installing chaincode ==="

# Package issuance chaincode locally
echo "[1/4] Packaging issuance chaincode..."
cd "$CHAINCODE_DIR/issuance"

# Ensure Go modules are ready
if [ -f "go.mod" ]; then
    GO111MODULE=on go mod download || true
    GO111MODULE=on go mod vendor || true
fi

# Package chaincode
tar -czf issuance.tar.gz -C "$CHAINCODE_DIR/issuance" . || {
    echo "Error: Failed to package issuance chaincode"
    exit 1
}

# Copy to peers
docker cp issuance.tar.gz peer0.ois-dev.example.com:/tmp/ || {
    echo "Error: Failed to copy issuance package to peer0"
    exit 1
}
docker cp issuance.tar.gz peer1.ois-dev.example.com:/tmp/ || {
    echo "Error: Failed to copy issuance package to peer1"
    exit 1
}

echo "[2/4] Installing issuance chaincode on peers..."
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    port=$(if [ "$peer" = "peer0.ois-dev.example.com" ]; then echo "7051"; else echo "8051"; fi)
    docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=${peer}:${port} \
        $peer \
        peer lifecycle chaincode install /tmp/issuance.tar.gz || {
        # If file doesn't exist in container, copy it
        docker cp "$CHAINCODE_DIR/issuance/issuance.tar.gz" $peer:/tmp/ 2>/dev/null || true
        docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
            -e CORE_PEER_TLS_ENABLED=true \
            -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
            -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
            -e CORE_PEER_ADDRESS=${peer}:${port} \
            $peer \
            peer lifecycle chaincode install /tmp/issuance.tar.gz
    }
done

# Package registry chaincode locally
echo "[3/4] Packaging registry chaincode..."
cd "$CHAINCODE_DIR/registry"

# Ensure Go modules are ready
if [ -f "go.mod" ]; then
    GO111MODULE=on go mod download || true
    GO111MODULE=on go mod vendor || true
fi

# Package chaincode
tar -czf registry.tar.gz -C "$CHAINCODE_DIR/registry" . || {
    echo "Error: Failed to package registry chaincode"
    exit 1
}

# Copy to peers
docker cp registry.tar.gz peer0.ois-dev.example.com:/tmp/ || {
    echo "Error: Failed to copy registry package to peer0"
    exit 1
}
docker cp registry.tar.gz peer1.ois-dev.example.com:/tmp/ || {
    echo "Error: Failed to copy registry package to peer1"
    exit 1
}

echo "[4/4] Installing registry chaincode on peers..."
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    port=$(if [ "$peer" = "peer0.ois-dev.example.com" ]; then echo "7051"; else echo "8051"; fi)
    docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=${peer}:${port} \
        $peer \
        peer lifecycle chaincode install /tmp/registry.tar.gz
done

echo "Chaincode installed successfully!"
echo ""
echo "Next: Approve and commit chaincode definitions"
echo "  ./scripts/approve-chaincode.sh"

