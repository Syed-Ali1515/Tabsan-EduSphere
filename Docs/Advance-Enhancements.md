# Advance Enhancements — EduSphere Competitive Upgrade Roadmap

**Source documents:** `EduSphere_Competitive_Roadmap.txt`, `EduSphere_Million_User_Scalability_Guide.txt`, `EduSphere_MSSQL_Indexing_Strategy.txt`, `New Enhancements Guide.docx`, `University_Portal_PRD.docx`  
**Scope:** Phases 23 onward — builds on top of the completed Phases 12–21 and the planned Phase 22.  
**Ordering principle:** Each phase is ordered so that foundational changes come before dependent features. No phase requires touching files that a later phase will also modify for the same purpose.  
**Status:** All phases are **Planned — Not Started** unless noted.

---

## Role-Rights Matrix (Preserved Throughout All Phases)

The following rights apply globally and must never be reduced by any enhancement phase.

| Action | SuperAdmin | Admin | Faculty | Student |
|--------|-----------|-------|---------|---------|
| System configuration, license, institution type | ✅ Full | ❌ | ❌ | ❌ |
| Add / edit / deactivate anything system-wide | ✅ Full | ❌ | ❌ | ❌ |
| Manage institutes, departments, programs, courses | ✅ Full | ✅ Own institution | ❌ | ❌ |
| Manage degrees, grading configs, degree rules | ✅ Full | ✅ Own institution | ❌ | ❌ |
| Manage students (add / edit / deactivate) | ✅ Full | ✅ Own dept | ❌ | ❌ |
| Manage faculty (add / edit / deactivate) | ✅ Full | ✅ Own dept | ❌ | ❌ |
| Enter / update marks, grades, attendance | ✅ Full | ✅ Full | ✅ Own offerings | ❌ |
| View results | ✅ All | ✅ Own dept | ✅ Own offerings | ✅ Own only |
| Generate / export reports | ✅ All | ✅ Own institution | ✅ Own offerings | ❌ |
| Manage license and subscription | ✅ Full | ❌ | ❌ | ❌ |
| Branding, tenant configuration | ✅ Full | ❌ | ❌ | ❌ |
| View own academic data (results, plans, attendance) | ✅ | ✅ | ✅ | ✅ Own |

---

## Phase 23 — Multi-Institution Type Foundation

**Complexity:** High | **Dependencies:** Phase 19 ✅ (`HasSemesters`), existing `License` infrastructure  
**Must be implemented first** — all subsequent phases (24–27) build on top of the institution type concept introduced here.

> **Objective:** Introduce a configuration-driven institution type system (School / College / University) so the platform can serve all three education tiers from a single codebase without duplicating logic. Institution type controls academic structure labels, result calculation mode, and promotion rules throughout the system.

### Stage 23.1 — Institution Type System Setting

**What changes:**
- Add `InstitutionType` enum: `University = 0 (default)`, `School = 1`, `College = 2`.
- Add `SystemSetting.InstitutionType` (stored in existing settings infrastructure) — SuperAdmin sets this once during initial configuration.
- Create `IInstitutionTypeService` with `GetCurrentTypeAsync()` cached in memory after first load.
- All API controllers call `IInstitutionTypeService` to determine active mode before applying business logic.

**Role rights:**
- **SuperAdmin only:** read and write institution type.
- All other roles: read-only (used to filter UI).

**Files (new/modified):**
- `Domain/Enums/InstitutionType.cs` (new enum)
- `Application/Interfaces/IInstitutionTypeService.cs` (new)
- `Application/Services/InstitutionTypeService.cs` (new)
- `Infrastructure/Repositories/SystemSettingRepository.cs` (add getter/setter)
- `API/Program.cs` (DI registration)
- `API/Controllers/SystemSettingController.cs` (new endpoint: GET/PUT `/api/v1/system/institution-type`)
- `Web/Controllers/PortalController.cs` (read type for UI)

---

### Stage 23.2 — Academic Structure Label Adaptation

**What changes:**
- Introduce `ILabelService` that returns context-appropriate labels based on `InstitutionType`:
  - `School` → "Grade" (instead of "Semester"), "Subject" (instead of "Course"), "Percentage" (instead of "GPA")
  - `College` → "Year", "Subject", "Percentage"
  - `University` → "Semester", "Course", "GPA / CGPA" (current behaviour — no change)
- Portal views read labels from a shared `ViewBag.Labels` dictionary populated by a base controller action filter.
- No database changes — purely a presentation and service layer change.

**Role rights:** All roles benefit; no role restriction on label display.

**Files (new/modified):**
- `Application/Interfaces/ILabelService.cs` (new)
- `Application/Services/LabelService.cs` (new)
- `Web/Filters/InstitutionLabelFilter.cs` (action filter that sets ViewBag.Labels)
- `Web/Controllers/PortalController.cs` (apply filter)
- Key portal views updated to use `ViewBag.Labels["Semester"]` instead of hard-coded "Semester"

---

### Stage 23.3 — Dashboard Filtering by Institution Type

**What changes:**
- Student dashboard (`Dashboard.cshtml`) renders different widgets based on institution type:
  - **School:** Class/Grade widget, percentage summary, promotion status.
  - **College:** Year-of-study widget, percentage summary.
  - **University:** Semester widget, GPA, CGPA (current behaviour).
- Faculty and Admin dashboards similarly hide irrelevant metrics.
- Backend: `IDashboardService` gains `GetStudentDashboardAsync(studentId, institutionType)` overload.

**Role rights:**
- Each role sees only their own data filtered by institution type.
- Admin/SuperAdmin see aggregate summaries for their institution type.

**Files (new/modified):**
- `Application/Interfaces/IDashboardService.cs` (extend)
- `Application/Services/DashboardService.cs` (new overloads)
- `Web/Views/Portal/Dashboard.cshtml` (conditional Razor blocks per institution type)
- `Web/Controllers/PortalController.cs` (pass institution type to view)

---

## Phase 24 — License-Driven Module Control

**Complexity:** Medium | **Dependencies:** Phase 23 ✅ (institution type exists)

> **Objective:** Upgrade the existing license system to carry `IncludeSchool`, `IncludeCollege`, `IncludeUniversity` flags. The system activates only licensed institution type modules and hides/disables all others at both UI and API level.

### Stage 24.1 — License Flag Extension

**What changes:**
- Extend the license key / license file structure with three boolean fields:
  - `IncludeSchool` (bool)
  - `IncludeCollege` (bool)
  - `IncludeUniversity` (bool)
- `LicenseCheckWorker` validates these flags on startup and stores them in `LicenseService`.
- At least one flag must be `true` — license activation is rejected if all are `false`.
- Existing `LicenseService` and `LicenseCheckWorker` are extended; no rebuild of the licensing core.

**Role rights:**
- **SuperAdmin only:** manage and view license flags.
- All other roles: license flags silently filter what they see.

**Files (new/modified):**
- `tools/KeyGen/` — key generation updated to include three flags in license payload
- `tools/Tabsan.Lic/` — license parser updated
- `BackgroundJobs/LicenseCheckWorker.cs` (validate flags)
- `Application/Interfaces/ILicenseService.cs` (add flag accessors)
- `Application/Services/LicenseService.cs` (expose `IncludeSchool`, `IncludeCollege`, `IncludeUniversity`)

---

### Stage 24.2 — API-Level Module Enforcement

**What changes:**
- Create `[RequiresInstitutionType(InstitutionType.School)]` and similar custom authorization attributes.
- API controllers for school-specific and college-specific endpoints (introduced in Phase 25/26) are decorated with these attributes.
- `ILicenseService.IsModuleEnabled(InstitutionType)` returns `false` → HTTP 403 response.
- Existing University controllers are unchanged (they are the default and always available if `IncludeUniversity = true`).

**Role rights:** All roles are subject to module enforcement; SuperAdmin can override at license level only.

**Files (new/modified):**
- `API/Attributes/RequiresInstitutionTypeAttribute.cs` (new custom attribute)
- `API/Middleware/InstitutionTypeMiddleware.cs` (registers enforcement globally)
- `API/Program.cs` (register middleware)

---

### Stage 24.3 — UI-Level Module Filtering

**What changes:**
- Sidebar menu (`_Layout.cshtml`) checks `ILicenseService` flags before rendering each group.
- School-only groups (e.g., "Streams", "Grade Promotion") are hidden when `IncludeSchool = false`.
- University-only groups (e.g., "GPA / CGPA", "Study Planner") are hidden when `IncludeUniversity = false`.
- Admin portal settings page shows current license flags (read-only for Admin, editable for SuperAdmin via license re-import).

**Role rights:** All roles see only modules allowed by their institution's license. No role can manually override.

**Files (new/modified):**
- `Web/Views/Shared/_Layout.cshtml` (wrap sidebar groups in license-flag conditionals)
- `Web/Services/EduApiClient.cs` (add `GetLicenseFlagsAsync`)
- `Web/Controllers/PortalController.cs` (pass flags to layout via ViewBag)

---

## Phase 25 — School Academic System

**Complexity:** High | **Dependencies:** Phase 23 ✅, Phase 24 ✅ (license flags), Phase 19 ✅ (`HasSemesters`)  
**Only active when `IncludeSchool = true`.**

> **Objective:** Implement a full school-grade academic system (Grades 1–12, streams for Grades 9–12, percentage-based results, and end-of-year promotion) that reuses the existing EF schema where possible.

### Stage 25.1 — Grade / Class Structure

**What changes:**
- Add `SchoolGrade` entity: `GradeNumber` (1–12), `GradeName` (e.g. "Grade 1"), `AcademicYearId`, `IsStreamBased` (true for Grades 9–12).
- The existing `Semester` concept maps to `SchoolGrade` when `InstitutionType = School` (same table, different label via `ILabelService`).
- Admin creates and manages grades (same as creating semesters for university courses but labelled "Grade").
- Existing `CourseOffering` records link to a grade the same way they link to a semester.

**Role rights:**
- **SuperAdmin / Admin:** create, edit, deactivate grades.
- **Faculty / Student:** read-only (see their assigned grade).

**Files (new/modified):**
- `Domain/Academic/SchoolGrade.cs` (new entity — thin wrapper, maps to existing structure)
- `Infrastructure/Persistence/Configurations/SchoolConfigurations.cs` (new)
- `Infrastructure/Migrations/` (new migration `Phase25_SchoolSystem`)
- `API/Controllers/SchoolGradeController.cs` (new — CRUD)
- `Web/Views/Portal/SchoolGrades.cshtml` (Admin page)

---

### Stage 25.2 — Stream Selection for Grades 9–12

**What changes:**
- Add `Stream` entity: `Name` (Science / Biology / Computer / Commerce / Arts), `GradeNumber`, `IsActive`.
- `StudentProfile` gains nullable `StreamId` for school students in Grades 9–12.
- Course (subject) CRUD gains a `StreamId` association — subjects visible only to students in the matching stream.
- Admin assigns streams to students in Grades 9–12; Faculty sees only subjects for their stream.

**Role rights:**
- **SuperAdmin / Admin:** create streams, assign subjects to streams, assign stream to student.
- **Faculty:** view and teach subjects in their assigned stream.
- **Student:** view only subjects and results for their stream.

**Files (new/modified):**
- `Domain/Academic/Stream.cs` (new entity)
- `Domain/Academic/StudentProfile.cs` (add `StreamId`)
- `Infrastructure/Persistence/Configurations/SchoolConfigurations.cs` (add stream config)
- `Infrastructure/Migrations/` (included in `Phase25_SchoolSystem`)
- `API/Controllers/StreamController.cs` (new — CRUD for streams)
- `Web/Views/Portal/Streams.cshtml` (Admin page)
- `Web/Views/Portal/Students.cshtml` (add stream selector for Grade 9–12 students)

---

### Stage 25.3 — Percentage-Based Result Calculation for Schools

**What changes:**
- When `InstitutionType = School`, `ResultCalculationService` computes:
  - `TotalMarks` = sum of all subject maximum marks.
  - `ObtainedMarks` = sum of student's obtained marks.
  - `Percentage = (ObtainedMarks / TotalMarks) × 100`.
  - `LetterGrade` = derived from `CourseGradingConfig.GradeRangesJson` (Phase 19).
- New `SchoolResultDto` returned by `ResultController` with `Percentage` and `LetterGrade` fields.
- Faculty enter marks per subject per student the same way as university (existing mark-entry UI re-used).
- GPA calculation is skipped entirely when `InstitutionType = School`.

**Role rights:**
- **Faculty:** enter marks for their subjects.
- **Admin / SuperAdmin:** view all marks, publish results, override marks.
- **Student:** view their own published result (percentage + letter grade).

**Files (new/modified):**
- `Application/Services/ResultCalculationService.cs` (add school percentage branch)
- `Application/DTOs/Academic/SchoolResultDto.cs` (new)
- `API/Controllers/ResultController.cs` (return `SchoolResultDto` when school mode)
- `Web/Views/Portal/ResultCalculation.cshtml` (show Percentage/Grade columns instead of GPA)

---

### Stage 25.4 — End-of-Year Promotion System

**What changes:**
- New `PromotionRule` entity (per institution / per grade): `MinPassPercentage` (e.g. 40), `RequireAllSubjectsPass` (bool).
- New `PromotionService.EvaluatePromotionAsync(studentId, academicYearId)`:
  - Checks if student's percentage ≥ `MinPassPercentage`.
  - Optionally checks all subjects individually passed.
  - Returns `Promoted / Failed / Conditional`.
- Bulk promotion action: Admin can promote an entire grade cohort after results are published.
- Failed students are flagged for repeat; Admin marks resolution (repeat / special exam).
- Notification dispatched to student on promotion decision.

**Role rights:**
- **SuperAdmin:** define promotion rules; override any promotion decision.
- **Admin:** run bulk promotion for their department/grade; view promotion reports.
- **Faculty:** view promotion status for their class; cannot change.
- **Student:** view own promotion status.

**Files (new/modified):**
- `Domain/Academic/PromotionRule.cs` (new entity)
- `Domain/Academic/PromotionRecord.cs` (new — stores per-student outcome)
- `Application/Interfaces/IPromotionService.cs` (new)
- `Application/Services/PromotionService.cs` (new)
- `Infrastructure/Repositories/PromotionRepository.cs` (new)
- `Infrastructure/Migrations/` (included in `Phase25_SchoolSystem`)
- `API/Controllers/PromotionController.cs` (new)
- `Web/Views/Portal/Promotion.cshtml` (Admin: bulk promotion + results)

---

## Phase 26 — College Academic System

**Complexity:** Medium | **Dependencies:** Phase 23 ✅, Phase 24 ✅  
**Only active when `IncludeCollege = true`.**

> **Objective:** Implement year-based academic structure (Year 1, Year 2, …) and percentage-based grading for college institutions. Reuses Phase 25 percentage result logic with college-specific labels.

### Stage 26.1 — Year-Based Academic Structure

**What changes:**
- `CollegeYear` entity: `YearNumber` (1, 2, 3…), `YearName` (e.g. "First Year"), `ProgramId`.
- Mapped to existing `Semester` structure — same database rows, displayed as "Year" via `ILabelService`.
- Admin creates and manages college years per program.
- Subjects (courses) linked to college years; students enrolled per year.

**Role rights:**
- **SuperAdmin / Admin:** create, edit, deactivate years.
- **Faculty / Student:** read-only.

**Files (new/modified):**
- `Domain/Academic/CollegeYear.cs` (thin entity, mirrors school grade concept)
- `Infrastructure/Persistence/Configurations/CollegeConfigurations.cs` (new)
- `Infrastructure/Migrations/` (new migration `Phase26_CollegeSystem`)
- `API/Controllers/CollegeYearController.cs` (new)
- `Web/Views/Portal/CollegeYears.cshtml` (Admin page)

---

### Stage 26.2 — College Percentage Result & Promotion

**What changes:**
- `ResultCalculationService` gains a `College` branch: same percentage formula as Stage 25.3.
- `PromotionService` (from Phase 25) is extended for college: Year N → Year N+1 on pass.
- Failed students may be allowed a supplementary exam (configurable by SuperAdmin per program).
- Existing `PromotionRule` entity is reused with an `InstitutionType` discriminator column.

**Role rights:** Same as Phase 25 promotion rights.

**Files (new/modified):**
- `Application/Services/ResultCalculationService.cs` (add college branch)
- `Application/Services/PromotionService.cs` (extend for college years)
- `Domain/Academic/PromotionRule.cs` (add `AppliesTo` discriminator)
- `Web/Views/Portal/ResultCalculation.cshtml` (college mode: Year labels)

---

## Phase 27 — Grading Configuration by Institution Type

**Complexity:** Medium | **Dependencies:** Phase 23 ✅, Phase 25 ✅, Phase 26 ✅, Phase 19 ✅ (`CourseGradingConfig`)

> **Objective:** Give SuperAdmin a dedicated grading setup section split by institution type (School / College / University), where each type's pass threshold, grade bands, and calculation logic can be configured independently. Extends the existing `CourseGradingConfig` foundation (Phase 19).

### Stage 27.1 — Institution-Type Grading Setup Page

**What changes:**
- New `InstitutionGradingConfig` entity: `InstitutionType`, `PassThreshold`, `GradeRangesJson`, `CalculationMode` (Percentage / GPA / Grade-Band).
- SuperAdmin portal page **GradingSetup.cshtml** shows three tabs: School, College, University.
  - Each tab is visible only if the matching license flag is `true` (Phase 24 enforcement).
- On tab, SuperAdmin defines:
  - Minimum pass percentage / GPA.
  - Grade range table (mark-from, mark-to, label, e.g. 90–100 → A+).
  - Calculation logic (Total Percentage vs GPA).
- `IInstitutionGradingConfigService` + `InstitutionGradingConfigService` — CRUD.
- `ResultCalculationService` reads `InstitutionGradingConfig` when calculating results for a given institution type.

**Role rights:**
- **SuperAdmin only:** read and write grading configurations.
- **Admin / Faculty / Student:** grading rules are applied silently; not editable.

**Files (new/modified):**
- `Domain/Academic/InstitutionGradingConfig.cs` (new entity)
- `Application/Interfaces/IInstitutionGradingConfigService.cs` (new)
- `Application/Services/InstitutionGradingConfigService.cs` (new)
- `Application/DTOs/Academic/GradingConfigDTOs.cs` (extend existing)
- `Infrastructure/Repositories/InstitutionGradingConfigRepository.cs` (new)
- `Infrastructure/Migrations/` (new migration `Phase27_InstitutionGrading`)
- `API/Controllers/InstitutionGradingController.cs` (new — SuperAdmin only)
- `Web/Views/Portal/GradingSetup.cshtml` (new tabbed page)
- `Web/Views/Shared/_Layout.cshtml` (add "Grading Setup" under SuperAdmin Result group)

---

### Stage 27.2 — Promotion Logic Integration with Grading Config

**What changes:**
- `PromotionService` reads `InstitutionGradingConfig.PassThreshold` instead of a hard-coded value.
- SuperAdmin changes to grading config automatically affect future promotion evaluations.
- No separate promotion config needed — single source of truth via `InstitutionGradingConfig`.

**Files (new/modified):**
- `Application/Services/PromotionService.cs` (read pass threshold from `IInstitutionGradingConfigService`)

---

## Phase 28 — Parent Portal

**Complexity:** Medium | **Dependencies:** Phase 25 ✅ (school system), Phase 23 ✅ (institution type)  
**Only active when `IncludeSchool = true`.**

> **Objective:** Allow parents/guardians to monitor their child's academic progress — grades, attendance, announcements — through a read-only portal view. No data mutation allowed from the parent role.

### Stage 28.1 — Parent User Role & Account

**What changes:**
- Add `Parent` to the user roles enum.
- `ParentStudentLink` entity: `ParentUserId`, `StudentProfileId` — many-to-many (one parent can have multiple children).
- Admin creates parent accounts and links them to student profiles.
- Parent login is restricted to school-mode institutions (`IncludeSchool = true`); otherwise blocked at middleware.

**Role rights:**
- **Admin / SuperAdmin:** create and manage parent accounts; link parents to students.
- **Parent:** read-only access to linked student(s) data — no write operations whatsoever.
- **Faculty / Student:** no visibility into parent accounts.

**Files (new/modified):**
- `Domain/Enums/UserRole.cs` (add `Parent`)
- `Domain/Academic/ParentStudentLink.cs` (new entity)
- `Infrastructure/Migrations/` (new migration `Phase28_ParentPortal`)
- `API/Controllers/ParentController.cs` (new — GET endpoints only)
- `Web/Controllers/PortalController.cs` (parent-specific view actions)

---

### Stage 28.2 — Parent Dashboard & Views

**What changes:**
- Parent dashboard shows:
  - Child selector (if linked to multiple students).
  - Child's current grade, percentage, and attendance summary.
  - Latest announcements from child's teachers.
  - Upcoming exam dates (from Academic Calendar, Phase 12).
- Parent cannot post, submit, or modify anything — all write actions return HTTP 403.
- Separate portal layout section (`ParentDashboard.cshtml`, `ParentResults.cshtml`, `ParentAttendance.cshtml`).

**Role rights:**
- **Parent:** read-only views of their linked child's data.
- No write access from parent role — enforced at both API attribute and portal controller level.

**Files (new/modified):**
- `Web/Views/Portal/ParentDashboard.cshtml` (new)
- `Web/Views/Portal/ParentResults.cshtml` (new)
- `Web/Views/Portal/ParentAttendance.cshtml` (new)
- `Web/Services/EduApiClient.cs` (parent-specific API methods)
- `Web/Controllers/PortalController.cs` (parent actions)
- `Web/Views/Shared/_Layout.cshtml` (parent sidebar group: "My Children")

---

### Stage 28.3 — Parent Notifications

**What changes:**
- When a student's result is published, a `NotificationType.ResultPublished` notification is dispatched to all linked parents (fan-out, same as student notification).
- When attendance drops below a configurable threshold (e.g. < 75%), a warning notification is sent to parents.
- Parents see notifications in the existing notification bell UI.

**Files (new/modified):**
- `Application/Services/ResultCalculationService.cs` (fan-out to parents on publish)
- `Application/Services/AttendanceAlertService.cs` / `BackgroundJobs/AttendanceAlertJob.cs` (extend to include parent notification)

---

## Phase 29 — Performance Optimization & Database Indexing

**Complexity:** Medium | **Dependencies:** None — can be applied at any point; no code rewrites required

> **Objective:** Apply MSSQL composite indexing, enforce pagination on all list endpoints, and add query plan annotations for the most expensive queries identified in the MSSQL Indexing Strategy guide.

### Stage 29.1 — Composite Index Strategy

**What changes:**
- Add non-clustered composite indexes via EF `HasIndex()` calls in existing configuration files:
  - `students` table: `(DepartmentId, IsActive)`, `(UserId)`
  - `enrollments` table: `(StudentProfileId, OfferingId)`, `(OfferingId, Status)`
  - `results` table: `(StudentProfileId, SemesterId)`, `(OfferingId, IsPublished)`
  - `course_offerings` table: `(CourseId, SemesterId)`, `(DepartmentId, IsActive)`
  - `attendance_records` table: `(StudentProfileId, OfferingId, Date)` — partial index `IsPresent = false`
  - `support_tickets` table: `(Status, AssignedToId)`, `(SubmitterId)`
  - `notifications` table: `(UserId, IsRead)`, `(CreatedAt)` — covering index
  - `study_plans` table: `(StudentProfileId, IsDeleted)`
- All indexes are added via new EF configurations (no existing configs modified — only new `HasIndex` entries in the relevant `Configuration` classes).
- Migration: `Phase29_PerformanceIndexes`.

**Role rights:** No UI/role impact — backend only.

**Files (new/modified):**
- `Infrastructure/Persistence/Configurations/` — targeted files per entity group (add `HasIndex` only)
- `Infrastructure/Migrations/Phase29_PerformanceIndexes` (new migration)

---

### Stage 29.2 — Pagination Enforcement

**What changes:**
- All existing list API endpoints that return unbounded collections get `page` / `pageSize` query parameters (max `pageSize` capped at 100).
- A shared `PagedResult<T>` wrapper DTO is introduced: `{ Items, Page, PageSize, TotalCount }`.
- Affected endpoints: `StudentController.GetAll`, `CourseController.GetAll`, `EnrollmentController.GetByOffering`, `ResultController.GetAll`, `HelpdeskController.GetAll`, `NotificationController.GetAll`.
- Existing single-entity GET endpoints are not changed.
- Web portal list pages adopt `PagedResult` and render a Bootstrap pagination control.

**Role rights:** Pagination applies to all roles equally; no role-based restriction changes.

**Files (new/modified):**
- `Application/DTOs/Common/PagedResult.cs` (new shared wrapper)
- Targeted API controllers (add pagination params — existing endpoints, additive change only)
- Targeted portal views (add pagination UI)

---

### Stage 29.3 — Query Optimization Annotations

**What changes:**
- Add `.AsNoTracking()` to all read-only EF queries in repositories (where not already present).
- Add `.AsSplitQuery()` to complex includes (e.g. `ResultRepository.GetWithComponents`, `LmsRepository.GetModulesByOfferingAsync`) to eliminate cartesian explosion.
- Identify and add `SELECT` projection (anonymous type or DTO projection) for the five most data-heavy list queries (results grid, enrollment list, gradebook).
- No schema changes — code-only optimization.

**Files (new/modified):**
- Targeted repository files (`ResultRepository.cs`, `EnrollmentRepository.cs`, `LmsRepository.cs`, `StudyPlanRepository.cs`, `AttendanceRepository.cs`)

---

## Phase 30 — Distributed Caching & Background Job Optimization

**Complexity:** Medium–High | **Dependencies:** Phase 29 ✅ (indexing must be done first for cache to be effective)

> **Objective:** Introduce Redis distributed caching for expensive read paths (dashboard summaries, reports, student profiles) and move heavy operations (report generation, result pre-computation) to background jobs.

### Stage 30.1 — Redis Cache Integration

**What changes:**
- Add `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package.
- `appsettings.json` gains a `Redis:ConnectionString` section.
- Create `ICacheService` abstraction with `GetOrSetAsync<T>(key, factory, expiry)` and `InvalidateAsync(key)`.
- `RedisCacheService` implements `ICacheService`; falls back to `IMemoryCache` if Redis is unavailable (graceful degradation).
- Cache applied to:
  - `DashboardService.GetStudentDashboardAsync` — 2-minute TTL, invalidated on result publish.
  - `DashboardService.GetAdminSummaryAsync` — 5-minute TTL.
  - `InstitutionGradingConfigService.GetConfigAsync` — 30-minute TTL, invalidated on config change.
  - `LicenseService.GetFlagsAsync` — 60-minute TTL.

**Role rights:** Transparent to all roles — cache is invalidated on write operations that any authorized role performs.

**Files (new/modified):**
- `Application/Interfaces/ICacheService.cs` (new)
- `Infrastructure/Cache/RedisCacheService.cs` (new)
- `API/Program.cs` (add Redis + DI)
- `appsettings.json` / `appsettings.Production.json` (add `Redis` section)
- Targeted services (inject `ICacheService`)

---

### Stage 30.2 — Background Report Generation

**What changes:**
- Move large report generation (enrollment summary, result summary, promotion report) out of the API request pipeline and into Hangfire-style background jobs (using the existing `BackgroundJobs` project).
- New `ReportGenerationJob`: triggered by Admin via `POST /api/v1/reports/generate` → returns a `JobId`.
- `GET /api/v1/reports/status/{jobId}` returns job progress.
- Completed report stored as a file in `wwwroot/reports/` and linked to the requesting user.
- Admin is notified (in-app) when the report is ready to download.

**Role rights:**
- **SuperAdmin / Admin:** trigger report generation, download completed reports.
- **Faculty:** can trigger course-scoped reports only.
- **Student:** no access to report generation.

**Files (new/modified):**
- `BackgroundJobs/ReportGenerationJob.cs` (new)
- `Domain/Reports/ReportJob.cs` (new entity — tracks job status)
- `Infrastructure/Migrations/` (new migration `Phase30_ReportJobs`)
- `API/Controllers/ReportJobController.cs` (new)
- `Web/Views/Portal/Reports.cshtml` (add "Generate Report" + status polling)

---

### Stage 30.3 — Pre-Computed Result Snapshots

**What changes:**
- After a result batch is published, a background job computes and stores a `ResultSnapshot` (flat denormalized record) for each student.
- Snapshot stores: `StudentProfileId`, `OfferingId`, `SemesterId`, `Percentage`, `GPA`, `LetterGrade`, `ComputedAt`.
- Dashboard and report queries read from `ResultSnapshot` instead of joining multiple tables — dramatically reduces query complexity.
- Snapshots are invalidated and recomputed when a result is re-published or overridden.

**Files (new/modified):**
- `Domain/Academic/ResultSnapshot.cs` (new entity)
- `Infrastructure/Migrations/` (new migration `Phase30_ResultSnapshots`)
- `Application/Services/ResultPublishService.cs` (trigger snapshot job on publish)
- `BackgroundJobs/ResultSnapshotJob.cs` (new)
- `Application/Services/DashboardService.cs` (read from snapshot)

---

## Phase 31 — Advanced Reporting & Analytics

**Complexity:** Medium | **Dependencies:** Phase 27 ✅ (grading config), Phase 30 ✅ (snapshots for fast queries), Phase 23 ✅ (institution type for report sectioning)

> **Objective:** Divide the Report Center into institution-type sections, add top-performer analytics, subject-wise performance charts, and generate printable PDF report cards per student.

### Stage 31.1 — Report Center Sectioned by Institution Type

**What changes:**
- Report Center portal page gains institution-type tabs (School / College / University) — tab visible only if license flag is `true` (Phase 24).
- Each tab exposes reports relevant to that type:
  - **School:** Class Result Report, Grade Promotion Report, Stream-wise Performance.
  - **College:** Year Result Report, Year Promotion Report.
  - **University:** Semester GPA Report, CGPA Progression, Enrollment Summary.
- Existing reports are reorganized into the University tab (no logic change).

**Role rights:**
- **SuperAdmin:** all tabs and all institution reports.
- **Admin:** only tabs matching their institution's license flags; filtered to their departments.
- **Faculty:** course/subject-scoped reports only (own offerings).
- **Student:** no access to Report Center.

**Files (new/modified):**
- `Web/Views/Portal/Reports.cshtml` (restructure into institution-type tabs)
- `Web/Controllers/PortalController.cs` (institution-type report actions)
- `Application/Services/ReportService.cs` (add school/college report methods)
- `API/Controllers/ReportController.cs` (new school/college endpoints)

---

### Stage 31.2 — Top Performers & Trend Analytics

**What changes:**
- New `GET /api/v1/analytics/top-performers?semesterId=&limit=10` — returns ranked students by GPA/percentage.
- New `GET /api/v1/analytics/subject-performance?offeringId=` — returns average, highest, lowest, pass-rate per subject.
- New `GET /api/v1/analytics/attendance-trend?offeringId=&weeks=8` — returns weekly attendance percentage.
- Data sourced from `ResultSnapshot` (Phase 30) for performance; attendance from existing tables.
- Portal page **Analytics.cshtml** with Chart.js bar/line charts.

**Role rights:**
- **SuperAdmin / Admin:** all analytics across their institution.
- **Faculty:** analytics scoped to their own offerings.
- **Student:** no access to aggregate analytics.

**Files (new/modified):**
- `Application/Interfaces/IAnalyticsService.cs` (new)
- `Application/Services/AnalyticsService.cs` (new)
- `Infrastructure/Repositories/AnalyticsRepository.cs` (new)
- `API/Controllers/AnalyticsController.cs` (new)
- `Application/DTOs/Analytics/AnalyticsDTOs.cs` (new)
- `Web/Views/Portal/Analytics.cshtml` (new — Chart.js charts)
- `Web/Views/Shared/_Layout.cshtml` (sidebar: "Analytics" under Admin section)

---

### Stage 31.3 — PDF Report Card Generation

**What changes:**
- New `ReportCardService` generates a per-student PDF using QuestPDF (already installed for graduation certificates).
- Report card content varies by institution type:
  - **School:** student name, grade, stream, all subjects with marks + percentage + letter grade, promotion status, attendance %.
  - **College:** student name, year, all subjects with marks + percentage, year status.
  - **University:** student name, semester, all courses with GPA contribution, CGPA, standing.
- Admin triggers generation per student or in bulk per class/year/semester.
- PDF stored in `wwwroot/reportcards/{studentId}/` and linked to student profile.
- Student can download their own published report card from the portal.

**Role rights:**
- **SuperAdmin / Admin:** generate and download any student's report card.
- **Faculty:** generate report cards for students in their class/offering.
- **Student:** download own published report card only.

**Files (new/modified):**
- `Application/Interfaces/IReportCardService.cs` (new)
- `Infrastructure/Services/ReportCardService.cs` (new — QuestPDF)
- `API/Controllers/ReportCardController.cs` (new)
- `Web/Views/Portal/Students.cshtml` (add "Generate Report Card" button)
- `Web/Views/Portal/StudentProfile.cshtml` (add download link if report card exists)

---

### Stage 31.4 — Export Enhancements (PDF + Excel)

**What changes:**
- All existing CSV exports gain a companion Excel (`.xlsx`) export using `ClosedXML` or `EPPlus`.
- All grid-based reports gain a PDF print view using browser `window.print()` + a print-optimized stylesheet.
- No new API endpoints needed — existing report endpoints gain an `?format=excel` query parameter.

**Files (new/modified):**
- `API/Controllers/ReportController.cs` (add Excel format branch)
- `Application/Services/ReportService.cs` (Excel renderer)
- `Web/Views/Portal/Reports.cshtml` (Excel download buttons)

---

## Phase 32 — Communication Enhancements

**Complexity:** Medium | **Dependencies:** Existing notification system (Phase 14 ✅)

> **Objective:** Add direct in-portal messaging between users, email integration for notifications, and SMS delivery for critical alerts.

### Stage 32.1 — Direct Messaging System

**What changes:**
- New `Message` entity: `SenderId`, `RecipientId`, `Body`, `SentAt`, `IsRead`.
- `MessageThread` (pair of users) aggregates all messages between two parties.
- Portal page **Messaging.cshtml**: inbox, thread view, compose form.
- Faculty can message their enrolled students; Students can message their Faculty.
- Admin/SuperAdmin can message any user.
- Students cannot initiate messages to Admin (they use helpdesk for that — Phase 14).

**Role rights:**
- **SuperAdmin / Admin:** message any user.
- **Faculty:** message enrolled students and Admin.
- **Student:** message only their enrolled Faculty (course teachers).
- **Parent:** message only linked student's Faculty.

**Files (new/modified):**
- `Domain/Communication/Message.cs` (new entity)
- `Domain/Communication/MessageThread.cs` (new entity)
- `Application/Interfaces/IMessagingService.cs` (new)
- `Application/Services/MessagingService.cs` (new)
- `Infrastructure/Repositories/MessagingRepository.cs` (new)
- `Infrastructure/Migrations/` (new migration `Phase32_Messaging`)
- `API/Controllers/MessagingController.cs` (new)
- `Web/Views/Portal/Messaging.cshtml` (new)
- `Web/Views/Shared/_Layout.cshtml` (sidebar: "Messages" with unread badge)

---

### Stage 32.2 — Email Notification Integration

**What changes:**
- `IEmailService` abstraction with `SendAsync(to, subject, htmlBody)`.
- `SmtpEmailService` implementation using `MailKit`; configured via `appsettings.json` (`Email:SmtpHost`, `Email:Port`, `Email:From`, `Email:Password`).
- Email sent alongside in-app notification for critical events:
  - Result published → student + parent (if school mode).
  - Graduation approved → student.
  - Helpdesk ticket status change → submitter.
  - Password reset → user.
- SuperAdmin can enable/disable email integration per notification type in Portal Settings.

**Role rights:**
- **SuperAdmin:** configure SMTP settings, enable/disable per notification type.
- All roles: receive emails for events relevant to them.

**Files (new/modified):**
- `Application/Interfaces/IEmailService.cs` (new)
- `Infrastructure/Services/SmtpEmailService.cs` (new)
- `API/Program.cs` (DI registration)
- `appsettings.json` (add `Email` section)
- `Application/Services/NotificationService.cs` (trigger email alongside in-app notification)
- `Web/Views/Portal/Settings.cshtml` (SMTP config for SuperAdmin)

---

### Stage 32.3 — SMS Notification Support

**What changes:**
- `ISmsService` abstraction with `SendAsync(to, body)`.
- `TwilioSmsService` (or pluggable gateway) implementation.
- SMS triggered for highest-priority events only:
  - Result published (school/college).
  - Attendance warning to parent.
  - Exam schedule reminder (day before).
- Phone number stored on `User` profile (optional field); SMS silently skipped if no number.
- SuperAdmin configures SMS provider credentials and enables per event type.

**Role rights:**
- **SuperAdmin:** configure SMS provider, enable/disable per event.
- All roles: receive SMS for their relevant events.

**Files (new/modified):**
- `Application/Interfaces/ISmsService.cs` (new)
- `Infrastructure/Services/TwilioSmsService.cs` (new)
- `Domain/Auth/User.cs` (add optional `PhoneNumber`)
- `Infrastructure/Migrations/` (migration `Phase32_Communication`)
- `API/Program.cs` (DI registration)
- `Web/Views/Portal/Settings.cshtml` (SMS config panel)

---

## Phase 33 — SaaS & Multi-Tenant Readiness

**Complexity:** High | **Dependencies:** Phase 23 ✅, Phase 24 ✅; requires production planning review before implementation

> **Objective:** Introduce tenant isolation (each institution is a tenant), subscription management, onboarding workflow, and per-tenant branding customization. This phase makes EduSphere commercially deployable as a SaaS product.

### Stage 33.1 — Tenant Model & Data Isolation

**What changes:**
- Add `Tenant` entity: `Id`, `Name`, `Subdomain`, `IsActive`, `CreatedAt`.
- Add `TenantId` (nullable, `Guid`) to all major tables: `users`, `students`, `faculty`, `courses`, `departments`, `programs`, `enrollments`, `results`, `timetables`, `notifications`, `support_tickets`.
- `TenantContextMiddleware` reads subdomain or `X-Tenant-Id` header and sets `ITenantContext.CurrentTenantId` for the request lifetime.
- All repository queries append `.Where(x => x.TenantId == _tenantContext.CurrentTenantId)` via a global query filter extension method.
- SuperAdmin role is tenant-scoped per tenant; the platform-level SuperAdmin (Tabsan staff) has `TenantId = null` (full access).

> **Note:** `TenantId` columns default `null` initially so existing single-tenant deployments continue working unchanged. Multi-tenant mode is opt-in via a feature flag.

**Role rights:**
- **Platform SuperAdmin (Tabsan):** full cross-tenant access (`TenantId = null`).
- **SuperAdmin (per tenant):** full access within their tenant.
- **Admin / Faculty / Student / Parent:** scoped to their tenant only.

**Files (new/modified):**
- `Domain/Tenant/Tenant.cs` (new entity)
- `Application/Interfaces/ITenantContext.cs` (new)
- `Infrastructure/Middleware/TenantContextMiddleware.cs` (new)
- `Infrastructure/Persistence/Extensions/TenantQueryFilterExtensions.cs` (new)
- `Infrastructure/Migrations/` (large migration `Phase33_MultiTenant` — adds TenantId columns)
- `API/Program.cs` (register middleware + ITenantContext)

---

### Stage 33.2 — Subscription Management

**What changes:**
- `Subscription` entity: `TenantId`, `Plan` (Basic / Professional / Enterprise), `StartsAt`, `ExpiresAt`, `IsActive`, `MaxStudents`, `MaxFaculty`.
- `SubscriptionService` checks active subscription on login; if expired, portal shows renewal notice and blocks write operations.
- Platform SuperAdmin can create, renew, or terminate subscriptions.
- Subscription tier controls max user counts (enforced at user-creation time).

**Role rights:**
- **Platform SuperAdmin:** full subscription management.
- **Tenant SuperAdmin:** view own subscription status; cannot modify.
- All other roles: no visibility.

**Files (new/modified):**
- `Domain/Tenant/Subscription.cs` (new entity)
- `Application/Interfaces/ISubscriptionService.cs` (new)
- `Application/Services/SubscriptionService.cs` (new)
- `Infrastructure/Migrations/` (included in `Phase33_MultiTenant`)
- `API/Controllers/SubscriptionController.cs` (new — platform SuperAdmin only)
- `Web/Views/Portal/Subscription.cshtml` (tenant SuperAdmin read-only view)

---

### Stage 33.3 — Tenant Onboarding Wizard

**What changes:**
- New multi-step onboarding wizard for new tenants:
  1. Institution name + type selection.
  2. License flag configuration (School / College / University).
  3. Admin account creation.
  4. Initial department + program setup.
  5. Confirmation + launch.
- Platform SuperAdmin triggers the wizard from a "New Tenant" action.
- After completion, tenant is active and Admin can log in.

**Files (new/modified):**
- `Web/Views/Platform/OnboardingWizard.cshtml` (new multi-step view)
- `API/Controllers/TenantController.cs` (new — provision tenant)
- `Application/Services/TenantProvisioningService.cs` (new)

---

### Stage 33.4 — Per-Tenant Branding Customization

**What changes:**
- `TenantBranding` entity: `TenantId`, `LogoUrl`, `PrimaryColor`, `SecondaryColor`, `PortalTitle`, `FaviconUrl`.
- Branding is loaded at login and injected into layout via `ViewBag.Branding`.
- SuperAdmin uploads logo via existing file-upload pattern; colors stored as hex.
- Default Tabsan EduSphere branding used when no custom branding is set.

**Role rights:**
- **SuperAdmin (per tenant):** manage their own branding.
- All other roles: see branding passively.

**Files (new/modified):**
- `Domain/Tenant/TenantBranding.cs` (new entity)
- `Infrastructure/Migrations/` (included in `Phase33_MultiTenant`)
- `API/Controllers/BrandingController.cs` (new)
- `Web/Views/Shared/_Layout.cshtml` (read `ViewBag.Branding` for logo, title, colors)
- `Web/Views/Portal/BrandingSettings.cshtml` (new SuperAdmin page)

---

## Phase 34 — Advanced Authentication & Security

**Complexity:** Medium | **Dependencies:** Existing JWT auth infrastructure

> **Objective:** Add Multi-Factor Authentication (MFA) support and strengthen audit logging for sensitive operations.

### Stage 34.1 — Multi-Factor Authentication (TOTP)

**What changes:**
- `User` entity gains `TotpSecret` (nullable, encrypted), `MfaEnabled` (bool).
- Users can opt-in to TOTP MFA from profile settings (any role).
- SuperAdmin can mandate MFA for Admin and SuperAdmin roles via system settings.
- Login flow: if MFA is enabled, issue a short-lived "MFA pending" JWT → user submits TOTP code → issued full access JWT.
- QR code generation endpoint: `GET /api/v1/auth/mfa/setup` (returns TOTP secret + QR code PNG).
- TOTP verification endpoint: `POST /api/v1/auth/mfa/verify`.

**Role rights:**
- **All roles:** can opt-in to MFA voluntarily.
- **SuperAdmin:** can mandate MFA for Admin/SuperAdmin roles system-wide.

**Files (new/modified):**
- `Domain/Auth/User.cs` (add `TotpSecret`, `MfaEnabled`)
- `Application/Services/AuthService.cs` (add MFA challenge flow)
- `API/Controllers/AuthController.cs` (add MFA endpoints)
- `Infrastructure/Migrations/` (migration `Phase34_MFA`)
- `Web/Views/Portal/Profile.cshtml` (MFA setup section)

---

### Stage 34.2 — Enhanced Audit Logging

**What changes:**
- Extend the existing audit trail to capture all sensitive operations:
  - User login / logout / failed attempt (with IP address).
  - Role changes, password resets.
  - License import and flag changes.
  - Grading config modifications.
  - Result publish / override.
  - Graduation approval / certificate issue.
  - Tenant creation / subscription change (Phase 33).
- `AuditLog` entity extended: add `IpAddress`, `UserAgent`, `EntityType`, `EntityId`, `OldValueJson`, `NewValueJson`.
- SuperAdmin and Platform SuperAdmin can search and export the audit log.
- Audit log entries are immutable (no update/delete operations).

**Role rights:**
- **Platform SuperAdmin:** full audit log across all tenants.
- **SuperAdmin (per tenant):** audit log for their own tenant.
- **Admin:** view audit log for operations they performed.
- **Faculty / Student / Parent:** no access to audit log.

**Files (new/modified):**
- `Domain/Audit/AuditLog.cs` (extend existing entity)
- `Infrastructure/Migrations/` (migration `Phase34_AuditEnhancement`)
- `Application/Services/AuditService.cs` (extend with new event types)
- `Web/Views/Portal/AuditLog.cshtml` (search + export UI)

---

## Phase 35 — Mobile & Progressive Web App (PWA) Readiness

**Complexity:** Low–Medium | **Dependencies:** Phase 29 ✅ (pagination — mobile needs paginated APIs)

> **Objective:** Make the EduSphere portal usable as a PWA on mobile browsers with offline-capable key pages, without requiring a native mobile app build.

### Stage 35.1 — Responsive UI Audit & Fixes

**What changes:**
- Audit all portal views for mobile breakpoint (≤ 768px) rendering issues.
- Fix tables that overflow on mobile (convert to card layout using Bootstrap responsive utilities).
- Ensure sidebar collapses correctly on mobile; navigation is thumb-friendly.
- No new views created — existing views corrected.

**Files (new/modified):** Targeted `.cshtml` portal views (CSS / layout fixes only)

---

### Stage 35.2 — PWA Manifest & Service Worker

**What changes:**
- Add `manifest.webmanifest` to `wwwroot/` with app name, icons, theme color, `display: standalone`.
- Add `service-worker.js` that caches the shell (layout, CSS, JS) and key static assets.
- Offline fallback page `offline.cshtml` shown when no network available.
- Students can "Add to Home Screen" on mobile for an app-like experience.

**Files (new/modified):**
- `wwwroot/manifest.webmanifest` (new)
- `wwwroot/service-worker.js` (new)
- `Web/Views/Shared/_Layout.cshtml` (link manifest, register service worker)
- `Web/Views/Portal/Offline.cshtml` (new fallback)

---

### Stage 35.3 — Mobile API Optimizations

**What changes:**
- Dashboard endpoint gains a `?compact=true` query param that returns a reduced payload (just the key stats, no detail lists) for mobile clients.
- All list endpoints enforce the pagination from Phase 29 — mobile callers use `pageSize=10`.
- Response compression (`UseResponseCompression` with Gzip/Brotli) enabled in the API pipeline.

**Files (new/modified):**
- `API/Program.cs` (add `UseResponseCompression`)
- `API/Controllers/DashboardController.cs` (add `?compact=true` param)

---

## Phase 36 — AI & Predictive Analytics

**Complexity:** High | **Dependencies:** Phase 30 ✅ (result snapshots), Phase 31 ✅ (analytics data), Phase 30 ✅ (caching for AI result delivery)  
**Implement last** — requires sufficient historical data in `ResultSnapshot` to be meaningful.

> **Objective:** Add AI-driven features: an in-portal chatbot for student queries, at-risk student early-warning detection, and study recommendation enhancement using performance history.

### Stage 36.1 — AI Chatbot for Students

**What changes:**
- Embed a configurable AI chatbot widget in the student portal (bottom-right floating button).
- Chatbot can answer FAQs from a knowledge base managed by Admin (Q&A pairs stored in `ChatbotKnowledge` entity).
- Optionally connect to an external LLM API (e.g. OpenAI) configured by SuperAdmin via API key in settings.
- Chat history stored per session (not persisted after logout — privacy-safe).
- Admin/SuperAdmin can view aggregated (anonymized) chatbot usage statistics.

**Role rights:**
- **Student / Faculty:** use the chatbot widget.
- **Admin:** manage knowledge base Q&A entries.
- **SuperAdmin:** configure LLM API key, view usage stats.

**Files (new/modified):**
- `Domain/AiChat/ChatbotKnowledge.cs` (new entity — extends existing `AiChat` domain folder)
- `Application/Interfaces/IChatbotService.cs` (new)
- `Application/Services/ChatbotService.cs` (new — local KB lookup + optional LLM fallback)
- `Infrastructure/Repositories/ChatbotKnowledgeRepository.cs` (new)
- `Infrastructure/Migrations/` (migration `Phase36_AI`)
- `API/Controllers/ChatbotController.cs` (new)
- `Web/Views/Shared/_Layout.cshtml` (add chatbot widget script)
- `Web/Views/Portal/ChatbotKnowledge.cshtml` (Admin knowledge management page)

---

### Stage 36.2 — At-Risk Student Early Warning System

**What changes:**
- `AtRiskDetectionService` runs as a weekly background job.
- Detection criteria (configurable by SuperAdmin):
  - Attendance below threshold (e.g. < 70%) in any offering.
  - Mark below pass threshold in more than one assessment component.
  - No LMS activity (no module viewed) for > 2 weeks.
- Flagged students are recorded in `AtRiskRecord` entity: `StudentProfileId`, `Reason`, `DetectedAt`, `IsResolved`.
- Faculty advisor is notified (in-app + email) for each at-risk student in their offerings.
- Admin dashboard shows an "At-Risk Students" count badge and list.
- Faculty can mark a student as resolved with a note; resolution is logged.

**Role rights:**
- **SuperAdmin / Admin:** view all at-risk records for their institution; configure detection thresholds.
- **Faculty:** view at-risk students in their own offerings; mark resolved.
- **Student / Parent:** no access to at-risk flags (privacy).

**Files (new/modified):**
- `Domain/Academic/AtRiskRecord.cs` (new entity)
- `Application/Interfaces/IAtRiskDetectionService.cs` (new)
- `Application/Services/AtRiskDetectionService.cs` (new)
- `BackgroundJobs/AtRiskDetectionJob.cs` (new weekly job)
- `Infrastructure/Migrations/` (included in `Phase36_AI`)
- `API/Controllers/AtRiskController.cs` (new)
- `Web/Views/Portal/AtRisk.cshtml` (new — Admin/Faculty view)
- `Web/Views/Shared/_Layout.cshtml` (at-risk badge on Admin dashboard link)

---

### Stage 36.3 — Predictive Performance Analytics

**What changes:**
- `PredictiveAnalyticsService` computes a simple linear-trend prediction for each student's likely end-of-semester GPA/percentage based on mid-semester marks and attendance.
- Prediction displayed on the student's own dashboard ("Estimated final GPA: 3.2 — on track") and on the Faculty gradebook.
- Prediction accuracy improves as more `ResultSnapshot` data accumulates.
- No external ML model required — trend extrapolation using existing data.

**Role rights:**
- **Student:** see their own prediction on dashboard.
- **Faculty:** see predictions for all their enrolled students.
- **Admin / SuperAdmin:** see aggregate prediction summaries.

**Files (new/modified):**
- `Application/Interfaces/IPredictiveAnalyticsService.cs` (new)
- `Application/Services/PredictiveAnalyticsService.cs` (new)
- `API/Controllers/AnalyticsController.cs` (add prediction endpoint)
- `Web/Views/Portal/Dashboard.cshtml` (student: add prediction widget)
- `Web/Views/Portal/Gradebook.cshtml` (faculty: add prediction column)

---

## Implementation Sequence Summary

| Phase | Feature | Complexity | Dependency | Status |
|-------|---------|-----------|------------|--------|
| 22 | External Integrations (Library + Accreditation) | High | None | Planned |
| **23** | **Multi-Institution Type Foundation** | High | Phase 19 | Planned |
| **24** | **License-Driven Module Control** | Medium | Phase 23 | Planned |
| **25** | **School Academic System** | High | Phase 23, 24 | Planned |
| **26** | **College Academic System** | Medium | Phase 23, 24 | Planned |
| **27** | **Grading Configuration by Institution Type** | Medium | Phase 25, 26 | Planned |
| **28** | **Parent Portal** | Medium | Phase 25 | Planned |
| **29** | **Performance Optimization & DB Indexing** | Medium | None | Planned |
| **30** | **Distributed Caching & Background Jobs** | Medium–High | Phase 29 | Planned |
| **31** | **Advanced Reporting & Analytics** | Medium | Phase 27, 30 | Planned |
| **32** | **Communication Enhancements** | Medium | Phase 14 ✅ | Planned |
| **33** | **SaaS & Multi-Tenant Readiness** | High | Phase 23, 24 | Planned |
| **34** | **Advanced Auth & Security (MFA)** | Medium | None | Planned |
| **35** | **Mobile & PWA Readiness** | Low–Medium | Phase 29 | Planned |
| **36** | **AI & Predictive Analytics** | High | Phase 30, 31 | Planned |

---

## Preserved Invariants (Must Not Change in Any Phase)

1. **SuperAdmin always has unrestricted CRUD** on every entity in the system.
2. **Admin can add/edit/deactivate** any data within their own institution: courses, degrees, programs, students, faculty, results, timetables, enrollments.
3. **Faculty can only modify data in their own course offerings** — no access to other faculty's data.
4. **Students see only their own data** — role enforcement is at API level (JWT claims) and repository level (query filters), not just UI.
5. **Soft-delete is preserved** for all `AuditableEntity` subclasses — physical deletes are never used for auditable data.
6. **Comment convention** on every code block: `// Final-Touches Phase X Stage X.X — <description>`.
7. **EF Code-First** with `ApplyConfigurationsFromAssembly`; no raw SQL in application code.
8. **Repository pattern** — domain interfaces in `Domain/Interfaces/`, implementations in `Infrastructure/Repositories/`.
9. **DI registrations** via fully-qualified names in `API/Program.cs` only.
10. **License enforcement** is additive — no existing licensed functionality is removed or restricted.
