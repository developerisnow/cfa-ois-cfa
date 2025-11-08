#!/usr/bin/env bash
# Patch GitLab Runner to fix 403 Forbidden
# Usage: RUNNER_TOKEN=glrt-... ./ops/ci/patch_runner_fix_403.sh

set -euo pipefail

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Defaults
NS="${NS:-gitlab-runner}"
RUNNER_TOKEN="${RUNNER_TOKEN:-}"

echo -e "${GREEN}=== GitLab Runner 403 Fix ===${NC}"
echo ""

# Check token
if [ -z "${RUNNER_TOKEN}" ]; then
    echo -e "${RED}Error: RUNNER_TOKEN not set${NC}"
    echo ""
    echo "Токен должен быть Runner Authentication Token (glrt-...), НЕ registration token!"
    echo ""
    echo "Как получить:"
    echo "1. Открыть: https://git.telex.global/npk/ois-cfa/-/settings/ci_cd"
    echo "2. Раздел: Runners"
    echo "3. Найти зарегистрированный runner"
    echo "4. Скопировать Authentication Token (glrt-...)"
    echo ""
    echo "Или если runner не зарегистрирован:"
    echo "1. Использовать Registration Token (GR...) для первой регистрации"
    echo "2. После регистрации runner получит Authentication Token (glrt-...)"
    echo ""
    exit 1
fi

# Check token format
if [[ ! "${RUNNER_TOKEN}" =~ ^glrt- ]]; then
    if [[ "${RUNNER_TOKEN}" =~ ^GR[0-9]+ ]]; then
        echo -e "${YELLOW}⚠ ВНИМАНИЕ: Это Registration Token (GR...), не Authentication Token${NC}"
        echo "Registration Token используется только для первой регистрации."
        echo "После регистрации runner получит Authentication Token (glrt-...)."
        echo ""
        echo "Продолжить с Registration Token? (y/N)"
        read -r CONFIRM
        if [ "${CONFIRM}" != "y" ] && [ "${CONFIRM}" != "Y" ]; then
            echo "Отменено"
            exit 1
        fi
    else
        echo -e "${RED}Error: Неверный формат токена${NC}"
        echo "Ожидается: glrt-... (Authentication Token) или GR... (Registration Token)"
        exit 1
    fi
fi

# Setup kubeconfig
KUBECONFIG_FILE="${KUBECONFIG:-}"
if [ -z "${KUBECONFIG_FILE}" ] && [ -f "${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml" ]; then
    KUBECONFIG_FILE="${PROJECT_ROOT}/ops/infra/timeweb/kubeconfig.yaml"
    export KUBECONFIG="${KUBECONFIG_FILE}"
fi

if [ -z "${KUBECONFIG_FILE}" ] || [ ! -f "${KUBECONFIG_FILE}" ]; then
    echo -e "${RED}Error: KUBECONFIG not found${NC}"
    exit 1
fi

export KUBECONFIG="${KUBECONFIG_FILE}"

# Mask token for display
TOKEN_PREFIX=$(echo "${RUNNER_TOKEN}" | cut -c1-12)
TOKEN_MASKED="${TOKEN_PREFIX}***MASKED***"

echo -e "${GREEN}✓ Using KUBECONFIG: ${KUBECONFIG_FILE}${NC}"
echo "Token: ${TOKEN_MASKED}"
echo "Namespace: ${NS}"
echo ""

# Step 1: Update ConfigMap with token
echo -e "${YELLOW}=== ШАГ 1: Обновление ConfigMap ===${NC}"
sed "s/__REPLACE_WITH_GLRT_TOKEN__/${RUNNER_TOKEN}/g" \
    "${PROJECT_ROOT}/ops/infra/k8s/gitlab-runner/configmap.yaml" | \
    kubectl apply -f -

echo -e "${GREEN}✓ ConfigMap обновлен${NC}"
echo ""

# Step 2: Apply deployment with writable volume
echo -e "${YELLOW}=== ШАГ 2: Применение исправленного Deployment ===${NC}"
kubectl apply -f "${PROJECT_ROOT}/ops/infra/k8s/gitlab-runner/deployment.yaml"

echo -e "${GREEN}✓ Deployment обновлен${NC}"
echo ""

# Step 3: Restart pods
echo -e "${YELLOW}=== ШАГ 3: Перезапуск pods ===${NC}"
kubectl rollout restart deployment/gitlab-runner -n "${NS}"

echo "Ожидание перезапуска..."
kubectl rollout status deployment/gitlab-runner -n "${NS}" --timeout=180s || {
    echo -e "${YELLOW}⚠ Rollout может еще продолжаться${NC}"
}

echo ""

# Step 4: Wait for pods
echo -e "${YELLOW}=== ШАГ 4: Ожидание готовности pods ===${NC}"
sleep 10
kubectl wait --for=condition=Ready pod -l app=gitlab-runner -n "${NS}" --timeout=120s || {
    echo -e "${YELLOW}⚠ Pods могут еще запускаться${NC}"
}

echo ""

# Step 5: Check logs
echo -e "${YELLOW}=== ШАГ 5: Проверка логов ===${NC}"
sleep 5
kubectl logs -n "${NS}" -l app=gitlab-runner --tail=30 2>&1 | tail -30

echo ""

# Step 6: Verify
echo -e "${YELLOW}=== ШАГ 6: Проверка статуса ===${NC}"
kubectl get pods -n "${NS}"

echo ""
echo -e "${GREEN}=== ИТОГ ===${NC}"
echo ""
echo "Проверьте логи на наличие ошибок 403:"
echo "  kubectl logs -n ${NS} -l app=gitlab-runner --tail=50 | grep -i '403\|forbidden'"
echo ""
echo "Если ошибок нет, runner должен работать!"
echo ""

