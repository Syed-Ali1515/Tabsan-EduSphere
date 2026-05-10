/**
 * Final-Touches Phase 31 Stage 31.3 — load-band certification script.
 *
 * Purpose:
 * - Run a single selected certification band against key read-heavy API routes.
 * - Enforce stage-level pass/fail thresholds for latency and error rate.
 *
 * Run examples:
 *   k6 run tests/load/k6-certification-bands.js
 *   k6 run --env BASE_URL=https://staging.example.com --env BAND=10k-100k tests/load/k6-certification-bands.js
 *   k6 run --env BAND=500k-1M --env ADMIN_TOKEN=<token> tests/load/k6-certification-bands.js
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const ADMIN_TOKEN = __ENV.ADMIN_TOKEN || '';
const BAND = (__ENV.BAND || 'up-to-10k').toLowerCase();
const DURATION_OVERRIDE = (__ENV.DURATION_OVERRIDE || '').trim();

const errorRate = new Rate('error_rate');

const BAND_CONFIG = {
  'up-to-10k': {
    vus: 20,
    duration: '2m',
    p95Ms: 250,
  },
  '10k-100k': {
    vus: 50,
    duration: '3m',
    p95Ms: 350,
  },
  '100k-500k': {
    vus: 100,
    duration: '4m',
    p95Ms: 500,
  },
  '500k-1m': {
    vus: 150,
    duration: '5m',
    p95Ms: 700,
  },
};

const selected = BAND_CONFIG[BAND] || BAND_CONFIG['up-to-10k'];
const selectedDuration = DURATION_OVERRIDE || selected.duration;

export const options = {
  scenarios: {
    certification_band: {
      executor: 'constant-vus',
      vus: selected.vus,
      duration: selectedDuration,
      tags: { scenario: `cert-${BAND}` },
    },
  },
  thresholds: {
    'http_req_duration{type:api}': [`p(95)<${selected.p95Ms}`],
    error_rate: ['rate<0.02'],
  },
};

function apiGet(path) {
  const headers = { 'Content-Type': 'application/json' };
  if (ADMIN_TOKEN) {
    headers.Authorization = `Bearer ${ADMIN_TOKEN}`;
  }

  const res = http.get(`${BASE_URL}/api/v1${path}`, {
    headers,
    tags: { type: 'api' },
  });

  errorRate.add(res.status >= 500 || res.status === 0 ? 1 : 0);
  return res;
}

export default function () {
  const health = http.get(`${BASE_URL}/health`, { tags: { type: 'api' } });
  check(health, { 'health: 200': (r) => r.status === 200 });

  const modules = apiGet('/modules');
  check(modules, { 'modules: 2xx or 401': (r) => r.status < 300 || r.status === 401 });

  const sidebar = apiGet('/sidebar-menu/my-visible');
  check(sidebar, { 'sidebar: 2xx or 401': (r) => r.status < 300 || r.status === 401 });

  const notifications = apiGet('/notifications/inbox?page=1&pageSize=10');
  check(notifications, { 'notifications: 2xx or 401': (r) => r.status < 300 || r.status === 401 });

  const attendance = apiGet('/attendance?page=1&pageSize=20');
  check(attendance, { 'attendance: 2xx or 401': (r) => r.status < 300 || r.status === 401 });

  sleep(1);
}
