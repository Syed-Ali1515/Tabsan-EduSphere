import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5181';
const LOGIN_PATH = '/api/v1/auth/login';
const SECURITY_PROFILE_PATH = '/api/v1/auth/security-profile';
const PROFILE = (__ENV.TEST_PROFILE || 'smoke').toLowerCase();

function toInt(value, fallback) {
  const n = Number.parseInt(value, 10);
  return Number.isFinite(n) && n > 0 ? n : fallback;
}

function profileDefaultMaxUsers(profile) {
  if (profile === 'max-50k') return 50000;
  if (profile === 'max-100k') return 100000;
  if (profile === 'max-1m') return 1000000;
  return 10000;
}

const CONFIGURED_MAX_USERS = toInt(__ENV.MAX_USERS, profileDefaultMaxUsers(PROFILE));

function buildStagesForProfile(profile) {
  if (profile === 'max' || profile === 'max-50k' || profile === 'max-100k' || profile === 'max-1m') {
    const maxUsers = CONFIGURED_MAX_USERS;
    const ramp1 = Math.max(1, Math.floor(maxUsers * 0.25));
    const ramp2 = Math.max(1, Math.floor(maxUsers * 0.6));
    return [
      { duration: __ENV.MAX_RAMP_1 || '2m', target: ramp1 },
      { duration: __ENV.MAX_RAMP_2 || '6m', target: ramp2 },
      { duration: __ENV.MAX_HOLD || '12m', target: maxUsers },
      { duration: __ENV.MAX_RAMP_DOWN || '2m', target: 0 },
    ];
  }

  if (profile === 'stress') {
    return [
      { duration: '1m', target: 100 },
      { duration: '4m', target: 500 },
      { duration: '4m', target: 1000 },
      { duration: '1m', target: 0 },
    ];
  }

  if (profile === 'load') {
    return [
      { duration: '1m', target: 50 },
      { duration: '3m', target: 250 },
      { duration: '3m', target: 500 },
      { duration: '1m', target: 0 },
    ];
  }

  return [
    { duration: '30s', target: 10 },
    { duration: '30s', target: 25 },
    { duration: '20s', target: 0 },
  ];
}

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
      stages: buildStagesForProfile(PROFILE),
      gracefulRampDown: '20s',
    },
  },
  thresholds: {
    auth_failures: ['rate<0.05'],
    http_req_duration: ['p(95)<2500'],
    auth_req_duration: ['p(95)<2000'],
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

  const isFailure = !acceptedStatus || (loginRes.status === 200 && !hasToken);
  if (!isFailure) {
    authSuccesses.add(1);
    authFailures.add(0);
  } else {
    authFailures.add(1);
  }

  sleep(Math.random() * 0.6 + 0.2);
}

export function handleSummary(data) {
  const txtPath = __ENV.SUMMARY_TXT_PATH;
  const lines = [
    `suite=auth`,
    `profile=${PROFILE}`,
    `configured_max_users=${CONFIGURED_MAX_USERS}`,
    `baseUrl=${BASE_URL}`,
    `iterations=${data.metrics.iterations ? data.metrics.iterations.values.count : 0}`,
    `http_req_failed=${data.metrics.http_req_failed ? data.metrics.http_req_failed.values.rate : 0}`,
    `http_req_duration_p95=${data.metrics.http_req_duration ? data.metrics.http_req_duration.values['p(95)'] : 0}`,
  ];

  const text = `${lines.join('\n')}\n`;
  const output = { stdout: text };
  if (txtPath) {
    output[txtPath] = text;
  }
  return output;
}
