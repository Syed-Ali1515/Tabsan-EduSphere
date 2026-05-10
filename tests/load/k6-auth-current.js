import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5181';
const LOGIN_PATH = '/api/v1/auth/login';
const SECURITY_PROFILE_PATH = '/api/v1/auth/security-profile';

const DEFAULT_USERS = [
  { username: 'admin', password: 'Admin@123' },
  { username: 'faculty', password: 'Faculty@123' },
  { username: 'student', password: 'Student@123' },
];

function parseUsers() {
  const raw = (__ENV.TEST_USERS_JSON || '').trim();
  if (!raw) return DEFAULT_USERS;

  try {
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed) || parsed.length === 0) return DEFAULT_USERS;
    return parsed.filter((u) => u && u.username && u.password);
  } catch (_) {
    return DEFAULT_USERS;
  }
}

const TEST_USERS = parseUsers();

export const authFailures = new Rate('auth_failures');
export const authReqDuration = new Trend('auth_req_duration', true);
export const authSuccesses = new Counter('auth_successes');
export const authAttempts = new Counter('auth_attempts');

export const options = {
  scenarios: {
    auth_ramp: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '1m', target: 20 },
        { duration: '3m', target: 150 },
        { duration: '3m', target: 300 },
        { duration: '1m', target: 0 },
      ],
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    http_req_failed: ['rate<0.05'],
    http_req_duration: ['p(95)<1500'],
    auth_req_duration: ['p(95)<1200'],
  },
};

function pickUser() {
  return TEST_USERS[Math.floor(Math.random() * TEST_USERS.length)];
}

export default function () {
  const user = pickUser();

  const secRes = http.get(`${BASE_URL}${SECURITY_PROFILE_PATH}`, {
    headers: { Accept: 'application/json' },
    tags: { endpoint: 'security-profile' },
  });

  check(secRes, {
    'security profile reachable': (r) => r.status === 200,
  });

  const payload = JSON.stringify({
    username: user.username,
    password: user.password,
    deviceInfo: 'k6-auth-current',
  });

  const loginRes = http.post(`${BASE_URL}${LOGIN_PATH}`, payload, {
    headers: { 'Content-Type': 'application/json' },
    tags: { endpoint: 'auth-login' },
  });

  authAttempts.add(1);
  authReqDuration.add(loginRes.timings.duration);

  const acceptedStatus = [200, 401, 403, 423, 428].includes(loginRes.status);
  const hasToken = loginRes.status === 200 && loginRes.body && loginRes.body.includes('accessToken');

  check(loginRes, {
    'login endpoint returns known status': () => acceptedStatus,
    'login returns token on success': () => loginRes.status !== 200 || hasToken,
  });

  if (loginRes.status === 200 && hasToken) {
    authSuccesses.add(1);
    authFailures.add(0);
  } else {
    authFailures.add(1);
  }

  sleep(Math.random() * 1.5 + 0.5);
}
