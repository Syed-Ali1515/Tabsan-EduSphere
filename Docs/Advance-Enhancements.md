# Advance Enhancements - Phased Execution Plan

Source input used:
- EduSphere_Competitive_Roadmap.txt
- EduSphere_Million_User_Scalability_Guide.txt
- EduSphere_MSSQL_Indexing_Strategy.txt
- New Enhancements Guide.docx
- University_Portal_PRD.docx

Purpose:
- Arrange upcoming enhancements into dependency-safe phases and stages.
- Avoid repeated code edits by implementing foundations first, then feature layers.
- Preserve core functionality, global configuration behavior, and role-rights policy.

Status:
- In progress (Phase 32 Stage 32.2 completed, ready to proceed with Stage 32.3)

## Execution Updates

### 2026-05-13 - Phase 23 Stage 23.1 (Institution Type Configuration)
- Completed Stage 23.1 by validating existing global institution-mode support across School, College, and University.
- Validation evidence:
	- `InstitutionPolicySnapshot` supports `IncludeSchool`, `IncludeCollege`, and `IncludeUniversity` with backward-compatible University default,
	- `InstitutionPolicyService` persists and resolves mode flags from `portal_settings` keys (`institution_include_school`, `institution_include_college`, `institution_include_university`),
	- seed baseline includes all three institution flags in `Scripts/02-Seed-Core.sql`.
- Behavior impact: no new runtime or schema changes were required; Stage 23.1 was a foundation confirmation closeout.
- Next stage: Stage 23.2 (Dynamic Academic Labels and Context).

### 2026-05-14 - Phase 23 Stage 23.2 (Dynamic Academic Labels and Context)
- Completed Stage 23.2 by verifying and adding integration tests for institution-aware academic vocabulary.
- Implementation evidence (already present):
	- `ILabelService` interface with `AcademicVocabulary` record containing `PeriodLabel`, `ProgressionLabel`, `GradingLabel`, `CourseLabel`, `StudentGroupLabel`,
	- `LabelService` implementation mapping labels by institution type:
		- University (default): Semester / Progression / GPA/CGPA / Course / Batch,
		- School: Grade / Promotion / Percentage / Subject / Class,
		- College: Year / Progression / Percentage / Subject / Year-Group,
	- `LabelController` API endpoint GET /api/v1/labels returning current policy-based vocabulary,
	- `EduApiClient.GetVocabularyAsync()` web layer method consuming label endpoint,
	- `ModuleComposition.cshtml` view displaying dynamic labels from model context,
	- Unit tests in Phase24Tests.cs (LabelServiceTests: 4/4 passed).
- New additions (Stage 23.2):
	- `DynamicLabelIntegrationTests` (8 integration tests) verifying:
		- University/School/College vocabulary retrieval via API endpoint,
		- Mixed-mode common-denominator behavior (University takes precedence when enabled),
		- Dashboard context includes vocabulary for web layer consumption,
		- Unauthenticated access denial (401),
		- Consistency across multiple requests,
		- School+College mode precedence behavior.
	- All 8 new integration tests passing (100%).
- Behavior impact: labels now dynamically adapt by institution policy (tenant-wide, not per-user). Views render correct terminology without code duplication.
- Residual risks: none for Stage 23.2; next stage is Stage 23.3 (Dashboard Context Switching).
- Documentation synchronization completed for Stage 23.2 across planning and tracker docs, including `Docs/Functionality.md`.

### 2026-05-14 - Phase 24 Stage 24.1 (License Flags)
- Completed Stage 24.1 by validating centralized institution-mode license flags and write guards.
- Validation evidence:
	- `InstitutionPolicyService` enforces at least one enabled mode before save (`IncludeSchool` / `IncludeCollege` / `IncludeUniversity`),
	- `InstitutionPolicyController` keeps GET readable by authenticated roles and PUT restricted to `SuperAdmin`,
	- dedicated integration suite `InstitutionPolicyLicenseFlagsIntegrationTests` validates:
		- GET access for SuperAdmin/Admin/Faculty/Student,
		- PUT forbidden for non-SuperAdmin roles,
		- all-false payload rejected with `400 BadRequest`,
		- valid payload persistence and read-back consistency.
- Behavior impact: license-mode configuration remains centralized and deterministic for downstream module filtering phases.
- Residual risks: none for Stage 24.1; next stage is Stage 24.2 (Backend Enforcement).
- Documentation synchronization completed for Stage 24.1 across planning and tracker docs.

### 2026-05-14 - Phase 24 Stage 24.2 (Backend Enforcement)
- Completed Stage 24.2 by introducing centralized backend module-license enforcement before controller execution.
- Implementation evidence:
	- Added `ModuleLicenseEnforcementMiddleware` to map API route prefixes to module keys,
	- Middleware checks `IModuleEntitlementResolver` and returns `403 Forbidden` when module is inactive,
	- Middleware registered in API pipeline after authentication and before authorization to block disabled module APIs consistently.
- Validation evidence:
	- Added `ModuleBackendEnforcementIntegrationTests` verifying disabled-module blocking (`403`) for representative modules and endpoints:
		- courses (`/api/v1/course`),
		- reports (`/api/v1/reports`),
		- ai_chat (`/api/ai/conversations`),
		- fyp (`/api/v1/fyp/{id}`).
	- All Stage 24.2 integration tests passing (4/4).
- Behavior impact: disabled modules are blocked at backend entry consistently, yielding clear forbidden responses without controller-specific duplication.
- Residual risks: none for Stage 24.2; next stage is Stage 24.3 (UI/Navigation Filtering).
- Documentation synchronization completed for Stage 24.2 across planning and tracker docs.

### 2026-05-14 - Phase 24 Stage 24.3 (UI/Navigation Filtering)
- Completed Stage 24.3 by aligning sidebar visibility with module activation state and preserving route guard consistency.
- Implementation evidence:
	- `SidebarMenuController` now applies module-activation filtering to `GET /api/v1/sidebar-menu/my-visible` results,
	- menu-key to module-key mapping added for module-governed areas (courses, reports, ai_chat, themes, sis-derived surfaces, and related entries),
	- existing portal guard flow remains consistent because guarded routes already depend on currently visible sidebar keys.
- Validation evidence:
	- extended `SidebarMenuIntegrationTests` with module-aware navigation checks:
		- disabled courses module hides `courses` menu entry,
		- disabled reports module hides report-related sidebar entries,
		- disabled themes module hides `theme_settings`.
	- test suite stabilized with deterministic module-state setup/restore in class lifecycle to avoid environment-dependent flakiness.
	- sidebar integration suite passing (`17/17`).
- Behavior impact: disabled modules are no longer shown in navigation surfaces and users are less likely to attempt unavailable module flows.
- Residual risks: low; portal route-level behavior remains sidebar-key driven and therefore inherits the filtered visibility contract.
- Documentation synchronization completed for Stage 24.3 across planning and tracker docs.

### 2026-05-14 - Phase 25 Stage 25.1 (Grade/Class Structure)
- Completed Stage 25.1 by introducing an academic-level lifecycle API path and institution-aware period wording in lifecycle navigation.
- Implementation evidence:
	- added `GET /api/v1/student-lifecycle/academic-level-students/{departmentId}/{levelNumber}`,
	- existing semester endpoint preserved as backward-compatible alias,
	- `IStudentLifecycleService` now exposes `GetStudentsByAcademicLevelAsync(...)` with compatibility mapping to existing semester-backed data,
	- portal lifecycle page now resolves dynamic `PeriodLabel` from `GET /api/v1/labels` and renders level controls/headings as Semester/Grade/Year based on institution policy.
- Validation evidence:
	- extended `StudentLifecycleIntegrationTests` with academic-level endpoint coverage,
	- focused lifecycle integration suite passing (`4/4`).
- Behavior impact: lifecycle screens are no longer semester-first in wording, enabling grade/class-oriented flow for School mode while preserving existing contracts.
- Residual risks: low; numeric storage remains `CurrentSemesterNumber` internally, with presentation and routing now academic-level oriented.
- Documentation synchronization completed for Stage 25.1 across planning and tracker docs.

### 2026-05-14 - Phase 25 Stage 25.2 (Stream Support for Grades 9-12)
- Completed Stage 25.2 by enforcing stream assignment eligibility and adding stream-aware subject filtering for student offerings.
- Implementation evidence:
	- `SchoolStreamService.AssignStudentAsync(...)` now requires School institution context, Grade 9-12 level range, and active stream assignment target,
	- `CourseController` now applies student stream filtering for School Grade 9-12 views on:
		- `GET /api/v1/course/offerings`,
		- `GET /api/v1/course/offerings/my`,
	- `SchoolStreamSubjectFilter` introduces stream keyword handling for Science, Biology, Computer, Commerce, and Arts with core-subject inclusion and compatibility fallback.
- Validation evidence:
	- extended `Phase26Tests` with stage-specific stream guard tests:
		- non-school assignment rejection,
		- out-of-range grade rejection,
		- successful eligible assignment and retrieval.
	- focused unit suite passing for new stream guard coverage.
- Behavior impact: School grade-band stream governance is now enforced server-side, and student subject visibility aligns with assigned stream without schema churn.
- Residual risks: low; keyword filtering is naming-dependent and intentionally keeps fallback behavior for legacy datasets until explicit stream-to-subject mapping is introduced.
- Documentation synchronization completed for Stage 25.2 across planning and tracker docs.

### 2026-05-14 - Phase 25 Stage 25.3 (School Grading and Promotion)
- Completed Stage 25.3 by enforcing School pass-rule promotion logic in lifecycle promotion flows.
- Implementation evidence:
	- `StudentLifecycleService.PromoteStudentAsync(...)` now routes School institution promotions through progression evaluation/promotion rules,
	- `ProgressionService` now normalizes School/College progression score interpretation to percentage semantics when legacy GPA-scale values are present,
	- `ProgressionController.GetMyProgression(...)` now accepts `studentProfileId` claim naming with fallback support.
- Validation evidence:
	- extended `StudentLifecycleIntegrationTests` with School promotion failure-path coverage when pass criteria are not met,
	- focused lifecycle integration suite passed after Stage 25.3 updates.
- Behavior impact: School grade promotion now requires pass-threshold eligibility instead of unconditional level increment.
- Residual risks: medium-low; percentage inference still relies on current academic standing fields until full school-native percentage storage is introduced.
- Documentation synchronization completed for Stage 25.3 across planning and tracker docs.

### 2026-05-14 - Phase 31 Stage 31.1 (Institution-Specific Report Sections)
- Completed Stage 31.1 by adding institution-aware report section composition for School, College, and University contexts.
- Implementation evidence:
	- added `GET /api/v1/reports/sections` in `ReportController` with claim-based institution scoping and optional SuperAdmin override,
	- added sectioned response contracts (`InstitutionReportSectionsResponse`, `ReportSectionResponse`, `ReportSectionItemResponse`) for report partition metadata,
	- implemented institution-specific section maps:
		- School: `school_outcomes`,
		- College: `college_progression`,
		- University: `university_academics`.
- Validation evidence:
	- added integration coverage in `ReportExportsIntegrationTests` for:
		- SuperAdmin School override section behavior,
		- Admin claim-based College section behavior without query override.
	- validation run passed for solution build and focused report integration tests.
- Behavior impact: report surfaces can now consume a deterministic institution-partitioned section model while preserving role-based catalog filtering.
- Residual risks: low; endpoint depends on seeded report keys and will omit sections with no role-allowed reports by design.
- Documentation synchronization completed for Stage 31.1 across planning and tracker docs.

### 2026-05-14 - Phase 31 Stage 31.2 (Advanced Analytics)
- Completed Stage 31.2 by adding advanced analytics summaries for top performers, performance trends, and comparative department metrics.
- Implementation evidence:
	- added analytics DTO contracts for advanced analytics reports and rows (`TopPerformersReport`, `PerformanceTrendReport`, `ComparativeSummaryReport`),
	- added new analytics service contract methods and infrastructure implementations with distributed-cache coverage,
	- added new analytics endpoints in `AnalyticsController`:
		- `GET /api/analytics/top-performers`,
		- `GET /api/analytics/performance-trends`,
		- `GET /api/analytics/comparative-summary`.
- Validation evidence:
	- extended `AnalyticsInstituteParityIntegrationTests` with Stage 31.2 scenarios,
	- validated claim-scoped institution behavior for top performers, trends, and comparative summary.
	- focused analytics integration suite passed (`5/5`) and solution build passed.
- Behavior impact: analytics surfaces now provide rank, trend, and cross-department comparative insights while preserving existing role and institution-scope enforcement.
- Residual risks: low; comparative summary currently prioritizes correctness over query minimization and may benefit from later query-shape optimization under very large datasets.
- Documentation synchronization completed for Stage 31.2 across planning and tracker docs.

### 2026-05-14 - Phase 31 Stage 31.3 (Export Enhancements)
- Completed Stage 31.3 by standardizing analytics export metadata and extending PDF/Excel coverage to advanced analytics reports.
- Implementation evidence:
	- added shared analytics export conventions for content type, extension, and filename shape (`analytics-{report-key}-{utcstamp}.{ext}`),
	- standardized sync and queued analytics export filenames/content types through the shared conventions,
	- added new export endpoints for Stage 31.2 advanced analytics:
		- `GET /api/analytics/top-performers/export/pdf|excel`,
		- `GET /api/analytics/performance-trends/export/pdf|excel`,
		- `GET /api/analytics/comparative-summary/export/pdf|excel`,
	- extended queued analytics export support to include advanced analytics report types.
- Validation evidence:
	- added `AnalyticsExportsIntegrationTests` with standardized export metadata assertions across ten analytics export routes,
	- focused analytics parity and export integration suites passed,
	- solution build passed.
- Behavior impact: analytics exports now follow one deterministic naming/content contract across synchronous downloads and queued export jobs, including all advanced analytics report families.
- Residual risks: low; PDF layout uses tabular summaries and may be further refined for visual density in future UX-focused reporting stages.
- Documentation synchronization completed for Stage 31.3 across planning and tracker docs.

---

## Locked Rights and Governance (Must Stay Intact)

These rules are mandatory across all phases:

1. SuperAdmin
- Full rights everywhere: add, edit, deactivate, configure, approve, override.
- Full control of license, institution configuration, grading policy, report configuration.

2. Admin
- Add/edit/deactivate operational academic data in assigned scope:
- Institutes, departments, programs, courses, degrees, students, faculty assignments, results operations.
- No platform-wide license authority; no cross-tenant override (future SaaS mode).

3. Faculty
- Manage only assigned academic workload: teaching content, attendance, grading, feedback.
- No institute-level configuration changes.

4. Student
- Read own academic data and perform allowed self-service actions only.

5. Parent (when enabled)
- Read-only access for linked student(s).

---

## Global Non-Negotiables (Technical)

1. Keep centralized policy checks:
- License checks in one service/middleware path.
- Role authorization in API layer (not UI-only).

2. Keep data isolation:
- Institution-scoped filtering for every query path.
- No mixed data visibility across institution contexts.

3. Keep architecture boundaries:
- Domain interfaces in Domain.
- Implementations in Infrastructure.
- Orchestration in Application.
- Exposure in API/Web.

4. Keep backward compatibility:
- Existing university flows continue working while school/college modes are added.

---

## No-Repeat Build Order

To avoid editing the same core pieces repeatedly, implement in this exact order:

Phase 23 -> Phase 24 -> Phase 25 -> Phase 26 -> Phase 27 -> Phase 28 -> Phase 29 -> Phase 30 -> Phase 31 -> Phase 32 -> Phase 33 -> Phase 34

Reason:
- Institution model first.
- License enforcement second.
- School/college feature modules next.
- Performance/scalability after data and workflow shape is stable.
- SaaS and AI last to avoid rework.

---

## Phase 23 - Institution-Type Foundation
Complexity: High

### Stage 23.1 - Institution Type Configuration
- Add/confirm global institution mode support: School, College, University.
- Keep current university mode as default-safe behavior.

### Stage 23.2 - Dynamic Academic Labels and Context
- Semester/Class/Year labels adapt by institution type.
- GPA/Percentage presentation adapts without duplicating core workflow screens.

### Stage 23.3 - Dashboard Context Switching

- Student/faculty/admin dashboards show only relevant metrics for selected institution type.
- Dashboard widgets and metrics are filtered by both role and institution policy (School/College/University).
- No workflow duplication: one configurable core, no cloned modules.

**Validation:**
- Integration tests in `DashboardContextSwitchingIntegrationTests` verify:
	- Dashboard widgets adapt for all roles (SuperAdmin/Admin/Faculty/Student) and institution types (School/College/University)
	- Vocabulary adapts in dashboard context for each institution type
	- All tests passing (13/13)
- Implementation: `DashboardCompositionService`, `DashboardCompositionController`, web client and view integration.

**Status:** Stage 23.3 completed and validated as of 2026-05-14. Documentation and repo synchronized.

----

## Phase 24 - License-Driven Module Enforcement
Complexity: High
Depends on: Phase 23

### Stage 24.1 - License Flags
- Enforce IncludeSchool / IncludeCollege / IncludeUniversity flags.
- Require at least one active mode.

### Stage 24.2 - Backend Enforcement
- Every request path checks licensed modules before processing.
- Block disabled module APIs with clear forbidden responses.

### Stage 24.3 - UI/Navigation Filtering
- Hide disabled modules from menus and pages.
- Prevent accidental navigation to unavailable areas.

Deliverable goal:
- UI and API both aligned to license scope.

---

## Phase 25 - School System Layer
Complexity: High
Depends on: Phase 24

### Stage 25.1 - Grade/Class Structure
- Grade-oriented academic flow (instead of semester-first behavior).
- Yearly progression model.

### Stage 25.2 - Stream Support (Grades 9-12)
- Science, Biology, Computer, Commerce, Arts stream handling.
- Subject filtering by stream.

### Stage 25.3 - School Grading and Promotion
- Percentage and grade-band result model.
- Promotion logic for next class based on pass rules.

Deliverable goal:
- Complete school workflow without affecting university logic.

---

## Phase 26 - College System Layer
Complexity: Medium
Depends on: Phase 24

### Stage 26.1 - Year-Based Academic Model
- Status: Completed (2026-05-14).
- Year-based progression support for College now reuses `CurrentSemesterNumber` with year mapping (`Year N` == semesters `2N-1` and `2N`).
- Academic-level student retrieval for College now resolves by semester range while preserving semester-based retrieval for School/University.
- Lifecycle promotion for College now routes through progression checks and advances by one academic year (two semesters) when eligible.
- Validation: focused unit and integration coverage added and passing.

### Stage 26.2 - College Result and Promotion
- Status: Completed (2026-05-14).
- Percentage-based grading model remains aligned through institution-aware progression thresholds and percentage normalization.
- Year-to-year promotion now applies through progression orchestration for College (year-step advancement).
- Supplementary handling policy is enforced in bulk promotion: non-eligible college promote entries are converted to hold with supplementary-required reason.

Deliverable goal:
- College workflows enabled with minimal duplication.

---

## Phase 27 - Grading Setup by Institution Type
Complexity: Medium
Depends on: Phases 25-26

### Stage 27.1 - SuperAdmin Grading Setup Sections
- Status: Completed (2026-05-14).
- SuperAdmin grading setup now includes explicit institution sections:
	- School Grading
	- College Grading
	- University Grading
- Implemented through portal section cards with per-section threshold, grade-ranges JSON, and active-state controls backed by institution-grading-profile APIs.

### Stage 27.2 - Rule Application Engine
- Status: Completed (2026-05-14).
- Enrollment prerequisite pass checks now resolve threshold by student institution type from institution grading profiles.
- University prerequisite threshold is normalized from GPA-scale profile values to percentage for result-percentage comparisons.
- Rule checks no longer use a fixed 50% prerequisite pass rule.

Deliverable goal:
- One grading engine with institution-specific configuration.

---

## Phase 28 - Parent Portal (School-Focused)
Complexity: Medium
Depends on: Phase 25

### Stage 28.1 - Parent-Student Mapping
- Status: Completed (2026-05-14).
- Added controlled parent/guardian linking operations with Admin-managed upsert and deactivate flows.
- Link creation now validates Parent role and School-student scope before persistence.
- Parent self-view remains read-only and only returns active links.

### Stage 28.2 - Parent Read-Only Views
- Status: Completed (2026-05-14).
- Added linked-student read-only endpoints for results, attendance, announcements, and timetable.
- Enforced active parent-student link checks before returning student-scoped data.
- Preserved non-mutation behavior by exposing GET-only parent self-view routes.
- Validation: focused unit tests passed (`16/16`) and parent-portal integration tests passed (`10/10`).

### Stage 28.3 - Parent Notifications
- Status: Completed (2026-05-14).
- Added parent notification fan-out for result publication events.
- Added parent attendance-warning notifications from the background alert job.
- Added parent recipients to announcement broadcast fan-out for linked students.
- Validation: API and BackgroundJobs builds passed, unit tests passed (`144/144`), and parent-portal integration tests passed (`10/10`).

Deliverable goal:
- Parent transparency without role-risk expansion.

---

## Phase 29 - Performance Foundation (MSSQL + Query Discipline)
Complexity: Medium
Depends on: None (recommended after feature stabilization)

### Stage 29.1 - Indexing Plan Implementation
- Status: Completed (2026-05-14).
- Added parent-link notification hot-path index `(StudentProfileId, IsActive)` to accelerate Stage 28.3 parent fan-out lookups.
- Added EF migration `_20260514_Phase29_ParentLinkStudentActiveIndexHotPath` for schema deployment.
- Validation: build and targeted tests passed after index and migration updates.

### Stage 29.2 - Pagination and Projection Enforcement
- Status: Completed (2026-05-14).
- Delivered pagination and projection contracts across high-volume list paths:
	- Helpdesk tickets (Slice 1),
	- Graduation applications (Slice 2),
	- Payment receipts (Slice 3).
- Replaced unbounded list materialization with SQL-side page/pageSize query patterns and total-count metadata.
- Validation: build and automated test suites passed for Stage 29.2 delivery.

### Stage 29.3 - Query and Transaction Optimization
- Status: Completed (2026-05-14).
- Added operational lifecycle scripts for performance sustainability:
	- `Scripts/3-Phase29-ArchivePolicy.sql` (retention/cleanup policy with dry-run default),
	- `Scripts/4-Phase29-IndexMaintenance.sql` (fragmentation-aware index maintenance plan/execution),
	- `Scripts/5-Phase29-CapacityGrowthDashboard.sql` (capacity and growth telemetry dashboard).
- Updated `Scripts/README.md` with Stage 29.3 run commands and safe execution guidance.
- Validation: scripts compile as T-SQL batches and were reviewed for dry-run-first safety defaults.

### Phase 29 Completion
- Status: Completed (2026-05-14).
- Stage 29.1 delivered baseline and follow-up hot-path indexes.
- Stage 29.2 delivered pagination discipline on helpdesk/graduation/payment heavy-list routes.
- Stage 29.3 delivered operational archive/index/capacity scripts and runbook guidance.

Deliverable goal:
- Stable performance for high concurrency growth.

---

## Phase 30 - Distributed Cache and Background Processing
Complexity: High
Depends on: Phase 29

### Stage 30.1 - Redis Caching
- Status: Completed (2026-05-14).
- Added distributed-cache layer for dashboard context summaries (`/api/v1/dashboard/context`) with short TTL for role/policy scoped payload reuse.
- Added distributed-cache layer for report summary reads (attendance, result, assignment, quiz, GPA, enrollment, semester results) using parameterized cache keys.
- Added distributed-cache layer for common tenant profile reads (onboarding template, subscription plan, tenant profile) with write-path cache invalidation.
- Validation: solution build and unit tests passed after cache integration changes.

### Stage 30.2 - Background Job Offloading
- Status: Completed (2026-05-14).
- Expanded background offloading for heavy analytics exports by adding queued job APIs and a hosted worker for performance/attendance PDF/Excel generation.
- Existing offloading coverage remains active for large notification fan-out and report export workflows.
- Added async analytics export lifecycle endpoints (`queue`, `status`, `download`) so request threads return quickly under load while heavy generation executes in background.
- Validation: solution build and unit tests passed after queue + worker integration changes.

### Stage 30.3 - Reliability Controls
- Status: Completed (2026-05-14).
- Added configurable retry strategy with bounded backoff for background workers handling result publish, report export, and analytics export jobs.
- Added operational alerting via consecutive-failure threshold logging for each background worker pipeline.
- Added `/health/background-jobs` endpoint exposing retry configuration and live background-job processing/retry/failure metrics.
- Validation: solution build and unit tests passed after reliability-control integration changes.

Deliverable goal:
- Fast user-facing responses under load.

---

## Phase 31 - Reporting and Analytics Expansion
Complexity: Medium
Depends on: Phases 27, 29, 30

### Stage 31.1 - Institution-Specific Report Sections
- Status: Completed (2026-05-14).
- Added `GET /api/v1/reports/sections` for institution-aware report section composition.
- Added section contracts for institution model + grouped report items in reporting DTOs.
- Added School/College/University section partition behavior with role-filtered report inclusion.
- Validation: focused report integration tests passed for School override and College claim scope behavior.

### Stage 31.2 - Advanced Analytics
- Status: Completed (2026-05-14).
- Added top performers analytics endpoint with ranked performance rows (`GET /api/analytics/top-performers`).
- Added daily performance trend analytics endpoint (`GET /api/analytics/performance-trends`).
- Added comparative department summary analytics endpoint (`GET /api/analytics/comparative-summary`).
- Validation: focused analytics parity integration tests passed for institution-claim auto-scope behavior.

### Stage 31.3 - Export Enhancements
- Status: Completed (2026-05-14).
- Standardized analytics export filenames and content-type contracts across sync and queued exports.
- Added PDF/Excel export support for top performers, performance trends, and comparative summary analytics reports.
- Added integration test coverage validating metadata contract consistency across analytics export routes.

Deliverable goal:
- Actionable reporting without real-time heavy query pressure.

---

## Phase 32 - Communication Enhancements
Complexity: Medium
Depends on: Existing notifications

### Stage 32.1 - In-Portal Messaging
- Status: Completed (2026-05-14).
- Implemented AI chatbot entry-point improvement by removing menu-based chatbot discovery and introducing a persistent floating launcher in the shared portal layout.
- Launcher behavior:
	- fixed bottom-right placement across portal pages,
	- click/tap opens the existing AI chat interface,
	- visibility is role/module aware (shown only when chat module is available in current sidebar visibility contract),
	- responsive spacing for mobile and desktop with overlap-safe positioning.
- Optional UX polish implemented: subtle pulse animation with reduced-motion accessibility fallback.
- Validation: `dotnet build src/Tabsan.EduSphere.Web/Tabsan.EduSphere.Web.csproj` passed.

Stage planning addendum (2026-05-14):
- Add AI chatbot UI entry-point improvement to increase accessibility and reduce interaction friction.
- Move chatbot access from menu-only discovery to an always-visible floating launcher.
- Scope for Stage 32.1 execution:
	- remove primary chatbot entry from menu navigation,
	- add persistent floating chatbot icon/button at bottom-right across portal pages,
	- open chatbot interface through click/tap action (modal or drawer),
	- maintain responsive behavior for desktop and mobile layouts,
	- include overlap-safe positioning so critical UI actions remain unobstructed.
- Optional UX polish:
	- subtle pulse/bounce attention animation,
	- unread-message notification badge.
- Expected benefit:
	- faster chatbot access,
	- improved engagement,
	- modern interaction pattern parity with contemporary web applications.

### Stage 32.2 - Email Integration
- Status: Completed (2026-05-14).
- Implemented event-triggered notification email dispatch on top of existing in-app notification flows.
- Delivery behavior:
	- every in-app notification send path now optionally dispatches recipient email notifications,
	- recipient email list is resolved from active user accounts with non-empty email addresses,
	- dispatch uses template-based HTML rendering (`notification-alert`) and resilient outbound email provider execution.
- Configuration and free-service alignment:
	- added `NotificationEmail` settings section for enable/disable, subject prefix, and portal notification URL,
	- production/default SMTP host aligned to Brevo free-tier-compatible relay (`smtp-relay.brevo.com`) with credential placeholders,
	- development/default `NotificationEmail:Enabled` remains `false` to avoid local test breakage without SMTP credentials.
- Validation:
	- focused notification unit tests passed,
	- full solution build passed.

### Stage 32.3 - SMS Integration
- High-priority SMS alerts.

Deliverable goal:
- Multi-channel communication with role and event controls.

---

## Phase 33 - SaaS and Multi-Tenant Readiness
Complexity: High
Depends on: Phases 23-24

### Stage 33.1 - Tenant Isolation Model
- Tenant-aware data boundaries and filtering.

### Stage 33.2 - Subscription Management
- Plan lifecycle, expiry warnings, subscription state checks.

### Stage 33.3 - Onboarding and Branding
- Tenant onboarding workflow and branding customization.

Deliverable goal:
- Production SaaS posture with clear tenant boundaries.

---

## Phase 34 - Security and Reliability Hardening
Complexity: Medium
Depends on: Existing auth and audit systems

### Stage 34.1 - MFA and Access Hardening
- Multi-factor authentication for privileged roles.

### Stage 34.2 - Audit and Compliance Logging
- Sensitive action trail coverage with searchable audit history.

### Stage 34.3 - Failure and Recovery Readiness
- Backup/restore drills and rollback-safe deployment playbook.

Deliverable goal:
- Enterprise security baseline and operational trust.

---

## Implementation Guardrails (To Prevent Rework)

1. One-time foundation changes only in early phases:
- Institution type and license checks implemented once, reused everywhere.

2. Additive evolution:
- New behavior added behind configuration/license gates.
- Do not fork code paths unnecessarily.

3. Shared service policy:
- Authorization, license, filtering, and config retrieval remain centralized.

4. Avoid repeated schema churn:
- Group related DB changes per phase migration.
- Do not split one concern across multiple migrations unless required.

5. Role-rights regression checks each phase:
- SuperAdmin full rights remain intact.
- Admin operational rights (institutes/courses/degrees/results) remain intact.

---

## Acceptance Checklist Per Phase

- Core functionality unchanged for existing live flows.
- Configuration behavior consistent across UI, API, and jobs.
- Role-right matrix validated with test scenarios.
- No cross-institution data leaks.
- No repeated modifications to already-stabilized foundation layers.
- Build passes and migration path is clean.

---

## Compact Execution Table (Sprint Planning)

Use this table to schedule work without revisiting phase design decisions.

| Phase | Priority | Suggested Owner | ETA | Risk | Entry Gate |
|-------|----------|-----------------|-----|------|------------|
| 23 | P0 | Core Platform Team | 2-3 weeks | High | Phase 21 stable on main |
| 24 | P0 | Core Platform Team | 1-2 weeks | High | Phase 23 complete |
| 25 | P1 | Academic Features Team | 2-3 weeks | High | Phase 24 complete |
| 26 | P1 | Academic Features Team | 1-2 weeks | Medium | Phase 24 complete |
| 27 | P1 | Results/Rules Team | 1-2 weeks | Medium | Phases 25-26 complete |
| 28 | P2 | Portal UX Team | 1-2 weeks | Medium | Phase 25 complete |
| 29 | P0 | Data/Infra Team | 1-2 weeks | Medium | Feature schema freeze for current sprint |
| 30 | P0 | Infra + Background Jobs Team | 2-3 weeks | High | Phase 29 complete |
| 31 | P2 | Reporting Team | 1-2 weeks | Medium | Phases 27, 29, 30 complete |
| 32 | P2 | Communication Team | 1-2 weeks | Medium | Notification baseline validated |
| 33 | P3 | Platform SaaS Team | 3-5 weeks | High | Phases 23-24 complete + security review |
| 34 | P1 | Security Team | 1-2 weeks | Medium | Auth and audit baselines confirmed |

### Suggested Delivery Waves

1. Wave A (foundation): 23, 24, 29
2. Wave B (institution feature layer): 25, 26, 27, 28
3. Wave C (scale + insights): 30, 31, 32
4. Wave D (platform maturity): 33, 34

### Definition of Done for Each Phase

- Rights matrix validated (SuperAdmin/Admin behavior preserved).
- License + role checks verified at API and UI layers.
- Query/data isolation checks passed for institution context.
- Migration applied cleanly in Development and Staging.
- Regression tests pass for previously completed phases.
