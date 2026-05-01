# Load Tests — Tabsan EduSphere

Performance baseline target: **p95 < 200 ms** on core dashboard endpoints under realistic concurrent load.

## Prerequisites

Install k6: https://k6.io/docs/get-started/installation/

```
winget install k6
```

## Running

```powershell
# Run against local dev API (default)
k6 run tests/load/k6-baseline.js

# Run against a specific base URL
k6 run --env BASE_URL=https://your-staging-server tests/load/k6-baseline.js
```

## Interpreting Results

The script uses thresholds to assert pass/fail:
- `http_req_duration['p(95)'] < 200` — p95 response time under 200 ms
- `http_req_failed < 0.01` — error rate below 1 %

A failed threshold exits with code 99 (non-zero), which will fail the CI step.

## Scenarios

| Scenario | VUs | Duration | Endpoints |
|---|---|---|---|
| Smoke | 1 | 30 s | All core endpoints |
| Baseline | 20 | 1 min | All core endpoints |
| Spike | 50 | 30 s | Dashboard + Auth |
