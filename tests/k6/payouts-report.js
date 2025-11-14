import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '1m', target: 100 },
    { duration: '30s', target: 0 },
  ],
  thresholds: {
    http_req_duration: ['p(95)<300'],
    errors: ['rate<0.01'],
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const TOKEN = __ENV.TOKEN; // optional JWT

export default function () {
  const today = new Date();
  const from = new Date(today);
  from.setDate(from.getDate() - 30);
  
  const toDate = today.toISOString().split('T')[0];
  const fromDate = from.toISOString().split('T')[0];

  const url = `${BASE_URL}/v1/reports/payouts?from=${fromDate}&to=${toDate}`;
  
  const params = {
    headers: {
      'Content-Type': 'application/json',
      ...(TOKEN ? { Authorization: `Bearer ${TOKEN}` } : {}),
    },
  };

  const res = http.get(url, params);
  
  const success = check(res, {
    'status is 200': (r) => r.status === 200,
    'response time < 300ms': (r) => r.timings.duration < 300,
    'has totalBatches field': (r) => JSON.parse(r.body).totalBatches !== undefined,
  });

  errorRate.add(!success);
  
  sleep(0.1);
}

