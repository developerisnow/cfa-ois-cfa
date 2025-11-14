#!/bin/bash
# Setup kubeconfig for Kubernetes cluster
# Usage: ./ops/scripts/setup-kubeconfig.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
KUBECONFIG_FILE="${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml"

echo -e "${BLUE}=== Kubernetes Kubeconfig Setup ===${NC}"
echo ""

# Check kubectl
if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}✗ kubectl is not installed${NC}"
    echo "Install kubectl: https://kubernetes.io/docs/tasks/tools/"
    exit 1
fi
echo -e "${GREEN}✓ kubectl is installed${NC}"

# Check if kubeconfig already works
if kubectl cluster-info &>/dev/null 2>&1; then
    echo -e "${GREEN}✓ kubeconfig is already configured and working${NC}"
    echo ""
    echo "Current cluster:"
    kubectl cluster-info | head -1
    echo ""
    echo "Nodes:"
    kubectl get nodes 2>/dev/null | head -3 || echo "  (Cannot list nodes - may need permissions)"
    exit 0
fi

echo -e "${YELLOW}⚠ kubeconfig is not configured${NC}"
echo ""

# Check if kubeconfig file exists
if [ -f "$KUBECONFIG_FILE" ]; then
    echo -e "${GREEN}✓ Found kubeconfig file: $KUBECONFIG_FILE${NC}"
    echo ""
    echo "Setting KUBECONFIG environment variable..."
    export KUBECONFIG="$KUBECONFIG_FILE"
    
    if kubectl cluster-info &>/dev/null 2>&1; then
        echo -e "${GREEN}✓ kubeconfig file is valid and working!${NC}"
        echo ""
        echo "To use this kubeconfig in current session:"
        echo -e "${BLUE}  export KUBECONFIG=\"$KUBECONFIG_FILE\"${NC}"
        echo ""
        echo "To make it permanent, add to ~/.bashrc or ~/.zshrc:"
        echo -e "${BLUE}  echo 'export KUBECONFIG=\"$KUBECONFIG_FILE\"' >> ~/.bashrc${NC}"
        exit 0
    else
        echo -e "${YELLOW}⚠ kubeconfig file exists but cannot connect to cluster${NC}"
        echo "  File may be outdated or cluster may be unavailable"
    fi
fi

# Try to export from Timeweb Cloud
echo "Attempting to export kubeconfig from Timeweb Cloud..."
echo ""

# Check twc CLI
if ! command -v twc &> /dev/null; then
    if [ -f "${HOME}/.local/bin/twc" ]; then
        export PATH="${HOME}/.local/bin:${PATH}"
    else
        echo -e "${RED}✗ twc CLI is not installed${NC}"
        echo ""
        echo "To install twc CLI:"
        echo "  ./tools/timeweb/install.sh"
        echo ""
        echo "Or manually:"
        echo "  pip install --user twc-cli"
        echo "  export PATH=\"\${HOME}/.local/bin:\${PATH}\""
        exit 1
    fi
fi
echo -e "${GREEN}✓ twc CLI is installed${NC}"

# Check TWC_TOKEN or twc config
if [ -z "${TWC_TOKEN:-}" ]; then
    # Check if twc can work without explicit token (may be configured via twc config)
    if ! twc k8s list &>/dev/null; then
        echo -e "${YELLOW}⚠ TWC_TOKEN is not set and twc config is not configured${NC}"
        echo ""
        echo "To get TWC_TOKEN:"
        echo "  1. Go to https://timeweb.cloud"
        echo "  2. API → Токены доступа"
        echo "  3. Create new token with permissions: k8s:read, k8s:write"
        echo ""
        echo "Then set it:"
        echo -e "${BLUE}  export TWC_TOKEN='your-token-here'${NC}"
        echo ""
        echo "Or configure twc:"
        echo -e "${BLUE}  twc config set token 'your-token-here'${NC}"
        exit 1
    else
        echo -e "${GREEN}✓ twc CLI is configured (token via twc config)${NC}"
    fi
else
    echo -e "${GREEN}✓ TWC_TOKEN is set${NC}"
fi

# Try to export kubeconfig
echo ""
echo "Exporting kubeconfig..."
if [ -f "${PROJECT_ROOT}/tools/timeweb/kubeconfig-export.sh" ]; then
    "${PROJECT_ROOT}/tools/timeweb/kubeconfig-export.sh" ois-cfa-k8s
else
    echo -e "${RED}✗ kubeconfig-export.sh not found${NC}"
    exit 1
fi

# Set KUBECONFIG
if [ -f "$KUBECONFIG_FILE" ]; then
    export KUBECONFIG="$KUBECONFIG_FILE"
    echo ""
    echo -e "${GREEN}✓ Kubeconfig exported successfully${NC}"
    echo ""
    echo "Testing connection..."
    if kubectl cluster-info &>/dev/null 2>&1; then
        echo -e "${GREEN}✓ Successfully connected to cluster!${NC}"
        echo ""
        echo "Cluster info:"
        kubectl cluster-info | head -3
        echo ""
        echo "To use this kubeconfig in current session:"
        echo -e "${BLUE}  export KUBECONFIG=\"$KUBECONFIG_FILE\"${NC}"
        echo ""
        echo "To make it permanent, add to ~/.bashrc or ~/.zshrc:"
        echo -e "${BLUE}  echo 'export KUBECONFIG=\"$KUBECONFIG_FILE\"' >> ~/.bashrc${NC}"
    else
        echo -e "${YELLOW}⚠ Kubeconfig exported but cannot connect to cluster${NC}"
        echo "  Cluster may be still provisioning or unavailable"
        echo ""
        echo "You can still use the kubeconfig file:"
        echo -e "${BLUE}  export KUBECONFIG=\"$KUBECONFIG_FILE\"${NC}"
    fi
else
    echo -e "${RED}✗ Failed to export kubeconfig${NC}"
    exit 1
fi

