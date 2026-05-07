# Enhancements — Gap Analysis (University Portal)

**Source:** Gap Analysis PRD (May 2026)  
**Scope:** Features identified as missing from the current system, organised into phases and stages using the same numbering scheme as `Issue-Fix-Phases.md` (continues from Phase 11).  
**Status:** All phases are **Planned — Not Started**.

---

## Phase 12 — Learning Management System (LMS)

### Stage 12.1 — Structured Course Content
- Faculty can create weekly module/lecture units per course offering.
- Each unit supports rich-text content, attachments, and ordering.
- Students can access published units in order within their enrolled offerings.
- SuperAdmin and Admin can configure which LMS modules are enabled per portal instance.

### Stage 12.2 — Video-Based Teaching
- Faculty can upload or embed video lectures against a module unit.
- Video playback is accessible from the student portal with progress tracking.
- Storage limits and allowed formats are configurable by SuperAdmin.

### Stage 12.3 — Discussion Forums
- Each course offering has a threaded discussion forum.
- Faculty can post announcements pinned to the top of the forum.
- Students can create threads and reply; Faculty can moderate (pin, delete, close threads).
- Notifications are sent on new replies to threads the user has participated in.

### Stage 12.4 — Course Announcements
- Faculty can post course-level announcements visible to all enrolled students.
- Announcements appear on the student dashboard and generate in-app notifications.
- Admin can post department-wide announcements visible to all students and faculty in that department.

---

## Phase 13 — Degree Audit System

### Stage 13.1 — Credit Completion Tracking
- System tracks total credits earned per student across all completed semesters.
- A credit summary breakdown (core vs elective) is visible to the student and their academic advisor (Faculty).
- Admin can view credit summaries for all students in their assigned departments.

### Stage 13.2 — Graduation Eligibility Checker
- SuperAdmin defines degree rules: minimum credits, required core courses, minimum GPA threshold.
- System automatically flags students as graduation-eligible or ineligible based on current academic record.
- Admin can view a dashboard of eligible vs ineligible students per department/program.
- Students can view their own eligibility status and the outstanding requirements.

### Stage 13.3 — Elective vs Core Validation
- Courses are tagged as Core or Elective at the course/program level.
- Degree audit validates that the student has satisfied core course requirements and a minimum elective credit count.
- System surfaces specific unmet requirements to the student and their assigned Faculty advisor.

---

## Phase 14 — Enrollment Rules Engine

### Stage 14.1 — Prerequisite Validation
- Courses can have prerequisite courses configured by Admin or SuperAdmin.
- Enrollment is blocked if the student has not passed the prerequisite course(s).
- Student receives a clear message listing the unmet prerequisites when enrollment is rejected.

### Stage 14.2 — Timetable Clash Detection
- System checks for time slot conflicts across a student's selected course offerings before confirming enrollment.
- Conflicting offerings are highlighted with the clashing schedule details.
- Admin can override clash detection for exceptional cases with an audit log entry.

### Stage 14.3 — Course Capacity Limits
- Faculty or Admin can set a maximum enrollment capacity per course offering.
- Enrollment is blocked when capacity is reached; student is shown current availability.
- Admin can increase capacity or override the limit with a documented reason.

---

## Phase 15 — Faculty Grading System

### Stage 15.1 — Gradebook Grid View
- Faculty have a spreadsheet-style gradebook showing all enrolled students as rows and assessment components (assignments, quizzes, exams) as columns.
- Marks can be entered directly in the grid with inline save.
- Gradebook auto-computes total/final grade per student based on configured weightings.

### Stage 15.2 — Rubric-Based Grading
- Faculty can define a rubric (criteria + performance levels + point values) per assessment.
- When grading, Faculty select a performance level per criterion; system totals the marks automatically.
- Students can view the rubric and their scored levels as part of their feedback.

### Stage 15.3 — Bulk Grading
- Faculty can upload a CSV of student marks for a given assessment component.
- System validates the CSV (student IDs, mark ranges) and previews changes before applying.
- Bulk submission triggers the same notifications as individual mark entry.

---

## Phase 16 — Academic Calendar System

### Stage 16.1 — Semester Timelines
- SuperAdmin defines an academic calendar: semester start/end dates, holiday periods.
- Calendar is visible to all roles as a portal-wide read-only view.
- Semester-scoped features (Assignments, Quizzes, Attendance) respect the active semester boundaries.

### Stage 16.2 — Key Deadlines Management
- Admin can add named deadlines to the academic calendar (census date, exam period start/end, assignment cutoff).
- Deadlines appear on student and faculty dashboards with days-remaining indicators.
- System sends automated reminders at configurable intervals before each deadline.

---

## Phase 17 — Study Planner

### Stage 17.1 — Semester Planning Tool
- Students can build a tentative course plan for future semesters by selecting available courses.
- Planner validates prerequisites and credit load limits before confirming a plan.
- Saved plans are visible to the student's assigned Faculty advisor.

### Stage 17.2 — Course Recommendation Engine
- System recommends courses for the next semester based on the student's current program, completed credits, and degree audit gaps.
- Faculty advisors can endorse or modify recommendations before they are shown to the student.
- SuperAdmin configures recommendation rules and weightings per program.

---

## Phase 18 — Helpdesk / Support Ticketing System

### Stage 18.1 — Ticket Submission and Tracking
- Students and Faculty can raise support tickets categorised by type (academic, technical, administrative).
- Each ticket has a status lifecycle: Open → In Progress → Resolved → Closed.
- Submitter receives in-app notifications on each status change.

### Stage 18.2 — Admin Case Management
- Admin can view, assign, and resolve tickets within their department scope.
- SuperAdmin has full visibility across all departments and can reassign or escalate tickets.
- SLA timers track time-to-resolution; overdue tickets are highlighted in the Admin dashboard.

### Stage 18.3 — Faculty Ticket Responses
- Faculty can respond to course-related tickets assigned to them.
- Response threads are visible to the original submitter with full history.
- Resolved tickets can be re-opened by the submitter within a configurable window.

---

## Phase 19 — Global Search

### Stage 19.1 — Cross-Entity Search
- A global search bar is accessible from all portal pages for all roles.
- Search covers students, courses, offerings, faculty, documents, and announcements.
- Results are scoped to the caller's role and department assignments (Admin sees only their dept data).

### Stage 19.2 — Search Result Navigation
- Each result links directly to the relevant portal page (student profile, course detail, document, etc.).
- Recent searches are persisted per session for quick re-access.
- SuperAdmin search is unrestricted across all departments and entities.

---

## Phase 20 — External Integrations

### Stage 20.1 — Library System Integration
- Portal links to an external library catalogue (configurable URL and auth method by SuperAdmin).
- Students and Faculty can browse and request library resources from within the portal.
- Loan status and due dates are surfaced on the student dashboard.

### Stage 20.2 — Government / Accreditation Reporting
- SuperAdmin can trigger structured data exports (enrollment counts, completion rates, demographic summaries) in formats required by regulatory bodies.
- Export templates are configurable; output is downloadable as CSV or PDF.
- All export events are logged in the audit trail.

---

## Phase 21 — Graduation Workflow

### Stage 21.1 — Graduation Application Flow
- Students who meet the degree audit eligibility criteria can submit a graduation application.
- Application enters an approval workflow: Faculty validates results → Admin approves → SuperAdmin confirms.
- Each approver receives a notification when the application reaches their stage.

### Stage 21.2 — Certificate Generation
- Upon final approval, the system generates a graduation certificate PDF (configurable template set by SuperAdmin).
- Certificate is stored against the student record and downloadable from the student portal.
- Admin can re-issue or revoke a certificate with a documented reason and audit log entry.

---

## Implementation Priority Notes

| Phase | Feature | Estimated Complexity | Suggested Order |
|---|---|---|---|
| 12 | LMS (Content + Video + Forums + Announcements) | High | 4th |
| 13 | Degree Audit System | Medium | 3rd |
| 14 | Enrollment Rules Engine | Medium | 2nd |
| 15 | Faculty Grading System | Medium | 2nd |
| 16 | Academic Calendar | Low–Medium | 1st |
| 17 | Study Planner | Medium | 5th |
| 18 | Helpdesk / Ticketing | Low–Medium | 1st |
| 19 | Global Search | Low | 1st |
| 20 | External Integrations | High | 6th |
| 21 | Graduation Workflow | Medium | 3rd |

**Recommended kick-off order:** Phase 16 (Calendar) → Phase 19 (Search) → Phase 18 (Helpdesk) → Phase 14 (Enrollment Rules) → Phase 15 (Grading) → Phase 13 (Degree Audit) → Phase 21 (Graduation) → Phase 12 (LMS) → Phase 17 (Study Planner) → Phase 20 (Integrations).
