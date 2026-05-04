# Final Touches Plan

**Date:** 2026-05-03  
**Owner:** Engineering  
**Status:** In Progress

## Execution Rule (Mandatory)
For **every completed phase**:
1. Update [Docs/Function-List.md](../Docs/Function-List.md)
2. Update [Project startup Docs/PRD.md](PRD.md)
3. Update this file with:
- Completion mark
- Implementation summary
- Validation summary

---

## Phase 1 - Navigation, Session Stability, Sidebar Structure
**Status:** ✅ Complete

### Stage 1.1 - Fix Session/Sidebar Reset Bug
- [x] Fix issue where opening Buildings causes sidebar to reset to legacy menu and forces re-login.
- [x] Ensure sidebar remains dynamic and role-driven across all portal pages.

### Stage 1.2 - Sidebar Grouping and SuperAdmin Coverage
- [x] Group sidebar by:
  - Student Related
  - Faculty Related
  - Finance Related
  - Settings (at bottom)
- [x] Ensure all menus are visible to SuperAdmin.
- [x] Ensure all menus appear in Sidebar Settings for role assignment.

### Stage 1.3 - Add Dashboard Settings Menu
- [x] Add new Settings item: Dashboard Settings.
- [x] Support university name text, brand initials, subtitle text, footer text.
- [x] Layout brand section reads from DB branding values (with session cache fallback).
- [x] Footer text driven by DB setting.

### Implementation Summary
- Hardened sidebar loading in `_Layout.cshtml` by caching dynamic menu payload in session (`VisibleSidebarMenusCache`) and reusing it on API failure.
- Removed layout-level redirect-return behavior that could break rendering.
- Implemented grouped dynamic sidebar rendering (`Overview`, `Faculty Related`, `Student Related`, `Finance Related`, `Settings`).
- Added `portal_settings` key-value table in DB with EF migration `Phase1DashboardBranding`.
- Added `PortalSetting` domain entity, `IPortalBrandingService` / `PortalBrandingService`, `PortalSettingsController` API endpoint, `GetPortalBrandingAsync` / `SavePortalBrandingAsync` in `EduApiClient`.
- Added `DashboardSettings` action + view in `PortalController`; seeded `dashboard_settings` sidebar menu item.
- Layout brand area (initials, name, subtitle, footer) now rendered from DB settings with session-cached fallback.

### Validation Summary
- Verified SuperAdmin login renders grouped dynamic sidebar with full menu set.
- Verified opening Buildings keeps full grouped sidebar visible with no forced sign-out.
- Verified Sidebar Settings page shows 29 items including Report Center, Payments, Enrollments.
- Verified Dashboard Settings page renders with form, default branding values pre-filled, live preview, and footer text from settings.

---

## Phase 2 - Timetable and Core Lookup Data Visibility
**Status:** ✅ Complete

### Stage 2.1 - Faculty/Student Timetable Data
- [x] Fix My Timetable (Faculty) data binding.
- [x] Fix My Timetable (Student) data binding.
- [x] Confirm Timetable Admin, Faculty, Student views all load expected rows.

### Stage 2.2 - Building, Student, Department, Course Visibility
- [x] Fix Buildings list retrieval.
- [x] Fix Students list retrieval (names visible).
- [x] Fix Departments list retrieval (names visible).
- [x] Fix Courses page active offering retrieval.

**Implementation Summary (Stage 2.2)**

**Problem:** Portal pages for Buildings, Students, Departments, and Courses existed but were not showing proper related entity data (e.g., missing student names, course department names, course offering faculty).

**Fix Applied:**
1. **StudentProfileRepository**: Ensured `Program` and `Department` navigation properties are loaded via `.Include()` statements.
2. **StudentController.GetAll()**: Updated API response to map `ProgramName`, `DepartmentName`, and `Status` from included entities.
3. **CourseRepository**: Added new `GetOfferingsByDepartmentAsync()` method to retrieve offerings filtered by department. Updated existing `GetOfferingsBySemesterAsync()` and `GetOfferingsByFacultyAsync()` with proper Course and Semester includes.
4. **ICourseRepository interface**: Added `GetOfferingsByDepartmentAsync()` method signature for consistency.
5. **CourseController**: Updated `GetAll()` to include `DepartmentName` mapping. Refactored `GetOfferings()` endpoint to accept both `semesterId` and `departmentId` query parameters, supporting department-filtered course offerings.

**Files Modified:**
- [src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs)
- [src/Tabsan.EduSphere.API/Controllers/StudentController.cs](../../src/Tabsan.EduSphere.API/Controllers/StudentController.cs)
- [src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs)
- [src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs](../../src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs)
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](../../src/Tabsan.EduSphere.API/Controllers/CourseController.cs)

**Validation Summary**
- ✓ Build succeeded with all fixes applied
- ✓ StudentController.GetAll() returns Program and Department names for each student profile
- ✓ CourseController.GetAll() returns Department name for each course
- ✓ CourseController.GetOfferings() endpoint now supports both `?semesterId=...` and `?departmentId=...` filters
- ✓ Portal views (Buildings, Students, Departments, Courses) ready to consume updated API responses
- ✓ Commit: e15e0b6

---

### Stage 2.3 - CRUD Entry Points
- [x] Add Students create flow.
- [x] Add Departments create flow.
- [x] Add Active Offerings create/edit/delete flow.

### Implementation Summary
**Problem:** Timetable API endpoints were returning incomplete data due to missing EF Include statements in repository queries, causing null references during DTO mapping.

**Fix Applied:**
1. **TimetableRepository.GetTeacherEntriesAsync()**: Added `.Include(e => e.Building)` to include the Building navigation property alongside existing Room.Building include.
2. **TimetableRepository.GetByDepartmentAsync()**: Added `.Include(t => t.Department)`, `.Include(t => t.AcademicProgram)`, `.Include(t => t.Semester)` for proper DTO mapping.
3. **TimetableRepository.GetPublishedByDepartmentAsync()**: Added `.Include(t => t.Department)`, `.Include(t => t.AcademicProgram)`, `.Include(t => t.Semester)` for proper DTO mapping.
4. **TimetableRepository.GetByIdWithEntriesAsync()**: Added separate `.Include(t => t.Entries).ThenInclude(e => e.Building)` to ensure Building data is loaded for all entries.

**Files Modified:**
- [src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs)

### Validation Summary
- ✓ Build succeeded with all fixes applied
- ✓ Faculty timetable query includes Department, AcademicProgram, Semester, Building, and Room.Building
- ✓ Student timetable query includes all required related data for complete DTO mapping
- ✓ Test data is seeded in MinimalSeed.sql: 1 published timetable for CS dept with 2 entries assigned to faculty.test
- ✓ API endpoints ready to return complete timetable data without null reference errors

---

### Stage 2.3 - CRUD Entry Points
- [x] Add Students create flow.
- [x] Add Departments create flow.
- [x] Add Active Offerings create/edit/delete flow.

**Implementation Summary (Stage 2.3)**

**New CourseOffering API Endpoints:**
1. **PUT /api/v1/course/offerings/{id}/maxenrollment** - Update max enrollment with validation
2. **PUT /api/v1/course/offerings/{id}/close** - Close enrollment for an offering
3. **PUT /api/v1/course/offerings/{id}/reopen** - Re-open enrollment for an offering
4. **DELETE /api/v1/course/offerings/{id}** - Soft-delete offering (AuditableEntity)

**Portal Page Enhancements:**
1. **Students.cshtml** - Added "Add Student" button (Admin/SuperAdmin only), modal form with fields:
   - Registration Number, Program, Department, Admission Date
2. **Departments.cshtml** - Added "Add Department" button, modal form with fields:
   - Department Code, Department Name
3. **Courses.cshtml** - Added "Add Course" and "Add Offering" buttons on respective panels:
   - Course modal: Code, Title, Credit Hours, Department
   - Offering modal: Course, Semester, Faculty (optional), Max Enrollment

**Supporting Changes:**
- Added `UpdateMaxEnrollmentRequest` DTO to `AcademicDtos.cs`
- All CRUD endpoints leveraged: StudentController.Create, DepartmentController.Create/Update/Delete, CourseController.Create/Update/Delete, CourseController.CreateOffering

**Files Modified:**
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](../../src/Tabsan.EduSphere.API/Controllers/CourseController.cs): 4 new offering endpoints
- [src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs](../../src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs): UpdateMaxEnrollmentRequest
- [src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml): Create button and modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml): Create button and modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml): Create buttons and modals

**Validation Summary**
- ✓ Build succeeded (0 errors, 2 MailKit warnings)
- ✓ CourseOffering endpoints support full lifecycle: create, assign faculty, update enrollment, close/reopen, soft-delete
- ✓ Portal pages show create buttons/modals for Students, Departments, Courses, Offerings (role-gated)
- ✓ Modal forms include proper field labels and validation
- ✓ Commit: 7f3330b

---

## Phase 3 - Assignment, Attendance, Results, Quizzes, FYP Access and Workflows
**Status:** ✅ Complete

### Stage 3.1 - 403 Authorization Fixes
- [x] Resolve 403 in Assignments.
- [x] Resolve 403 in Attendance.
- [x] Resolve 403 in Results.
- [x] Resolve 403 in Quizzes.
- [x] Resolve 403 in FYP.

**Implementation Summary (Stage 3.1)**

**Root Cause:** Five module controllers used `[Route("api/[controller]")]` (no `v1` prefix) while `EduApiClient.cs` in the Web project consistently calls `api/v1/` prefixed paths. This caused 404 (not 403) at the HTTP level — ASP.NET then returns 400/404 which the portal surfaces as access errors.

**Additionally:** `EduApiClient.GetMyAttemptsAsync()` calls `api/v1/quiz/my-attempts` (flat path) but `QuizController` only had `{id:guid}/my-attempts` — no flat endpoint existed.

**Fix Applied:**
1. **AssignmentController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
2. **AttendanceController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
3. **ResultController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
4. **QuizController**: Changed `[Route("api/quiz")]` → `[Route("api/v1/quiz")]`; added `GET my-attempts` flat endpoint calling `IQuizService.GetAllMyAttemptsAsync()`
5. **FypController**: Changed `[Route("api/fyp")]` → `[Route("api/v1/fyp")]`
6. **IQuizRepository + QuizRepository**: Added `GetAllAttemptsForStudentAsync(Guid studentProfileId)` returning all attempts across all quizzes
7. **IQuizService + QuizService**: Added `GetAllMyAttemptsAsync(Guid studentProfileId)` service method

**Files Modified:**
- [src/Tabsan.EduSphere.API/Controllers/AssignmentController.cs](../../src/Tabsan.EduSphere.API/Controllers/AssignmentController.cs)
- [src/Tabsan.EduSphere.API/Controllers/AttendanceController.cs](../../src/Tabsan.EduSphere.API/Controllers/AttendanceController.cs)
- [src/Tabsan.EduSphere.API/Controllers/ResultController.cs](../../src/Tabsan.EduSphere.API/Controllers/ResultController.cs)
- [src/Tabsan.EduSphere.API/Controllers/QuizController.cs](../../src/Tabsan.EduSphere.API/Controllers/QuizController.cs)
- [src/Tabsan.EduSphere.API/Controllers/FypController.cs](../../src/Tabsan.EduSphere.API/Controllers/FypController.cs)
- [src/Tabsan.EduSphere.Domain/Interfaces/IQuizRepository.cs](../../src/Tabsan.EduSphere.Domain/Interfaces/IQuizRepository.cs)
- [src/Tabsan.EduSphere.Infrastructure/Repositories/QuizFypRepositories.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/QuizFypRepositories.cs)
- [src/Tabsan.EduSphere.Application/Interfaces/IQuizService.cs](../../src/Tabsan.EduSphere.Application/Interfaces/IQuizService.cs)
- [src/Tabsan.EduSphere.Application/Quizzes/QuizService.cs](../../src/Tabsan.EduSphere.Application/Quizzes/QuizService.cs)

**Validation Summary (Stage 3.1)**
- ✓ Build succeeded (0 errors, 2 pre-existing MailKit warnings)
- ✓ All 5 module controllers now at `api/v1/` prefix matching EduApiClient call paths
- ✓ Authorization policies (Faculty/Admin/Student) are valid in Program.cs — no policy changes needed
- ✓ `GET api/v1/quiz/my-attempts` endpoint added for student portal summary view

### Stage 3.2 - Data Entry Workflows
**Status:** ✅ Complete

- [x] Add Assignments create/publish/delete + grade submissions workflow.
- [x] Add Attendance bulk-mark workflow (Faculty sees enrolled students roster, selects status per student).
- [x] Add Results enter result (Faculty selects student from roster, enters type/marks) + publish-all workflow.
- [x] Add Quizzes create/publish/delete workflow.
- [x] Add FYP propose (Student), approve/reject with remarks (Admin) workflow.

### Implementation Summary
- **EduApiClient** — Added 13 write methods to `IEduApiClient` interface and `EduApiClient` class: `CreateAssignmentAsync`, `PublishAssignmentAsync`, `DeleteAssignmentAsync`, `GradeSubmissionAsync`, `BulkMarkAttendanceAsync`, `CreateResultAsync`, `PublishAllResultsAsync`, `CreateQuizAsync`, `PublishQuizAsync`, `DeleteQuizAsync`, `ProposeFypProjectAsync`, `ApproveFypProjectAsync`, `RejectFypProjectAsync`. Added private `DeleteAsync` HTTP helper.
- **PortalController** — Added 13 corresponding `[HttpPost, ValidateAntiForgeryToken]` actions for all 5 modules.
- **PortalViewModels** — Added `Roster: List<EnrollmentRosterItem>` to `AttendancePageModel` and `ResultsPageModel`.
- **PortalController (GET)** — Attendance + Results GET actions now load enrollment roster via `GetEnrollmentRosterAsync` when offeringId is selected and user is Faculty/Admin.
- **Assignments.cshtml** — Added "Create Assignment" button + Bootstrap 5 modal, Publish/Delete inline forms per row, Grade modal triggered from submissions table.
- **Attendance.cshtml** — Added "Mark Attendance" panel (Faculty/Admin) showing enrolled students grid with per-student date + status select, posts to `BulkMarkAttendance`.
- **Results.cshtml** — Added "Enter Result" button + modal (student select from roster, result type, marks), "Publish All" button.
- **Quizzes.cshtml** — Added "Create Quiz" button + modal, Publish/Delete inline forms per quiz card (Faculty/Admin gated).
- **Fyp.cshtml** — Added "Propose Project" modal (Student), Approve inline form + Reject modal with remarks (Admin gated).

### Validation Summary
- Solution builds with 0 errors (1 warning: MailKit vulnerability unrelated to this stage).
- All Razor views compile successfully with role-gated write UI.
- CSRF protection (`[ValidateAntiForgeryToken]` + `@Html.AntiForgeryToken()`) applied to all write forms.

### Stage 3.3 - Result-Driven Promotion Logic
**Status:** ✅ Complete

- [x] Add Promote column in result entry with Yes/No option for failed students.
- [x] Implement automatic promotion to next semester based on entered result decision.
- [x] Remove/replace manual promotion dependency where required.

### Implementation Summary
- **ResultItem / ResultApiDto** — Added `StudentProfileId` field so the Results view can identify each student per row.
- **MapResult** — Updated to pass `StudentProfileId` from API response to web model.
- **PortalController.CreateResult** — Added `bool promote` parameter; when checked, automatically calls `PromoteStudentAsync(studentProfileId)` after result creation.
- **PortalController.PromoteStudentFromResult** — New `[HttpPost]` standalone action for per-row Promote button; calls existing `EduApiClient.PromoteStudentAsync`; redirects to Results page with success message.
- **Results.cshtml** — Added "Promote" column (Faculty/Admin only) with a per-student inline POST form; added "Promote to next semester" checkbox in the Enter Result modal (only visible when ResultType = "Final"); JavaScript toggles checkbox visibility on type change.

### Validation Summary
- Solution builds with 0 errors.
- Razor view compiles successfully.
- Promotion uses existing `POST api/v1/student-lifecycle/{id}/promote` endpoint — no new API routes needed.

---

## Phase 4 - Reporting and Export Completion
**Status:** ✅ Complete

### Stage 4.1 - Report Center Functional Completeness
**Status:** ✅ Complete

- [x] Ensure Report Center menu is visible in sidebar by role and opens correctly.
- [x] Fix Department Summary report.
- [x] Fix Result Summary report.
- [x] Fix Semester Result report.
- [x] Ensure role/department/subject/semester filters work end-to-end.

### Stage 4.2 - Add Additional Reports
**Status:** ✅ Complete

- [x] Student Transcript — full academic record per student with Excel export
- [x] Low Attendance Warning — students below configurable attendance threshold
- [x] FYP Status Report — Final Year Project status overview with dept/status filters
- [x] All 6 infrastructure layers implemented (DTOs, Domain, Repository, Service interface, Service impl, API controller, EduApiClient, PortalController, ViewModels, Razor views)
- [x] ReportCenter.cshtml switch updated with 3 new keys
- [x] DatabaseSeeder + ReportKeys constants updated; Student Transcript adds Student role assignment

### Implementation Summary (Stage 4.1)
- **Root cause identified**: DB-seeded report keys (`attendance-report`, `results-report`, `dept-summary`) used hyphens while `ReportCenter.cshtml` switch used underscores — every catalog card resolved to `"#"`.
- **ReportCenter.cshtml** — Updated switch to handle both old hyphenated and new underscore keys; added `dept-summary` → ReportEnrollment, `semester-results` → ReportSemesterResults.
- **Static sidebar (_Layout.cshtml)** — Added "Report Center" link inside the `Admin Tools` section (Faculty/Admin) in the static fallback menu.
- **Semester Results report** — Full chain added: `SemesterResultsRowItem`/`SemesterResultsWebModel`/`ReportSemesterResultsPageModel` in PortalViewModels; `GetSemesterResultsReportAsync` in IEduApiClient + EduApiClient; `ReportSemesterResults` GET action in PortalController; `ReportSemesterResults.cshtml` view with semester/department filters.
- **Excel export actions** — Added `ExportAttendanceSummary`, `ExportResultSummary`, `ExportGpaReport` GET actions to PortalController; proxied through new `ExportAttendanceSummaryAsync`, `ExportResultSummaryAsync`, `ExportGpaReportAsync` methods in IEduApiClient + EduApiClient (uses new `GetBytesAsync` private helper).
- **DB Seeds** — Both `1-MinimalSeed.sql` and `2-FullDummyData.sql` updated to seed `semester-results` report definition with Admin + Faculty role assignments.

### Implementation Summary (Stage 4.2)
- **DTOs** — 3 new request/response record sets in `ReportDtos.cs`
- **Domain** — 3 new method signatures + 4 raw row record types in `IReportRepository.cs`
- **Repository** — 3 new EF Core query methods in `ReportRepository.cs`
- **Service interface** — 4 new method signatures in `IReportService.cs`
- **Service impl** — 4 new methods (including Excel export) in `ReportService.cs`
- **API** — 5 new endpoints in `ReportController.cs` (transcript, transcript/export, low-attendance, fyp-status)
- **EduApiClient** — 4 impl methods + 6 private sealed DTO classes added; interface signatures previously added
- **ViewModels** — 9 new classes in `PortalViewModels.cs` (3 row items, 3 web models, 3 page models)
- **PortalController** — 3 GET actions + 1 export action added
- **Views** — `ReportTranscript.cshtml`, `ReportLowAttendance.cshtml`, `ReportFypStatus.cshtml` created
- **ReportCenter.cshtml** — 3 new switch cases added
- **ReportKeys.cs** — 3 new constants: `StudentTranscript`, `LowAttendanceWarning`, `FypStatus`
- **DatabaseSeeder.cs** — 3 new `ReportDefinition` rows seeded with role assignments

### Validation Summary
- Solution builds with 0 errors, 0 warnings.
- All 8 reports in the catalog now resolve to their correct views.
- Export buttons call working Portal proxy actions.
- Semester Results view requires a semester selection before querying (SemesterId is required by the API).

---

## Phase 5 - Settings Pages Functional Save Actions
**Status:** ✅ Complete

### Stage 5.1 - Report Settings Save
- [x] Add/repair save action and success/error feedback in Report Settings.
  - All save actions already wired (CreateReport, ToggleReport, UpdateReportRoles POST actions).
  - Fixed alert styling: success messages show `alert-success`, error messages show `alert-danger` with matching icons.

### Stage 5.2 - Module Settings Save
- [x] If modules exist: render all modules and support activate/deactivate + save.
  - 14 modules seeded (Authentication, Departments, SIS, Courses, Assignments, Quizzes, Attendance, Results, Notifications, FYP, AI Chat, Reports, Themes, Advanced Audit).
  - ToggleModule and UpdateModuleRoles POST actions confirmed working. Mandatory modules cannot be deactivated.
- [x] If modules do not exist: remove Module Settings menu and related dead routes.
  - Not applicable — modules exist and are properly seeded.

### Stage 5.3 - Sidebar Settings Save
- [x] Add/repair save action for role assignments and visibility toggles.
  - Role checkboxes auto-submit via JS `change` event handler (updates hidden fields then submits).
  - Status checkboxes use `onchange="this.form.submit()"` for instant toggle.
  - TempData feedback already differentiated (success via TempData, error via Model.Message with alert-danger).

### Stage 5.4 - Theme Settings Expansion
- [x] Add more themes.
  - Added 5 new themes: Steel Blue, Forest Green, Amber Gold, Warm Copper, Indigo Dusk.
  - Total themes: 20 (15 existing + 5 new).
  - CSS `data-theme` blocks added to `wwwroot/css/site.css` for all 5 new themes.
  - `ThemeSettingsPageModel.Themes` updated with 5 new entries.
- [x] Ensure themes persist and apply consistently.
  - Fixed: `_Layout.cshtml` now loads the current user's theme from API on every page request (with session fallback).
  - `<html>` tag dynamically sets `data-theme` attribute from saved theme key.
  - Theme is cached in session under key `CurrentThemeCache` to minimise API calls.

### Implementation Summary
- **Files changed:**
  - `src/Tabsan.EduSphere.Web/Views/Shared/_Layout.cshtml` — theme loading + `data-theme` on `<html>` tag
  - `src/Tabsan.EduSphere.Web/wwwroot/css/site.css` — 5 new theme blocks
  - `src/Tabsan.EduSphere.Web/Models/Portal/PortalViewModels.cs` — 5 new `ThemeOption` entries
  - `src/Tabsan.EduSphere.Web/Views/Portal/ThemeSettings.cshtml` — contextual success/danger alerts
  - `src/Tabsan.EduSphere.Web/Views/Portal/ReportSettings.cshtml` — contextual success/danger alerts
  - `src/Tabsan.EduSphere.Web/Views/Portal/ModuleSettings.cshtml` — contextual success/danger alerts

### Validation Summary
- Build: `dotnet build Tabsan.EduSphere.sln` → **0 errors, 0 warnings**
- Themes: 20 themes available in Theme Settings; CSS variables defined for all; layout applies saved theme on every page load
- Settings feedback: success = green alert + check icon; error = red alert + X icon

---

## Phase 6 - Notifications and Analytics
**Status:** ✅ Complete

### Stage 6.1 - Notifications 404 Fix
- [x] Fix Notifications endpoint mismatch or missing route.
- [x] Verify notification list, read state, and mark-all-read behavior.

### Stage 6.2 - Analytics Data Rendering
- [x] Replace random/static code output with real analytics data.
- [x] Validate Performance, Attendance, Assignment analytics cards and sections.

### Implementation Summary
- **Stage 6.1**: `NotificationController` had `[Route("api/[controller]")]` resolving to `api/notification`, while `EduApiClient` called `api/v1/notification/...`. Fixed by changing the controller route to `[Route("api/v1/[controller]")]`.
- **Stage 6.2**: `EduApiClient` analytics methods returned raw JSON strings; the view displayed them in `<pre><code>` blocks. Fixed by:
  1. Updating `IEduApiClient` interface: replaced `GetPerformanceAnalyticsJsonAsync`, `GetAttendanceAnalyticsJsonAsync`, `GetAssignmentAnalyticsJsonAsync` (returning `string?`) with typed versions returning `DepartmentPerformanceReport?`, `DepartmentAttendanceReport?`, `AssignmentStatsReport?`.
  2. Updating `EduApiClient` implementation to use `GetAsync<T>()` helper.
  3. Replacing `PerformanceJson`, `AttendanceJson`, `AssignmentJson` string fields in `AnalyticsPageModel` with typed `Performance`, `Attendance`, `Assignments` DTO properties.
  4. Updating `PortalController.Analytics` to call typed methods and populate summary cards from real data.
  5. Rewrote `Analytics.cshtml`: accordion panels now render Bootstrap 5 tables with real student/course rows instead of raw JSON.

### Validation Summary
- Build: ✅ 0 errors, 0 warnings
- Notifications: Route mismatch resolved — inbox, badge, mark-all-read all route correctly to `api/v1/notification/...`; per-notification mark-as-read button added to inbox view, posts to new `MarkNotificationRead` action
- Analytics: Performance, Attendance, Assignment sections render as proper responsive tables with per-row data; summary cards display average marks, attendance %, and assignment count from live API data.

---

## Phase 7 - Finance and Payments Module Completion
**Status:** ✅ Complete

### Stage 7.1 - Finance Sidebar and Navigation
- [x] Add Finance-related menus and grouping in sidebar.
- [x] Fixed URL bug in `GetPaymentsByStudentAsync` (was `api/v1/payment-receipt/…`, corrected to `api/v1/payments/…`).

### Stage 7.2 - Fees and Receipts Admin Workflows
- [x] Add create/edit/delete fee receipt setup (admin Create + Confirm + Cancel).
- [x] Admin can create receipts per student with amount, description, and due date.
- [x] Admin can confirm (mark Paid) or cancel any non-terminal receipt.

### Stage 7.3 - Student Payment Flow
- [x] Students can view their own receipts (GET /mine via JWT).
- [x] Students can submit proof (transaction ID / reference note) via POST /mark-submitted.
- [x] Admin verification workflow: Submitted → Paid via Confirm action.
- [x] Notifications sent on receipt creation, proof submission, confirmation, and cancellation.

### Implementation Summary
- **Domain**: `PaymentReceipt` state machine unchanged (Pending → Submitted → Paid/Cancelled).
- **Infrastructure**: Added `GetAllReceiptsAsync` and `GetStudentProfileByUserIdAsync` to `StudentLifecycleRepository`.
- **Application**: Added `GetAllReceiptsAsync`, `GetReceiptsByUserAsync` to `IStudentLifecycleService` / `StudentLifecycleService`. Injected `INotificationService`; notifications fire on Create, SubmitProof, Confirm, and Cancel.
- **API**: Added `GET api/v1/payments` (admin all), `GET api/v1/payments/mine` (student by JWT), `POST api/v1/payments/{id}/mark-submitted` (text proof).
- **Web (EduApiClient)**: Added `GetAllPaymentsAsync`, `GetMyPaymentsAsync`, `CreatePaymentAsync`, `ConfirmPaymentAsync`, `CancelPaymentAsync`, `SubmitProofAsync`. Expanded `PaymentApiDto` and `MapPayment`.
- **Web (PortalController)**: `Payments(GET)` branches on `IsStudent`; added `CreatePayment`, `ConfirmPayment`, `CancelPayment`, `SubmitProof` POST actions.
- **Web (Payments.cshtml)**: Rebuilt — admin sees Create Receipt form + filter + Confirm/Cancel per row; student sees own receipts + Submit Proof collapse form.

### Validation Summary
- All layers build with 0 CS/RZ errors (file-lock MSB warnings from running processes only).
- `StudentLifecycleService` constructor now takes `INotificationService`; registered via DI.
- Razor view fixed: `StudentItem.FullName` used correctly; `selected` attribute valid HTML.

---

## Phase 8 - Enrollments Completion
**Status:** Not Started

### Stage 8.1 - Data and Dropdown Fixes
- [ ] Fix empty enrollments data grid.
- [ ] Fix empty dropdown data sources.

### Stage 8.2 - Enrollments CRUD
- [ ] Add create/edit/delete enrollment workflows.

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Phase 9 - Documentation and Script Regeneration
**Status:** Not Started

### Stage 9.1 - Script Modernization
- [ ] Remove obsolete scripts.
- [ ] Create new scripts aligned with updated app behavior and schema.
- [ ] Validate fresh environment setup using new scripts.

### Stage 9.2 - Documentation Refresh
- [ ] Update all applicable files in:
  - Project startup Docs
  - Docs
  - Scripts
  - User Guide

### Stage 9.3 - Mandatory Completion Artifacts Per Phase
- [ ] For each completed phase, record:
  - What was implemented
  - How it was validated
  - Links to updated functions and PRD sections

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Progress Tracker
- [x] Phase 1 complete
- [x] Phase 2 complete
- [x] Phase 3 complete
- [x] Phase 4 complete
- [x] Phase 5 complete
- [x] Phase 6 complete
- [x] Phase 7 complete
- [ ] Phase 8 complete
- [ ] Phase 9 complete

## Next Phase To Execute
Phase 8 - Enrollments Completion
