# University Portal – Module Definition, Activation & Packaging

**Document Version:** 1.2 (Implementation Baseline)  
**Aligned With PRD Version:** 1.6  
**Audience:** Super Admin, University Decision Makers  
**Purpose:** Define selectable system modules, activation rules, and pricing packages  

---

## 1. Overview

The University Portal follows a **modular architecture**, allowing universities to choose which functional modules they require.

⚠️ **Important:**  
👉 **License handling is NOT a module.**  
Licenses are created externally and uploaded by the **Super Admin**.  
Modules become available or unavailable **based on license entitlements**, but licensing itself cannot be disabled.

---

## 2. Core System Capabilities (Not Modules)

The following are **mandatory system capabilities** and are **ALWAYS present**:

- Licensing enforcement (key upload & validation)
- Role-based access control (RBAC)
- Super Admin controls
- Security & encryption
- Audit logging (minimum)

✅ These **cannot be turned off** and are not selectable modules.

---

## 3. Module Management Rules

### 3.1 Super Admin Responsibilities
Only the **Super Admin** can:
- Activate or deactivate modules
- Control module availability based on license
- Hide or expose functionality to users
- Enable paid add-on modules

---

### 3.2 Module Activation Behavior

- ✅ **Active Module**
  - Visible in UI
  - APIs enabled
  - Data accessible by role & department

- ❌ **Inactive Module**
  - Hidden from UI
  - APIs disabled
  - Existing data preserved but inaccessible

✅ Module toggling does **not delete any data**

---

## 4. Mandatory Modules (Always Enabled)

These modules are required for the portal to function and **cannot be disabled**:

### 4.1 Authentication & User Management
- Login & logout
- User roles (Student, Faculty, Admin, Super Admin)
- Student signup via **registration number**
- Password & session management

---

### 4.2 Department Management
- Department creation
- Assign faculty & programs
- Department-based access control

---

### 4.3 Student Information System (SIS)
- Student profiles
- Enrollment records
- Semester tracking
- **Full academic history across all semesters**

---

## 5. Academic Modules (Selectable)

### 5.1 Course & Program Management
- Courses and programs
- Semester mapping
- Faculty assignment

---

### 5.2 Assignment Management
- Faculty-created assignments
- Deadlines & submission rules
- Student submissions
- Feedback & grading
- Semester-based grouping

---

### 5.3 Quiz & Assessment Module
- Timed quizzes
- Attempt limits
- Auto/manual grading

---

### 5.4 Attendance Management
- Attendance tracking
- Low attendance thresholds
- Automatic student notifications

---

### 5.5 Examination & Results
- Marks entry
- GPA / CGPA calculation
- Result publishing
- Transcript generation

---

## 6. Communication & Coordination Modules

### 6.1 Notifications Module
- Dashboard notifications
- Assignment & quiz alerts
- Result announcements
- Low attendance warnings
- Read/unread tracking

---

### 6.2 Final Year Project (FYP) Module
- Project allocation
- Meeting scheduling
- Location entry:
  - Department
  - Room number
- Panel member selection
- Student & panel notifications

---

## 7. Intelligence & Insights Modules

### 7.1 AI Chatbot Module
- Role-aware responses
- Department-aware context
- Helps with:
  - Assignments
  - Results
  - Attendance
  - FYP schedules
- License-controlled access

---

### 7.2 Reporting & Analytics
- Student performance reports
- Department analytics
- Assignment & quiz statistics
- Export to PDF / Excel

---

## 8. UI & Experience Modules

### 8.1 Theme & Personalization Module
- Minimum **15 themes**
- Light & Dark modes
- High-contrast accessibility themes
- User-level theme selection
- Admin default theme control
- AI chatbot inherits theme

---

## 9. Compliance & Governance Modules

### 9.1 Advanced Audit & Logs
- Detailed activity logging
- Data access tracking
- Exportable audit trails
- Compliance reporting

---

## 10. Module Dependency Summary

| Module | Can Be Disabled |
|------|---------------|
| Authentication & Users | ❌ No |
| Departments | ❌ No |
| SIS | ❌ No |
| Courses & Programs | ✅ Yes |
| Assignments | ✅ Yes |
| Quizzes | ✅ Yes |
| Attendance | ✅ Yes |
| Exams & Results | ✅ Yes |
| Notifications | ✅ Yes |
| FYP | ✅ Yes |
| AI Chatbot | ✅ Yes |
| Reports & Analytics | ✅ Yes |
| Themes & Personalization | ✅ Yes |
| Advanced Audit | ✅ Yes |

---

## 11. Basic Package (Included by Default)

The **Basic Package** includes:

- Authentication & User Management
- Department Management
- Student Information System (SIS)
- Course & Program Management
- Assignment Management
- Examination & Results
- Basic Notifications
- Core Themes (Light & Dark)
- Core Audit Logging

✅ Suitable for standard academic operations

---

## 12. Optional Paid Modules (Add-On)

Modules that can be enabled later (paid):

- Quiz & Assessment Module
- Attendance Management
- AI Chatbot
- Final Year Project (FYP)
- Advanced Reporting & Analytics
- Extended Theme Pack (15+ themes)
- Advanced Audit & Compliance
- Multi-campus support

---

## 13. Upgrade & Expansion Rules

- Modules can be activated anytime by Super Admin
- No system reinstall required
- Historical data becomes visible upon activation
- License upgrade required for paid modules

---

## 14. Final Notes

- Licensing is handled exclusively by Super Admin
- Universities never interact with license creation
- Modules ensure flexibility, scalability, and cost control
- Academic records are never deleted

---

## 15. ASP.NET Implementation Mapping

### 15.1 Module-to-Bounded-Context Mapping

| Functional Module | Bounded Context | Primary API Area |
|------|------|------|
| Authentication & Users | Identity and Access | /api/v1/auth, /api/v1/users |
| Departments | Academic Core | /api/v1/departments |
| SIS | Student Lifecycle | /api/v1/students, /api/v1/enrollments |
| Courses & Programs | Academic Core | /api/v1/programs, /api/v1/courses |
| Assignments | Learning Delivery | /api/v1/assignments |
| Quizzes | Learning Delivery | /api/v1/quizzes |
| Attendance | Learning Delivery | /api/v1/attendance |
| Exams & Results | Assessment and Results | /api/v1/results |
| Notifications | Notifications | /api/v1/notifications |
| FYP | FYP Management | /api/v1/fyp |
| AI Chatbot | AI Services | /api/v1/ai/chat |
| Reports & Analytics | Reporting | /api/v1/reports |
| Themes & Personalization | UX Personalization | /api/v1/themes |
| Advanced Audit | Audit and Compliance | /api/v1/audit |

---

### 15.2 Dependency Rules

- Courses & Programs depends on Departments
- Assignments depends on Courses & Programs and SIS
- Quizzes depends on Courses & Programs and SIS
- Attendance depends on Courses & Programs and SIS
- Exams & Results depends on Courses & Programs and SIS
- FYP depends on SIS and Notifications
- AI Chatbot depends on Licensing, RBAC, and at least one academic module
- Reporting depends on data-producing modules (Assignments, Quizzes, Attendance, Results)

If a dependency module is inactive, dependent module endpoints must return module-inactive responses.

---

### 15.3 Technical Activation Rules

- UI menu rendering checks module entitlements before route exposure
- API endpoints enforce module entitlement through policy filters
- Background jobs for a module run only when module is active
- Deactivation hides UI immediately and blocks writes; read access follows role and policy
- Reactivation restores feature access without data migration or data loss

---

### 15.4 Module State Contract

Module state should expose:

- module_key
- is_active
- source (mandatory, license, manual)
- last_changed_at
- changed_by

This contract enables auditability and deterministic behavior across UI, API, and jobs.

---

### 15.5 Packaging and Release Alignment

- v1 package: mandatory modules plus Courses, Assignments, Results, Notifications
- v1.1 package: Quizzes, Attendance, FYP, AI Chatbot baseline
- v1.2 package: Reporting, Advanced Audit, extended themes, multi-campus foundations

---