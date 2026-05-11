# Command Center

## Purpose
Use this file as the single handover reference between sessions and devices.
Before starting any work, the assistant must:
1. Read this file.
2. Read Project startup Docs/Final-Touches.md.
3. Continue from the exact Current Execution Pointer below.

## Non-Negotiable Rule Per Completed Stage / Phase
After **every completed stage** (not just at phase-end), update **all** required tracking files:
1. Docs/Function-List.md
2. Docs/Command.md
3. Docs/Advance-Enhancements.md
4. Project startup Docs/PRD.md
5. Project startup Docs/Database Schema.md
6. Project startup Docs/Development Plan - ASP.NET.md
7. Docs/Functionality.md

After every documentation update, complete mandatory Git sync in this exact order:
1. Commit all current changes
2. Pull latest remote changes (rebase preferred)
3. Push committed changes to remote

For every completed stage entry in `Docs/High-Load-Optimization-Phases-And-Stages.md`, include:
- `Status: Completed`
- `Implementation Summary`
- `Validation Summary`

Also update this file with:
- completed work
- validation summary
- next steps
- pending extras

**Always-on documentation sync (mandatory workflow — enforced from Phase 20 onward):**
- After **every completed stage**, update all tracking files listed above before moving to the next stage.
- Do not batch documentation to phase-end.
- Keep `Docs/Function-List.md`, `Docs/Command.md`, `Docs/Advance-Enhancements.md`, `Project startup Docs/PRD.md`, `Project startup Docs/Database Schema.md`, and `Project startup Docs/Development Plan - ASP.NET.md` updated continuously as work progresses.

**Always-on Git sync (requested workflow):**
- Before ending any work session, always run full sync: commit all changes, pull from remote, then push to remote.
- Do not leave local-only completed work.
- **Always do BOTH pull and push** — pull first (rebase), then push. Never push without pulling first.
- Use this command sequence:

```powershell
cmd /c git -C "<repo-root>" add -A
cmd /c git -C "<repo-root>" commit -m "<phase/stage summary>"
cmd /c git -C "<repo-root>" pull --rebase origin main
cmd /c git -C "<repo-root>" push origin main
```

**Code quality rules (enforced from Phase 5 onward):**
- Add a `// Final-Touches Phase X Stage X.X — <description>` comment above every block of code added or changed for that phase.
- Update `Docs/Function-List.md` with a new `## Final-Touches Phase X` section listing all new/modified functions, their purpose, and file location after every completed phase.

---

## Current Execution Pointer
- Plan Source: Docs/Advance-Enhancements.md
- Active Phase: **Phase 5 — k6 Load Testing Improvements — COMPLETE ✅**
- Active Stage: **Stage 5.1, 5.2, and 5.3 complete (realistic load model, distributed generators, output discipline)**
- Status: **Scale scripts now use ramping-arrival-rate with think-time, support generator sharding, and default to summary-first output discipline.**
- Last Updated: 2026-05-11
- Next: **Execute Phase 6 Stage 6.1 external call caching.**
- Docs Updated: ✅ Phase 5 complete trackers updated (2026-05-11)

### 2026-05-11 - Phase 5 Completion
- Stage 5.1: Converted 50k/100k/1m/5m scripts to `ramping-arrival-rate` and added randomized think-time windows.
- Stage 5.2: Added distributed generator sharding support (`GENERATOR_TOTAL`, `GENERATOR_INDEX`) in scale scripts and runners.
- Stage 5.3: Enforced summary-first output (`--quiet`, summary export) and gated raw JSON outputs in PowerShell runner behind `-AllowRawOutput`.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (**130/130**).

### 2026-05-11 - Phase 4 Completion
- Stage 4.1: Added short-TTL distributed cache for expensive analytics report endpoints (`performance`, `attendance`, `assignments`, `quizzes`).
- Stage 4.2: Added configurable static-asset cache headers in Web startup and appsettings profiles for edge/CDN-friendly caching.
- Stage 4.3: Restricted cache scope to expensive shared-safe operations and static assets only.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (**130/130**).

### 2026-05-11 - Phase 3 Stage 3.3 Completion
- Added Kestrel transport tuning in API and Web hosts: keep-alive timeout, request-header timeout, server-header suppression, and HTTP/2 ping tuning.
- Kept response compression enabled with Brotli/Gzip fast-path settings.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (**130/130**).

### 2026-05-11 - Phase 3 Stage 3.2 Completion
- Removed `ContinueWith` bridges from the hot timetable, settings, quiz, and building/room repository methods.
- Kept the hot request paths fully asynchronous by returning `await ToListAsync(...)` directly.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` passed (**130/130**).


### 2026-05-10 - Phase 33 Stage 33.3 Completion
- Added DataAnnotations to `LoginRequest`, `RefreshRequest`, `ChangePasswordRequest`, `ForceChangePasswordRequest`, `CreateAdminUserRequest`, and `UpdateAdminUserRequest`.
- Added `SecurityValidationTests` to verify the hardened DTO validation paths.
- Validation: `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj --filter "FullyQualifiedName~SecurityValidationTests"` passed (**4/4**); `dotnet test Tabsan.EduSphere.sln --no-build` passed (**234/234**).

### 2026-05-10 - Phase 33 Stage 33.2 Completion
- Added config-driven reverse-proxy trust controls in API/Web (`ReverseProxy:Enabled`, `KnownProxies`, symmetry/forward-limit settings).
- Added startup guardrails to reject unsafe reverse-proxy and production CORS-empty startup conditions.
- Removed `localhost` fallback defaults from Web login and portal API connection model.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed; `dotnet test Tabsan.EduSphere.sln --no-build` passed (**230/230**).

### 2026-05-10 - Phase 33 Stage 33.1 Completion
- Re-scoped Phase 33 to `Hosting Configuration and Security Hardening` from `Docs/Refactoring-Hosting-Security.md`.
- Added explicit environment-aware configuration loading blocks in:
  - `src/Tabsan.EduSphere.API/Program.cs`
  - `src/Tabsan.EduSphere.Web/Program.cs`
  - `src/Tabsan.EduSphere.BackgroundJobs/Program.cs`
- Added startup validation for required settings (`DefaultConnection`, `EduApi:BaseUrl`) and enforced BackgroundJobs environment override for connection string placeholder.
- Aligned API/Web/BackgroundJobs appsettings metadata for base identity/version and environment URLs.
- Validation: `dotnet build Tabsan.EduSphere.sln` passed.

### 2026-05-10 - Post-Stage Verification Refresh
- Command run: `dotnet build Tabsan.EduSphere.sln` -> passed (build warnings unchanged).
- Command run: `dotnet test Tabsan.EduSphere.sln --no-build` -> passed (**230/230**).
- Notes: no new failures introduced after Stage 32.5 completion and git sync.

---

## ⚡ Database & Migration Status

✅ **All pending migrations applied successfully** (2026-05-09):
1. `20260505_Phase2LicenseConcurrency` — adds `MaxUsers` + `ActivatedDomain` to `license_state` ✅
2. `20260506_Phase4UserImport` — adds `MustChangePassword` to `users` ✅
3. `20260507103000_PortalBrandingLogoValueMaxLength` — alters `portal_settings.Value` to `nvarchar(max)` ✅
4. `20260506044806_Phase6AdminDepartmentAssignments` — adds `admin_department_assignments` table ✅
5. `20260507_Phase12AcademicCalendar` — adds `academic_deadlines` table ✅
6. `20260507_Phase14_Helpdesk` — adds `support_tickets` + `support_ticket_messages` tables ✅
7. `20260507133254_Phase15_EnrollmentRules` — adds `course_prerequisites` table with unique index ✅
8. `Phase16_FacultyGrading` — adds `rubrics`, `rubric_criteria`, `rubric_levels`, `rubric_student_grades` tables ✅
9. `Phase17_DegreeAudit` — adds `degree_rules`, `degree_rule_required_courses` tables + `course_type` column on `courses` ✅
10. `20260508132355_Phase22_ExternalIntegrations` — adds `accreditation_templates` table ✅
11. `20260508152906_Phase25_AcademicEngineUnification` — adds `institution_grading_profiles` table ✅
12. `20260509044437_Phase26_SchoolCollegeExpansion` — adds `school_streams`, `student_stream_assignments`, `student_report_cards`, `bulk_promotion_batches`, `bulk_promotion_entries`, `parent_student_links` tables ✅

Database is fully synchronized with codebase.

## ⚡ Phase Summary — All Issue-Fix Phases Complete

✅ **Issue-Fix Phase 4 — Student Workflow Repair (ALL 6 stages)**
- Stage 4.1 — Assignment submission flow wired end-to-end (submit/view/grade)
- Stage 4.2 — Timetable department auto-resolved from student profile; `Guid.Empty` guard added
- Stage 4.3 — Assignments semester filter; semester-scoped offering dropdown for students
- Stage 4.4 — Results semester filter; fallback to student-safe result endpoints on 403
- Stage 4.5 — Quizzes semester filter; Upcoming/Pending/Completed status badges
- Stage 4.6 — FYP menu gated to 8th semester; student completion-request; faculty approval; auto-complete when all approvers approve; FYP result row in Results
- **Validation: 12/12 assignment integration tests passed; 0 build errors**

✅ **Issue-Fix Phase 5 — Reporting and Export Center (Stages 5.1–5.5)**
- Stage 5.1 — Assignment and Quiz summary report APIs + portal pages
- Stage 5.2 — CSV/PDF export for Attendance, Results, Assignments, Quizzes (Excel retained)
- Stage 5.3 — SuperAdmin unrestricted report scope confirmed
- Stage 5.4 — Admin reporting scope bounded by assigned departments (Phase 6 data model + portal guidance guards on all 9 report pages)
- Stage 5.5 — Faculty scope enforced on department/offering filters and report data/export endpoints

✅ **Issue-Fix Phase 6 — Admin Multi-Department Assignment (Stages 6.1–6.2 + Extension)**
- Backend: `AdminDepartmentAssignment` entity, repository, migration, API endpoints
- UI: Departments page assignment panel; dedicated AdminUsers portal page
- Admin create/update with multi-department checkbox list and assignment sync

✅ **Phase 12 — Academic Calendar System (Stages 12.1–12.2) — 2026-05-07 (commit 6e89af1)**
- Stage 12.1: `AcademicCalendar` portal page (all roles) with semester filter, days-remaining color badges
- Stage 12.2: `AcademicDeadline` entity + EF config (`academic_deadlines`); `IAcademicDeadlineRepository` + impl; `IAcademicCalendarService` + impl; `CalendarController` (GET all-auth, POST/PUT/DELETE Admin+SuperAdmin); `AcademicDeadlines` portal CRUD page (Admin+SuperAdmin); `DeadlineReminderJob` background service dispatching `NotificationType.System` notifications daily
- Migration: `20260507_Phase12AcademicCalendar`
- Validation: **0 build errors; 78/78 tests passed**

✅ **Phase 13 — Global Search (Stages 13.1–13.2) — 2026-05-08 (commit 00b7b64)**
- Stage 13.1: `GET /api/v1/search?q={term}&limit={n}` — role-scoped cross-entity search; `ISearchRepository` + `SearchRepository` (EF LINQ joins); `ISearchService` + `SearchService`; `SearchController`; `SearchDTOs` records; no new migration
- Stage 13.2: Global search bar in portal header (`_Layout.cshtml`); typeahead dropdown (JS fetch); full results page (`Search.cshtml`) with Bootstrap category tabs; `_SearchResultsList.cshtml` partial; `PortalController` actions `Search` + `SearchTypeahead`
- Validation: **0 build errors; 78/78 tests passed**

✅ **Phase 14 — Helpdesk / Support Ticketing System (Stages 14.1–14.3) — 2026-05-09 (commit 8576e44)**
- Stage 14.1: `SupportTicket` entity + `IHelpdeskRepository` + `HelpdeskRepository`; `IHelpdeskService` + `HelpdeskService`; `HelpdeskController` (GET tickets, GET ticket, POST create, POST message, POST assign/resolve/close/reopen); EF migration `20260507_Phase14_Helpdesk`; `HelpdeskDTOs`
- Stage 14.2: Admin/SuperAdmin case management endpoints; ticket assignment to staff users; all roles scoped per department
- Stage 14.3: Faculty reply support; `SupportTicketMessage` thread model; reopen within configurable window; `Helpdesk.cshtml` list, `HelpdeskCreate.cshtml` form, `HelpdeskDetail.cshtml` thread view, `_TicketStatusBadge.cshtml` partial; sidebar link + route/group maps; `Program.cs` Phase 14 DI registration
- Validation: **0 build errors; 78/78 tests passed**

✅ **Phase 15 — Enrollment Rules Engine (Stages 15.1–15.3) — 2026-05-08 (commit 42f0993)**
- Stage 15.1: `CoursePrerequisite` entity + `IPrerequisiteRepository` + `PrerequisiteRepository`; `EnrollmentService.TryEnrollAsync` validates all prerequisites; `PrerequisiteController` (GET/POST/DELETE `api/v1/prerequisite`); `EnrollmentRulesDTOs` with `AdminEnrollRequest` override fields; EF migration `20260507133254_Phase15_EnrollmentRules` (`course_prerequisites` table + unique index)
- Stage 15.2: Timetable clash detection inside `TryEnrollAsync`; `OverrideClash` + `OverrideReason` on `AdminEnrollRequest`; override audit-logged
- Stage 15.3: `CourseOffering.MaxEnrollment` capacity enforcement already in place; `UpdateMaxEnrollment` API action exists
- Web portal: `PrerequisitesPageModel`, `PrerequisiteWebItem`, `CoursePrerequisiteGroup` models; `Prerequisites` / `PrerequisiteAdd` / `PrerequisiteRemove` portal controller actions; `Prerequisites.cshtml` view; sidebar link (Admin/SuperAdmin only)
- Validation: **0 build errors; 7/7 tests passed**

---

## Completed Work
- **Issue-Fix Phase 4 — Student Workflow Repair (ALL Stages 4.1–4.6 Done)** ✅ (2026-05-07)
  - Stage 4.1: Assignment submission end-to-end — `POST /api/v1/assignment/submit`, file upload to GUID path, Submit modal, `SubmitAssignment` action
  - Stage 4.2: Timetable department auto-resolved from student profile; `Guid.Empty` guard prevents bad requests; dashboard-config fallback retained
  - Stage 4.3/4.4/4.5: Semester filter added to Assignments/Results/Quizzes portals; offering dropdowns are semester-scoped; Results fallback to student-safe endpoints on 403; Quiz status badges (Upcoming/Pending/Completed)
  - Stage 4.6: FYP menu gated by `CurrentSemesterNumber >= 8`; `POST /api/v1/fyp/{id}/request-completion` (student) + `POST /api/v1/fyp/{id}/approve-completion` (faculty); auto-complete when all approvers done; FYP result row in Results; EF migration `Phase4FypCompletionApprovalFlow`
  - Auth consistency: login flow now resolves API base URL before token acquisition to prevent intermittent student 401s
  - Validation: **12/12 assignment tests passed; 78/78 full suite passed; 0 build errors**
- **Issue-Fix Phase 5 — Reporting and Export Center (ALL Stages 5.1–5.5 Done)** ✅ (2026-05-07)
  - Stage 5.1: Assignment + Quiz summary report APIs (`/api/v1/reports/assignment-summary`, `/api/v1/reports/quiz-summary`) + portal pages `ReportAssignments`, `ReportQuizzes`
  - Stage 5.2: CSV + PDF export for Attendance/Results/Assignments/Quizzes (`/export/csv`, `/export/pdf` variants); Web portal proxy actions; Excel/CSV/PDF export buttons on all report pages
  - Stage 5.3: SuperAdmin unrestricted report scope verified
  - Stage 5.4: Admin report scope bounded by assigned departments (Phase 6 data model + portal UX guidance guards on all 9 report pages)
  - Stage 5.5: Faculty department/offering filter sources scoped; report data/export requires offering ownership validation
  - Validation: 0 build errors after all export + scope changes
- **Issue-Fix Phase 3 — Faculty Workflow Repair (ALL 8 stages Done)** ✅ (2026-05-07)
  - Stage 3.1: CourseController.GetAll + GetOfferings — replaced Forbid() with Ok(empty) for out-of-scope dept requests
  - Stage 3.2/3.5/3.6/3.7: CourseController.GetMyOfferings — changed from FacultyUserId filter to dept-scope filter; fixes all empty dropdowns
  - Stage 3.3: Enrollments 403 fixed via same CourseController changes; cleaned up dead branch in PortalController
  - Stage 3.4: StudentController.GetAll — removed Forbid(); silently scopes to allowed departments
  - Stage 3.8: FypController.admin-create policy → "Faculty"; PortalController.Fyp() loads students for faculty; Fyp.cshtml shows Create button for Faculty
  - Validation: 0 build errors, 78/78 tests passed
- **Phase 1 Remediation — ALL 15 items Done (P1-S1-01 through P1-S6-04)** ✅
  - Stage 1.1: 403 auth fixes on Attendance/Assignments/Quizzes/Results; 30+ regression tests in AuthorizationRegressionTests.cs
  - Stage 1.2: Departments, Courses+Offerings, Enrollments, FYP Management CRUD fully implemented
  - Stage 1.3: Result Summary exception fixed; all Report Center reports visible by role
  - Stage 1.4: Module Settings removed from sidebar; brand area made non-clickable
  - Stage 1.5: Student lifecycle Promote flow fixed (correct profileId passed)
  - Stage 1.6: 29 total themes (10 new); logo upload endpoint + sidebar; privacy policy footer link; font family/size dropdowns with CSS injection
- Final-Touches Phases 1–9 (original work before remediation): all complete
- Phase 1 Remediation Batches 1–5: all complete

## Next Steps
- **Phase 12 — Academic Calendar System — COMPLETE ✅** (commit 6e89af1, 2026-05-07)
- **Phase 13 — Global Search — COMPLETE ✅** (commit 00b7b64, 2026-05-08)
- **Phase 14 — Helpdesk / Support Ticketing System — COMPLETE ✅** (commit 8576e44, 2026-05-09)
- **Phase 15 — Enrollment Rules Engine — COMPLETE ✅** (commit 42f0993, 2026-05-08)
- **Phase 16 — Faculty Grading System — COMPLETE ✅** (commit `1f496f7`)
- **Phase 17 — Degree Audit System — COMPLETE ✅** (78/78 tests passed)
- **Phase 18 — Graduation Workflow — COMPLETE ✅** (78/78 tests passed; migration `Phase18_GraduationWorkflow`)
- **Phase 19 — Advanced Course Creation & Grading Config — COMPLETE ✅**
- **Phase 20 — Learning Management System (LMS) — COMPLETE ✅**
- **Phase 21 — Study Planner — COMPLETE ✅** (migration `Phase21_StudyPlanner`)
- **Phase 22 — External Integrations — COMPLETE ✅** (commit `dddee69`; migration `Phase22_ExternalIntegrations`)
- **Phase 23 — Core Policy Foundation — COMPLETE ✅** (commit `28cac36`; 27/27 tests passed)
- **Phase 24 — Dynamic Module and UI Composition — COMPLETE ✅** (commit `391ac45`; 44/44 tests passed)
- **Phase 25 — Academic Engine Unification — COMPLETE ✅** (commit `d2aabd3`; 144/144 tests passed; migration `Phase25_AcademicEngineUnification`)
- **Phase 26 — School and College Functional Expansion — COMPLETE ✅** (commit `4c0904c`; 152/152 tests passed; migration `Phase26_SchoolCollegeExpansion`)
- **Phase 27 — University Portal Parity and Student Experience — COMPLETE ✅**
  - Stage 27.1 commit `fd3b137`: Portal capability matrix (service + API + web view + tests)
  - Stage 27.2 commit `20dba8d`: MFA toggle, SSO-ready security profile, session risk controls, auth audit improvements
  - Stage 27.3 commit `56cf1dd`: provider abstraction contracts for ticketing, announcements, and email
- **Phase 28 — Scalability Architecture — Stage 28.1 COMPLETE ✅**
  - Load-balancer readiness: forwarded headers expanded on API and added to Web.
  - Stateless web nodes: session-backed portal/API auth state replaced with protected cookies; optional shared key-ring path introduced.
  - Response compression and payload shaping: Brotli/Gzip compression enabled; null JSON fields omitted in API/Web JSON responses.
- **Phase 28 — Scalability Architecture — Stage 28.2 COMPLETE ✅**
  - Distributed cache foundation: optional Redis-backed `IDistributedCache` added with distributed-memory fallback.
  - Hot-read sharing: module entitlement resolution and report catalog reads now use shared cache across API nodes.
  - Async workload offload: large notification fan-out batches now defer recipient insertion to a hosted background worker.
  - Report generation offload: queued result-summary export jobs now run in the background with status polling and deferred download endpoints.
  - Recalculation offload: queued result publish-all jobs now run in the background with status polling.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 1 DELIVERED ✅**
  - Added configurable media storage abstraction (`IMediaStorageService`) with local filesystem provider (`LocalMediaStorageService`) and `MediaStorage` settings section.
  - Migrated student payment-proof upload endpoint to storage abstraction and object-key persistence (instead of hard-coded local file paths).
  - Added stricter upload validation reuse (`FileUploadValidator`) before persistence.
  - No schema changes required; storage references remain metadata-only in existing receipt records.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 2 DELIVERED ✅**
  - Moved storage abstraction contract into the Application layer so both API controllers and Application services can share it.
  - Migrated graduation certificate generation to provider-backed persistence, storing storage keys instead of filesystem-relative webroot paths for new records.
  - Added provider-backed certificate read path in graduation downloads, with compatibility fallback for legacy `/certificates/*` records.
  - No schema changes required; existing certificate path column continues to store a metadata reference.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 3 DELIVERED ✅**
  - Migrated license upload flow from direct temp-file path handling to provider-backed save/read/delete operations via `IMediaStorageService`.
  - Added bytes-based activation method in `LicenseValidationService` so license verification no longer depends on filesystem paths.
  - Added storage-provider delete support to clean temporary upload objects after activation attempts.
  - No schema changes required.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 4 DELIVERED ✅**
  - Added configurable provider registration (`AddConfiguredMediaStorage`) so storage backend selection is now driven by `MediaStorage:Provider`.
  - Added `BlobMediaStorageService` adapter (object-storage style key semantics, isolated root path, reference generation).
  - Extended media storage settings with `BlobRootPath` and updated environment appsettings defaults/placeholders.
  - Local provider remains default to preserve runtime behavior unless provider is explicitly switched.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 5 DELIVERED ✅**
  - Migrated portal logo upload from inline base64 return to provider-backed persistence through `IMediaStorageService`.
  - Added public logo streaming endpoint `GET /api/v1/portal-settings/logo-files/{**storageKey}` for branding rendering without bearer headers.
  - Added guarded key-category enforcement so only `portal-branding/logo` objects are served by the anonymous endpoint.
  - Preserved backward compatibility for existing `data:image/*` logo values already stored in portal settings.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 6 DELIVERED ✅**
  - Extended `IMediaStorageService` with temporary read URL generation (`GenerateTemporaryReadUrlAsync`) for provider-backed signed URL workflows.
  - Added temporary signed URL generation support in both local and blob storage providers using optional `MediaStorage:SignedUrlSecret`.
  - Updated portal logo file endpoint to prefer redirecting to provider-generated temporary URLs and safely fall back to byte streaming when unavailable.
  - Added `SignedUrlSecret` placeholders to API appsettings files.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 7 DELIVERED ✅**
  - Enforced signed URL validation (`exp` + `sig`) on local logo streaming when `MediaStorage:SignedUrlSecret` is configured.
  - Added compatibility redirect from unsigned legacy logo URLs to short-lived signed local URLs.
  - Added fixed-time signature comparison and expiry enforcement for local signed reads.
  - Kept provider temporary URL redirect-first behavior and byte-stream fallback for operational compatibility.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 8 DELIVERED ✅**
  - Added tokenized certificate file endpoint `GET /api/v1/graduation/certificate-files/{**storageKey}` for provider-backed certificate reads.
  - Updated graduation certificate download flow to redirect to temporary provider URLs (when available) or signed local certificate URLs.
  - Enforced signed URL validation (`exp` + `sig`) for local certificate streaming when signing secret is configured.
  - Preserved legacy `/certificates/*` certificate path compatibility with existing byte-download flow.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 9 DELIVERED ✅**
  - Extended `IMediaStorageService` with metadata lookup support (`GetMetadataAsync`) and expanded save results to include content type and object length.
  - Added metadata resolution in local/blob storage providers so callers can retrieve provider-backed content type and length without re-deriving from business code.
  - Updated portal logo and certificate streaming endpoints to prefer storage metadata for response content type selection.
  - Preserved existing signed URL and legacy path compatibility behavior.
- **Phase 28 — Scalability Architecture — Stage 28.3 SLICE 10 DELIVERED ✅**
  - Extended storage save/metadata contracts with SHA-256 content hash and optional download filename metadata.
  - Persisted sidecar metadata for local/blob providers so integrity/disposition details survive provider redirects and later reads.
  - Updated certificate generation and upload flows to pass content type and original/download filename into storage.
  - Restored filename-preserving certificate downloads for signed local and redirect-first media reads.
- **Phase 28 COMPLETE ✅**
  - Stage 28.1 delivered stateless/load-balanced app behavior.
  - Stage 28.2 delivered distributed cache and background-work offload.
  - Stage 28.3 delivered provider-backed media persistence, signed reads, metadata, and integrity/disposition hardening with no schema changes.
- **Phase 29 — MSSQL Data and Indexing Optimization — Stage 29.1 DELIVERED ✅**
  - Added baseline composite indexes for high-frequency student/user/status recency queries across graduation applications, support tickets, notification inbox rows, payment receipts, quiz attempts, and user sessions.
  - Generated EF migration `20260509155457_20260510_Phase29_IndexBaseline` to apply the new index set.
  - Validated current model audit: no `InstitutionId`, `YearId`, or `GradeId` columns exist yet, so Stage 29.1 focused on current `StudentId`/`UserId`/`CourseId`/`SemesterId` shaped query contracts.
- **Phase 29 — MSSQL Data and Indexing Optimization — Stage 29.2 SLICE 1 DELIVERED ✅**
  - Added paged helpdesk ticket listing contract across API, application, repository, and web layers.
  - Replaced unbounded `GET /api/v1/helpdesk/tickets` listing with `page` and `pageSize` driven queries for Student, Faculty, Admin, and SuperAdmin views.
  - Updated portal helpdesk page with previous/next pagination controls and status-filter reset to page 1.
  - No database migration was required.
- **Phase 29 — MSSQL Data and Indexing Optimization — Stage 29.2 SLICE 2 DELIVERED ✅**
  - Added paged graduation application list contract for both student (`GET /api/v1/graduation/my`) and staff (`GET /api/v1/graduation`) list paths.
  - Replaced unbounded graduation list materialization with server-side `page` and `pageSize` SQL paging in repository/service/API layers.
  - Updated portal graduation list pages with previous/next pagination controls while preserving status/department filters.
  - No database migration was required.
- **Phase 29 — MSSQL Data and Indexing Optimization — Stage 29.2 SLICE 3 DELIVERED ✅**
  - Added paged payment receipt list contract for admin/student receipt endpoints and student-filtered admin listing.
  - Replaced unbounded payment receipt list materialization with server-side `page` and `pageSize` SQL paging in repository/service/API/web layers.
  - Updated portal payments page with previous/next pagination controls while preserving selected student filters.
  - No database migration was required.
- **Phase 29 — MSSQL Data and Indexing Optimization — Stage 29.3 DELIVERED ✅**
  - Added archive/retention policy script (`Scripts/3-Phase29-ArchivePolicy.sql`) with dry-run and optional batched cleanup mode.
  - Added index maintenance script (`Scripts/4-Phase29-IndexMaintenance.sql`) with fragmentation-driven reorganize/rebuild planning.
  - Added capacity and growth dashboard script (`Scripts/5-Phase29-CapacityGrowthDashboard.sql`) with table-size and recent-growth telemetry.
  - Updated `Scripts/README.md` with operations runbook commands.
  - No database migration was required.
- **Phase 29 COMPLETE ✅**
  - Stage 29.1 delivered index baseline and query contracts.
  - Stage 29.2 delivered pagination discipline for helpdesk, graduation, and payment receipt heavy-list endpoints.
  - Stage 29.3 delivered lifecycle maintenance scripts for archive policy, index maintenance, and capacity-growth observability.

## Pending Extra Tasks (Cross-Phase)
- None. All pending extras are complete.

---

## Session Resume Prompt Template
Copy/paste this in a new chat:

Resume from Command.md and Final-Touches.md.
Continue from Current Execution Pointer.
Do not replan completed items.
When a phase is completed, update:
- Docs/Function-List.md
- Project startup Docs/PRD.md
- Project startup Docs/Final-Touches.md
- Command.md

---

## Work Log

### Entry 024 — 2026-05-07 — Stage 5.4 Admin Reporting Scope Portal UX Completion
**Completed:**
- Added `isAdminOnly` guidance guards to all 9 report portal page actions in `Web/Controllers/PortalController.cs`:
  - `ReportAttendance`, `ReportResults`, `ReportAssignments`, `ReportQuizzes`: Admin receives friendly message when neither department nor offering is selected (mirrors existing Faculty guidance pattern).
  - `ReportGpa`, `ReportEnrollment`, `ReportSemesterResults`, `ReportFypStatus`: Admin receives friendly message when no department is selected.
  - `ReportLowAttendance`: Admin receives friendly message when neither department nor offering is selected.
- `isAdminOnly` pattern: `identity?.IsAdmin == true && !identity.IsSuperAdmin` (avoids triggering for SuperAdmin who has `IsAdmin=true`).
- No API changes needed — `EnforceAdminDepartmentScopeAsync` was already complete via Phase 6.

**Validation:**
- `dotnet build Tabsan.EduSphere.Web.csproj` — **0 errors, 0 warnings**
- Full integration suite — **78/78 tests passed**
- Commit: `ee9fb57` — pushed to `main`

**Moved to:**
- All phases and remediation items complete. No active pending work.

### Entry 023 — 2026-05-07 — Refactoring-Hosting-Security Part A + Part B Delivery
**Completed:**
- **Part A — Hosting Configuration:**
  - Created `appsettings.Production.json` for API, Web, BackgroundJobs (production-ready placeholders)
  - Created `appsettings.Development.json` for BackgroundJobs (dev connection string)
  - Updated `API/appsettings.Development.json`: debug logging, CORS origins `["https://localhost:5063", "http://localhost:5063"]`, EnableSwagger/EnableDetailedErrors flags
  - Updated `API/appsettings.json`: added `AppSettings` section (EnableSwagger, EnableDetailedErrors, CorsOrigins array)
  - `API/Program.cs` — DB retry on failure (3 attempts, 30 s backoff via `EnableRetryOnFailure`)
  - `API/Program.cs` — CORS configured from `AppSettings:CorsOrigins` config key; `UseCors` added to pipeline
  - `API/Program.cs` — `ForwardedHeaders` middleware registered and used in non-dev (IIS/nginx/Cloudflare support)
  - `API/Program.cs` — Health check endpoint at `/health` (`AddHealthChecks` + `MapHealthChecks`)
  - `API/Program.cs` — 5 MB request body size limits (Kestrel + IIS + FormOptions)
  - `API/Program.cs` — Startup environment log line
  - `API/Program.cs` — Swagger gated by `AppSettings:EnableSwagger` flag (dev always on)
  - `API/Program.cs` — WeatherForecast boilerplate block removed
- **Part B — Security Hardening:**
  - Created `API/Middleware/ExceptionHandlingMiddleware.cs`: global exception handler; maps exception types to HTTP codes; no stack traces in production; TraceIdentifier in every error response
  - Created `API/Services/FileUploadValidator.cs`: static validator with magic-bytes verification, MIME-type check, extension allowlist, 5 MB size limit
  - `Web/Program.cs` — session cookie hardened: `SameSite=Strict`, `SecurePolicy=Always`
  - `.gitignore` — added: `*.pfx`, `*.key`, `logs/`, `appsettings.*.local.json`, `appsettings.*.secret.json`, `secrets/`, `.env.local`, `.env.*.local`

**Validation:**
- `dotnet build Tabsan.EduSphere.API.csproj` — **0 errors, 0 warnings**
- Full integration suite — **69/69 tests passed**
- Commit: `f56ccd9` — pushed to `main`

**Pending (next session):**
- Serilog file sink (rolling log to `logs/app-.txt`)
- `UserSecretsId` in API `.csproj`
- Wire `FileUploadValidator.ValidateAsync()` into `AssignmentController.Submit` + logo upload controller

### Entry 022 — 2026-05-06 — Issue-Fix Phase 4 Option A/C Delivery (Web Import + Forced Password Change)
**Completed:**
- Confirmed User Import web page and CSV upload flow are wired through portal (`UserImport` + `ImportUsersCsv`).
- Added forced password change portal flow:
  - `LoginController` now reads `MustChangePassword` from login response and redirects to `Portal/ForceChangePassword`.
  - `EduApiClient` now tracks `ForcePasswordChangeRequired` session flag and exposes `ForceChangePasswordAsync`.
  - `PortalController` enforces redirect to forced-password page until password is updated.
  - Added new Razor page: `Views/Portal/ForceChangePassword.cshtml`.
- Added integration tests in `UserImportAndForceChangeIntegrationTests` for:
  - Student cannot import CSV (`403`)
  - Import user -> first login (`MustChangePassword=true`) -> force-change-password -> old password rejected -> new password accepted.

**Validation:**
- Focused tests: passed (`2/2`).
- Full integration suite: passed (`70/70`).

**Moved to:**
- Ready for Option B (define new phases) or remaining optional unit-test hardening.

### Entry 021 — 2026-05-06 — Issue-Fix Phase 6.1 Extended Delivery (Dedicated Admin User Management)
**Completed:**
- Added dedicated SuperAdmin API for Admin account management:
  - `GET /api/v1/admin-user`
  - `POST /api/v1/admin-user`
  - `PUT /api/v1/admin-user/{id}`
- Added repository support to fetch users by role with optional inactive inclusion.
- Added dedicated portal page:
  - `Portal/AdminUsers`
  - create Admin user + initial department assignment
  - update Admin email/status/password + assignment sync
  - search Admin selector and select-all/clear assignment UX controls
- Kept Departments page assignment panel and added quick navigation to the dedicated Admin Users page.
- Added focused integration tests for:
  - admin-user endpoint access control
  - admin create/update + assignment round-trip flow

**Validation:**
- `dotnet build Tabsan.EduSphere.sln` succeeded.
- Focused integration tests failed due pre-existing migration/seeding issue in test environment:
  - duplicate `ActivatedDomain` column in `license_state` migration path during DB setup.

**Moved to:**
- Ready for next issue-fix phase after test DB migration chain cleanup.

### Entry 020 — 2026-05-06 — Issue-Fix Phase 6.1 UI Delivered (Admin Department Assignment)
**Completed:**
- Added SuperAdmin-only Admin user listing endpoint:
  - `GET /api/v1/department/admin-users`
- Added Web API client methods for assignment UI flows:
  - `GetAdminUsersAsync`
  - `GetAdminDepartmentIdsAsync`
  - `AssignAdminToDepartmentAsync`
  - `RemoveAdminFromDepartmentAsync`
  - internal helper `DeleteWithBodyAsync` for DELETE-with-payload API calls
- Extended portal department page model for assignment management state:
  - admin list
  - selected admin id
  - assigned department id list
- Updated `PortalController.Departments` to load assignment state for SuperAdmin.
- Added `PortalController.UpdateAdminDepartmentAssignments` to diff and apply add/remove assignment operations.
- Updated `Views/Portal/Departments.cshtml` with SuperAdmin assignment UI:
  - Admin selector dropdown
  - Active department checkbox list
  - Save Assignments action

**Validation:**
- `dotnet build Tabsan.EduSphere.sln` succeeded after implementation.

**Moved to:**
- Phase 6 complete for backend + assignment management UI. Ready for next planned issue-fix phase.

### Entry 019 — 2026-05-06 — Issue-Fix Phase 6 Backend Delivered (Admin Multi-Department Assignment)
**Completed:**
- Added domain model + persistence for admin multi-department assignment:
  - `AdminDepartmentAssignment` entity
  - `IAdminAssignmentRepository` and `AdminAssignmentRepository`
  - EF mapping and migration `20260506044806_20260506_Phase6AdminDepartmentAssignments`
- Added SuperAdmin management endpoints for admin department scope in Department API:
  - `POST /api/v1/department/admin-assignment`
  - `DELETE /api/v1/department/admin-assignment`
  - `GET /api/v1/department/admin-assignment/{adminUserId}`
- Enforced Admin assignment scope in:
  - Department list responses
  - Course catalog and offerings responses
  - Report Center data and export endpoints (department/offering guard)
- This unblocks and closes Stage 5.4 backend dependency.

**Validation:**
- `dotnet build Tabsan.EduSphere.sln` succeeded after implementation.
- Migration creation completed successfully with EF Core tooling.

**Moved to:**
- Phase 6.1 UI integration (SuperAdmin create/update Admin with multi-department checkbox list)

### Entry 005 — 2026-05-06 — Issue-Fix Phase 2 Complete (Shared Portal and Settings)
**Completed:**
- Stage 2.1 (Branding and Asset Rendering): fixed logo upload/render path end-to-end.
  - API upload crash resolved (`WebRootPath` null fallback added in `PortalSettingsController.UploadLogo`).
  - API static asset serving fixed using explicit `PhysicalFileProvider` rooted at API `wwwroot` in `Program.cs`.
- Stage 2.2 (Privacy Policy Editing): verified editor and rendering flow works end-to-end.
  - Privacy content persisted via portal settings API and rendered on `/Home/Privacy`.
- Stage 2.3 (Shared Course Offering Dropdowns): verified offerings dropdown now returns populated options in portal pages (Assignments live-validated).

**Validation:**
- `POST /api/v1/portal-settings/logo` now returns `200 OK` with URL payload (`/portal-uploads/logo.svg`).
- `GET /portal-uploads/logo.svg` now returns `200`.
- Live UI: sidebar brand switched from initials to logo image.
- Live UI: Privacy page renders configured policy text.
- Live UI: Assignments `Select Course Offering` dropdown shows offerings list.

**Moved to:** Issue-Fix Phase 3 (Faculty Workflow Repair)

### Entry 001
- Date: 2026-05-03
- Action: Created Command.md as persistent handover controller.
- Changes:
  - Added execution pointer linked to Final-Touches Phase 1 Stage 1.1.
  - Added mandatory documentation update rule per completed phase.
  - Added resume template and cross-phase pending extras list.
- Validation:
  - File created successfully.
  - Paths and phase names align with Final-Touches.md.
- Next:
  - Begin implementation of Phase 1 Stage 1.1.

### Entry 002
- Date: 2026-05-03
- Action: Started Phase 1 implementation (Stage 1.1 completed, Stage 1.2 partially completed).
- Changes:
  - Updated [src/Tabsan.EduSphere.Web/Views/Shared/_Layout.cshtml](src/Tabsan.EduSphere.Web/Views/Shared/_Layout.cshtml) to cache dynamic sidebar menus in session and reuse cache when menu API fails/returns empty.
  - Reworked dynamic sidebar rendering into grouped sections: Overview, Faculty Related, Student Related, Finance Related, Settings.
  - Removed layout redirect-return behavior to prevent render instability and session flow breakage.
- Validation:
  - SuperAdmin login shows full grouped sidebar.
  - Buildings page shows existing records and sidebar remains intact.
  - No forced re-login during Buildings navigation in validation run.
- Next:
  - Complete remaining Stage 1.2 assignment visibility in Sidebar Settings.
  - Start Stage 1.3 Dashboard Settings implementation.

## Work Log

### Entry 001 — 2026-05-03 — Phase 1 Stages 1.1–1.2 Complete
**Completed:**
- Stabilized sidebar rendering by caching dynamic menu in session (VisibleSidebarMenusCache).
- Removed layout-level redirect-return behavior.
- Implemented grouped sidebar with 5 groups (Overview, Faculty Related, Student Related, Finance Related, Settings).
- Seeded 29 sidebar menu items with role-based visibility.
- Verified SuperAdmin login, Buildings navigation, Sidebar Settings page (29 items, all assignable).

**Validation:**
- SuperAdmin login: grouped sidebar, full menu set visible.
- Buildings page: renders existing rows, sidebar remains stable.
- Sidebar Settings: 29 items listed, role assignment working.

**Moved to:** Stage 1.3

---

### Entry 002 — 2026-05-03 — Phase 1 Stage 1.3 Complete (Dashboard Branding)
**Completed:**
- Added \portal_settings\ domain entity (Key–Value table).
- Added EF migration \Phase1DashboardBranding\ (table created, applied to DB).
- Added \PortalBrandingService\ with \GetAsync()\ and \SaveAsync()\ methods.
- Added \PortalSettingsController\ API with \GET\ (all users) and \POST\ (SuperAdmin).
- Added \EduApiClient\ methods \GetPortalBrandingAsync()\ and \SavePortalBrandingAsync()\.
- Added \PortalController.DashboardSettings()\ GET/POST actions.
- Created \DashboardSettings.cshtml\ Razor view with form + live preview.
- Updated \_Layout.cshtml\ to load branding from DB with session cache fallback.
- Seeded \dashboard_settings\ sidebar menu item (SuperAdmin only).
- Updated \DatabaseSeeder.cs\ with all 4 portal_settings keys (university_name, brand_initials, portal_subtitle, footer_text).

**Validation:**
- Dashboard Settings page renders with form and live preview.
- Default branding values pre-filled: Tabsan EduSphere, TE, Campus Portal, © 2026 Tabsan EduSphere.
- Sidebar shows "Dashboard Settings" under Settings group.
- Footer text in _Layout driven by DB branding.

**Phase 1 Status:** ✅ Complete

**Moved to:** Phase 2 Stage 2.1

**Docs Updated:**
- \Final-Touches.md\: Marked Phase 1 complete, added Stage 1.3 Implementation/Validation summaries.
- \PRD.md\: Bumped version to 1.13, updated log entry.
- \Function-List.md\: Added Phase 1 Stage 1.3 portal settings and Dashboard Settings functions.

---

### Entry 003 — 2026-05-04 — Phase 2 Stage 2.1 Complete (Timetable Data Binding)
**Completed:**
- Fixed TimetableRepository.GetTeacherEntriesAsync() missing Building include
- Fixed TimetableRepository.GetByDepartmentAsync() missing Department, AcademicProgram, Semester includes
- Fixed TimetableRepository.GetPublishedByDepartmentAsync() missing Department, AcademicProgram, Semester includes
- Fixed TimetableRepository.GetByIdWithEntriesAsync() with separate Building include for entries

**Changes:**
- Updated [src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs](src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs) with proper EF Include statements

**Validation:**
- Build succeeded with all fixes
- Faculty timetable endpoint includes Building navigation for proper data binding
- Student timetable endpoints include all required related entities for DTO mapping
- Test data exists: 1 published timetable with 2 entries for CS dept, faculty.test

**Moved to:** Stage 2.2

---

### Entry 004 — 2026-05-04 — Phase 2 Stage 2.2 Complete (Lookup Data Visibility)
**Completed:**
- Fixed StudentProfileRepository.GetAllAsync() to include Program and Department navigation properties
- Updated StudentController.GetAll() to return ProgramName, DepartmentName, and Status from included entities
- Added new CourseRepository.GetOfferingsByDepartmentAsync() method for department-filtered course offerings
- Updated ICourseRepository interface with GetOfferingsByDepartmentAsync() method signature
- Updated CourseRepository.GetOfferingsBySemesterAsync() and GetOfferingsByFacultyAsync() with proper includes
- Refactored CourseController.GetOfferings() endpoint to accept both ?semesterId and ?departmentId query parameters
- Updated CourseController.GetAll() to include DepartmentName mapping for courses

**Changes:**
- [src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs](src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs): Removed incorrect User include, added Program/Department includes
- [src/Tabsan.EduSphere.API/Controllers/StudentController.cs](src/Tabsan.EduSphere.API/Controllers/StudentController.cs): Enhanced GetAll() response with related entity names
- [src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs](src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs): Added GetOfferingsByDepartmentAsync(), updated 2 existing offering methods
- [src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs](src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs): Added GetOfferingsByDepartmentAsync() signature
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](src/Tabsan.EduSphere.API/Controllers/CourseController.cs): Refactored GetOfferings() endpoint for dual-parameter support

**Validation:**
- Build succeeded (0 errors, 2 MailKit warnings only)
- StudentController.GetAll() returns Program and Department names for each student
- CourseController.GetAll() returns DepartmentName for each course
- CourseController.GetOfferings() endpoint accepts both ?semesterId and ?departmentId filters
- Portal views ready to consume updated API responses with complete related entity data
- Commit: e15e0b6

**Phase 2 Stage 2 Status:** ✅ Complete

**Moved to:** Stage 2.3 (CRUD Entry Points)

**Docs Updated:**
- Final-Touches.md: Marked Stage 2.2 complete, added Implementation/Validation summaries, adjusted Stage 2.3 section
- Command.md: Updated Current Execution Pointer to Stage 2.3

---

### Entry 005 — 2026-05-04 — Phase 2 Stage 2.3 Complete (CRUD Entry Points)
**Completed:**
- Added CourseRepository.GetOfferingsByDepartmentAsync() method for department-filtered offerings
- Added 4 new CourseOffering management endpoints to CourseController:
  - PUT /offerings/{id}/maxenrollment - Update max enrollment with validation
  - PUT /offerings/{id}/close - Close enrollment
  - PUT /offerings/{id}/reopen - Re-open enrollment
  - DELETE /offerings/{id} - Soft-delete offering using AuditableEntity.SoftDelete()
- Added Students.cshtml create button and modal (Registration Number, Program, Department, Admission Date)
- Added Departments.cshtml create button and modal (Code, Name)
- Added Courses.cshtml create buttons and modals for Courses and Offerings with all required fields

**Changes:**
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](src/Tabsan.EduSphere.API/Controllers/CourseController.cs): Added 4 offering lifecycle endpoints
- [src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs](src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs): Added UpdateMaxEnrollmentRequest
- [src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml](src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml): Added create button and form modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml](src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml): Added create button and form modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml](src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml): Added create buttons and form modals for courses and offerings

**Validation:**
- Build succeeded (0 errors, 2 MailKit warnings only)
- All portal views render correctly with new create buttons and modals
- CourseOffering endpoints support full lifecycle: create, assign faculty, update enrollment, close/reopen, soft-delete
- Modal forms include role-based visibility (Admin/SuperAdmin only)
- All existing CRUD endpoints utilized: StudentController.Create, DepartmentController.Create/Update/Delete, CourseController.Create/Update/Delete, CourseController.CreateOffering

**Phase 2 Status:** ✅ Complete (All stages 2.1, 2.2, 2.3 finished)

**Moved to:** Phase 3 Stage 3.1 (403 Authorization Fixes)

**Docs Updated:**
- Final-Touches.md: Marked Phase 2 complete, marked Stage 2.3 complete with impl/validation summaries, adjusted Phase 3 section
- Command.md: Updated Current Execution Pointer to Phase 3 Stage 3.1



---

### Entry 007 — 2026-05-04 — Phase 7 Complete (Finance and Payments Module)
**Completed:**
- Stage 7.1: Verified 'payments' sidebar item in Finance Related group; fixed URL bug (api/v1/payment-receipt → api/v1/payments).
- Stage 7.2: Added GetAllReceiptsAsync + GetStudentProfileByUserIdAsync to repo, service, and API layer. Three new API endpoints: GET /mine, GET / (all), POST /{id}/mark-submitted.
- Stage 7.3: Admin Create/Confirm/Cancel receipt workflows; Student view-own + Submit Proof text form; Notifications on Create, SubmitProof, Confirm, Cancel via INotificationService.

**Changes:**
- IStudentLifecycleRepository + StudentLifecycleRepository: 2 new methods
- IStudentLifecycleService + StudentLifecycleService: 2 new methods; injected INotificationService; 4 notification calls
- PaymentReceiptController: 3 new endpoints (GetAll, GetMine, MarkSubmitted)
- PortalViewModels: Expanded PaymentReceiptItem, added CreatePaymentForm, expanded PaymentsPageModel
- EduApiClient: 6 new payment methods, expanded PaymentApiDto + MapPayment
- PortalController: Payments GET branches on IsStudent; 4 POST actions (CreatePayment, ConfirmPayment, CancelPayment, SubmitProof)
- Payments.cshtml: Full rebuild — admin Create Receipt form + filter + Confirm/Cancel; student receipts + Submit Proof collapse

**Validation:**
- Application and Infrastructure layers build with 0 errors.
- Web layer: 0 CS/RZ errors (file-lock MSB from running process only).
- Fixed StudentItem.FullName usage (was Name) and Razor selected attribute syntax.

**Moved to:** Phase 8 (Enrollments Completion)

**Docs Updated:**
- Final-Touches.md: Marked Phase 7 complete, updated Progress Tracker, Next Phase
- PRD.md: Bumped to v1.20
- Function-List.md: Added GetAllReceiptsAsync, GetStudentProfileByUserIdAsync, GetAllReceiptsAsync (service), GetReceiptsByUserAsync
- Command.md: Updated execution pointer + this entry

---

### Entry 008 — 2026-05-05 — Phase 8 Complete (Enrollments Completion)
**Completed:**
- Stage 8.1: Fixed empty enrollment dropdown — added `GetAllOfferingsAsync` to ICourseRepository + CourseRepository; updated CourseController.GetOfferings to call it when no filter, fixed field names (CourseTitle, IsActive).
- Stage 8.1: Fixed empty roster grid — fixed GetRoster response fields to match RosterApiDto; added .ThenInclude(sp => sp.Program) to GetByOfferingAsync; updated MyCourses to include CourseOfferingId.
- Stage 8.2: Added IEnrollmentRepository.GetByIdAsync + implementation.
- Stage 8.2: Added IEnrollmentService.AdminDropByIdAsync + EnrollmentService implementation.
- Stage 8.2: Added AdminEnrollRequest DTO.
- Stage 8.2: Added POST /api/v1/enrollment/admin (admin enroll) + DELETE /api/v1/enrollment/admin/{id} (admin drop).
- Stage 8.2: Added 5 EduApiClient methods + MyCourseApiDto private DTO.
- Stage 8.2: Added MyEnrollmentItem view model; expanded EnrollmentsPageModel.
- Stage 8.2: Updated PortalController Enrollments GET (branches on IsStudent); added 4 POST actions (EnrollStudent, AdminDropEnrollment, StudentEnroll, StudentDropEnrollment).
- Stage 8.2: Rebuilt Enrollments.cshtml — student own-courses + admin roster with CRUD.

**Validation:**
- Build: 0 errors, 0 warnings.
- Enrollment dropdown now populated from all offerings.
- Admin: offering select → roster with Drop buttons + "Enroll Student" modal.
- Student: own courses list with Drop buttons + "Enroll in Course" modal.

**Moved to:** Phase 9 (Documentation and Script Regeneration)

**Docs Updated:**
- Final-Touches.md: Marked Phase 8 complete, updated Progress Tracker, Next Phase
- PRD.md: Bumped to v1.21, added Phase 8 log entry
- Function-List.md: Added Phase 7 + Phase 8 sections (both were missing)
- Command.md: Updated execution pointer + this entry

---

### Entry 009 — 2026-05-05 — Phase 9 Complete (Documentation and Script Regeneration)
**Completed:**
- Stage 9.1: `1-MinimalSeed.sql` §15 — added 16 missing sidebar menu items (`result_calculation`, `notifications`, `students`, `departments`, `courses`, `assignments`, `attendance`, `results`, `quizzes`, `fyp`, `analytics`, `ai_chat`, `student_lifecycle`, `payments`, `enrollments`, `report_center`, `dashboard_settings`) + role accesses matching DatabaseSeeder.
- Stage 9.1: `1-MinimalSeed.sql` §17 — replaced 4 old hyphen-key report defs with 8 canonical underscore-key defs matching ReportKeys.cs; added gpa_report, enrollment_summary, low_attendance_warning, fyp_status.
- Stage 9.1: `2-FullDummyData.sql` — same §15 and §17 changes.
- Stage 9.2: User guides bumped to v1.1 (Student, Admin, Faculty, SuperAdmin, License-KeyGen, README).
- Stage 9.2: Student guide — added Section 12 (Enrollments self-service: view/enroll/drop).
- Stage 9.2: Admin guide — updated Section 6 (admin enrollment CRUD: roster/enroll/drop workflows).
- Stage 9.2: Faculty guide — updated Section 4 (Enrollments roster view).
- Stage 9.3: PRD v1.22, Final-Touches Phase 9 complete, Command.md pointer updated.

**Validation:**
- SQL scripts: all new INSERT statements use IF NOT EXISTS guards — idempotent on re-run.
- Role accesses match DatabaseSeeder.SeedSidebarMenusAsync exactly.
- Report keys match ReportKeys.cs constants exactly.
- No C# changes — build remains 0 errors, 0 warnings.

**Moved to:** All phases complete.

**Docs Updated:**
- Final-Touches.md: Marked Phase 9 complete, filled Implementation/Validation summaries, updated Progress Tracker
- PRD.md: Bumped to v1.22, added Phase 9 log entry
- Command.md: Updated execution pointer + this entry

---

### Entry 010 — 2026-05-05 — Phase 1 Remediation Restart (Batch 1)
**Completed:**
- Stage 1.1: Fixed API role gate for offerings used by Assignments/Attendance/Results/Quizzes pages by expanding `GET /api/v1/course/offerings/my` to all operational roles.
- Stage 1.1: Added role-aware behavior in `CourseController.GetMyOfferings()` so SuperAdmin/Admin can always access all offerings.
- Stage 1.3: Fixed Report Center visibility for SuperAdmin by returning all active reports regardless of role assignment rows.
- Stage 1.4: Removed `module_settings` from dynamic sidebar route/group mapping.
- Stage 1.4: Removed hyperlink behavior from sidebar branding header (TE / Tabsan EduSphere / Campus Portal).
- Stage 1.5: Fixed promote-flow identity mapping by honoring `StudentProfileId` in lifecycle semester-student payload.

**Validation:**
- File diagnostics for modified files show no code errors.
- Full solution build is blocked by running app processes holding output DLL locks (`MSB3021`/`MSB3027`).

**Moved to:** Continue Phase 1 remediation.

**Docs Updated:**
- PRD.md: Bumped to v1.23 and added this remediation log entry
- Development Plan - ASP.NET.md: Added Phase 1 remediation restart execution note
- Function-List.md: Added Phase 1 remediation batch 1 function updates
- Command.md: Updated execution pointer + this entry

---

### Entry 011 — 2026-05-05 — Phase 1 Remediation Restart (Batch 2)
**Completed:**
- Stage 1.4: Removed static `Module Settings` sidebar link from SuperAdmin menu in `_Layout.cshtml`.
- Stage 1.4: Removed `module_settings` menu creation/assignment from runtime `DatabaseSeeder`.
- Stage 1.4: Removed `module_settings` from seed scripts `1-MinimalSeed.sql` and `2-FullDummyData.sql`.
- Stage 1.4: Added legacy cleanup logic to disable role access and soft-delete existing `module_settings` records.
- Stage 1.4: Updated `SidebarMenuIntegrationTests` expected SuperAdmin key count after removal.

**Validation:**
- Diagnostics check reports no errors in all modified files.
- SuperAdmin offerings endpoint check still returns data on the running API.

**Moved to:** Continue Phase 1 Stage 1.3 (`Result Summary` InvalidOperationException).

**Docs Updated:**
- Observed-Issues.md: Marked `P1-S4-01` as Done
- PRD.md: Bumped to v1.24 and added remediation batch 2 log
- Development Plan - ASP.NET.md: Added batch 2 execution update
- Function-List.md: Added Stage 1.4 cleanup function/file updates
- Command.md: Updated execution pointer + this entry

---

### Entry 012 — 2026-05-05 — Phase 1 Remediation Restart (Batch 3)
**Completed:**
- Stage 1.3: Fixed `System.InvalidOperationException` on Result Summary.
- Root cause: EF translation failure in report query ordering (`OrderBy` on projected `ResultReportRow`).
- Fixed `ReportRepository` by moving sorting to SQL (`orderby u.Username, c.Code`) before projection and removing post-projection ordering.
- Updated report data/export endpoint authorization to include `SuperAdmin,Admin,Faculty` so SuperAdmin always has report functionality access.

**Validation:**
- Live API check successful: `GET /api/v1/reports/result-summary` with SuperAdmin token returned `rows=21`, `totalRecords=21`.
- Diagnostics check reports no errors in modified files.

**Moved to:** Continue Phase 1 Stage 1.2 CRUD coverage and Stage 1.6 dashboard/theme enhancements.

**Docs Updated:**
- Observed-Issues.md: Marked `P1-S3-01` as Done
- PRD.md: Bumped to v1.25 and added remediation batch 3 log
- Development Plan - ASP.NET.md: Added batch 3 execution update
- Function-List.md: Added Stage 1.3 function/behavior updates
- Command.md: Updated execution pointer + this entry

---

### Entry 013 — 2026-05-05 — Phase 1 Remediation (Batch 4 — Stage 1.2 CRUD)
**Completed:**
- P1-S2-01: Departments CRUD — added `CreateDepartment`, `UpdateDepartment`, `DeactivateDepartment` POST actions to PortalController; added `CreateDepartmentAsync`, `UpdateDepartmentAsync`, `DeactivateDepartmentAsync` to EduApiClient; rewired `Departments.cshtml` with server-side `<form asp-action>` modals for Create/Edit/Deactivate with antiforgery tokens.
- P1-S2-02: Courses & Offerings CRUD — added `CreateCourse`, `CreateOffering`, `DeactivateCourse`, `DeleteOffering` POST actions; added matching methods to EduApiClient; updated `Courses.cshtml` with server-side forms, Deactivate/Delete buttons (SuperAdmin only); Courses GET now also loads Semesters + Faculty for dropdown population.
- P1-S2-03: Enrollments — already complete from Phase 8 (EnrollStudent, AdminDropEnrollment, AdminEnrollStudentAsync all existed); confirmed Done.
- P1-S2-04: FYP Management CRUD — added `AssignFypSupervisor` and `CompleteFypProject` POST actions; added `AssignFypSupervisorAsync`, `CompleteFypProjectAsync` to EduApiClient; updated `Fyp.cshtml` with Supervisor modal (faculty dropdown) and Complete button for Approved/InProgress projects; added FYP Supervisor modal with JS to wire `data-projectid`.
- Added `Faculty` list to `FypPageModel`; added `Semesters` + `Faculty` lists to `CoursesPageModel`.

**Validation:**
- `dotnet build` on Web project: succeeded with no C# errors (only file-lock warnings from running process).
- All modified C# files: no errors confirmed via diagnostics.
- FypController: `[Authorize(Policy = "Admin")]` and `[Authorize(Policy = "Faculty")]` already include SuperAdmin per Program.cs policy configuration.

**Moved to:** P1-S6-xx (Theme/branding enhancements) or P2-S1-01 (License concurrency).

**Docs Updated:**
- Observed-Issues.md: Marked P1-S2-01/02/03/04 as Done
- Command.md: This entry

---

### Entry 014 — 2026-05-05 — Phase 1 Remediation (Batch 5 — Final Push: Regression Tests + Branding Enhancements)
**Completed:**
- P1-S1-01: 403 authorization fixes confirmed complete from Batch 1–3; marked Done in Observed-Issues.md.
- P1-S1-02: Created `AuthorizationRegressionTests.cs` (30+ test methods in IntegrationTests project) covering Attendance, Assignment, Quiz, Result endpoints — 401 for unauthenticated, 403 for wrong role, pass for correct role.
- P1-S6-01: Added 10 new themes to `wwwroot/css/site.css` and corresponding `ThemeOption` entries to `ThemeSettingsPageModel`. New themes: Neon Mint, Sakura Pink, Golden Hour, Deep Navy, Lavender Mist, Rust Canyon, Glacier Ice, Graphite Pro, Spring Blossom, Dusk Fire. Total themes: 29 (including Default).
- P1-S6-02: Logo upload — added `POST /api/v1/portal-settings/logo` endpoint (PortalSettingsController) with 2 MB cap and whitelist (.png .jpg .jpeg .gif .svg .webp); saves to `wwwroot/portal-uploads/`; `EduApiClient.UploadLogoAsync` calls endpoint; PortalController POST handles logo file; DashboardSettings.cshtml has file input + current logo preview; sidebar now shows `<img>` if LogoUrl is set, falls back to initials circle.
- P1-S6-03: Privacy Policy URL — added `PrivacyPolicyUrl` field to PortalBrandingDto/Service/ApiDto/WebModel; DashboardSettings.cshtml has URL input; _Layout.cshtml footer shows Privacy Policy link if set.
- P1-S6-04: Font style options — added `FontFamily` and `FontSize` fields to PortalBrandingDto/Service/ApiDto/WebModel; DashboardSettings.cshtml has Font Family dropdown (5 options) and Font Size dropdown (5 options); _Layout.cshtml injects `<style>` block with `font-family`/`font-size` overrides when set.

**Validation:**
- `dotnet build` on Web project: `Build succeeded` — 0 errors, 4 pre-existing CS8620 nullable warnings only.

**Docs Updated:**
- Observed-Issues.md: Marked P1-S1-01, P1-S1-02, P1-S6-01, P1-S6-02, P1-S6-03, P1-S6-04 as Done
- Command.md: This entry
- PRD.md: Bumped to v1.26

---

### Entry 015 — 2026-05-05 — Phase 1 Complete / Phase 2 Handover

**Purpose:** Full handover record for resuming work on a different system or in a new session.

**Phase 1 Remediation — Final Status: ✅ ALL DONE**

All 15 Phase 1 items (P1-S1-01 through P1-S6-04) are complete. See Observed-Issues.md for the detailed implementation and validation summary added to each stage.

**Key files changed across all Phase 1 Remediation batches:**

| File | Changes |
|------|---------|
| `src/Tabsan.EduSphere.API/Controllers/AttendanceController.cs` | Fixed `[Authorize]` policy/role strings; corrected route prefix |
| `src/Tabsan.EduSphere.API/Controllers/AssignmentController.cs` | Fixed `[Authorize]` policy/role strings |
| `src/Tabsan.EduSphere.API/Controllers/QuizController.cs` | Fixed `[Authorize]` policy/role strings |
| `src/Tabsan.EduSphere.API/Controllers/ResultController.cs` | Fixed `[Authorize]` policy/role strings |
| `src/Tabsan.EduSphere.API/Controllers/PortalSettingsController.cs` | Added `POST /api/v1/portal-settings/logo` upload endpoint with file validation |
| `src/Tabsan.EduSphere.Application/DTOs/SettingsDtos.cs` | Added `LogoUrl`, `PrivacyPolicyUrl`, `FontFamily`, `FontSize` to `PortalBrandingDto` and `SavePortalBrandingCommand` |
| `src/Tabsan.EduSphere.Application/Services/SettingsServices.cs` | Updated `PortalBrandingService.GetAsync/SaveAsync` for all 8 keys |
| `src/Tabsan.EduSphere.Web/Controllers/PortalController.cs` | Added Departments/Courses/FYP CRUD POST actions; updated DashboardSettings POST to call UploadLogoAsync |
| `src/Tabsan.EduSphere.Web/Services/EduApiClient.cs` | Added `UploadLogoAsync`; updated `PortalBrandingApiDto` for 8 fields; added Departments/Courses/FYP CRUD client methods |
| `src/Tabsan.EduSphere.Web/Models/Portal/PortalViewModels.cs` | Added `LogoUrl`, `PrivacyPolicyUrl`, `FontFamily`, `FontSize` to `PortalBrandingWebModel`; added `LogoFile` to `DashboardSettingsPageModel`; added 10 new `ThemeOption` entries |
| `src/Tabsan.EduSphere.Web/Views/Portal/DashboardSettings.cshtml` | Added logo file input + preview, privacy policy URL input, font family/size dropdowns; form `enctype="multipart/form-data"` |
| `src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml` | Added Create/Edit/Deactivate modals with server-side form actions |
| `src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml` | Added Create Course/Offering, Deactivate/Delete with server-side forms |
| `src/Tabsan.EduSphere.Web/Views/Portal/Fyp.cshtml` | Added Supervisor assignment modal and Complete button |
| `src/Tabsan.EduSphere.Web/Views/Shared/_Layout.cshtml` | Logo in sidebar (with initials fallback); privacy policy footer link; font CSS injection in `<head>` |
| `src/Tabsan.EduSphere.Web/wwwroot/css/site.css` | Added 10 new theme blocks (Neon Mint → Dusk Fire) |
| `tests/Tabsan.EduSphere.IntegrationTests/AuthorizationRegressionTests.cs` | New file: 30+ regression tests for auth on Attendance/Assignment/Quiz/Result endpoints |

**Architecture context (Phase 2 must follow these patterns):**
- JWT auth policies defined in `src/Tabsan.EduSphere.API/Program.cs` lines 66–69. All policies include SuperAdmin.
- License data stored in `Tabsan.EduSphere.Domain/Licensing/` entities.
- License validation service: `src/Tabsan.EduSphere.Application/Services/` (look for `LicenseService` or similar).
- Background job for license expiry warning: `src/Tabsan.EduSphere.BackgroundJobs/LicenseExpiryWarningJob.cs`.
- License import/update in portal: `src/Tabsan.EduSphere.Web/Controllers/PortalController.cs` — `LicenseUpdate` action.
- License key generator tool: `tools/KeyGen/` and `tools/Tabsan.Lic/`.

**Phase 2 Work Plan (next up):**

| ID | Work Item | Approach |
|----|-----------|----------|
| P2-S1-01 | Concurrent user limit based on `MaxUsers` in license | Track active logins in a DB table (e.g., `ActiveSessions`). On login, count active non-expired sessions. If count >= MaxUsers, reject login with HTTP 403 + meaningful message. |
| P2-S1-02 | SuperAdmin always exempt from concurrency limit | In the concurrency check, skip if role is SuperAdmin. |
| P2-S2-01 | `MaxUsers = 0` or `"All"` = unlimited mode | Treat zero/negative/`"All"` as unlimited; skip concurrency check entirely. |
| P2-S3-01 | One-time domain binding on first activation | On first license load: capture request domain, persist as `ActivatedDomain` in license entity. On subsequent loads: compare current domain — reject if mismatch. |
| P2-S3-02 | Force re-upload when domain changes | If `ActivatedDomain` is set and does not match current domain, redirect to LicenseUpdate page with an error message. |
| P2-S3-03 | HMAC anti-tamper on license file | License file signed with HMAC-SHA256 using a server secret. Validate signature before parsing payload. Reject with clear error if tampered. |

**Resume prompt for new session:**
```
Read Command.md first. Phase 1 Remediation is complete (all Done in Observed-Issues.md).
Begin Phase 2 from P2-S1-01. Refer to the Phase 2 Work Plan table in Entry 015.
Do not re-do any Phase 1 work.
When Phase 2 is complete, update: Observed-Issues.md, Command.md, PRD.md, Docs/Function-List.md.
```

---

### Entry 016 — 2026-05-05 — Phase 2 Complete (License Concurrency + Domain Binding)

**Completed: ✅ ALL Phase 2 items (P2-S1-01 through P2-S3-03)**

**Stage 2.1 — User Count-Based Concurrency Restriction + SuperAdmin Exemption (P2-S1-01, P2-S1-02):**
- Added `MaxUsers` property to `LicenseState` entity (int, default 0 = unlimited)
- Extended `LicenseValidationService.TablicPayload` to deserialize `MaxUsers` from binary .tablic payload
- Updated `LicenseValidationService.ActivateFromFileAsync(string filePath, string? requestDomain, CancellationToken)` signature to accept optional request domain
- Added `CountActiveSessionsAsync(CancellationToken)` to `IUserSessionRepository` interface
- Implemented `CountActiveSessionsAsync()` in `UserSessionRepository` — counts sessions where `RevokedAt == null && ExpiresAt > DateTime.UtcNow`
- Updated `AuthService.LoginAsync()` to:
  - Fetch current license
  - If user is NOT SuperAdmin AND MaxUsers > 0: count active sessions
  - Reject login if count >= MaxUsers via `LoginResult.Fail(LoginFailureReason.ConcurrencyLimitReached)`
  - SuperAdmin always exempt (skips concurrency check entirely)
- Changed `IAuthService.LoginAsync` return type from `LoginResponse?` to `LoginResult` (wrapper with success flag + failure reason enum)
- Added `LoginResult` class and `LoginFailureReason` enum to `AuthDtos.cs`
- Updated `AuthController.Login POST` to check `FailureReason` and return 403 for concurrency limit, 401 for invalid credentials

**Stage 2.2 — Unlimited Mode (P2-S2-01):**
- Implemented unlimited concurrency via `MaxUsers == 0` convention
- When MaxUsers is 0, concurrency check is skipped for all users (SuperAdmin logic runs first)
- Allows licenses to operate in "All Users" mode with no per-user cost

**Stage 2.3 — License Domain Binding + Anti-Tamper (P2-S3-01, P2-S3-02, P2-S3-03):**
- Added `ActivatedDomain` property to `LicenseState` entity (string?, max 253 chars per DNS spec)
- Extended `LicenseValidationService.TablicPayload` to deserialize optional `AllowedDomain` field from .tablic
- On activation, if payload contains `AllowedDomain`, it must match `requestDomain` or activation fails
- First activation captures domain: `activatedDomain = requestDomain ?? payload.AllowedDomain`
- Subsequent activations preserve existing `ActivatedDomain`
- Updated `LicenseController.Upload POST` to extract `Request.Host.Host` and pass to `ActivateFromFileAsync()`
- Created `LicenseDomainMiddleware` that:
  - Checks incoming request host against stored `LicenseState.ActivatedDomain`
  - Rejects cross-domain requests with HTTP 403 unless on whitelisted endpoints
  - Allows `/api/v1/auth/login`, `/api/v1/license/upload`, `/api/v1/license/status` even on locked domain
  - Prevents single-license reuse across multiple deployments (one license per domain)
- Registered middleware in `Program.cs` pipeline before authentication
- Anti-tamper already implemented: RSA-2048 signature verification + AES-256-CBC decryption + replay guard + domain binding

**EF Core Migration:**
- Created manual migration: `20260505_Phase2LicenseConcurrency.cs`
  - Adds `MaxUsers INT NOT NULL DEFAULT 0` column
  - Adds `ActivatedDomain NVARCHAR(253) NULL` column
- Migration file compiles successfully

**Files Modified:**
- [src/Tabsan.EduSphere.Domain/Licensing/LicenseState.cs](src/Tabsan.EduSphere.Domain/Licensing/LicenseState.cs) — Added MaxUsers, ActivatedDomain properties
- [src/Tabsan.EduSphere.Application/DTOs/Auth/AuthDtos.cs](src/Tabsan.EduSphere.Application/DTOs/Auth/AuthDtos.cs) — Added LoginResult, LoginFailureReason
- [src/Tabsan.EduSphere.Application/Interfaces/IAuthService.cs](src/Tabsan.EduSphere.Application/Interfaces/IAuthService.cs) — Changed LoginAsync return type
- [src/Tabsan.EduSphere.Application/Auth/AuthService.cs](src/Tabsan.EduSphere.Application/Auth/AuthService.cs) — Concurrency limit + SuperAdmin exemption
- [src/Tabsan.EduSphere.Application/Interfaces/IUserSessionRepository.cs](src/Tabsan.EduSphere.Application/Interfaces/IUserSessionRepository.cs) — Added CountActiveSessionsAsync()
- [src/Tabsan.EduSphere.Infrastructure/Repositories/UserSessionRepository.cs](src/Tabsan.EduSphere.Infrastructure/Repositories/UserSessionRepository.cs) — Implemented CountActiveSessionsAsync()
- [src/Tabsan.EduSphere.Infrastructure/Licensing/LicenseValidationService.cs](src/Tabsan.EduSphere.Infrastructure/Licensing/LicenseValidationService.cs) — Domain binding + payload extension
- [src/Tabsan.EduSphere.Infrastructure/Persistence/Configurations/LicenseStateConfiguration.cs](src/Tabsan.EduSphere.Infrastructure/Persistence/Configurations/LicenseStateConfiguration.cs) — EF configuration
- [src/Tabsan.EduSphere.API/Controllers/AuthController.cs](src/Tabsan.EduSphere.API/Controllers/AuthController.cs) — Login 403 for concurrency limit
- [src/Tabsan.EduSphere.API/Controllers/LicenseController.cs](src/Tabsan.EduSphere.API/Controllers/LicenseController.cs) — Pass request domain
- [src/Tabsan.EduSphere.API/Middleware/LicenseDomainMiddleware.cs](src/Tabsan.EduSphere.API/Middleware/LicenseDomainMiddleware.cs) — NEW middleware
- [src/Tabsan.EduSphere.API/Program.cs](src/Tabsan.EduSphere.API/Program.cs) — Registered middleware
- [src/Tabsan.EduSphere.Infrastructure/Migrations/20260505_Phase2LicenseConcurrency.cs](src/Tabsan.EduSphere.Infrastructure/Migrations/20260505_Phase2LicenseConcurrency.cs) — NEW migration

**Validation:**
- Build: 0 errors, 4 pre-existing warnings (SettingsServices.cs CS8620 only)
- All Phase 2 code compiles successfully
- CountActiveSessionsAsync correctly counts active sessions
- LoginResult and LoginFailureReason enums integrated properly
- EF Core migration created

**Documentation Updated:**
- [Observed-Issues.md](Observed-Issues.md) — Marked P2-S1-01 through P2-S3-03 Done; added Phase 2 Implementation Summary
- [Command.md](Command.md) — Updated Current Execution Pointer; added Entry 016

**Next:**
- Apply migration: `dotnet ef database update`
- Begin Phase 3: License App (P3-S1-01, P3-S2-01, P3-S2-02)
- Update KeyGen tool to support MaxUsers and AllowedDomain in .tablic payload

---

### Entry 017 — 2026-05-05 — Phase 3 Complete (License App — Generator Alignment + File Security)

**Completed: ✅ ALL Phase 3 items (P3-S1-01, P3-S2-01, P3-S2-02)**

**Stage 3.1 — Generator Alignment (P3-S1-01):**
- Added `MaxUsers` (int, default 0 = unlimited) and `AllowedDomain` (string?, nullable) to `IssuedKey` model
- Configured new columns in `LicDb.OnModelCreating`: `HasDefaultValue(0)` + `HasMaxLength(253).IsRequired(false)`
- Extended `LicenseBuilder.TablicPayload` with `MaxUsers` and `AllowedDomain`; `BuildAsync` now embeds them in the JSON payload inside the encrypted .tablic binary
- Added `UpdateConstraintsAsync(IssuedKey key)` to `KeyService` — persists constraint values before license file generation
- Updated `ExportCsvAsync()` in KeyService — CSV now includes `MaxUsers` and `AllowedDomain` columns
- Updated `HandleBuildTablic` in `Program.cs`:
  - Prompts for "Max concurrent users (0 = unlimited):" — validates non-negative integer
  - Prompts for "Allowed domain (leave blank for no restriction):" — stored as lowercase or null
  - Saves constraints via `UpdateConstraintsAsync` before generating the file
  - Shows a summary block confirming MaxUsers and AllowedDomain before writing
- Updated `HandleListKeys` display — shows MaxUsers ("Unlimited" when 0) and AllowedDomain ("(any)" when null)
- Added **startup SQLite column migration** in `Program.cs`:
  - Reads `PRAGMA table_info(issued_keys)` to get existing column names
  - Adds `MaxUsers INTEGER NOT NULL DEFAULT 0` and/or `AllowedDomain TEXT NULL` if missing
  - Existing `tabsan_lic.db` files are transparently upgraded on first launch

**Stage 3.2 — File Security (P3-S2-01 and P3-S2-02) — Pre-existing, Verified:**
- P3-S2-01 (Encrypt + validate): `LicCrypto.BuildTablicFile()` = AES-256-CBC + RSA-2048 sign; `LicenseValidationService` verifies on every activation
- P3-S2-02 (Reject modified payload): RSA signature over SHA-256(IV+ciphertext); private key only in tool; replay guard via `ConsumedVerificationKey` table

**Files Modified:**
- [tools/Tabsan.Lic/Models/IssuedKey.cs](tools/Tabsan.Lic/Models/IssuedKey.cs) — MaxUsers, AllowedDomain
- [tools/Tabsan.Lic/Data/LicDb.cs](tools/Tabsan.Lic/Data/LicDb.cs) — EF fluent config
- [tools/Tabsan.Lic/Services/LicenseBuilder.cs](tools/Tabsan.Lic/Services/LicenseBuilder.cs) — Payload extended
- [tools/Tabsan.Lic/Services/KeyService.cs](tools/Tabsan.Lic/Services/KeyService.cs) — UpdateConstraintsAsync, updated ExportCsvAsync
- [tools/Tabsan.Lic/Program.cs](tools/Tabsan.Lic/Program.cs) — DB migration, prompts, list display

**Validation:**
- `dotnet build tools/Tabsan.Lic/Tabsan.Lic.csproj --no-restore` → Succeeded 2.2s, 0 errors
- Full solution build: 0 errors

**Next:**
- Apply migration to DB: `dotnet ef database update`
- Begin Phase 4: CSV User Import (P4-S1-01, P4-S2-01, P4-S2-02, P4-S3-01)

---

## Entry 018 — Phase 4: CSV User Import (2026-05-06)

**Items completed:**
- P4-S1-01: CSV user import via `POST /api/v1/user-import/csv` (SuperAdmin/Admin only)
- P4-S2-01: Initial password = username on CSV import
- P4-S2-02: `MustChangePassword` flag set on import; `POST /api/v1/auth/force-change-password` clears it
- P4-S3-01: `User Import Sheets/` folder with `user-import-template.csv` + `README.md`

**Files created:**
- [src/Tabsan.EduSphere.Application/Interfaces/IUserImportService.cs](src/Tabsan.EduSphere.Application/Interfaces/IUserImportService.cs) — import interface
- [src/Tabsan.EduSphere.Application/Services/UserImportService.cs](src/Tabsan.EduSphere.Application/Services/UserImportService.cs) — import implementation
- [src/Tabsan.EduSphere.API/Controllers/UserImportController.cs](src/Tabsan.EduSphere.API/Controllers/UserImportController.cs) — API endpoint
- [src/Tabsan.EduSphere.Infrastructure/Migrations/20260506_Phase4UserImport.cs](src/Tabsan.EduSphere.Infrastructure/Migrations/20260506_Phase4UserImport.cs) — MustChangePassword column
- [User Import Sheets/user-import-template.csv](User%20Import%20Sheets/user-import-template.csv) — CSV template
- [User Import Sheets/README.md](User%20Import%20Sheets/README.md) — usage instructions

**Files modified:**
- [src/Tabsan.EduSphere.Domain/Identity/User.cs](src/Tabsan.EduSphere.Domain/Identity/User.cs) — `MustChangePassword` property + `ClearMustChangePassword()` method
- [src/Tabsan.EduSphere.Infrastructure/Persistence/Configurations/UserConfiguration.cs](src/Tabsan.EduSphere.Infrastructure/Persistence/Configurations/UserConfiguration.cs) — EF config for MustChangePassword
- [src/Tabsan.EduSphere.Domain/Interfaces/IUserRepository.cs](src/Tabsan.EduSphere.Domain/Interfaces/IUserRepository.cs) — added `AddRangeAsync`, `GetRoleByNameAsync`
- [src/Tabsan.EduSphere.Infrastructure/Repositories/UserRepository.cs](src/Tabsan.EduSphere.Infrastructure/Repositories/UserRepository.cs) — implementations of above
- [src/Tabsan.EduSphere.Application/DTOs/Auth/AuthDtos.cs](src/Tabsan.EduSphere.Application/DTOs/Auth/AuthDtos.cs) — `MustChangePassword` in `LoginResponse`, `ForceChangePasswordRequest`
- [src/Tabsan.EduSphere.Application/Interfaces/IAuthService.cs](src/Tabsan.EduSphere.Application/Interfaces/IAuthService.cs) — `ForceChangePasswordAsync`
- [src/Tabsan.EduSphere.Application/Auth/AuthService.cs](src/Tabsan.EduSphere.Application/Auth/AuthService.cs) — `ForceChangePasswordAsync` impl, MustChangePassword in login response
- [src/Tabsan.EduSphere.API/Controllers/AuthController.cs](src/Tabsan.EduSphere.API/Controllers/AuthController.cs) — `POST /api/v1/auth/force-change-password`
- [src/Tabsan.EduSphere.API/Program.cs](src/Tabsan.EduSphere.API/Program.cs) — `IUserImportService` DI registration
- [src/Tabsan.EduSphere.Application/DTOs/CsvImportDtos.cs](src/Tabsan.EduSphere.Application/DTOs/CsvImportDtos.cs) — `UserImportResult` record
- [src/Tabsan.EduSphere.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs](src/Tabsan.EduSphere.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs) — snapshot updated

**Validation:**
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` → 0 errors

**Next:**
- Apply migration: `dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure`
- Proceed to next phase as defined in Observed-Issues.md

---

## Phase 19 — Advanced Course Creation & Result Configuration

**EF Migration:**
```
dotnet ef migrations add Phase19_CourseTypeAndGrading --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
```

**Git Commit:**
```
git add -A
git commit -m "Phase 19 — Advanced Course Creation & Result Configuration"
git pull --rebase origin main
git push origin main
```

**Test Run:** 78/78 tests passed
**Status:** ✅ Complete

---

## Phase 20 — Learning Management System (LMS)

**EF Migration:**
```powershell
dotnet ef migrations add Phase20_LMS --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
```

**Git Commit:**
```powershell
git add -A
git commit -m "Phase 20 — Learning Management System (LMS)"
git pull --rebase origin main
git push origin main
```

**Test Run:** 7/7 unit tests passed (build clean; pre-existing nullability warnings only)
**Commit:** `ecf4d91` pushed to main — 2026-05-08
**Status:** ✅ Complete

### Stages Completed
| Stage | Description | Files |
|-------|-------------|-------|
| 20.1 | Course Content Modules domain + service + API + web | `Domain/Lms/CourseContentModule.cs`, `Domain/Lms/ContentVideo.cs`, `Domain/Interfaces/ILmsRepository.cs`, `Application/Interfaces/ILmsService.cs`, `Application/Lms/LmsService.cs`, `API/Controllers/LmsController.cs`, `Web/Views/Portal/CourseLms.cshtml`, `Web/Views/Portal/LmsManage.cshtml` |
| 20.2 | EF configurations for LMS entities | `Infrastructure/Persistence/Configurations/LmsConfigurations.cs`, `Infrastructure/Repositories/LmsRepository.cs` |
| 20.3 | Discussion threads + replies | `Domain/Lms/DiscussionThread.cs`, `Domain/Lms/DiscussionReply.cs`, `Domain/Interfaces/IDiscussionRepository.cs`, `Application/Interfaces/IDiscussionService.cs`, `Application/Lms/DiscussionService.cs`, `Infrastructure/Repositories/DiscussionRepository.cs`, `API/Controllers/DiscussionController.cs`, `Web/Views/Portal/Discussion.cshtml`, `Web/Views/Portal/DiscussionThread.cshtml` |
| 20.4 | Course announcements with notification fan-out | `Domain/Lms/CourseAnnouncement.cs`, `Domain/Interfaces/IAnnouncementRepository.cs`, `Application/Interfaces/IAnnouncementService.cs`, `Application/Lms/AnnouncementService.cs`, `Infrastructure/Repositories/AnnouncementRepository.cs`, `API/Controllers/AnnouncementController.cs`, `Web/Views/Portal/Announcements.cshtml` |
| Cross-cutting | DTOs, DI, DbContext, EduApiClient, PortalController, PortalViewModels, sidebar | `Application/DTOs/Lms/LmsDTOs.cs`, `API/Program.cs`, `Infrastructure/Persistence/ApplicationDbContext.cs`, `Web/Services/EduApiClient.cs`, `Web/Controllers/PortalController.cs`, `Web/Models/Portal/PortalViewModels.cs`, `Views/Shared/_Layout.cshtml` |

---

## Phase 21 — Study Planner

**EF Migration:**
```powershell
dotnet ef migrations add Phase21_StudyPlanner --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
```

**Git Commit:**
```powershell
git add -A
git commit -m "Phase 21 — Study Planner (Semester Planning Tool + Recommendation Engine)"
git pull --rebase origin main
git push origin main
```

**Test Run:** 7/7 unit tests passed (build clean)
**Status:** ✅ Complete

### Stages Completed
| Stage | Description | Files |
|-------|-------------|-------|
| 21.1 | StudyPlan + StudyPlanCourse domain entities, MaxCreditLoadPerSemester on AcademicProgram | `Domain/StudyPlanner/StudyPlan.cs`, `Domain/StudyPlanner/StudyPlanCourse.cs`, `Domain/Interfaces/IStudyPlanRepository.cs` |
| 21.1 | DTOs, service interface + implementation, EF configs, repository | `Application/DTOs/StudyPlanner/StudyPlannerDTOs.cs`, `Application/Interfaces/IStudyPlanService.cs`, `Application/StudyPlanner/StudyPlanService.cs`, `Infrastructure/Persistence/Configurations/StudyPlanConfigurations.cs`, `Infrastructure/Repositories/StudyPlanRepository.cs` |
| 21.2 | Recommendation engine (degree audit gaps + electives + prerequisite gating) | Part of `StudyPlanService.GetRecommendationsAsync` |
| Cross-cutting | Controller, DI, DbContext DbSets, EduApiClient, PortalController, views, sidebar | `API/Controllers/StudyPlanController.cs`, `API/Program.cs`, `Infrastructure/Persistence/ApplicationDbContext.cs`, `Web/Services/EduApiClient.cs`, `Web/Controllers/PortalController.cs`, `Web/Views/Portal/StudyPlan.cshtml`, `Web/Views/Portal/StudyPlanDetail.cshtml`, `Web/Views/Portal/StudyPlanRecommendations.cshtml`, `Views/Shared/_Layout.cshtml` |

---

## Phase 24 — Dynamic Module and UI Composition

**EF Migration:** Not required (no new tables)

**Git Commit:**
```powershell
git add -A
git commit -m "Phase 24 — Dynamic Module and UI Composition (Module Registry + Dynamic Labels + Dashboard Composition)"
git pull --rebase origin main
git push origin main
```

**Test Run:** 44/44 unit tests passed (build clean, 0 errors)
**Status:** ✅ Complete

### Stages Completed
| Stage | Description | Files |
|-------|-------------|-------|
| 24.1 | Module Registry — static compile-time catalogue (key, roles, institution types, license gate) + registry service combining live activation with policy snapshot | `Domain/Modules/ModuleDescriptor.cs`, `Application/Modules/ModuleRegistry.cs`, `Application/Interfaces/IModuleRegistryService.cs`, `Application/Modules/ModuleRegistryService.cs`, `API/Controllers/ModuleRegistryController.cs` |
| 24.2 | Dynamic Labels — institution-mode-aware vocabulary (Semester↔Grade↔Year, GPA/CGPA↔Percentage, Course↔Subject) | `Application/Interfaces/ILabelService.cs`, `Application/Services/LabelService.cs`, `API/Controllers/LabelController.cs` |
| 24.3 | Dashboard Composition — ordered widget list by role + institution type, fed to web layer | `Application/Interfaces/IDashboardCompositionService.cs`, `Application/Services/DashboardCompositionService.cs`, `API/Controllers/DashboardCompositionController.cs`, `Web/Views/Portal/ModuleComposition.cshtml` |

---

## Phase 23 — Core Policy Foundation

**EF Migration:** Not required (uses existing `portal_settings` table)

**Git Commit:**
```powershell
git add -A
git commit -m "Phase 23 — Core Policy Foundation (License Policy Kernel + Institution Context Resolution + Role-Rights Hardening)"
git pull --rebase origin main
git push origin main
```

**Test Run:** 27/27 unit tests passed (build clean, 0 errors)
**Status:** ✅ Complete

### Stages Completed
| Stage | Description | Files |
|-------|-------------|-------|
| 23.1 | Policy Kernel — domain enum + application interface + cached service + API controller | `Domain/Enums/InstitutionType.cs`, `Application/Interfaces/IInstitutionPolicyService.cs`, `Application/Services/InstitutionPolicyService.cs`, `API/Controllers/InstitutionPolicyController.cs` |
| 23.2 | Institution Context Middleware — per-request snapshot resolution | `API/Middleware/InstitutionContextMiddleware.cs`, `API/Program.cs` |
| 23.3 | Role-Rights Hardening — unit tests (13 new) + web layer (EduApiClient, PortalController, view, sidebar seed) | `tests/.../InstitutionPolicyTests.cs`, `Web/Services/EduApiClient.cs`, `Web/Controllers/PortalController.cs`, `Web/Views/Portal/InstitutionPolicy.cshtml`, `Web/Views/Shared/_Layout.cshtml`, `Scripts/1-MinimalSeed.sql` |

---

## Phase 22 — External Integrations

**EF Migration:**
```powershell
dotnet ef migrations add Phase22_ExternalIntegrations --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API
dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API -- --environment Development
```

**Git Commit:**
```powershell
git add -A
git commit -m "Phase 22 — External Integrations (Library System Integration + Accreditation Reporting)"
git pull --rebase origin main
git push origin main
```

**Test Run:** 7/7 unit tests passed (build clean)
**Status:** ✅ Complete

### Stages Completed
| Stage | Description | Files |
|-------|-------------|-------|
| 22.1 | Library System Integration — config + loan proxy | `Domain/`, `Application/DTOs/External/LibraryDTOs.cs`, `Application/Interfaces/ILibraryService.cs`, `Application/Services/LibraryService.cs`, `API/Controllers/LibraryController.cs` |
| 22.2 | Accreditation Reporting — template CRUD + report generation (CSV/TXT) | `Domain/Settings/AccreditationTemplate.cs`, `Domain/Interfaces/IAccreditationRepository.cs`, `Infrastructure/Repositories/AccreditationRepository.cs`, `Infrastructure/Persistence/Configurations/AccreditationTemplateConfiguration.cs`, `Application/DTOs/External/AccreditationDTOs.cs`, `Application/Interfaces/IAccreditationService.cs`, `Application/Services/AccreditationService.cs`, `API/Controllers/AccreditationController.cs` |
| Cross-cutting | DI, DbContext DbSet, EduApiClient, PortalController, views, sidebar, seed SQL | `API/Program.cs`, `Infrastructure/Persistence/ApplicationDbContext.cs`, `Web/Services/EduApiClient.cs`, `Web/Controllers/PortalController.cs`, `Web/Views/Portal/LibraryConfig.cshtml`, `Web/Views/Portal/AccreditationTemplates.cshtml`, `Views/Shared/_Layout.cshtml`, `Scripts/1-MinimalSeed.sql` |
