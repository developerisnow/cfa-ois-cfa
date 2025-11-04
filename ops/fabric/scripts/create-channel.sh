#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$SCRIPT_DIR"

CHANNEL_NAME=cfa-main

echo "=== Creating channel $CHANNEL_NAME ==="

# Set environment variables
export CORE_PEER_LOCALMSPID=OisDevMSP
export CORE_PEER_TLS_ENABLED=true
export CORE_PEER_TLS_ROOTCERT_FILE=$SCRIPT_DIR/crypto-config/peerOrganizations/ois-dev.example.com/peers/peer0.ois-dev.example.com/tls/ca.crt
export CORE_PEER_MSPCONFIGPATH=$SCRIPT_DIR/crypto-config/peerOrganizations/ois-dev.example.com/users/Admin@ois-dev.example.com/msp
export CORE_PEER_ADDRESS=localhost:7051
export ORDERER_CA=$SCRIPT_DIR/crypto-config/ordererOrganizations/example.com/orderers/orderer.example.com/msp/tlscacerts/tlsca.example.com-cert.pem

# Create channel
echo "Creating channel..."
docker cp $SCRIPT_DIR/channel-artifacts/${CHANNEL_NAME}.tx peer0.ois-dev.example.com:/etc/hyperledger/fabric/ || {
  echo "Error: Channel transaction file not found. Run dev-up.sh first."
  exit 1
}

docker exec -e CORE_PEER_LOCALMSPID=$CORE_PEER_LOCALMSPID \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer channel create \
    -o orderer.example.com:7050 \
    -c $CHANNEL_NAME \
    -f /etc/hyperledger/fabric/${CHANNEL_NAME}.tx \
    --tls \
    --cafile /etc/hyperledger/fabric/orderer-tlsca.crt || {
  echo "Error: Failed to create channel"
  exit 1
}

# Copy channel block to peer (if created successfully)
if [ -f "$SCRIPT_DIR/channel-artifacts/${CHANNEL_NAME}.block" ]; then
  docker cp $SCRIPT_DIR/channel-artifacts/${CHANNEL_NAME}.block peer0.ois-dev.example.com:/etc/hyperledger/fabric/
else
  # Extract block from peer container
  docker exec peer0.ois-dev.example.com cat /etc/hyperledger/fabric/${CHANNEL_NAME}.block > $SCRIPT_DIR/channel-artifacts/${CHANNEL_NAME}.block 2>/dev/null || true
fi

# Join peer0 to channel
echo "Peer0 joining channel..."
docker exec -e CORE_PEER_LOCALMSPID=$CORE_PEER_LOCALMSPID \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
    peer0.ois-dev.example.com \
    peer channel join \
    -b /etc/hyperledger/fabric/${CHANNEL_NAME}.block

# Join peer1 to channel
echo "Peer1 joining channel..."
docker cp $SCRIPT_DIR/channel-artifacts/${CHANNEL_NAME}.block peer1.ois-dev.example.com:/etc/hyperledger/fabric/
docker exec -e CORE_PEER_LOCALMSPID=$CORE_PEER_LOCALMSPID \
    -e CORE_PEER_TLS_ENABLED=true \
    -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
    -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
    -e CORE_PEER_ADDRESS=peer1.ois-dev.example.com:8051 \
    peer1.ois-dev.example.com \
    peer channel join \
    -b /etc/hyperledger/fabric/${CHANNEL_NAME}.block

echo "Channel $CHANNEL_NAME created and peers joined successfully!"

