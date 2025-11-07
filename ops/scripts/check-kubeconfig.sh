#!/bin/bash
# Check if kubeconfig is configured and accessible
# Usage: ./ops/scripts/check-kubeconfig.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=== Checking kubeconfig ==="

# Check kubectl
if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}✗ kubectl is not installed${NC}"
    echo "Install kubectl: https://kubernetes.io/docs/tasks/tools/"
    exit 1
fi

echo -e "${GREEN}✓ kubectl is installed${NC}"

# Check kubeconfig
if ! kubectl cluster-info &>/dev/null; then
    echo -e "${RED}✗ kubectl cannot connect to cluster${NC}"
    echo ""
    echo "Kubeconfig is not configured or cluster is not accessible."
    echo ""
    echo "To configure kubeconfig:"
    echo ""
    echo "1. If using Timeweb Cloud:"
    echo "   export TWC_TOKEN='your-token'"
    echo "   ./tools/timeweb/kubeconfig-export.sh ois-cfa-k8s"
    echo "   export KUBECONFIG=\"\$(pwd)/ops/infra/timeweb/kubeconfig.yaml\""
    echo ""
    echo "2. If you have existing kubeconfig:"
    echo "   export KUBECONFIG=\"/path/to/kubeconfig.yaml\""
    echo ""
    echo "3. Or copy to default location:"
    echo "   mkdir -p ~/.kube"
    echo "   cp kubeconfig.yaml ~/.kube/config"
    echo ""
    echo "See: docs/ops/timeweb/kubeconfig.md"
    exit 1
fi

echo -e "${GREEN}✓ kubectl can connect to cluster${NC}"

# Show cluster info
echo ""
echo "=== Cluster Information ==="
kubectl cluster-info | head -3
echo ""
kubectl get nodes 2>/dev/null | head -5 || echo "Cannot get nodes (may need permissions)"

echo ""
echo -e "${GREEN}✓ Kubeconfig is properly configured${NC}"

