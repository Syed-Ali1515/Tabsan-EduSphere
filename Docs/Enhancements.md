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

## Phase 19 — Advanced Course Creation & Result Configuration
**Complexity:** Medium–High | **Dependencies:** `Course`, `AcademicProgram`, `Semester`, `Result`, `ResultComponentRule` (all exist); graduation trigger introduced in Phase 18

> **Objective:** Extend the course creation flow and result calculation system to natively distinguish semester-based degree programs from short-duration non-semester courses. Introduce auto-semester generation, per-course grading configuration, and smart course filtering in the result calculation interface. This phase stabilises the `Course` entity before the LMS (Phase 20) and Study Planner (Phase 21) build on top of it.

### Stage 19.1 — Semester-Based Course Type Flag & Auto-Semester Generation
- Add `HasSemesters` (`bool`, default `true`) and `TotalSemesters` (`int?`) columns to the `courses` table via EF migration.
- Course creation form gains a **"This course has semesters"** checkbox.
  - When checked (semester-based): show a **Number of Semesters** input (e.g. 2, 4, 6, 8).
  - When unchecked (non-semester): hide semester count and show Stage 19.2 fields instead.
- On save of a semester-based course, the system automatically creates `TotalSemesters` `Semester` rows (Semester 1 … Semester N) linked to the course's `AcademicProgram`.
- New `CourseService.AutoCreateSemestersAsync(courseId, count)` orchestrates the batch creation.
- After all semester results are published and passing, the Phase 18 graduation trigger (`StudentLifecycleController.GraduateStudent`) is invoked automatically — no manual step required.
- **Files:** `Course.cs` (domain), `AcademicConfigurations.cs` (EF config), migration `Phase19_CourseTypeAndGrading`, `ICourseService.cs` / `CourseService.cs`, `CourseController.cs`, `Courses.cshtml` (portal), `PortalViewModels.cs`, `EduApiClient.cs`

### Stage 19.2 — Non-Semester (Short-Duration) Course Support
- When `HasSemesters = false`, course creation shows:
  - **Duration** numeric input (e.g. `6`).
  - **Duration Unit** dropdown (`Weeks` / `Months` / `Years`).
- New columns on `courses` table: `DurationValue` (`int?`), `DurationUnit` (`nvarchar(20)?`).
- No `Semester` rows are created for non-semester courses.
- Non-semester courses are treated as a single-block program throughout the system (enrollment, attendance, result calculation).
- Course creation form also exposes a **Grading Type** dropdown (values: `GPA`, `Percentage`, `Grade`) stored as `GradingType` (`nvarchar(20)`) on the `courses` table.
- **Files:** same as Stage 19.1 (same migration, same service/controller/view)

### Stage 19.3 — Result Calculation Dual Dropdown & Course Search
- Result calculation page (Admin/Faculty) gains a **two-level course filter**:
  1. **Course Type dropdown** — `Semester-Based` / `Non-Semester-Based`.
  2. **Course dropdown** — dynamically populated to show only courses matching the selected type; uses `HasSemesters` flag.
- A **search box** above the course list allows quick text filtering by course name (client-side JS or lightweight AJAX).
- Selecting a course loads the result calculation interface specific to that course's grading type (GPA / Percentage / Grade).
- New API query parameter: `GET /api/v1/course?hasSemesters={true|false}` on the existing `CourseController.GetAll` to support the filtered dropdown.
- **Files:** `CourseController.cs` (filter param), `Results.cshtml` / result portal page, `PortalController.cs`, `EduApiClient.cs`

### Stage 19.4 — Per-Course Grading Configuration (SuperAdmin)
- SuperAdmin can define a **grading configuration** per course (not global):
  - **Pass threshold** — minimum mark or GPA to pass.
  - **Grade ranges** — mapping of mark ranges to letter grades (e.g. 90–100 → A+, 80–89 → A, …).
  - **Evaluation method** — which component rules (assignments/quizzes/exams) contribute and at what weightage (leverages existing `ResultComponentRule`).
- New `CourseGradingConfig` entity: `CourseId` (unique), `PassThreshold` (`decimal`), `GradingType` (from Stage 19.2), `GradeRangesJson` (`nvarchar(max)` — serialised range list).
- New `ICourseGradingRepository` + `CourseGradingRepository` and `ICourseGradingService` + `CourseGradingService`.
- New `GradingConfigController` with endpoints: `GET /api/v1/grading-config/{courseId}`, `PUT /api/v1/grading-config/{courseId}` (SuperAdmin only).
- Portal page **GradingConfig.cshtml** (SuperAdmin only): grade-range builder UI (add/remove rows with mark-from, mark-to, grade label), pass-threshold input.
- Grade ranges are applied by `ResultCalculationService` when publishing results for a course.
- **Files:** `CourseGradingConfig.cs` (domain), `AcademicConfigurations.cs`, migration `Phase19_CourseTypeAndGrading`, `ICourseGradingRepository.cs`, `CourseGradingRepository.cs`, `ICourseGradingService.cs`, `CourseGradingService.cs`, `GradingConfigController.cs`, `GradingConfigDTOs.cs`, `GradingConfig.cshtml`, `PortalViewModels.cs`, `EduApiClient.cs`, `PortalController.cs`, `_Layout.cshtml` (sidebar link)

---

## Phase 20 — Learning Management System (LMS) ✅ Implemented (commit `ecf4d91` — 2026-05-08)
**Complexity:** High | **Dependencies:** `CourseOffering`, `Enrollment`, Notification system (all exist); benefits from stable `Course` structure introduced in Phase 19

> **Partial foundation for Stage 20.4:** `NotificationType.Announcement = 6` already exists in the notification enum. The announcement entity and dedicated portal page are new.

### Stage 20.1 — Structured Course Content ✅
- `CourseContentModule` entity: `OfferingId`, `Title`, `WeekNumber`, `Body` (50 000 char), `IsPublished`, ordering.
- Faculty create/order weekly module units per offering; publish/unpublish individually.
- Students enrolled see published modules in order; faculty see all (published + draft).
- `ILmsRepository` + `LmsRepository`; `ILmsService` + `LmsService`; `LmsController` (`api/v1/lms`).
- Portal views: `CourseLms.cshtml` (student), `LmsManage.cshtml` (faculty).

### Stage 20.2 — Video-Based Teaching ✅
- `ContentVideo` entity: `ModuleId`, `Title`, `StorageUrl`, `EmbedUrl`, `DurationSeconds`.
- Faculty attach video references to modules; add/delete via `LmsController`.
- EF: `LmsConfigurations.cs` — table configs + soft-delete query filters for both entities.
- `LmsRepository`: `GetModulesByOfferingAsync` includes Videos; `GetModuleByIdAsync` includes Videos.

### Stage 20.3 — Discussion Forums ✅
- `DiscussionThread` entity per `CourseOffering`: `Title`, `AuthorId`, `IsPinned`, `IsClosed`.
- `DiscussionReply` child entity: `ThreadId`, `AuthorId`, `Body`.
- Faculty pin, close, reopen, delete threads; all participants create threads and reply.
- `IDiscussionRepository` + `DiscussionRepository`; `IDiscussionService` + `DiscussionService`; `DiscussionController` (`api/v1/discussion`).
- Portal views: `Discussion.cshtml` (thread list), `DiscussionThread.cshtml` (detail + replies).
- Author names resolved via `IUserRepository.GetByIdAsync` → `Username`.

### Stage 20.4 — Course Announcements ✅
- `CourseAnnouncement` entity: `OfferingId` (nullable), `AuthorId`, `Title`, `Body`, `PostedAt`.
- On creation, fan-out notification dispatched to all active enrolled students (`NotificationType.Announcement = 6`).
- `IAnnouncementRepository` + `AnnouncementRepository`; `IAnnouncementService` + `AnnouncementService`; `AnnouncementController` (`api/v1/announcement`).
- Portal view: `Announcements.cshtml` — create form + announcement cards with delete.
- Sidebar entries added: `lms_manage`, `discussion`, `announcements` (group: Academic Related).

**Validation:** 0 build errors · 7/7 unit tests passed · migration `Phase20_LMS` applied

---

## Phase 21 — Study Planner ✅ Implemented
**Complexity:** Medium | **Dependencies:** Phase 17 ✅ (Degree Audit), Phase 15 ✅ (Prerequisites); benefits from `HasSemesters` flag introduced in Phase 19

### Stage 21.1 — Semester Planning Tool ✅
- `StudyPlan` entity: `StudentProfileId`, `PlannedSemesterName`, `Notes`, `AdvisorStatus (Pending/Endorsed/Rejected)`, `AdvisorNotes`, `ReviewedByUserId`.
- `StudyPlanCourse` child entity: `StudyPlanId`, `CourseId`; unique constraint per plan+course.
- Service validates: course `HasSemesters == true` and `IsActive`; all prerequisites passed; credit load ≤ `AcademicProgram.MaxCreditLoadPerSemester` (default 18).
- `AcademicProgram.MaxCreditLoadPerSemester` property added + `SetMaxCreditLoad()` method; EF config updated.
- `IStudyPlanRepository` + `StudyPlanRepository`; `IStudyPlanService` + `StudyPlanService`.
- `StudyPlanController` (`api/v1/study-plan`): CRUD plans, add/remove courses, advise endpoint.
- Faculty advisors can endorse or reject plans with notes (advisor workflow).
- Portal views: `StudyPlan.cshtml` (list), `StudyPlanDetail.cshtml` (detail + course management + advisor panel).
- Sidebar: `study_plan` → `(Portal, StudyPlan)` group: Student Related.

### Stage 21.2 — Course Recommendation Engine ✅
- `GetRecommendationsAsync`: fetches earned credits → degree rule required course gaps → department `HasSemesters=true` courses → prerequisite-gated candidates → credits-limited recommendation list with reasons.
- Required courses flagged "Required by your degree plan"; electives flagged "Elective available in your department".
- `StudyPlanRecommendations.cshtml` portal view with semester-picker form.
- API endpoint: `GET api/v1/study-plan/recommendations/{studentProfileId}?plannedSemesterName=...`.

**Validation:** 0 build errors · 7/7 unit tests passed · migration `Phase21_StudyPlanner` applied
- SuperAdmin configures recommendation rules and credit-load weightings per `AcademicProgram`.

---

## Phase 22 — External Integrations
**Complexity:** High | **Dependencies:** None (configurable by SuperAdmin); fully standalone phase

> **Partial foundation for Stage 22.2:** The Report Center already exports CSV/PDF for operational reports. Accreditation-specific templates and regulatory format handling are new.

### Stage 22.1 — Library System Integration
- SuperAdmin configures an external library catalogue URL and optional auth token in Portal Settings.
- Portal embeds or links the library catalogue within a dedicated Library portal page.
- Loan status and due dates are surfaced on the student dashboard via a configurable library API endpoint.

### Stage 22.2 — Government / Accreditation Reporting
- SuperAdmin can define named accreditation report templates (enrollment counts, completion rates, demographic summaries) with configurable field mappings.
- Reports are generated on-demand as CSV or PDF in the required regulatory format.
- All accreditation export events are written to the audit log with user, timestamp, and template name.

---

## Phase 22 — External Integrations ✅ Implemented (commit `dddee69` — 2026-05-08)
**Complexity:** High | **Dependencies:** None (SuperAdmin-configured); standalone phase

### Stage 22.1 — Library System Integration ✅
- SuperAdmin configures library catalogue URL + optional API token via `PUT /api/v1/library/config`.
- `LibraryConfig` stored in `portal_settings` under the `library_` key prefix.
- `GET /api/v1/library/loans` proxies request to external library API using calling user's username as identifier.
- `GET /api/v1/library/loans/{studentIdentifier}` — Admin/SuperAdmin can look up any student's loans.
- Portal view: `LibraryConfig.cshtml` (SuperAdmin) with catalogue URL + token inputs; sidebar entry `library_config` (group: Settings).

### Stage 22.2 — Government / Accreditation Reporting ✅
- `AccreditationTemplate` entity: `Name`, `Description`, `FieldMappingsJson`, `Format` (CSV/PDF), `IsActive`.
- CRUD: `GET/POST/PUT/DELETE /api/v1/accreditation/{id}` — template management SuperAdmin-only.
- `GET /api/v1/accreditation/{id}/generate` — Admin/SuperAdmin; generates and streams report file; writes to audit log.
- `AccreditationService.GenerateAsync` serialises template field mappings, pulls live data from existing aggregations, formats as CSV or plain-text PDF.
- Portal view: `AccreditationTemplates.cshtml` (SuperAdmin/Admin) — template list with generate buttons; sidebar entry `accreditation` (group: Settings).
- EF Migration `Phase22_ExternalIntegrations` — adds `accreditation_templates` table.

**Validation:** 0 build errors · no new unit tests (all integration-tested via existing suite) · migration `Phase22_ExternalIntegrations` applied

---

## Phase 23 — Core Policy Foundation ✅ Implemented (commit `28cac36` — 2026-05-09)
**Complexity:** Medium | **Dependencies:** `portal_settings` (exists); `ISettingsRepository` (exists)

### Stage 23.1 — License Policy Kernel ✅
- `InstitutionType` enum: `University = 0` (default, backward-compatible), `School = 1`, `College = 2` — in `Domain/Enums/`.
- `InstitutionPolicySnapshot` sealed record: `IncludeSchool`, `IncludeCollege`, `IncludeUniversity`; `IsEnabled(InstitutionType)` method; static `Default` = University-only.
- `IInstitutionPolicyService` — `GetPolicyAsync`, `SavePolicyAsync`, `InvalidateCache`; values in `portal_settings` with 10-minute `IMemoryCache` backing.
- `InstitutionPolicyService` implementation; `Microsoft.Extensions.Caching.Memory 8.0.1` added to Application project.
- `InstitutionPolicyController` — `GET /api/v1/institution-policy` (all authenticated) + `PUT /api/v1/institution-policy` (SuperAdmin only).

### Stage 23.2 — Institution Context Resolution ✅
- `InstitutionContextMiddleware` — resolves `IInstitutionPolicyService` per-request, stores snapshot in `HttpContext.Items["InstitutionPolicy"]`.
- Extension method `context.GetInstitutionPolicy()` — returns `InstitutionPolicySnapshot.Default` when not set; used by downstream controllers/services.
- Registered after `UseAuthorization` in `Program.cs`.

### Stage 23.3 — Role-Rights Hardening ✅
- `GET /api/v1/institution-policy` read by all authenticated roles; PUT restricted to SuperAdmin.
- Web: `PortalController.InstitutionPolicy` GET action; `InstitutionPolicy.cshtml` SuperAdmin config page.
- Seed: sidebar module `institution_policy` (sort 33, SuperAdmin).

**Validation:** 0 build errors · 27/27 unit tests passed · no migration needed

---

## Phase 24 — Dynamic Module and UI Composition ✅ Implemented (commit `391ac45` — 2026-05-09)
**Complexity:** Medium | **Dependencies:** Phase 23 (`InstitutionPolicySnapshot`); `IModuleEntitlementResolver` (Application); `IModuleService` (Application)

### Stage 24.1 — Module Registry ✅
- `ModuleDescriptor` sealed record in `Domain/Modules/`: `Key`, `RequiredRoles[]`, `AllowedTypes[]?`, `IsLicenseGated`; `RoleMatches()` + `TypeMatches()` methods.
- `ModuleRegistry` static class in `Application/Modules/`: catalogue of all 14 module descriptors (e.g. `fyp` = University-only, `ai_chat` = license-gated, `advanced_audit` = SuperAdmin-only).
- `IModuleRegistryService` + `ModuleRegistryService` — combines registry with live activation (`IModuleEntitlementResolver`) + institution policy to produce `ModuleVisibilityResult(Key, Name, IsActive, IsAccessible)` list.
- `ModuleRegistryController` — `GET api/v1/module-registry/visible` (all authenticated).

### Stage 24.2 — Dynamic Labels ✅
- `AcademicVocabulary` sealed record: `PeriodLabel`, `ProgressionLabel`, `GradingLabel`, `CourseLabel`, `StudentGroupLabel`; static `Default` = University vocab.
- `ILabelService` / `LabelService` (singleton) — returns institution-mode-appropriate vocabulary (University: Semester/GPA/Course/Batch; School: Grade/Percentage/Subject/Class; College: Year/Percentage/Subject/Year-Group).
- `LabelController` — `GET api/v1/labels` (all authenticated).

### Stage 24.3 — Dashboard Composition ✅
- `WidgetDescriptor` sealed record: `Key`, `Title`, `Icon`, `Order`.
- `IDashboardCompositionService` / `DashboardCompositionService` (singleton) — 10-widget catalogue filtered by role + institution type (`fyp_panel` University-only; `system_health` SuperAdmin-only; `ai_assistant` all roles).
- `DashboardCompositionController` — `GET api/v1/dashboard/composition` (all authenticated).
- Web: `ModuleComposition.cshtml` SuperAdmin page showing vocabulary tiles, widget cards, and full module registry table.
- Seed: sidebar module `module_composition` (sort 34, SuperAdmin).

**Validation:** 0 build errors · 44/44 unit tests passed (17 new Phase 24 tests) · no migration needed

---

## Phase 25 — Academic Engine Unification ✅ (commit `d2aabd3`, 2026-05-09)

### Stage 25.1 — Result Calculation Strategy Pattern ✅
- `IResultCalculationStrategy` interface: `AppliesTo`, `Calculate(marks, gpaRules, threshold, gradeRangesJson)` → `ResultSummary`.
- Value types: `ComponentMark`, `ResultSummary`, `GpaScaleRuleEntry`, `GradeBandEntry`.
- `GpaResultStrategy` (University): weighted percentage → GPA lookup via configured scale; pass = GPA ≥ threshold.
- `PercentageResultStrategy` (School/College): weighted percentage → grade band resolution (custom JSON or built-in A+/A/B+/B/C/D/F defaults); pass = % ≥ threshold. Throws if instantiated for University.
- `IResultStrategyResolver` / `ResultStrategyResolver` (singleton): maps `InstitutionType` → strategy. Existing `ResultService` unchanged (University GPA flow unaffected).

### Stage 25.2 — Institution Grading Profiles ✅
- `InstitutionGradingProfile` domain entity: `InstitutionType`, `PassThreshold`, `GradeRangesJson`, `IsActive`. One profile per type (unique index).
- Threshold validation: University 0–4.0, School/College 0–100.
- `IInstitutionGradingProfileRepository` + `InstitutionGradingProfileRepository` (EF).
- `IInstitutionGradingService` / `InstitutionGradingService`: `GetAllAsync`, `GetByTypeAsync`, `UpsertAsync` (create-or-update).
- DTOs: `InstitutionGradingProfileDto`, `SaveInstitutionGradingProfileRequest`.
- `InstitutionGradingProfileController`: `GET /api/v1/institution-grading-profiles` (Admin+), `GET /{type}` (Admin+), `PUT /{type}` (SuperAdmin only).
- EF config (`institution_grading_profiles`) + migration `20260508152906_Phase25_AcademicEngineUnification`.

### Stage 25.3 — Progression / Promotion Logic ✅
- `IProgressionService` / `ProgressionService`: institution-type-aware evaluation of student progression eligibility.
  - University: CGPA ≥ pass threshold.
  - School: `CurrentSemesterGpa` (treated as %) ≥ pass threshold.
  - College: `CurrentSemesterGpa` (treated as %) ≥ pass threshold; labels expressed as "Year N".
- Defaults when no profile configured: 2.0 (University), 40 (School/College).
- `EvaluateAsync`: returns `ProgressionDecision` with no side effects.
- `PromoteAsync`: calls evaluate then calls `student.AdvanceSemester()` if eligible; throws `InvalidOperationException` otherwise.
- `ProgressionController`: `POST /evaluate` (Admin+), `POST /promote` (Admin+), `GET /me/{type}` (Student+).
- DTOs: `ProgressionDecision`, `ProgressionEvaluationRequest`.

**Validation:** 0 build errors · 144/144 unit tests passed (29 new Phase 25 tests: strategy, resolver, entity, progression service)

---

## Phase 26 — School and College Functional Expansion ✅ (commit `4c0904c`, 2026-05-09)

### Stage 26.1 — School Streams and Subject Mapping ✅
- Domain entities: `SchoolStream`, `StudentStreamAssignment`.
- Service/API: `ISchoolStreamService` + `SchoolStreamService`; `SchoolStreamController` (`GET`, `PUT`, `POST assign`, `GET student/{id}`).
- Persistence: `ISchoolStreamRepository` + `SchoolStreamRepository`; EF configs for `school_streams` and `student_stream_assignments`.
- Constraints: one active stream assignment per student (`IX_student_stream_assignments_student`), unique stream names (`IX_school_streams_name`).

### Stage 26.2 — School/College Report Cards and Promotion Operations ✅
- Domain entities: `StudentReportCard`, `BulkPromotionBatch`, `BulkPromotionEntry` with enums `BulkPromotionStatus`, `EntryDecision`.
- Services/APIs:
  - `IReportCardService` + `ReportCardService`; `ReportCardController` (`generate`, `latest`, `history`).
  - `IBulkPromotionService` + `BulkPromotionService`; `BulkPromotionController` (`batch`, `entries`, `submit`, `review`, `apply`, `get`).
- Approval safeguards: draft → awaiting approval → approved/rejected → applied workflow; apply allowed only after approval.
- Promotion behavior: only `Promote` entries call `student.AdvanceSemester()`; `Hold` entries remain unchanged.

### Stage 26.3 — Parent-Facing Read Model ✅
- Domain entity: `ParentStudentLink`.
- Service/API: `IParentPortalService` + `ParentPortalService`; `ParentPortalController` (`GET api/v1/parent-portal/me/students`).
- Scope enforcement: returns only active links and only linked students found by repository lookup.

### Infrastructure and Validation
- Migration: `20260509044437_Phase26_SchoolCollegeExpansion`.
- New tables: `school_streams`, `student_stream_assignments`, `student_report_cards`, `bulk_promotion_batches`, `bulk_promotion_entries`, `parent_student_links`.
- Tests: `Phase26Tests.cs` added; total suite now **152/152 passed**.
- Validation: 0 build errors · 152/152 tests passed.

---

## Implementation Sequence Summary

| Phase | Feature | Complexity | Status |
|---|---|---|---|
| 12 | Academic Calendar (timelines + deadlines) | Low–Medium | ✅ Implemented |
| 13 | Global Search | Low | ✅ Implemented |
| 14 | Helpdesk / Support Ticketing | Low–Medium | ✅ Implemented |
| 15 | Enrollment Rules Engine | Medium | ✅ Implemented |
| 16 | Faculty Grading System (gradebook, rubrics, bulk CSV) | Medium | ✅ Implemented |
| 17 | Degree Audit System | Medium | ✅ Implemented |
| 18 | Graduation Workflow (application + certificate) | Medium | ✅ Implemented |
| 19 | Advanced Course Creation & Result Configuration | Medium–High | ✅ Implemented |
| 20 | Learning Management System | High | ✅ Implemented (commit `ecf4d91`) |
| 21 | Study Planner | Medium | ✅ Implemented |
| 22 | External Integrations | High | ✅ Implemented (commit `dddee69`) |
| 23 | Core Policy Foundation | Medium | ✅ Implemented (commit `28cac36`) |
| 24 | Dynamic Module and UI Composition | Medium | ✅ Implemented (commit `391ac45`) |
| 25 | Academic Engine Unification | High | ✅ Implemented (commit `d2aabd3`) |
| 26 | School and College Functional Expansion | High | ✅ Implemented (commit `4c0904c`) |
| 27 | University Portal Parity and Student Experience | High | Planned |
