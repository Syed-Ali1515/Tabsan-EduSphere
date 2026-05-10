# Phase 31 Stage 31.3 - Performance and Reliability Certification

Date: 2026-05-10
Status: Completed

## Scope

Stage 31.3 certifies:

- Load readiness across target user bands
- Recovery readiness for node/service failures

## Implemented Certification Assets

## 1) Load Certification Bands

Added script:

- tests/load/k6-certification-bands.js

Band coverage implemented:

- up-to-10k
- 10k-100k
- 100k-500k
- 500k-1m

Each band defines:

- Dedicated VU level and run duration
- p95 latency threshold
- Error-rate threshold

Endpoint set exercised:

- /health
- /api/v1/modules
- /api/v1/sidebar-menu/my-visible
- /api/v1/notifications/inbox
- /api/v1/attendance

## 2) Recovery Certification

Added script:

- tests/load/recovery-smoke.ps1

Recovery smoke procedure automated by script:

1. Start API process and verify /health returns 200.
2. Force-stop process to simulate node/service failure.
3. Restart API process and poll /health until recovery timeout.
4. Fail if recovery does not complete in the configured window.

## 3) Runbook Updates

Updated:

- tests/load/README.md

README now includes:

- Stage 31.3 banded k6 execution commands
- Recovery smoke command
- Band-to-threshold mapping table

## Validation Summary

Project regression validation run after Stage 31.3 updates:

- dotnet test Tabsan.EduSphere.sln --no-build
- Result: 201/201 passed

Operational execution attempt (local):

- k6 runtime installation verified (`k6 v2.0.0-rc1`).
- Local API startup failed before `/health` due missing SQL Server connectivity in current environment.
- Recovery smoke and k6 band execution therefore require a reachable SQL-backed API environment (local SQL instance or staging URL).

Operational certification execution commands:

- k6 run --env BAND=up-to-10k tests/load/k6-certification-bands.js
- k6 run --env BAND=10k-100k tests/load/k6-certification-bands.js
- k6 run --env BAND=100k-500k tests/load/k6-certification-bands.js
- k6 run --env BAND=500k-1m tests/load/k6-certification-bands.js
- powershell -ExecutionPolicy Bypass -File tests/load/recovery-smoke.ps1

## Outcome

Stage 31.3 certification framework is complete with executable, repeatable performance-band and recovery-smoke validation artifacts and updated operational runbook guidance.
