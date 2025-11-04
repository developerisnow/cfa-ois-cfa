#!/bin/bash
set -e

echo "=== Fabric Network Health Check ==="

# Check orderer
echo -n "Orderer: "
if docker exec orderer.example.com ls /var/hyperledger/orderer/orderer.genesis.block > /dev/null 2>&1; then
    echo "✓ OK"
else
    echo "✗ FAILED"
    exit 1
fi

# Check peers
for peer in peer0.ois-dev.example.com peer1.ois-dev.example.com; do
    echo -n "$peer: "
    if docker exec $peer peer channel list > /dev/null 2>&1; then
        echo "✓ OK"
    else
        echo "✗ FAILED"
        exit 1
    fi
done

# Check CouchDB
for couchdb in couchdb0 couchdb1; do
    echo -n "$couchdb: "
    if docker exec $couchdb curl -s http://admin:adminpw@localhost:5984/_up > /dev/null 2>&1; then
        echo "✓ OK"
    else
        echo "✗ FAILED"
        exit 1
    fi
done

# Check CA
echo -n "CA: "
if docker exec ca.ois-dev.example.com fabric-ca-server version > /dev/null 2>&1; then
    echo "✓ OK"
else
    echo "✗ FAILED"
    exit 1
fi

echo ""
echo "All services are healthy!"

