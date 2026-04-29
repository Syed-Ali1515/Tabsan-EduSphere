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

### 1.3 What Has Been Added

- ASP.NET implementation architecture baseline in PRD
- Expanded schema conventions and additional core tables
- Module dependency and activation rules mapped to technical implementation
- 12-sprint phased development plan with exit criteria

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

- v1.0: Core operations and licensing controls
- v1.1: Quizzes, attendance, FYP, AI baseline
- v1.2: Analytics, advanced audit, expansion capabilities

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

## Phase 5: Quizzes and FYP (Sprints 10-11)

### Stage 5.1 Quizzes
- [ ] Implement quiz authoring, question bank, and options
- [ ] Implement attempts and answer persistence
- [ ] Enforce attempt limits and scoring rules

### Stage 5.2 FYP
- [ ] Implement project allocation and meeting scheduling
- [ ] Implement room and panel member assignment
- [ ] Implement FYP notification triggers

### Stage 5.3 Dashboards
- [ ] Add student dashboard views for quizzes and FYP schedule
- [ ] Add faculty views for pending reviews and meetings

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

---

## 8. Master TODO Status (Requested)

- [x] Revise PRD for ASP.NET architecture
- [x] Expand schema for implementation readiness
- [x] Refine modules with dependencies and entitlements
- [x] Create phased ASP.NET development plan
- [x] Validate changes and summarize deliverables
- [x] Add user guides
- [x] Add training manuals

---
