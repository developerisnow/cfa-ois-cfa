#!/usr/bin/env bash
# Check GitLab CI jobs status
# Usage: ./ops/scripts/check-gitlab-jobs.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}=== GitLab CI Jobs Status Check ===${NC}"
echo ""

# Check if we have GitLab token
GITLAB_TOKEN="${GITLAB_TOKEN:-}"
GITLAB_URL="${GITLAB_URL:-https://git.telex.global}"
PROJECT_ID="${PROJECT_ID:-npk/ois-cfa}"

if [ -z "${GITLAB_TOKEN}" ]; then
    echo -e "${YELLOW}⚠ GITLAB_TOKEN not set${NC}"
    echo "Set it with: export GITLAB_TOKEN='your-token'"
    echo ""
    echo "Checking runner status instead..."
    echo ""
else
    echo -e "${GREEN}✓ GITLAB_TOKEN set${NC}"
    echo "GitLab URL: ${GITLAB_URL}"
    echo "Project: ${PROJECT_ID}"
    echo ""
    
    # Get latest pipelines
    echo -e "${BLUE}=== Latest Pipelines ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/pipelines?per_page=5" | \
        jq -r '.[] | "\(.id) | \(.status) | \(.ref) | \(.created_at)"' || echo "Failed to get pipelines"
    echo ""
    
    # Get running jobs
    echo -e "${BLUE}=== Running Jobs ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/jobs?scope=running" | \
        jq -r '.[] | "\(.id) | \(.name) | \(.status) | \(.stage) | Runner: \(.runner.description // "none")"' || echo "No running jobs or failed to get jobs"
    echo ""
    
    # Get pending jobs
    echo -e "${BLUE}=== Pending Jobs ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/jobs?scope=pending" | \
        jq -r '.[] | "\(.id) | \(.name) | \(.status) | \(.stage) | Runner: \(.runner.description // "none")"' || echo "No pending jobs or failed to get jobs"
    echo ""
fi

# Check runner status
echo -e "${BLUE}=== Runner Status (Kubernetes) ===${NC}"
KUBECONFIG_FILE="${KUBECONFIG:-}"
if [ -z "${KUBECONFIG_FILE}" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then
    KUBECONFIG_FILE="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
fi

if [ -n "${KUBECONFIG_FILE}" ] && [ -f "${KUBECONFIG_FILE}" ]; then
    export KUBECONFIG="${KUBECONFIG_FILE}"
    kubectl get pods -n gitlab-runner -l app=gitlab-runner 2>/dev/null || echo "Cannot connect to cluster"
    echo ""
    echo "Runner logs (last 20 lines):"
    kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=20 2>/dev/null | tail -20 || echo "Cannot get logs"
else
    echo "KUBECONFIG not configured"
fi

echo ""
echo -e "${BLUE}=== Check Complete ===${NC}"

