/**
 * k6 load test — Tabsan EduSphere performance baseline
 *
 * Target: p95 < 200 ms on core dashboard / read endpoints under 20 VUs.
 *
 * Run:
 *   k6 run tests/load/k6-baseline.js
 *   k6 run --env BASE_URL=https://staging.example.com tests/load/k6-baseline.js
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

// ── Config ────────────────────────────────────────────────────────────────────
const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const ADMIN_TOKEN = __ENV.ADMIN_TOKEN || '';  // Set via env var; never hard-code credentials

const apiDuration = new Trend('api_duration', true);
const errorRate   = new Rate('error_rate');

// ── Thresholds ────────────────────────────────────────────────────────────────
export const options = {
  scenarios: {
    smoke: {
      executor: 'constant-vus',
      vus: 1,
      duration: '30s',
      tags: { scenario: 'smoke' },
    },
    baseline: {
      executor: 'constant-vus',
      vus: 20,
      duration: '1m',
      startTime: '35s',   // runs after smoke
      tags: { scenario: 'baseline' },
    },
    spike: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '10s', target: 50 },
        { duration: '20s', target: 50 },
        { duration: '10s', target: 0 },
      ],
      startTime: '2m10s',   // runs after baseline
      tags: { scenario: 'spike' },
    },
  },
  thresholds: {
    // p95 under 200 ms across all API requests
    'http_req_duration{type:api}': ['p(95)<200'],
    // Overall error rate below 1 %
    'error_rate': ['rate<0.01'],
  },
};

// ── Helpers ───────────────────────────────────────────────────────────────────
function apiGet(path) {
  const headers = { 'Content-Type': 'application/json' };
  if (ADMIN_TOKEN) {
    headers['Authorization'] = `Bearer ${ADMIN_TOKEN}`;
  }

  const res = http.get(`${BASE_URL}/api/v1${path}`, {
    headers,
    tags: { type: 'api' },
  });

  apiDuration.add(res.timings.duration);
  errorRate.add(res.status >= 400 || res.status === 0 ? 1 : 0);
  return res;
}

// ── Virtual user scenario ─────────────────────────────────────────────────────
export default function () {
  // Health check (no auth required)
  {
    const res = http.get(`${BASE_URL}/health`, { tags: { type: 'api' } });
    check(res, { 'health: 200': (r) => r.status === 200 });
  }

  // Module list (read-heavy, cached path)
  {
    const res = apiGet('/modules');
    check(res, { 'modules: 2xx': (r) => r.status < 300 });
  }

  // Sidebar menu (called on every page load in the web app)
  {
    const res = apiGet('/sidebar-menu/my-visible');
    check(res, { 'sidebar: 2xx': (r) => r.status < 300 });
  }

  // Notifications inbox (authenticated read path)
  {
    const res = apiGet('/notifications/inbox?page=1&pageSize=10');
    check(res, { 'notifications: 2xx or 401': (r) => r.status < 300 || r.status === 401 });
  }

  // Department list
  {
    const res = apiGet('/departments');
    check(res, { 'departments: 2xx or 401': (r) => r.status < 300 || r.status === 401 });
  }

  // Attendance summary view (high-traffic read)
  {
    const res = apiGet('/attendance?page=1&pageSize=20');
    check(res, { 'attendance: 2xx or 401': (r) => r.status < 300 || r.status === 401 });
  }

  sleep(1);
}
