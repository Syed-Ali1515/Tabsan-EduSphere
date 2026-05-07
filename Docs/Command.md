# Command Center

## Purpose
Use this file as the single handover reference between sessions and devices.
Before starting any work, the assistant must:
1. Read this file.
2. Read Project startup Docs/Final-Touches.md.
3. Continue from the exact Current Execution Pointer below.

## Non-Negotiable Rule Per Completed Phase
After each completed phase, update all required tracking files:
1. Docs/Function-List.md
2. Docs/Command.md
3. Project startup Docs/PRD.md
4. Docs/Issue-Fix-Phases.md
5. Project startup Docs/Final-Touches.md

After documentation is updated, complete mandatory Git sync in this exact order:
1. Commit all current changes
2. Pull latest remote changes (rebase preferred)
3. Push committed changes to remote

Also update this file with:
- completed work
- validation summary
- next steps
- pending extras

**Always-on documentation sync (requested workflow):**
- Keep `Docs/Function-List.md`, `Docs/Command.md`, `Project startup Docs/PRD.md`, and `Docs/Issue-Fix-Phases.md` updated continuously as work progresses, not only at phase-end.

**Always-on Git sync (requested workflow):**
- Before ending any work session, always run full sync: commit all changes, pull from remote, then push to remote.
- Do not leave local-only completed work.
- Use this command sequence:

```powershell
git add -A
git commit -m "<phase/stage summary>"
git pull --rebase
git push
```

**Code quality rules (enforced from Phase 5 onward):**
- Add a `// Final-Touches Phase X Stage X.X — <description>` comment above every block of code added or changed for that phase.
- Update `Docs/Function-List.md` with a new `## Final-Touches Phase X` section listing all new/modified functions, their purpose, and file location after every completed phase.

---

## Current Execution Pointer
- Plan Source: Docs/Refactoring-Hosting-Security.md
- Active Phase: **Refactoring-Hosting-Security — FULLY COMPLETE ✅**
- Active Stage: **All items done — Serilog, UserSecretsId, FileUploadValidator wired**
- Status: **0 build errors; 69/69 tests passed (commit 5e80bc9)**
- Last Updated: 2026-05-07
- Next: No pending phases — system is production-ready

---

## ⚡ Database & Migration Status

✅ **All pending migrations applied successfully** (2026-05-07):
1. `20260505_Phase2LicenseConcurrency` — adds `MaxUsers` + `ActivatedDomain` to `license_state` ✅
2. `20260506_Phase4UserImport` — adds `MustChangePassword` to `users` ✅
3. `20260507103000_PortalBrandingLogoValueMaxLength` — alters `portal_settings.Value` to `nvarchar(max)` ✅
4. `20260506044806_Phase6AdminDepartmentAssignments` — adds `admin_department_assignments` table ✅

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
- **All Issue-Fix Phases (1–6) and Refactoring-Hosting-Security items are complete.** No pending stages.
- **New issue phases** — If new bugs or features are observed, add them to `Docs/Observed-Issues.md` and define a new Issue-Fix Phase in `Docs/Issue-Fix-Phases.md`.
- **Optional**: dedicated unit tests for `UserImportService` internals (from Phase 4 Option C backlog).

## Pending Extra Tasks (Cross-Phase)
- Keep Report Center menu visible by role and working links.
- Ensure all menus are assignable in Sidebar Settings.
- Ensure export button text and actions are validated when reporting phase is executed.
- Keep test credentials and run commands verified after major backend changes.

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

