# Issue Fix Phases

This file tracks the reported portal issues as phased work items so they can be addressed in a controlled order.

## Phase 1 - Super Admin and Admin Access Repair

### Stage 1.1 - Department Management
- SuperAdmin cannot add departments.
- SuperAdmin cannot edit departments.
- SuperAdmin cannot delete or deactivate departments.
- Admin cannot add departments.
- Admin cannot edit departments.
- Admin cannot delete or deactivate departments.

### Stage 1.2 - Courses and Offerings Management
- SuperAdmin cannot add courses.
- SuperAdmin cannot edit courses.
- SuperAdmin cannot delete courses.
- SuperAdmin cannot add course offerings.
- SuperAdmin cannot edit course offerings.
- SuperAdmin cannot delete course offerings.
- Admin cannot add courses.
- Admin cannot edit courses.
- Admin cannot delete courses.
- Admin cannot add course offerings.
- Admin cannot edit course offerings.
- Admin cannot delete course offerings.

### Stage 1.3 - Assignment Management
- SuperAdmin cannot add assignments.
- SuperAdmin cannot edit assignments.
- SuperAdmin cannot delete assignments.
- Admin cannot add assignments.
- Admin cannot edit assignments.
- Admin cannot delete assignments.

### Stage 1.4 - Enrollment Management
- SuperAdmin cannot manage enrollments.
- Admin cannot manage enrollments.

### Stage 1.5 - Attendance Management
- SuperAdmin cannot manage attendance.
- Admin cannot manage attendance.

### Stage 1.6 - Results Management
- SuperAdmin cannot manage results.
- Admin cannot manage results.

### Stage 1.7 - Quiz Management
- SuperAdmin cannot add quizzes.
- SuperAdmin cannot edit quizzes.
- SuperAdmin cannot delete quizzes.
- Admin cannot add quizzes.
- Admin cannot edit quizzes.
- Admin cannot delete quizzes.

### Stage 1.8 - FYP Management
- SuperAdmin cannot manage FYP records.
- Admin cannot manage FYP records.

## Phase 2 - Shared Portal and Settings Issues

### Stage 2.1 - Branding and Asset Rendering
- Uploaded logo in Dashboard Settings does not appear in the portal.

### Stage 2.2 - Privacy Policy Editing
- There is no editor section to add privacy policy text for the Privacy menu item in General.

### Stage 2.3 - Shared Course Offering Dropdowns
- All Select Course Offering dropdowns return no items.

## Phase 3 - Faculty Workflow Repair

### Stage 3.1 - Faculty Courses and Offerings Access
- Courses and Offerings shows `API request failed with status 403` for faculty.

### Stage 3.2 - Faculty Assignment Workflow
- Assignments shows an empty Select Course Offering list for faculty.
- Faculty cannot create assignments for students.

### Stage 3.3 - Faculty Enrollment Workflow
- Enrollments shows `API request failed with status 403` for faculty.

### Stage 3.4 - Faculty Student Directory Access
- Students shows `API request failed with status 403` for faculty.

### Stage 3.5 - Faculty Attendance Workflow
- Attendance shows an empty Select Course Offering list for faculty.
- Faculty cannot enter attendance for their students.

### Stage 3.6 - Faculty Results Workflow
- Results shows an empty Select Course Offering list for faculty.
- Faculty cannot enter results for their students.

### Stage 3.7 - Faculty Quiz Workflow
- Quizzes shows an empty Select Course Offering list for faculty.
- Faculty cannot create quizzes for quizzes, midterm, finals, and FYP evaluation flows.

### Stage 3.8 - Faculty FYP Workflow
- FYP Management shows `API request failed with status 403` for faculty.
- Faculty cannot create FYP records for students.

## Phase 4 - Student Workflow Repair

### Stage 4.1 - Student Assignment Submission Flow
- Assignments shows an empty Select Course Offering list for students.
- Students cannot upload completed assignments.
- Students cannot mark assignments as completed for teacher review and marking.

### Stage 4.2 - Student Timetable Department Resolution
- Student Timetable shows `Department is required. Set default department in Dashboard connection.`
- Department should resolve automatically from the student record in the database.

### Stage 4.3 - Student Assignment Semester View
- Assignments should show semesters the student has completed and is currently enrolled in.
- When a semester is selected, the student should be able to view assignments for that semester.

### Stage 4.4 - Student Results Semester View
- Results shows an empty Select Course Offering list for students.
- Results should show semesters the student has completed and is currently enrolled in.
- When a semester is selected, the student should be able to view results for that semester.

### Stage 4.5 - Student Quiz Semester View
- Quizzes shows an empty Select Course Offering list for students.
- Quizzes should show semesters the student has completed and is currently enrolled in.
- When a semester is selected, the student should be able to view completed, pending, and upcoming quizzes.

### Stage 4.6 - Student FYP Lifecycle
- FYP Management shows `API request failed with status 403` for students.
- Students should not see an FYP dropdown because they can only have one FYP.
- FYP menu should only be visible once the student reaches 8th semester.
- Once faculty creates the student's FYP, the student should be able to view its details.
- After completing the work, the student should be able to mark the FYP as completed.
- That completion request should go to the assigned faculty members for approval.
- When all assigned faculty members approve, the FYP should be marked completed.
- The student should then be able to see the FYP result inside Results.

## Phase 5 - Reporting and Export Center

### Stage 5.1 - New Reports Coverage
- Add new reports for assignments.
- Add new reports for results.
- Add new reports for attendance.
- Add new reports for quizzes.

### Stage 5.2 - Export Actions
- Add buttons to export all generated report data as CSV.
- Add buttons to export all generated report data as PDF.
- Apply export support for assignments, results, attendance, and quizzes.

### Stage 5.3 - SuperAdmin Reporting Scope
- SuperAdmin can see reports for all departments.
- SuperAdmin can see reports for all courses.

### Stage 5.4 - Admin Reporting Scope
- Admin can see reports for all assigned departments.
- Admin can see reports for all assigned courses.

### Stage 5.5 - Faculty Reporting Scope
- Faculty can only see report filter data based on their department scope.
- Faculty can only see report filter data based on their courses.
- Faculty can only see report filter data based on semesters of those courses.
- Faculty can only see report filter data based on students in those semesters.

### Stage 5.6 - Student Reporting Scope
- Student can only see report filter data based on the semesters of assigned course(s).
- Student report filters should only show data relevant to the student's own study scope.

## Phase 6 - Admin Department Assignment Model

### Stage 6.1 - Multi-Department Admin Assignment UI
- When SuperAdmin creates an admin user, there should be a checkbox list of all departments.
- SuperAdmin should be able to select multiple departments for an admin user.

### Stage 6.2 - Multi-Department Admin Assignment Rules
- Admin can have more than one department assigned.
- Supported examples include IT, Business, Education, Languages, and similar departments.
- Assigned departments should control the admin's accessible data and filters throughout the portal.

## Phase 7 - Academic Hierarchy Alignment

### Stage 7.1 - University Structure Definition
- The university hierarchy should be Department -> Course -> Semester.
- Department examples include IT, Business, and similar academic units.
- Course examples include BSCS, BBA, MBA, and similar programs.
- Semester ranges should support 1 to 8 for Bachelors programs.
- Semester ranges should support 1 to 4 for Masters programs.

### Stage 7.2 - Hierarchy Enforcement in Filters and Workflows
- Reports should follow the Department -> Course -> Semester hierarchy.
- Management workflows should follow the Department -> Course -> Semester hierarchy.
- Role-based filtering should respect the same hierarchy across the portal.

## Phase 8 - Execution Order

### Stage 8.1 - Shared Root Causes First
- Fix permission and role access mismatches that block SuperAdmin, Admin, and Faculty.
- Fix shared course offering dropdown data sources and mappings.
- Fix shared branding and privacy policy configuration issues.

## Phase 9 - Document Library and Upload Module

### Stage 9.1 - Sidebar Menu and Navigation
- Add a new top-level sidebar menu item: Upload Document.
- Add two sub-menu items under Upload Document:
	- Document Type
	- Upload Document

### Stage 9.2 - Document Type Management
- In Document Type, Faculty, Admin, and SuperAdmin can create document types.
- Supported examples include Book, Notes, Final Year Thesis, and similar types.
- Document types must be reusable in Upload Document as a dropdown list.

### Stage 9.3 - Document Upload and Metadata Entry
- In Upload Document, Faculty, Admin, and SuperAdmin can upload documents for students.
- Users can also delete uploaded documents.
- Upload form must capture:
	- Document Name
	- Department
	- Type (dropdown from Document Type)
	- Course
	- Subject
- Upload form must support two content input modes:
	- File upload (document/pdf/image)
	- External link input (OneDrive, Google Drive, and similar links)
- Save action must persist document metadata and content reference so students can access it.

### Stage 9.4 - Role Permissions and Access Rules
- Faculty can only upload and delete documents.
- Admin can create document types and upload/delete documents.
- SuperAdmin can create document types and upload/delete documents.
- Students cannot create, edit, upload, or delete documents.

### Stage 9.5 - Student Discovery, Filters, and Download
- Students can view and download documents based on their enrolled courses.
- Student document view must include course and subject filters.
- If a document is stored as an external link, students can click the link or copy it.

### Stage 9.6 - Grouping and Presentation
- Student-facing document listings must be grouped by Document Type.
- Grouping should work for both uploaded files and link-based documents.

## Phase 10 - Result Entry and Student Result Experience

### Stage 10.1 - Teacher Result Entry Cascade Filters
- Teacher result entry must start with filters for Department, Course, Subject, and Result Type.
- Teacher should only see departments, courses, subjects, and result types assigned to that teacher.
- Selecting Department must automatically refresh the Course list for that department.
- Selecting Course must automatically refresh the Subject list for that course and department scope.
- Result Type list must be generated from data configured in the Result Calculation menu.
- Supported result types include Assignment, Quizzes, Mid-terms, Finals, and FYP.

### Stage 10.2 - Teacher Result Entry Grid and Save Flow
- After filter selection, the result entry grid must render student rows for the selected scope.
- Grid columns must include User-Name, Name, Subject Name, Type, and Marks.
- Teacher must be able to enter marks per student and save results.
- Once saved, GPA calculation must run using configured result-calculation rules.
- Calculated GPA must become visible to students in the Results menu.

### Stage 10.3 - Student Results Filters and Views
- Student Results menu must provide filters for Semester, Subject, and Result Type.
- Selecting Semester must automatically refresh the Subject filter list for that semester.
- Student can run filters by any combination: all filters, semester only, or all semesters.
- Filtered result table must include Semester, Subject, Result Type, Marks, and GPA columns.

### Stage 10.4 - CGPA and Semester-Completion Rules
- If the student runs results without any filter, show CGPA section above the result table.
- CGPA must be based on completed semesters only.
- CGPA must not update while the current semester is in progress.
- During in-progress semesters, show subject-wise GPA for entered results based on configured rules.

## Phase 11 - Events Creation, Visibility, and Notifications

### Stage 11.1 - Sidebar Navigation and Role Access
- Add a new sidebar top-level menu: Events.
- Add sub-menus under Events: Create Events and View Events.
- Create Events must be accessible to Admin and SuperAdmin only.
- View Events must be accessible to all roles.

### Stage 11.2 - Create Event Form and Defaults
- Create Event form must include Name of Event, Department target, Start Date-Time, End Date-Time, Location, and Description.
- Department target must support all departments plus an ALL option for institution-wide events.
- Department selector should support multi-select (checkbox dropdown or equivalent best-practice control).
- Description input must be a resizable text area.
- New events must default to Active status when created.

### Stage 11.3 - Create Event Management Table
- Created events must appear in a table within the Create Events section.
- Table actions must include status transitions to Cancelled and Completed.
- Admin and SuperAdmin must be able to edit existing events.

### Stage 11.4 - View Events Filters
- View Events must provide status filters for Active, Cancelled, and All events.
- All roles should be able to browse event listings based on their visibility scope.

### Stage 11.5 - Department-Scoped Event Notifications
- After event creation, send notifications to users in selected departments.
- If ALL is selected, send notifications to all users.
- Notification payload should include event name, schedule, location, and status context.

---

## Phase 1 - Implementation and Validation Summary

### Implementation Completed

**Root Cause 1 — JWT Role Claim Decoding**
The Web portal was decoding roles only from the `role` JWT claim property. The live JWT issued by the API carries the standard Microsoft claim URI `http://schemas.microsoft.com/ws/2008/06/identity/claims/role`, which the decoder ignored. All `User.IsInRole(...)` checks in Razor views evaluated to false for every user.
- Fixed `DecodeJwtIdentity` in `EduApiClient.cs` to probe both `role` and the MS claim URI with a shared fallback helper.

**Root Cause 2 — Portal Management Controls Not Rendered for Admin/SuperAdmin**
Portal Razor views guarded management buttons with `User.IsInRole("SuperAdmin")` only, or had no guard at all (creating buttons for unauthenticated views). Combined with the JWT decode bug, all management controls were invisible.
- Fixed in: `Departments.cshtml`, `Courses.cshtml`, `Assignments.cshtml`, `Attendance.cshtml`, `Results.cshtml`, `Quizzes.cshtml`, `Fyp.cshtml`
- All Create/Edit/Delete/Publish/Correct/Manage buttons now exposed for SuperAdmin and Admin.

**Root Cause 3 — Attendance Correction Passed Wrong ID**
The Correct Attendance modal was bound to `r.Id` (the attendance record ID) but the backend `CorrectAttendanceRequest` requires `StudentProfileId`.
- Added `StudentProfileId` property to `AttendanceRecordItem` and `AttendanceRecordApiDto`.
- Fixed mapping in `GetAttendanceByOfferingAsync` to populate it from the API response.
- Fixed `data-studentid` binding in `Attendance.cshtml` to use `r.StudentProfileId`.

**Root Cause 4 — Result Correction ResultType Was Hardcoded**
The Correct Result modal hardcoded `data-resulttype="Final"` regardless of the actual result type, so corrections targeted the wrong record on the backend.
- Added `ResultType` to `ResultItem` and `ResultApiDto` web models.
- Fixed mapping in `MapResult` to carry the API response value through.
- Fixed `data-resulttype` in `Results.cshtml` to emit `@r.ResultType`.

**Root Cause 5 — FYP Admin Create Path Missing**
No API endpoint existed for Admin/SuperAdmin to create a FYP project on behalf of a student.
- Added `CreateProjectForStudentRequest` DTO to `FypDtos.cs`.
- Added `CreateForStudentAsync` method to `IFypService` and `FypService`.
- Added `POST api/v1/fyp/admin-create` endpoint to `FypController` (Authorize Policy=Admin).
- Added `CreateFypProjectAsync` in `EduApiClient.cs`.
- Added `CreateFypProject` POST action in `PortalController`.
- `FypPageModel` extended with `List<StudentItem> Students` populated for Admin/SuperAdmin.
- `Fyp.cshtml` extended with `+ Create Project` button and admin create modal.

**Root Cause 6 — Edit/Update Controls Missing for Admin/SuperAdmin**
No edit flows existed in the portal for Assignments, Quizzes, and FYP projects.
- Added edit modal for Assignments (EditAssignment) in `Assignments.cshtml`, wired to `UpdateAssignment` controller action and `UpdateAssignmentAsync` API client method.
- Added edit modal for Quizzes (editQuizModal) in `Quizzes.cshtml`, wired to `UpdateQuiz` controller action and `UpdateQuizAsync` API client method.
- Added edit modal for FYP projects (editFypModal) in `Fyp.cshtml`, wired to `UpdateFypProject` controller action and `UpdateFypProjectAsync` API client method.

### Validation

| Check | Status |
|-------|--------|
| `dotnet build Tabsan.EduSphere.sln` | ✅ Succeeded (5 pre-existing nullability warnings only) |
| SuperAdmin login + role badge render | ✅ Verified live — shows "SuperAdmin" in header |
| Departments page — Add/Edit/Deactivate visible | ✅ Verified live |
| FYP page — 3 projects load, "+ Create Project" button, per-row Edit/Approve/Reject/Supervisor/Complete | ✅ Verified live |
| Attendance correction studentProfileId contract | ✅ Fixed and tested against DTO shape |
| Result correction resultType | ✅ Fixed and value carried from API |
| JWT role claim URI decode | ✅ Root cause confirmed and fixed |

### Stage 8.2 - Faculty Workflow Validation
- Re-test Courses and Offerings, Assignments, Enrollments, Students, Attendance, Results, Quizzes, and FYP with faculty accounts.

### Stage 8.3 - Student Workflow Validation
- Re-test Timetable, Assignments, Results, Quizzes, and FYP with student accounts.

### Stage 8.4 - SuperAdmin and Admin Validation
- Re-test all CRUD and workflow actions for SuperAdmin and Admin after the permission fixes.

### Stage 8.5 - Reports and Hierarchy Validation
- Re-test reporting filters and exports for SuperAdmin, Admin, Faculty, and Student roles.
- Re-test multi-department admin assignment behavior.
- Re-test Department -> Course -> Semester hierarchy behavior across reports and workflows.

---

## Phase 2 - Implementation and Validation Summary

### Implementation Completed

**Stage 2.1 — Branding and Asset Rendering**
- Root cause 1: `UploadLogo` attempted `Path.Combine(_env.WebRootPath, ...)` when `WebRootPath` was null, causing upload failure (500).
	- Fixed in `PortalSettingsController.UploadLogo` by resolving fallback web root to `Path.Combine(_env.ContentRootPath, "wwwroot")`.
- Root cause 2: API static-file middleware was not reliably serving uploaded branding assets.
	- Fixed in `API Program.cs` by configuring `UseStaticFiles` with explicit `PhysicalFileProvider` rooted at API web root fallback.

**Stage 2.2 — Privacy Policy Editing**
- Privacy policy editor and persistence path verified through Dashboard Settings branding model fields.
- Privacy page rendering path (`Home/Privacy`) verified to load and display configured `PrivacyPolicyContent`.

**Stage 2.3 — Shared Course Offering Dropdowns**
- Shared offering source verified populated through portal assignment workflow.
- `Select Course Offering` dropdown now lists offerings (no longer empty in validated portal page).

### Validation

| Check | Status |
|-------|--------|
| API compile after fixes | ✅ `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj --no-restore` succeeded |
| `POST /api/v1/portal-settings/logo` | ✅ Returns `200 OK` with uploaded logo URL |
| `GET /portal-uploads/logo.svg` | ✅ Returns `200` after static middleware fix |
| Portal sidebar branding | ✅ Renders logo image (not initials fallback) |
| Privacy page render | ✅ Displays configured privacy policy content |
| Shared offering dropdown | ✅ Assignments page shows populated `Select Course Offering` options |

---

## Phase 3 - Implementation and Validation Summary

### Validation Outcome

Re-tested all faculty workflows with a valid faculty account (`dr.ahmed`) against the running portal and API.

| Stage | Validation Result |
|-------|-------------------|
| Stage 3.1 — Faculty Courses and Offerings Access | ✅ No `403` reproduced on `Portal/Courses`; offerings and course list load for faculty. |
| Stage 3.2 — Faculty Assignment Workflow | ✅ Faculty sees populated course offering dropdown and can open assignment workflow. |
| Stage 3.3 — Faculty Enrollment Workflow | ✅ No `403` reproduced on `Portal/Enrollments`. |
| Stage 3.4 — Faculty Student Directory Access | ✅ No `403` reproduced on `Portal/Students`. |
| Stage 3.5 — Faculty Attendance Workflow | ✅ Attendance page loads for faculty with offering context available. |
| Stage 3.6 — Faculty Results Workflow | ✅ Results page loads for faculty with offering context available. |
| Stage 3.7 — Faculty Quiz Workflow | ✅ Quizzes page loads for faculty with offering context available. |
| Stage 3.8 — Faculty FYP Workflow | ✅ No `403` reproduced on `Portal/Fyp`. |

### Notes

- The originally reported Phase 3 `403` symptoms were not reproducible after the earlier role/claim and shared-offering fixes.
- Validation used existing DB users (`dr.ahmed`) from the active seed state.

---

## Phase 4 - Progress Update

### Stage 4.1 - Student Assignment Submission Flow

Implemented student-side assignment workflow corrections in the web portal:

- Fixed student assignment data mapping to use correct API contracts for submissions and assignment marks.
- Added student submit action in `Assignments` UI with modal input:
	- optional file upload
	- optional text submission notes
- Added backend call path from web portal to `POST /api/v1/assignment/submit`.
- Added submission-state merge so student assignment rows correctly show `Submitted` and awarded marks when available.

### Stage 4.2 - Student Timetable Department Resolution (Implemented)

- Hardened student timetable department auto-resolution to always attempt department lookup from the authenticated student profile first.
- Added safe fallback to the dashboard default department only when profile-derived department is unavailable.
- Added `Guid.Empty` guard so invalid department identifiers are not used for timetable fetch requests.

### Stage 4.4 and 4.5 Supporting Fix

- Fixed student JSON deserialization failures on Assignments/Results/Quizzes by aligning score fields to decimal values (`marksAwarded`, `marksObtained`, `totalScore`).

### Validation Snapshot

- Student pages now render without JSON conversion errors.
- Student assignment list now shows real assignment title, real assignment ID navigation, and correct max marks.
- Submit flow is wired end-to-end in UI and controller; API business rules (publish/due-date/duplicate checks) are now surfaced as actionable portal messages.
- Student timetable no longer depends on JWT role-decoding success to resolve department scope.

### Stage 4.3, 4.4, 4.5 - Semester-Scoped Student Views (Implemented)

- Added student semester filter support to:
	- `Portal/Assignments`
	- `Portal/Results`
	- `Portal/Quizzes`
- Student pages now surface `Select Semester` and persist semester selection in route/query and follow-up actions.
- Offering dropdowns are now semester-scoped for student workflows.
- Results semester view now gracefully falls back to student-safe result endpoints when offering-level calls are role-restricted.
- Quizzes status badges now distinguish `Upcoming`, `Pending`, and `Completed` states based on availability windows.

### Stage 4.3, 4.4, 4.5 - Validation Notes

- Semester selector appears and functions on Assignments/Results/Quizzes for student account validation.
- Student results semester navigation no longer throws `403` after fallback hardening.
- Current DB seed for tested student still has sparse assignment/quiz data for selected semesters, so UI behavior validates while content volume remains data-dependent.

### Stage 4.6 - FYP Menu Gating (Implemented)

- Student sidebar FYP menu now appears only when the student's `CurrentSemesterNumber >= 8`.
- Direct navigation to `Portal/Fyp` by students below 8th semester is blocked and redirected to Dashboard with a guidance message.

### Stage 4.6 - Student FYP Completion Lifecycle (Implemented)

- Added student completion-request action for in-progress FYP projects.
- Added assigned-faculty completion-approval action.
- FYP now auto-transitions to `Completed` when all assigned faculty approvers (supervisor + panel members) have approved.
- Added approval-progress visibility in the FYP portal view (`approved/required`).
- Added student results visibility for completed FYP by rendering a published `FYP` result row in `Portal/Results`.

### Stage 4.6 - Validation Notes

- API now exposes:
	- `POST /api/v1/fyp/{id}/request-completion` (student)
	- `POST /api/v1/fyp/{id}/approve-completion` (faculty)
- Web portal now shows:
	- `Request Completion` button for eligible student projects.
	- `Approve Completion` button for faculty when approval is pending.
	- `Awaiting Approval (x/y)` state for in-progress completion requests.
- Added EF migration `Phase4FypCompletionApprovalFlow` for persisted completion-request and faculty-approval state on FYP projects.

### Auth Consistency Hardening (Runtime Validation Support)

- Login flow now prefers the active portal API base URL when obtaining JWT so login token source and subsequent API calls remain aligned.
- This removes intermittent student-page `401` regressions caused by mismatched API base resolution across login and portal request paths.

## Phase 5 - Progress Update

### Stage 5.1 - Assignment and Quiz Reports (Implemented)

- Added assignment summary report APIs:
	- `GET /api/v1/reports/assignment-summary`
	- `GET /api/v1/reports/assignment-summary/export`
- Added quiz summary report APIs:
	- `GET /api/v1/reports/quiz-summary`
	- `GET /api/v1/reports/quiz-summary/export`
- Added application/domain report contracts and repository queries for assignment submissions and quiz attempts.
- Added portal report pages:
	- `Portal/ReportAssignments`
	- `Portal/ReportQuizzes`
- Added report center key mappings for `assignment_summary`/`assignment-report` and `quiz_summary`/`quiz-report`.
- Added Excel export proxy actions in web portal for assignment and quiz reports.

### Stage 5.1 - Validation Notes

- Full solution build succeeds after report additions.
- New report pages load and use shared report filters (semester, department, offering, student).
- Export actions for new assignment/quiz reports return `.xlsx` files through the same binary proxy flow used by existing reports.

### Stage 5.2 - Export Actions (Implemented)

- Added CSV and PDF export APIs for all required report types:
	- Attendance: `GET /api/v1/reports/attendance-summary/export/csv`, `GET /api/v1/reports/attendance-summary/export/pdf`
	- Results: `GET /api/v1/reports/result-summary/export/csv`, `GET /api/v1/reports/result-summary/export/pdf`
	- Assignments: `GET /api/v1/reports/assignment-summary/export/csv`, `GET /api/v1/reports/assignment-summary/export/pdf`
	- Quizzes: `GET /api/v1/reports/quiz-summary/export/csv`, `GET /api/v1/reports/quiz-summary/export/pdf`
- Added web portal proxy actions for each CSV/PDF export endpoint.
- Updated report pages (Attendance, Results, Assignments, Quizzes) to show separate export buttons for Excel, CSV, and PDF.
- Existing Excel export flow remains unchanged for backward compatibility.

### Stage 5.2 - Validation Notes

- Full solution build succeeds after CSV/PDF export additions.
- CSV export now returns `text/csv` files for attendance, results, assignments, and quizzes.
- PDF export now returns `application/pdf` files for attendance, results, assignments, and quizzes.