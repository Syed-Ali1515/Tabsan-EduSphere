# Phase 30 Stage 30.3 Reliability and Rollback Runbook

## Purpose
This runbook defines safe rollout and emergency rollback procedures using the feature-flag control plane.

## Scope
- Outbound integration controls
- Tenant operations write paths
- Gateway diagnostics exposure

## Feature Flags
- `tenant-operations.write`
- `integration-gateway.enabled`
- `gateway-diagnostics.enabled`

## Safe Deployment Procedure
1. Confirm `main` branch is green (build and tests pass).
2. Deploy artifact to staging.
3. Keep risky flags disabled in staging until smoke checks complete.
4. Enable one flag at a time using `PUT /api/v1/feature-flags/{key}`.
5. Verify logs, API latency, and failure rates after each enablement window.
6. Promote to production with the same progressive flag sequence.

## Emergency Rollback Procedure
1. Trigger rollback endpoint:
   - `POST /api/v1/feature-flags/rollback`
   - Body example:
     ```json
     {
       "keys": ["tenant-operations.write", "integration-gateway.enabled"],
       "reason": "Incident rollback"
     }
     ```
2. Verify flags are disabled via `GET /api/v1/feature-flags`.
3. Confirm impacted write paths now return rollback-safe responses.
4. Announce rollback completion to stakeholders.
5. Start post-incident review and root-cause analysis.

## Post-Rollback Checklist
- Capture timeline, affected endpoints, and customer impact.
- Attach logs/metrics snapshots to incident report.
- Open corrective action tasks before re-enabling flags.
