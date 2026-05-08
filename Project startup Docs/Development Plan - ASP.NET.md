# Tabsan EduSphere Development Plan (ASP.NET)

**Version:** 1.4  
**Date:** 8 May 2026  
**Based On:** PRD v1.33, Modules v1.3, Database Schema v1.2

---

## Execution Updates

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
- Continue Phase 1 remaining tasks: Stage 1.1 completion validation, Stage 1.2 CRUD surfaces, Stage 1.6 themes/branding expansion.

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
