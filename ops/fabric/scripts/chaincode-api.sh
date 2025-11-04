#!/bin/bash
# Simple HTTP API server for chaincode invocations via peer CLI
# This is a development-only solution

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CHANNEL_NAME=cfa-main
PEER_CONTAINER=peer0.ois-dev.example.com

# Simple HTTP server using netcat or Python
# For production, use proper HTTP server

handle_request() {
    local method=$1
    local path=$2
    local body=$3

    case "$path" in
        /chaincode/invoke)
            if [ "$method" = "POST" ]; then
                handle_invoke "$body"
            else
                echo "HTTP/1.1 405 Method Not Allowed"
                echo "Content-Type: application/json"
                echo ""
                echo '{"error": "Method not allowed"}'
            fi
            ;;
        /chaincode/query)
            if [ "$method" = "POST" ]; then
                handle_query "$body"
            else
                echo "HTTP/1.1 405 Method Not Allowed"
                echo "Content-Type: application/json"
                echo ""
                echo '{"error": "Method not allowed"}'
            fi
            ;;
        /health)
            echo "HTTP/1.1 200 OK"
            echo "Content-Type: application/json"
            echo ""
            echo '{"status": "ok"}'
            ;;
        *)
            echo "HTTP/1.1 404 Not Found"
            echo "Content-Type: application/json"
            echo ""
            echo '{"error": "Not found"}'
            ;;
    esac
}

handle_invoke() {
    local body=$1
    local chaincode=$(echo "$body" | jq -r '.chaincode')
    local function=$(echo "$body" | jq -r '.function')
    local args=$(echo "$body" | jq -r '.args[]?')

    # Build peer command
    local peer_args=""
    for arg in $args; do
        peer_args="$peer_args \"$arg\""
    done

    # Execute via docker exec
    local result=$(docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
        $PEER_CONTAINER \
        peer chaincode invoke \
        -o orderer.example.com:7050 \
        --tls \
        --cafile /etc/hyperledger/fabric/orderer-tlsca.crt \
        -C $CHANNEL_NAME \
        -n $chaincode \
        --peerAddresses peer0.ois-dev.example.com:7051 \
        --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
        --peerAddresses peer1.ois-dev.example.com:8051 \
        --tlsRootCertFiles /etc/hyperledger/fabric/tls/ca.crt \
        -c "{\"function\":\"$function\",\"Args\":[$peer_args]}" 2>&1)

    local tx_hash=$(echo "$result" | grep -oP 'txid: \K[^ ]+' | head -1)

    if [ -n "$tx_hash" ]; then
        echo "HTTP/1.1 200 OK"
        echo "Content-Type: application/json"
        echo ""
        echo "{\"transactionHash\": \"$tx_hash\", \"success\": true}"
    else
        echo "HTTP/1.1 500 Internal Server Error"
        echo "Content-Type: application/json"
        echo ""
        echo "{\"error\": \"$result\", \"success\": false}"
    fi
}

handle_query() {
    local body=$1
    local chaincode=$(echo "$body" | jq -r '.chaincode')
    local function=$(echo "$body" | jq -r '.function')
    local args=$(echo "$body" | jq -r '.args[]?')

    local peer_args=""
    for arg in $args; do
        peer_args="$peer_args \"$arg\""
    done

    local result=$(docker exec -e CORE_PEER_LOCALMSPID=OisDevMSP \
        -e CORE_PEER_TLS_ENABLED=true \
        -e CORE_PEER_TLS_ROOTCERT_FILE=/etc/hyperledger/fabric/tls/ca.crt \
        -e CORE_PEER_MSPCONFIGPATH=/etc/hyperledger/fabric/msp \
        -e CORE_PEER_ADDRESS=peer0.ois-dev.example.com:7051 \
        $PEER_CONTAINER \
        peer chaincode query \
        -C $CHANNEL_NAME \
        -n $chaincode \
        -c "{\"function\":\"$function\",\"Args\":[$peer_args]}" 2>&1)

    if [ $? -eq 0 ]; then
        echo "HTTP/1.1 200 OK"
        echo "Content-Type: application/json"
        echo ""
        echo "$result"
    else
        echo "HTTP/1.1 404 Not Found"
        echo "Content-Type: application/json"
        echo ""
        echo "{\"error\": \"$result\"}"
    fi
}

# Start server (simplified - use Python HTTP server for production)
echo "Chaincode API server starting on port 8080..."
echo "Usage: This is a development helper. For production, use Fabric Gateway SDK."

# For now, provide instructions
cat > /tmp/chaincode-api-instructions.txt <<EOF
To use chaincode API:

1. Install Python HTTP server:
   python3 -m http.server 8080

2. Or use this script with socat:
   socat TCP-LISTEN:8080,fork EXEC:./chaincode-api.sh

3. Or use the .NET Fabric Gateway service (recommended for production)
EOF

cat /tmp/chaincode-api-instructions.txt

