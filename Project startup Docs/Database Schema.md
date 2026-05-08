# Database Schema Documentation
## University Portal & License Creation Tool

**Version:** 1.2  
**Aligned With PRD:** v1.33  
**Purpose:** Define database schemas for the University Portal Application and the License Creation Tool  

---

# PART 1: UNIVERSITY PORTAL APPLICATION DATABASE

---

## 1. Identity & Access Control

### users
Stores all system users (students, faculty, admins, super admins).

- id (UUID, PK)
- username (unique)
- email (unique, nullable)
- password_hash
- role_id (FK → roles.id)
- department_id (FK → departments.id, nullable)
- is_active
- created_at
- updated_at
- last_login_at

---

### roles
Predefined system roles.

- id (PK)
- name (Student, Faculty, Admin, SuperAdmin)
- description
- is_system_role

---

## 2. Department & Academic Structure

### departments
Core academic departments.

- id (PK)
- name
- code (unique)
- is_active
- created_at

---

### programs
Academic programs offered by a department.

- id (PK)
- department_id (FK)
- name
- code
- duration_years
- is_active

---

### courses
Courses offered under programs.

- id (PK)
- department_id (FK)
- program_id (FK)
- code
- title
- credit_hours
- is_active

---

### semesters
Academic semesters.

- id (PK)
- name (e.g., Fall 2025)
- start_date
- end_date
- is_active

---

## 3. Student Information System (Permanent Records)

### students
Student core identity.

- id (UUID, PK)
- user_id (FK → users.id)
- registration_number (unique)
- program_id (FK)
- current_semester_id (FK)
- admission_date
- status

---

### student_semester_records
Complete academic history (never deleted).

- id (PK)
- student_id (FK)
- semester_id (FK)
- gpa
- cgpa
- academic_status
- created_at

---

### student_course_enrollments
Tracks course enrollment per semester.

- id (PK)
- student_id (FK)
- course_id (FK)
- semester_id (FK)
- status (enrolled, dropped, completed)

---

## 4. Assignments & Submissions

### assignments
Teacher-created assignments.

- id (PK)
- course_id (FK)
- faculty_id (FK → users.id)
- semester_id (FK)
- title
- description
- due_date
- is_published
- created_at

---

### assignment_submissions
Student submissions.

- id (PK)
- assignment_id (FK)
- student_id (FK)
- file_path
- submitted_at
- grade
- feedback

---

## 5. Quizzes & Assessments

### quizzes
Quiz definitions.

- id (PK)
- course_id (FK)
- semester_id (FK)
- title
- start_time
- end_time
- max_attempts

---

### quiz_attempts
Student quiz attempts.

- id (PK)
- quiz_id (FK)
- student_id (FK)
- score
- attempted_at

---

## 6. Attendance Management

### attendance_records
Daily attendance tracking.

- id (PK)
- student_id (FK)
- course_id (FK)
- semester_id (FK)
- attendance_date
- status (present, absent)

---

## 7. Results & Grades

### grades
Final course results.

- id (PK)
- student_id (FK)
- course_id (FK)
- semester_id (FK)
- grade

---

## 8. Notifications System

### notifications
Central notification messages.

- id (PK)
- title
- message
- type (assignment, quiz, result, attendance, fyp)
- created_by (FK → users.id)
- created_at

---

### notification_recipients
Per-user delivery tracking.

- id (PK)
- notification_id (FK)
- user_id (FK)
- is_read
- read_at

---

## 9. Final Year Project (FYP)

### fyp_projects
Student project records.

- id (PK)
- student_id (FK)
- title
- semester_id (FK)

---

### fyp_meetings
Scheduled project meetings.

- id (PK)
- fyp_project_id (FK)
- meeting_datetime
- department_id (FK)
- room_number

---

### fyp_panel_members
Faculty panel participants.

- id (PK)
- fyp_meeting_id (FK)
- faculty_id (FK)

---

## 10. UI Themes & Personalization

### themes
Available UI themes.

- id (PK)
- name
- is_dark
- is_accessible
- is_active

---

### user_theme_preferences
User-selected themes.

- id (PK)
- user_id (FK)
- theme_id (FK)

---

## 11. Module Control (Feature Toggles)

### modules
All selectable modules.

- id (PK)
- key (assignment, quiz, ai_chat, fyp, etc.)
- name
- is_mandatory

---

### module_status
Super Admin-controlled activation.

- id (PK)
- module_id (FK)
- is_active
- activated_at

---

## 12. License State (Application Side Only)

### license_state
Stores validated license status (no license creation).

- id (PK)
- license_hash
- license_type (yearly, permanent)
- status (active, expired)
- activated_at
- expires_at (nullable)

---

# PART 2: LICENSE CREATION TOOL DATABASE

*(Separate system, separate database)*

---

## 13. License Storage

### licenses
Only hashed keys stored.

- id (PK)
- license_key_hash (unique)
- license_type (yearly, permanent)
- issued_at
- expires_at (nullable)
- status (active, revoked)

---

## 14. License Issuance Logs

### license_issuance_logs
Tracks who generated licenses.

- id (PK)
- license_id (FK)
- issued_by_user
- issued_at
- notes

---

## 15. License Revocation

### license_revocations
Revoked licenses history.

- id (PK)
- license_id (FK)
- revoked_at
- reason

---

## 16. License Tool Users

### license_tool_users
Admins of license tool.

- id (PK)
- username
- password_hash
- role (SuperAdmin)
- created_at

---

## 17. License Audit Logs

### license_audit_logs
Full traceability.

- id (PK)
- action (create, revoke, view)
- license_id (FK)
- performed_by
- performed_at
- ip_address

---

## 18. Academic Calendar (Phase 12)

### academic_deadlines
Named academic deadlines and key dates attached to a semester. Used by the Academic Calendar portal page and the `DeadlineReminderJob` background service.

- id (UUID, PK)
- semester_id (FK → semesters.id, cascade delete)
- title (nvarchar 200, required)
- description (nvarchar 1000, nullable)
- deadline_date (datetime2)
- reminder_days_before (int, default 3 — 0 means day-of reminder only)
- is_active (bool, default true)
- last_reminder_sent_at (datetime2, nullable — set by DeadlineReminderJob when notification is dispatched)
- is_deleted (bool — soft delete via global query filter)
- deleted_at (datetime2, nullable)
- created_at (datetime2)
- updated_at (datetime2)
- row_version (rowversion / timestamp — optimistic concurrency)

**Indexes:**
- `IX_academic_deadlines_semester` on `semester_id`
- `IX_academic_deadlines_date_active` on `(deadline_date, is_active)`

**EF Migration:** `20260507_Phase12AcademicCalendar`

---

## 19. Global Search (Phase 13)

Phase 13 introduces no new database tables. All search queries execute against existing tables using EF Core LINQ joins:

| Entity searched | Table(s) queried |
|---|---|
| Students | `student_profiles` JOIN `users` |
| Courses | `courses` |
| Course Offerings | `course_offerings` JOIN `courses` JOIN `semesters` |
| Faculty | `users` JOIN `roles` (where `roles.name = 'Faculty'`) |
| Departments | `departments` |
| Student-enrolled offerings | `enrollments` JOIN `course_offerings` JOIN `courses` |

All queries respect global soft-delete query filters (`is_deleted = 0`) automatically.

Role-scoped filtering applied at the application service layer:
- **SuperAdmin** — all entities across all departments
- **Admin** — entities within their assigned departments
- **Faculty** — entities within their own department + their own course offerings
- **Student** — only their enrolled course offerings

**EF Migration:** None required (no schema changes)

---

## 18. Design Guarantees

- License contains **no university identity**
- License file is cryptographically protected
- License keys cannot be altered
- Academic data is never deleted
- Module-based feature control supported
- Fully offline-capable license validation

---

## 19. Implementation Conventions (ASP.NET + EF Core)

- Use GUID PKs for all user-facing and distributed entities (users, students, assignments, licenses)
- Use bigint PKs for high-volume append-only logs where sequential inserts are beneficial
- Add created_at, updated_at, and row_version (concurrency token) to mutable aggregates
- Use soft-delete columns (is_deleted, deleted_at) for operational entities; never soft-delete academic history tables
- Store all timestamps in UTC

---

## 20. Additional Core Tables Required for Build Readiness

### user_sessions
Tracks web/API sessions and refresh-token family state.

- id (UUID, PK)
- user_id (FK -> users.id)
- refresh_token_hash
- device_info
- ip_address
- expires_at
- revoked_at (nullable)

### faculty_department_assignments
Supports faculty mapped to one or more departments.

- id (PK)
- faculty_id (FK -> users.id)
- department_id (FK -> departments.id)
- is_primary

### course_offerings
Represents a course running in a specific semester and department context.

- id (PK)
- course_id (FK)
- semester_id (FK)
- department_id (FK)
- faculty_id (FK -> users.id)
- section
- capacity
- is_active

### registration_whitelist
Pre-approved registration numbers for controlled student signup.

- id (PK)
- registration_number (unique)
- program_id (FK)
- semester_id (FK)
- is_claimed
- claimed_by_student_id (FK, nullable)

### quiz_questions
Question bank entries tied to quizzes.

- id (PK)
- quiz_id (FK)
- question_text
- question_type (mcq, short, true_false)
- marks
- display_order

### quiz_question_options
Options for objective quiz questions.

- id (PK)
- quiz_question_id (FK)
- option_text
- is_correct

### quiz_attempt_answers
Submitted answers per attempt.

- id (PK)
- quiz_attempt_id (FK)
- quiz_question_id (FK)
- selected_option_id (FK, nullable)
- answer_text (nullable)
- awarded_marks

### transcript_exports
Tracks transcript generation history for compliance and auditability.

- id (PK)
- student_id (FK)
- exported_by (FK -> users.id)
- exported_at
- format (pdf, excel)

### audit_logs
Operational audit logs for privileged activities.

- id (bigint, PK)
- actor_user_id (FK -> users.id, nullable)
- action
- entity_name
- entity_id
- old_values_json
- new_values_json
- occurred_at
- ip_address

---

## 21. Constraint and Index Strategy

- users: unique indexes on username and email (filtered where email is not null)
- students: unique index on registration_number and unique index on user_id
- student_course_enrollments: unique composite index on (student_id, course_id, semester_id)
- attendance_records: unique composite index on (student_id, course_id, semester_id, attendance_date)
- assignment_submissions: unique composite index on (assignment_id, student_id)
- module_status: unique index on module_id to enforce single active status row
- notifications_recipients: index on (user_id, is_read)
- audit_logs: clustered index by occurred_at for time-range queries

---

## 22. Data Retention and Archival Rules

- Academic records: never deleted; archive to cold storage after policy threshold
- Audit logs: retain online for 24 months, archive for 7 years
- Notification delivery logs: retain online for 12 months, then archive
- Session and token records: purge expired and revoked entries after 90 days

---

## 23. Migration and Seeding Plan

- Baseline migration: identity, departments, SIS core, license_state
- Seed mandatory roles, modules, and default themes
- Seed Super Admin bootstrap user through secure deployment script
- Apply feature migrations per release train (v1, v1.1, v1.2)

---

## 24. Phase 20 LMS Tables (Migration `Phase20_LMS` — 2026-05-08)

### course_content_modules
Structured weekly learning modules created by faculty per `CourseOffering`.

- id (UUID, PK)
- offering_id (FK → course_offerings.id, ON DELETE CASCADE)
- title (varchar 300)
- week_number (int)
- body (nvarchar 50000)
- is_published (bool, default false)
- is_deleted (bool, default false)
- deleted_at (datetime, nullable)
- created_at / updated_at / row_version

### content_videos
Video attachments linked to a `CourseContentModule`.

- id (UUID, PK)
- module_id (FK → course_content_modules.id, ON DELETE CASCADE)
- title (varchar 300)
- storage_url (varchar 1000, nullable)
- embed_url (varchar 1000, nullable)
- duration_seconds (int, default 0)
- is_deleted (bool, default false)
- deleted_at (datetime, nullable)
- created_at / updated_at / row_version

### discussion_threads
Discussion threads opened within a `CourseOffering`.

- id (UUID, PK)
- offering_id (FK → course_offerings.id, ON DELETE CASCADE)
- title (varchar 500)
- author_id (FK → users.id, ON DELETE NO ACTION)
- is_pinned (bool, default false)
- is_closed (bool, default false)
- is_deleted (bool, default false)
- deleted_at (datetime, nullable)
- created_at / updated_at / row_version

### discussion_replies
Replies within a `DiscussionThread`.

- id (UUID, PK)
- thread_id (FK → discussion_threads.id, ON DELETE CASCADE)
- author_id (FK → users.id, ON DELETE NO ACTION)
- body (nvarchar 10000)
- is_deleted (bool, default false)
- deleted_at (datetime, nullable)
- created_at / updated_at / row_version

### course_announcements
Course-level announcements posted by faculty; triggers fan-out notification to enrolled students.

- id (UUID, PK)
- offering_id (FK → course_offerings.id, ON DELETE SET NULL, nullable)
- author_id (FK → users.id, ON DELETE NO ACTION)
- title (varchar 300)
- body (nvarchar 10000)
- posted_at (datetime)
- is_deleted (bool, default false)
- deleted_at (datetime, nullable)
- created_at / updated_at / row_version

---