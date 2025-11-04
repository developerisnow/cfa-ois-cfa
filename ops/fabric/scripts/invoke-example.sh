#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

CHANNEL_NAME=cfa-main

echo "=== Example chaincode invocations ==="

# Example: Issue an issuance
echo "[1/3] Invoking issuance chaincode - Issue..."
ISSUANCE_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
ASSET_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')
ISSUER_ID=$(uuidgen | tr '[:upper:]' '[:lower:]')

docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer chaincode invoke \
    -o orderer.example.com:7050 \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
    -C $CHANNEL_NAME \
    -n issuance \
    --peerAddresses peer0.ois-dev.example.com:7051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    --peerAddresses peer1.ois-dev.example.com:8051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    -c "{\"function\":\"Issue\",\"Args\":[\"$ISSUANCE_ID\",\"$ASSET_ID\",\"$ISSUER_ID\",\"1000000\",\"1000\",\"2024-01-01\",\"2025-01-01\",\"{}\"]}"

echo "Issuance created: $ISSUANCE_ID"

# Example: Query issuance
echo "[2/3] Querying issuance chaincode - Get..."
docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer chaincode query \
    -C $CHANNEL_NAME \
    -n issuance \
    -c "{\"function\":\"Get\",\"Args\":[\"$ISSUANCE_ID\"]}"

# Example: Transfer
echo "[3/3] Invoking registry chaincode - Transfer..."
FROM_HOLDER=""
TO_HOLDER=$(uuidgen | tr '[:upper:]' '[:lower:]')
AMOUNT=10000

docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer chaincode invoke \
    -o orderer.example.com:7050 \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
    -C $CHANNEL_NAME \
    -n registry \
    --peerAddresses peer0.ois-dev.example.com:7051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    --peerAddresses peer1.ois-dev.example.com:8051 \
    --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
    -c "{\"function\":\"Transfer\",\"Args\":[\"\",\"$TO_HOLDER\",\"$ISSUANCE_ID\",\"$AMOUNT\"]}"

echo "Transfer completed!"

