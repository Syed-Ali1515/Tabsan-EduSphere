# Institute Parity Issue Fix Phases

This plan defines phased execution to fix School/College/University parity gaps across modules, filters, reports, permissions, and seeded database data.

## Mandatory Stage Closeout Protocol

After completion of every stage in this plan, add both sections under that stage before proceeding:

1. `Implementation Summary`
2. `Validation Summary`

Required minimum content per stage closeout:
Implementation Summary must capture backend/API/service/repository updates, frontend/menu/filter updates, authorization/policy updates, and DB/schema/script updates (if any).
Validation Summary must capture test scope executed (unit/integration/UAT), endpoint and role/institute checks run, regression checks performed, and pass/fail counts with unresolved items.

## Stage Completion Entry Template

Use this exact template after each completed stage:

```md
### Stage X.Y - <Stage Name> (Completed: YYYY-MM-DD)

Implementation Summary
- <change 1>
- <change 2>
- <change 3>

Validation Summary
- Automated tests: <command> -> <result>
- Role/Institute checks: <result>
- Regression checks: <result>
- Residual risks: <none or details>
```

## Mandatory Documentation Sync Per Completed Stage

After every completed stage, update all tracking documents below in the same session:

1. `Docs/Institute-Parity-Issue-Fix-Phases.md`
2. `Docs/Function-List.md`
3. `Docs/Complete-Functionality-Reference.md`
4. `Project startup Docs/Database Schema.md`
5. `Project startup Docs/Development Plan - ASP.NET.md`
6. `Project startup Docs/PRD.md`
7. `Docs/Command.md`

Then run repository sync in order: commit -> pull --rebase -> push.

## Reported Issues (Input Scope)

1. Time Table, Courses, Buildings, Rooms, Departments, Assignments, Enrollments, Reports, Results, Quizzes, Student Lifecycle, Payments, Result Calculation, and Settings are currently University-only in create/edit/use flows.
2. Analytics and Reports are missing School/College filters.
3. Some reports are not working.
4. Student-related submenus are University-only in filters and data behavior.

## Required Outcomes (Definition of Done)

1. Data for all institutes (School, College, University) is available.
2. All users work correctly by assigned institute and role.
3. SuperAdmin has full add/edit/deactivate permissions for core academic and admin modules plus user creation/assignment.
4. Role + institute-based filters are correctly applied across menus.
5. DB scripts include:
   - Institutes (School, College, University)
   - Updated schema/indexing/roles/permissions/functionality access
   - Full dummy data coverage for complete testing
6. Student Lifecycle works correctly by institute.

---

## Phase 0 - Baseline Audit and Gap Mapping

### Stage 0.1 - Module-by-Module Parity Audit
- Verify each module for School/College/University create/read/update/deactivate behavior:
  - Timetable, Courses, Buildings, Rooms, Departments, Assignments, Enrollments
  - Reports, Results, Quizzes, Student Lifecycle, Payments, Result Calculation, Settings
- Capture API endpoints, service methods, repositories, UI menus/forms, and DB dependencies.

### Stage 0.2 - Role and Institute Access Matrix
- Produce matrix: Role x Institute x Module x Action (view/create/edit/deactivate/export).
- Confirm expected SuperAdmin global scope and constrained Admin/Faculty/Student scopes.

### Stage 0.3 - Report Failure Inventory
- List all failing reports with root-cause tags:
  - Query logic
  - Missing joins/filters
  - Incorrect institute scoping
  - Data absence in dummy seed
  - Authorization/policy mismatch

### Stage 0.4 - Exit Criteria
- Signed baseline list of all parity defects.
- Prioritized backlog with severity and dependencies.

---

## Phase 1 - Institute Domain and Data Foundation

### Stage 1.1 - Institute Model Normalization
- Ensure canonical institute dimension supports: School, College, University.
- Verify references from users, departments, programs, courses, offerings, student entities.

### Stage 1.2 - Referential Integrity + Indexing
- Add/adjust constraints and indexes for institute-scoped queries and joins.
- Add covering indexes for high-use filter paths used by reports/analytics.

### Stage 1.3 - Script Hardening
- Update schema script for institute parity objects and constraints.
- Keep scripts idempotent and context-safe for target DB execution.

### Stage 1.4 - Exit Criteria
- Institute types fully represented and queryable across schema.
- No orphaned records for institute-linked entities.

---

## Phase 2 - Authorization and Permission Correction

### Stage 2.1 - SuperAdmin Global Capability
- Enforce full add/edit/deactivate rights for SuperAdmin in all required modules.
- Validate user creation and assignment to roles/departments/courses across institutes.

### Stage 2.2 - Role-Scoped Institute Enforcement
- Admin/Faculty/Student access constrained to assigned institute(s) and role policies.
- Remove University-only assumptions in handlers and policies.

### Stage 2.3 - Menu/Action Guard Consistency
- Align UI menu visibility with backend authorization rules.
- Prevent hidden-action leakage or backend-only forbidden mismatches.

### Stage 2.4 - Exit Criteria
- Role + institute access matrix passes end-to-end authorization tests.

---

## Phase 3 - Module CRUD and Workflow Parity

### Stage 3.1 - Core Academic/Admin Modules
- Fix create/edit/deactivate parity for:
  - Timetable, Courses, Buildings, Rooms, Departments
  - Assignments, Enrollments, Results, Quizzes
  - Payments, Result Calculation, Settings

### Stage 3.2 - Student Lifecycle Institute Parity
- Validate lifecycle workflows by institute:
  - Promotion/hold/withdraw/transfer/graduation states (as implemented)
- Ensure filters, transitions, and reporting match institute constraints.

### Stage 3.3 - Student Submenu Parity
- Fix student-related submenus to support School/College/University data and filters.
- Ensure menu routes and list/detail forms behave consistently by institute.

### Stage 3.4 - Exit Criteria
- All listed modules function correctly for School, College, University.

---

## Phase 4 - Analytics and Reports Parity + Reliability

### Stage 4.1 - Analytics Filter Expansion
- Add institute filters (School/College/University) to analytics dashboards and API queries.
- Ensure role-aware defaults (e.g., constrained roles auto-filtered to assigned institute).

### Stage 4.2 - Reports Filter Expansion
- Add institute filters to report definitions, generation endpoints, and UI controls.
- Ensure exports include correct institute scope.

### Stage 4.3 - Broken Report Fixes
- Repair all failing reports from Phase 0 inventory.
- Add deterministic validation dataset checks per report.

### Stage 4.4 - Exit Criteria
- Reports and analytics are institute-aware and error-free in UAT scenarios.

---

## Phase 5 - Database Scripts and Full Dummy Data Completion

### Stage 5.1 - Core Seed Coverage
- Seed institute-aware foundational data for School, College, University.
- Seed role assignments and access configurations aligned with policy matrix.

### Stage 5.2 - Full Dummy Coverage (All Tables)
- Expand full dummy script to cover all major entities and relationships for parity testing.
- Ensure each institute has representative users, departments, courses, offerings, enrollments, results, quizzes, payments, lifecycle records, and reports artifacts.

### Stage 5.3 - Data Quality and Replay Safety
- Ensure idempotent inserts and stable deterministic identifiers where needed.
- Add post-deployment checks for institute-level row counts and critical workflow entities.

### Stage 5.4 - Exit Criteria
- Full dummy script can populate complete parity test data in one run.

---

## Phase 6 - QA, UAT, and Regression Protection

### Stage 6.1 - Automated Test Expansion
- Add/extend tests for institute parity in API, service, repository, and permission layers.
- Add report generation tests for institute filters and broken-report regressions.

### Stage 6.2 - Cross-Role UAT Matrix
- Validate scenarios for SuperAdmin, Admin, Faculty, Student in School/College/University contexts.
- Include CRUD, filters, reports, analytics, and lifecycle checkpoints.

### Stage 6.3 - Performance and Query Validation
- Validate index effectiveness for institute-filtered pages/reports.
- Confirm no major regressions in common dashboard/report load paths.

### Stage 6.4 - Exit Criteria
- All parity scenarios pass with no critical/blocker defects.

---

## Phase 7 - Release, Documentation, and Operational Readiness

### Stage 7.1 - Deployment Runbook
- Finalize DB script run-order and environment notes.
- Include rollback/verification checklist.

### Stage 7.2 - Functional Documentation Update
- Update functionality and command docs with institute parity behavior.
- Update user guides for role/institute filter behavior.

### Stage 7.3 - Monitoring and Support Handover
- Define report/analytics failure monitoring points.
- Provide issue triage checklist for institute-scope defects.

### Stage 7.4 - Exit Criteria
- Release package ready with docs, scripts, and validated parity behavior.

---

## Traceability Matrix (Issues -> Phases)

- Issue 1 (University-only module options)
  - Phases 1, 2, 3, 5, 6
- Issue 2 (Analytics/reports missing School/College filters)
  - Phase 4 + Phase 6
- Issue 3 (Some reports not working)
  - Phases 0, 4, 5, 6
- Issue 4 (Student submenu University-only)
  - Phases 2, 3, 5, 6

## Traceability Matrix (Requirements -> Phases)

- Requirement 1 (All institute data available)
  - Phases 1, 5
- Requirement 2 (Users work by institute and role)
  - Phases 2, 3, 6
- Requirement 3 (SuperAdmin full permissions)
  - Phase 2 + Phase 6
- Requirement 4 (Proper role/institute filters)
  - Phases 2, 3, 4, 6
- Requirement 5 (DB schema/index/roles/permissions/full dummy)
  - Phases 1, 2, 5
- Requirement 6 (Student lifecycle by institute)
  - Phase 3 + Phase 6

## Recommended Delivery Sequence

1. Complete Phase 0 baseline and sign-off.
2. Deliver Phases 1-2 together (foundation + permissions).
3. Deliver Phase 3 module parity.
4. Deliver Phase 4 analytics/report parity and fixes.
5. Deliver Phase 5 scripts and full dummy coverage.
6. Finalize with Phases 6-7 validation and release readiness.

## Stage Completion Log

### 2026-05-13 - Stage Governance Initialization

Implementation Summary
- Added mandatory stage closeout protocol requiring both Implementation Summary and Validation Summary for every completed stage.
- Added reusable stage completion template to standardize evidence capture.
- Added mandatory cross-document synchronization list and Git sync order for each completed stage.

Validation Summary
- Document structure reviewed and template verified for direct reuse across all upcoming stage updates.
- Cross-document requirements align with Command Center governance and requested tracking files.
- No code/runtime changes introduced by this governance update.

### Stage 0.1 - Module-by-Module Parity Audit (Completed: 2026-05-13)

Implementation Summary
- Completed controller-level audit for parity-scope modules and routes across timetable, course, building, room, department, assignment, enrollment, report, result, quiz, student lifecycle, payments, and settings/branding surfaces.
- Mapped service/repository dependencies from controllers to identify institute-scoping enforcement points (department/offering scoped paths vs global defaults).
- Identified University-default hotspots that still require parity normalization in later stages, including institution-policy defaults, branding/onboarding defaults, AI role prompt wording, and certificate wording.
- Captured DB dependency map through the central DbContext and parity-related policy/settings entities for follow-on Stage 0.2 and Phase 1 remediation.

Validation Summary
- Static audit validation executed via workspace scans and source reads over API controllers, application services, infrastructure services, web policy/UI models, and DB context/entity mappings.
- Verified current role guard and scoped-access patterns exist for core module surfaces (Admin/Faculty/Student/SuperAdmin combinations), with additional institute-specific hardening still required by later stages.
- Confirmed no schema or runtime code mutation in Stage 0.1; this stage produced baseline inventory and dependency evidence only.
- Residual risks: University-centric strings/defaults remain in selected services and templates; these are now explicitly queued for correction in upcoming stages.

### Stage 0.2 - Role and Institute Access Matrix (Completed: 2026-05-13)

Implementation Summary
- Produced baseline role x institute x module x action matrix from API authorization and scope-guard behavior across parity-scope modules.
- Mapped effective access patterns by role for view/create/edit/deactivate/export operations and identified current institute-scope basis (policy flags, department scope, course-offering scope, or global).
- Cataloged enforcement gaps where institute-specific checks are still indirect (department/offering proxies) and require explicit parity hardening in later phases.

Role x Institute x Module x Action Matrix (Baseline)

| Module | SuperAdmin | Admin | Faculty | Student | Institute Scope Basis | Gap / Follow-up |
|---|---|---|---|---|---|---|
| Institution Policy | View/Update | View | View | View | Explicit policy flags (`IncludeSchool/College/University`) | Flags exist, but downstream module enforcement is mixed.
| Admin User Mgmt | View/Create/Update | No | No | No | Global + optional `InstitutionType` assignment | Needs broader institute assignment propagation beyond admin-create path.
| Department | View/Create/Edit/Deactivate | View/Create/Edit/Deactivate | View (assigned depts) | View (filtered read) | Department assignment scoping | Department proxy used; no direct institute claim enforcement.
| Course/Offerings | View/Create/Edit/Deactivate | View/Create/Edit/Deactivate (assigned depts) | View (assigned depts), limited managed actions | View/enroll path via student flows | Department + offering scoping | Institute parity depends on department mappings, not explicit institute checks.
| Timetable | View/Create/Edit/Deactivate/Export | View/Create/Edit/Deactivate/Export | View published | View published | Department-based visibility | Requires institute-aware filter normalization in UI/API combinations.
| Buildings/Rooms | View/Create/Edit/Deactivate | View/Create/Edit/Deactivate | View | View | Global catalog | No institute partitioning currently enforced.
| Assignments | Full manage (author/publish/retract/grade) | Full manage | Full manage | View/submit/own submissions | Offering + role scope | Institute behavior inherited indirectly via offering ownership.
| Enrollment | Admin enroll/drop + roster | Admin enroll/drop + roster | Roster view | Enroll/drop/my courses | Student profile + offering scope | Institute matrix needs explicit checks for cross-institute edge cases.
| Results | Create/publish/correct/view/export | Create/publish/correct/view/export | Create/publish/view/export | View own published | Offering + role scope | Explicit institute filter missing on several result paths.
| Quizzes | Author/publish/grade/view | Author/publish/grade/view | Author/publish/grade/view | Attempt/view own | Offering + policy/role scope | Institute-specific restrictions are mostly implicit.
| Reports/Analytics | View/export (scoped) | View/export (scoped dept) | View/export (scoped offering/dept) | Limited read-only surfaces | Admin dept scope + faculty offering scope | School/College/University filters still incomplete for full parity.
| Student Lifecycle | View/manage promote/deactivate/graduate | View/manage promote/deactivate/graduate | No direct lifecycle mutation | No direct lifecycle mutation | Department/entity context | Institute-type-aware transitions need formalized rules.
| Payments | Create/confirm/cancel/view | Create/confirm/cancel/view | No | View own/submit proof | Role + student ownership | Finance scope present; institute-level fee policy checks not explicit.
| Settings/Branding | View/Update | View | View | View | Global settings + policy overlays | University-default labels/templates still present in several paths.

Validation Summary
- Validation source: controller and service inspections for authorization attributes, role policies, and explicit scope guards (department/offering/user ownership).
- Confirmed SuperAdmin global capability is largely present, Admin/Faculty scoping is mostly department/offering bounded, and Student actions are limited to self-service flows.
- Confirmed institute parity is currently enforced via mixed mechanisms (policy flags and indirect scope proxies), creating inconsistent behavior risk across modules.
- Residual risks: missing explicit institute checks in selected module mutation paths and incomplete School/College filter propagation in reports/analytics.

### Stage 0.3 - Report Failure Inventory (Completed: 2026-05-13)

Implementation Summary
- Created baseline report failure inventory from historical issue logs, current report controller guards, report repository/query patterns, and integration-test coverage.
- Classified each report issue with root-cause tags required by this stage: query logic, missing joins/filters, incorrect institute scoping, data absence, and authorization/policy mismatch.
- Mapped current resolution status (historically fixed vs parity risk still open) to drive Phase 4 remediation prioritization.

Report Failure Inventory (Baseline)

| Report / Surface | Symptom | Root-Cause Tag(s) | Current Status | Follow-up Stage |
|---|---|---|---|---|
| Result Summary (`/api/v1/reports/result-summary`) | Historical `System.InvalidOperationException` during summary load | Query logic | Resolved historically (query/order and safe projection updates) | Stage 4.3 regression verification |
| Report Center catalog visibility | Historical missing report items for privileged roles | Authorization/policy mismatch; data absence in role mapping rows | Resolved historically (SuperAdmin active-report bypass + visibility fixes) | Stage 4.3 regression verification |
| Report exports (job + direct) | Runtime 400/403/404 outcomes when filters/scope are invalid | Authorization/policy mismatch | By design for invalid scope; inventory retained to avoid false-positive defect reports | Stage 4.2 documentation + UX hints |
| Analytics/report institute filters | School/College/University parity filters not consistently explicit across all report paths | Incorrect institute scoping; missing joins/filters | Open parity risk | Stage 4.1 + Stage 4.2 |
| Faculty report generation | Fails without `courseOfferingId` for faculty role | Authorization/policy mismatch | Expected guardrail, but operational friction risk | Stage 4.2 UX + API contract clarity |
| Admin report generation | Fails without explicit department/offering filter in multi-dept admin scope | Authorization/policy mismatch | Expected guardrail, but operational friction risk | Stage 4.2 UX + API contract clarity |
| Transcript report (`/api/v1/reports/student-transcript`) | NotFound when student profile is absent | Data absence in dummy seed | Expected behavior; demo data completeness dependency | Stage 5.2 + Stage 5.3 |
| Low-attendance / semester reports | Potential empty outputs under sparse seed distributions | Data absence in dummy seed | Open demo data quality risk | Stage 5.2 + Stage 5.3 |

Validation Summary
- Evidence sources validated: `Docs/Observed-Issues.md`, report API guard conditions, report repository query builders, and integration tests for catalog and export metadata routes.
- Confirmed historical critical failures (Result Summary exception and Report Center visibility) are documented as resolved with regression safeguards present.
- Confirmed remaining report parity risks are primarily institute filter propagation and seeded-data completeness, not unresolved core runtime crashes.
- Residual risks: parity regressions may still appear where institute-specific constraints are inferred indirectly via department/offering scope rather than explicit institute filters.
