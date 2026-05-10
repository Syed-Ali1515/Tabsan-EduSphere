# Phase 1 Execution Checklist

## Scope
Phase 1 focuses on database optimization and hot-path response performance for the current load-test endpoints.

## Hot Paths Under Test
- GET /api/v1/dashboard/composition
- GET /api/v1/sidebar-menu/my-visible
- GET /api/v1/notification/inbox?page=0&pageSize=20

Primary API targets:
- src/Tabsan.EduSphere.API/Controllers/DashboardCompositionController.cs
- src/Tabsan.EduSphere.API/Controllers/SidebarMenuController.cs
- src/Tabsan.EduSphere.API/Controllers/NotificationController.cs

Primary service/repository targets:
- src/Tabsan.EduSphere.Application/Notifications/NotificationService.cs
- src/Tabsan.EduSphere.Infrastructure/Repositories/SettingsRepository.cs
- src/Tabsan.EduSphere.Infrastructure/Persistence/ApplicationDbContext.cs

Startup/config targets:
- src/Tabsan.EduSphere.API/Program.cs
- src/Tabsan.EduSphere.API/appsettings.json
- src/Tabsan.EduSphere.API/appsettings.Development.json
- src/Tabsan.EduSphere.API/appsettings.Production.json

## Stage 1.1: Baseline and Query Visibility
### Task 1.1.1
- Capture baseline for 12k and 16k runs using the 50k suite with local cap overrides.

### Task 1.1.2
- Enable EF query timing and slow-query logging thresholds for profiling only (development and staging).

### Exit Criteria
- Baseline summary files exist for both runs.
- Top 10 slow DB queries identified for the three hot endpoints.

## Stage 1.2: Connection Pooling and Resiliency
### Task 1.2.1
- Add explicit SQL connection pool settings in connection strings:
  - Min Pool Size
  - Max Pool Size
  - Connect Timeout
  - Optional MultipleActiveResultSets review

### Task 1.2.2
- Keep SQL transient retry enabled and tune retry count/backoff if needed in Program.cs.

### Task 1.2.3
- Validate no connection exhaustion under 16k cap.

### Exit Criteria
- No pool timeout or login timeout spikes in run window.
- Error rate remains within current thresholds.

## Stage 1.3: Endpoint Query Optimization
### Task 1.3.1 Dashboard composition
- Verify this endpoint remains CPU-only (no unnecessary DB calls).
- If any DB lookups are introduced, cache policy snapshot and role metadata.

### Task 1.3.2 Sidebar my-visible
- Ensure menu fetch uses minimal selected fields and AsNoTracking.
- Ensure role filtering avoids repeated in-memory scans over large trees.
- Add or validate indexes for role access joins:
  - sidebar_menu_role_accesses(MenuItemId, RoleName, IsAllowed)
  - sidebar_menu_items(ParentId, IsActive, DisplayOrder)

### Task 1.3.3 Notification inbox
- Ensure inbox query is fully paged in DB (skip/take before materialization).
- Use AsNoTracking for read-only projection.
- Add or validate indexes for recipient/user lookup:
  - notification_recipients(UserId, IsRead, NotificationId)
  - notifications(IsActive, CreatedAt)

### Exit Criteria
- p95 latency improves or remains stable at same cap.
- Query plan scans reduced on inbox and sidebar paths.

## Stage 1.4: Short-TTL Read Caching
### Task 1.4.1
- Introduce short-lived cache for sidebar visibility tree per role set (5-30 sec).

### Task 1.4.2
- Introduce short-lived cache for inbox badge count (5-15 sec), user-scoped.

### Task 1.4.3
- Keep dashboard composition cacheable per role plus institution policy snapshot.

### Exit Criteria
- Request volume to DB decreases measurably for repeated read hits.
- Cache hit rate tracked and visible in logs/metrics.

## Stage 1.5: Retest Gate and Promotion
### Task 1.5.1
- Re-run 12k, 16k, 20k cap tests with unchanged scenario timings.

### Task 1.5.2
- Compare baseline vs optimized metrics:
  - api_duration p95
  - api_errors rate
  - request throughput
  - DB CPU and waits

### Promotion Rule
- Promote to next cap only if error rate is stable and p95 does not regress materially.

## Suggested Initial Parameter Set
- For local and single generator:
  - LOCAL_VU_CAP: 12000 then 16000 then 20000
- For server-like generator:
  - LOCAL_VU_CAP: 25000+

## Run Commands
- run-50k.bat http://localhost:5181 normal 12000
- run-50k.bat http://localhost:5181 normal 16000
- run-50k.bat http://localhost:5181 normal 20000

## Notes
- Keep raw output mode off unless diagnosing a specific failure.
- Distributed load generation is required for true 50k+ concurrency realism.
- Record each stage result in this checklist before moving to the next stage.
