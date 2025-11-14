#!/bin/bash
# Script to create Helm chart from api-gateway template
# Usage: ./ops/scripts/create-helm-chart.sh <chart-name>

set -euo pipefail

CHART_NAME="${1:-}"
if [ -z "$CHART_NAME" ]; then
  echo "Usage: $0 <chart-name>"
  echo "Example: $0 identity"
  exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
TEMPLATE_CHART="api-gateway"
CHART_DIR="${PROJECT_ROOT}/ops/infra/helm/${CHART_NAME}"

if [ -d "$CHART_DIR" ]; then
  echo "Error: Chart $CHART_NAME already exists at $CHART_DIR"
  exit 1
fi

echo "Creating Helm chart: $CHART_NAME"
echo "Template: $TEMPLATE_CHART"
echo "Target: $CHART_DIR"

# Create directory structure
mkdir -p "${CHART_DIR}/templates"

# Copy files from template
cp "${PROJECT_ROOT}/ops/infra/helm/${TEMPLATE_CHART}/Chart.yaml" "${CHART_DIR}/"
cp "${PROJECT_ROOT}/ops/infra/helm/${TEMPLATE_CHART}/values.yaml" "${CHART_DIR}/"
cp "${PROJECT_ROOT}/ops/infra/helm/${TEMPLATE_CHART}/values-"*.yaml "${CHART_DIR}/" 2>/dev/null || true
cp "${PROJECT_ROOT}/ops/infra/helm/${TEMPLATE_CHART}/templates/"* "${CHART_DIR}/templates/"

# Replace chart name in all files
find "${CHART_DIR}" -type f \( -name "*.yaml" -o -name "*.tpl" \) -exec sed -i "s/${TEMPLATE_CHART}/${CHART_NAME}/g" {} \;

# Update Chart.yaml name
sed -i "s/name: ${TEMPLATE_CHART}/name: ${CHART_NAME}/" "${CHART_DIR}/Chart.yaml"

echo "✓ Created Helm chart: $CHART_DIR"
echo ""
echo "Next steps:"
echo "1. Review and update values.yaml with service-specific settings"
echo "2. Update values-*.yaml for environment-specific configurations"
echo "3. Test with: helm template $CHART_NAME $CHART_DIR"

