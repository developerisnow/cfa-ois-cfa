#!/bin/bash
# Kubernetes Cluster Health Check
# Generates HTML report with cluster status, checks, and action items

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
OUTPUT_DIR="${OUTPUT_DIR:-/tmp/k8s-healthcheck}"
ARTIFACTS_DIR="${ARTIFACTS_DIR:-/tmp/artifacts}"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
REPORT_FILE="${OUTPUT_DIR}/healthcheck-report-${TIMESTAMP}.html"
JSON_FILE="${OUTPUT_DIR}/healthcheck-${TIMESTAMP}.json"

mkdir -p "${OUTPUT_DIR}"
mkdir -p "${ARTIFACTS_DIR}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Initialize results
declare -A CHECKS
declare -a ACTION_ITEMS
declare -a WARNINGS
declare -a ERRORS

# Helper functions
check_pass() {
    local name="$1"
    local message="$2"
    CHECKS["${name}"]="PASS"
    echo -e "${GREEN}✓${NC} ${name}: ${message}"
}

check_fail() {
    local name="$1"
    local message="$2"
    local fix_cmd="${3:-}"
    CHECKS["${name}"]="FAIL"
    ERRORS+=("${name}: ${message}")
    if [ -n "${fix_cmd}" ]; then
        ACTION_ITEMS+=("${name}|${message}|${fix_cmd}")
    else
        ACTION_ITEMS+=("${name}|${message}|N/A")
    fi
    echo -e "${RED}✗${NC} ${name}: ${message}"
}

check_warn() {
    local name="$1"
    local message="$2"
    CHECKS["${name}"]="WARN"
    WARNINGS+=("${name}: ${message}")
    echo -e "${YELLOW}⚠${NC} ${name}: ${message}"
}

# Check if kubectl is available
if ! command -v kubectl &> /dev/null; then
    echo "Error: kubectl not found. Please install kubectl."
    exit 1
fi

# Check if cluster is accessible
if ! kubectl cluster-info &> /dev/null; then
    check_fail "cluster-access" "Cannot access Kubernetes cluster" "kubectl cluster-info"
    exit 1
fi

echo "=== Kubernetes Cluster Health Check ==="
echo "Timestamp: ${TIMESTAMP}"
echo ""

# 1. Check Nodes
echo "=== Checking Nodes ==="
NODES=$(kubectl get nodes --no-headers 2>/dev/null | wc -l || echo "0")
if [ "${NODES}" -eq 0 ]; then
    check_fail "nodes-count" "No nodes found in cluster" "kubectl get nodes"
else
    check_pass "nodes-count" "${NODES} node(s) found"
fi

# Check node status
READY_NODES=$(kubectl get nodes --no-headers 2>/dev/null | grep -c " Ready " || echo "0")
NOT_READY_NODES=$(kubectl get nodes --no-headers 2>/dev/null | grep -v " Ready " | wc -l || echo "0")

if [ "${NOT_READY_NODES}" -gt 0 ]; then
    check_fail "nodes-ready" "${NOT_READY_NODES} node(s) not ready" "kubectl get nodes; kubectl describe node <node-name>"
else
    check_pass "nodes-ready" "All ${READY_NODES} node(s) ready"
fi

# Get node versions
NODE_VERSIONS=$(kubectl get nodes -o jsonpath='{range .items[*]}{.metadata.name}: {.status.nodeInfo.kubeletVersion}{"\n"}{end}' 2>/dev/null || echo "")
if [ -n "${NODE_VERSIONS}" ]; then
    echo "Node versions:"
    echo "${NODE_VERSIONS}" | while IFS= read -r line; do
        echo "  - ${line}"
    done
fi

# Check node resources
echo ""
echo "Node resources:"
kubectl top nodes 2>/dev/null || check_warn "node-metrics" "Metrics server not available (kubectl top nodes)"

# 2. Check Ingress Controller
echo ""
echo "=== Checking Ingress Controller ==="
INGRESS_NS="ingress-nginx"
if kubectl get namespace "${INGRESS_NS}" &>/dev/null; then
    INGRESS_PODS=$(kubectl get pods -n "${INGRESS_NS}" --no-headers 2>/dev/null | grep -c " Running " || echo "0")
    if [ "${INGRESS_PODS}" -gt 0 ]; then
        check_pass "ingress-controller" "Ingress controller running (${INGRESS_PODS} pod(s))"
    else
        check_fail "ingress-controller" "Ingress controller pods not running" "kubectl get pods -n ${INGRESS_NS}; kubectl describe pod -n ${INGRESS_NS}"
    fi
else
    check_warn "ingress-controller" "Ingress namespace not found (${INGRESS_NS})"
fi

# Check ingress service
INGRESS_SVC=$(kubectl get svc -n "${INGRESS_NS}" -l app.kubernetes.io/component=controller 2>/dev/null | grep -v "^NAME" | head -1 | awk '{print $1}' || echo "")
if [ -n "${INGRESS_SVC}" ]; then
    INGRESS_IP=$(kubectl get svc -n "${INGRESS_NS}" "${INGRESS_SVC}" -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || echo "")
    if [ -n "${INGRESS_IP}" ]; then
        check_pass "ingress-lb" "Ingress LoadBalancer IP: ${INGRESS_IP}"
    else
        check_warn "ingress-lb" "Ingress LoadBalancer IP not assigned"
    fi
fi

# 3. Check Cert-Manager
echo ""
echo "=== Checking Cert-Manager ==="
CERT_MANAGER_NS="cert-manager"
if kubectl get namespace "${CERT_MANAGER_NS}" &>/dev/null; then
    CERT_MANAGER_PODS=$(kubectl get pods -n "${CERT_MANAGER_NS}" --no-headers 2>/dev/null | grep -c " Running " || echo "0")
    if [ "${CERT_MANAGER_PODS}" -gt 0 ]; then
        check_pass "cert-manager-pods" "Cert-Manager running (${CERT_MANAGER_PODS} pod(s))"
    else
        check_fail "cert-manager-pods" "Cert-Manager pods not running" "kubectl get pods -n ${CERT_MANAGER_NS}; kubectl describe pod -n ${CERT_MANAGER_NS}"
    fi
    
    # Check certificates
    CERT_COUNT=$(kubectl get certificates -A --no-headers 2>/dev/null | wc -l || echo "0")
    if [ "${CERT_COUNT}" -gt 0 ]; then
        READY_CERTS=$(kubectl get certificates -A --no-headers 2>/dev/null | grep -c " True " || echo "0")
        NOT_READY_CERTS=$(kubectl get certificates -A --no-headers 2>/dev/null | grep -v " True " | wc -l || echo "0")
        
        if [ "${NOT_READY_CERTS}" -gt 0 ]; then
            check_fail "certificates-ready" "${NOT_READY_CERTS} certificate(s) not ready" "kubectl get certificates -A; kubectl describe certificate -n <namespace> <cert-name>"
        else
            check_pass "certificates-ready" "All ${READY_CERTS} certificate(s) ready"
        fi
    else
        check_warn "certificates" "No certificates found"
    fi
    
    # Check certificate issuers
    ISSUER_COUNT=$(kubectl get clusterissuers,issuers -A --no-headers 2>/dev/null | wc -l || echo "0")
    if [ "${ISSUER_COUNT}" -gt 0 ]; then
        READY_ISSUERS=$(kubectl get clusterissuers,issuers -A --no-headers 2>/dev/null | grep -c " True " || echo "0")
        check_pass "issuers" "${READY_ISSUERS} issuer(s) ready"
    else
        check_warn "issuers" "No certificate issuers found"
    fi
else
    check_warn "cert-manager" "Cert-Manager namespace not found (${CERT_MANAGER_NS})"
fi

# 4. Check DNS
echo ""
echo "=== Checking DNS ==="
# Check CoreDNS
COREDNS_NS="kube-system"
COREDNS_PODS=$(kubectl get pods -n "${COREDNS_NS}" -l k8s-app=kube-dns --no-headers 2>/dev/null | grep -c " Running " || echo "0")
if [ "${COREDNS_PODS}" -gt 0 ]; then
    check_pass "coredns" "CoreDNS running (${COREDNS_PODS} pod(s))"
else
    check_fail "coredns" "CoreDNS pods not running" "kubectl get pods -n ${COREDNS_NS} -l k8s-app=kube-dns"
fi

# Test DNS resolution (if test pod can be created)
# Use timeout to prevent hanging
DNS_TEST_OUTPUT=$(timeout 15 kubectl run dns-test-${TIMESTAMP} --image=busybox:1.36 --rm -i --restart=Never -- nslookup kubernetes.default 2>&1 || echo "FAIL")
if echo "${DNS_TEST_OUTPUT}" | grep -q "kubernetes.default"; then
    check_pass "dns-resolution" "DNS resolution working"
else
    # Cleanup pod if it wasn't cleaned up automatically
    kubectl delete pod dns-test-${TIMESTAMP} --ignore-not-found=true &>/dev/null || true
    check_warn "dns-resolution" "DNS resolution test inconclusive (may require manual verification)"
fi

# 5. Check critical namespaces
echo ""
echo "=== Checking Critical Namespaces ==="
CRITICAL_NS=("kube-system" "kube-public" "kube-node-lease")
for ns in "${CRITICAL_NS[@]}"; do
    if kubectl get namespace "${ns}" &>/dev/null; then
        check_pass "namespace-${ns}" "Namespace ${ns} exists"
    else
        check_fail "namespace-${ns}" "Namespace ${ns} not found" "kubectl create namespace ${ns}"
    fi
done

# 6. Check system pods
echo ""
echo "=== Checking System Pods ==="
SYSTEM_PODS_NOT_READY=$(kubectl get pods -n kube-system --no-headers 2>/dev/null | grep -v " Running " | grep -v " Completed " | wc -l || echo "0")
if [ "${SYSTEM_PODS_NOT_READY}" -gt 0 ]; then
    check_fail "system-pods" "${SYSTEM_PODS_NOT_READY} system pod(s) not ready" "kubectl get pods -n kube-system; kubectl describe pod -n kube-system <pod-name>"
else
    check_pass "system-pods" "All system pods ready"
fi

# 7. Check API server
echo ""
echo "=== Checking API Server ==="
API_SERVER=$(kubectl cluster-info | grep "Kubernetes control plane" | awk '{print $NF}' || echo "")
if [ -n "${API_SERVER}" ]; then
    if curl -k -s -o /dev/null -w "%{http_code}" "${API_SERVER}/healthz" | grep -q "200"; then
        check_pass "api-server" "API server healthy"
    else
        check_fail "api-server" "API server health check failed" "kubectl cluster-info"
    fi
else
    check_warn "api-server" "Cannot determine API server URL"
fi

# Generate JSON report
cat > "${JSON_FILE}" <<EOF
{
  "timestamp": "${TIMESTAMP}",
  "cluster": "$(kubectl config current-context 2>/dev/null || echo 'unknown')",
  "checks": {
$(for key in "${!CHECKS[@]}"; do
    echo "    \"${key}\": \"${CHECKS[$key]}\","
done | sed '$ s/,$//')
  },
  "summary": {
    "total": ${#CHECKS[@]},
    "passed": $(echo "${CHECKS[@]}" | grep -o "PASS" | wc -l),
    "failed": $(echo "${CHECKS[@]}" | grep -o "FAIL" | wc -l),
    "warnings": $(echo "${CHECKS[@]}" | grep -o "WARN" | wc -l)
  },
  "errors": [
$(for error in "${ERRORS[@]}"; do
    echo "    \"${error}\","
done | sed '$ s/,$//')
  ],
  "warnings": [
$(for warning in "${WARNINGS[@]}"; do
    echo "    \"${warning}\","
done | sed '$ s/,$//')
  ],
  "action_items": [
$(for item in "${ACTION_ITEMS[@]}"; do
    IFS='|' read -r name message cmd <<< "${item}"
    echo "    {\"check\": \"${name}\", \"message\": \"${message}\", \"command\": \"${cmd}\"},"
done | sed '$ s/,$//')
  ]
}
EOF

# Generate HTML report
cat > "${REPORT_FILE}" <<EOF
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Kubernetes Cluster Health Check Report</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #333;
            border-bottom: 3px solid #4CAF50;
            padding-bottom: 10px;
        }
        h2 {
            color: #555;
            margin-top: 30px;
            border-bottom: 2px solid #e0e0e0;
            padding-bottom: 5px;
        }
        .summary {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 20px;
            margin: 20px 0;
        }
        .summary-card {
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }
        .summary-card.total { background-color: #e3f2fd; }
        .summary-card.passed { background-color: #e8f5e9; }
        .summary-card.failed { background-color: #ffebee; }
        .summary-card.warnings { background-color: #fff3e0; }
        .summary-card h3 {
            margin: 0;
            font-size: 2em;
            color: #333;
        }
        .summary-card p {
            margin: 5px 0 0 0;
            color: #666;
        }
        .check-item {
            padding: 10px;
            margin: 5px 0;
            border-radius: 4px;
            display: flex;
            align-items: center;
        }
        .check-item.pass {
            background-color: #e8f5e9;
            border-left: 4px solid #4CAF50;
        }
        .check-item.fail {
            background-color: #ffebee;
            border-left: 4px solid #f44336;
        }
        .check-item.warn {
            background-color: #fff3e0;
            border-left: 4px solid #ff9800;
        }
        .status-icon {
            font-size: 1.5em;
            margin-right: 10px;
        }
        .action-items {
            background-color: #fff3cd;
            border: 1px solid #ffc107;
            border-radius: 4px;
            padding: 15px;
            margin: 20px 0;
        }
        .action-items h3 {
            margin-top: 0;
            color: #856404;
        }
        .action-item {
            background: white;
            padding: 10px;
            margin: 10px 0;
            border-radius: 4px;
            border-left: 3px solid #ff9800;
        }
        .action-item strong {
            color: #f44336;
        }
        .command {
            background-color: #f5f5f5;
            padding: 8px;
            border-radius: 4px;
            font-family: 'Courier New', monospace;
            font-size: 0.9em;
            margin-top: 5px;
            word-break: break-all;
        }
        .timestamp {
            color: #666;
            font-size: 0.9em;
        }
        table {
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }
        th, td {
            padding: 12px;
            text-align: left;
            border-bottom: 1px solid #ddd;
        }
        th {
            background-color: #4CAF50;
            color: white;
        }
        tr:hover {
            background-color: #f5f5f5;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>Kubernetes Cluster Health Check Report</h1>
        <p class="timestamp">Generated: $(date -u +"%Y-%m-%d %H:%M:%S UTC")</p>
        <p class="timestamp">Cluster: $(kubectl config current-context 2>/dev/null || echo 'unknown')</p>
        
        <div class="summary">
            <div class="summary-card total">
                <h3>${#CHECKS[@]}</h3>
                <p>Total Checks</p>
            </div>
            <div class="summary-card passed">
                <h3>$(echo "${CHECKS[@]}" | grep -o "PASS" | wc -l)</h3>
                <p>Passed</p>
            </div>
            <div class="summary-card failed">
                <h3>$(echo "${CHECKS[@]}" | grep -o "FAIL" | wc -l)</h3>
                <p>Failed</p>
            </div>
            <div class="summary-card warnings">
                <h3>$(echo "${CHECKS[@]}" | grep -o "WARN" | wc -l)</h3>
                <p>Warnings</p>
            </div>
        </div>
        
        <h2>Check Results</h2>
        <table>
            <thead>
                <tr>
                    <th>Check</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
$(for key in $(printf '%s\n' "${!CHECKS[@]}" | sort); do
    status="${CHECKS[$key]}"
    case "${status}" in
        PASS)
            echo "                <tr><td>${key}</td><td><span class=\"status-icon\">✓</span> Pass</td></tr>"
            ;;
        FAIL)
            echo "                <tr><td>${key}</td><td><span class=\"status-icon\">✗</span> Fail</td></tr>"
            ;;
        WARN)
            echo "                <tr><td>${key}</td><td><span class=\"status-icon\">⚠</span> Warning</td></tr>"
            ;;
    esac
done)
            </tbody>
        </table>
        
$(if [ ${#ACTION_ITEMS[@]} -gt 0 ]; then
cat <<ACTION_EOF
        <div class="action-items">
            <h3>Action Items</h3>
            <p>The following issues were detected and require attention:</p>
$(for item in "${ACTION_ITEMS[@]}"; do
    IFS='|' read -r name message cmd <<< "${item}"
    echo "            <div class=\"action-item\">"
    echo "                <strong>${name}</strong>: ${message}"
    if [ "${cmd}" != "N/A" ]; then
        echo "                <div class=\"command\">${cmd}</div>"
    fi
    echo "            </div>"
done)
        </div>
ACTION_EOF
fi)

$(if [ ${#WARNINGS[@]} -gt 0 ]; then
cat <<WARN_EOF
        <h2>Warnings</h2>
        <ul>
$(for warning in "${WARNINGS[@]}"; do
    echo "            <li>${warning}</li>"
done)
        </ul>
WARN_EOF
fi)

        <h2>Cluster Information</h2>
        <pre style="background-color: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto;">
$(kubectl cluster-info 2>/dev/null || echo "Cluster info not available")
        </pre>
        
        <h2>Node Information</h2>
        <pre style="background-color: #f5f5f5; padding: 15px; border-radius: 4px; overflow-x: auto;">
$(kubectl get nodes -o wide 2>/dev/null || echo "Node info not available")
        </pre>
    </div>
</body>
</html>
EOF

# Copy reports to artifacts
cp "${REPORT_FILE}" "${ARTIFACTS_DIR}/" || true
cp "${JSON_FILE}" "${ARTIFACTS_DIR}/" || true

echo ""
echo "=== Health Check Complete ==="
echo "HTML Report: ${REPORT_FILE}"
echo "JSON Report: ${JSON_FILE}"
echo "Artifacts: ${ARTIFACTS_DIR}/"

# Exit with error code if there are failures
if [ ${#ERRORS[@]} -gt 0 ]; then
    echo ""
    echo "Health check completed with ${#ERRORS[@]} error(s). See report for details."
    exit 1
else
    echo ""
    echo "All health checks passed!"
    exit 0
fi

