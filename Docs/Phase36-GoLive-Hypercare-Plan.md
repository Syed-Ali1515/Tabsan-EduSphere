# Phase 36 Go-Live and Hypercare Plan

## Objective
Execute production go-live with rollback-safe controls and maintain a structured 24-72h hypercare window.

## Go-Live Execution Controls

1. Use rollback-safe deployment flow only (`Scripts/Phase34-Rollback-Safe-Deployment.ps1`).
2. Run immediate post-deploy smoke validation for:
   - authentication,
   - dashboard,
   - student lifecycle,
   - reporting exports,
   - user import,
   - notifications.
3. Confirm health and observability surfaces are online:
   - `/health`, `/health/instance`, `/health/observability`, `/health/background-jobs`, `/metrics`.

## Incident Triage Board (Hypercare)

| Priority | Trigger | Action | Owner |
|---|---|---|---|
| P1 | Auth blocked, sustained 5xx spike, health endpoint outage | Initiate rollback decision workflow immediately | Release commander + platform on-call |
| P2 | Reporting export or user import degraded | Mitigate within hypercare SLA, track issue in triage board | Platform + QA lead |
| P3 | Non-blocking UX defects | Log and schedule post-hypercare patch train | Product operations |

## Monitoring and SLO Guardrails

- Error-rate threshold: >5% for 10 consecutive minutes triggers rollback review.
- Health endpoint threshold: any critical health endpoint unavailable for >10 minutes triggers incident escalation.
- Latency guardrail: p95 and p99 regressions outside established Stage 34/36 smoke baseline trigger investigation.

## Hypercare Checkpoints

1. H+24:
   - review incident triage board,
   - verify authentication and dashboard stability,
   - confirm no unresolved P1 incidents.
2. H+48:
   - review report export and user import reliability,
   - verify SLO trend stability and background-job health.
3. H+72:
   - complete hypercare closeout review,
   - hand over to steady-state operations,
   - publish final checkpoint summary.

## Stage 36.6 Evidence

Primary execution evidence:
- `Artifacts/Phase36/Stage36.6/GoLive-Hypercare-20260515.md`

Operational dependencies:
- `Docs/Phase36-Deployment-Rollback-Runbook.md`
- `UAT-SAT docs/Phase36-Stage36.5-Approval-Pack.md`