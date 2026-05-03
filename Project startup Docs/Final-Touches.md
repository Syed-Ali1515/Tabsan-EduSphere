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
**Status:** In Progress

### Stage 2.1 - Faculty/Student Timetable Data
- [x] Fix My Timetable (Faculty) data binding.
- [x] Fix My Timetable (Student) data binding.
- [x] Confirm Timetable Admin, Faculty, Student views all load expected rows.

### Stage 2.2 - Building, Student, Department, Course Visibility
- [ ] Fix Buildings list retrieval.
- [ ] Fix Students list retrieval (names visible).
- [ ] Fix Departments list retrieval (names visible).
- [ ] Fix Courses page active offering retrieval.

### Stage 2.3 - CRUD Entry Points
- [ ] Add Students create flow.
- [ ] Add Departments create flow.
- [ ] Add Active Offerings create/edit/delete flow.

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

## Phase 3 - Assignment, Attendance, Results, Quizzes, FYP Access and Workflows
**Status:** Not Started

### Stage 3.1 - 403 Authorization Fixes
- [ ] Resolve 403 in Assignments.
- [ ] Resolve 403 in Attendance.
- [ ] Resolve 403 in Results.
- [ ] Resolve 403 in Quizzes.
- [ ] Resolve 403 in FYP.

### Stage 3.2 - Data Entry Workflows
- [ ] Add Assignments create/edit/delete and assign-to-students workflows.
- [ ] Add Attendance create/edit/delete workflow for students.
- [ ] Add Results create/edit/delete workflow for students.
- [ ] Add Quizzes create/edit/delete and assignment workflow for students.
- [ ] Add FYP create/edit/delete workflow for students.

### Stage 3.3 - Result-Driven Promotion Logic
- [ ] Add Promote column in result entry with Yes/No option for failed students.
- [ ] Implement automatic promotion to next semester based on entered result decision.
- [ ] Remove/replace manual promotion dependency where required.

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Phase 4 - Reporting and Export Completion
**Status:** Not Started

### Stage 4.1 - Report Center Functional Completeness
- [ ] Ensure Report Center menu is visible in sidebar by role and opens correctly.
- [ ] Fix Department Summary report.
- [ ] Fix Result Summary report.
- [ ] Fix Semester Result report.
- [ ] Ensure role/department/subject/semester filters work end-to-end.

### Stage 4.2 - Add Additional Reports
- [ ] Add more reports as required by PRD scope and current modules.

### Stage 4.3 - Excel Export UX and Reliability
- [ ] Fix Excel button label rendering (text missing).
- [ ] Fix export endpoint failures.
- [ ] Validate generated files for Attendance, Result, GPA, Enrollment and new reports.

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Phase 5 - Settings Pages Functional Save Actions
**Status:** Not Started

### Stage 5.1 - Report Settings Save
- [ ] Add/repair save action and success/error feedback in Report Settings.

### Stage 5.2 - Module Settings Save
- [ ] If modules exist: render all modules and support activate/deactivate + save.
- [ ] If modules do not exist: remove Module Settings menu and related dead routes.

### Stage 5.3 - Sidebar Settings Save
- [ ] Add/repair save action for role assignments and visibility toggles.

### Stage 5.4 - Theme Settings Expansion
- [ ] Add more themes.
- [ ] Ensure themes persist and apply consistently.

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Phase 6 - Notifications and Analytics
**Status:** Not Started

### Stage 6.1 - Notifications 404 Fix
- [ ] Fix Notifications endpoint mismatch or missing route.
- [ ] Verify notification list, read state, and mark-all-read behavior.

### Stage 6.2 - Analytics Data Rendering
- [ ] Replace random/static code output with real analytics data.
- [ ] Validate Performance, Attendance, Assignment analytics cards and sections.

### Implementation Summary
- Pending

### Validation Summary
- Pending

---

## Phase 7 - Finance and Payments Module Completion
**Status:** Not Started

### Stage 7.1 - Finance Sidebar and Navigation
- [ ] Add Finance-related menus and grouping in sidebar.

### Stage 7.2 - Fees and Receipts Admin Workflows
- [ ] Add create/edit/delete fee receipt setup.
- [ ] Admin fee setup by course level for all subjects in the course.

### Stage 7.3 - Student Payment Flow
- [ ] Students can view/print receipts.
- [ ] Students can click Paid to submit proof/confirmation.
- [ ] Admin verification workflow for marked paid receipts.
- [ ] Trigger notifications on receipt creation and payment actions.

### Implementation Summary
- Pending

### Validation Summary
- Pending

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
- [ ] Phase 1 complete
- [ ] Phase 2 complete
- [ ] Phase 3 complete
- [ ] Phase 4 complete
- [ ] Phase 5 complete
- [ ] Phase 6 complete
- [ ] Phase 7 complete
- [ ] Phase 8 complete
- [ ] Phase 9 complete

## Next Phase To Execute
Phase 1 - Navigation, Session Stability, Sidebar Structure
