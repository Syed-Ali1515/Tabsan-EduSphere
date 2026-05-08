# Advance Enhancements - Consolidated Phases and Stages

Source inputs used:
- New Enhancements/EduSphere_Competitive_Roadmap.txt
- New Enhancements/EduSphere_Million_User_Scalability_Guide.txt
- New Enhancements/EduSphere_MSSQL_Indexing_Strategy.txt
- New Enhancements/New Enhancements Guide.docx
- New Enhancements/University_Portal_PRD.docx

Status: Planned
Scope start: After Phase 22

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

### Stage 29.3 - Data Lifecycle and Maintenance
- Archive policy for old result/report/activity data.
- Index rebuild/reorg schedule.
- Capacity and growth dashboards.

Done when:
- DB performance remains stable as row counts grow to enterprise scale.

---

## Phase 30 - Integrations and SaaS Operations
Objective: Expand integrations with clean contracts and tenant safety.

### Stage 30.1 - Integration Gateway Layer
- Single outbound integration gateway for:
  - Payment
  - Email/SMS/Push
  - LMS/external APIs
- Retry policies, timeout policies, dead-letter handling.

### Stage 30.2 - Tenant and Subscription Operations
- Tenant onboarding templates.
- Subscription plan controls.
- Branding/profile settings per tenant.

### Stage 30.3 - Reliability and Rollback Controls
- Feature flags for risky rollouts.
- Safe deployment and rollback runbooks.

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

### Stage 31.2 - Security Hardening
- Endpoint authorization audit.
- Data exposure checks.
- Audit log coverage for sensitive actions.

### Stage 31.3 - Performance and Reliability Certification
- Load tests by target bands:
  - up to 10k users
  - 10k-100k users
  - 100k-500k users
  - 500k-1M+
- Recovery tests for node/service failures.

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
