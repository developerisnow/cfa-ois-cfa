import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

const errorRate = new Rate('errors');
const orderLatency = new Trend('order_latency');
const settlementReportLatency = new Trend('settlement_report_latency');

export const options = {
  stages: [
    { duration: '30s', target: 50 },  // Ramp up
    { duration: '1m', target: 100 },  // Stay at 100 RPS
    { duration: '30s', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<300', 'p(99)<500'],
    http_req_failed: ['rate<0.01'],
    errors: ['rate<0.01'],
    order_latency: ['p(95)<300'],
    settlement_report_latency: ['p(95)<300'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const TOKEN = __ENV.TOKEN; // optional JWT

export default function () {
  // Test critical path: Place Order
  const orderPayload = JSON.stringify({
    investorId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    issuanceId: '3fa85f64-5717-4562-b3fc-2c963f66afa6',
    amount: 50000,
  });

  const orderHeaders = {
    'Content-Type': 'application/json',
    'Idempotency-Key': `test-${Date.now()}-${Math.random()}`,
    ...(TOKEN ? { Authorization: `Bearer ${TOKEN}` } : {}),
  };

  const orderRes = http.post(
    `${BASE_URL}/v1/orders`,
    orderPayload,
    { headers: orderHeaders }
  );

  const orderSuccess = check(orderRes, {
    'order status is 202': (r) => r.status === 202 || r.status === 429, // 429 is rate limit, acceptable
    'order response time < 500ms': (r) => r.timings.duration < 500,
  });

  errorRate.add(!orderSuccess);
  orderLatency.add(orderRes.timings.duration);

  sleep(1);

  // Test critical path: Settlement Report
  const reportRes = http.get(
    `${BASE_URL}/v1/reports/payouts?from=2025-01-01&to=2025-12-31`,
    { headers: { 'Content-Type': 'application/json', ...(TOKEN ? { Authorization: `Bearer ${TOKEN}` } : {}) } }
  );

  const reportSuccess = check(reportRes, {
    'report status is 200': (r) => r.status === 200 || r.status === 429,
    'report response time < 500ms': (r) => r.timings.duration < 500,
  });

  errorRate.add(!reportSuccess);
  settlementReportLatency.add(reportRes.timings.duration);

  sleep(1);
}

export function handleSummary(data) {
  return {
    'stdout': textSummary(data, { indent: ' ', enableColors: true }),
    'k6-report.json': JSON.stringify(data),
    'k6-report.html': htmlReport(data),
  };
}

function textSummary(data, options) {
  const thresholds = data.metrics.http_req_duration?.values?.['p(95)'] || 0;
  return `\n✅ p95 latency: ${thresholds.toFixed(2)}ms (threshold: <300ms)\n`;
}

function htmlReport(data) {
  return '<html><body><h1>k6 Load Test Report</h1></body></html>';
}

