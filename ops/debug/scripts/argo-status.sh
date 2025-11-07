#!/bin/bash
# Check ArgoCD applications status
# Usage: ./argo-status.sh

set -euo pipefail

ARTIFACTS_DIR="${ARTIFACTS_DIR:-/tmp/artifacts}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
OUTPUT_FILE="${ARTIFACTS_DIR}/argocd-status-${TIMESTAMP}.txt"

echo "=== ArgoCD Status Check ==="
echo "Timestamp: ${TIMESTAMP}"

mkdir -p "${ARTIFACTS_DIR}"

# Check if ArgoCD is installed
if ! kubectl get namespace argocd &>/dev/null; then
  echo "Error: ArgoCD namespace not found"
  echo "ArgoCD is not installed in this cluster" > "${OUTPUT_FILE}"
  exit 1
fi

# Check ArgoCD server status
echo ""
echo "=== ArgoCD Server Status ==="
kubectl get pods -n argocd -l app.kubernetes.io/name=argocd-server || echo "ArgoCD server pods not found"

# Get all applications
echo ""
echo "=== ArgoCD Applications ==="
kubectl get applications -n argocd -o wide || echo "No applications found"

# Detailed status for each application
echo ""
echo "=== Detailed Application Status ===" > "${OUTPUT_FILE}"
for app in $(kubectl get applications -n argocd -o jsonpath='{.items[*].metadata.name}'); do
  echo ""
  echo "Application: ${app}"
  echo "---" | tee -a "${OUTPUT_FILE}"
  
  # Get application status
  kubectl get application "${app}" -n argocd -o yaml | tee -a "${OUTPUT_FILE}" || true
  
  # Get application conditions
  echo ""
  echo "Conditions:" | tee -a "${OUTPUT_FILE}"
  kubectl get application "${app}" -n argocd -o jsonpath='{.status.conditions[*]}' | jq -r '.' 2>/dev/null | tee -a "${OUTPUT_FILE}" || true
  
  # Get sync status
  echo ""
  echo "Sync Status:" | tee -a "${OUTPUT_FILE}"
  kubectl get application "${app}" -n argocd -o jsonpath='{.status.sync.status}' | tee -a "${OUTPUT_FILE}" || true
  
  # Get health status
  echo ""
  echo "Health Status:" | tee -a "${OUTPUT_FILE}"
  kubectl get application "${app}" -n argocd -o jsonpath='{.status.health.status}' | tee -a "${OUTPUT_FILE}" || true
  
  echo ""
done

# Summary
echo ""
echo "=== Summary ===" | tee -a "${OUTPUT_FILE}"
SYNCED=$(kubectl get applications -n argocd -o jsonpath='{.items[?(@.status.sync.status=="Synced")].metadata.name}' | wc -w || echo "0")
HEALTHY=$(kubectl get applications -n argocd -o jsonpath='{.items[?(@.status.health.status=="Healthy")].metadata.name}' | wc -w || echo "0")
TOTAL=$(kubectl get applications -n argocd --no-headers 2>/dev/null | wc -l || echo "0")

echo "Total Applications: ${TOTAL}" | tee -a "${OUTPUT_FILE}"
echo "Synced: ${SYNCED}" | tee -a "${OUTPUT_FILE}"
echo "Healthy: ${HEALTHY}" | tee -a "${OUTPUT_FILE}"

# Applications with issues
echo ""
echo "=== Applications with Issues ===" | tee -a "${OUTPUT_FILE}"
kubectl get applications -n argocd -o json | jq -r '.items[] | select(.status.sync.status != "Synced" or .status.health.status != "Healthy") | "\(.metadata.name): Sync=\(.status.sync.status // "Unknown"), Health=\(.status.health.status // "Unknown")"' 2>/dev/null | tee -a "${OUTPUT_FILE}" || echo "No issues found" | tee -a "${OUTPUT_FILE}"

echo ""
echo "ArgoCD status check completed"
echo "Output: ${OUTPUT_FILE}"

# Output for GitLab CI
if [ -n "${CI_JOB_ID:-}" ]; then
  echo "CI_JOB_ID=${CI_JOB_ID}" >> "${ARTIFACTS_DIR}/job-info.txt"
  echo "ARGOCD_STATUS_FILE=${OUTPUT_FILE}" >> "${ARTIFACTS_DIR}/job-info.txt"
fi

