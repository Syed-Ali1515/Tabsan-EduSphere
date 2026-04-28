# Database Schema Documentation
## University Portal & License Creation Tool

**Version:** 1.0  
**Aligned With PRD:** v1.6  
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

## 18. Design Guarantees

- License contains **no university identity**
- License file is cryptographically protected
- License keys cannot be altered
- Academic data is never deleted
- Module-based feature control supported
- Fully offline-capable license validation

---