/**
 * K6 Load Testing Script for Tabsan-EduSphere Login Endpoint
 * 
 * This script simulates high-traffic login scenarios with:
 * - Gradual virtual user ramp-up (100 → 1,000 → 10,000 → 100,000+)
 * - Realistic request delays
 * - Error handling and validation
 * - Comprehensive metrics tracking
 * 
 * Usage: k6 run login-load-test.js
 */

import http from 'k6/http';
import { check, group, sleep } from 'k6';
import { Rate, Trend, Counter, Gauge } from 'k6/metrics';

// ============================================================================
// CUSTOM METRICS
// ============================================================================

// Track error rate (percentage of failed requests)
export const errorRate = new Rate('errors');

// Track response time (milliseconds)
export const responseTime = new Trend('response_time');

// Track successful logins
export const successfulLogins = new Counter('successful_logins');

// Track failed logins
export const failedLogins = new Counter('failed_logins');

// Track concurrent users
export const activeVirtualUsers = new Gauge('active_vus');

// ============================================================================
// TEST CONFIGURATION
// ============================================================================

export const options = {
  // Define test stages with gradual ramp-up
  // Each stage specifies: duration and target number of virtual users
  stages: [
    // Stage 1: Warm-up with 100 users for 2 minutes
    { duration: '2m', target: 100, name: 'Warm-up' },
    
    // Stage 2: Ramp to 1,000 users over 5 minutes
    { duration: '5m', target: 1000, name: 'Ramp-up 1k' },
    
    // Stage 3: Hold 1,000 users for 5 minutes (stress test)
    { duration: '5m', target: 1000, name: 'Stress 1k' },
    
    // Stage 4: Ramp to 10,000 users over 10 minutes
    { duration: '10m', target: 10000, name: 'Ramp-up 10k' },
    
    // Stage 5: Hold 10,000 users for 10 minutes
    { duration: '10m', target: 10000, name: 'Stress 10k' },
    
    // Stage 6: Ramp to 100,000 users over 15 minutes (high load)
    { duration: '15m', target: 100000, name: 'Ramp-up 100k' },
    
    // Stage 7: Hold 100,000 users for 15 minutes (sustained high load)
    { duration: '15m', target: 100000, name: 'Stress 100k' },
    
    // Stage 8: Ramp down to 0 over 5 minutes (cool-down)
    { duration: '5m', target: 0, name: 'Cool-down' },
  ],

  // Thresholds define pass/fail criteria for the test
  thresholds: {
    // Response time should be under 500ms for 95% of requests
    'response_time': ['p(95)<500', 'p(99)<1000', 'avg<200'],
    
    // Error rate should be below 1%
    'errors': ['rate<0.01'],
  },

  // Report summary after test completes
  summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(95)', 'p(99)'],
};

// ============================================================================
// TEST DATA & CONFIGURATION
// ============================================================================

// Base URL of your API (customize as needed)
const BASE_URL = __ENV.TARGET_URL || 'http://localhost:5000';

// Test user credentials (generate as needed)
const TEST_USERS = [
  { username: 'student1@example.com', password: 'TestPassword123!' },
  { username: 'student2@example.com', password: 'TestPassword123!' },
  { username: 'student3@example.com', password: 'TestPassword123!' },
  { username: 'faculty1@example.com', password: 'TestPassword123!' },
  { username: 'faculty2@example.com', password: 'TestPassword123!' },
  { username: 'admin1@example.com', password: 'TestPassword123!' },
  { username: 'admin2@example.com', password: 'TestPassword123!' },
  { username: 'admin3@example.com', password: 'TestPassword123!' },
];

// HTTP request headers
const HEADERS = {
  'Content-Type': 'application/json',
  'Accept': 'application/json',
  'User-Agent': 'k6-load-test/1.0',
};

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

/**
 * Get a random test user from the pool
 * This simulates different users logging in concurrently
 */
function getRandomTestUser() {
  return TEST_USERS[Math.floor(Math.random() * TEST_USERS.length)];
}

/**
 * Generate realistic think time (delay between requests)
 * Simulates user behavior between login and next action
 * Range: 1-5 seconds
 */
function generateThinkTime() {
  return Math.random() * 4 + 1; // 1-5 seconds
}

/**
 * Log test progress to console
 */
function logProgress(stageName, vus, successCount, failureCount) {
  const successRate = ((successCount / (successCount + failureCount)) * 100).toFixed(2);
  console.log(`[${stageName}] VUs: ${vus} | Success: ${successCount} | Failures: ${failureCount} | Rate: ${successRate}%`);
}

/**
 * Prepare login payload
 */
function prepareLoginPayload(user) {
  return JSON.stringify({
    email: user.username,
    password: user.password,
    rememberMe: false,
  });
}

// ============================================================================
// MAIN TEST FUNCTION
// ============================================================================

export default function () {
  // Get random user and think time
  const testUser = getRandomTestUser();
  const thinkTime = generateThinkTime();

  group('Login Test', function () {
    // Prepare the request
    const loginPayload = prepareLoginPayload(testUser);
    const url = `${BASE_URL}/api/auth/login`;

    // Send POST request to login endpoint
    const response = http.post(url, loginPayload, {
      headers: HEADERS,
      timeout: '30s', // Request timeout
      tags: { name: 'LoginEndpoint' },
    });

    // =========================================================================
    // RESPONSE VALIDATION & ERROR HANDLING
    // =========================================================================

    // Check if response status is 200 OK
    const loginSuccess = check(response, {
      // Status check (200 = success, 401 = invalid credentials, 500 = server error)
      'Status is 200': (r) => r.status === 200,
      'Status is not 500': (r) => r.status !== 500,
      'Status is not 429': (r) => r.status !== 429, // Rate limit check
      
      // Response body checks
      'Response body contains access token': (r) => {
        try {
          const body = r.json();
          return body.data && body.data.accessToken;
        } catch (e) {
          return false;
        }
      },
      'Response body valid JSON': (r) => {
        try {
          r.json();
          return true;
        } catch (e) {
          return false;
        }
      },
      
      // Response time check
      'Response time < 500ms': (r) => r.timings.duration < 500,
      'Response time < 1000ms': (r) => r.timings.duration < 1000,
    });

    // Track response time metric
    responseTime.add(response.timings.duration, { user: testUser.username });

    // Update success/failure counters and error rate
    if (loginSuccess) {
      successfulLogins.add(1);
      errorRate.add(0); // 0 for success
    } else {
      failedLogins.add(1);
      errorRate.add(1); // 1 for failure
      
      // Log error details for debugging
      console.error(`Login failed for ${testUser.username}: Status ${response.status}`);
      
      // Check for specific error conditions
      if (response.status === 429) {
        console.warn('Rate limit hit! Consider reducing VU count or implementing backoff.');
      }
      if (response.status === 500) {
        console.error('Server error! Check backend logs.');
      }
    }

    // Update concurrent VUs metric
    activeVirtualUsers.set(__VU);

    // Log detailed timing information
    if (response.status === 200) {
      console.log(`[${__ITER}] Login successful - ${response.timings.duration}ms (User: ${testUser.username})`);
    } else {
      console.log(`[${__ITER}] Login failed with status ${response.status} (User: ${testUser.username})`);
    }
  });

  // Realistic think time between requests (simulates user delay)
  sleep(thinkTime);
}

// ============================================================================
// TEST TEARDOWN & SUMMARY
// ============================================================================

/**
 * Summary function runs after test completes
 * Provides detailed metrics and recommendations
 */
export function summary(data) {
  console.log('\n========================================');
  console.log('    LOAD TEST SUMMARY REPORT');
  console.log('========================================\n');

  // Extract key metrics
  const httpDuration = data.metrics.response_time.values;
  const totalRequests = data.metrics.successful_logins.values + data.metrics.failed_logins.values;
  const errorPercentage = (data.metrics.failed_logins.values / totalRequests) * 100;

  console.log(`Total Requests: ${totalRequests}`);
  console.log(`Successful: ${data.metrics.successful_logins.values}`);
  console.log(`Failed: ${data.metrics.failed_logins.values}`);
  console.log(`Error Rate: ${errorPercentage.toFixed(2)}%\n`);

  console.log('Response Time Metrics:');
  console.log(`  Min: ${data.metrics.response_time.values.min}ms`);
  console.log(`  Max: ${data.metrics.response_time.values.max}ms`);
  console.log(`  Avg: ${data.metrics.response_time.values.avg.toFixed(2)}ms`);
  console.log(`  P95: ${data.metrics.response_time.values.p95}ms`);
  console.log(`  P99: ${data.metrics.response_time.values.p99}ms\n`);

  // Recommendations based on results
  if (errorPercentage > 5) {
    console.log('⚠️  WARNING: High error rate detected!');
    console.log('   Recommendations:');
    console.log('   - Check backend server logs for errors');
    console.log('   - Verify database connection pooling');
    console.log('   - Monitor CPU and memory usage');
    console.log('   - Check network bandwidth');
  }

  if (data.metrics.response_time.values.p95 > 500) {
    console.log('⚠️  WARNING: High response times detected!');
    console.log('   Recommendations:');
    console.log('   - Implement caching strategy');
    console.log('   - Optimize database queries');
    console.log('   - Add horizontal scaling');
    console.log('   - Review authentication mechanism efficiency');
  }

  console.log('\n========================================\n');
}
