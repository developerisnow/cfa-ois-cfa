#!/bin/bash
# Setup Timeweb Cloud cluster access and get configuration
# Usage: ./ops/scripts/setup-twc-cluster.sh [cluster-name]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
CLUSTER_NAME="${1:-ois-cfa-k8s}"

# Ensure twc is in PATH
export PATH="${HOME}/.local/bin:${PATH}"

echo "=== Timeweb Cloud Cluster Setup ==="
echo "Cluster name: ${CLUSTER_NAME}"
echo ""

# Check if twc is installed
if ! command -v twc &> /dev/null; then
    echo "Error: twc CLI is not installed."
    echo "Installing twc..."
    "${PROJECT_ROOT}/tools/timeweb/install.sh"
    export PATH="${HOME}/.local/bin:${PATH}"
fi

# Check for token
TWC_TOKEN="${TWC_TOKEN:-}"
if [ -z "${TWC_TOKEN}" ]; then
    # Try to get from terraform.tfvars
    if [ -f "${PROJECT_ROOT}/ops/infra/timeweb/terraform.tfvars" ]; then
        echo "Reading token from terraform.tfvars..."
        TWC_TOKEN=$(grep -E "^twc_token" "${PROJECT_ROOT}/ops/infra/timeweb/terraform.tfvars" | sed 's/twc_token = "\(.*\)"/\1/' | tr -d ' ')
    fi
fi

if [ -z "${TWC_TOKEN}" ]; then
    echo "Error: TWC_TOKEN is not set."
    echo ""
    echo "Please set it using one of the following methods:"
    echo "1. Environment variable:"
    echo "   export TWC_TOKEN='your-token-here'"
    echo ""
    echo "2. Configure twc:"
    echo "   twc config set token 'your-token-here'"
    echo ""
    echo "3. Or create terraform.tfvars with token:"
    echo "   cp ops/infra/timeweb/terraform.tfvars.example ops/infra/timeweb/terraform.tfvars"
    echo "   # Edit terraform.tfvars and set twc_token"
    exit 1
fi

# Set token for twc
export TWC_TOKEN="${TWC_TOKEN}"

echo "=== Verifying twc configuration ==="
if ! twc k8s list &>/dev/null; then
    echo "Error: Failed to authenticate with Timeweb Cloud."
    echo "Please check your TWC_TOKEN."
    exit 1
fi

echo "✓ Authentication successful"
echo ""

# List clusters
echo "=== Available Kubernetes Clusters ==="
twc k8s list
echo ""

# Get cluster ID
echo "=== Finding cluster: ${CLUSTER_NAME} ==="
CLUSTER_ID=$(twc k8s list --format json 2>/dev/null | jq -r ".[] | select(.name == \"${CLUSTER_NAME}\") | .id" 2>/dev/null || echo "")

if [ -z "${CLUSTER_ID}" ] || [ "${CLUSTER_ID}" == "null" ]; then
    echo "Warning: Cluster '${CLUSTER_NAME}' not found."
    echo ""
    echo "Available clusters:"
    twc k8s list
    echo ""
    echo "To create a new cluster, use Terraform:"
    echo "  cd ops/infra/timeweb"
    echo "  terraform init"
    echo "  terraform plan"
    echo "  terraform apply"
    exit 1
fi

echo "✓ Found cluster ID: ${CLUSTER_ID}"
echo ""

# Get cluster details
echo "=== Cluster Details ==="
twc k8s show "${CLUSTER_ID}" 2>/dev/null || twc k8s list --format json | jq ".[] | select(.id == \"${CLUSTER_ID}\")"
echo ""

# Get node groups
echo "=== Node Groups ==="
twc k8s group list --cluster-id "${CLUSTER_ID}" 2>/dev/null || echo "No node groups found or command not available"
echo ""

# Export kubeconfig
echo "=== Exporting Kubeconfig ==="
KUBECONFIG_FILE="${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml"
twc k8s kubeconfig "${CLUSTER_ID}" > "${KUBECONFIG_FILE}" 2>/dev/null || \
twc k8s cluster kubeconfig "${CLUSTER_ID}" > "${KUBECONFIG_FILE}" 2>/dev/null || \
(twc k8s cluster get-kubeconfig "${CLUSTER_ID}" > "${KUBECONFIG_FILE}" 2>/dev/null && echo "Using get-kubeconfig command")

if [ $? -eq 0 ] && [ -f "${KUBECONFIG_FILE}" ]; then
    chmod 600 "${KUBECONFIG_FILE}"
    echo "✓ Kubeconfig exported to: ${KUBECONFIG_FILE}"
    echo ""
    
    # Verify kubeconfig
    if command -v kubectl &> /dev/null; then
        echo "=== Verifying Kubeconfig ==="
        export KUBECONFIG="${KUBECONFIG_FILE}"
        if kubectl cluster-info --request-timeout=10s &>/dev/null; then
            echo "✓ Successfully connected to cluster"
            echo ""
            echo "Cluster information:"
            kubectl cluster-info | head -3
            echo ""
            echo "Nodes:"
            kubectl get nodes
            echo ""
            echo "To use this kubeconfig:"
            echo "  export KUBECONFIG=\"${KUBECONFIG_FILE}\""
            echo "  kubectl get nodes"
        else
            echo "⚠ Warning: Could not connect to cluster (may be still provisioning)"
        fi
    else
        echo "kubectl not found. Install kubectl to verify connection."
    fi
else
    echo "Error: Failed to export kubeconfig"
    echo "Trying alternative method..."
    "${PROJECT_ROOT}/tools/timeweb/kubeconfig-export.sh" "${CLUSTER_NAME}" "${KUBECONFIG_FILE}"
fi

echo ""
echo "=== Setup Complete ==="
echo "Kubeconfig: ${KUBECONFIG_FILE}"
echo "Cluster ID: ${CLUSTER_ID}"
echo "Cluster Name: ${CLUSTER_NAME}"

