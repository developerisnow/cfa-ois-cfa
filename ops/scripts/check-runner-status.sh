#!/bin/bash
# Check GitLab Runner status and configuration
# Usage: ./ops/scripts/check-runner-status.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

echo "=== GitLab Runner Status Check ==="
echo ""

# Check kubeconfig
KUBECONFIG_FILE="${KUBECONFIG:-}"
if [ -z "${KUBECONFIG_FILE}" ] && [ -f "${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml" ]; then
    KUBECONFIG_FILE="${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml"
    export KUBECONFIG="${KUBECONFIG_FILE}"
fi

if [ -z "${KUBECONFIG_FILE}" ]; then
    echo "⚠ Warning: KUBECONFIG not set"
    echo "Set it with: export KUBECONFIG=\$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
    echo "Or run: make setup-kubeconfig"
    echo ""
else
    export KUBECONFIG="${KUBECONFIG_FILE}"
    echo "✓ Using KUBECONFIG: ${KUBECONFIG_FILE}"
fi

# Check kubectl
if ! command -v kubectl &> /dev/null; then
    echo "Error: kubectl not found"
    exit 1
fi

# Check cluster connection
if ! kubectl cluster-info &>/dev/null; then
    echo "⚠ Warning: Cannot connect to cluster"
    echo "Check kubeconfig: kubectl cluster-info"
    exit 1
fi

echo "✓ Cluster connection OK"
echo ""

# Check namespace
if ! kubectl get namespace gitlab-runner &>/dev/null; then
    echo "⚠ Warning: gitlab-runner namespace not found"
    echo "Install runner: make gitlab-runner-install"
    exit 1
fi

echo "=== Runner Pods ==="
kubectl get pods -n gitlab-runner
echo ""

# Check pod status
RUNNING_PODS=$(kubectl get pods -n gitlab-runner -l app=gitlab-runner --no-headers 2>/dev/null | grep -c " Running " || echo "0")
TOTAL_PODS=$(kubectl get pods -n gitlab-runner -l app=gitlab-runner --no-headers 2>/dev/null | wc -l || echo "0")

if [ "${RUNNING_PODS}" -eq 0 ]; then
    echo "❌ Error: No running runner pods"
    echo "Check logs: make gitlab-runner-logs"
    exit 1
elif [ "${RUNNING_PODS}" -lt "${TOTAL_PODS}" ]; then
    echo "⚠ Warning: Not all pods are running (${RUNNING_PODS}/${TOTAL_PODS})"
else
    echo "✓ All runner pods running (${RUNNING_PODS}/${TOTAL_PODS})"
fi

echo ""
echo "=== Runner Configuration ==="
kubectl get configmap gitlab-runner-config -n gitlab-runner -o yaml | grep -A 5 "name = " || echo "ConfigMap not found"

echo ""
echo "=== Runner Logs (last 10 lines) ==="
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=10 || echo "Cannot get logs"

echo ""
echo "=== Next Steps ==="
echo "1. Check runner in GitLab UI: Settings → CI/CD → Runners"
echo "2. Verify runner is 'Online' (green indicator)"
echo "3. Check runner tags (should be empty or match job tags)"
echo "4. Try running debug:deploy job manually"
echo ""
echo "To view full logs: make gitlab-runner-logs"
echo "To restart runner: make gitlab-runner-restart"

