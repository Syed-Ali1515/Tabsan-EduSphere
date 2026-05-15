# Phase 36 Deployment and Rollback Runbook

## Scope
This runbook defines deployment-day execution, rollback criteria, ownership, and escalation for Phase 36 production go-live.

## Ownership and On-Call Escalation

| Function | Primary Owner | Backup Owner | Escalation SLA |
|---|---|---|---|
| Release commander | SuperAdmin Operations Lead | Platform Lead | Immediate |
| API/Web deployment | Platform Engineer On-Call | Senior Backend Engineer | 10 minutes |
| Database migration and rollback | DBA On-Call | Platform Engineer On-Call | 10 minutes |
| Validation and UAT witness | QA Lead | Product Operations Lead | 15 minutes |
| Stakeholder communications | Product Operations Lead | SuperAdmin Operations Lead | 15 minutes |

Escalation path:
1. Primary owner attempts mitigation.
2. Backup owner joins if issue is unresolved within SLA.
3. Release commander declares go/no-go and rollback decision.

## Maintenance Window

- Planned window: 120 minutes
- Start trigger: release commander approval and communication sent to all stakeholders
- Freeze policy: no non-blocker code or configuration changes during deployment window

## Deployment Sequence

1. Confirm RC baseline and Stage 36.1 manifest alignment.
2. Execute environment and secret readiness validation:
   - `powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Validate-Environment-Readiness.ps1" -RepoRoot "<repo>" -FailOnIssues`
3. Execute deployment rehearsal validation:
   - Demo flow: `powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Deployment-Rehearsal.ps1" -RepoRoot "<repo>" -DeploymentMode "Demo"`
   - Clean flow: `powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Deployment-Rehearsal.ps1" -RepoRoot "<repo>" -DeploymentMode "Clean"`
4. Execute rollback-safe deployment flow:
   - Demo flow: `powershell -ExecutionPolicy Bypass -File "Scripts\Phase34-Rollback-Safe-Deployment.ps1" -ServerInstance "<server>" -DatabaseName "Tabsan-EduSphere" -DeploymentMode "Demo"`
   - Clean flow: `powershell -ExecutionPolicy Bypass -File "Scripts\Phase34-Rollback-Safe-Deployment.ps1" -ServerInstance "<server>" -DatabaseName "Tabsan-EduSphere" -DeploymentMode "Clean"`
5. Execute backup/restore drill evidence step:
   - `powershell -ExecutionPolicy Bypass -File "Scripts\Phase34-BackupRestore-Drill.ps1" -ServerInstance "<server>" -DatabaseName "Tabsan-EduSphere"`

## Rollback Decision Thresholds

Immediate rollback is required if any of the following occurs:
1. Authentication or authorization regression blocks SuperAdmin login.
2. Any health endpoint (`/health`, `/health/instance`, `/health/observability`, `/health/background-jobs`) is unavailable for more than 10 minutes.
3. API error-rate spike persists above 5% for more than 10 minutes.
4. Critical reporting or student lifecycle endpoint returns persistent 5xx responses.
5. Data migration check fails post-deploy validation script (`05-PostDeployment-Checks.sql`).

Rollback authority:
- Release commander with DBA and platform owner concurrence.

## Communications Plan

1. T-30 minutes: deployment-start notice to product, support, and operations channels.
2. T+0 minutes: maintenance-mode confirmation.
3. T+45 minutes: midpoint status update (on track / risk flagged).
4. T+90 minutes: go-live validation status update.
5. Completion: success announcement or rollback announcement with incident reference.

## Post-Deploy Validation Script Set

Run in this order:
1. `Scripts\05-PostDeployment-Checks.sql`
2. `Scripts\Phase36-Security-Reliability-Performance-Gates.ps1` (dry-run gate plan)
3. Focused validation tests:
   - `Phase36Stage4HealthAndLicenseGateTests`
   - `Phase36Stage4PerformanceSmokeTests`
   - `Phase31Stage2SecurityHardeningTests`

## Sign-Off

- Release commander sign-off: Approved
- Platform owner sign-off: Approved
- DBA sign-off: Approved
- QA lead sign-off: Approved
- Product operations sign-off: Approved