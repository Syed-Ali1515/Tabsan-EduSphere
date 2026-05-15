# Tabsan EduSphere - Database Scripts (Current Hierarchy)

This folder contains the refreshed database scripts aligned to the current EF Core model, active modules, and present application hierarchy.

## Script Hierarchy

| Order | File | Purpose |
| --- | --- | --- |
| 01 | `01-Schema-Current.sql` | Idempotent full schema generated from current EF migrations. |
| 02 | `02-Seed-Core.sql` | Core seed: roles, modules, module status, portal settings, report definitions, and base sidebar menus. |
| 03 | `03-FullDummyData.sql` | Full dummy data for demos: multi-department hierarchy, users, programs, courses, offerings, enrollments, assignments, submissions, attendance, results, quizzes, support tickets, discussions, and notifications. |
| 04 | `04-Maintenance-Indexes-And-Views.sql` | Safe maintenance indexes + operational views for current workload paths. |
| 05 | `05-PostDeployment-Checks.sql` | Validation queries for migration/version, core entity counts, and demo-domain counts (results/quizzes/helpdesk/discussions). |

## Recommended Execution

Run step-by-step:

```powershell
sqlcmd -S "localhost" -E -d "master" -i "Scripts\01-Schema-Current.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\02-Seed-Core.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\03-FullDummyData.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\04-Maintenance-Indexes-And-Views.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\05-PostDeployment-Checks.sql"
```

Environment notes:

- Run `01-Schema-Current.sql` from `master`; it creates and switches to `[Tabsan-EduSphere]` automatically.
- Run scripts with an account that can create databases, alter schema, and create indexes/views.
- Execute in the exact order `01 -> 02 -> 03 -> 04 -> 05` for full deployment + validation.
- If legacy EduSphere objects exist in `master`, run `00-Cleanup-Master-Mistake.sql` once before `01`.

Rollback and verification checklist:

1. Pre-deployment backup:
	- take a full backup/snapshot of target SQL Server database state.
2. Deployment execution:
	- run `01 -> 05` in sequence and capture command output.
3. Verification gate:
	- require `05-PostDeployment-Checks.sql` to complete without `RAISERROR` failures.
4. Failure handling:
	- if schema/seed execution fails, restore pre-deployment backup and stop rollout.
	- if accidental `master` pollution is detected, run `00-Cleanup-Master-Mistake.sql` then re-run from `01`.
5. Sign-off evidence:
	- archive script logs and post-deployment check output with timestamp/environment label.

## Notes

- `01-Schema-Current.sql` is generated from the current migration chain and can be safely rerun.
- All main scripts now explicitly switch to `[Tabsan-EduSphere]`, and schema script creates the database if it does not exist.
- `01-Schema-Current.sql` also provisions a custom metadata table `[Tabsan-EduSphere]` for demo tracking values.
- Demo users in `03-FullDummyData.sql` use `REPLACE_WITH_VALID_HASH` placeholder for password hashes. Replace before using interactive login.
- Scripts are designed to be idempotent where practical (MERGE / NOT EXISTS guards).

## Non-Deployment Utility Scripts

The only scripts required for deployment are `01` through `05`.

Other files in this folder (load balancer helpers, phase utilities, and one-off maintenance helpers) are not part of the standard database setup sequence.

### Stage 34.3 Failure/Recovery Utilities

These scripts are operational drills and safeguards; they do not replace the required `01` through `05` deployment flow.

1. `Phase34-BackupRestore-Drill.ps1`
- Performs a backup/restore drill by creating a full backup, running `RESTORE VERIFYONLY`, and restoring into a drill database.
- Produces backup artifacts under `Artifacts/Phase34/Backups` by default.

Example:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase34-BackupRestore-Drill.ps1" -ServerInstance "localhost"
```

2. `Phase34-Rollback-Safe-Deployment.ps1`
- Runs scripts `01 -> 05` with a pre-deployment backup gate for existing databases.
- If deployment fails after backup, it performs automatic rollback restore from the pre-deployment backup.
- Stores rollback backups under `Artifacts/Phase34/RollbackBackups` by default.

Example:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase34-Rollback-Safe-Deployment.ps1" -ServerInstance "localhost"
```

### Stage 36.2 Environment and Secret Readiness Utility

`Phase36-Validate-Environment-Readiness.ps1` validates deployment-readiness configuration for API/Web/BackgroundJobs.

Checks included:
1. Appsettings parity for critical keys across base/development/production files.
2. Effective production secret readiness (file values + environment-variable overrides).
3. Optional fail-fast gate with `-FailOnIssues`.

Baseline report generation:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Validate-Environment-Readiness.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere"
```

Strict gate mode (fails command on findings):
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Validate-Environment-Readiness.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere" -FailOnIssues
```

### Stage 36.3 Deployment and Migration Rehearsal Utility

`Phase36-Deployment-Rehearsal.ps1` validates the required deployment sequence before production go-live.

Checks included:
1. Required script presence for `01 -> 05` and the Stage 34 rollback/drill helpers.
2. Ordered rehearsal reporting for schema, seed, dummy data, maintenance, and post-deployment checks.
3. Optional execution mode for environments that have `sqlcmd` and SQL Server available.

Dry-run report generation:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Deployment-Rehearsal.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere"
```

Strict execution mode (requires reachable SQL Server + `sqlcmd`):
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Deployment-Rehearsal.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere" -Execute -ServerInstance "localhost"
```

The rehearsal script also invokes the Stage 34 rollback/drill utilities in dry-run mode so operators can confirm the backup and recovery playbook is part of the deployment gate.

### Stage 36.4 Security, Reliability, and Performance Gates

`Phase36-Security-Reliability-Performance-Gates.ps1` runs the Stage 36.4 gate set and writes a markdown report under `Artifacts/Phase36/Stage36.4/`.

Checks included:
1. MFA and security hardening regression tests.
2. Dashboard health-visibility coverage for SuperAdmin.
3. Public health snapshot and license-blocking smoke tests.
4. Performance smoke regression coverage for critical portal/API paths.
5. Backup/restore dry-run evidence gate.

Dry-run report generation:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Security-Reliability-Performance-Gates.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere"
```

Execution mode:
```powershell
powershell -ExecutionPolicy Bypass -File "Scripts\Phase36-Security-Reliability-Performance-Gates.ps1" -RepoRoot "C:\path\to\Tabsan-EduSphere" -Execute
```

### Stage 36.5 Operational Runbook and Sign-Off

Stage 36.5 operational handoff references:
1. Deployment and rollback runbook:
	- `Docs/Phase36-Deployment-Rollback-Runbook.md`
2. UAT/SAT approval pack:
	- `UAT-SAT docs/Phase36-Stage36.5-Approval-Pack.md`
3. Stage 36.5 sign-off evidence:
	- `Artifacts/Phase36/Stage36.5/UAT-SAT-Operational-SignOff-20260515.md`
