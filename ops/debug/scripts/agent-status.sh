#!/bin/bash
# Check GitLab Agent status
# Usage: ./agent-status.sh

set -euo pipefail

ARTIFACTS_DIR="${ARTIFACTS_DIR:-/tmp/artifacts}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
OUTPUT_FILE="${ARTIFACTS_DIR}/gitlab-agent-status-${TIMESTAMP}.txt"

echo "=== GitLab Agent Status Check ==="
echo "Timestamp: ${TIMESTAMP}"

mkdir -p "${ARTIFACTS_DIR}"

# Check if GitLab Agent is installed
if ! kubectl get namespace gitlab-agent &>/dev/null; then
  echo "Error: GitLab Agent namespace not found"
  echo "GitLab Agent is not installed in this cluster" > "${OUTPUT_FILE}"
  exit 1
fi

# Check agent pods
echo ""
echo "=== GitLab Agent Pods ===" | tee "${OUTPUT_FILE}"
kubectl get pods -n gitlab-agent -o wide | tee -a "${OUTPUT_FILE}"

# Agent pod status
echo ""
echo "=== Agent Pod Status ===" | tee -a "${OUTPUT_FILE}"
for pod in $(kubectl get pods -n gitlab-agent -o jsonpath='{.items[*].metadata.name}'); do
  echo ""
  echo "Pod: ${pod}" | tee -a "${OUTPUT_FILE}"
  kubectl get pod "${pod}" -n gitlab-agent -o yaml | tee -a "${OUTPUT_FILE}" || true
  
  # Get logs (last 50 lines)
  echo ""
  echo "Recent logs (last 50 lines):" | tee -a "${OUTPUT_FILE}"
  kubectl logs -n gitlab-agent "${pod}" --tail=50 | tee -a "${OUTPUT_FILE}" || true
done

# Check agent configuration
echo ""
echo "=== Agent Configuration ===" | tee -a "${OUTPUT_FILE}"
kubectl get configmap -n gitlab-agent -o yaml | tee -a "${OUTPUT_FILE}" || true

# Check connected repositories
echo ""
echo "=== Connected Repositories ===" | tee -a "${OUTPUT_FILE}"
# This would require GitLab API access
if [ -n "${GITLAB_TOKEN:-}" ] && [ -n "${CI_PROJECT_ID:-}" ]; then
  echo "Checking GitLab API for agent status..."
  curl -s --header "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
    "https://${GITLAB_HOST}/api/v4/projects/${CI_PROJECT_ID}/cluster_agents" | \
    jq '.' | tee -a "${OUTPUT_FILE}" || echo "Failed to fetch agent status from GitLab API" | tee -a "${OUTPUT_FILE}"
else
  echo "GITLAB_TOKEN or CI_PROJECT_ID not set, skipping API check" | tee -a "${OUTPUT_FILE}"
fi

# Summary
echo ""
echo "=== Summary ===" | tee -a "${OUTPUT_FILE}"
RUNNING=$(kubectl get pods -n gitlab-agent -o jsonpath='{.items[?(@.status.phase=="Running")].metadata.name}' | wc -w || echo "0")
TOTAL=$(kubectl get pods -n gitlab-agent --no-headers 2>/dev/null | wc -l || echo "0")

echo "Total Agent Pods: ${TOTAL}" | tee -a "${OUTPUT_FILE}"
echo "Running: ${RUNNING}" | tee -a "${OUTPUT_FILE}"

if [ "${RUNNING}" -eq "${TOTAL}" ] && [ "${TOTAL}" -gt 0 ]; then
  echo "Status: OK" | tee -a "${OUTPUT_FILE}"
else
  echo "Status: ISSUES DETECTED" | tee -a "${OUTPUT_FILE}"
fi

echo ""
echo "GitLab Agent status check completed"
echo "Output: ${OUTPUT_FILE}"

# Output for GitLab CI
if [ -n "${CI_JOB_ID:-}" ]; then
  echo "CI_JOB_ID=${CI_JOB_ID}" >> "${ARTIFACTS_DIR}/job-info.txt"
  echo "AGENT_STATUS_FILE=${OUTPUT_FILE}" >> "${ARTIFACTS_DIR}/job-info.txt"
fi

