#!/bin/bash
# Get GitLab Runner Registration Token via API
# Usage: ./ops/scripts/get-runner-token.sh [GITLAB_TOKEN] [PROJECT_PATH]

set -euo pipefail

GITLAB_URL="${GITLAB_URL:-https://git.telex.global}"
GITLAB_TOKEN="${1:-${GITLAB_TOKEN:-}}"
PROJECT_PATH="${2:-${CI_PROJECT_PATH:-npk/ois-cfa}}"

if [ -z "${GITLAB_TOKEN}" ]; then
    echo "Error: GITLAB_TOKEN not set"
    echo "Usage: $0 <gitlab-token> [project-path]"
    echo "Or set: export GITLAB_TOKEN='your-token'"
    exit 1
fi

echo "Getting Runner Registration Token for project: ${PROJECT_PATH}"
echo ""

# Encode project path for URL
PROJECT_PATH_ENCODED=$(echo "${PROJECT_PATH}" | sed 's/\//%2F/g')

# Try to get runner token via API
RESPONSE=$(curl -s --header "PRIVATE-TOKEN: ${GITLAB_TOKEN}" \
    "${GITLAB_URL}/api/v4/projects/${PROJECT_PATH_ENCODED}/runners_token" 2>/dev/null)

if [ $? -eq 0 ] && [ -n "${RESPONSE}" ]; then
    TOKEN=$(echo "${RESPONSE}" | jq -r '.token // .' 2>/dev/null || echo "${RESPONSE}")
    
    if [ "${TOKEN}" != "null" ] && [ "${TOKEN}" != "" ] && [ "${TOKEN}" != "${RESPONSE}" ]; then
        echo "✓ Runner Registration Token получен:"
        echo "${TOKEN}"
        echo ""
        echo "Для обновления раннера:"
        echo "  export RUNNER_TOKEN=\"${TOKEN}\""
        echo "  make gitlab-runner-update-token"
        exit 0
    fi
fi

echo "⚠ Не удалось получить токен через API"
echo ""
echo "Получите токен вручную из GitLab UI:"
echo "1. Откройте: ${GITLAB_URL}/${PROJECT_PATH}/-/settings/ci_cd"
echo "2. Раздел: Runners"
echo "3. Скопируйте Registration token"
echo ""
echo "Или используйте групповой/instance runner token:"
echo "  Settings → CI/CD → Runners → Expand 'Runners' → Registration token"

