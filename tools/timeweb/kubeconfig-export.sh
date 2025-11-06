#!/bin/bash
# Export kubeconfig from Timeweb Cloud Kubernetes cluster
# Usage: ./tools/timeweb/kubeconfig-export.sh [cluster-name] [output-file]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Default values
CLUSTER_NAME="${1:-ois-cfa-k8s}"
OUTPUT_FILE="${2:-${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml}"

# Check if twc is installed
if ! command -v twc &> /dev/null; then
    if [ -f "${HOME}/.local/bin/twc" ]; then
        export PATH="${HOME}/.local/bin:${PATH}"
    else
        echo "Error: twc CLI is not installed."
        echo "Run: ./tools/timeweb/install.sh"
        exit 1
    fi
fi

# Check if TWC_TOKEN is set
if [ -z "${TWC_TOKEN:-}" ]; then
    echo "Error: TWC_TOKEN environment variable is not set."
    echo "Set it with: export TWC_TOKEN='your-token-here'"
    echo "Or configure twc: twc config set token <your-token>"
    exit 1
fi

echo "Exporting kubeconfig for cluster: ${CLUSTER_NAME}"
echo "Output file: ${OUTPUT_FILE}"

# Get cluster ID by name
echo "Finding cluster ID..."
CLUSTER_ID=$(twc k8s cluster list --format json | jq -r ".[] | select(.name == \"${CLUSTER_NAME}\") | .id")

if [ -z "${CLUSTER_ID}" ] || [ "${CLUSTER_ID}" == "null" ]; then
    echo "Error: Cluster '${CLUSTER_NAME}' not found."
    echo "Available clusters:"
    twc k8s cluster list
    exit 1
fi

echo "Found cluster ID: ${CLUSTER_ID}"

# Export kubeconfig
echo "Exporting kubeconfig..."
twc k8s cluster get-kubeconfig "${CLUSTER_ID}" > "${OUTPUT_FILE}"

if [ $? -eq 0 ]; then
    echo "✓ Kubeconfig exported successfully to ${OUTPUT_FILE}"
    
    # Set permissions
    chmod 600 "${OUTPUT_FILE}"
    
    # Verify kubeconfig
    echo ""
    echo "Verifying kubeconfig..."
    if command -v kubectl &> /dev/null; then
        export KUBECONFIG="${OUTPUT_FILE}"
        echo "Testing connection..."
        kubectl cluster-info --request-timeout=10s || echo "Warning: Could not connect to cluster (may be still provisioning)"
        echo ""
        echo "To use this kubeconfig:"
        echo "  export KUBECONFIG=\"${OUTPUT_FILE}\""
        echo "  kubectl get nodes"
    else
        echo "kubectl not found. Install kubectl to verify connection."
    fi
else
    echo "Error: Failed to export kubeconfig"
    exit 1
fi

