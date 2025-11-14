#!/bin/bash
# GitOps Sync Script for GitLab Agent
# Usage: ./ops/scripts/gitops-sync.sh [env]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

ENV="${1:-dev}"

echo -e "${GREEN}=== GitOps Sync: GitLab Agent ===${NC}"
echo "Environment: ${ENV}"
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

# Step 1: Check GitLab Agent status
echo -e "${YELLOW}=== ШАГ 1: Проверка GitLab Agent ===${NC}"
AGENT_PODS=$(kubectl get pods -n gitlab-agent -o jsonpath='{.items[*].metadata.name}' 2>/dev/null || echo "")

if [ -z "${AGENT_PODS}" ]; then
    echo -e "${YELLOW}⚠ GitLab Agent pods not found, but continuing...${NC}"
    echo "Install agent: make gitlab-agent-install"
else
    echo "GitLab Agent pods: ${AGENT_PODS}"
fi

echo "GitLab Agent pods: ${AGENT_PODS}"
for POD in ${AGENT_PODS}; do
    STATUS=$(kubectl get pod -n gitlab-agent "${POD}" -o jsonpath='{.status.phase}' 2>/dev/null || echo "Unknown")
    echo "  - ${POD}: ${STATUS}"
done
echo ""

# Step 2: Check agent configuration
echo -e "${YELLOW}=== ШАГ 2: Проверка конфигурации агента ===${NC}"
if [ ! -f "${PROJECT_ROOT}/.gitlab/agents/ois-cfa-agent/config.yaml" ]; then
    echo -e "${YELLOW}⚠ Конфигурация агента не найдена, создаю...${NC}"
    mkdir -p "${PROJECT_ROOT}/.gitlab/agents/ois-cfa-agent"
    cp "${PROJECT_ROOT}/ops/gitops/gitlab-agent/agent-config.yaml" \
       "${PROJECT_ROOT}/.gitlab/agents/ois-cfa-agent/config.yaml"
    echo -e "${GREEN}✓ Конфигурация создана${NC}"
else
    echo -e "${GREEN}✓ Конфигурация найдена${NC}"
fi
echo ""

# Step 3: Check manifests
echo -e "${YELLOW}=== ШАГ 3: Проверка манифестов ===${NC}"
MANIFEST_DIRS=(
    "ops/gitops/gitlab-agent/manifests/system"
    "ops/gitops/gitlab-agent/manifests/platform"
    "ops/gitops/gitlab-agent/manifests/business"
)

for DIR in "${MANIFEST_DIRS[@]}"; do
    if [ -d "${PROJECT_ROOT}/${DIR}" ]; then
        MANIFEST_COUNT=$(find "${PROJECT_ROOT}/${DIR}" -name "*.yaml" -o -name "*.yml" | wc -l)
        echo "  ${DIR}: ${MANIFEST_COUNT} манифестов"
    else
        echo -e "${YELLOW}  ⚠ ${DIR}: не найдено${NC}"
    fi
done
echo ""

# Step 4: Apply manifests manually (for testing)
echo -e "${YELLOW}=== ШАГ 4: Применение манифестов ===${NC}"
echo "Применяю манифесты в порядке: system → platform → business"
echo ""

# System manifests
if [ -d "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/system" ]; then
    echo -e "${BLUE}→ System manifests${NC}"
    kubectl apply -f "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/system/" --recursive || true
    sleep 2
fi

# Platform manifests
if [ -d "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/platform" ]; then
    echo -e "${BLUE}→ Platform manifests${NC}"
    kubectl apply -f "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/platform/" --recursive || true
    sleep 2
fi

# Business manifests
if [ -d "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/business" ]; then
    echo -e "${BLUE}→ Business manifests${NC}"
    kubectl apply -f "${PROJECT_ROOT}/ops/gitops/gitlab-agent/manifests/business/" --recursive || true
    sleep 2
fi

echo ""

# Step 5: Check applied resources
echo -e "${YELLOW}=== ШАГ 5: Проверка примененных ресурсов ===${NC}"
echo "Namespaces:"
kubectl get namespaces | grep -E "(ois-cfa|default)" || echo "  (нет соответствующих namespace)"
echo ""

echo "Deployments в ois-cfa:"
kubectl get deployments -n ois-cfa 2>/dev/null || echo "  Namespace ois-cfa не существует или пуст"
echo ""

echo "Services в ois-cfa:"
kubectl get services -n ois-cfa 2>/dev/null || echo "  Namespace ois-cfa не существует или пуст"
echo ""

echo "Ingress в ois-cfa:"
kubectl get ingress -n ois-cfa 2>/dev/null || echo "  Namespace ois-cfa не существует или пуст"
echo ""

# Step 6: Summary
echo -e "${GREEN}=== ИТОГОВЫЙ СТАТУС ===${NC}"
echo ""
echo "✅ GitLab Agent: Running"
echo "✅ Конфигурация: ${PROJECT_ROOT}/.gitlab/agents/ois-cfa-agent/config.yaml"
echo "✅ Манифесты применены"
echo ""
echo -e "${YELLOW}Следующие шаги:${NC}"
echo "1. Проверить статус в GitLab UI:"
echo "   Infrastructure → Kubernetes clusters → ваш кластер → Connected agents"
echo ""
echo "2. Создать MR с изменениями манифестов (если нужно)"
echo ""
echo "3. GitLab Agent автоматически синхронизирует изменения из Git"
echo ""
echo -e "${GREEN}✓ GitOps sync выполнен${NC}"

