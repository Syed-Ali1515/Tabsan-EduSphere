# Tabsan EduSphere - Database Scripts (Current Hierarchy)

This folder contains the refreshed database scripts aligned to the current EF Core model, active modules, and present application hierarchy.

## Script Hierarchy

| Order | File | Purpose |
|---|---|---|
| 01 | `01-Schema-Current.sql` | Idempotent full schema generated from current EF migrations. |
| 02 | `02-Seed-Core.sql` | Core seed: roles, modules, module status, portal settings, report definitions, and base sidebar menus. |
| 03 | `03-FullDummyData.sql` | Full dummy data for demos: multi-department hierarchy, users, programs, courses, offerings, enrollments, assignments, submissions, attendance, and notifications. |
| 04 | `04-Maintenance-Indexes-And-Views.sql` | Safe maintenance indexes + operational views for current workload paths. |
| 05 | `05-PostDeployment-Checks.sql` | Validation queries for migration/version and core entity counts. |

## Recommended Execution

Run step-by-step:

```powershell
sqlcmd -S "localhost" -E -d "TabsanEduSphere" -i "Scripts\01-Schema-Current.sql"
sqlcmd -S "localhost" -E -d "TabsanEduSphere" -i "Scripts\02-Seed-Core.sql"
sqlcmd -S "localhost" -E -d "TabsanEduSphere" -i "Scripts\03-FullDummyData.sql"
sqlcmd -S "localhost" -E -d "TabsanEduSphere" -i "Scripts\04-Maintenance-Indexes-And-Views.sql"
sqlcmd -S "localhost" -E -d "TabsanEduSphere" -i "Scripts\05-PostDeployment-Checks.sql"
```

## Notes

- `01-Schema-Current.sql` is generated from the current migration chain and can be safely rerun.
- Demo users in `03-FullDummyData.sql` use `REPLACE_WITH_VALID_HASH` placeholder for password hashes. Replace before using interactive login.
- Scripts are designed to be idempotent where practical (MERGE / NOT EXISTS guards).

## Operational Scale Scripts

| File | Purpose |
|---|---|
| `Phase2-Stage2.1-MultiInstance-Api.ps1` | Starts/stops multiple local API instances for horizontal-scale baseline checks (`/health/instance`, `X-EduSphere-Instance`). |
| `Phase2-Stage2.2-nginx-leastconn.conf.template` | Nginx least-connections upstream template for Stage 2.2 load balancer policy baseline. |
| `Phase2-Stage2.2-LoadBalancer.ps1` | Starts/stops a local Nginx load balancer container using least-connections upstream policy over API instances. |
| `Phase2-Stage2.2-Validate-LB.ps1` | Sends repeated requests via the load balancer and summarizes per-instance distribution using `X-EduSphere-Instance`. |
