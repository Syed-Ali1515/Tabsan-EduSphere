import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5181';
const LOGIN_PATH = '/api/v1/auth/login';
const PROFILE = (__ENV.TEST_PROFILE || 'smoke').toLowerCase();

function toInt(value, fallback) {
  const n = Number.parseInt(value, 10);
  return Number.isFinite(n) && n > 0 ? n : fallback;
}

function buildStagesForProfile(profile) {
  if (profile === 'max') {
    const maxUsers = toInt(__ENV.MAX_USERS, 8000);
    const ramp1 = Math.max(1, Math.floor(maxUsers * 0.3));
    const ramp2 = Math.max(1, Math.floor(maxUsers * 0.65));
    return [
      { duration: __ENV.MAX_RAMP_1 || '2m', target: ramp1 },
      { duration: __ENV.MAX_RAMP_2 || '4m', target: ramp2 },
      { duration: __ENV.MAX_HOLD || '10m', target: maxUsers },
      { duration: __ENV.MAX_RAMP_DOWN || '2m', target: 0 },
    ];
  }

  if (profile === 'stress') {
    return [
      { duration: '1m', target: 80 },
      { duration: '4m', target: 350 },
      { duration: '4m', target: 700 },
      { duration: '1m', target: 0 },
    ];
  }

  if (profile === 'load') {
    return [
      { duration: '1m', target: 40 },
      { duration: '3m', target: 150 },
      { duration: '3m', target: 300 },
      { duration: '1m', target: 0 },
    ];
  }

  return [
    { duration: '30s', target: 10 },
    { duration: '30s', target: 30 },
    { duration: '20s', target: 0 },
  ];
}

const TEST_USERNAME = __ENV.TEST_USERNAME || '';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || '';

export const coreDuration = new Trend('core_duration', true);
export const coreErrors = new Rate('core_errors');

export const options = {
  scenarios: {
    core_traffic: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: buildStagesForProfile(PROFILE),
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.10'],
    core_duration: ['p(95)<3000'],
  },
};

export function setup() {
  if (!TEST_USERNAME || !TEST_PASSWORD) {
    return { token: null };
  }

  const loginRes = http.post(
    `${BASE_URL}${LOGIN_PATH}`,
    JSON.stringify({ username: TEST_USERNAME, password: TEST_PASSWORD, deviceInfo: 'k6-core-current' }),
    { headers: { 'Content-Type': 'application/json' }, tags: { endpoint: 'auth-login-setup' } }
  );

  if (loginRes.status !== 200) {
    return { token: null };
  }

  try {
    const body = loginRes.json();
    return { token: body && body.accessToken ? body.accessToken : null };
  } catch (_) {
    return { token: null };
  }
}

function callGet(path, token, accepted) {
  const headers = { Accept: 'application/json' };
  if (token) headers.Authorization = `Bearer ${token}`;

  const res = http.get(`${BASE_URL}${path}`, {
    headers,
    tags: { endpoint: path },
  });

  coreDuration.add(res.timings.duration);
  const ok = accepted.includes(res.status);
  coreErrors.add(ok ? 0 : 1);

  check(res, {
    [`${path} expected status`]: () => ok,
  });

  return res;
}

export default function (data) {
  const token = data && data.token ? data.token : null;

  group('anon-security-profile', function () {
    callGet('/api/v1/auth/security-profile', token, [200]);
  });

  group('authenticated-dashboard', function () {
    callGet('/api/v1/dashboard/composition', token, [200, 401, 403]);
  });

  group('authenticated-sidebar', function () {
    callGet('/api/v1/sidebar-menu/my-visible', token, [200, 401, 403]);
  });

  group('authenticated-notification-inbox', function () {
    callGet('/api/v1/notification/inbox?page=0&pageSize=20', token, [200, 401, 403]);
  });

  sleep(Math.random() * 0.7 + 0.2);
}

export function handleSummary(data) {
  const txtPath = __ENV.SUMMARY_TXT_PATH;
  const lines = [
    `suite=core`,
    `profile=${PROFILE}`,
    `baseUrl=${BASE_URL}`,
    `iterations=${data.metrics.iterations ? data.metrics.iterations.values.count : 0}`,
    `http_req_failed=${data.metrics.http_req_failed ? data.metrics.http_req_failed.values.rate : 0}`,
    `core_duration_p95=${data.metrics.core_duration ? data.metrics.core_duration.values['p(95)'] : 0}`,
  ];

  const text = `${lines.join('\n')}\n`;
  const output = { stdout: text };
  if (txtPath) {
    output[txtPath] = text;
  }
  return output;
}
