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

### Stage 0.4 - Exit Criteria (Completed: 2026-05-13)

Implementation Summary
- Completed Phase 0 baseline sign-off by consolidating outputs from Stage 0.1 (module audit), Stage 0.2 (role/institute matrix), and Stage 0.3 (report failure inventory).
- Produced prioritized parity backlog slices and sequencing alignment for Phase 1 through Phase 5 execution.
- Confirmed traceability linkage from reported issues to planned remediation phases is complete and actionable.

Prioritized Backlog (Phase 0 Sign-off Output)

| Priority | Defect Cluster | Primary Risk | Planned Phase / Stage |
|---|---|---|---|
| P0 | Explicit institute scoping in report/analytics filters | Incorrect School/College/University data visibility | Phase 4 Stage 4.1 + 4.2 |
| P0 | Role-scope parity hardening on mutation paths using indirect scope proxies | Cross-institute authorization drift | Phase 2 Stage 2.2 + 2.3 |
| P1 | University-default labels/templates/messages in mixed-mode contexts | Incorrect institute semantics in UX/docs | Phase 3 Stage 3.3 + Phase 7 Stage 7.2 |
| P1 | Dummy data sparsity for report/transcript parity scenarios | False-negative UAT outcomes and empty report outputs | Phase 5 Stage 5.2 + 5.3 |
| P2 | Guardrail UX clarity for expected 400/403 report outcomes | Support load and mis-triaged defects | Phase 4 Stage 4.2 + 4.3 |

Validation Summary
- Verified Phase 0 exit conditions are satisfied:
  - baseline parity defect inventory is documented,
  - role/institute/module/action matrix is documented,
  - report failure inventory is documented with root-cause tags,
  - prioritized remediation mapping is present.
- Verified no unresolved blocker in Phase 0 artifacts that prevents starting Phase 1 execution.
- Residual risks accepted for next phase execution: institute-filter parity and seed completeness remain open by design and are queued in planned stages.

### Stage 1.1 - Institute Model Normalization (Completed: 2026-05-13)

Implementation Summary
- Normalized the canonical institute dimension by adding `InstitutionType` to the `Department` domain model, including constructor assignment and controlled mutation through `SetInstitutionType`.
- Updated EF Core configuration for departments to persist `InstitutionType` with a default value (`University`) and added index `IX_departments_institution_type` for institute-scoped query paths.
- Added migration `20260513121000_Phase1Stage11DepartmentInstitutionType` to introduce the new `departments.InstitutionType` column and supporting index.
- Updated Department API create/update/read contracts to include institution type and enforce current license policy checks via `IInstitutionPolicyService`.
- Updated Web API client department payload handling to round-trip institution type data without breaking existing create/update flows.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln` -> passed after updating one pre-existing unit test constructor call to include optional institution-type argument.
- Automated tests: `SecurityValidationTests` -> `4/4` passed.
- Role/Institute checks: Department create/update now rejects institution types disabled by active policy; default create path remains University-compatible.
- Regression checks: Existing department CRUD request shapes remain compatible because create defaults to University and update keeps institution type optional.
- Residual risks: Phase 1.2 still required for broader referential/index tuning across additional institute-filter-heavy query paths.

### Stage 1.2 - Referential Integrity + Indexing (Completed: 2026-05-13)

Implementation Summary
- Tightened referential integrity for academic write paths by enforcing department existence before course creation, semester/course existence before offering creation, and faculty-to-department assignment validation when faculty is bound to a new offering.
- Hardened student-profile integrity by validating Program/Department alignment in both whitelist self-registration and admin profile creation flows.
- Normalized academic program uniqueness to department scope (`Code + DepartmentId`) instead of global code-only uniqueness.
- Added Stage 1.2 index coverage for high-use institute-scoped/report paths:
  - programs (`DepartmentId + IsActive`),
  - courses (`DepartmentId + IsActive`),
  - offerings (`SemesterId + IsOpen`, `FacultyUserId + IsOpen`),
  - student profiles (`DepartmentId + Status`, `ProgramId + Status`),
  - enrollments (`CourseOfferingId + Status`, `StudentProfileId + Status`),
  - assignment lookups (`AdminUserId/FacultyUserId + RemovedAt + DepartmentId`).
- Added migration `20260513124500_Phase1Stage12ReferentialIntegrityAndIndexes` and adjusted enrollment status column length to support indexed status filters in SQL Server.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln` -> passed.
- Automated tests: targeted validation set -> `AdminUserManagementIntegrationTests` + `SecurityValidationTests` passed (`8/8`).
- Role/Institute checks: department/course/offering creation paths now reject cross-scope or missing-link references earlier with explicit `BadRequest` responses.
- Regression checks: existing department/program/course flows remain functional; integration suite confirmed admin-user + department assignment round-trips remain green after index/integrity changes.
- Residual risks: Stage 1.3 still required to align SQL script artifacts with new index/constraint posture for deployment replay safety.

### Stage 1.3 - Script Hardening (Completed: 2026-05-13)

Implementation Summary
- Updated `Scripts/01-Schema-Current.sql` with idempotent Stage 1.1 and Stage 1.2 migration-aligned blocks so schema-only deployments now apply:
  - `departments.InstitutionType` with default and index,
  - academic/report parity index pack,
  - enrollment status column normalization to `nvarchar(32)`,
  - migration-history inserts for Stage 1.1 and Stage 1.2 IDs.
- Updated `Scripts/04-Maintenance-Indexes-And-Views.sql` to add replay-safe parity maintenance operations for institute/department/offering/student/enrollment/assignment index paths and safe legacy-index replacement for program uniqueness.
- Updated `Scripts/05-PostDeployment-Checks.sql` with explicit parity checks for:
  - stage migration presence,
  - critical column existence/shape,
  - critical index existence.

Validation Summary
- Script validation: verified stage migration IDs and new index/column checks are present in schema, maintenance, and post-deployment scripts.
- Idempotency checks: all new DDL operations are guarded by `COL_LENGTH`, `sys.indexes`, and migration-history existence checks.
- Regression checks: application code/tests remained unchanged by Stage 1.3 script-only hardening; no runtime behavior change expected until scripts are executed.
- Residual risks: Stage 1.4 remains for formal exit verification after script execution in target environments.

### Stage 1.4 - Exit Criteria (Completed: 2026-05-13)

Implementation Summary
- Extended `Scripts/05-PostDeployment-Checks.sql` with explicit Phase 1 exit-criteria checks for institute representation and orphan detection across institute-linked entities.
- Added institute-type validation checks:
  - invalid department institution-type count,
  - distinct department institution-type coverage count,
  - per-type counts for School (`0`), College (`1`), University (`2`).
- Added orphan-count checks for key institute-linked relationships:
  - academic programs -> departments,
  - courses -> departments,
  - student profiles -> departments/programs,
  - course offerings -> courses/semesters,
  - enrollments -> student profiles/course offerings,
  - faculty/admin department assignments -> departments.
- No API/service/repository/front-end logic change in Stage 1.4; this stage closes Phase 1 with script-verifiable data integrity evidence.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AdminUserManagementIntegrationTests|FullyQualifiedName~SecurityValidationTests" -v minimal` -> passed (`4/4`).
- Role/Institute checks: verified Stage 1.4 post-deployment check markers are present for institute coverage and orphan counts.
- Regression checks: no runtime code paths changed in Stage 1.4; build and targeted integration/security checks remained green.
- Residual risks: final orphan/coverage numeric outcomes depend on execution against target database data; script checks are now in place for deterministic verification.

### Stage 2.1 - SuperAdmin Global Capability (Completed: 2026-05-13)

Implementation Summary
- Extended SuperAdmin user-assignment capabilities in `DepartmentController` by adding full faculty department-assignment management endpoints:
  - assign faculty to department,
  - remove faculty from department,
  - list faculty department assignments,
  - list active faculty users for assignment workflows.
- Strengthened assignment integrity for cross-institute operation by enforcing institution-type compatibility checks on assignment writes:
  - admin-to-department assignment rejects institution mismatch,
  - faculty-to-department assignment rejects institution mismatch.
- Expanded assignment management response payloads with user institution type for better SuperAdmin visibility in institute-aware assignment flows.
- Added request contract support for faculty-assignment revoke operations (`RemoveFacultyFromDepartmentRequest`).
- Validation coverage extended through integration tests for:
  - SuperAdmin faculty assignment round-trip,
  - institution-mismatch rejection on admin department assignment.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AdminUserManagementIntegrationTests" -v minimal` -> passed (`6/6`).
- Role/Institute checks: SuperAdmin can now manage faculty department assignments directly and receives deterministic `BadRequest` responses for cross-institute mismatched assignment attempts.
- Regression checks: existing admin user create/update/assignment integration flows remained green in the same test suite.
- Residual risks: Stage 2.2 is still required to complete broader role-scoped institute enforcement across remaining module handlers and policies.

### Stage 2.2 - Role-Scoped Institute Enforcement (Completed: 2026-05-13)

Implementation Summary
- Added token-level institution scope propagation for authenticated users with explicit institution assignment:
  - `TokenService` now emits `institutionType` claim in JWT access tokens when the user has an assigned institution type.
- Hardened report handler scope enforcement in `ReportController` for non-SuperAdmin roles:
  - Admin and Faculty report requests now enforce department institution-type compatibility when `institutionType` claim is present.
  - Existing admin department-assignment and faculty offering-ownership checks remain in place and are now composed with institute checks.
- Added focused integration regression coverage proving role scope + institute scope composition:
  - admin with valid department assignment but mismatched institution claim is denied (`403`) on report endpoint access.
- Frontend/menu updates: none in this stage.
- Authorization/policy updates: handler-level scope enforcement extended in report endpoints; no policy name changes required.
- DB/schema/script updates: none.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~ReportExportsIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests" -v minimal` -> passed (`20/20`).
- Role/Institute checks: verified Admin access is denied when department scope is valid but institute claim mismatches target department.
- Regression checks: export endpoint metadata tests and SuperAdmin/Admin assignment management integration tests remained green.
- Residual risks: Stage 2.3 still required to align menu/action guard consistency and remove any remaining backend/UI authorization mismatches.

### Stage 2.3 - Menu/Action Guard Consistency (Completed: 2026-05-13)

Implementation Summary
- Added centralized menu/action guard enforcement in `PortalController` so direct portal URL navigation is validated against current sidebar visibility rules for non-SuperAdmin users.
- Added explicit action-to-menu-key mapping for parity-scope menu routes to keep UI-visible navigation and backend-invokable portal actions aligned.
- Preserved SuperAdmin global bypass behavior while enforcing redirect-on-deny behavior for hidden sections to prevent hidden-action leakage.
- Added integration checks in `SidebarMenuIntegrationTests` to assert consistency between hidden menu state and endpoint authorization outcomes (`403` for hidden settings path, `200` for SuperAdmin visible path).
- DB/schema/script updates: none.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~SidebarMenuIntegrationTests" -v minimal` -> passed (`14/14`).
- Role/Institute checks: verified Admin hidden sidebar settings path remains blocked while SuperAdmin visible settings path remains accessible.
- Regression checks: existing sidebar role matrix tests remained green alongside new guard consistency tests.
- Residual risks: write-action routing outside mapped menu surfaces still relies on downstream API authorization and should be expanded in later parity hardening stages.

### Stage 2.4 - Exit Criteria (Completed: 2026-05-13)

Implementation Summary
- Completed Phase 2 authorization closure by validating the consolidated role + institute access matrix across SuperAdmin/Admin/Faculty/Student behavior surfaces.
- Consolidated Stage 2 evidence from:
  - SuperAdmin assignment capability and institute-compatibility enforcement (Stage 2.1),
  - role-scoped institute enforcement on report handlers (Stage 2.2),
  - menu/action guard consistency between visible sidebar routes and direct portal access (Stage 2.3).
- Verified the Stage 2 matrix through end-to-end integration suites covering assignment authorization, report scope authorization, and sidebar/section guard consistency.
- Backend/API/service/repository updates: none in Stage 2.4 (validation and closeout stage).
- Frontend/menu/filter updates: none in Stage 2.4 (validation and closeout stage).
- Authorization/policy updates: none in Stage 2.4 (validation and closeout stage).
- DB/schema/script updates: none.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AdminUserManagementIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests" -v minimal` -> passed (`34/34`).
- Role/Institute checks:
  - SuperAdmin assignment administration and institute-compatible assignment paths passed,
  - Admin/Faculty report access institute-scope checks passed (including mismatch denial),
  - Student/Admin/Faculty/SuperAdmin sidebar visibility and guarded section access expectations passed.
- Regression checks: all selected Stage 2 suites passed with no failures and no new unresolved authorization mismatches.
- Residual risks: broader module parity beyond Stage 2 authorization scope remains in Phase 3+ execution backlog.

### Stage 3.1 - Core Academic/Admin Modules (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service updates:
  - updated Web-to-API department create/update flow to pass explicit `institutionType` instead of silently forcing University mode,
  - updated department update flow to support institution-type edits from portal management surfaces.
- Frontend/menu/filter updates:
  - updated Departments portal page to display each department's institution type (School/College/University),
  - added institution-type selector to department create modal,
  - added institution-type selector to department edit modal and bound existing value during edit open.
- Authorization/policy updates:
  - added end-to-end integration validation that temporarily enables all institution policy flags and verifies core department/course CRUD operations succeed across School/College/University contexts.
- Repository/test updates:
  - added `DepartmentAndCourse_Crud_WorksAcrossAllInstitutionTypes_WhenPolicyEnablesAll` integration test,
  - hardened existing admin assignment round-trip test to select/create institution-compatible departments in mixed-institution datasets.
- DB/schema/script updates: none.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AdminUserManagementIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests" -v minimal` -> passed (`35/35`).
- Role/Institute checks:
  - verified department create/update and course create/update/deactivate flows execute successfully for School/College/University when policy enables all three,
  - verified existing role/institute authorization and menu/report guard suites remain green.
- Regression checks: Stage 2 authorization and sidebar/report parity tests remained green after Stage 3.1 changes.
- Residual risks: additional module parity hardening for timetable/assignments/enrollments/results/quizzes/payments/settings remains in upcoming Phase 3 stages.

### Stage 3.2 - Student Lifecycle Institute Parity (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service updates:
  - added institute-aware lifecycle scope enforcement in `StudentLifecycleController` for graduation candidates, semester-student listing, graduate, promote, deactivate, reactivate, and lifecycle batch endpoints,
  - enforced Admin department-assignment scope checks on lifecycle endpoints before lifecycle operations are executed,
  - added student-target-to-department scope resolution in lifecycle endpoint guards so student-level mutations cannot bypass department/institute boundaries.
- Frontend/menu/filter updates:
  - added session-level `institutionType` decoding in web token identity so portal lifecycle screens can apply institute-aware filtering,
  - filtered Student Lifecycle department dropdown by caller institute type for non-SuperAdmin sessions,
  - fixed lifecycle action wiring to preserve selected department/semester filters across promote/graduate actions.
- Authorization/policy updates:
  - aligned lifecycle authorization behavior with Stage 2 institute-scope guard model used by report endpoints,
  - preserved SuperAdmin global access behavior while constraining Admin flows to assignment + institute scope.
- DB/schema/script updates: none.
- Repository/test updates:
  - added dedicated lifecycle integration suite `StudentLifecycleIntegrationTests` covering admin institution mismatch deny behavior on graduation candidate read and promote mutation endpoints.

Validation Summary
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~StudentLifecycleIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests" -v minimal` -> passed (`37/37`).
- Role/Institute checks:
  - verified Admin requests with valid department assignment but mismatched institution claim are denied (`403`) on lifecycle candidate and promote paths,
  - verified Stage 2 assignment/report/sidebar authorization suites remain green with lifecycle scope hardening in place,
  - verified SuperAdmin authorization behavior remains unaffected by institute-claim restrictions.
- Regression checks: no new failures observed in Stage 2 authorization/menu/report and Stage 3.1 admin-management parity suites.
- Residual risks: lifecycle parity for hold/withdraw/transfer/graduation reporting depth and student submenu parity breadth continues in Stage 3.3.

### Stage 3.3 - Student Submenu Parity (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service updates:
  - hardened `StudentController` student-list endpoint with Admin assignment scope checks to ensure submenu data cannot include out-of-assignment departments,
  - added institution-claim compatibility checks for student list queries, including explicit forbidden behavior when requested department institution type mismatches caller institution claim,
  - aligned role behavior so SuperAdmin remains global while Admin/Faculty student-list reads stay constrained by assigned scope and institute compatibility.
- Frontend/menu/filter updates:
  - updated student submenu UI labels from `Semester` to institute-neutral `Level` in Students and Enrollments pages to remove University-only terminology assumptions,
  - kept existing submenu routes and forms intact while ensuring displayed terminology is consistent across School/College/University contexts.
- Authorization/policy updates:
  - extended institute-scope enforcement from reports/lifecycle into student-submenu data endpoint surfaces that power Students/Enrollments/Payments filtering paths.
- DB/schema/script updates: none.
- Repository/test updates:
  - added `StudentSubmenuParityIntegrationTests` verifying admin institution mismatch denial and institute-scoped student list filtering behavior.

Validation Summary
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~StudentSubmenuParityIntegrationTests|FullyQualifiedName~StudentLifecycleIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests" -v minimal` -> passed (`39/39`).
- Role/Institute checks:
  - verified `GET /api/v1/student?departmentId=<dept>` returns `403` when Admin institution claim mismatches target department institution,
  - verified `GET /api/v1/student` returns only institute-compatible students for Admin callers even when assignment rows span mixed institution departments,
  - verified Stage 2 role/institute report/menu guards and Stage 3.2 lifecycle scope checks remain green.
- Regression checks: no new failures in focused Stage 2+3 integration suites.
- Residual risks: broader student-submenu parity for additional institute-adaptive terminology/widgets and cross-page filter cohesion can be expanded in Stage 3.4 closeout hardening.

### Stage 3.4 - Exit Criteria (Completed: 2026-05-13)

Implementation Summary
- Completed Phase 3 exit-criteria consolidation across Stage 3.1 (core module CRUD parity), Stage 3.2 (student lifecycle institute scope), and Stage 3.3 (student submenu institute scope).
- Added portal lookup contract parity by extending shared `LookupItem` with optional `InstitutionType` to support institute-aware lifecycle department filtering in web compilation paths.
- Backend/API/service/repository updates in Stage 3.4: none beyond the web contract compile-alignment fix above.
- Frontend/menu/filter updates in Stage 3.4: none (Stage 3.3 wording/filter work carried forward as-is).
- Authorization/policy updates in Stage 3.4: none (validation closeout stage).
- DB/schema/script updates: none.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj -v minimal` -> passed (`115/115`).
- Role/Institute checks:
  - verified Admin/SuperAdmin/Faculty/Student role-scope suites remain green under full integration run,
  - verified institute mismatch denial remains enforced on lifecycle and student-submenu scope endpoints,
  - verified cross-institution department/course parity path from Stage 3.1 remains covered via integration suite.
- Regression checks: no failures across complete integration suite and no new compile blockers after Stage 3.4 contract alignment.
- Residual risks: analytics/report institute-filter breadth and report reliability items are intentionally deferred to Phase 4 stages.

### Stage 4.1 - Analytics Filter Expansion (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates:
  - expanded analytics API endpoints and exports to accept optional `institutionType` filter in addition to existing department filters,
  - added role-aware analytics scope resolution in `AnalyticsController` so constrained roles auto-inherit their JWT `institutionType` claim and explicit mismatch requests are denied,
  - added department-to-institution compatibility enforcement for analytics requests to prevent cross-institute filter bypasses,
  - extended `IAnalyticsService` and `AnalyticsService` to apply institution-type filtering in performance, attendance, assignment, and quiz analytics queries.
- Frontend/menu/filter updates:
  - added Analytics page filter controls for institution and department,
  - applied role-aware default filter behavior in portal analytics action so non-SuperAdmin sessions auto-scope to claim institution and out-of-scope department selections are cleared safely,
  - wired analytics API client calls to send `departmentId` and `institutionType` query filters.
- Authorization/policy updates:
  - analytics institute mismatch requests now return `403` when a constrained role attempts to query outside claim scope.
- DB/schema/script updates: none.
- Repository/test updates:
  - added `AnalyticsInstituteParityIntegrationTests` for mismatch deny and claim-default scoping behavior on analytics assignment endpoint.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AnalyticsInstituteParityIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests|FullyQualifiedName~StudentSubmenuParityIntegrationTests|FullyQualifiedName~StudentLifecycleIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests" -v minimal` -> passed (`41/41`).
- Role/Institute checks:
  - verified analytics institute mismatch query for Admin claim is denied (`403`),
  - verified analytics endpoint defaults to claim-compatible institute scope when no explicit analytics filters are supplied,
  - verified report/sidebar/student-lifecycle/student-submenu parity suites remained green with analytics filter expansion in place.
- Regression checks: no failures in selected Stage 2/3/4 parity guard suites.
- Residual risks: report-definition/report-export institute filter breadth is handled in Stage 4.2; broken report reliability items remain staged for Stage 4.3.

### Stage 4.2 - Reports Filter Expansion (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates:
  - added optional `institutionType` query support across report generation endpoints and export endpoints in `ReportController`,
  - added role-aware report-scope resolver to auto-default constrained roles to JWT claim institution and deny explicit mismatch institution filters,
  - extended report DTO contracts and repository query signatures to carry institution filter through attendance/result/assignment/quiz/GPA/enrollment/semester-results/low-attendance/FYP report paths,
  - extended background queued result-summary export request payload to preserve institution filter scope.
- Frontend/menu/filter updates:
  - expanded report-center pages with institution filter controls on report forms,
  - updated portal report actions and export links to persist and forward `institutionType` in report navigation,
  - updated web API client report query builders and method signatures to include institution filter propagation.
- Authorization/policy updates:
  - constrained-role report calls now enforce mismatch-deny behavior when explicit `institutionType` conflicts with caller claim,
  - constrained-role report calls now auto-scope to claim institution when explicit institution filter is omitted.
- DB/schema/script updates: none.
- Repository/test updates:
  - added report integration coverage for institute-scoped report filtering and explicit mismatch-deny checks.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~AnalyticsInstituteParityIntegrationTests|FullyQualifiedName~ReportExportsIntegrationTests|FullyQualifiedName~SidebarMenuIntegrationTests|FullyQualifiedName~StudentSubmenuParityIntegrationTests|FullyQualifiedName~StudentLifecycleIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests" -v minimal` -> passed (`43/43`).
- Role/Institute checks:
  - verified report enrollment endpoint respects explicit institution filters for super-admin scoped report reads,
  - verified admin report requests with explicit mismatched institution filter return `403`,
  - verified report export/report parity suites remained green under expanded report filter contract.
- Regression checks: no failures in selected Stage 2/3/4 parity guard suites.
- Residual risks: broken-report reliability fixes remain staged for Stage 4.3.

### Stage 4.3 - Broken Report Fixes (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates:
  - repaired report authorization scope for faculty access on department-scoped reports (`gpa-report`, `enrollment-summary`, `semester-results`, `low-attendance`, `fyp-status`),
  - added faculty department-assignment validation for report endpoints that previously allowed over-broad faculty reads,
  - updated faculty offering-scope report checks to use department-assignment scope instead of strict `FacultyUserId` ownership to prevent false forbids on valid assigned-department offerings.
- Frontend/menu/filter updates:
  - no report UI contract change required; existing report filters now align correctly with corrected backend faculty scope enforcement.
- Authorization/policy updates:
  - faculty requests without required department or offering filters now return `400` on department-scoped report routes,
  - faculty requests using unassigned department filters now return `403` consistently across repaired report endpoints.
- DB/schema/script updates: none.
- Repository/test updates:
  - added deterministic Stage 4.3 report integration tests for faculty report-scope reliability and mismatch-deny coverage.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~ReportExportsIntegrationTests|FullyQualifiedName~ReportCatalogIntegrationTests|FullyQualifiedName~AnalyticsInstituteParityIntegrationTests|FullyQualifiedName~StudentSubmenuParityIntegrationTests|FullyQualifiedName~StudentLifecycleIntegrationTests|FullyQualifiedName~AdminUserManagementIntegrationTests" -v minimal` -> passed (`42/42`).
- Role/Institute checks:
  - verified faculty `gpa-report` without department is rejected (`400`),
  - verified faculty unassigned department filters are denied (`403`) for enrollment, semester-results, and FYP status report endpoints,
  - verified faculty low-attendance report requires department or offering filter (`400`).
- Regression checks: no failures in selected Stage 2/3/4 parity guard suites.
- Residual risks: none within Stage 4.3 scope; Phase 4 exit criteria closure remains Stage 4.4.

### Stage 4.4 - Exit Criteria (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates:
  - no new code changes required; Stage 4.4 serves as the phase-exit gate over the already repaired analytics/report surfaces.
- Frontend/menu/filter updates:
  - no new UI changes required; the existing analytics/report filters remain aligned to the final scope.
- Authorization/policy updates:
  - no new policy changes required; role and institute guards were already validated in Stages 4.1-4.3.
- DB/schema/script updates: none.
- Repository/test updates:
  - completed the full integration-suite exit gate to confirm report and analytics parity remains stable after Stage 4.3.

Validation Summary
- Automated tests: `dotnet build Tabsan.EduSphere.sln -v minimal` -> passed.
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj -v minimal` -> passed (`124/124`).
- Role/Institute checks:
  - verified no regressions across the broader integration suite covering report, analytics, role, and institute guard paths,
  - confirmed phase-exit stability for School/College/University parity flows.
- Regression checks: full integration suite passed with no failures.
- Residual risks: none within Phase 4 exit scope.

### Stage 5.1 - Core Seed Coverage (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates: none.
- Frontend/menu/filter updates: none.
- Authorization/policy updates:
  - aligned core DB seed role-access matrix with explicit SuperAdmin allowance rows on baseline sidebar menus,
  - aligned seeded report-role assignments with current report policy matrix including Student access for transcript report only.
- DB/schema/script updates:
  - updated `Scripts/02-Seed-Core.sql` to seed institution policy flags (`institution_include_school|college|university`) with idempotent upsert behavior,
  - added deterministic institute-aware baseline departments for School, College, and University with explicit `InstitutionType` values,
  - normalized legacy report keys (`academic-transcript`, `attendance-summary`, `result-sheet`) to current underscore keys and seeded full parity report definition set,
  - preserved idempotent behavior for rerunnable core seed execution.
- Repository/test updates: none.

Validation Summary
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~UserImportAndForceChangeIntegrationTests" -v minimal` -> passed (`3/3`).
- Role/Institute checks:
  - verified core seed now contains School/College/University policy flags,
  - verified seeded report-role matrix includes SuperAdmin/Admin/Faculty for operational reports and Student for transcript only,
  - verified baseline seed includes one deterministic core department per institution type.
- Regression checks: no code-layer regressions introduced (script-only stage).
- Residual risks: full cross-entity institute dummy coverage remains in Stage 5.2.

### Stage 5.2 - Full Dummy Coverage (All Tables) (Completed: 2026-05-13)

Implementation Summary
- Backend/API/service/repository updates: none.
- Frontend/menu/filter updates: none.
- Authorization/policy updates:
  - aligned dummy users with explicit `InstitutionType` assignment (School/College/University representative coverage),
  - added deterministic admin/faculty department-assignment rows used by role/institute scope checks in parity scenarios.
- DB/schema/script updates:
  - expanded `Scripts/03-FullDummyData.sql` to include parity coverage for buildings, rooms, timetables, timetable entries, payment receipts, transcript export logs, lifecycle artifacts (bulk promotion, graduation approval path, school stream assignment), and student report cards,
  - added deterministic institute-aware updates for seeded departments (`InstitutionType`) and user institution assignments to keep replay output parity-safe,
  - kept script idempotency via stable GUID keys and `NOT EXISTS` insertion guards.
- Repository/test updates: none.

Validation Summary
- Automated tests: `dotnet test tests/Tabsan.EduSphere.IntegrationTests/Tabsan.EduSphere.IntegrationTests.csproj --filter "FullyQualifiedName~UserImportAndForceChangeIntegrationTests" -v minimal` -> passed (`3/3`).
- Role/Institute checks:
  - verified full dummy script now seeds representative parity entities for School/College/University across users/departments/programs/courses/offerings/enrollments/results/quizzes/payments/lifecycle/report artifacts,
  - verified deterministic institution assignment values are explicitly persisted for parity demo users and departments.
- Regression checks: no application-code regressions introduced (script-only stage).
- Residual risks: replay safety count assertions and post-deployment aggregate checks are completed in Stage 5.3.
