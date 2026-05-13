# Tabsan EduSphere Development Plan (ASP.NET)

**Version:** 1.4  
**Date:** 8 May 2026  
**Based On:** PRD v1.33, Modules v1.3, Database Schema v1.2

## Institute Parity Stage Documentation Rule (2026-05-13)

For each completed stage in `Docs/Institute-Parity-Issue-Fix-Phases.md`, this plan must be updated with:
- `Implementation Summary`
- `Validation Summary`

Validation summaries must include at minimum:
- commands/test suites executed,
- role/institute behavior checks,
- regression confirmation status.

---

## Execution Updates

### 2026-05-13 - Institute Parity Stage 1.1 (Execution Snapshot)
- Completed Phase 1 Stage 1.1 institute model normalization.
- Implementation Summary:
  - added canonical `InstitutionType` to `Department` domain entity,
  - added EF persistence + default + institute index on departments,
  - added migration `20260513121000_Phase1Stage11DepartmentInstitutionType`,
  - updated department API contracts and policy enforcement to validate institution type against active license policy.
- Validation Summary:
  - `dotnet build Tabsan.EduSphere.sln` passed,
  - targeted unit validation (`SecurityValidationTests`) passed `4/4`,
  - verified backward-compatible defaults/optionals for existing department create/update flows.
- Stage status: Stage 1.1 completed.
- Phase status: Phase 1 in progress (next: Stage 1.2).

### 2026-05-13 - Institute Parity Stage 0.4 (Execution Snapshot)
- Completed Phase 0 exit criteria and readiness sign-off.
- Implementation Summary:
  - consolidated Stage 0.1/0.2/0.3 outputs into prioritized remediation backlog,
  - confirmed traceability from reported issues to remediation phases/stages.
- Validation Summary:
  - verified all Phase 0 baseline artifacts are complete and internally consistent,
  - verified no blocker preventing transition to Phase 1 execution.
- Stage status: Stage 0.4 completed.
- Phase status: Phase 0 completed.

### 2026-05-13 - Institute Parity Stage 0.3 (Execution Snapshot)
- Completed report failure inventory baseline with required root-cause tagging.
- Implementation Summary:
  - inventoried historical and current report failures across result/report center/analytics surfaces,
  - classified outcomes into query logic, filter/join gaps, scoping mismatch, and dummy-data absence,
  - mapped each item to remediation stage ownership.
- Validation Summary:
  - validated evidence against issue docs, report controller guards, report repository query paths, and report integration tests,
  - confirmed historical critical failures are resolved,
  - confirmed remaining parity risks are filter-propagation and data completeness oriented.
- Stage status: Stage 0.3 completed.

### 2026-05-13 - Institute Parity Stage 0.2 (Execution Snapshot)
- Completed role and institute access matrix baseline for parity-scope modules.
- Implementation Summary:
  - mapped role-based view/create/edit/deactivate/export coverage by module,
  - mapped institute-scope enforcement basis (policy flags vs department/offering/ownership scope),
  - documented enforcement gaps for explicit institute checks.
- Validation Summary:
  - source-level verification completed on policy, admin-user, analytics/report, academic, assessment, lifecycle, and payment controller/service surfaces,
  - confirmed no runtime or schema mutation in this stage,
  - queued identified gaps for Phase 1-4 remediation stages.
- Stage status: Stage 0.2 completed.

### 2026-05-13 - Institute Parity Stage 0.1 (Execution Snapshot)
- Completed baseline module parity audit and dependency mapping for School/College/University execution planning.
- Implementation Summary:
  - audited API controllers/routes for all parity-scope modules,
  - mapped service/repository dependencies and scope-guard patterns,
  - captured University-default hotspots requiring normalization in next stages.
- Validation Summary:
  - static evidence collected from controller/service/infrastructure/web/db source scans,
  - confirmed broad module coverage exists; no runtime code changes in this stage,
  - flagged residual University-centric defaults for Stage 0.2 onward.
- Stage status: Stage 0.1 completed.

### 2026-05-12 - Institution License Validation Phase 7 (Execution Snapshot)
- Completed SuperAdmin full-access matrix execution.
- Captured evidence in `Artifacts/Phase7/SuperAdmin/20260512-151302`.
- Validated CRUD and activation/deactivation operations:
  - Department create/update/deactivate.
  - Admin-user create/deactivate/reactivate.
- Validated policy mode-switch and cross-institution privileged visibility across School, College, and University modes.
- Matrix summary: `35` checks executed, `35` passed, `0` failed.
- Phase 7 completed; Institution License Validation plan closed.

### 2026-05-12 - Institution License Validation Phase 6 (Execution Snapshot)
- Completed role-boundary validation for institution-scoped report access.
- Captured evidence in `Artifacts/Phase6/Access/20260512-150824`.
- Verified report export scope enforcement behavior:
  - Admin assigned department succeeds; non-assigned department denied; missing scope rejected.
  - Faculty assigned offering succeeds; non-assigned offering denied; missing scope rejected.
  - Student remains blocked from operational report exports.
- Verified allowed student read surfaces continue to work (`/reports`, `/dashboard/context`).
- Phase 6 completed.

### 2026-05-12 - Institution License Validation Phase 5 (Execution Snapshot)
- Implemented explicit per-user institution assignment for manual admin creation and CSV import.
- Added persistence and migration support:
  - nullable `users.InstitutionType` column via EF migration `AddUserInstitutionTypeAssignment`.
- Added policy-aware validation behavior:
  - assignment to disabled institution type is rejected,
  - assignment to enabled institution type is accepted and persisted.
- Updated portal create-admin flow and import templates/documentation to include optional institution assignment.
- Captured evidence in `Artifacts/Phase5/Api` (`20260512-144212` set).
- Phase 5 completed.

### 2026-05-12 - Institution License Validation Phase 4 (Execution Snapshot)
- Completed mode-role validation for School, College, and University across SuperAdmin, Admin, Faculty, and Student.
- Captured execution evidence in `Artifacts/Phase4/ModeRole/20260512-142021` including:
  - policy, labels, capability matrix,
  - dashboard composition context,
  - report catalog,
  - scoped report data and CSV export checks,
  - negative authorization probes.
- Scoped report/export behavior validated:
  - SuperAdmin/Admin/Faculty succeed with valid scope filters,
  - Student operational report endpoints remain denied (`403`).
- Role/mode dashboard composition and vocabulary results align with institution-policy flags and report scope rules.
- Phase 4 completed.

### 2026-05-12 - Institution License Validation Phase 3 (Execution Snapshot)
- Completed mixed-mode license validation for all planned combinations:
  - School + College
  - School + University
  - College + University
  - School + College + University
- Captured evidence per combination for:
  - license upload,
  - institution policy,
  - labels,
  - portal capability matrix row union,
  - progression evaluation by institution type,
  - persisted `portal_settings` policy keys.
- Validated union behavior:
  - matrix rows expand according to enabled institution flags,
  - persisted DB keys match runtime policy output.
- Observed current label-resolution behavior:
  - School+College uses School vocabulary,
  - any combination containing University uses University vocabulary.
- Known tooling caveat persisted:
  - sequential uploads require clearing `consumed_verification_keys` because generated licenses reuse verification-key material.
- Phase 3 completed.

### 2026-05-12 - Institution License Validation Phase 2 (Execution Snapshot)
- Completed end-to-end mode validation for School, College, and University after applying policy persistence fix.
- Applied service fix:
  - `InstitutionPolicyService.SavePolicyAsync` now commits settings repository changes so policy flags persist.
- Phase 2 evidence captured for each mode:
  - successful license activation,
  - policy flags,
  - labels vocabulary,
  - portal capability matrix rows,
  - progression evaluation result by institution type,
  - DB policy-key confirmation in `portal_settings`.
- Results summary:
  - School: Grade/Promotion flow active, school-only matrix rows.
  - College: Year/Progression flow active, college-only matrix rows.
  - University: Semester/GPA flow active, university-only matrix rows.
- Known tooling caveat noted in validation:
  - generated licenses currently reuse verification key; sequential mode validation required resetting `consumed_verification_keys` between activations.
- Phase 2 completed.

### 2026-05-12 - Institution License Validation Phase 1 (Execution Snapshot)
- Ran Phase 1 endpoint validation using SuperAdmin credentials and HTTPS API host.
- Baseline observed:
  - license status: `Invalid`
  - license details: `None`
  - institution policy: `University=true`, `School=false`, `College=false`
- Attempted `.tablic` upload from `tools/Tabsan.Lic/License`.
- Initial upload failure root cause: DB save failed due legacy non-null `license_state` columns (`InstitutionScope`, `ExpiryType`) without defaults.
- Applied environment-level SQL defaults for those legacy columns and retried upload.
- Retry succeeded with `License activated successfully`.
- Post-upload state: `license status=Active`, `license details=Active (Yearly)`, policy remained `University=true`, `School=false`, `College=false`.
- Final module restriction validation via `GET /api/v1/portal-capabilities/matrix` confirmed `school=false`, `college=false`, `university=true` with no School/College-enabled rows.
- Phase 1 completed.

### 2026-05-12 - Institution License Validation Plan Added
- Added phased validation baseline in `Docs/Institution-License-Validation-Phases.md`.
- Defined 7 validation phases covering:
  - license-to-institution binding,
  - student lifecycle routing by School/College/University,
  - mixed-mode feature/configuration union,
  - institution-scoped charts/tables/menus/reports,
  - institution assignment in create/import user flows,
  - role-based institution access boundaries,
  - SuperAdmin full-permission verification.
- Each phase now has mandatory deliverables: `Implementation Summary`, `Validation Summary`, and `Status of Checks Done`.
- Each phase completion requires docs synchronization and git synchronization (commit, pull, push).

### 2026-05-11 — Phase 10 Complete
- Stage 10.1: Added a parameterized progressive gate runner for 10k -> 20k -> 50k -> 80k -> 100k progression plus an extended high-tier plan.
- Stage 10.2: Added bottleneck classification heuristics that report the first likely limiter from each gate summary.
- Stage 10.3: Added a retest loop so targeted fixes can be revalidated against the same gate before moving forward.
- Validation: PowerShell syntax check on `tests/load/run-phase10-progressive.ps1` passed; editor diagnostics on `tests/load/k6-phase10-progressive.js` reported no errors.

### 2026-05-11 — Phase 9 Complete
- Stage 9.1: Added OpenTelemetry metrics publishing with Prometheus scraping support in the API host.
- Stage 9.2: Added rolling request-latency capture with `/health/observability` snapshots for p50/p95/p99 tracking.
- Stage 9.3: Added database, CPU, memory, network, and error-rate health checks for continuous runtime monitoring.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.

### 2026-05-11 — Phase 8 Complete
- Stage 8.1: Added `InfrastructureTuning:AutoScaling` controls and startup validation for replica bounds in API, Web, and BackgroundJobs.
- Stage 8.2: Added `InfrastructureTuning:HostLimits` controls with thread-pool minimum tuning and API/Web Kestrel concurrent connection settings.
- Stage 8.3: Added `InfrastructureTuning:NetworkStack` controls with HTTP/2 stream tuning and outbound HTTP handler connection limits.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.

### 2026-05-11 — Phase 7 Complete
- Stage 7.1: Added queue offloading for account-security unlock/reset transactional emails so request handlers enqueue background work.
- Stage 7.2: Added queue platform integration with startup-configurable `QueuePlatform:Provider` supporting in-memory and RabbitMQ account-security queue processing.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched files reported no errors.

### 2026-05-11 — Phase 6 Complete
- Stage 6.1: Added short-TTL distributed caching in `LibraryService` for safe external loan API lookups.
- Stage 6.2: Added channel-level circuit-breaker controls in `ResilientOutboundIntegrationGateway` with configurable threshold/open durations.
- Stage 6.3: Replaced blocking `.Result` reads in `GradebookService.GetGradebookAsync(...)` with awaited async results.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched files reported no errors.

### 2026-05-11 — Phase 5 Complete
- Stage 5.1: Reworked the 50k/100k/1m/5m k6 scripts to `ramping-arrival-rate` workloads with randomized think-time.
- Stage 5.2: Added distributed generator shard controls (`GENERATOR_TOTAL`, `GENERATOR_INDEX`) and runner support for multi-machine execution.
- Stage 5.3: Standardized summary-first output (`--quiet`, summary-export + compact summaries) and kept heavy raw output as explicit diagnostics-only mode.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched load scripts reported no errors.

### 2026-05-11 — Phase 4 Complete
- Stage 4.1: Added short-TTL distributed cache in `AnalyticsService` for expensive report reads (`performance`, `attendance`, `assignments`, `quizzes`).
- Stage 4.2: Added configurable static-asset cache headers in Web startup via `UseStaticFiles(...OnPrepareResponse...)` and `StaticAssetCaching` appsettings keys.
- Stage 4.3: Kept cache scope constrained to expensive shared-safe analytics reads and static file responses only.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched files reported no errors.

### 2026-05-11 — Phase 3 Stage 3.3
- Added Kestrel transport tuning in API and Web startup for keep-alive timeout, request-header timeout, server-header suppression, and HTTP/2 ping tuning.
- Preserved Brotli/Gzip response compression with Fastest settings in both hosts.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on the updated startup files reported no errors.

### 2026-05-11 — Phase 3 Stage 3.2
- Replaced `ContinueWith` wrappers with direct async `await` returns in hot repository methods for timetable, settings, quiz, and building/room reads.
- Kept the data-access layer fully async on the high-traffic query paths that still fed portal screens and scheduling/reporting flows.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched repository files reported no errors.

### 2026-05-11 — Phase 3 Stage 3.1
- Added aggregated dashboard-context endpoint in API (`GET /api/v1/dashboard/context`) to reduce ModuleComposition screen round-trips.
- Updated portal ModuleComposition flow to consume the single dashboard-context payload instead of three API calls.
- Added Web client support for the aggregated dashboard-context response model.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`; syntax checks on touched files reported no errors.

### 2026-05-11 — Phase 2 Stage 2.3
- Hardened API startup to require Redis-backed distributed cache outside Development/Testing (`ScaleOut:RedisConnectionString`) so cache state stays stateless across nodes.
- Hardened Web startup to require shared data-protection key ring outside Development/Testing (`ScaleOut:SharedDataProtectionKeyRingPath`) so auth cookies stay decryptable across nodes.
- Preserved Development/Testing fallback behavior for local iteration.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.

### 2026-05-11 — Phase 2 Stage 2.2
- Added Nginx least-connections baseline template for API horizontal routing (`Scripts/Phase2-Stage2.2-nginx-leastconn.conf.template`).
- Added Stage 2.2 load balancer control script to start/stop local balancer container with generated upstream members (`Scripts/Phase2-Stage2.2-LoadBalancer.ps1`).
- Added Stage 2.2 distribution validator script to sample request spread per instance (`Scripts/Phase2-Stage2.2-Validate-LB.ps1`).
- Updated scripts catalog with Stage 2.2 entries (`Scripts/README.md`).
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.

### 2026-05-11 — Phase 2 Stage 2.1
- Added API per-instance identity bootstrap in startup using `ScaleOut:InstanceId` with machine/process fallback for horizontal node uniqueness.
- Added optional node telemetry header emission (`X-EduSphere-Instance`) controlled by `ScaleOut:ExposeInstanceHeader`.
- Added node probe endpoint `GET /health/instance` for load balancer verification across scaled API instances.
- Added operational script `Scripts/Phase2-Stage2.1-MultiInstance-Api.ps1` to launch/stop multiple local API nodes for baseline scale testing.
- Validation:
  - `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.
  - `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj -v minimal` encountered expected file-lock warnings/errors due running API process (PID 35564).

### 2026-05-11 — Phase 1 Stage 1.4
- Added short-TTL dashboard composition caching in `DashboardCompositionService.GetWidgets(...)` with role + institution policy cache keys.
- Added short-TTL sidebar read caching in `SidebarMenuService` for top-level and role-visible menus, plus version bump invalidation on sidebar mutations.
- Added short-TTL notification read caching in `NotificationService` for inbox and unread badge responses, plus version bump invalidation on send/deactivate/mark-read mutations.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed `130/130`.

### 2026-05-11 — Phase 1 Stage 1.3
- Optimized notification inbox read path by introducing repository-level no-tracking option and using it in `NotificationService.GetInboxAsync`.
- Optimized unread badge count query by removing unnecessary Include loading in `NotificationRepository.GetUnreadCountAsync`.
- Optimized sidebar read paths in `SettingsRepository`:
  - no-tracking for top-level/sub-menu/visible-menu queries,
  - split-query pattern on include-heavy sidebar graph reads.
- Validation plan: re-run 12k and 16k load caps and compare p95 latency + error-rate against Stage 1.2 baseline.

### 2026-05-11 — Phase 1 Stage 1.2
- Tuned SQL connection pooling settings for API runtime profiles:
  - `appsettings.json` and `appsettings.Development.json` now include `Min Pool Size=20;Max Pool Size=500;Connect Timeout=30`.
  - `appsettings.Production.json` connection string placeholder now includes guidance values `Min Pool Size=50;Max Pool Size=800;Connect Timeout=30`.
- Objective: reduce connection churn and timeout spikes during high-concurrency load-test stages.
- Validation plan: rerun 12k and 16k caps and compare p95 latency/error deltas before Stage 1.3 query tuning.

### 2026-05-10 — Phase 33 Stage 33.3
- Added DataAnnotations-based validation to auth/admin DTOs:
  - login, refresh, change-password, and forced-password-change requests now enforce required and length-constrained inputs,
  - admin user create/update requests now enforce required username/email/password constraints.
- Added `SecurityValidationTests` for executable coverage of the hardened validation paths.
- Validation:
  - `dotnet build Tabsan.EduSphere.sln` passed.
  - `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj --filter "FullyQualifiedName~SecurityValidationTests"` passed `4/4`.
  - `dotnet test Tabsan.EduSphere.sln --no-build` passed `234/234`.

### 2026-05-10 — Phase 33 Stage 33.2
- Hardened runtime hosting behavior in API/Web:
  - added config-driven reverse-proxy trust options (`ReverseProxy:Enabled`, `KnownProxies`, `ForwardLimit`, `RequireHeaderSymmetry`),
  - restricted forwarded-header middleware activation to configured reverse-proxy mode,
  - added startup guardrails for unsafe production startup conditions.
- API-specific hardening:
  - startup guard requires non-empty `AppSettings:CorsOrigins` outside Development/Testing.
- Web-specific hardening:
  - removed localhost fallback behavior in login API base URL resolution,
  - removed localhost default from portal API connection model.
- Validation:
  - `dotnet build Tabsan.EduSphere.sln` passed.
  - `dotnet test Tabsan.EduSphere.sln --no-build` passed `230/230`.

### 2026-05-10 — Phase 33 Stage 33.1
- Re-scoped Phase 33 to `Hosting Configuration and Security Hardening` using `Docs/Refactoring-Hosting-Security.md` as execution baseline.
- Added explicit config-load bootstrapping in API/Web/BackgroundJobs startup paths:
  - `appsettings.json`,
  - `appsettings.{Environment}.json`,
  - environment variables.
- Added startup setting guards for required values (`DefaultConnection`, `EduApi:BaseUrl`) and placeholder-rejection for BackgroundJobs base connection string.
- Aligned `AppSettings` metadata in environment appsettings files for API/Web/BackgroundJobs portability.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed.

### 2026-05-10 — Phase 32 Stage 32.5
- Added `CredentialVerificationIntegrationTests` to preserve credential/run-command guardrails:
  - deterministic smoke users are provisioned for SuperAdmin/Admin/Faculty/Student,
  - `POST /api/v1/auth/login` is verified for each role,
  - login response token + expected role contract is asserted.
- Validation command (targeted):
  - `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~CredentialVerificationIntegrationTests"`
- Validation: targeted integration tests passed `4/4`.

### 2026-05-10 — Phase 32 Stage 32.4
- Added report-center visibility/link guardrails in sidebar integration tests:
  - `report_center` is visible for Admin, Faculty, and Student,
  - report-center-visible roles can successfully load report catalog data,
  - student sidebar visibility baseline now includes `report_center`.
- Hardened sidebar seed role-access logic to self-heal existing role rows by applying expected allow/deny values during seeding.
- Synced SQL minimal seed role-access matrix so student includes `report_center`.
- Validation: targeted integration tests passed `12/12`.

### 2026-05-10 — Phase 32 Stage 32.3
- Added `SidebarMenuIntegrationTests.SetRoles_AllSeededMenus_AreAssignable` to preserve cross-phase Sidebar Settings guardrails:
  - every seeded top-level and sub-menu key accepts role-assignment updates,
  - menu-configuration actions remain operational for full sidebar inventory,
  - existing role visibility and system-menu protections remain intact.
- Validation: targeted integration tests passed `9/9`.

### 2026-05-10 — Phase 32 Stage 32.2
- Added `ReportExportsIntegrationTests` to preserve cross-phase report-export guardrails:
  - anonymous requests to export endpoints remain blocked,
  - attendance/result/assignment/quiz export routes return expected media types,
  - export responses preserve attachment filename contracts,
  - export payloads remain non-empty for downloadable output integrity checks.
- Validation: targeted integration tests passed `13/13`.

### 2026-05-10 — Phase 32 Stage 32.1
- Added `ReportCatalogIntegrationTests` to preserve cross-phase report-center guardrails:
  - report catalog remains role-scoped,
  - seeded report keys remain present for privileged roles,
  - student catalog remains restricted to student-allowed definitions,
  - each catalog key maps to a live report endpoint route (no 404 regressions).
- Validation: targeted integration tests passed `8/8`.

### 2026-05-10 — Phase 31 Stage 31.3
- Added Stage 31.3 load certification script `tests/load/k6-certification-bands.js` with executable band profiles for:
  - up-to-10k
  - 10k-100k
  - 100k-500k
  - 500k-1m
- Added Stage 31.3 recovery-smoke script `tests/load/recovery-smoke.ps1` for node/service restart recovery validation.
- Updated `tests/load/README.md` with Stage 31.3 run commands and threshold tables.
- Added Stage 31.3 certification document: `Docs/Phase31-Stage31.3-Performance-Reliability-Certification.md`.
- Validation: full automated tests passed `201/201`.

### 2026-05-10 — Phase 31 Complete
- Stage 31.1, Stage 31.2, and Stage 31.3 are complete.

### 2026-05-10 — Phase 31 Stage 31.2
- Added explicit audit-log emission to sensitive control-plane mutation endpoints in `FeatureFlagsController`, `TenantOperationsController`, and `InstitutionPolicyController`.
- Added Stage 31.2 integration suite (`Phase31Stage2SecurityHardeningTests`) for:
  - endpoint authorization guard coverage,
  - anonymous endpoint whitelist enforcement,
  - audit log coverage validation for sensitive actions.
- Added Stage 31.2 artifact document: `Docs/Phase31-Stage31.2-Security-Hardening.md`.
- Validation: targeted tests passed `4/4`; full automated tests passed `201/201`.

### 2026-05-10 — Phase 31 Stage 31.1
- Added Stage 31.1 regression matrix unit suite (`Phase31Stage1RegressionMatrixTests`) with 24 scenario combinations across institution mode, role, and license profile states.
- Added tenant isolation baseline verification using isolated settings repositories.
- Added matrix artifact and traceability document: `Docs/Phase31-Stage31.1-Regression-Matrix.md`.
- Validation: targeted tests passed `25/25`; full automated tests passed `197/197`.

### 2026-05-10 — Phase 30 Stage 30.3
- Added feature-flag DTOs/contracts and `IFeatureFlagService` for rollout/rollback operations.
- Implemented `FeatureFlagService` using `portal_settings` persistence with default flags and batch rollback disable flow.
- Added `FeatureFlagsController` (`api/v1/feature-flags`) for SuperAdmin list/get/save/rollback workflows.
- Added `tenant-operations.write` guard to tenant write endpoints for rollback-safe shutdown behavior.
- Added rollout and rollback runbook doc: `Docs/Phase30-Stage30.3-Reliability-Rollback-Runbook.md`.
- Added focused Stage 30.3 unit tests (`Phase30Stage3Tests`).
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed `172/172`.

### 2026-05-10 — Phase 30 Complete
- Completed Stage 30.1 (integration gateway), Stage 30.2 (tenant/subscription operations), and Stage 30.3 (reliability/rollback controls).

### 2026-05-10 — Phase 30 Stage 30.2
- Added tenant/subscription operations DTOs and service contract (`ITenantOperationsService`) for onboarding template, subscription plan controls, and tenant profile settings.
- Implemented `TenantOperationsService` in Application settings services, backed by `portal_settings` key-value persistence.
- Added SuperAdmin API controller `TenantOperationsController` with `GET/PUT` endpoints for onboarding-template, subscription-plan, and tenant-profile operations.
- Registered tenant operations service in API DI.
- Added focused unit tests (`Phase30Stage2Tests`) for defaults and persistence round-trip behavior.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed `169/169`.

### 2026-05-10 — Phase 30 Stage 30.1
- Added centralized outbound integration gateway contracts and runtime (`IOutboundIntegrationGateway`, `ResilientOutboundIntegrationGateway`) with channel-based retry/timeout policies.
- Added distributed-cache-backed dead-letter capture for terminal outbound integration failures.
- Routed existing outbound integration flows through gateway policy execution:
  - SMTP email delivery
  - In-app support/announcement push notifications
  - External library loan API calls
- Added communication integration diagnostics endpoints for gateway policies and dead-letter inspection.
- Added focused unit tests covering retry success, timeout dead-letter behavior, and default policy fallback.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed `166/166`.

### 2026-05-10 — Phase 29 Stage 29.3
- Added `Scripts/3-Phase29-ArchivePolicy.sql` with retention policy windows, dry-run visibility, and optional batched cleanup execution.
- Added `Scripts/4-Phase29-IndexMaintenance.sql` with fragmentation threshold planning and optional reorganize/rebuild execution.
- Added `Scripts/5-Phase29-CapacityGrowthDashboard.sql` for capacity footprint and recent growth telemetry.
- Updated `Scripts/README.md` with Stage 29.3 operations run commands.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed `162/162`.

### 2026-05-10 — Phase 29 Complete
- Stage 29.1 (index baseline), Stage 29.2 (heavy-list pagination), and Stage 29.3 (lifecycle maintenance operations) are complete.

### 2026-05-10 — Phase 29 Stage 29.2 Slice 3
- Added server-side pagination for payment receipt lists across repository, application service, API controller, portal client, and portal page.
- Replaced previous unbounded payment list paths with `page` and `pageSize` aware queries plus total-count metadata.
- Preserved the admin student-filtered Payments workflow while adding previous/next navigation.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed `162/162`.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 3
- Added provider-based file/media storage abstraction in API (`IMediaStorageService`, `LocalMediaStorageService`, `MediaStorageOptions`).
- Added `MediaStorage` configuration to API appsettings (provider mode, local root path, key prefix, optional public base URL).
- Migrated payment-proof upload flow to storage abstraction so database records persist storage object keys instead of hard-coded file system paths.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 2
- Moved `IMediaStorageService` into the Application layer to support provider-backed media persistence from both controllers and application services.
- Extended local provider with read-by-key support.
- Migrated graduation certificate generation/download to provider-backed storage with legacy `/certificates/*` fallback for existing records.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 3
- Migrated `LicenseController.Upload` temporary file workflow to provider-backed media storage save/read/delete operations.
- Added `LicenseValidationService.ActivateFromBytesAsync` for path-independent license verification and activation.
- Extended storage contract with delete support to clean temporary upload objects.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 4
- Added `AddConfiguredMediaStorage` to register storage provider implementation from configuration.
- Added `BlobMediaStorageService` as an object-storage style adapter behind `IMediaStorageService`.
- Added `BlobRootPath` storage configuration key in API environment appsettings.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 5
- Migrated portal logo upload in `PortalSettingsController` to provider-backed save flow through `IMediaStorageService`.
- Added `GET /api/v1/portal-settings/logo-files/{**storageKey}` endpoint to stream persisted logo bytes for branding display paths.
- Added category guardrails so only `portal-branding/logo` keys are anonymously streamable.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 6
- Extended `IMediaStorageService` with temporary read URL generation (`GenerateTemporaryReadUrlAsync`) for signed URL ready media reads.
- Added temporary signed URL generation support in both local and blob provider implementations.
- Updated portal logo-file endpoint to use provider temporary URL redirect when available, then fall back to in-process byte streaming.
- Added `SignedUrlSecret` placeholders in API appsettings files.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 7
- Enforced local signed URL validation (`exp`/`sig`) in `PortalSettingsController.GetLogoFile` when `MediaStorage:SignedUrlSecret` is configured.
- Added unsigned legacy URL compatibility redirect to short-lived local signed logo URLs.
- Added fixed-time signature comparison and expiry validation for local signed reads.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 8
- Added authenticated certificate streaming endpoint `GET /api/v1/graduation/certificate-files/{**storageKey}`.
- Updated `GET /api/v1/graduation/{id}/certificate` to redirect to temporary provider URLs when available or signed local certificate URLs otherwise.
- Added local signed URL validation (`exp`/`sig`) for certificate-file reads when signing is configured.
- Preserved legacy `/certificates/*` compatibility for existing path-based records.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 9
- Extended `IMediaStorageService` with metadata lookup support (`GetMetadataAsync`) and added content type/length fields on save results.
- Implemented provider metadata resolution in `LocalMediaStorageService` and `BlobMediaStorageService`.
- Updated portal logo and certificate streaming endpoints to use provider metadata for response content type selection.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.3 Slice 10
- Extended the storage contract with SHA-256 hash and optional download filename metadata on save and metadata reads.
- Implemented sidecar metadata persistence in local/blob storage providers.
- Updated certificate generation and certificate-file streaming so filename-aware downloads survive signed local and redirect-first reads.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Complete
- Stage 28.1, Stage 28.2, and Stage 28.3 are complete.
- Phase 28 delivered multi-node readiness, distributed cache/background offload, and provider-backed media hardening without schema changes.

### 2026-05-10 — Phase 29 Stage 29.1
- Added composite indexes for hot student/user/status recency queries in the EF model.
- Generated migration `20260509155457_20260510_Phase29_IndexBaseline`.
- Documented that the current schema has no `InstitutionId`, `YearId`, or `GradeId` columns yet, so Stage 29.1 targeted the active query contracts instead.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 29 Stage 29.2 Slice 1
- Added server-side pagination for helpdesk ticket listing across the repository, application service, API controller, portal client, and portal page.
- Replaced the previous unbounded helpdesk list path with `page` and `pageSize` aware queries.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 29 Stage 29.2 Slice 2
- Added server-side pagination for graduation application list endpoints across repository, application service, API controller, portal client, and portal pages.
- Replaced previous unbounded graduation list paths with `page` and `pageSize` aware queries plus total-count metadata.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-09 — Phase 28 Stage 28.1 Complete
- Completed **API and App Tier Scaling** as the first stage of the scalability architecture roadmap.
- Replaced Web session-backed connection/auth state with protected-cookie storage to keep portal nodes stateless across a load-balanced deployment.
- Added optional shared data-protection key-ring configuration to support multi-node cookie decryption.
- Enabled Brotli/Gzip response compression in API and Web, and configured JSON payload shaping to omit null values.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **160/160**.

### 2026-05-09 — Phase 28 Stage 28.2 Foundation Batch
- Added optional Redis-backed distributed cache registration in API startup, with distributed-memory fallback for local or single-node environments.
- Shifted module entitlement resolution and report catalog retrieval onto the shared cache layer for scale-out reuse.
- Added a hosted notification fan-out worker so large recipient batches no longer block the API request path.
- Added focused unit tests for deferred fan-out behavior.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-10 — Phase 28 Stage 28.2 Completion
- Added queue-backed result-summary export jobs in `ReportController` with dedicated status and download endpoints.
- Added queue-backed result publish-all jobs in `ResultController` for asynchronous recalculation-heavy publishing.
- Added `ResultPublishJobWorker` and `ReportExportJobWorker` hosted services with distributed-cache-backed job state/payload storage.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; automated tests passed **162/162**.

### 2026-05-05 — Phase 1 Remediation Restart (Batch 1)
- Re-opened Phase 1 workstream to address Observed-Issues Phase 1 items.
- Implemented role-access remediation for offerings used by Assignments, Attendance, Results, and Quizzes page data loads.
- Applied SuperAdmin visibility correction for Report Center catalog retrieval.
- Applied sidebar cleanup: removed module-settings route mapping in dynamic menu and removed brand-header hyperlink behavior.
- Applied student lifecycle mapping fix to prevent empty GUID promote requests from semester-student payload mapping.

### 2026-05-05 — Phase 1 Remediation Restart (Batch 2)
- Completed Stage 1.4 script/runtime cleanup for Module Settings removal.
- Removed `module_settings` seeding from runtime database seeder and both SQL seed scripts.
- Added legacy cleanup to disable role access and soft-delete existing `module_settings` menu rows.
- Removed remaining static SuperAdmin sidebar link to Module Settings.
- Updated sidebar integration test expected SuperAdmin menu count.

### 2026-05-05 — Phase 1 Remediation Restart (Batch 3)
- Completed Stage 1.3 Result Summary runtime exception fix.
- Corrected report query ordering in repository to SQL-level ordering prior to projection, eliminating EF translation failure.
- Expanded report data/export endpoint role gates to `SuperAdmin,Admin,Faculty`.
- Validated SuperAdmin result summary endpoint response with non-empty records.

### Next Execution Target
- Continue **next phase planning/execution per roadmap directive**.

### 2026-05-08 — Phase 13 Global Search Complete (commit 00b7b64)
- **Stage 13.1 — Cross-Entity Search API:**
  - `ISearchRepository` (Application/Interfaces) + `SearchRepository` (Infrastructure): EF LINQ join queries against `StudentProfiles`, `Users`, `Courses`, `CourseOfferings`, `Semesters`, `Departments`, `Enrollments`. No new migration.
  - `ISearchService` + `SearchService`: role-scoped orchestration (SuperAdmin → all; Admin → assigned depts; Faculty → own dept + offerings; Student → enrolled offerings).
  - `SearchController`: `GET /api/v1/search?q={term}&limit={n}` — all authenticated roles; extracts callerId + role from JWT claims; validates q ≥ 2 chars; clamps limit 1–50.
  - `SearchDTOs`: `SearchRequest`, `SearchResultItem`, `SearchResponse` records.
- **Stage 13.2 — Portal Search Bar:**
  - Global search `<input>` in `_Layout.cshtml` header (visible all pages when connected).
  - Vanilla JS typeahead: debounced 300 ms fetch to `/Portal/SearchTypeahead`; renders top 5 + "See all" link.
  - Full results page `Search.cshtml`: Bootstrap nav-tabs per category with `_SearchResultsList.cshtml` partial.
  - `PortalController` — `Search()` + `SearchTypeahead()` actions.
- Validation: **0 build errors; 78/78 tests passed**

### 2026-05-07 — Phase 12 Academic Calendar System Complete (commit 6e89af1)
- **Stage 12.1 — Semester Timeline View:**
  - `AcademicCalendar` portal page (all roles): semester dropdown filter, days-remaining badges, link to manage page for Admin/SuperAdmin.
- **Stage 12.2 — Key Deadlines Management:**
  - New domain entity `AcademicDeadline` (`AuditableEntity`): `SemesterId`, `Title`, `Description`, `DeadlineDate`, `ReminderDaysBefore`, `IsActive`, `LastReminderSentAt`.
  - `IAcademicDeadlineRepository` + `AcademicDeadlineRepository` (EF Core, table `academic_deadlines`, soft-delete filter).
  - `IAcademicCalendarService` + `AcademicCalendarService` (CRUD + reminder dispatch).
  - EF migration `20260507_Phase12AcademicCalendar`.
  - `CalendarController` at `api/v1/calendar/deadlines` — reads open to all auth roles; writes restricted to Admin/SuperAdmin.
  - `AcademicDeadlines.cshtml` portal CRUD page with create/edit modals; `AcademicCalendar.cshtml` read-only calendar view.
  - `DeadlineReminderJob` (`BackgroundService`): runs daily, calls `DispatchPendingRemindersAsync`, dispatches `NotificationType.System` notifications to all active users.
  - `IEduApiClient` + `EduApiClient` extended with 5 calendar methods; `PortalController` extended with 5 new actions.
- Validation: **0 build errors; 78/78 tests passed**

---

## 1. Delivery Strategy

- Delivery model: Phased, test-first, modular monolith
- Sprint length: 2 weeks
- Initial roadmap horizon: **42 weeks (21 sprints)**
- Release train:
  - v1.0 core operations (Sprints 1–5)
  - v1.1 academic expansion (Sprints 6–12)
  - v1.2 lifecycle, licensing tool, dashboard, finance (Sprints 13–18)
  - v1.3 security, performance, email, mobile (Sprint 19)
  - v1.4 result calculation and GPA automation (Sprints 20–21)

---

## 2. Engineering Baseline

### 2.1 Proposed Solution Structure

- src/Web: ASP.NET Core MVC and Razor UI
- src/API: ASP.NET Core Web API
- src/Application: CQRS handlers, validation, business use cases
- src/Domain: Entities, value objects, domain services
- src/Infrastructure: EF Core, auth adapters, storage, integrations
- src/BackgroundJobs: license checks, notifications, cleanups
- tests/UnitTests
- tests/IntegrationTests
- tests/ContractTests

### 2.2 Core Technical Standards

- .NET 8 LTS
- EF Core with SQL Server
- ASP.NET Core Identity + JWT
- FluentValidation for request validation
- Serilog for structured logging
- OpenTelemetry for tracing and metrics
- xUnit + FluentAssertions + Testcontainers for testing

---

## 3. Phase Plan

## Phase 0: Project Foundation (Sprint 1)

### Objectives
- Create solution skeleton and architecture boundaries
- Configure CI/CD pipeline and environments
- Establish coding standards and quality gates

### Deliverables
- Working solution with build and test pipeline
- Environment configuration (dev, staging, production)
- Baseline health check and logging endpoints

### Exit Criteria
- CI pipeline green on pull requests
- Code coverage baseline established
- Deployment to staging succeeds

---

## Phase 1: Identity, Licensing, and Access Control (Sprints 2-3)

### Objectives
- Implement authentication and RBAC
- Implement license upload, validation, and degraded mode
- Implement module entitlement resolver

### Deliverables
- Login/logout and role policies (Student, Faculty, Admin, Super Admin)
- License verification service with signed payload checks
- Module activation API and admin UI

### Exit Criteria
- Unauthorized access blocked by policy tests
- Expired/invalid license enforces read-only operations
- Mandatory modules cannot be deactivated

---

## Phase 2: Academic Core and SIS (Sprints 4-5)

### Objectives
- Implement departments, programs, courses, semesters
- Implement student profile and enrollment workflow
- Preserve permanent academic history

### Deliverables
- Department and program management screens
- Student signup using registration whitelist
- Semester records with immutable history behavior

### Exit Criteria
- Student record creation is fully auditable
- Academic history cannot be deleted via UI or API
- Faculty can only access assigned department data

---

## Phase 3: Assignments and Results (Sprints 6-7)

### Objectives
- Implement assignment lifecycle and submissions
- Implement grading workflows and result publication
- Implement transcript export

### Deliverables
- Assignment creation and submission APIs/UI
- Faculty grading and feedback workflows
- Result publication and transcript export logs

### Exit Criteria
- Assignment submissions enforce one-per-student rule
- Result publication is role-restricted and auditable
- Transcript export appears in audit/report logs

---

## Phase 4: Notifications and Attendance (Sprints 8-9)

### Objectives
- Implement notification engine and recipient tracking
- Implement attendance management and alerts

### Deliverables
- Notification compose, dispatch, read-state tracking
- Attendance daily capture and low-attendance checks
- Scheduled job for attendance alerts

### Exit Criteria
- Notifications tracked per user with read state
- Duplicate attendance entries prevented per day
- Alert job execution visible in operational logs

---

## Phase 5: Quizzes and FYP (Sprints 10-11)

### Objectives
- Implement quiz authoring and attempts
- Implement FYP project and meeting scheduling

### Deliverables
- Quiz question bank, attempts, and scoring
- FYP meetings with panel members and room scheduling
- Student dashboard views for quizzes and FYP events

### Exit Criteria
- Quiz attempts honor configured attempt limits
- FYP meetings generate notifications for stakeholders
- Faculty and department access boundaries verified

---

## Phase 6: AI, Analytics, and Hardening (Sprint 12)

### Objectives
- Integrate AI chatbot with role-aware context
- Implement initial reporting dashboards
- Complete performance and security hardening

### Deliverables
- AI chat endpoint with module/license guardrails
- Core analytics (performance, attendance, results)
- Security checklist completion and load test report

### Exit Criteria
- AI access obeys module and license policies
- p95 response targets met for core endpoints
- UAT sign-off for v1.x release candidate

---

## Phase 7: Tabsan-Lic — License Creation Tool (Sprints 13–14)

### Objectives
- Build a standalone .NET application for generating encrypted license files
- Implement one-time-use VerificationKey mechanism
- Wire EduSphere license import to consume Tabsan-Lic’s `.tablic` files

### Deliverables
- `Tabsan-Lic` standalone .NET app with VerificationKey generation UI
- AES-256 encrypted + RSA-2048 signed `.tablic` license file output
- VerificationKey expiry options: 1 year / 2 years / 3 years / Permanent
- EduSphere import endpoint: signature verify → decrypt → apply → mark key consumed
- License expiry background job: notification to Admin/Super Admin 5 days before expiry

### Exit Criteria
- Re-importing a used VerificationKey is rejected with error
- Tampered `.tablic` file fails signature check and is rejected
- License status table updates correctly on import
- Expiry notification fires on schedule in tests

---

## Phase 8: Student Lifecycle & Academic Operations (Sprints 15–16)

### Objectives
- Implement end-to-end student lifecycle: graduation, semester progression, dropout, transfer
- Implement finance and payment receipt workflow
- Implement CSV-based registration import
- Implement teacher modification request with admin approval

### Deliverables
- "Graduated Students" menu: per-department checkbox list; graduated → read-only dashboard
- "Semester Management" menu: per-student subject selection; promotion or failure logic
- Student inactive status (dropout/leave): blocks login, preserves data
- Department/program transfer admin action
- Admin change request workflow for non-self-editable fields
- Teacher attendance/result modification request with reason + admin approval + audit trail
- Finance role: payment receipts CRUD; student payment submission; Finance confirmation
- Read-only mode for students with unpaid fees
- Online payment gateway toggle (Super Admin, Module Settings)
- CSV import for registration whitelist with duplicate detection and error report
- Account lockout after 5 consecutive failures; Admin/Super Admin unlock + password reset

### Exit Criteria
- Graduated student cannot create/edit/submit anything on their dashboard
- Promoted students’ active semester number updates automatically
- Student with unpaid fees cannot write to any resource until Finance marks Paid
- CSV import rejects duplicates and produces downloadable error report
- Locked account cannot log in; unlocked account can log in immediately

---

## Phase 9: Dashboard, Navigation & System Settings (Sprints 17–18)

### Objectives
- Implement role-based sidebar navigation with dynamic menus
- Implement per-user theme persistence
- Implement Departments admin menu (timetable included)
- Implement full System Settings menu (License, Theme, Reports, Modules)

### Deliverables
- Collapsible sidebar: menus and sub-menus per role and active modules
- Per-user theme stored in user profile; theme picker with preview in Settings
- Departments admin CRUD: departments, degree programs, semesters, subjects, timetables
- Timetable PDF/Excel download for all department users
- Settings → License Update: upload `.tablic`; status table (Status, Expiry, Date Updated, Remaining Days)
- Settings → Report Settings: SR#, Report Name, Purpose, Roles (multi-select), active/inactive
- Settings → Module Settings: SR#, Module Name, Purpose, Roles (multi-select), Status dropdown
- License expiry notification (5 days prior) wired to background job

### Exit Criteria
- Sidebar shows only menus for active modules and user’s role
- Two users with different themes see their own theme independently
- Deactivated module hidden from all dashboards except Super Admin
- Deactivating a module does not delete any data
- Timetable PDF and Excel download verified in integration tests

---

## Phase 10: Security, Performance & Email Infrastructure (Sprint 19)

### Objectives
- Complete OWASP Top 10 hardening
- Add database views and stored procedures for performance
- Integrate free/open-source email API
- Deliver mobile-responsive, WCAG 2.1 AA accessible UI

### Deliverables
- OWASP Top 10 checklist: injection, broken auth, XSS, IDOR, misconfiguration remediation
- Security headers: HSTS, CSP, X-Frame-Options, X-Content-Type-Options
- Rate limiting on auth and sensitive endpoints
- Password policy: Argon2id hashing, complexity rules, lockout, no last-5 reuse
- SQL Views for student dashboard summary, department reports, attendance summary
- Stored Procedures for semester promotion batch, graduation batch, payment status update
- Covering indexes on all FK columns and frequently filtered columns
- `IEmailSender` abstraction wired to MailKit SMTP / SendGrid free tier (configurable)
- Email notifications: results, assignment deadlines, low attendance, license expiry, password reset, account unlock
- All outbound email attempts logged with status
- Bootstrap 5 responsive layout tested at 360 px, 768 px, 1280 px
- Lighthouse score ≥ 90 on core pages
- Penetration test report signed off; zero critical/high CVEs

### Exit Criteria
- OWASP Top 10 checklist fully signed off
- p95 < 200 ms for core dashboards under simulated load
- Email delivery verified in staging environment
- Lighthouse scores ≥ 90 on core pages
- No critical or high CVEs in dependency scan

---

## Phase 11: Result Calculation & GPA Automation (Sprints 20-21)

### Objectives
- Add a new sidebar menu named `Result Calculation` for admins
- Allow admins to configure GPA-to-score thresholds and subject component weightages
- Automatically calculate subject GPA, semester GPA, and cumulative CGPA from entered marks

### Deliverables
- Result Calculation screen with two sections:
  - Section 1: repeatable `GPA` and `Score` rows with `Add Row` and `Save`
  - Section 2: repeatable assessment component rows for items such as Quizzes, Midterms, Finals and their score weights
- Database tables and APIs for GPA mappings and result component configuration
- Validation rule that component score weights must total `100`
- Result-entry workflow updates so faculty mark entry triggers automatic subject total calculation
- Semester completion logic that automatically computes SGPA when all subject marks are present
- CGPA aggregation logic that automatically updates cumulative GPA after every semester completion or approved result modification
- Audit trail for recalculation operations and admin configuration changes

### Exit Criteria
- Admin can add, edit, and save multiple GPA/Score rows without data loss
- Admin can configure component weightages and cannot save an active rule set unless the total is `100`
- Entering quiz, midterm, or final marks automatically updates subject totals and subject GPA
- When all subjects for a semester are marked, the student’s SGPA and CGPA are recalculated and stored automatically
- Integration tests cover initial calculation, recalculation after mark edits, and incomplete-mark scenarios

---

## 4. Cross-Cutting Workstreams

## 4.1 Data and Migration Workstream
- Apply incremental EF Core migrations per phase
- Seed base roles/modules/themes in non-production and production-safe scripts
- Backup and restore runbook validated before each release

## 4.2 Security and Compliance Workstream
- Threat modeling at phase boundaries
- SAST/dependency scanning on each PR
- Audit log completeness checks for privileged operations

## 4.3 Quality Engineering Workstream
- Unit tests for domain and application layers
- Integration tests for API and data persistence
- Contract tests for client-server compatibility
- Regression suite before each milestone release

---

## 5. Definition of Done

A feature is complete only when:

- Functional requirements are implemented and demoed
- Unit and integration tests pass in CI
- Logging, metrics, and audit events are included
- Authorization and module-activation rules are enforced
- API documentation is updated
- No critical or high vulnerabilities remain open

---

## 6. Risk Register and Mitigations

- Licensing complexity risk:
  - Mitigation: implement and test license service early in Phase 1; Tabsan-Lic built as Phase 7
- Access-control regression risk:
  - Mitigation: centralized policy tests and authorization integration tests
- Performance risk under peak enrollment windows:
  - Mitigation: load testing from Phase 3 onward; SQL Views and Stored Procedures in Phase 10
- Scope creep risk:
  - Mitigation: v1.0 scope locked; Phases 7–10 routed to v1.2/v1.3 release train
- Finance/payment integration risk:
  - Mitigation: finance workflow built without gateway first; gateway added as a toggled add-on module
- Email delivery risk:
  - Mitigation: `IEmailSender` abstraction allows provider swap without code changes; SMTP fallback always available
- Student lifecycle complexity risk:
  - Mitigation: graduation, semester promotion, and dropout handled as discrete admin operations with individual audit trails
- Security hardening risk:
  - Mitigation: OWASP checklist tracked from Phase 1; dedicated Sprint 19 for final hardening and pen test

---

## 7. Immediate Next Actions (Week 1)

- Finalize solution structure and repository conventions
- Create initial ASP.NET solution and projects
- Configure CI pipeline with build, test, lint, and security scan
- Implement baseline auth model and role seed
- Draft initial EF Core migration for identity and department core

---

## 8. Execution Status Update (Kickoff)

### Completed

- .NET 8 modular solution scaffold created with `src/` and `tests/` layout
- Projects created: Web, API, Application, Domain, Infrastructure, BackgroundJobs
- Test projects created: UnitTests, IntegrationTests, ContractTests
- Solution files created: `Tabsan.EduSphere.sln` and `Tabsan.EduSphere.slnx`
- Project references wired according to planned architecture direction
- Baseline packages added (FluentValidation, EF Core SQL Server, Serilog, OpenTelemetry, testing stack)
- GitHub Actions CI workflow created at `.github/workflows/dotnet-ci.yml`
- Local validation completed: restore, build, and tests passed

### Next Immediate Implementation Tasks

- Add architecture decision records (ADRs) for auth, data, and module enforcement
- Implement `ApplicationDbContext` and first EF Core migration
- Implement role seed and Super Admin bootstrap flow
- Add first policy-based authorization matrix tests

---

## Phase 12: Reporting & Document Generation (Sprints 22-23)

### Objectives
- Build a role-gated Report Center portal backed by `ReportDefinition` / `ReportRoleAssignment` records from Phase 9
- Provide five standard reports accessible via web portal and REST API
- Support Excel (`.xlsx`) export on attendance, result, and GPA reports
- Enforce role-based access to each report using the existing report role assignment system

### Deliverables
- **Report Catalog endpoint** — `GET /api/v1/reports` returns the subset of active reports the caller's role is permitted to view
- **Attendance Summary Report** — per-student per-offering session counts and attendance percentage, filterable by semester / department / offering / student; Excel export
- **Result Summary Report** — all published results with marks and percentage, filterable by semester / department / offering / student; Excel export
- **GPA & CGPA Report** — per-student academic standing (Current Semester GPA + CGPA) filterable by department / program; Excel export; average CGPA summary row
- **Enrollment Summary Report** — per course-offering seat utilisation (enrolled vs max capacity); filterable by semester / department
- **Semester Results Report** — full published result set for a selected semester with optional department filter
- **Report Center web page** — landing page listing all reports available to the user's role as cards with filter and view buttons
- **Four report detail pages** — `ReportAttendance`, `ReportResults`, `ReportGpa`, `ReportEnrollment` — each with filter form, sortable data table, and Export button
- **Sidebar menu item** — `reports` entry visible to Admin, Faculty, and Student roles
- **Seed data** — five `ReportDefinition` rows seeded at startup with default role assignments

### Technical Highlights
- `ReportKeys` constants class prevents magic string duplication across backend and seeder
- `ReportRepository` uses EF Core 8 queries with explicit joins (no raw SQL) for portability and testability
- `ReportService` uses `ClosedXML` for in-memory Excel workbook generation — no temporary files on disk
- No new EF migration required — `report_definitions` and `report_role_assignments` tables were created in Phase 9

### Exit Criteria
- All five report endpoints return 200 for authenticated admin user
- All three export endpoints return `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Non-authorised role receives 403 on a report they are not assigned to
- `Build succeeded. 0 Error(s)` on `dotnet build`

---

### 2026-05-08 — Phase 20 Learning Management System Complete (commit `ecf4d91`)

**Stage 20.1 — Course Content Modules:**
- `CourseContentModule` + `ContentVideo` domain entities (`Domain/Lms/`).
- `ILmsRepository` + `LmsRepository`; `ILmsService` + `LmsService`.
- `LmsController` at `api/v1/lms` — full CRUD + publish/unpublish for modules; add/delete for videos.
- Students auto-scoped to `publishedOnly=true`; faculty see all modules.
- Portal views: `CourseLms.cshtml` (student accordion), `LmsManage.cshtml` (faculty management panel).

**Stage 20.2 — EF Configuration:**
- `LmsConfigurations.cs` — 5 EF entity configurations with table names, FK cascade rules, field lengths, and `HasQueryFilter(!IsDeleted)` for all LMS entities.
- EF migration `Phase20_LMS` — creates `course_content_modules`, `content_videos`, `discussion_threads`, `discussion_replies`, `course_announcements`.

**Stage 20.3 — Discussion Forums:**
- `DiscussionThread` + `DiscussionReply` domain entities.
- `IDiscussionRepository` + `DiscussionRepository`; `IDiscussionService` + `DiscussionService`.
- Author names resolved at service layer via `IUserRepository.GetByIdAsync`.
- `DiscussionController` at `api/v1/discussion`; JWT-enforced `AuthorId` on all write endpoints.
- Portal views: `Discussion.cshtml` + `DiscussionThread.cshtml`.

**Stage 20.4 — Course Announcements:**
- `CourseAnnouncement` domain entity; optional `OfferingId` FK with SET NULL on cascade.
- `IAnnouncementRepository` + `AnnouncementRepository`; `IAnnouncementService` + `AnnouncementService`.
- Fan-out on create: queries enrolled students → dispatches `NotificationType.Announcement` notification batch.
- `AnnouncementController` at `api/v1/announcement`; JWT-enforced `AuthorId`.
- Portal view: `Announcements.cshtml`.

**Cross-cutting:**
- `ApplicationDbContext` updated with 5 new `DbSet<T>` properties.
- `Program.cs` Phase 20 DI block: 6 scoped registrations.
- `EduApiClient` interface + models + 21 new method implementations.
- `PortalController` 19 new actions (LMS + Discussion + Announcements).
- `PortalViewModels.cs` 10 new view models.
- `_Layout.cshtml` sidebar entries: `lms_manage`, `discussion`, `announcements` (group: Academic Related).

**Validation:** 0 build errors · 7/7 unit tests · migration applied · commit `ecf4d91` pushed

---

### 2026-05-08 — Phase 21 Study Planner Complete

**Changes:**
- `Domain/StudyPlanner/StudyPlan.cs` — aggregate root; `StudyPlanStatus` enum; endorsement workflow methods.
- `Domain/StudyPlanner/StudyPlanCourse.cs` — child entity (physical delete).
- `AcademicProgram.MaxCreditLoadPerSemester` property + `SetMaxCreditLoad()` method.
- `Domain/Interfaces/IStudyPlanRepository.cs` — 7 methods.
- `Application/DTOs/StudyPlanner/StudyPlannerDTOs.cs` — 4 requests + 4 response records.
- `Application/Interfaces/IStudyPlanService.cs` + `Application/StudyPlanner/StudyPlanService.cs` — CRUD + prerequisite/credit-load validation + recommendation engine.
- `Infrastructure/Persistence/Configurations/StudyPlanConfigurations.cs` — `StudyPlanConfiguration` + `StudyPlanCourseConfiguration`.
- `Infrastructure/Repositories/StudyPlanRepository.cs`.
- `Infrastructure/Persistence/ApplicationDbContext.cs` — `StudyPlans` + `StudyPlanCourses` DbSets.
- `API/Controllers/StudyPlanController.cs` — 9 endpoints.
- `API/Program.cs` Phase 21 DI block: 2 scoped registrations.
- `Web/Services/EduApiClient.cs` — 9 new methods + 4 API models.
- `Web/Controllers/PortalController.cs` — 9 new actions + `MapStudyPlanItem` helper.
- `Web/Models/Portal/PortalViewModels.cs` — 6 new view models.
- `_Layout.cshtml` sidebar: `study_plan` → `(Portal, StudyPlan)` (group: Student Related).
- Views: `StudyPlan.cshtml`, `StudyPlanDetail.cshtml`, `StudyPlanRecommendations.cshtml`.
- Migration `Phase21_StudyPlanner` applied.

**Validation:** 0 build errors · 7/7 unit tests · migration applied

---

### 2026-05-08 — Phase 22 External Integrations Complete

**Changes:**
- `Domain/Settings/AccreditationTemplate.cs` — `AccreditationTemplate` entity + `AccreditationTemplateConfiguration` EF config.
- `Application/DTOs/External/LibraryDTOs.cs` — `SaveLibraryConfigCommand`, `LibraryConfigDto`, `LibraryLoanDto`.
- `Application/DTOs/External/AccreditationDTOs.cs` — `CreateAccreditationTemplateCommand`, `UpdateAccreditationTemplateCommand`, `AccreditationTemplateDto`, `GeneratedReport`.
- `Application/Interfaces/ILibraryService.cs` + `Application/Services/LibraryService.cs` — `GetConfigAsync`, `SaveConfigAsync`, `GetLoansAsync`.
- `Application/Interfaces/IAccreditationService.cs` + `Application/Services/AccreditationService.cs` — full CRUD + `GenerateAsync` (field mapping materialisation + CSV/PDF export + audit log).
- `Domain/Interfaces/IAccreditationRepository.cs` + `Infrastructure/Repositories/AccreditationRepository.cs`.
- `Infrastructure/Persistence/Configurations/AccreditationTemplateConfiguration.cs`.
- `Infrastructure/Persistence/ApplicationDbContext.cs` — `AccreditationTemplates` DbSet added.
- `API/Controllers/LibraryController.cs` — 4 endpoints (`GET/PUT config`, `GET loans`, `GET loans/{id}`).
- `API/Controllers/AccreditationController.cs` — 6 endpoints (CRUD + generate/stream).
- `API/Program.cs` Phase 22 DI block: `LibraryService` (scoped), `AccreditationService` (scoped), `AccreditationRepository` (scoped).
- `Web/Services/EduApiClient.cs` — 11 new methods + API models.
- `Web/Controllers/PortalController.cs` — 9 new actions.
- `Web/Views/Portal/LibraryConfig.cshtml` — library URL + token config view.
- `Web/Views/Portal/AccreditationTemplates.cshtml` — template list with CRUD modals + generate buttons.
- `_Layout.cshtml` sidebar entries: `library_config`, `accreditation` (Settings group).
- EF Migration `Phase22_ExternalIntegrations` — adds `accreditation_templates` table.
- `Scripts/1-MinimalSeed.sql` — adds sidebar seed entries for `library_config` (sort 31, SuperAdmin) and `accreditation` (sort 32, Admin+SuperAdmin).

**Validation:** 0 build errors · migration `Phase22_ExternalIntegrations` applied · commit `dddee69` pushed

---

### 2026-05-09 — Phase 23 Core Policy Foundation Complete

**Changes:**
- `Domain/Enums/InstitutionType.cs` — `InstitutionType` enum (`University=0`, `School=1`, `College=2`).
- `Application/Interfaces/IInstitutionPolicyService.cs` — `InstitutionPolicySnapshot` sealed record + `SaveInstitutionPolicyCommand` + `IInstitutionPolicyService` interface.
- `Application/Services/InstitutionPolicyService.cs` — implementation with `IMemoryCache` (10-min TTL) + `ISettingsRepository` persistence.
- `Tabsan.EduSphere.Application.csproj` — added `Microsoft.Extensions.Caching.Memory 8.0.1`.
- `API/Middleware/InstitutionContextMiddleware.cs` — resolves snapshot per-request; `GetInstitutionPolicy()` extension method with `Default` fallback.
- `API/Controllers/InstitutionPolicyController.cs` — `GET api/v1/institution-policy` (all authenticated) + `PUT api/v1/institution-policy` (SuperAdmin only).
- `API/Program.cs` — `IInstitutionPolicyService` scoped DI; `InstitutionContextMiddleware` after `UseAuthorization`.
- `Web/Services/EduApiClient.cs` — `GetInstitutionPolicyAsync`, `SaveInstitutionPolicyAsync`.
- `Web/Controllers/PortalController.cs` — `InstitutionPolicy` GET/POST actions.
- `Web/Views/Portal/InstitutionPolicy.cshtml` — three-flag toggle config form.
- `_Layout.cshtml` sidebar entry `institution_policy` (Settings group).
- `Scripts/1-MinimalSeed.sql` — sidebar seed `institution_policy` (sort 33, SuperAdmin).
- `tests/Tabsan.EduSphere.UnitTests/InstitutionPolicyTests.cs` — 13 new tests (27 total).

**Validation:** 0 build errors · 27/27 unit tests · no migration needed · commit `28cac36` pushed

---

### 2026-05-09 — Phase 24 Dynamic Module and UI Composition Complete

**Changes:**
- `Domain/Modules/ModuleDescriptor.cs` — `ModuleDescriptor` sealed record: `Key`, `RequiredRoles[]`, `AllowedTypes[]?`, `IsLicenseGated`; `RoleMatches()` + `TypeMatches()` methods.
- `Application/Modules/ModuleRegistry.cs` — static catalogue of all 14 module descriptors.
- `Application/Interfaces/IModuleRegistryService.cs` — `ModuleVisibilityResult` record + interface with `GetVisibleModulesAsync` + `IsAccessibleAsync`.
- `Application/Modules/ModuleRegistryService.cs` — combines registry + `IModuleEntitlementResolver` + institution policy.
- `Application/Interfaces/ILabelService.cs` — `AcademicVocabulary` sealed record + `ILabelService.GetVocabulary(policy)`.
- `Application/Services/LabelService.cs` — stateless singleton (University/School/College vocab branches).
- `Application/Interfaces/IDashboardCompositionService.cs` — `WidgetDescriptor` sealed record + `IDashboardCompositionService.GetWidgets(role, policy)`.
- `Application/Services/DashboardCompositionService.cs` — 10-widget catalogue, role + institution-type filtered.
- `API/Controllers/ModuleRegistryController.cs` — `GET api/v1/module-registry/visible`.
- `API/Controllers/LabelController.cs` — `GET api/v1/labels`.
- `API/Controllers/DashboardCompositionController.cs` — `GET api/v1/dashboard/composition`.
- `API/Program.cs` Phase 24 DI block: `ModuleRegistryService` (scoped), `LabelService` (singleton), `DashboardCompositionService` (singleton).
- `Web/Services/EduApiClient.cs` — `GetVisibleModulesAsync`, `GetVocabularyAsync`, `GetDashboardWidgetsAsync` + 3 API response models.
- `Web/Controllers/PortalController.cs` — `ModuleComposition` GET action (parallel `Task.WhenAll`).
- `Web/Models/Portal/PortalViewModels.cs` — `DashboardCompositionModel`, `ModuleVisibilityItem`, `WidgetItem`.
- `Web/Views/Portal/ModuleComposition.cshtml` — vocabulary tiles, widget cards, module registry table.
- `_Layout.cshtml` sidebar entry `module_composition` (Settings group).
- `Scripts/1-MinimalSeed.sql` — sidebar seed `module_composition` (sort 34, SuperAdmin).
- `tests/Tabsan.EduSphere.UnitTests/Phase24Tests.cs` — 17 new tests (44 total).

**Validation:** 0 build errors · 44/44 unit tests · no migration needed · commit `391ac45` pushed

---

### 2026-05-09 — Phase 25 Academic Engine Unification Complete

**Stage 25.1 — Result Calculation Strategy Pattern:**
- `Application/Interfaces/IResultCalculationStrategy.cs` — `IResultCalculationStrategy` interface + value types: `ComponentMark`, `ResultSummary`, `GpaScaleRuleEntry`, `GradeBandEntry`.
- `Application/Academic/GpaResultStrategy.cs` — University: weighted % → GPA 0.0–4.0 scale lookup. `AppliesTo = InstitutionType.University`.
- `Application/Academic/PercentageResultStrategy.cs` — School/College: weighted % → grade band label (custom JSON or built-in A+/A/B+/B/C/D/F). Throws if constructed for University.
- `Application/Interfaces/IResultStrategyResolver.cs` — `Resolve(InstitutionType)` contract.
- `Application/Academic/ResultStrategyResolver.cs` — singleton resolver: University→GpaResultStrategy, School/College→PercentageResultStrategy. Registered as Singleton.

**Stage 25.2 — Institution Grading Profiles:**
- `Domain/Academic/InstitutionGradingProfile.cs` — entity with `InstitutionType`, `PassThreshold` (validated), `GradeRangesJson`, `IsActive`. `Update()` method.
- `Domain/Interfaces/IInstitutionGradingProfileRepository.cs` — `GetAllAsync`, `GetByTypeAsync`, `GetByIdAsync`, `AddAsync`, `Update`, `SaveChangesAsync`.
- `Application/Interfaces/IInstitutionGradingService.cs` — `GetAllAsync`, `GetByTypeAsync`, `UpsertAsync`.
- `Application/Academic/InstitutionGradingService.cs` — upsert (create if missing, update if exists). Maps to `InstitutionGradingProfileDto`.
- `Application/DTOs/Academic/InstitutionGradingDtos.cs` — `InstitutionGradingProfileDto`, `SaveInstitutionGradingProfileRequest`.
- `Infrastructure/Repositories/InstitutionGradingProfileRepository.cs` — EF Core implementation.
- `Infrastructure/Persistence/Configurations/InstitutionGradingProfileConfiguration.cs` — table `institution_grading_profiles`, `decimal(5,2)` for threshold, unique index on `InstitutionType`.
- `Infrastructure/Persistence/ApplicationDbContext.cs` — `DbSet<InstitutionGradingProfile> InstitutionGradingProfiles`.
- `Infrastructure/Migrations/20260508152906_Phase25_AcademicEngineUnification.cs` — creates `institution_grading_profiles` table.
- `API/Controllers/InstitutionGradingProfileController.cs` — `GET /` (Admin+), `GET /{type}` (Admin+), `PUT /{type}` (SuperAdmin).

**Stage 25.3 — Progression / Promotion Logic:**
- `Application/Interfaces/IProgressionService.cs` — `EvaluateAsync`, `PromoteAsync`.
- `Application/Academic/ProgressionService.cs` — institution-aware evaluation; `PromoteAsync` calls `student.AdvanceSemester()`. Default thresholds: 2.0 (University), 40 (School/College).
- `Application/DTOs/Academic/ProgressionDtos.cs` — `ProgressionDecision`, `ProgressionEvaluationRequest`.
- `API/Controllers/ProgressionController.cs` — `POST /evaluate` (Admin+), `POST /promote` (Admin+), `GET /me/{type}` (Student+).
- `API/Program.cs` Phase 25 DI: `IResultStrategyResolver` (singleton), `IInstitutionGradingProfileRepository` (scoped), `IInstitutionGradingService` (scoped), `IProgressionService` (scoped).
- `tests/Tabsan.EduSphere.UnitTests/Phase25Tests.cs` — 29 new tests (144 total): `GpaResultStrategyTests`, `PercentageResultStrategyTests`, `ResultStrategyResolverTests`, `InstitutionGradingProfileTests`, `ProgressionServiceTests`.

**Validation:** 0 build errors · 144/144 unit tests · migration `20260508152906_Phase25_AcademicEngineUnification` · commit `d2aabd3` pushed

---

### 2026-05-09 — Phase 26 School and College Functional Expansion Complete

**Stage 26.1 — School Streams and Subject Mapping:**
- `Domain/Academic/SchoolStream.cs` and `Domain/Academic/StudentStreamAssignment.cs` created.
- `Domain/Interfaces/ISchoolStreamRepository.cs` + `Infrastructure/Repositories/Phase26Repositories.cs` (`SchoolStreamRepository`).
- `Application/Interfaces/ISchoolStreamService.cs` + `Application/Academic/SchoolStreamService.cs`.
- `API/Controllers/SchoolStreamController.cs` endpoints for stream listing/upsert and student assignment.
- EF configs: `SchoolStreamConfiguration.cs`, `StudentStreamAssignmentConfiguration.cs`.

**Stage 26.2 — Report Cards and Bulk Promotion:**
- `Domain/Academic/StudentReportCard.cs`, `BulkPromotionBatch.cs`, `BulkPromotionEntry.cs`.
- Enums: `BulkPromotionStatus.cs`, `EntryDecision.cs`.
- Repository interfaces: `IReportCardRepository.cs`, `IBulkPromotionRepository.cs`.
- Services: `IReportCardService`/`ReportCardService`, `IBulkPromotionService`/`BulkPromotionService`.
- API: `ReportCardController.cs`, `BulkPromotionController.cs`.
- EF configs: `StudentReportCardConfiguration.cs`, `BulkPromotionBatchConfiguration.cs`, `BulkPromotionEntryConfiguration.cs`.

**Stage 26.3 — Parent-Facing Read Model:**
- `Domain/Academic/ParentStudentLink.cs` + `Domain/Interfaces/IParentStudentLinkRepository.cs`.
- `Application/Interfaces/IParentPortalService.cs` + `Application/Academic/ParentPortalService.cs`.
- `API/Controllers/ParentPortalController.cs`.
- EF config: `ParentStudentLinkConfiguration.cs`.

**Cross-Cutting / Wiring:**
- `Application/DTOs/Academic/Phase26Dtos.cs` added for stream/report card/bulk promotion/parent read DTOs.
- `Infrastructure/Persistence/ApplicationDbContext.cs` adds 6 new DbSets.
- `API/Program.cs` Phase 26 DI registrations for repositories and services.
- Migration: `Infrastructure/Migrations/20260509044437_Phase26_SchoolCollegeExpansion.cs`.

**Validation:**
- `dotnet build Tabsan.EduSphere.sln` — 0 build errors.
- Unit + integration/contract suite: **152/152 tests passed**.
- Migration visible in list: `20260509044437_Phase26_SchoolCollegeExpansion`.
- Commit: `4c0904c` pushed.

---

### 2026-05-09 — Phase 27 Stage 27.1 Portal Capability Matrix Complete

**Changes:**
- Added `IPortalCapabilityMatrixService` + `PortalCapabilityMatrixService`.
- Added `PortalCapabilitiesController` endpoint: `GET api/v1/portal-capabilities/matrix`.
- Added web support: `PortalController.PortalCapabilityMatrix`, portal page model, and `PortalCapabilityMatrix.cshtml`.
- Added unit tests in `Phase27Tests.cs`.

**Validation:**
- Unit tests passed.
- Solution build successful.
- Commit: `fd3b137` pushed.

---

### 2026-05-09 — Phase 27 Stage 27.2 Authentication and Security UX Complete

**Changes:**
- Added `AuthSecurityOptions` config contract (`AuthSecurity` section in appsettings).
- Extended auth DTO/service/controller flows for MFA toggle, SSO-ready security profile, and session-risk controls.
- Added API endpoint `GET api/v1/auth/security-profile`.
- Updated web login flow/UI for MFA field, SSO/risk hints, and richer auth error handling.
- Added unit tests in `Phase27Stage2Tests.cs`.

**Validation:**
- Unit tests passed.
- Solution build successful.
- Commit: `20dba8d` pushed.

---

### 2026-05-09 — Phase 27 Stage 27.3 Support and Communication Integration Complete

**Changes:**
- Added provider abstraction contracts:
  - `ISupportTicketingProvider`
  - `IAnnouncementBroadcastProvider`
  - `IEmailDeliveryProvider`
- Added default adapters in Infrastructure:
  - `InAppSupportTicketingProvider`
  - `InAppAnnouncementBroadcastProvider`
  - `SmtpEmailDeliveryProvider`
- Refactored `HelpdeskService`, `AnnouncementService`, and `LicenseExpiryWarningJob` to use provider contracts.
- Added `ICommunicationIntegrationService` + `CommunicationIntegrationService` and API endpoint `GET api/v1/communication-integrations/profile`.
- Added unit test `Phase27Stage3Tests.cs`.

**Validation:**
- `dotnet test` (unit project): 89/89 passed.
- `dotnet build Tabsan.EduSphere.sln`: success.
- Commit: `56cf1dd` pushed.

---
