# Load Tests — Tabsan EduSphere

Performance baseline target: **p95 < 200 ms** on core dashboard endpoints under realistic concurrent load.

## Prerequisites

Install k6: <https://k6.io/docs/get-started/installation/>

```powershell
winget install k6
```

Runtime preconditions for local execution:

- API must start successfully and expose `/health`.
- SQL Server connection configured in API settings must be reachable.
- If local SQL is unavailable, run certification against a staging URL with `--env BASE_URL=...`.

## Running

```powershell
# Run against local dev API (default)
k6 run tests/load/k6-baseline.js

# Run against a specific base URL
k6 run --env BASE_URL=https://your-staging-server tests/load/k6-baseline.js

# Phase 31 Stage 31.3 certification band runs
k6 run --env BAND=up-to-10k tests/load/k6-certification-bands.js
k6 run --env BAND=10k-100k tests/load/k6-certification-bands.js
k6 run --env BAND=100k-500k tests/load/k6-certification-bands.js
k6 run --env BAND=500k-1m tests/load/k6-certification-bands.js

# Optional duration override for constrained CI/terminal runtimes
k6 run --env BASE_URL=http://localhost:5181 --env BAND=500k-1m --env DURATION_OVERRIDE=90s tests/load/k6-certification-bands.js

# Optional: include an admin token for authenticated paths
k6 run --env BAND=10k-100k --env ADMIN_TOKEN=<jwt-token> tests/load/k6-certification-bands.js

# Recovery smoke (node/service failure simulation)
powershell -ExecutionPolicy Bypass -File tests/load/recovery-smoke.ps1 -BaseUrl http://localhost:5181

# Recovery smoke with explicit DB connection override
powershell -ExecutionPolicy Bypass -File tests/load/recovery-smoke.ps1 -BaseUrl http://localhost:5181 -ConnectionString "Server=(localdb)\MSSQLLocalDB;Database=TabsanEduSphere;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

## Interpreting Results

The script uses thresholds to assert pass/fail:

- `http_req_duration['p(95)'] < 200` — p95 response time under 200 ms
- `http_req_failed < 0.01` — error rate below 1 %

A failed threshold exits with code 99 (non-zero), which will fail the CI step.

## Scenarios

| Scenario | VUs | Duration | Endpoints |
| --- | --- | --- | --- |
| Smoke | 1 | 30 s | All core endpoints |
| Baseline | 20 | 1 min | All core endpoints |
| Spike | 50 | 30 s | Dashboard + Auth |

## Stage 31.3 Certification Bands

The Stage 31.3 script (`k6-certification-bands.js`) maps required target bands to concrete VU and duration settings:

| Band | Script BAND value | VUs | Duration | p95 threshold |
| --- | --- | --- | --- | --- |
| Up to 10k users | `up-to-10k` | 20 | 2 min | < 250 ms |
| 10k to 100k users | `10k-100k` | 50 | 3 min | < 350 ms |
| 100k to 500k users | `100k-500k` | 100 | 4 min | < 500 ms |
| 500k to 1M+ users | `500k-1m` | 150 | 5 min | < 700 ms |

Common threshold for all bands:

- `error_rate < 2 %`

## Recovery Test

`recovery-smoke.ps1` validates node/service failure recovery by:

1. Starting the API and validating `/health` returns 200.
2. Running API startup with `--no-launch-profile` to avoid environment/profile drift.
3. Stopping the API process to simulate node failure.
4. Restarting the API process and polling `/health` until recovery or timeout.
