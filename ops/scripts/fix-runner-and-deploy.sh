#!/bin/bash
# Master script: Fix Runner and Deploy Test Pod
# Usage: ./ops/scripts/fix-runner-and-deploy.sh [RUNNER_TOKEN]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}=== Мастер-скрипт: Исправление Runner и выкатка тестового pod ===${NC}"
echo ""

# Setup kubeconfig
KUBECONFIG_FILE="${KUBECONFIG:-}"
if [ -z "${KUBECONFIG_FILE}" ] && [ -f "${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml" ]; then
    KUBECONFIG_FILE="${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml"
    export KUBECONFIG="${KUBECONFIG_FILE}"
fi

if [ -z "${KUBECONFIG_FILE}" ] || [ ! -f "${KUBECONFIG_FILE}" ]; then
    echo -e "${RED}Error: KUBECONFIG not found${NC}"
    echo "Run: make setup-kubeconfig"
    exit 1
fi

export KUBECONFIG="${KUBECONFIG_FILE}"
echo -e "${GREEN}✓ Using KUBECONFIG: ${KUBECONFIG_FILE}${NC}"
echo ""

# Step 1: Fix Runner
RUNNER_TOKEN="${1:-${RUNNER_TOKEN:-}}"
if [ -n "${RUNNER_TOKEN}" ]; then
    echo -e "${YELLOW}=== ШАГ 1: Исправление GitLab Runner ===${NC}"
    echo "Updating runner token..."
    
    # Update ConfigMap
    sed -e "s/__REPLACE_WITH_RUNNER_TOKEN__/${RUNNER_TOKEN}/g" \
        -e "s/token = \".*\"/token = \"${RUNNER_TOKEN}\"/g" \
        "${PROJECT_ROOT}/ops/infra/k8s/gitlab-runner/configmap.yaml" | \
        kubectl apply -f -
    
    # Restart pods
    echo "Restarting runner pods..."
    kubectl rollout restart deployment/gitlab-runner -n gitlab-runner
    kubectl wait --for=condition=Ready pod -l app=gitlab-runner -n gitlab-runner --timeout=120s || true
    
    echo -e "${GREEN}✓ Runner token updated${NC}"
    echo ""
else
    echo -e "${YELLOW}⚠ RUNNER_TOKEN not provided, skipping runner update${NC}"
    echo "To update runner: export RUNNER_TOKEN='token' && $0"
    echo ""
fi

# Step 2: Deploy test pod
echo -e "${YELLOW}=== ШАГ 2: Выкатка тестового pod ===${NC}"
kubectl apply -f "${PROJECT_ROOT}/ops/infra/k8s/test-pod.yaml"

echo "Waiting for pod to be ready..."
kubectl wait --for=condition=Ready pod -l app=test-nginx -n ois-cfa --timeout=60s || {
    echo -e "${YELLOW}⚠ Pod may still be starting${NC}"
}

echo ""
echo -e "${GREEN}=== Статус развёртывания ===${NC}"
kubectl get pods -n ois-cfa
echo ""
kubectl get svc -n ois-cfa
echo ""
kubectl get ingress -n ois-cfa
echo ""

# Step 3: Get access info
NODE_IP=$(kubectl get nodes -o jsonpath='{.items[0].status.addresses[?(@.type=="ExternalIP")].address}' 2>/dev/null || \
          kubectl get nodes -o jsonpath='{.items[0].status.addresses[?(@.type=="InternalIP")].address}')

echo -e "${GREEN}=== Информация для доступа ===${NC}"
echo "Node IP: ${NODE_IP}"
echo ""
echo "Доступ к тестовому pod:"
echo "1. Через Ingress (если DNS настроен): http://cfa.capital"
echo "2. По IP узла (если настроен NodePort): http://${NODE_IP}"
echo ""
echo "Проверка:"
echo "  curl http://cfa.capital"
echo "  или"
echo "  curl http://${NODE_IP} -H 'Host: cfa.capital'"
echo ""

echo -e "${GREEN}✓ Тестовый pod выкачен${NC}"

