#!/bin/bash
# Force GitLab Runner to re-register by clearing saved state
# Usage: ./ops/scripts/force-runner-reregister.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

echo -e "${GREEN}=== Принудительная перерегистрация GitLab Runner ===${NC}"
echo ""

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
echo -e "${GREEN}✓ Using KUBECONFIG: ${KUBECONFIG_FILE}${NC}"
echo ""

# Step 1: Get pods
echo -e "${YELLOW}=== ШАГ 1: Поиск pods GitLab Runner ===${NC}"
PODS=$(kubectl get pods -n gitlab-runner -l app=gitlab-runner -o jsonpath='{.items[*].metadata.name}' 2>/dev/null || echo "")

if [ -z "${PODS}" ]; then
    echo -e "${RED}Error: No GitLab Runner pods found${NC}"
    exit 1
fi

echo "Найдено pods: ${PODS}"
echo ""

# Step 2: Clear runner state in pods (if possible)
echo -e "${YELLOW}=== ШАГ 2: Очистка сохраненной конфигурации ===${NC}"
for POD in ${PODS}; do
    echo "Обработка pod: ${POD}"
    
    # Try to clear runner state files (may fail if ConfigMap is read-only)
    kubectl exec -n gitlab-runner "${POD}" -- sh -c "
        rm -f /etc/gitlab-runner/.runner_system_id 2>/dev/null || true
        rm -f /etc/gitlab-runner/.runner_* 2>/dev/null || true
        echo 'State files cleared (if writable)'
    " 2>&1 || echo "Не удалось очистить state files (ConfigMap read-only - это нормально)"
done
echo ""

# Step 3: Delete pods to force re-registration
echo -e "${YELLOW}=== ШАГ 3: Удаление pods для перерегистрации ===${NC}"
echo "Удаление pods..."
kubectl delete pods -n gitlab-runner -l app=gitlab-runner

echo "Ожидание пересоздания pods..."
sleep 10

# Step 4: Wait for pods to be ready
echo -e "${YELLOW}=== ШАГ 4: Ожидание готовности pods ===${NC}"
kubectl wait --for=condition=Ready pod -l app=gitlab-runner -n gitlab-runner --timeout=120s || {
    echo -e "${YELLOW}⚠ Pods могут еще запускаться${NC}"
}

echo ""
echo -e "${GREEN}=== Статус pods ===${NC}"
kubectl get pods -n gitlab-runner
echo ""

# Step 5: Check logs
echo -e "${YELLOW}=== ШАГ 5: Проверка логов ===${NC}"
sleep 5
kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=30 2>&1 | tail -30
echo ""

echo -e "${GREEN}=== ИНСТРУКЦИИ ===${NC}"
echo ""
echo "⚠️  ВАЖНО: Если runner все еще получает 403 Forbidden:"
echo ""
echo "1. Удалите старый runner из GitLab UI:"
echo "   https://git.telex.global/npk/ois-cfa/-/settings/ci_cd"
echo "   → Runners → Удалить runner с ID HYErDk_6w"
echo ""
echo "2. Перезапустите pods еще раз:"
echo "   kubectl delete pods -n gitlab-runner -l app=gitlab-runner"
echo ""
echo "3. Проверьте логи:"
echo "   kubectl logs -n gitlab-runner -l app=gitlab-runner --tail=50"
echo ""

echo -e "${GREEN}✓ Скрипт выполнен${NC}"

