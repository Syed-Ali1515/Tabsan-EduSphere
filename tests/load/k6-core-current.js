import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Trend, Rate } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5181';
const LOGIN_PATH = '/api/v1/auth/login';

const TEST_USERNAME = __ENV.TEST_USERNAME || '';
const TEST_PASSWORD = __ENV.TEST_PASSWORD || '';

export const coreDuration = new Trend('core_duration', true);
export const coreErrors = new Rate('core_errors');

export const options = {
  scenarios: {
    core_traffic: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '1m', target: 15 },
        { duration: '4m', target: 100 },
        { duration: '4m', target: 200 },
        { duration: '1m', target: 0 },
      ],
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    core_duration: ['p(95)<2000'],
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

  sleep(Math.random() + 0.5);
}
