#!/bin/bash
# Dump all Kubernetes events sorted by timestamp
# Usage: ./events-dump.sh [output-file]

set -euo pipefail

OUTPUT_FILE="${1:-/tmp/events.txt}"
ARTIFACTS_DIR="${ARTIFACTS_DIR:-/tmp/artifacts}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)

echo "=== Dumping Kubernetes events ==="
echo "Output file: ${OUTPUT_FILE}"
echo "Timestamp: ${TIMESTAMP}"

mkdir -p "$(dirname "${OUTPUT_FILE}")"
mkdir -p "${ARTIFACTS_DIR}"

# Get all events sorted by timestamp
echo "Collecting events from all namespaces..."
kubectl get events -A --sort-by='.lastTimestamp' > "${OUTPUT_FILE}" 2>&1 || {
  echo "Error: Failed to collect events"
  exit 1
}

# Count events by type
echo ""
echo "=== Event Summary ==="
echo "Total events: $(wc -l < "${OUTPUT_FILE}" | tr -d ' ')"
echo "Warning events: $(grep -c "Warning" "${OUTPUT_FILE}" || echo "0")"
echo "Normal events: $(grep -c "Normal" "${OUTPUT_FILE}" || echo "0")"

# Get recent warnings (last 100)
echo ""
echo "=== Recent Warnings (last 100) ==="
grep "Warning" "${OUTPUT_FILE}" | tail -100 > "${ARTIFACTS_DIR}/events-warnings-${TIMESTAMP}.txt" || true

# Get events by namespace
echo ""
echo "=== Events by Namespace ==="
for ns in $(kubectl get namespaces -o jsonpath='{.items[*].metadata.name}'); do
  count=$(kubectl get events -n "${ns}" --no-headers 2>/dev/null | wc -l || echo "0")
  if [ "${count}" -gt 0 ]; then
    echo "  ${ns}: ${count} events"
  fi
done

# Copy to artifacts
cp "${OUTPUT_FILE}" "${ARTIFACTS_DIR}/events-${TIMESTAMP}.txt"

echo ""
echo "Events dumped successfully"
echo "Output: ${OUTPUT_FILE}"
echo "Artifacts: ${ARTIFACTS_DIR}/events-${TIMESTAMP}.txt"

# Output for GitLab CI
if [ -n "${CI_JOB_ID:-}" ]; then
  echo "CI_JOB_ID=${CI_JOB_ID}" >> "${ARTIFACTS_DIR}/job-info.txt"
  echo "EVENTS_FILE=${ARTIFACTS_DIR}/events-${TIMESTAMP}.txt" >> "${ARTIFACTS_DIR}/job-info.txt"
fi

