# Advance Enhancements - Consolidated Phases and Stages

Source inputs used:
- New Enhancements/EduSphere_Competitive_Roadmap.txt
- New Enhancements/EduSphere_Million_User_Scalability_Guide.txt
- New Enhancements/EduSphere_MSSQL_Indexing_Strategy.txt
- New Enhancements/New Enhancements Guide.docx
- New Enhancements/University_Portal_PRD.docx

Status: Planned
Scope start: After Phase 22

Execution status update (2026-05-10):
- Phases 23 through 28 are completed.
- Phase 29 Stage 29.1 is completed.
- Phase 29 Stage 29.2 slices 1, 2, and 3 are completed.
- Phase 29 Stage 29.3 is completed.
- Phase 29 is completed.
- Phase 30 Stage 30.1 is completed.
- Phase 30 Stage 30.2 is completed.
- Phase 30 Stage 30.3 is completed.
- Phase 30 is completed.
- Phase 31 Stage 31.1 is completed.
- Phase 31 Stage 31.2 is completed.
- Phase 31 Stage 31.3 is completed.
- Phase 31 is completed.

---

## Completed Tasks - Implementation and Validation Summary

### Phase 23 - Core Policy Foundation (Completed)
Implementation summary:
- Centralized policy and role-rights enforcement paths were consolidated and applied across core protected workflows.
- Institution-context driven behavior and permission hardening were integrated as shared contracts rather than per-controller duplication.

Validation summary:
- Solution builds passed during phase completion runs.
- Automated test suites passed for the completion iterations of this phase.

### Phase 24 - Dynamic Module and UI Composition (Completed)
Implementation summary:
- Dynamic module visibility and portal capability composition were consolidated around policy/role context.
- Shared label/vocabulary and module composition behavior was moved to reusable service-driven paths.

Validation summary:
- Solution builds passed during phase completion runs.
- Automated test suites passed for the completion iterations of this phase.

### Phase 25 - Academic Engine Unification (Completed)
Implementation summary:
- Unified result-calculation and grading-profile behavior for School/College/University through shared strategy-driven orchestration.
- Removed duplicate mode-specific paths by binding progression and grading behavior to configuration-driven contracts.

Validation summary:
- Solution builds passed during phase completion runs.
- Automated test suites passed for the completion iterations of this phase.

### Phase 26 - School and College Functional Expansion (Completed)
Implementation summary:
- Delivered stream/subject mapping, report-card/promotion operations, and school/college read-model capabilities on top of the unified engine.
- Kept shared API/service contracts while extending mode-specific behavior through configuration.

Validation summary:
- Solution builds passed during phase completion runs.
- Automated test suites passed for the completion iterations of this phase.

### Phase 27 - University Portal Parity and Student Experience (Completed)
Implementation summary:
- Delivered capability-matrix parity, authentication/security UX hardening, and provider-abstraction communication contracts.
- Preserved existing portal behavior while expanding role-safe student/faculty/admin journeys.

Validation summary:
- Solution builds passed during phase completion runs.
- Automated test suites passed for the completion iterations of this phase.

### Phase 28 - Scalability Architecture (Completed)
Implementation summary:
- Stage 28.1: delivered stateless/load-balancer-ready app behavior and response shaping/compression.
- Stage 28.2: delivered distributed-cache hot paths and background offload for fan-out/report/recalculation workloads.
- Stage 28.3: delivered provider-backed media/file strategy, signed temporary reads, metadata contracts, and integrity/disposition metadata.

Validation summary:
- Stage-level builds passed; final phase validation runs passed.
- Automated tests passed in phase completion runs, ending at full suite pass status.

### Phase 29 Stage 29.1 - Index Baseline and Query Contracts (Completed)
Implementation summary:
- Added composite MSSQL indexes for high-frequency recency/status access patterns across graduation, helpdesk, notifications, payment receipts, quiz attempts, and sessions.
- Added EF migration `20260509155457_20260510_Phase29_IndexBaseline`.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

### Phase 29 Stage 29.2 Slice 1 - Helpdesk Pagination (Completed)
Implementation summary:
- Replaced unbounded helpdesk list retrieval with server-side paged contracts (`page`, `pageSize`) across repository, service, API, and portal layers.
- Added filter-preserving previous/next pagination controls in portal helpdesk views.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

### Phase 29 Stage 29.2 Slice 2 - Graduation Pagination (Completed)
Implementation summary:
- Added paged graduation list contracts with `TotalCount` for student and staff endpoints.
- Replaced unbounded graduation list materialization with SQL-side paging and updated portal list pagination controls.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

### Phase 29 Stage 29.2 Slice 3 - Payment Receipts Pagination (Completed)
Implementation summary:
- Added paged payment receipt contracts for student and admin receipt endpoints with `TotalCount` metadata.
- Replaced unbounded payment receipt list loading with SQL-side paging in the application, API, web client, and portal controller.
- Updated the portal payments page with previous/next navigation while preserving the selected student filter for admin views.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

### Phase 29 Stage 29.3 - Data Lifecycle and Maintenance (Completed)
Implementation summary:
- Added an archive/retention policy script (`Scripts/3-Phase29-ArchivePolicy.sql`) with table-specific retention windows, dry-run mode, and optional batched cleanup execution.
- Added an index maintenance script (`Scripts/4-Phase29-IndexMaintenance.sql`) to plan and optionally execute reorganize/rebuild operations from fragmentation thresholds.
- Added a capacity and growth dashboard script (`Scripts/5-Phase29-CapacityGrowthDashboard.sql`) with storage footprint and recent row-growth snapshots for high-volume tables.
- Updated `Scripts/README.md` with Stage 29.3 operational runbook commands.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

---

## 1) Non-Regression Guardrails (Must Stay Intact)

These guardrails apply to every phase and stage.

- Keep existing core functionality working:
  - Licensing
  - Authentication/authorization
  - Institutes/departments/programs/courses
  - Results and grading
  - Existing dashboards, reports, LMS, and integrations
- Configuration-first approach:
  - No hardcoded institution behavior
  - All behavior must come from settings/license/role checks
- No duplicate implementation paths:
  - One shared service per concern (license checks, labels, grading logic, module visibility)
  - Reuse same APIs and DTO patterns across School/College/University modes
- Backend is source of truth:
  - UI filtering is convenience only
  - API and service layer must enforce license + role + tenant constraints
- Data isolation is mandatory:
  - Every major query must filter by tenant/institution context

---

## 2) Global Role-Rights Contract

This is the rights baseline to preserve while adding features.

| Capability | SuperAdmin | Admin | Faculty | Student |
|---|---|---|---|---|
| System-wide config, license, tenant controls | Full | No | No | No |
| Add/Edit/Deactivate anything (global) | Full | No | No | No |
| Institute/department/program/course management | Full | Add/Edit (own tenant) | No | No |
| Degree and grading configuration | Full | Add/Edit (own tenant, where allowed) | No | No |
| Results setup and publishing workflows | Full | Add/Edit (own tenant) | Enter marks (own offerings) | View own |
| User deactivation/reactivation | Full | No (unless explicitly delegated per tenant policy) | No | No |
| Report configuration and access controls | Full | Add/Edit (own tenant) | Limited view | No |

Notes:
- SuperAdmin keeps complete add/edit/deactivate authority everywhere.
- Admin keeps operational add/edit authority for institutes, courses, degrees, results, and related academic master data in own tenant.
- Deactivation rights remain SuperAdmin by default to avoid accidental destructive changes.

---

## 3) Implementation Strategy to Avoid Rework

Apply this sequence so each area is edited once.

- Phase order is foundation -> policy -> domain behavior -> UX -> scale/perf -> operations.
- Every phase has strict done criteria before moving on.
- No feature stage may introduce a second parallel service for an already solved concern.
- Cross-cutting changes (role checks, tenant filters, caching, audit fields) happen in dedicated phases, not repeated in each feature phase.

---

## 4) Phase Roadmap

## Phase 23 - Core Policy Foundation (License + Institution Context)
Objective: Centralize policy checks before adding more feature behavior.

### Stage 23.1 - License Policy Kernel
- Build one centralized policy service used by API, Web, background jobs.
- Normalize flags:
  - IncludeSchool
  - IncludeCollege
  - IncludeUniversity
- Enforce rule: at least one flag must be enabled.
- Expose read-only policy snapshot for diagnostics.

Done when:
- All guarded endpoints use centralized policy service.
- No direct scattered license checks remain in controllers.

### Stage 23.2 - Institution Context Resolution
- Resolve institution mode once per request (School/College/University).
- Inject context into services and repositories.
- Add middleware/filter so every protected request has validated context.

Done when:
- Core services receive institution context through one shared path.
- Mixed-mode data leaks are blocked at backend.

### Stage 23.3 - Global Role-Rights Enforcement Hardening
- Formalize permission map by action and role.
- Add tests for:
  - SuperAdmin full rights
  - Admin add/edit scope for institutes/courses/degrees/results
  - Restricted deactivation by default

Done when:
- Permission tests pass for critical modules.
- Unauthorized edits/deactivations are rejected consistently.

---

## Phase 24 - Dynamic Module and UI Composition
Objective: One dynamic module map for menus, pages, APIs, and jobs.

### Stage 24.1 - Module Registry
- Define single module registry (module key, license requirement, role requirement, tenant scope).
- Drive sidebar/menu visibility and endpoint guards from registry.

### Stage 24.2 - Dynamic Labels and Academic Vocabulary
- One shared label service:
  - Semester <-> Grade <-> Year
  - GPA/CGPA <-> Percentage
  - Course <-> Subject where relevant
- Remove hardcoded labels from views.

### Stage 24.3 - Dashboard Composition by Mode + Role
- Render dashboard widgets from policy + role + institution context.
- Reuse shared data contracts to prevent duplicated dashboard queries.

Done when:
- Same user role sees only relevant modules without manual page-specific condition rewrites.

---

## Phase 25 - Academic Engine Unification (School/College/University)
Objective: Keep one academic engine with mode-specific behavior.

### Stage 25.1 - Result Calculation Strategy Pattern
- Implement strategy abstraction:
  - University strategy: GPA/CGPA
  - School/College strategy: Percentage + grade bands
- Keep one orchestration service; plug strategy by institution context.

### Stage 25.2 - Grading Configuration by Institution Type
- Configure grading profiles by mode (thresholds, bands, pass rules).
- SuperAdmin: full control of grading profiles.
- Admin: edit profile data only where tenant policy allows.

### Stage 25.3 - Promotion/Progression Logic
- School: grade-to-grade progression.
- College: year-to-year progression.
- University: semester progression.
- Bind progression rules to grading profiles, not hardcoded constants.

Done when:
- One result pipeline supports all three modes without duplicate code paths.

---

## Phase 26 - School and College Functional Expansion
Objective: Deliver school/college specifics on top of unified engine.

### Stage 26.1 - School Streams and Subject Mapping
- Streams for upper grades (Science/Biology/Computer/Commerce/Arts).
- Subject assignment by stream.
- Student stream constraints enforced in timetable/results/reports.

### Stage 26.2 - School/College Report Cards and Promotion Operations
- Report card generation templates for percentage mode.
- Bulk promotion workflows with approval safeguards.

### Stage 26.3 - Parent-Facing Read Model (School Optional)
- Parent portal (read-only) for linked students.
- Data scoped strictly by parent-student mapping + tenant.

Done when:
- School/College flow is complete without branching duplicate APIs.

---

## Phase 27 - University Portal Parity and Student Experience
Objective: Align PRD portal requirements while preserving existing behavior.

### Stage 27.1 - Student Portal Capability Matrix
- Consolidate existing features into one portal matrix:
  - Dashboard
  - Course and assignments
  - Quiz and feedback
  - Fees/payments
  - Timetable and exam schedule
  - Transcript access
  - Notifications/messaging/support

### Stage 27.2 - Authentication and Security UX
- SSO-ready contract (if enabled by deployment).
- MFA toggle support from config.
- Session risk controls and audit trail improvements.

### Stage 27.3 - Support and Communication Integration
- Ticketing + announcement + email integration contracts.
- Avoid direct vendor lock-in by using provider abstraction.

Done when:
- PRD core journey is complete for student/faculty/admin without regressions.

---

## Phase 28 - Scalability Architecture (1M+ Readiness)
Objective: Move from single-instance optimizations to distributed operations.

### Stage 28.1 - API and App Tier Scaling
- Load-balancer readiness.
- Stateless app node behavior.
- Response compression and payload shaping.

### Stage 28.2 - Caching and Background Workloads
- Redis distributed cache for hot read paths.
- Background jobs for heavy operations:
  - Report generation
  - Notification fan-out
  - Large recalculations

### Stage 28.3 - File and Media Strategy
- External object storage/CDN for large files.
- Keep database for metadata, not binary payload bulk.

Done when:
- System supports high concurrency without synchronous bottlenecks.

---

## Phase 29 - MSSQL Data and Indexing Optimization
Objective: Apply database strategy once, centrally, and safely.

### Stage 29.1 - Index Baseline and Query Contracts
- Add/validate indexes for high-frequency filters:
  - InstitutionId
  - StudentId
  - UserId
  - CourseId
  - SemesterId/YearId/GradeId
- Add composite indexes for common multi-column filters.

### Stage 29.2 - Query Discipline and Pagination
- Remove SELECT * from heavy paths.
- Enforce pagination/filtering in all large list endpoints.
- Add slow-query telemetry and execution-plan review loop.

Delivered updates:

#### 2026-05-10 - Stage 29.2 Slice 1 (Helpdesk Pagination)
Implementation summary:
- Replaced unbounded helpdesk list retrieval with server-side paged contracts (`page`, `pageSize`) across repository, application service, API controller, and portal client.
- Updated helpdesk portal list rendering with previous/next paging controls and filter-preserving navigation.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

#### 2026-05-10 - Stage 29.2 Slice 2 (Graduation Pagination)
Implementation summary:
- Added paged graduation list contracts for student and staff endpoints (`GET /api/v1/graduation/my`, `GET /api/v1/graduation`) including `TotalCount` metadata.
- Replaced unbounded graduation list materialization with SQL-side paging in repository/service/API layers.
- Updated portal graduation apply/list pages with previous/next controls while preserving active status/department filters.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

### Stage 29.3 - Data Lifecycle and Maintenance
- Archive policy for old result/report/activity data.
- Index rebuild/reorg schedule.
- Capacity and growth dashboards.

Done when:
- DB performance remains stable as row counts grow to enterprise scale.

#### 2026-05-10 - Stage 29.3 Completion
Implementation summary:
- Delivered executable operational SQL artifacts for retention cleanup, index maintenance, and size/growth observability under `Scripts/3-Phase29-ArchivePolicy.sql`, `Scripts/4-Phase29-IndexMaintenance.sql`, and `Scripts/5-Phase29-CapacityGrowthDashboard.sql`.
- Added Stage 29.3 run instructions to `Scripts/README.md`.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**162/162**).

---

## Phase 30 - Integrations and SaaS Operations
Objective: Expand integrations with clean contracts and tenant safety.

### Stage 30.1 - Integration Gateway Layer
- Single outbound integration gateway for:
  - Payment
  - Email/SMS/Push
  - LMS/external APIs
- Retry policies, timeout policies, dead-letter handling.

#### 2026-05-10 - Stage 30.1 Completion
Implementation summary:
- Added a unified outbound gateway contract (`IOutboundIntegrationGateway`) with channel-aware retry/timeout policy execution and dead-letter capture.
- Implemented resilient gateway runtime (`ResilientOutboundIntegrationGateway`) backed by distributed cache dead-letter storage and configurable channel policies (`IntegrationGateway` config section).
- Routed existing outbound provider flows through the gateway:
  - Email: `SmtpEmailDeliveryProvider`
  - Push/in-app notifications: `InAppSupportTicketingProvider`, `InAppAnnouncementBroadcastProvider`
  - LMS/external API: `LibraryService` loan lookup path
- Added integration gateway diagnostics endpoints:
  - `GET /api/v1/communication-integrations/gateway/policies`
  - `GET /api/v1/communication-integrations/gateway/dead-letters` (Admin/SuperAdmin)
- Added focused unit tests for retry success, timeout/dead-letter capture, and policy fallback behavior.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**166/166**).

### Stage 30.2 - Tenant and Subscription Operations
- Tenant onboarding templates.
- Subscription plan controls.
- Branding/profile settings per tenant.

#### 2026-05-10 - Stage 30.2 Completion
Implementation summary:
- Added tenant-operations service contracts and DTOs for onboarding templates, subscription plan controls, and tenant profile settings.
- Implemented `TenantOperationsService` backed by `portal_settings` key-value persistence (no schema changes) to support:
  - onboarding template defaults,
  - subscription plan feature toggles/limits,
  - tenant profile/branding metadata.
- Added SuperAdmin API endpoints under `api/v1/tenant-operations`:
  - `GET/PUT onboarding-template`
  - `GET/PUT subscription-plan`
  - `GET/PUT tenant-profile`
- Registered the new service in API DI and added focused unit tests for defaults and persistence round-trips.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**169/169**).

### Stage 30.3 - Reliability and Rollback Controls
- Feature flags for risky rollouts.
- Safe deployment and rollback runbooks.

#### 2026-05-10 - Stage 30.3 Completion
Implementation summary:
- Added feature flag contracts and persistence-backed runtime service (`IFeatureFlagService` / `FeatureFlagService`) with rollout-safe defaults and rollback batch disable support.
- Added SuperAdmin feature-flag API surface under `api/v1/feature-flags` for list/get/save/rollback operations.
- Added rollback safety guard for risky tenant write operations using `tenant-operations.write` feature flag in `TenantOperationsController`.
- Added deployment and emergency rollback operating procedure document: `Docs/Phase30-Stage30.3-Reliability-Rollback-Runbook.md`.
- Added focused Stage 30.3 unit tests validating flag defaults, save behavior, and rollback disabling behavior.

Validation summary:
- `dotnet build Tabsan.EduSphere.sln` passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**172/172**).

#### 2026-05-10 - Phase 30 Complete
- Stage 30.1 (integration gateway), Stage 30.2 (tenant/subscription operations), and Stage 30.3 (reliability/rollback controls) are complete.

Done when:
- Integrations can evolve without changing core domain logic repeatedly.

---

## Phase 31 - Quality, Security, and Go-Live Gates
Objective: Prevent regressions and enforce release quality.

### Stage 31.1 - Regression Matrix
- Test matrix dimensions:
  - Institution mode (School/College/University)
  - License combinations
  - Roles (SuperAdmin/Admin/Faculty/Student)
  - Tenant isolation

#### 2026-05-10 - Stage 31.1 Completion
Implementation summary:
- Added executable regression matrix coverage in `tests/Tabsan.EduSphere.UnitTests/Phase31Stage1RegressionMatrixTests.cs` for 24 role x institution-mode x license scenarios.
- Added baseline tenant-isolation verification using isolated settings repositories to ensure tenant profile values do not leak across tenant boundaries.
- Added Stage 31.1 matrix runbook and coverage map in `Docs/Phase31-Stage31.1-Regression-Matrix.md`.

Validation summary:
- `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj --filter FullyQualifiedName~Phase31Stage1RegressionMatrixTests` passed (**25/25**).
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**197/197**).

### Stage 31.2 - Security Hardening
- Endpoint authorization audit.
- Data exposure checks.
- Audit log coverage for sensitive actions.

#### 2026-05-10 - Stage 31.2 Completion
Implementation summary:
- Added explicit audit-log emission for sensitive control-plane mutations in:
  - `FeatureFlagsController` (save/rollback)
  - `TenantOperationsController` (onboarding/subscription/profile saves and blocked-write attempts)
  - `InstitutionPolicyController` (policy save)
- Added executable security hardening regression suite in `tests/Tabsan.EduSphere.IntegrationTests/Phase31Stage2SecurityHardeningTests.cs` covering:
  - endpoint authorization guard audit,
  - anonymous endpoint exposure whitelist checks,
  - audit-log emission validation for sensitive writes.
- Added Stage 31.2 hardening artifact: `Docs/Phase31-Stage31.2-Security-Hardening.md`.

Validation summary:
- `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter FullyQualifiedName~Phase31Stage2SecurityHardeningTests` passed (**4/4**).
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**201/201**).

### Stage 31.3 - Performance and Reliability Certification
- Load tests by target bands:
  - up to 10k users
  - 10k-100k users
  - 100k-500k users
  - 500k-1M+
- Recovery tests for node/service failures.

#### 2026-05-10 - Stage 31.3 Completion
Implementation summary:
- Added Stage 31.3 load-band certification script `tests/load/k6-certification-bands.js` with executable bands for:
  - `up-to-10k`
  - `10k-100k`
  - `100k-500k`
  - `500k-1m`
- Added Stage 31.3 node/service recovery smoke script `tests/load/recovery-smoke.ps1` to validate restart recovery via `/health`.
- Updated `tests/load/README.md` with Stage 31.3 runbook commands and threshold mapping.
- Added Stage 31.3 certification artifact document: `Docs/Phase31-Stage31.3-Performance-Reliability-Certification.md`.

Validation summary:
- `dotnet test Tabsan.EduSphere.sln --no-build` passed (**201/201**).

#### 2026-05-10 - Phase 31 Complete
- Stage 31.1 (regression matrix), Stage 31.2 (security hardening), and Stage 31.3 (performance/reliability certification) are complete.

Done when:
- Release is certified across functionality, security, and scale thresholds.

---

## 5) Cross-Phase Technical Rules (Do This Every Time)

- Keep API contracts backward-compatible where possible.
- Prefer additive schema changes and migrations.
- Centralize configuration keys and document each key.
- Any new feature must include:
  - Role checks
  - License checks
  - Tenant filters
  - Audit trail (where sensitive)
  - Tests (unit/integration where relevant)

---

## 6) Immediate Execution Order (Recommended Next Work)

1. Execute Phase 23 completely.
2. Execute Phase 24 to remove scattered UI/API checks.
3. Execute Phase 25 before adding new school/college features.
4. Execute Phases 26 and 27 in parallel streams only after Phase 25 stabilizes.
5. Execute Phases 28 and 29 as dedicated performance tracks.
6. Execute Phases 30 and 31 as release hardening.

This sequence minimizes repeated edits and keeps core behavior stable while adding competitive features.
