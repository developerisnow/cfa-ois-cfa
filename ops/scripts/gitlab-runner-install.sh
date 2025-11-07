#!/bin/bash
# Install GitLab Runner in Kubernetes
# Usage: ./ops/scripts/gitlab-runner-install.sh [RUNNER_TOKEN]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check kubectl
if ! command -v kubectl &> /dev/null; then
    echo -e "${RED}Error: kubectl is not installed${NC}"
    exit 1
fi

# Get runner token
RUNNER_TOKEN="${1:-${RUNNER_TOKEN:-}}"

if [ -z "$RUNNER_TOKEN" ]; then
    echo -e "${YELLOW}Runner token not provided.${NC}"
    echo "Getting token from GitLab API..."
    
    if [ -z "${GITLAB_TOKEN:-}" ]; then
        echo -e "${RED}Error: GITLAB_TOKEN not set${NC}"
        echo "Set it with: export GITLAB_TOKEN='your-token'"
        echo ""
        echo "Or provide runner token directly:"
        echo "  $0 <runner-token>"
        echo ""
        echo "To get token from GitLab UI:"
        echo "  Settings → CI/CD → Runners → Registration token"
        exit 1
    fi
    
    echo -e "${RED}Cannot get runner token from API (endpoint not available)${NC}"
    echo "Please get token from GitLab UI:"
    echo "  1. Open: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd"
    echo "  2. Expand 'Runners' section"
    echo "  3. Copy 'Registration token'"
    echo ""
    echo "Then run:"
    echo "  $0 <runner-token>"
    exit 1
fi

# Runner directory
RUNNER_DIR="${PROJECT_ROOT}/ops/infra/k8s/gitlab-runner"

echo -e "${GREEN}=== Installing GitLab Runner ===${NC}"
echo "Runner token: ${RUNNER_TOKEN:0:10}...${RUNNER_TOKEN: -10}"
echo ""

# Check if namespace exists
if kubectl get namespace gitlab-runner &>/dev/null; then
    echo -e "${YELLOW}Namespace gitlab-runner already exists${NC}"
else
    echo "Creating namespace..."
    kubectl apply -f "${RUNNER_DIR}/namespace.yaml"
fi

# Apply RBAC
echo "Applying RBAC..."
kubectl apply -f "${RUNNER_DIR}/rbac.yaml"

# Update configmap with token
echo "Updating configmap with runner token..."
TEMP_CONFIGMAP=$(mktemp)
sed "s/__REPLACE_WITH_RUNNER_TOKEN__/$RUNNER_TOKEN/g" \
    "${RUNNER_DIR}/configmap.yaml" > "$TEMP_CONFIGMAP"
kubectl apply -f "$TEMP_CONFIGMAP"
rm "$TEMP_CONFIGMAP"

# Apply deployment and service
echo "Applying deployment..."
kubectl apply -f "${RUNNER_DIR}/deployment.yaml"
kubectl apply -f "${RUNNER_DIR}/service.yaml"

# Wait for pods
echo ""
echo "Waiting for pods to be ready..."
if kubectl wait --for=condition=Ready pod -l app=gitlab-runner -n gitlab-runner --timeout=120s 2>/dev/null; then
    echo -e "${GREEN}✓ GitLab Runner pods are ready${NC}"
else
    echo -e "${YELLOW}⚠ Pods may still be starting. Check status:${NC}"
    echo "  kubectl get pods -n gitlab-runner"
fi

# Show status
echo ""
echo -e "${GREEN}=== GitLab Runner Status ===${NC}"
kubectl get pods -n gitlab-runner
echo ""
echo "Runner logs (last 10 lines):"
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=10 || true

echo ""
echo -e "${GREEN}=== Installation Complete ===${NC}"
echo "Check runners in GitLab UI:"
echo "  https://git.telex.global/npk/ois-cfa/-/settings/ci_cd"
echo ""
echo "Useful commands:"
echo "  kubectl logs -n gitlab-runner -l app=gitlab-runner -f  # Watch logs"
echo "  kubectl get pods -n gitlab-runner                      # Check pods"
echo "  make gitlab-runner-status                              # Show status"

