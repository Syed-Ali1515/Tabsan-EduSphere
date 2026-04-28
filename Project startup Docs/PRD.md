# Product Requirements Document (PRD)
## University Portal (License-Based, Department-Oriented System)

**Version:** 1.6 (Final)  
**Status:** Approved  
**Prepared By:** Product Team  
**Last Updated:** 29 April 2026  

---

## 1. Purpose & Vision

### 1.1 Purpose
The University Portal is a secure, license-based web platform designed to manage academic, administrative, and communication processes for universities. The system uses a **unique, cryptographically protected license key** to control access and features, without embedding any institution-identifying data inside the license itself.

### 1.2 Vision
To deliver a scalable, long‑term university management system that preserves academic records permanently, enforces access through strong licensing, and enhances productivity with AI assistance.

---

## 2. Goals & Objectives

### 2.1 Business Goals
- Enable commercial distribution via licensing
- Support yearly and permanent license models
- Prevent license tampering or unauthorized reuse
- Reduce administrative workload
- Preserve academic records across all semesters permanently

### 2.2 Product Objectives
- Enforce license validity using secure cryptographic verification
- Ensure licenses cannot be edited or forged
- Provide role-based and department-based governance
- Deliver AI-assisted academic and administrative workflows
- Maintain high performance, security, and accessibility

---

## 3. User Roles

- Student
- Faculty
- Admin (All departments access)
- Super Admin (System authority)
- External users (limited access)

---

## 4. Scope

### 4.1 In Scope
- Admissions & enrollment
- Student Information System (SIS)
- Assignments & quizzes
- Notifications & alerts
- Final Year Project (FYP) management
- AI chatbot
- Reporting & analytics
- License management & enforcement
- Multi-theme UI system (15+ themes)

### 4.2 Out of Scope
- Native mobile apps
- Alumni management
- Career marketplace
- Automated grading

---

## 5. Licensing Model

### 5.1 License Types

#### 5.1.1 Yearly License
- Valid for 12 months from activation
- Renewal required
- Includes:
  - Feature updates
  - Security updates
  - AI improvements
  - New UI themes
- Grace period after expiry
- Read-only access once expired

#### 5.1.2 Permanent License
- One-time activation
- Never expires
- No re-activation required
- Retains all features available at activation
- Academic data always accessible
- Optional paid upgrades for major versions

---

## 6. License Creation & Protection System

### 6.1 License Creation Tool

#### Description
A **dedicated License Creation Tool** is used to generate all licenses.  
Only licenses created by this tool are valid.

#### Functional Requirements
- Generate a **unique license key**
- License key contains:
  - Encrypted license type
  - Encrypted lifecycle rules (yearly/permanent)
  - Cryptographic signature
- License contains **NO**:
  - University name
  - University ID
  - Domain
  - Hardware identifiers
- Licenses are:
  - Machine-readable only
  - Not human-editable
  - Signed using a private key
- License revocation and regeneration supported
- Full audit logging

✅ The License Creation Tool is accessible **only to Super Admin / Vendor**

---

### 6.2 License File Security (Best Practice)

#### Protection Rules
- License file is:
  - Digitally signed
  - Encrypted
  - Obfuscated
- Signature verification uses an embedded public key
- Any alteration invalidates the license
- License stored in OS-protected directory
- Validation is backend-only

✅ No fields inside the license can be manually edited without breaking validation

---

### 6.3 License Validation Rules

- License validation occurs:
  - On system startup
  - On Super Admin login
  - Daily background validation
- Offline validation supported via cryptographic signature
- No online verification required
- Graceful degradation on license expiry

---

### 6.4 Graceful Degradation

If license expires or becomes invalid:
- Allowed:
  - View student data
  - Export transcripts
  - View historical records
- Blocked:
  - Creating assignments
  - New registrations
  - Data modifications

✅ Academic history is **never locked, deleted, or corrupted**

---

## 7. Authentication & User Management

### Student Signup
- Signup using official **registration number**
- Registration number must:
  - Exist in system
  - Be unique
- Auto-link student to:
  - Department
  - Program
  - Current semester

---

## 8. Role-Based Functional Requirements

### 8.1 Student
- View full academic history (all semesters)
- Submit assignments and quizzes
- View attendance, grades, transcripts
- Receive notifications
- View FYP meeting schedules
- Interact with AI chatbot
- Select UI theme

---

### 8.2 Faculty
- Assigned to department(s)
- Create assignments and quizzes
- Grade and provide feedback
- View student history (department-restricted)
- Schedule FYP meetings:
  - Date & time
  - Department
  - Room number
  - Panel participants
- Send notifications to students

---

### 8.3 Admin
- View all departments
- Access complete student academic history
- Generate university-wide reports
- Send notifications
- No role or system configuration access

---

### 8.4 Super Admin
- Create roles and users
- Assign departments
- Manage licenses
- Configure system settings
- Control AI chatbot
- Manage UI themes
- View audit logs

---

## 9. Department-Based Structure

- Department is a core entity
- Faculty, courses, programs, and assignments are department-linked
- Admins see all departments
- Faculty restricted to assigned department(s)

---

## 10. Assignments & Quizzes

### Teachers
- Create assignments/quizzes per course and semester
- Set deadlines and rules
- Publish and notify students

### Students
- View assignments grouped by:
  - Semester → Course → Assignment
- Submit work
- View feedback and grades

---

## 11. Student Academic History

- Complete semester-by-semester record
- Includes:
  - Enrollment
  - Assignments
  - Quizzes
  - Grades
  - Attendance
  - Project history
- Never deleted or overwritten

---

## 12. Notifications & Alerts

- Results
- Assignments/quizzes
- Low attendance warnings
- FYP meetings
- Admin notices
- Delivered via dashboard (+ optional email)

---

## 13. Final Year Project (FYP)

- Faculty schedule meetings
- Define:
  - Location (Department + Room)
  - Panel members
- Students receive notifications and see details on dashboard

---

## 14. AI Chatbot

- Role-aware, department-aware
- Assists with:
  - Assignments
  - Results
  - Attendance
  - FYP meetings
- Escalation support
- Multilingual
- License-aware feature limits

---

## 15. UI Themes & Personalization

- Minimum **15 themes**
- Light & Dark included
- High-contrast accessibility themes
- User-selectable
- Persistent across sessions
- AI chatbot inherits active theme

---

## 16. Security & Compliance

- Role-based access control
- Encrypted data at rest and in transit
- Secure license validation
- Full audit logs
- GDPR / Australian Privacy Act
- WCAG 2.1 compliance

---

## 17. Non-Functional Requirements

- Page load < 3 seconds
- Chatbot response < 2 seconds
- 10,000+ concurrent users
- 99.9% uptime

---

## 18. Technical Overview

- Frontend: React / Angular
- Backend: ASP.NET Core / Node.js
- Database: PostgreSQL / SQL Server
- REST APIs
- AI integration (LLM)
- Cloud-ready (Azure / AWS)

---

## 19. Approval

| Name | Role | Signature | Date |
|-----|------|----------|------|
| | Product Owner | | |
| | University Representative | | |
| | Technical Lead | | |

---