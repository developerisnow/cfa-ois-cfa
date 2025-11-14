#!/usr/bin/env bash
# Diagnose GitLab Runner 403 Forbidden issue
# Usage: GITLAB_URL=https://git.telex.global RUNNER_TOKEN=glrt-... ./ops/ci/diagnose_runner.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Defaults
GITLAB_URL="${GITLAB_URL:-https://git.telex.global}"
RUNNER_TOKEN="${RUNNER_TOKEN:-}"
STATE_FILE="${STATE_FILE:-/home/gitlab-runner/.runner_system_id}"

TS=$(date +%Y%m%d-%H%M%S)
ARCHIVE_DIR="ARCHIVE/runner"
mkdir -p "${ARCHIVE_DIR}"

echo -e "${GREEN}=== GitLab Runner 403 Diagnosis ===${NC}"
echo "Timestamp: ${TS}"
echo ""

# Check token
if [ -z "${RUNNER_TOKEN}" ]; then
    echo -e "${RED}Error: RUNNER_TOKEN not set${NC}"
    echo "Usage: RUNNER_TOKEN=glrt-... ./ops/ci/diagnose_runner.sh"
    exit 1
fi

# Mask token for display
TOKEN_PREFIX=$(echo "${RUNNER_TOKEN}" | cut -c1-12)
TOKEN_MASKED="${TOKEN_PREFIX}***MASKED***"

echo -e "${YELLOW}=== Token Analysis ===${NC}"
echo "Token prefix: ${TOKEN_PREFIX}"
if [[ "${RUNNER_TOKEN}" =~ ^glrt- ]]; then
    echo -e "${GREEN}✓ Token type: Authentication token (glrt-)${NC}"
elif [[ "${RUNNER_TOKEN}" =~ ^GR[0-9]+ ]]; then
    echo -e "${YELLOW}⚠ Token type: Registration token (GR...) - should be authentication token${NC}"
else
    echo -e "${RED}✗ Unknown token format${NC}"
fi
echo ""

# Check state_file
echo -e "${YELLOW}=== State File Check ===${NC}"
if [ -f "${STATE_FILE}" ]; then
    echo -e "${GREEN}✓ State file exists: ${STATE_FILE}${NC}"
    if [ -r "${STATE_FILE}" ]; then
        SYSTEM_ID=$(cat "${STATE_FILE}" 2>/dev/null || echo "")
        if [ -n "${SYSTEM_ID}" ]; then
            echo "System ID: ${SYSTEM_ID}"
        else
            echo -e "${YELLOW}⚠ State file is empty${NC}"
        fi
    else
        echo -e "${RED}✗ State file not readable${NC}"
    fi
    if [ -w "${STATE_FILE}" ]; then
        echo -e "${GREEN}✓ State file is writable${NC}"
    else
        echo -e "${RED}✗ State file not writable${NC}"
    fi
else
    echo -e "${YELLOW}⚠ State file not found: ${STATE_FILE}${NC}"
    SYSTEM_ID=""
fi
echo ""

# Verify with GitLab API
echo -e "${YELLOW}=== GitLab API Verification ===${NC}"
echo "URL: ${GITLAB_URL}/api/v4/runners/verify"
echo "Token: ${TOKEN_MASKED}"
if [ -n "${SYSTEM_ID}" ]; then
    echo "System ID: ${SYSTEM_ID}"
else
    echo "System ID: (not provided)"
fi
echo ""

VERIFY_LOG="${ARCHIVE_DIR}/verify-${TS}.log"
{
    echo "=== GitLab Runner Verify Request ==="
    echo "Timestamp: $(date -Iseconds)"
    echo "URL: ${GITLAB_URL}/api/v4/runners/verify"
    echo "Token: ${TOKEN_MASKED}"
    echo "System ID: ${SYSTEM_ID:-not-provided}"
    echo ""
    echo "--- Request ---"
    
    if [ -n "${SYSTEM_ID}" ]; then
        RESPONSE=$(curl -sS -w "\nHTTP_CODE:%{http_code}" -X POST \
            "${GITLAB_URL}/api/v4/runners/verify" \
            -F "token=${RUNNER_TOKEN}" \
            -F "system_id=${SYSTEM_ID}" \
            -i 2>&1)
    else
        RESPONSE=$(curl -sS -w "\nHTTP_CODE:%{http_code}" -X POST \
            "${GITLAB_URL}/api/v4/runners/verify" \
            -F "token=${RUNNER_TOKEN}" \
            -i 2>&1)
    fi
    
    echo "${RESPONSE}"
    echo ""
    echo "--- Response Analysis ---"
    
    HTTP_CODE=$(echo "${RESPONSE}" | grep -o "HTTP_CODE:[0-9]*" | cut -d: -f2 || echo "unknown")
    echo "HTTP Status: ${HTTP_CODE}"
    
    if [ "${HTTP_CODE}" = "200" ]; then
        echo -e "${GREEN}✓ Verification successful (200 OK)${NC}"
    elif [ "${HTTP_CODE}" = "403" ]; then
        echo -e "${RED}✗ Verification failed (403 Forbidden)${NC}"
        echo "Possible causes:"
        echo "  - Invalid or expired authentication token"
        echo "  - Token is registration token, not authentication token"
        echo "  - Runner not registered or deleted in GitLab"
    elif [ "${HTTP_CODE}" = "401" ]; then
        echo -e "${RED}✗ Authentication failed (401 Unauthorized)${NC}"
    else
        echo -e "${YELLOW}⚠ Unexpected status: ${HTTP_CODE}${NC}"
    fi
    
} | tee "${VERIFY_LOG}"

echo ""
echo -e "${GREEN}=== Diagnosis Complete ===${NC}"
echo "Log saved to: ${VERIFY_LOG}"
echo ""

