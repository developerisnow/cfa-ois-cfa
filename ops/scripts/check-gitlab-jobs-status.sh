#!/usr/bin/env bash
# Check GitLab CI jobs status via API and Kubernetes
# Usage: ./ops/scripts/check-gitlab-jobs-status.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

GITLAB_URL="${GITLAB_URL:-https://git.telex.global}"
PROJECT_ID="${PROJECT_ID:-npk/ois-cfa}"
KUBECONFIG_FILE="${KUBECONFIG:-}"

if [ -z "${KUBECONFIG_FILE}" ] && [ -f "ops/infra/timeweb/kubeconfig.yaml" ]; then
    KUBECONFIG_FILE="$(pwd)/ops/infra/timeweb/kubeconfig.yaml"
fi

echo -e "${BLUE}=== GitLab CI Jobs Status Check ===${NC}"
echo ""

# Check Kubernetes pods
if [ -n "${KUBECONFIG_FILE}" ] && [ -f "${KUBECONFIG_FILE}" ]; then
    export KUBECONFIG="${KUBECONFIG_FILE}"
    
    echo -e "${BLUE}=== Kubernetes Job Pods ===${NC}"
    kubectl get pods -n gitlab-runner --no-headers 2>/dev/null | grep "runner-" | while read -r line; do
        NAME=$(echo "$line" | awk '{print $1}')
        STATUS=$(echo "$line" | awk '{print $3}')
        RESTARTS=$(echo "$line" | awk '{print $4}')
        AGE=$(echo "$line" | awk '{print $5}')
        
        if [ "$STATUS" = "Running" ]; then
            echo -e "${GREEN}✓${NC} $NAME: $STATUS (restarts: $RESTARTS, age: $AGE)"
        elif [ "$STATUS" = "Pending" ]; then
            echo -e "${YELLOW}⚠${NC} $NAME: $STATUS (restarts: $RESTARTS, age: $AGE)"
            # Get reason
            REASON=$(kubectl get pod -n gitlab-runner "$NAME" -o jsonpath='{.status.conditions[?(@.type=="PodScheduled")].message}' 2>/dev/null || echo "")
            if [ -n "$REASON" ]; then
                echo "    Reason: $REASON"
            fi
        elif [ "$STATUS" = "Failed" ] || [ "$STATUS" = "Error" ]; then
            echo -e "${RED}✗${NC} $NAME: $STATUS (restarts: $RESTARTS, age: $AGE)"
        else
            echo "  $NAME: $STATUS (restarts: $RESTARTS, age: $AGE)"
        fi
    done
    
    echo ""
    echo -e "${BLUE}=== Runner Pods ===${NC}"
    kubectl get pods -n gitlab-runner -l app=gitlab-runner --no-headers 2>/dev/null | while read -r line; do
        NAME=$(echo "$line" | awk '{print $1}')
        STATUS=$(echo "$line" | awk '{print $3}')
        READY=$(echo "$line" | awk '{print $2}')
        
        if [ "$STATUS" = "Running" ] && [[ "$READY" == "1/1" ]]; then
            echo -e "${GREEN}✓${NC} $NAME: $STATUS ($READY)"
        else
            echo -e "${YELLOW}⚠${NC} $NAME: $STATUS ($READY)"
        fi
    done
    
    echo ""
fi

# Check GitLab API if token available
if [ -n "${GITLAB_TOKEN:-}" ]; then
    echo -e "${BLUE}=== GitLab API: Running Jobs ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/jobs?scope=running" 2>/dev/null | \
        jq -r '.[] | "\(.id) | \(.name) | \(.status) | \(.stage) | Runner: \(.runner.description // "none")"' 2>/dev/null || \
        echo "Нет запущенных jobs или ошибка API"
    
    echo ""
    echo -e "${BLUE}=== GitLab API: Pending Jobs ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/jobs?scope=pending" 2>/dev/null | \
        jq -r '.[] | "\(.id) | \(.name) | \(.status) | \(.stage)"' 2>/dev/null || \
        echo "Нет ожидающих jobs или ошибка API"
    
    echo ""
    echo -e "${BLUE}=== GitLab API: Latest Pipelines ===${NC}"
    curl -sS -H "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}/pipelines?per_page=3" 2>/dev/null | \
        jq -r '.[] | "\(.id) | \(.status) | \(.ref) | \(.created_at)"' 2>/dev/null || \
        echo "Ошибка получения pipelines"
else
    echo -e "${YELLOW}⚠ GITLAB_TOKEN не установлен, пропуск API проверки${NC}"
    echo "Для проверки через API установите:"
    echo "  export GITLAB_TOKEN='ваш-токен'"
fi

echo ""
echo -e "${BLUE}=== Check Complete ===${NC}"

