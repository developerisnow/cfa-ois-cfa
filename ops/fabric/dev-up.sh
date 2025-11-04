#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo -e "${GREEN}=== Hyperledger Fabric Dev Network Setup ===${NC}"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Error: Docker is not running${NC}"
    exit 1
fi

# Check if cryptogen and configtxgen are available
if ! command -v cryptogen &> /dev/null; then
    echo -e "${YELLOW}Warning: cryptogen not found. Using fabric-tools container.${NC}"
    USE_CONTAINER=true
else
    USE_CONTAINER=false
fi

# Generate crypto material
echo -e "${GREEN}[1/5] Generating crypto material...${NC}"
if [ "$USE_CONTAINER" = true ]; then
    docker run --rm -v "$SCRIPT_DIR:/workspace" -w /workspace \
        hyperledger/fabric-tools:2.5 \
        cryptogen generate --config=./crypto-config.yaml --output="crypto-config"
else
    cryptogen generate --config=./crypto-config.yaml --output="crypto-config"
fi

# Generate genesis block
echo -e "${GREEN}[2/5] Generating genesis block...${NC}"
mkdir -p channel-artifacts
if [ "$USE_CONTAINER" = true ]; then
    docker run --rm -v "$SCRIPT_DIR:/workspace" -w /workspace \
        -e FABRIC_CFG_PATH=/workspace \
        hyperledger/fabric-tools:2.5 \
        configtxgen -profile Genesis -channelID system-channel -outputBlock ./channel-artifacts/genesis.block
else
    export FABRIC_CFG_PATH="$SCRIPT_DIR"
    configtxgen -profile Genesis -channelID system-channel -outputBlock ./channel-artifacts/genesis.block
fi

# Create channel
echo -e "${GREEN}[3/5] Creating channel cfa-main...${NC}"
if [ "$USE_CONTAINER" = true ]; then
    docker run --rm -v "$SCRIPT_DIR:/workspace" -w /workspace \
        -e FABRIC_CFG_PATH=/workspace \
        hyperledger/fabric-tools:2.5 \
        configtxgen -profile CfaMainChannel -channelID cfa-main -outputCreateChannelTx ./channel-artifacts/cfa-main.tx || {
        echo -e "${RED}Error: Failed to create channel transaction${NC}"
        exit 1
    }
else
    export FABRIC_CFG_PATH="$SCRIPT_DIR"
    configtxgen -profile CfaMainChannel -channelID cfa-main -outputCreateChannelTx ./channel-artifacts/cfa-main.tx || {
        echo -e "${RED}Error: Failed to create channel transaction${NC}"
        exit 1
    }
fi

# Start network
echo -e "${GREEN}[4/5] Starting Fabric network...${NC}"
docker-compose up -d

# Wait for network to be ready
echo -e "${GREEN}[5/5] Waiting for network to be ready...${NC}"
sleep 10

# Check if orderer is ready
for i in {1..30}; do
    if docker exec orderer.example.com ls /var/hyperledger/orderer/orderer.genesis.block > /dev/null 2>&1; then
        echo -e "${GREEN}Orderer is ready${NC}"
        break
    fi
    if [ $i -eq 30 ]; then
        echo -e "${RED}Orderer failed to start${NC}"
        exit 1
    fi
    sleep 2
done

# Check if peers are ready
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    for i in {1..30}; do
        if docker exec "$peer" peer channel list > /dev/null 2>&1; then
            echo -e "${GREEN}$peer is ready${NC}"
            break
        fi
        if [ $i -eq 30 ]; then
            echo -e "${RED}$peer failed to start${NC}"
            exit 1
        fi
        sleep 2
    done
done

echo -e "${GREEN}=== Network is ready! ===${NC}"
echo ""
echo "Next steps:"
echo "1. Create channel: ./scripts/create-channel.sh"
echo "2. Install chaincode: ./scripts/install-chaincode.sh"
echo ""
echo "Network endpoints:"
echo "  Orderer:  localhost:7050"
echo "  Peer0:    localhost:7051"
echo "  Peer1:    localhost:8051"
echo "  CA:       localhost:7054"
echo "  CouchDB0: localhost:5984"
echo "  CouchDB1: localhost:5985"

