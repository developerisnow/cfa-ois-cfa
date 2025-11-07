#!/bin/bash
# Collect logs from pods in specified namespaces
# Usage: ./logs-collect.sh [namespace1] [namespace2] ...

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${OUTPUT_DIR:-/tmp/logs-collect}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
ARTIFACTS_DIR="${ARTIFACTS_DIR:-/tmp/artifacts}"

# Default namespaces if not provided
NAMESPACES="${@:-fabric-network ois-cfa argocd keycloak monitoring}"

echo "=== Collecting logs ==="
echo "Namespaces: ${NAMESPACES}"
echo "Output directory: ${OUTPUT_DIR}"
echo "Timestamp: ${TIMESTAMP}"

mkdir -p "${OUTPUT_DIR}"
mkdir -p "${ARTIFACTS_DIR}"

# Collect logs for each namespace
for namespace in ${NAMESPACES}; do
  echo ""
  echo "Processing namespace: ${namespace}"
  
  # Check if namespace exists
  if ! kubectl get namespace "${namespace}" &>/dev/null; then
    echo "Warning: Namespace ${namespace} does not exist, skipping"
    continue
  fi
  
  NS_DIR="${OUTPUT_DIR}/${namespace}"
  mkdir -p "${NS_DIR}"
  
  # Get all pods in namespace
  PODS=$(kubectl get pods -n "${namespace}" -o jsonpath='{.items[*].metadata.name}' 2>/dev/null || echo "")
  
  if [ -z "${PODS}" ]; then
    echo "No pods found in namespace ${namespace}"
    continue
  fi
  
  # Collect logs for each pod
  for pod in ${PODS}; do
    echo "  Collecting logs for pod: ${pod}"
    
    # Get pod logs (all containers)
    kubectl logs -n "${namespace}" "${pod}" --all-containers=true --timestamps=true > "${NS_DIR}/${pod}.log" 2>&1 || {
      echo "    Warning: Failed to get logs for ${pod}"
      echo "Failed to get logs for ${pod}" > "${NS_DIR}/${pod}.log.error"
    }
    
    # Get pod description
    kubectl describe pod -n "${namespace}" "${pod}" > "${NS_DIR}/${pod}.describe.txt" 2>&1 || true
    
    # Get pod YAML
    kubectl get pod -n "${namespace}" "${pod}" -o yaml > "${NS_DIR}/${pod}.yaml" 2>&1 || true
  done
  
  # Get namespace events
  echo "  Collecting events for namespace: ${namespace}"
  kubectl get events -n "${namespace}" --sort-by='.lastTimestamp' > "${NS_DIR}/events.txt" 2>&1 || true
  
  # Get all resources in namespace (summary)
  echo "  Collecting resource summary for namespace: ${namespace}"
  kubectl get all -n "${namespace}" > "${NS_DIR}/resources.txt" 2>&1 || true
done

# Create summary
echo ""
echo "=== Creating summary ==="
SUMMARY_FILE="${OUTPUT_DIR}/summary.txt"
cat > "${SUMMARY_FILE}" <<EOF
Logs Collection Summary
=======================
Timestamp: ${TIMESTAMP}
Namespaces: ${NAMESPACES}

Namespaces Processed:
EOF

for namespace in ${NAMESPACES}; do
  if kubectl get namespace "${namespace}" &>/dev/null; then
    POD_COUNT=$(kubectl get pods -n "${namespace}" --no-headers 2>/dev/null | wc -l || echo "0")
    echo "  - ${namespace}: ${POD_COUNT} pods" >> "${SUMMARY_FILE}"
  fi
done

# Create archive
echo ""
echo "=== Creating archive ==="
ARCHIVE_NAME="logs-${TIMESTAMP}.tar.gz"
cd "${OUTPUT_DIR}"
tar -czf "${ARTIFACTS_DIR}/${ARCHIVE_NAME}" . 2>/dev/null || {
  echo "Warning: Failed to create archive, copying files directly"
  cp -r "${OUTPUT_DIR}"/* "${ARTIFACTS_DIR}/" || true
}

echo "Logs collected successfully"
echo "Archive: ${ARTIFACTS_DIR}/${ARCHIVE_NAME}"
echo "Summary: ${SUMMARY_FILE}"

# Output for GitLab CI artifacts
if [ -n "${CI_JOB_ID:-}" ]; then
  echo "CI_JOB_ID=${CI_JOB_ID}" > "${ARTIFACTS_DIR}/job-info.txt"
  echo "ARTIFACT_PATH=${ARTIFACTS_DIR}/${ARCHIVE_NAME}" >> "${ARTIFACTS_DIR}/job-info.txt"
fi

