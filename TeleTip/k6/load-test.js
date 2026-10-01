import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const errorRate = new Rate('errors');

export const options = {
  stages: [
    { duration: '30s', target: 10 },   // Ramp-up
    { duration: '1m',  target: 50 },   // Steady state
    { duration: '30s', target: 100 },  // Peak load
    { duration: '30s', target: 0 },    // Ramp-down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'],  // 95% istekler 500ms altında
    errors: ['rate<0.01'],             // %1'den az hata
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';

export default function () {
  // GET all products
  const listRes = http.get(`${BASE_URL}/api/products`, {
    headers: { 'X-Tenant-Id': 'tenant-1' },
  });

  check(listRes, {
    'GET /api/products status 200': (r) => r.status === 200,
    'GET /api/products duration < 500ms': (r) => r.timings.duration < 500,
  }) || errorRate.add(1);

  sleep(0.5);

  // POST create product
  const payload = JSON.stringify({
    name: `Product-${Date.now()}`,
    description: 'K6 test product',
    price: 99.99,
    stock: 100,
  });

  const createRes = http.post(`${BASE_URL}/api/products`, payload, {
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': 'tenant-1',
    },
  });

  check(createRes, {
    'POST /api/products status 201': (r) => r.status === 201,
  }) || errorRate.add(1);

  sleep(1);
}
