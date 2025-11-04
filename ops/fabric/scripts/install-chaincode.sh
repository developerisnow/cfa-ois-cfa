#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CHAINCODE_DIR="$(cd "$SCRIPT_DIR/../../chaincode" && pwd)"

CHANNEL_NAME=cfa-main
CC_VERSION=1.0
CC_SEQUENCE=1

echo "=== Installing chaincode ==="

# Install issuance chaincode
echo "[1/4] Packaging issuance chaincode..."
cd "$CHAINCODE_DIR/issuance"
GO111MODULE=on go mod vendor
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer lifecycle chaincode package issuance.tar.gz \
    --path /opt/gopath/src/github.com/chaincode/issuance \
    --lang golang \
    --label issuance_${CC_VERSION} || {
    # Fallback: package locally and copy
    tar -czf issuance.tar.gz -C "$CHAINCODE_DIR/issuance" .
    docker cp issuance.tar.gz peer0.ois-dev.example.com:/tmp/
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

# Install registry chaincode
echo "[3/4] Packaging registry chaincode..."
cd "$CHAINCODE_DIR/registry"
GO111MODULE=on go mod vendor
tar -czf registry.tar.gz -C "$CHAINCODE_DIR/registry" .
docker cp registry.tar.gz peer0.ois-dev.example.com:/tmp/
docker cp registry.tar.gz peer1.ois-dev.example.com:/tmp/

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

