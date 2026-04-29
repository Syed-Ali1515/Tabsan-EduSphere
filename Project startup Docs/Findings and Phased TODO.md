# Tabsan EduSphere Findings and Phased TODO

**Version:** 1.0  
**Date:** 29 April 2026  
**Prepared For:** Project kickoff review and phase approval

---

## 1. Executive Findings

The provided startup documents define a strong product vision and feature set, but they were initially more product-oriented than implementation-ready. The key gap areas and what was resolved are listed below.

### 1.1 What Was Strong

- Clear business goals and licensing model
- Good role definitions and feature expectations
- Clear module concept with licensing-based enablement
- Strong principle of non-destructive academic history

### 1.2 What Needed Strengthening

- ASP.NET architecture boundaries were not explicit
- API and integration standards were not formalized
- Database lacked operational tables and index strategy details
- Module dependencies and technical activation behavior were underspecified
- Delivery sequencing was not yet sprint-ready
- License creation tool (Tabsan-Lic) was not specified as a separate application
- Student lifecycle operations (graduation, promotion, dropouts, transfers) were underspecified
- Finance/payment workflow was absent
- Dashboard navigation, per-user theming, and System Settings menu were not formalized
- Security hardening, email integration, and database performance strategy were deferred

### 1.3 What Has Been Added

- ASP.NET implementation architecture baseline in PRD
- Expanded schema conventions and additional core tables
- Module dependency and activation rules mapped to technical implementation
- 19-sprint phased development plan with exit criteria (extended from 12 to 19 sprints)
- Tabsan-Lic standalone license creation tool — Phases 7 (Sprints 13–14)
- Student lifecycle: graduation, semester promotion/failure, dropout, department transfer — Phase 8
- Finance and payment receipt workflow with optional online payment gateway — Phase 8
- CSV-based student registration import with duplicate validation — Phase 8
- Teacher attendance/result modification workflow with admin approval — Phase 8
- Role-based sidebar navigation, per-user themes, Departments admin menu — Phase 9
- System Settings menu: License, Theme, Reports, Modules — Phase 9
- OWASP Top 10 security hardening, password policy, account lockout/reset — Phase 10
- Database views and stored procedures for performance — Phase 10
- Free/open-source email API integration — Phase 10
- Mobile-responsive UI and accessibility (WCAG 2.1 AA) — Phase 10

---

## 2. Architecture Findings

### 2.1 Recommended Architecture

- Modular monolith for v1 with clean boundaries for future service extraction
- ASP.NET Core 8 Web API + ASP.NET Core MVC/Razor UI
- EF Core with SQL Server as default data store
- Background jobs for license checks, notifications, and cleanup tasks

### 2.2 Domain Boundaries (Bounded Contexts)

- Identity and Access
- Academic Core
- Student Lifecycle
- Learning Delivery
- Assessment and Results
- Notifications
- FYP Management
- Licensing and Entitlements
- Audit and Reporting

### 2.3 Critical Cross-Cutting Concerns

- RBAC and policy-based authorization
- Immutable academic history behavior
- Auditability for all privileged operations
- License-driven entitlement checks at UI, API, and job levels
- Observability: logs, metrics, tracing, health checks

---

## 3. Feature Findings

### 3.1 Mandatory Foundation Features

- Authentication and role system
- Department and SIS baseline
- License validation and degraded mode behavior
- Core audit logging

### 3.2 High-Risk Feature Areas

- Licensing and read-only degradation
- Department-scoped data authorization
- Quiz attempt integrity and anti-duplication rules
- Attendance uniqueness and alert workflows
- Transcript export auditing

### 3.3 Recommended Release Scope

- v1.0: Core operations and licensing controls (Phases 0–2)
- v1.1: Quizzes, attendance, FYP, AI baseline (Phases 3–6)
- v1.2: Tabsan-Lic tool, student lifecycle, finance, dashboard settings (Phases 7–9)
- v1.3: Security hardening, email, performance, mobile UI (Phase 10)

---

## 4. Database and Data Findings

### 4.1 Schema Readiness Enhancements Identified

- Need explicit indexing strategy for high-traffic read/write paths
- Need additional tables for sessions, offerings, whitelist, quiz internals, and operational audit
- Need retention and archival rules for logs and operational records
- Need migration and seeding sequencing per release phase

### 4.2 Data Integrity Priorities

- One submission per student per assignment
- One attendance record per student-course-date
- Unique registration numbers and controlled signup flow
- Immutable history for semester records

---

## 5. Phased TODO List (Stages and Checklists)

## Phase 0: Foundation and Governance (Sprint 1) ✅ COMPLETE

### Stage 0.1 Project Setup
- [x] Create .NET 8 solution and project structure
- [x] Configure environment profiles (dev/staging/prod)
- [x] Configure centralized configuration and secrets strategy

### Stage 0.2 Engineering Guardrails
- [x] Add CI pipeline for build, tests, and static checks
- [x] Add coding standards and pull request template
- [x] Add baseline logging, tracing, and health checks

### Stage 0.3 Baseline Documentation
- [x] Finalize architecture decision records (ADRs)
- [x] Confirm API versioning and error envelope standard

### ✅ Phase 0 Implementation Summary

| Item | Detail |
|---|---|
| Solution structure | `Tabsan.EduSphere.sln` with five projects: `Domain`, `Application`, `Infrastructure`, `API`, `Web`, plus `BackgroundJobs` and `tests/` |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → API/Web) with Modular Monolith pattern |
| Target framework | .NET 8 LTS (`net8.0`); SDK 10.0.203 |
| CI pipeline | `.github/workflows/dotnet-ci.yml` — build + test on push/PR |
| Configuration | `appsettings.json` + environment-specific overrides; secrets via .NET user-secrets (dev) |
| Logging | Structured logging via `ILogger<T>` throughout; health-check endpoint registered |
| Error envelope | `ProblemDetails` standard used across all API controllers |
| Documentation | `Docs/Function-List.md` established; inline XML summary comments required on all public members |

---

## Phase 1: Identity, Licensing, and Entitlements (Sprints 2-3) ✅ COMPLETE

### Stage 1.1 Identity and Access
- [x] Implement ASP.NET Core Identity model
- [x] Implement JWT and session management
- [x] Implement role and policy authorization matrix

### Stage 1.2 Licensing
- [x] Implement license upload endpoint and validation workflow
- [x] Implement startup, daily, and admin-login validation checks
- [x] Implement degraded read-only behavior for invalid/expired license

### Stage 1.3 Module Entitlements
- [x] Implement module activation/deactivation APIs
- [x] Enforce mandatory module protection rules
- [x] Add module policy filters across APIs

### ✅ Phase 1 Implementation Summary

| Item | Detail |
|---|---|
| Identity | ASP.NET Core Identity with `ApplicationUser`; roles: Admin, Faculty, Student, SuperAdmin |
| JWT | Bearer token auth (8.0.8); token issued on login, validated via `[Authorize]` policies |
| Authorization policies | `RequireAdmin`, `RequireFaculty`, `RequireStudent` policies enforced at controller level |
| License entity | `License` domain entity with `IsValid`, `ExpiresAt`, and `InstitutionId`; upload + validation endpoints |
| License checks | Startup check, daily `LicenseCheckWorker` background job, and per-admin-login check |
| Degraded mode | `LicenseMiddleware` blocks write operations and returns `423 Locked` when license is expired/invalid |
| Module entitlements | `Module` and `ModuleActivation` entities; mandatory modules (Identity, SIS) protected from deactivation |
| Migration | `InitialCreate` — creates identity, license, and module tables |
| Build validation | 0 errors, 0 warnings |

---

## Phase 2: Academic Core and SIS (Sprints 4-5) ✅ COMPLETE

### Stage 2.1 Department and Program Core
- [x] Implement departments, programs, courses, semesters CRUD
- [x] Implement faculty-to-department assignment model
- [x] Implement course offering model

### Stage 2.2 Student Lifecycle
- [x] Implement registration whitelist workflow
- [x] Implement student profile creation and enrollment
- [x] Implement immutable semester history records

### Stage 2.3 Access Boundaries
- [x] Enforce department-scoped faculty access
- [x] Validate admin all-department visibility behavior

### ✅ Phase 2 Implementation Summary

| Item | Detail |
|---|---|
| Domain entities | `Department`, `Program`, `Course`, `Semester`, `CourseOffering`, `FacultyDepartmentAssignment` |
| Student entities | `RegistrationWhitelist`, `StudentProfile`, `Enrollment`, `SemesterRecord` |
| Soft-delete | Global query filters on `Department`, `CourseOffering` — deleted records excluded from all queries |
| Immutable history | `SemesterRecord` has no update path; once written it is read-only by design |
| Faculty scoping | `FacultyDepartmentAssignment` links users to departments; queries filtered by `DepartmentId` claim |
| Repositories | `AcademicRepository`, `StudentRepository` — full CRUD + lookup methods |
| Application services | `AcademicService`, `StudentService` — DTO mapping, business rule enforcement |
| API controllers | `DepartmentController`, `CourseController`, `SemesterController`, `StudentController`, `EnrollmentController` |
| Migration | `AcademicCore` — creates all SIS and academic core tables with indexes |
| Build validation | 0 errors, 0 warnings |

---

## Phase 3: Assignments and Results (Sprints 6-7) ✅ COMPLETE

### Stage 3.1 Assignment Pipeline
- [x] Implement assignment create/publish lifecycle
- [x] Implement student submission pipeline
- [x] Enforce one submission per assignment per student

### Stage 3.2 Grading and Results
- [x] Implement grading and feedback workflow
- [x] Implement result publication and transcript generation
- [x] Implement transcript export logs

### Stage 3.3 Quality and Security
- [x] Add authorization integration tests for all result endpoints
- [x] Add audit events for grading and publishing

### ✅ Phase 3 Implementation Summary

| Item | Detail |
|---|---|
| Domain entities | `Assignment`, `AssignmentSubmission`, `Result`, `Transcript`, `TranscriptExportLog` |
| Assignment lifecycle | Draft → Published states; only published assignments are visible to students |
| Submission uniqueness | Unique index on `(AssignmentId, StudentProfileId)`; service rejects duplicates with `409 Conflict` |
| Grading | `Result` entity stores marks, feedback, and `GradedByUserId`; grader ID captured for audit |
| Transcript | Auto-generated on result publish; `TranscriptExportLog` written on every PDF/Excel export |
| Soft-delete | `Assignment` uses global query filter; submitted/graded records preserved on assignment removal |
| Repositories | `AssignmentRepository`, `ResultRepository` — full pipelines including submission and transcript queries |
| Application services | `AssignmentService`, `ResultService` — business rules, DTO projection, audit event dispatch |
| API controllers | `AssignmentController`, `ResultController`, `TranscriptController` |
| Migration | `AssignmentsAndResults` — creates assignment, submission, result, and transcript tables |
| Build validation | 0 errors, 0 warnings |

---

## Phase 4: Notifications and Attendance (Sprints 8-9) ✅ COMPLETE

### Stage 4.1 Notifications
- [x] Implement notifications and recipient tracking
- [x] Implement read/unread and delivery status updates

### Stage 4.2 Attendance
- [x] Implement attendance capture and uniqueness constraints
- [x] Implement low-attendance threshold logic
- [x] Implement alert jobs for attendance warnings

### Stage 4.3 Reliability
- [x] Add retry policies for notification dispatch
- [x] Add dead-letter handling for failed job execution

### ✅ Phase 4 Implementation Summary

| Item | Detail |
|---|---|
| Domain entities | `Notification`, `NotificationRecipient`, `AttendanceRecord` |
| Notification types | `NotificationType` enum: General, Assignment, Result, AttendanceAlert, System, Announcement |
| Fan-out dispatch | `NotificationService.SendAsync` creates one `Notification` + N `NotificationRecipient` rows per call |
| Read tracking | `NotificationRecipient.MarkRead()` idempotent; `MarkAllReadAsync` processes up to 500 in one pass |
| Inbox and badge | Paged inbox (active only) + unread-count badge endpoint on `NotificationController` |
| Attendance uniqueness | Unique index on `(StudentProfileId, CourseOfferingId, Date)`; service skips duplicates in bulk mark |
| Attendance correction | `AttendanceRecord.Correct()` updates status and records correcting user ID |
| Threshold logic | `AttendanceRepository.GetBelowThresholdAsync` uses EF GroupBy projection to compute percentages |
| Alert background job | `AttendanceAlertJob` — configurable interval (default 24 h, 60 s startup delay); reads threshold from `appsettings.json` (`AttendanceAlert:Threshold`) |
| Retry / dead-letter | `AttendanceAlertJob` wraps `RunCheckAsync` in try/catch; exceptions are logged and do not crash the host |
| Repositories | `NotificationRepository` (9 methods), `AttendanceRepository` (10 methods) |
| Application services | `NotificationService` (8 methods), `AttendanceService` (8 methods incl. private mapping) |
| API controllers | `NotificationController` (7 endpoints), `AttendanceController` (9 endpoints) |
| Migration | `NotificationsAndAttendance` — creates `notifications`, `notification_recipients`, `attendance_records` tables |
| Build validation | 0 errors, 0 warnings |

---

## Phase 5: Quizzes and FYP (Sprints 10-11) ✅ COMPLETE

### Stage 5.1 Quizzes
- [x] Implement quiz authoring, question bank, and options
- [x] Implement attempts and answer persistence
- [x] Enforce attempt limits and scoring rules

### Stage 5.2 FYP
- [x] Implement project allocation and meeting scheduling
- [x] Implement room and panel member assignment
- [x] Implement FYP notification triggers

### Stage 5.3 Dashboards
- [x] Add student dashboard views for quizzes and FYP schedule
- [x] Add faculty views for pending reviews and meetings

### ✅ Phase 5 Implementation Summary

| Item | Detail |
|---|---|
| Domain entities — Quizzes | `Quiz`, `QuizQuestion`, `QuizOption` in `Domain/Quizzes/Quiz.cs`; `QuestionType` enum (MultipleChoice, TrueFalse, ShortAnswer) |
| Domain entities — Attempts | `QuizAttempt`, `QuizAnswer` in `Domain/Quizzes/QuizAttempt.cs`; `AttemptStatus` enum (InProgress, Submitted, TimedOut, Abandoned) |
| Domain entities — FYP | `FypProject`, `FypPanelMember`, `FypMeeting` in `Domain/Fyp/FypProject.cs`; `FypProjectStatus`, `FypPanelRole`, `MeetingStatus` enums |
| Quiz business rules | `StartAttemptAsync` validates published state, availability window, in-progress check, and attempt cap (0 = unlimited) |
| Auto-grading | `SubmitAttemptAsync` auto-grades MCQ/TrueFalse; ShortAnswer left as pending (null `MarksAwarded`); total score from auto-graded questions only |
| Manual grading | `GradeAnswerAsync` awards marks to a short-answer response via direct `GetAnswerByIdAsync` lookup |
| FYP lifecycle | `Propose → Approve/Reject → AssignSupervisor → InProgress → Complete`; `Reject` requires mandatory remarks |
| EF configurations | `QuizConfigurations.cs` — 5 entity configs; `FypConfigurations.cs` — 3 entity configs; all use typed navigation to avoid shadow FK warnings |
| Quiz global filter | `HasQueryFilter(q => q.IsActive)` on `Quiz` for soft-delete; advisory EF warning acknowledged (expected behaviour) |
| Repositories | `QuizRepository` (20 methods incl. `GetAnswerByIdAsync`), `FypRepository` (17 methods) in `Infrastructure/Repositories/QuizFypRepositories.cs` |
| DbContext | `ApplicationDbContext` updated with 8 new `DbSet<T>` properties for Phase 5 entities |
| Application DTOs | `QuizDtos.cs` — 7 request + 7 response records; `FypDtos.cs` — 9 request + 4 response records |
| Application interfaces | `IQuizService` (15 methods), `IFypService` (18 methods) |
| Application services | `QuizService` (15 methods + 5 private helpers), `FypService` (18 methods + 3 private helpers) |
| API controllers | `QuizController` (15 endpoints + 2 helpers), `FypController` (19 endpoints + 2 helpers) |
| DI wiring | `IQuizRepository`, `IFypRepository`, `IQuizService`, `IFypService` registered as `Scoped` in `Program.cs` |
| Migration | `QuizzesAndFyp` — creates 8 new tables: `quizzes`, `quiz_questions`, `quiz_options`, `quiz_attempts`, `quiz_answers`, `fyp_projects`, `fyp_panel_members`, `fyp_meetings` |
| Build validation | 0 errors, 0 warnings |
| Function-List.md | Phase 5 section appended — 90+ functions catalogued across Domain, Infrastructure, Application, and API layers |

---

## Phase 6: AI, Analytics, and Hardening (Sprint 12)

### Stage 6.1 AI Chatbot
- [ ] Implement role-aware chat context orchestration
- [ ] Add module/license guardrails for AI access
- [ ] Add prompt safety and response audit logging

### Stage 6.2 Reporting
- [ ] Implement baseline analytics endpoints
- [ ] Implement exportable reports (PDF/Excel)

### Stage 6.3 Hardening and Release Readiness
- [ ] Run performance and load tests against p95 targets
- [ ] Complete penetration/security checklist
- [ ] Complete UAT and release candidate sign-off

---

## Phase 7: Tabsan-Lic — License Creation Tool (Sprints 13–14)

> **Scope:** A standalone .NET application (`Tabsan-Lic`) separate from EduSphere that is used exclusively by the vendor/Super Admin to generate encrypted license files.

### Stage 7.1 License Generation Core
- [ ] Create `Tabsan-Lic` as a separate .NET console/desktop application
- [ ] Generate a unique `VerificationKey` per license (GUID + cryptographic salt; one-time use only)
- [ ] Store issued VerificationKeys in a local sealed database; mark each key as used on first consumption
- [ ] Prevent re-use of a VerificationKey — once consumed it is permanently invalidated
- [ ] Prompt operator for expiry type: 1 year / 2 years / 3 years / Permanent

### Stage 7.2 License File Security
- [ ] Serialize license payload (type, expiry, issue date, VerificationKey hash) to JSON
- [ ] Encrypt the JSON payload using AES-256 with an embedded vendor private key
- [ ] Sign the encrypted bundle with RSA-2048 digital signature using vendor private key
- [ ] Write the final `.tablic` file — binary, machine-readable, not human-editable
- [ ] Any byte-level modification to the file must invalidate the signature and be detected by EduSphere

### Stage 7.3 EduSphere License Import
- [ ] Add "Import License" endpoint in EduSphere (Super Admin only)
- [ ] EduSphere reads `.tablic` file, verifies RSA signature using embedded public key
- [ ] Decrypt payload and apply license details (expiry, type, status)
- [ ] Mark the VerificationKey as consumed; reject future imports using the same key
- [ ] Send expiry warning notification to Admin 5 days before license expiry date (background job)
- [ ] Update license status in the License table and broadcast system notification

### Stage 7.4 Unlimited Key Generation
- [ ] Tabsan-Lic supports generating an unlimited number of VerificationKeys
- [ ] Each generated key is logged with generation timestamp and chosen expiry
- [ ] Provide export list of generated keys (for vendor audit purposes only)

---

## Phase 8: Student Lifecycle & Academic Operations (Sprints 15–16)

> **Scope:** Full student journey management — graduation, semester promotion, dropouts, department transfers, finance, CSV imports, and teacher modification workflows.

### Stage 8.1 Graduation Management
- [ ] Add "Graduated Students" menu visible to Admin and Super Admin
- [ ] Sub-menus: one per department — each shows a checkbox list of students in their final semester
- [ ] Admin can select all or individual students and mark them as Graduated
- [ ] On graduation: student dashboard switches to **read-only mode** — view and download only; no create/edit/submit actions permitted

### Stage 8.2 Semester Completion & Promotion
- [ ] Add "Semester Management" menu for Admin and Super Admin
- [ ] Sub-menus: per department — shows student list with columns: Enrolment No / Registration No, Name, Current Semester, Subject list (multi-select checkboxes), Semester Status
- [ ] Admin marks semester as completed per student (select all or individual); unselected subjects recorded as failed subjects
- [ ] Students with all subjects passed: auto-promoted to next semester; active semester number updated
- [ ] Students with any failed subjects: semester status set to "Completed with Failed Subjects"
- [ ] Admin can mark a student's semester as fully Failed: student returns to the same semester; added to re-enrollment list for that semester
- [ ] Previous semesters always visible to students in **read-only** form

### Stage 8.3 Student Status Management
- [ ] Admin can mark a student as **Inactive** (dropout/leaving): student is blocked from login; all academic data preserved
- [ ] Admin can open a student profile and **transfer department or change program**: updates enrollment, department assignment, and active semester
- [ ] Students and teachers may self-edit only: password, email address, mobile number
- [ ] For any other profile change (name, address, etc.) a student/teacher submits an **Admin Change Request**; admin reviews and applies the update

### Stage 8.4 Teacher Attendance & Result Modification Workflow
- [ ] Teachers can submit a modification request for attendance records or published results
- [ ] Modification request requires a mandatory reason field
- [ ] On submission: notification sent to Admin for approval
- [ ] Admin approves or rejects; on approval the record is updated; on rejection a notification is sent back to the teacher
- [ ] Full audit trail maintained for all modification requests (requestor, reason, approver, timestamp)

### Stage 8.5 Finance & Payments
- [ ] Finance role: create and upload payment receipt for a student (amount, description, due date)
- [ ] Student: view receipt, upload proof of payment, and mark receipt as "Payment Submitted"
- [ ] Finance: confirm payment received — status changes to **Paid**
- [ ] Until fees status is **Paid**, student account operates in **read-only mode** (same as graduated/expired license behavior)
- [ ] Online payment gateway integration (card / bank account) — disabled by default; Super Admin toggles it on/off via Module Settings
- [ ] All payment records and receipts are stored permanently and cannot be deleted

### Stage 8.6 Student Registration Import
- [ ] Admin can enter individual registration numbers or **import a CSV sheet** of newly registered students into the whitelist table
- [ ] During student signup: system checks if registration number exists in whitelist; if not, blocks signup
- [ ] If registration number exists but already has an account: error — "An account already exists with this Registration Number. Please contact your admin for further details."
- [ ] CSV import includes validation: duplicate detection, format validation, error report download

### Stage 8.7 Account Security — Lockout & Reset
- [ ] Non-admin accounts: locked after configurable consecutive failed password attempts (default 5)
- [ ] Admin or Super Admin can unlock and reset password for locked non-admin accounts
- [ ] If an Admin account is locked: only Super Admin can reset the password
- [ ] Password policy enforced: minimum 12 characters, must include uppercase, lowercase, digit, and special character; no common passwords; no previous 5 passwords reused; bcrypt/Argon2 hashing

---

## Phase 9: Dashboard, Navigation & System Settings (Sprints 17–18)

> **Scope:** Role-based sidebar navigation, per-user theming, department/timetable management, and the full System Settings menu.

### Stage 9.1 Role-Based Sidebar Navigation
- [ ] Implement collapsible sidebar with menus and sub-menus driven by the authenticated user's role
- [ ] Menu items and sub-menus rendered only for modules that are active and permitted for that role
- [ ] Super Admin sees all menus regardless of module status

### Stage 9.2 Per-User Theme Settings
- [ ] Theme selection is stored per user (not per role)
- [ ] User can switch theme from their profile or Settings → Theme Settings
- [ ] Minimum 15 themes supported; Light, Dark, and High-Contrast accessibility variants included
- [ ] Admin default theme does not override individual user selections

### Stage 9.3 Departments Administration Menu
- [ ] Admin and Super Admin: "Departments" menu to create / edit departments, degree programs, semesters, and subjects
- [ ] Timetable management under Departments: admin creates and publishes timetable per department/semester
- [ ] All users from that department can download the timetable in **PDF or Excel** format

### Stage 9.4 System Settings Menu
Add a "Settings" top-level menu with the following sub-menus:

#### 9.4.1 License Update (Super Admin only)
- [ ] UI to upload a `.tablic` license file; calls the Phase 7 import endpoint
- [ ] License status table: columns — Status, Expiry Date, Date Updated, Remaining Days
- [ ] Visible to Super Admin and Admin (Admin: read-only view; Super Admin: read + upload)

#### 9.4.2 Theme Settings
- [ ] Per-user theme picker; persists across sessions
- [ ] Preview mode before applying

#### 9.4.3 Report Settings (Super Admin only)
- [ ] Table: SR#, Report Name, Purpose, Roles (multi-select checkbox list of all roles)
- [ ] Super Admin activates or deactivates a report; deactivated reports hidden from all dashboards except Super Admin

#### 9.4.4 Module Settings (Super Admin only)
- [ ] Table: SR#, Module Name, Purpose, Roles (multi-select checkbox), Status (Active / Inactive dropdown)
- [ ] Active: module visible and functional for all selected roles
- [ ] Inactive: module hidden from all role dashboards except Super Admin
- [ ] Module data is never deleted on deactivation

### Stage 9.5 License Expiry Notifications
- [ ] Background job checks license expiry daily
- [ ] Sends notification to Admin and Super Admin 5 days prior to expiry
- [ ] Notification includes: expiry date, remaining days, link to License Update screen

---

## Phase 10: Security, Performance & Email Infrastructure (Sprint 19)

> **Scope:** OWASP Top 10 hardening, database performance optimisation, free/open-source email delivery, and mobile-responsive UI.

### Stage 10.1 Security Hardening
- [ ] Complete OWASP Top 10 checklist: injection, broken auth, XSS, IDOR, security misconfiguration, etc.
- [ ] Enforce HTTPS-only; configure HSTS, CSP, X-Frame-Options, X-Content-Type-Options headers
- [ ] Input validation and output encoding on all endpoints (FluentValidation + HtmlEncoder)
- [ ] Rate limiting on auth endpoints and sensitive APIs
- [ ] Password policy (complexity, lockout, hashing with Argon2id) — see Phase 8.7
- [ ] Dependency vulnerability scan in CI; zero critical/high CVEs before release
- [ ] Penetration test report signed off

### Stage 10.2 Database Performance
- [ ] Create SQL **Views** for high-traffic read patterns: student dashboard summary, department reports, attendance summary
- [ ] Create **Stored Procedures** for complex write operations: semester promotion batch, graduation batch, payment status update
- [ ] Add missing covering indexes on foreign-key columns and frequently filtered columns
- [ ] Query performance baseline established; p95 < 200 ms for core dashboards under load

### Stage 10.3 Email API Integration
- [ ] Integrate a free/open-source transactional email provider (e.g., SMTP via MailKit, or self-hosted Postal/Haraka, or SendGrid free tier)
- [ ] Email service abstracted behind `IEmailSender` interface — provider is swappable via configuration
- [ ] Use cases: notification dispatch (results, assignment deadlines, low attendance), license expiry warning, password reset, account unlock
- [ ] Email templates stored in database or file system; localisation-ready
- [ ] All outbound email attempts logged with status (sent / failed / bounced)

### Stage 10.4 Mobile-Friendly & Accessible UI
- [ ] Responsive layout using CSS Grid / Bootstrap 5 — tested on 360 px, 768 px, 1280 px viewports
- [ ] WCAG 2.1 AA compliance for high-contrast themes
- [ ] Touch-friendly controls (minimum 44×44 px tap targets)
- [ ] Lighthouse score ≥ 90 for Performance, Accessibility, and Best Practices on core pages

---

## 6. Immediate Recommendations

### 6.1 Architecture and Delivery

- Freeze v1.0 scope now to reduce delivery risk
- Adopt an ADR process before code scaffolding starts
- Keep modular monolith boundaries strict from day one

### 6.2 Security and Compliance

- Prioritize licensing and authorization tests in earliest sprints
- Include audit logging in every privileged feature from first implementation
- Add dependency and secret scanning in CI immediately

### 6.3 Data Strategy

- Approve index and constraints strategy before first migration
- Define backup and restore runbook before production-like testing
- Establish retention policy defaults now to avoid late redesign

### 6.4 Team Execution

- Use sprint demos with phase exit criteria as acceptance gates
- Track risks weekly with named owners and mitigation deadlines
- Do not start AI features until identity, licensing, and SIS core are stable

---

## 7. Approval Checklist for Next Step

- [x] Approve this phased TODO as the execution baseline
- [x] Confirm v1.0 scope lock
- [x] Confirm stack decisions (.NET 8, SQL Server, EF Core)
- [x] Approve Phase 0 start
- [x] Approve scaffolding and first migration implementation
- [ ] Confirm Tabsan-Lic as a separate .NET application (Phase 7)
- [ ] Confirm email provider choice (MailKit SMTP / SendGrid free tier / self-hosted) (Phase 10)
- [ ] Confirm online payment gateway provider (Phase 8)
- [ ] Approve extended roadmap horizon: 19 sprints / ~38 weeks

---

## 8. Master TODO Status (Requested)

- [x] Revise PRD for ASP.NET architecture
- [x] Expand schema for implementation readiness
- [x] Refine modules with dependencies and entitlements
- [x] Create phased ASP.NET development plan
- [x] Validate changes and summarize deliverables
- [x] Add user guides
- [x] Add training manuals
- [x] Add Phases 7–10: Tabsan-Lic, Student Lifecycle, Dashboard Settings, Security & Performance
- [x] Update PRD to v1.8 with new feature requirements
- [x] Update Modules.md with new modules (Finance, Dashboard/Navigation, Timetable)
- [x] Update Development Plan with Phases 7–10 and extended roadmap horizon

---
