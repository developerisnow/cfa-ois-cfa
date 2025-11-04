#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CHANNEL_NAME=cfa-main
CC_VERSION=1.0
CC_SEQUENCE=1

echo "=== Approving and committing chaincode ==="

# Get package IDs
ISSUANCE_PACKAGE_ID=$(docker exec peer0.ois-dev.example.com peer lifecycle chaincode queryinstalled | grep -oP 'issuance_\K[^ ]+' | head -1)
REGISTRY_PACKAGE_ID=$(docker exec peer0.ois-dev.example.com peer lifecycle chaincode queryinstalled | grep -oP 'registry_\K[^ ]+' | head -1)

if [ -z "$ISSUANCE_PACKAGE_ID" ]; then
    echo "Error: Could not find issuance package ID"
    exit 1
fi

if [ -z "$REGISTRY_PACKAGE_ID" ]; then
    echo "Error: Could not find registry package ID"
    exit 1
fi

echo "Issuance Package ID: $ISSUANCE_PACKAGE_ID"
echo "Registry Package ID: $REGISTRY_PACKAGE_ID"

# Approve issuance chaincode
echo "[1/4] Approving issuance chaincode..."
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    port=$(if [ "$peer" = "peer0.ois-dev.example.com" ]; then echo "7051"; else echo "8051"; fi)
    docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=${peer}:${port} \
        $peer \
        peer lifecycle chaincode approveformyorg \
        -o orderer.example.com:7050 \
        --channelID $CHANNEL_NAME \
        --name issuance \
        --version $CC_VERSION \
        --package-id $ISSUANCE_PACKAGE_ID \
        --sequence $CC_SEQUENCE \
        --tls \
        --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
done

# Approve registry chaincode
echo "[2/4] Approving registry chaincode..."
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    port=$(if [ "$peer" = "peer0.ois-dev.example.com" ]; then echo "7051"; else echo "8051"; fi)
    docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=${peer}:${port} \
        $peer \
        peer lifecycle chaincode approveformyorg \
        -o orderer.example.com:7050 \
        --channelID $CHANNEL_NAME \
        --name registry \
        --version $CC_VERSION \
        --package-id $REGISTRY_PACKAGE_ID \
        --sequence $CC_SEQUENCE \
        --tls \
        --cafile /etc/hyperledger/fabric/orderer-tlsca.crt
done

# Commit issuance chaincode
echo "[3/4] Committing issuance chaincode..."
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer lifecycle chaincode commit \
    -o orderer.example.com:7050 \
    --channelID $CHANNEL_NAME \
    --name issuance \
    --version $CC_VERSION \
    --sequence $CC_SEQUENCE \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
    --peerAddresses peer0.ois-dev.example.com:7051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    --peerAddresses peer1.ois-dev.example.com:8051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt

# Commit registry chaincode
echo "[4/4] Committing registry chaincode..."
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer lifecycle chaincode commit \
    -o orderer.example.com:7050 \
    --channelID $CHANNEL_NAME \
    --name registry \
    --version $CC_VERSION \
    --sequence $CC_SEQUENCE \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
    --peerAddresses peer0.ois-dev.example.com:7051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    --peerAddresses peer1.ois-dev.example.com:8051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt

echo "Chaincode approved and committed successfully!"

