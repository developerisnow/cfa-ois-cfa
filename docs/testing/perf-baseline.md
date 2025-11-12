# Performance Baseline (OIS Backend)

Goal: establish SLOs and reproducible load tests for critical API paths.

SLO (initial):
- GET endpoints (wallet, reports): p95 < 300 ms
- POST /v1/orders: p95 < 800 ms at target steady RPS (to be finalized after first run)

Tools:
- k6
- Prometheus + Grafana dashboard: audit/09_Artifacts/observability/grafana-dashboards/service-overview.json
- Alerts: audit/09_Artifacts/observability/prometheus-rules.yaml

k6 Scenarios:
- Critical paths: tests/k6/gateway-critical-paths.js
- Payouts report: tests/k6/payouts-report.js

Auth:
- Scripts accept optional env TOKEN to set Authorization header.

Examples:

```bash
# Gateway base URL and token
export BASE_URL=http://localhost:5000
export TOKEN=eyJhbGci...

# Orders scenario (includes POST /v1/orders)
make k6-orders BASE_URL=$BASE_URL TOKEN=$TOKEN

# Reports (GET /v1/reports/payouts)
make k6-reports BASE_URL=$BASE_URL TOKEN=$TOKEN
```

Readouts:
- k6 output and generated k6-report.json/html (if enabled in scripts)
- Prometheus: latency histogram `request_duration_ms_bucket`, error counter `request_errors_total`
- Grafana: import dashboard JSON and check p95, RPS, error rates during load

Indexing/Caching:
- DB indices already applied on IdemKey, status, investorId, issuanceId for Registry; wallets (owner) and holdings (investor,issuance) covered
- If GET /v1/wallets is a hotspot, consider short-lived memory cache keyed by investorId (e.g., 1–5s TTL) to shield bursts
- Ensure Postgres connection pool max matches expected concurrency

Next Steps:
- Stabilize RPS/N with live env; adjust thresholds and alert rules as needed
- Add Testcontainers-based perf smoke job in CI targeting ephemeral env

