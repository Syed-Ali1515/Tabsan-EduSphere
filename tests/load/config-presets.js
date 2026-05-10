/**
 * K6 Load Test Configuration Presets
 * 
 * Quick-start configurations for different testing scenarios
 * Usage: k6 run -c preset-name config-presets.js
 */

// ============================================================================
// PRESET: Light Load (Local Testing)
// ============================================================================

export const lightLoad = {
  name: 'Light Load - Local Testing',
  stages: [
    { duration: '1m', target: 10 },
    { duration: '2m', target: 10 },
    { duration: '1m', target: 0 },
  ],
  vus: 10,
  duration: '4m',
  thresholds: {
    'response_time': ['p(95)<200'],
    'errors': ['rate<0.05'],
  },
};

// ============================================================================
// PRESET: Medium Load (Staging Testing)
// ============================================================================

export const mediumLoad = {
  name: 'Medium Load - Staging Testing',
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 1000 },
    { duration: '5m', target: 1000 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    'response_time': ['p(95)<500', 'p(99)<1000'],
    'errors': ['rate<0.01'],
  },
};

// ============================================================================
// PRESET: High Load (Production Validation)
// ============================================================================

export const highLoad = {
  name: 'High Load - Production Validation',
  stages: [
    { duration: '2m', target: 100 },
    { duration: '5m', target: 1000 },
    { duration: '5m', target: 10000 },
    { duration: '10m', target: 10000 },
    { duration: '2m', target: 0 },
  ],
  thresholds: {
    'response_time': ['p(95)<800', 'p(99)<1500'],
    'errors': ['rate<0.02'],
  },
};

// ============================================================================
// PRESET: Extreme Load (1M Users Simulation)
// ============================================================================

export const extremeLoad = {
  name: 'Extreme Load - 1M Users',
  stages: [
    { duration: '2m', target: 100, name: 'Warm-up' },
    { duration: '5m', target: 1000, name: 'Ramp 1k' },
    { duration: '5m', target: 10000, name: 'Ramp 10k' },
    { duration: '10m', target: 100000, name: 'Ramp 100k' },
    { duration: '10m', target: 1000000, name: 'Peak 1M' },
    { duration: '15m', target: 1000000, name: 'Sustain 1M' },
    { duration: '5m', target: 0, name: 'Cool-down' },
  ],
  thresholds: {
    'response_time': ['p(95)<2000', 'p(99)<5000'],
    'errors': ['rate<0.05'],
  },
};

// ============================================================================
// PRESET: Spike Test (Sudden Traffic Surge)
// ============================================================================

export const spikeTest = {
  name: 'Spike Test - Sudden Load Surge',
  stages: [
    { duration: '2m', target: 100, name: 'Normal' },
    { duration: '1m', target: 100000, name: 'Spike' },
    { duration: '2m', target: 100000, name: 'Sustain' },
    { duration: '1m', target: 100, name: 'Recovery' },
  ],
  thresholds: {
    'response_time': ['p(95)<1000'],
    'errors': ['rate<0.03'],
  },
};

// ============================================================================
// PRESET: Soak Test (Long-running, Steady Load)
// ============================================================================

export const soakTest = {
  name: 'Soak Test - Long Duration',
  stages: [
    { duration: '5m', target: 1000, name: 'Ramp-up' },
    { duration: '2h', target: 1000, name: 'Soak' },
    { duration: '5m', target: 0, name: 'Cool-down' },
  ],
  thresholds: {
    'response_time': ['p(95)<500'],
    'errors': ['rate<0.01'],
  },
};

// ============================================================================
// PRESET: Stress Test (Push to Limits)
// ============================================================================

export const stressTest = {
  name: 'Stress Test - Find Breaking Point',
  stages: [
    { duration: '2m', target: 1000 },
    { duration: '2m', target: 5000 },
    { duration: '2m', target: 10000 },
    { duration: '2m', target: 50000 },
    { duration: '2m', target: 100000 },
    { duration: '2m', target: 500000 },
    { duration: '5m', target: 0 },
  ],
  thresholds: {
    'response_time': ['p(95)<3000'],
    'errors': ['rate<0.1'],
  },
};

// ============================================================================
// PRESET: Custom Configuration
// ============================================================================

export const customConfig = {
  name: 'Custom Configuration',
  stages: [
    // Modify as needed
    { duration: '1m', target: 100 },
    { duration: '2m', target: 100 },
    { duration: '1m', target: 0 },
  ],
  thresholds: {
    'response_time': ['p(95)<500'],
    'errors': ['rate<0.01'],
  },
};

// ============================================================================
// Helper function to get configuration by name
// ============================================================================

export function getConfig(name) {
  const configs = {
    'light': lightLoad,
    'medium': mediumLoad,
    'high': highLoad,
    'extreme': extremeLoad,
    'spike': spikeTest,
    'soak': soakTest,
    'stress': stressTest,
    'custom': customConfig,
  };
  
  return configs[name] || lightLoad;
}

export default {
  lightLoad,
  mediumLoad,
  highLoad,
  extremeLoad,
  spikeTest,
  soakTest,
  stressTest,
  customConfig,
  getConfig,
};
