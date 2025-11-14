#!/bin/bash
# Check GitLab runners via API
# Usage: ./ops/scripts/check-gitlab-runners.sh [GITLAB_TOKEN] [PROJECT_ID]

set -euo pipefail

GITLAB_URL="${GITLAB_URL:-https://git.telex.global}"
GITLAB_TOKEN="${1:-${GITLAB_TOKEN:-}}"
PROJECT_ID="${2:-${CI_PROJECT_ID:-npk/ois-cfa}}"

if [ -z "${GITLAB_TOKEN}" ]; then
    echo "Error: GITLAB_TOKEN not set"
    echo "Usage: $0 <gitlab-token> [project-id]"
    echo "Or set: export GITLAB_TOKEN='your-token'"
    exit 1
fi

echo "=== GitLab Runners Status ==="
echo "GitLab URL: ${GITLAB_URL}"
echo "Project: ${PROJECT_ID}"
echo ""

# Try to get project ID if path provided
if [[ "${PROJECT_ID}" == *"/"* ]]; then
    echo "Resolving project ID from path: ${PROJECT_ID}"
    PROJECT_ID=$(curl -s --header "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
        "${GITLAB_URL}/api/v4/projects/${PROJECT_ID//\//%2F}" | \
        jq -r '.id // empty' 2>/dev/null || echo "")
    
    if [ -z "${PROJECT_ID}" ] || [ "${PROJECT_ID}" == "null" ]; then
        echo "Error: Cannot resolve project ID. Using path directly."
        PROJECT_PATH="${PROJECT_ID}"
    else
        echo "Project ID: ${PROJECT_ID}"
        PROJECT_PATH="${PROJECT_ID}"
    fi
else
    PROJECT_PATH="${PROJECT_ID}"
fi

echo ""
echo "=== Project Runners ==="
curl -s --header "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
    "${GITLAB_URL}/api/v4/projects/${PROJECT_PATH}/runners" | \
    jq -r '.[] | "\(.id) | \(.name) | \(.description // "N/A") | \(.online) | \(.active) | \(.tag_list | join(",") // "none")"' 2>/dev/null || \
    echo "Error: Cannot fetch runners. Check token permissions."

echo ""
echo "=== Instance/Group Runners ==="
curl -s --header "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
    "${GITLAB_URL}/api/v4/runners/all" | \
    jq -r '.[] | select(.project_ids == [] or (.project_ids | length > 0)) | "\(.id) | \(.name) | \(.description // "N/A") | \(.online) | \(.active) | \(.tag_list | join(",") // "none")"' 2>/dev/null || \
    echo "Error: Cannot fetch instance runners. Check token permissions."

echo ""
echo "=== Runner Registration Token ==="
echo "To get registration token:"
echo "1. Open: ${GITLAB_URL}/${PROJECT_PATH}/-/settings/ci_cd"
echo "2. Expand 'Runners' section"
echo "3. Copy 'Registration token'"
echo ""
echo "Or via API (if token has admin rights):"
echo "curl --header \"PRIVATE-TOKEN: \${GITLAB_TOKEN}\" \\"
echo "  \"${GITLAB_URL}/api/v4/projects/${PROJECT_PATH}/runners_token\""

