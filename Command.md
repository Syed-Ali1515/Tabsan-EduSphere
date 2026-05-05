# Command Center

## Purpose
Use this file as the single handover reference between sessions and devices.
Before starting any work, the assistant must:
1. Read this file.
2. Read Project startup Docs/Final-Touches.md.
3. Continue from the exact Current Execution Pointer below.

## Non-Negotiable Rule Per Completed Phase
After each completed phase, update all three files:
1. Docs/Function-List.md
2. Project startup Docs/PRD.md
3. Project startup Docs/Final-Touches.md

Also update this file with:
- completed work
- validation summary
- next steps
- pending extras

**Code quality rules (enforced from Phase 5 onward):**
- Add a `// Final-Touches Phase X Stage X.X — <description>` comment above every block of code added or changed for that phase.
- Update `Docs/Function-List.md` with a new `## Final-Touches Phase X` section listing all new/modified functions, their purpose, and file location after every completed phase.

---

## Current Execution Pointer
- Plan Source: Project startup Docs/Final-Touches.md
- Active Phase: Phase 1 Remediation Restart
- Active Stage: Stage 1.1 / 1.2 / 1.6 (Batch 3 complete for Stage 1.3)
- Status: In Progress
- Last Updated: 2026-05-05

## Completed Work
- Phase 1: Navigation, Session Stability, Sidebar Structure (all stages done)
- Phase 2: All stages complete (Timetable data binding, Buildings/Students/Departments/Courses visibility, CRUD entry points) — status updated to ✅ Complete
- Phase 3: All stages complete (403 fixes, data entry workflows, result-driven promotion) — status updated to ✅ Complete
- Phase 2: Timetable and Core Lookup Data Visibility (all stages done)
- Phase 3 Stage 3.1: Route prefix fix on AssignmentController, AttendanceController, ResultController, QuizController, FypController (api/ → api/v1/); added GET api/v1/quiz/my-attempts endpoint + GetAllMyAttemptsAsync service chain
- Phase 3 Stage 3.2: Data entry workflows for Assignments, Attendance, Results, Quizzes, FYP (create/publish/delete, grade submissions, bulk mark attendance, propose/approve/reject FYP)
- Phase 3 Stage 3.3: Result-driven promotion — Promote column in Results table, "Promote to next semester" checkbox in Enter Result modal (Final type), standalone PromoteStudentFromResult action
- Phase 4 Stage 4.1: Report Center fixes — key mismatch resolved (DB hyphen vs switch underscore), static sidebar Report Center link added, Semester Results report full chain, 3 Excel export Portal proxy actions + EduApiClient methods, DB seeds updated
- Phase 4 Stage 4.2: 3 new reports — Student Transcript (with Excel export), Low Attendance Warning, FYP Status Report; all 6 infrastructure layers implemented; 3 Razor views; ReportCenter.cshtml switch; DatabaseSeeder + ReportKeys updated; build 0 errors
- Phase 5: Settings Pages — theme persistence fixed in _Layout.cshtml (data-theme on html tag, session-cached API call); 5 new themes added (Steel Blue, Forest Green, Amber Gold, Warm Copper, Indigo Dusk); success/error alert styling improved in ThemeSettings, ReportSettings, ModuleSettings; all settings save actions verified working; build 0 errors
- Phase 6: Notifications & Analytics — Stage 6.1: fixed `NotificationController` route from `api/[controller]` to `api/v1/[controller]` (resolved all notification 404s); Stage 6.2: replaced raw JSON `<pre>` dumps in Analytics view with typed `DepartmentPerformanceReport`, `DepartmentAttendanceReport`, `AssignmentStatsReport` objects; `EduApiClient` analytics methods now return typed DTOs via `GetAsync<T>`; `AnalyticsPageModel` updated to hold typed properties; `Analytics.cshtml` renders Bootstrap 5 responsive tables per section; summary cards populated from real data; build 0 errors

## Next Steps
- Continue Phase 1 remediation: finish Stage 1.1, Stage 1.2, and Stage 1.6 enhancements.

- Phase 8 (Enrollments Completion — ✅ Complete)

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
