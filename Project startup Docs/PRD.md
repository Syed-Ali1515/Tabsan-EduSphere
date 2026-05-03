# Product Requirements Document (PRD)
## University Portal (License-Based, Department-Oriented System)

**Version:** 1.15 (Phase 2 Complete - All Stages 2.1–2.3 Lookup Data & CRUD Complete)  
**Status:** Approved  
**Prepared By:** Product Team  
**Last Updated:** 4 May 2026  

---

## 0. Implementation Update Log

### 2026-05-04 — Phase 2 Stages 2.1–2.3 Complete (Data Visibility & CRUD Entry Points)
- **Stage 2.1 (Timetable Data Binding - COMPLETE)**
  - Fixed TimetableRepository query methods with proper EF Include statements for Building, Department, AcademicProgram, Semester
  - Faculty and Student My Timetable endpoints now render complete calendar data
  
- **Stage 2.2 (Lookup Data Visibility - COMPLETE)**
  - Fixed StudentProfileRepository to include Program and Department for accurate student profile display
  - Updated StudentController.GetAll() to return student names, program names, department names via related entities
  - Added CourseRepository.GetOfferingsByDepartmentAsync() for department-scoped offering retrieval
  - Refactored CourseController.GetOfferings() to support both ?semesterId and ?departmentId query parameters
  - Updated CourseController.GetAll() to include DepartmentName for course catalogue
  
- **Stage 2.3 (CRUD Entry Points - COMPLETE)**
  - Added 4 new CourseOffering lifecycle endpoints: maxenrollment update, close/reopen enrollment, soft-delete
  - Enhanced portal pages with create entry points and Bootstrap modals:
    - Students page: Add Student modal with Registration Number, Program, Department, Admission Date
    - Departments page: Add Department modal with Code, Name
    - Courses page: Add Course and Add Offering modals with all required fields
  - All modal forms include role-based visibility (Admin/SuperAdmin only)
  - Leveraged existing StudentController.Create, DepartmentController.Create/Update/Delete, CourseController.Create/Update/Delete endpoints
  
- **Build Status:** Verified (0 errors, all 3 stages successfully compiled)
- **Portal Enhancement:** Users can now create Students, Departments, Courses, and CourseOfferings directly from portal pages
- **API Completeness:** CourseOffering now supports full lifecycle management

### 2026-05-04 — Phase 2 Stages 2.1–2.2 Data Binding Complete
- **Stage 2.1 (Timetable Data Binding):**
  - Fixed TimetableRepository query methods to properly load Building, Department, AcademicProgram, and Semester navigation properties.
  - Faculty and Student My Timetable endpoints now return complete data for calendar display without null reference errors.
  - Test validation confirmed: 1 published timetable with 2 entries for CS dept, assigned to faculty.test.
  
- **Stage 2.2 (Lookup Data Visibility):**
  - Fixed StudentProfileRepository to include Program and Department navigation properties for accurate student profile data.
  - Updated StudentController.GetAll() to return student names, program names, and department names via related entity mapping.
  - Added CourseRepository.GetOfferingsByDepartmentAsync() method for department-scoped offering retrieval.
  - Refactored CourseController.GetOfferings() endpoint to accept both ?semesterId and ?departmentId query parameters for flexible filtering.
  - Updated CourseController.GetAll() to include department names for course catalogue display.
  - Build verified: 0 errors, all portal views ready to render with complete related entity data.

### 2026-05-03 — Final-Touches Phase 1 Complete
- Stabilized sidebar rendering behavior for portal pages by introducing resilient dynamic-menu loading with session cache fallback in web layout.
- Implemented grouped sidebar presentation for role-based navigation: Overview, Faculty Related, Student Related, Finance Related, Settings.
- Added `portal_settings` key-value table and `PortalBrandingService` to support configurable branding (university name, brand initials, portal subtitle, footer text).
- Added `DashboardSettings` page allowing SuperAdmin to set and save all branding values.
- Layout brand section (sidebar mark, name, subtitle, footer) now reads from DB settings with session cache fallback.
- Phase 1 validation passed: grouped sidebar, stable navigation, Dashboard Settings form, default values, live preview, and footer text all confirmed.

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
- Finance (Payment and receipt management)
- External users (limited access)

---

## 4. Scope

### 4.1 In Scope
- Admissions & enrollment
- Student Information System (SIS)
- Assignments & quizzes
- Result calculation configuration and automated GPA / CGPA processing
- Notifications & alerts (dashboard + email)
- Final Year Project (FYP) management
- AI chatbot
- Reporting & analytics
- License management & enforcement
- Multi-theme UI system (15+ themes, per-user)
- **Tabsan-Lic** standalone license creation tool
- Student lifecycle: graduation, semester promotion/failure, dropout, department transfer
- Finance & payment receipts (with optional online payment gateway)
- CSV import for student registration whitelist
- Role-based sidebar navigation with per-role menus and sub-menus
- System Settings: License, Theme, Report, Module, Sidebar management
- Departments administration: degrees, semesters, subjects, timetable (PDF/Excel)
- Teacher modification requests with admin approval workflow
- Account lockout and admin/super-admin password reset
- OWASP Top 10 security hardening
- Database views and stored procedures
- Free/open-source email API integration
- Mobile-responsive, WCAG 2.1 AA accessible UI

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
- View full academic history (all semesters — read-only for past semesters)
- Submit assignments and quizzes for active semester
- View attendance, grades, transcripts
- Receive notifications (dashboard + email)
- View FYP meeting schedules
- Interact with AI chatbot
- Select personal UI theme
- Self-edit password, email address, and mobile number only
- Submit admin change request for all other profile fields
- Upload payment receipt proof and mark as "Payment Submitted"
- Download timetable (PDF/Excel)
- **Graduated students**: full read-only dashboard — view and download only; no create/edit/submit
- **Students with unpaid fees**: read-only mode until Finance confirms payment
- **Inactive students**: blocked from login; all data preserved

---

### 8.2 Faculty
- Assigned to department(s)
- Create assignments and quizzes
- Grade and provide feedback
- View student history (department-restricted)
- Schedule FYP meetings (date, time, department, room, panel participants)
- Send notifications to students
- Submit attendance/result modification requests with a mandatory reason field
- Modification takes effect only after Admin approval
- Self-edit password, email address, and mobile number only; submit admin change request for other fields

---

### 8.3 Admin
- View all departments
- Access complete student academic history
- Generate university-wide reports
- Send notifications
- Manage departments: create/edit departments, degree programs, semesters, subjects, timetables
- Mark students as Graduated (checkbox list by department)
- Mark semester as completed or failed per student (with subject-level selection)
- Auto-promote students to next semester on completion
- Mark students as Inactive (dropout/leave)
- Transfer student to another department or change program
- Update student/faculty profiles on change request
- Approve or reject teacher attendance/result modification requests
- Enter or import CSV sheet of newly registered students to whitelist
- Create and manage payment receipts for students
- Confirm payment received (status → Paid)
- Configure Result Calculation rules: GPA-to-score mappings and assessment component weightages
- Unlock and reset passwords for non-admin locked accounts
- View license status (read-only)
- No role or system configuration access

---

### 8.4 Super Admin
- All Admin capabilities, plus:
- Create roles and users
- Assign departments
- Manage and upload licenses (via System Settings → License Update)
- Configure system settings: modules, reports, themes
- Control AI chatbot and online payment gateway toggle
- Manage UI themes
- View and export audit logs
- Reset passwords for any locked account including Admin accounts
- Manage Tabsan-Lic license generation (vendor tool, separate app)

### 8.5 Finance
- Create and upload payment receipts for students
- View payment status per student
- Confirm payment received → mark as Paid
- Cannot access academic records

### 8.6 External Users
- Limited read-only access as defined by admin configuration

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

### 10.1 Result Calculation & GPA Automation

#### Section 1: GPA-to-Score Mapping
- Admin configures a sidebar menu named **Result Calculation**
- The first section contains repeatable rows with two text boxes:
  - GPA
  - Score threshold / minimum score
- Example mappings:
  - GPA `2.0` = Score `60`
  - GPA `2.5` = Score `65`
- Admin can use:
  - **Add Row** to append another GPA/Score pair
  - **Save** to persist all rows to the database

#### Section 2: Assessment Component Weightage
- The second section contains repeatable rows for subject result components and their score contributions
- Admin defines how each subject total score is composed, for example:
  - Quizzes = 20
  - Midterms = 30
  - Finals = 50
- The configured weightages must total 100 before the configuration can be saved
- The configuration is stored in the database and used by all result-entry workflows

#### Automatic GPA, SGPA, and CGPA Processing
- When faculty enter quiz, midterm, or final marks, the system automatically recalculates the subject total using the saved component weightages
- The system automatically determines subject GPA using the saved GPA-to-score mapping
- Once all subjects for a semester are fully marked for a student, the system automatically calculates and stores:
  - Semester GPA (SGPA)
  - Total cumulative GPA (CGPA)
- Recalculation must also run whenever an existing mark is edited through an approved modification workflow
- All recalculation events must be auditable

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
- **Per-user selection** — theme applies to the individual user, not the role
- Persistent across sessions
- AI chatbot inherits active theme
- Admin can set a system default theme; individual users can override it

---

## 16. Student Lifecycle Management

### 16.1 Graduation
- Admin marks students as Graduated via "Graduated Students" menu (per department, checkbox list of final-semester students)
- Graduated student dashboard becomes permanently read-only: view and download only

### 16.2 Semester Completion & Promotion
- Admin marks semester as completed per student; selects passed/failed subjects at subject level
- Students with all subjects passed: auto-promoted to next semester
- Students with failed subjects: status = "Completed with Failed Subjects"; failed subject list recorded
- Admin can mark semester as fully Failed: student repeats the semester; added to re-enrollment list
- Previous semesters always visible to students in read-only form

### 16.3 Student Status Changes
- Inactive: student blocked from login; all data preserved indefinitely
- Department/program transfer: admin opens student profile and updates department, program, and semester
- Admin change request workflow for name, address, and other non-self-editable fields

### 16.4 Student Signup via Registration Number
- Admin enters or imports a CSV of registration numbers into the whitelist before semester
- Student signup checks: registration number exists in whitelist + no duplicate account exists
- Duplicate account error: "An account already exists with this Registration Number. Please contact your admin for further details."

---

## 17. Finance & Payments

- Finance role creates fee receipts (amount, description, due date) per student
- Student uploads proof of payment and marks as "Payment Submitted"
- Finance confirms → status = **Paid**
- Until Paid: student account operates in read-only mode
- **Online payment gateway** (card / bank): disabled by default; Super Admin toggles on/off via Module Settings
- All payment records stored permanently; no deletion

---

## 18. System Settings

Accessible from the top navigation as a dedicated "Settings" menu:

| Sub-Menu | Access | Description |
|---|---|---|
| License Update | Super Admin (upload); Admin (view) | Upload `.tablic` license file; view status table (Status, Expiry Date, Date Updated, Remaining Days) |
| Theme Settings | All users | Per-user theme picker with preview |
| Report Settings | Super Admin only | Activate/deactivate reports per role; table: SR#, Report Name, Purpose, Roles (multi-select) |
| Module Settings | Super Admin only | Activate/deactivate modules per role; table: SR#, Module Name, Purpose, Roles (multi-select), Status (Active/Inactive) |
| Sidebar Settings | Super Admin only | Configure sidebar navigation visibility per role; table: SR#, Name, Purpose, Roles (checkbox list), Status (Active/Inactive). Click any top-level menu to reveal its sub-menus in the panel below. Super Admin always retains full access regardless of settings. |

---

## 19. Tabsan-Lic — License Creation Tool

- Standalone .NET application; separate from EduSphere
- Generates unique `VerificationKey` per license (one-time use; permanently invalidated after consumption)
- Operator selects expiry: 1 year / 2 years / 3 years / Permanent
- License file (`.tablic`) is AES-256 encrypted + RSA-2048 signed — machine-readable only
- Any modification to the file invalidates the signature
- EduSphere imports `.tablic`, verifies signature, applies license, marks VerificationKey as consumed
- Unlimited keys can be generated; each logged with timestamp and expiry choice
- EduSphere notifies Admin/Super Admin 5 days before license expiry

---

## 20. Security Requirements

- OWASP Top 10 compliance mandatory
- HTTPS-only; HSTS, CSP, X-Frame-Options, X-Content-Type-Options headers enforced
- Password policy: minimum 12 characters, uppercase + lowercase + digit + special character required; no common passwords; no last 5 reuse; hashed with Argon2id
- Rate limiting on authentication and sensitive endpoints
- Account lockout after 5 consecutive failed login attempts (configurable)
- Admin or Super Admin resets non-admin locked accounts; only Super Admin resets Admin accounts
- Dependency vulnerability scanning in CI; zero critical/high CVEs
- Penetration test sign-off before production release

---

## 21. Performance & Infrastructure Requirements

- Page load < 3 seconds; core dashboard p95 < 200 ms under load
- Chatbot response < 2 seconds
- 10,000+ concurrent users
- 99.9% uptime SLA
- SQL Views for high-traffic read patterns; Stored Procedures for complex write batches
- Horizontal scaling-ready (stateless API layer)

---

## 22. Email Integration

- Free/open-source transactional email via `IEmailSender` abstraction (MailKit SMTP, SendGrid free tier, or self-hosted)
- Email notifications for: results, assignment deadlines, low attendance, license expiry, password reset, account unlock
- All outbound email attempts logged with status

---

## 23. Mobile & Accessibility

- Responsive UI: tested at 360 px, 768 px, 1280 px viewports
- WCAG 2.1 AA compliance
- Touch-friendly controls (44×44 px minimum tap targets)
- Lighthouse score ≥ 90 for Performance, Accessibility, Best Practices on core pages

---

## 24. Non-Functional Requirements

- Page load < 3 seconds
- Chatbot response < 2 seconds
- 10,000+ concurrent users
- 99.9% uptime

---

## 25. Technical Overview

- Frontend: ASP.NET Core MVC + Razor (Web project); Bootstrap 5 responsive layout
- Backend: ASP.NET Core 8 Web API
- Database: SQL Server with EF Core 8; Views and Stored Procedures for performance
- REST APIs with JWT Bearer authentication
- AI integration (LLM via abstracted `IAiChatService`)
- Email: `IEmailSender` abstraction over MailKit/SendGrid
- Cloud-ready (Azure / AWS); Docker-compatible

---

## 26. Approval

| Name | Role | Signature | Date |
|-----|------|----------|------|
| | Product Owner | | |
| | University Representative | | |
| | Technical Lead | | |

---

## 20. Implementation Architecture Baseline (ASP.NET)

### 20.1 Target Solution Style
- Modular monolith for v1, with clean boundaries and migration path to services
- Backend stack: ASP.NET Core 8 Web API
- UI stack: ASP.NET Core MVC with Razor views and selective client-side enhancement
- Data access: Entity Framework Core with SQL Server as default provider
- Background jobs: Hosted Services for scheduled validation and notifications

### 20.2 Proposed Solution Layers
- Presentation: Web UI, REST API controllers, auth endpoints
- Application: Use cases, command/query handlers, validation, orchestration
- Domain: Entities, value objects, domain rules, domain events
- Infrastructure: EF Core, repositories, file storage, email/SMS adapters, audit sinks

### 20.3 Bounded Contexts
- Identity and Access
- Academic Core (departments, programs, courses, semesters)
- Student Lifecycle (profiles, enrollment, records)
- Learning Delivery (assignments, quizzes, attendance)
- Assessment and Results
- FYP Management
- Notifications
- Licensing and Entitlements
- Audit and Reporting

### 20.4 API and Contract Standards
- Versioned APIs under /api/v1
- Consistent envelope for success and error responses
- RFC7807-compatible problem details for validation and runtime errors
- Idempotency support for critical write operations (license activation, results publish)

### 20.5 Security Architecture
- ASP.NET Core Identity for authentication and password policies
- JWT bearer tokens for API access and secure cookies for web sessions
- Role-based authorization plus policy checks for department scoping
- Encryption at rest for sensitive columns and TLS in transit
- Centralized audit logging for privileged actions and data export operations

### 20.6 License Enforcement Integration
- License validated on startup, scheduled daily, and on Super Admin sign-in
- Degraded mode automatically enables read-only policy when invalid or expired
- Entitlement cache refreshed from signed local license payload
- Feature flags resolved by module entitlement plus system-level mandatory rules

### 20.7 Non-Functional Targets (Implementation)
- p95 API response under 500 ms for standard read endpoints
- p95 page load under 3 seconds on standard campus networks
- Horizontal scale readiness to 10,000 concurrent users
- Zero data-loss tolerance for academic history records

### 20.8 Observability and Operations
- Structured logging with correlation IDs
- Health checks for database, license state, and background workers
- Metrics: request latency, error rates, queue depth, failed jobs
- Audit export and retention controls for compliance reporting

### 20.9 Release Scope for v1
- In scope for v1: Authentication, Departments, SIS, Courses/Programs, Assignments, Results, Notifications, Licensing core
- In scope for v1.1: Quizzes, Attendance, FYP, AI Chatbot baseline, extended themes
- In scope for v1.2: Advanced analytics, advanced audit dashboards, multi-campus enhancements
- In scope for v1.4: Result Calculation configuration, automated subject GPA, semester GPA, and cumulative CGPA processing

### 20.10 Phase 11 Implementation Focus — Result Calculation and GPA Automation
- **Stage 11.1 Configuration UI and Data Model**
  - Add sidebar menu: `Result Calculation`
  - Add database tables for GPA-to-score mappings and assessment component weightages
  - Provide repeatable admin entry forms with Add Row and Save actions
- **Stage 11.2 Result Calculation Engine**
  - Compute subject totals from configured assessment weights
  - Resolve subject GPA from configured GPA/score thresholds
  - Validate total component weightage = 100 before activation
- **Stage 11.3 Academic Aggregation Automation**
  - Automatically compute semester GPA once all subject results are complete
  - Automatically update cumulative CGPA after semester GPA changes
  - Re-run calculations after approved mark modifications and retain audit logs

---