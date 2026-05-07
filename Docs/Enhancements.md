# Enhancements — Gap Analysis (University Portal)

**Source:** Gap Analysis PRD (May 2026)  
**Scope:** Features identified as missing from the current system, organised into phases and stages using the same numbering scheme as `Issue-Fix-Phases.md` (continues from Phase 11).  
**Phases are ordered by implementation sequence** — lowest complexity and fewest dependencies first.  
**Status:** All phases are **Planned — Not Started** unless noted.

---

## Phase 12 — Academic Calendar System ✅ Implemented
**Complexity:** Low–Medium | **Dependencies:** None (builds on existing `Semester` entity)
**Commit:** `6e89af1` — 2026-05-07

### Stage 12.1 — Semester Timeline View ✅
- `AcademicCalendar` portal page visible to all roles; semester filter dropdown.
- Days-remaining color badges (green/yellow/red/grey).
- Admin/SuperAdmin link to Manage Deadlines page.

### Stage 12.2 — Key Deadlines Management ✅
- `AcademicDeadline` entity (`SemesterId`, `Title`, `Description`, `DeadlineDate`, `ReminderDaysBefore`, `IsActive`, `LastReminderSentAt`).
- `IAcademicDeadlineRepository` + `AcademicDeadlineRepository` (EF Core, table `academic_deadlines`).
- `IAcademicCalendarService` + `AcademicCalendarService` — full CRUD + `DispatchPendingRemindersAsync`.
- EF migration: `20260507_Phase12AcademicCalendar`.
- `CalendarController`: GET endpoints for all authenticated roles; POST/PUT/DELETE restricted to Admin/SuperAdmin.
- `AcademicDeadlines` portal page (Admin/SuperAdmin only) with create/edit/delete modals.
- `DeadlineReminderJob`: BackgroundService running daily, dispatches `NotificationType.System` notifications when reminder window arrives.

---

## Phase 13 — Global Search ✅ Implemented (commit `00b7b64`)
**Complexity:** Low | **Dependencies:** None

### Stage 13.1 — Cross-Entity Search API ✅
- New `GET /api/v1/search?q={term}&limit={n}` endpoint accessible to all authenticated roles.
- Searches across: students (name, roll number), courses (code, title), course offerings, faculty (name), departments.
- Results are role-scoped: Admin sees only their assigned-department data; Faculty sees their dept + own offerings; Students see their own enrolled data.
- Returns a typed result list: `{ type, id, label, subLabel, url }`.
- **Files:** `SearchController.cs`, `ISearchService.cs`, `SearchService.cs`, `ISearchRepository.cs` (Application), `SearchRepository.cs`, `SearchDTOs.cs`

### Stage 13.2 — Portal Search Bar ✅
- Global search input in the portal header (`_Layout.cshtml`) — visible on all pages when connected.
- Typeahead dropdown shows top 5 results inline (JS fetch to `/Portal/SearchTypeahead`).
- Pressing Enter or clicking Search opens full results page (`Search.cshtml`) with Bootstrap category tabs.
- Each result links directly to the relevant portal page.
- **Files:** `_Layout.cshtml`, `PortalController.cs`, `PortalViewModels.cs`, `Search.cshtml`, `_SearchResultsList.cshtml`

### Implementation & Validation Summary
- Build: 0 errors, 0 warnings
- Tests: 78/78 passed
- `ISearchRepository` placed in Application layer (depends on Application DTOs)
- No new EF migration required (queries existing tables)
- Role-scoped results: SuperAdmin → all; Admin → assigned depts; Faculty → own dept + offerings; Student → enrolled offerings only

---

## Phase 14 — Helpdesk / Support Ticketing System ✅ Implemented
**Complexity:** Low–Medium | **Dependencies:** Notification system (already exists)
**Commit:** `<pending>` — 2026-05-07

### Stage 14.1 — Ticket Submission and Tracking ✅
- Students and Faculty can raise support tickets from any portal page, categorised by type (Academic, Technical, Administrative).
- New `SupportTicket` entity: `SubmitterId`, `Category`, `Subject`, `Body`, `Status` (Open / InProgress / Resolved / Closed), `AssignedToId`, timestamps.
- Submitter receives in-app notification on each status change.
- Students and Faculty can view their own ticket history with full thread.

### Stage 14.2 — Admin Case Management ✅
- Admin can view, assign, and resolve tickets within their department scope.
- SuperAdmin has unrestricted visibility and can reassign or escalate any ticket.
- Overdue tickets (configurable SLA threshold) are highlighted in the Admin dashboard.

### Stage 14.3 — Faculty Ticket Responses ✅
- Faculty can respond to course-related tickets assigned to them.
- Response messages are stored as `SupportTicketMessage` child rows (thread model).
- Resolved tickets can be re-opened by the submitter within a configurable window.

### Implementation & Validation Summary
- **Files:** `SupportTicket.cs`, `SupportTicketMessage.cs`, `IHelpdeskRepository.cs`, `HelpdeskRepository.cs`, `IHelpdeskService.cs`, `HelpdeskService.cs`, `HelpdeskDTOs.cs`, `HelpdeskController.cs`, `HelpdeskRepository.cs` (infra), EF migration `20260507_Phase14_Helpdesk`, `Helpdesk.cshtml`, `HelpdeskCreate.cshtml`, `HelpdeskDetail.cshtml`, `_TicketStatusBadge.cshtml`, `PortalViewModels.cs`, `EduApiClient.cs`, `PortalController.cs`, `_Layout.cshtml` (sidebar link + route/group maps), `Program.cs` (DI registration)
- Build: 0 errors, 0 warnings
- Tests: 78/78 passed

---

## Phase 15 — Enrollment Rules Engine ✅ Complete
**Complexity:** Medium | **Dependencies:** `Enrollment`, `CourseOffering`, `Result` entities (all exist)

### Stage 15.1 — Prerequisite Validation ✅
- `CoursePrerequisite` entity + `IPrerequisiteRepository` + `PrerequisiteRepository` added.
- `EnrollmentService.TryEnrollAsync` checks all prerequisites; rejects with unmet list.
- `PrerequisiteController` (GET/POST/DELETE) exposes prerequisite CRUD API.
- Web portal: Prerequisites page (Admin/SuperAdmin) to view/add/remove prerequisites per course.

### Stage 15.2 — Timetable Clash Detection ✅
- `TryEnrollAsync` joins timetable entries for the requested and already-enrolled offerings; rejects on overlap.
- Admin `AdminEnrollRequest` supports `OverrideClash` + `OverrideReason`; override is audit-logged.

### Stage 15.3 — Course Capacity Limits ✅ Already Implemented
- `CourseOffering.MaxEnrollment` enforced by `EnrollmentService`; `UpdateMaxEnrollment` API action exists.

---

## Phase 16 — Faculty Grading System
**Complexity:** Medium | **Dependencies:** `Result`, `ResultComponentRule`, `Assignment`, `Quiz` entities (all exist)

### Stage 16.1 — Gradebook Grid View
- Faculty have a grid view per course offering: rows = enrolled students, columns = assessment components (assignments, quizzes, plus an exam/final column).
- Each cell shows the current mark and is inline-editable with auto-save.
- Totals column auto-computes the weighted final mark using `ResultComponentRule` weightings.
- New `GET /api/v1/gradebook/{offeringId}` endpoint returns the grid data.

### Stage 16.2 — Rubric-Based Grading
- Faculty can define a rubric for any assessment: `Rubric` entity → `RubricCriteria` rows (criterion name, max points) → `RubricLevel` rows (performance label, points awarded).
- When grading a submission, Faculty select a level per criterion; system sums to the total mark.
- Students can view the rubric breakdown and their awarded levels as part of feedback.

### Stage 16.3 — Bulk Grading via CSV
- Faculty can download a blank CSV template for a component (student ID + name columns pre-filled).
- Faculty upload the completed CSV; system validates IDs and mark ranges, then previews changes before applying.
- Bulk apply triggers the same result-update notifications as individual mark entry.

---

## Phase 17 — Degree Audit System
**Complexity:** Medium | **Dependencies:** `Course.CreditHours`, `Result`, `AcademicProgram` (all exist — partial foundation)

> **Partial foundation:** `Course.CreditHours` and `StudentProfile.Cgpa` already exist. `ResultCalculationService` computes GPA. The audit layer (credit aggregation, eligibility rules, elective/core classification) is new.

### Stage 17.1 — Credit Completion Tracking
- New `GET /api/v1/degree-audit/{studentProfileId}` endpoint aggregates total credits earned from passing `Result` records.
- Breaks down credits by Core vs Elective (requires Stage 17.3 course tagging).
- Student can view their own credit progress; Faculty advisor and Admin (dept-scoped) can view any student.

### Stage 17.2 — Graduation Eligibility Checker
- SuperAdmin defines `DegreeRule` per `AcademicProgram`: minimum total credits, minimum GPA, required core course list.
- System evaluates eligibility automatically against the student's current audit; exposes `IsEligible` flag and list of unmet requirements.
- Admin can view a filtered list of eligible vs ineligible students per department/program.

### Stage 17.3 — Elective vs Core Course Tagging
- Add `CourseType` enum (`Core` / `Elective`) to `Course` entity; Admin or SuperAdmin sets the value per course.
- Degree audit uses `CourseType` to validate minimum elective credit count alongside core requirements.
- Migration: add `CourseType` column to `courses` table (default `Core`).

---

## Phase 18 — Graduation Workflow
**Complexity:** Medium | **Dependencies:** Phase 17 (Degree Audit), existing `StudentLifecycleController`

> **Partial foundation:** `StudentLifecycleController.GraduateStudent()` (admin batch action) and `StudentProfile.GraduatedDate` already exist. This phase adds the student-initiated application and multi-stage approval workflow on top.

### Stage 18.1 — Graduation Application Flow
- Students who are degree-audit eligible can submit a `GraduationApplication` from the portal.
- Application enters a three-stage approval workflow: Faculty (verify results) → Admin (approve) → SuperAdmin (confirm).
- Each approver sees a pending-applications list and receives an in-app notification when an application reaches their stage.
- Application status: Draft → PendingFaculty → PendingAdmin → PendingFinalApproval → Approved / Rejected.

### Stage 18.2 — Certificate Generation
- On final SuperAdmin approval, system generates a graduation certificate PDF from a configurable HTML template (set by SuperAdmin in Portal Settings).
- PDF is stored against the student record; student can download it from the portal.
- Admin can re-issue or revoke a certificate with a documented reason — all actions are audit-logged.

---

## Phase 19 — Learning Management System (LMS)
**Complexity:** High | **Dependencies:** `CourseOffering`, `Enrollment`, Notification system (all exist)

> **Partial foundation for Stage 19.4:** `NotificationType.Announcement = 6` already exists in the notification enum. The announcement entity and dedicated portal page are new.

### Stage 19.1 — Structured Course Content
- New `CourseContentModule` entity: `OfferingId`, `Title`, `WeekNumber`, `Body` (rich text), `IsPublished`, ordering.
- Faculty create and order weekly module units per offering; publish/unpublish individually.
- Students enrolled in the offering can access published modules in order from the portal.
- SuperAdmin and Admin can enable/disable the LMS feature per portal instance via Module Settings.

### Stage 19.2 — Video-Based Teaching
- Faculty can attach video references (upload or embed URL) to a `CourseContentModule`.
- New `ContentVideo` entity: `ModuleId`, `Title`, `StorageUrl` or `EmbedUrl`, `DurationSeconds`.
- Student portal shows video player (upload) or embedded iframe (URL) within the module view.
- Storage limits and allowed formats are configurable by SuperAdmin in Portal Settings.

### Stage 19.3 — Discussion Forums
- New `DiscussionThread` entity per `CourseOffering`: `Title`, `AuthorId`, `IsPinned`, `IsClosed`.
- New `DiscussionReply` child entity: `ThreadId`, `AuthorId`, `Body`, `CreatedAt`.
- Faculty can pin, close, and delete threads; Students can create threads and reply.
- Notification dispatched to all participants in a thread when a new reply is posted.

### Stage 19.4 — Course Announcements
- Faculty can post course-level announcements (distinct from discussion threads) visible to all enrolled students.
- New `CourseAnnouncement` entity: `OfferingId`, `AuthorId`, `Title`, `Body`, `PostedAt`.
- Announcement triggers an in-app notification (uses existing `NotificationType.Announcement = 6`) to all enrolled students.
- Admin can post department-wide announcements targeting all students and faculty in their assigned departments.

---

## Phase 20 — Study Planner
**Complexity:** Medium | **Dependencies:** Phase 17 (Degree Audit for prerequisite/credit validation)

### Stage 20.1 — Semester Planning Tool
- Students can build a tentative plan for future semesters by selecting available courses.
- New `StudyPlan` entity: `StudentProfileId`, `PlannedSemesterName`; child `StudyPlanCourse` rows.
- Planner validates prerequisites (Phase 15) and credit-load limits before saving.
- Saved plans are visible to the student's assigned Faculty advisor.

### Stage 20.2 — Course Recommendation Engine
- System recommends courses for the next semester based on completed credits, degree audit gaps (Phase 17), and available offerings.
- Faculty advisors can endorse or modify recommendations before they surface to the student.
- SuperAdmin configures recommendation rules and credit-load weightings per `AcademicProgram`.

---

## Phase 21 — External Integrations
**Complexity:** High | **Dependencies:** None (configurable by SuperAdmin)

> **Partial foundation for Stage 21.2:** The Report Center already exports CSV/PDF for operational reports. Accreditation-specific templates and regulatory format handling are new.

### Stage 21.1 — Library System Integration
- SuperAdmin configures an external library catalogue URL and optional auth token in Portal Settings.
- Portal embeds or links the library catalogue within a dedicated Library portal page.
- Loan status and due dates are surfaced on the student dashboard via a configurable library API endpoint.

### Stage 21.2 — Government / Accreditation Reporting
- SuperAdmin can define named accreditation report templates (enrollment counts, completion rates, demographic summaries) with configurable field mappings.
- Reports are generated on-demand as CSV or PDF in the required regulatory format.
- All accreditation export events are written to the audit log with user, timestamp, and template name.

---

## Implementation Sequence Summary

| Phase | Feature | Complexity | Status |
|---|---|---|---|
| 12 | Academic Calendar (timelines + deadlines) | Low–Medium | Planned |
| 13 | Global Search | Low | Planned |
| 14 | Helpdesk / Support Ticketing | Low–Medium | Planned |
| 15 | Enrollment Rules Engine | Medium | Planned (Stage 15.3 ✅ done) |
| 16 | Faculty Grading System (gradebook, rubrics, bulk CSV) | Medium | Planned |
| 17 | Degree Audit System | Medium | Planned (partial foundation) |
| 18 | Graduation Workflow (application + certificate) | Medium | Planned (partial foundation) |
| 19 | Learning Management System | High | Planned (Stage 19.4 partial) |
| 20 | Study Planner | Medium | Planned |
| 21 | External Integrations | High | Planned (Stage 21.2 partial) |
