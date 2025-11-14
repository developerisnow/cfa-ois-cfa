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

# Check if TWC_TOKEN is set or twc is configured
if [ -z "${TWC_TOKEN:-}" ]; then
    # Check if twc can work without explicit token (may be configured via twc config)
    if ! twc k8s list &>/dev/null; then
        echo "Error: TWC_TOKEN environment variable is not set and twc config is not configured."
        echo "Set it with: export TWC_TOKEN='your-token-here'"
        echo "Or configure twc: twc config set token <your-token>"
        exit 1
    fi
    # twc is configured, continue without TWC_TOKEN
fi

echo "Exporting kubeconfig for cluster: ${CLUSTER_NAME}"
echo "Output file: ${OUTPUT_FILE}"

# Get cluster ID by name
echo "Finding cluster ID..."

# Try with jq first (if available)
if command -v jq &> /dev/null; then
    CLUSTER_ID=$(twc k8s list --output json 2>/dev/null | jq -r ".clusters[]? | select(.name == \"${CLUSTER_NAME}\") | .id" 2>/dev/null || echo "")
else
    # Fallback: parse table output
    CLUSTER_ID=$(twc k8s list 2>/dev/null | grep -E "^\s*[0-9]+\s+${CLUSTER_NAME}\s+" | awk '{print $1}' | head -1 || echo "")
fi

if [ -z "${CLUSTER_ID}" ] || [ "${CLUSTER_ID}" == "null" ]; then
    echo "Error: Cluster '${CLUSTER_NAME}' not found."
    echo "Available clusters:"
    twc k8s list
    echo ""
    echo "Tip: Install jq for better parsing: sudo apt install jq"
    exit 1
fi

echo "Found cluster ID: ${CLUSTER_ID}"

# Export kubeconfig
echo "Exporting kubeconfig..."
if ! twc k8s kubeconfig "${CLUSTER_ID}" > "${OUTPUT_FILE}" 2>&1; then
    echo "Error: Failed to export kubeconfig"
    echo "Cluster ID: ${CLUSTER_ID}"
    echo "Check twc CLI version and permissions"
    exit 1
fi

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

