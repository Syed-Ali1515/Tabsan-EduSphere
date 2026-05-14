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

## Operational Scale Scripts

| File | Purpose |
| --- | --- |
| `3-Phase29-ArchivePolicy.sql` | Dry-run-first retention workflow for operational tables with optional batched cleanup mode (`@ApplyCleanup = 1`). |
| `4-Phase29-IndexMaintenance.sql` | Fragmentation analysis plus plan-first index maintenance (`REORGANIZE`/`REBUILD`) with optional execution mode (`@Execute = 1`). |
| `5-Phase29-CapacityGrowthDashboard.sql` | Capacity dashboard for table-size snapshots, recent row-growth windows, and index usage telemetry. |
| `Phase2-Stage2.1-MultiInstance-Api.ps1` | Starts/stops multiple local API instances for horizontal-scale baseline checks (`/health/instance`, `X-EduSphere-Instance`). |
| `Phase2-Stage2.2-nginx-leastconn.conf.template` | Nginx least-connections upstream template for Stage 2.2 load balancer policy baseline. |
| `Phase2-Stage2.2-LoadBalancer.ps1` | Starts/stops a local Nginx load balancer container using least-connections upstream policy over API instances. |
| `Phase2-Stage2.2-Validate-LB.ps1` | Sends repeated requests via the load balancer and summarizes per-instance distribution using `X-EduSphere-Instance`. |

## Phase 29.3 Operations Runbook

Run scripts in read-only/plan mode first:

```powershell
sqlcmd -S "localhost" -E -d "master" -i "Scripts\3-Phase29-ArchivePolicy.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\4-Phase29-IndexMaintenance.sql"
sqlcmd -S "localhost" -E -d "master" -i "Scripts\5-Phase29-CapacityGrowthDashboard.sql"
```

Execution notes:

- `3-Phase29-ArchivePolicy.sql`: keep `@ApplyCleanup = 0` for dry-run reporting; set to `1` only during approved maintenance windows.
- `4-Phase29-IndexMaintenance.sql`: keep `@Execute = 0` for maintenance planning; set to `1` for controlled maintenance execution.
- `5-Phase29-CapacityGrowthDashboard.sql`: read-only dashboard script for capacity planning and growth trend visibility.
