# University Portal – Module Definition, Activation & Packaging

**Document Version:** 1.1 (Final)  
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
``