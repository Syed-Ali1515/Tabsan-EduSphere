# Observed Issues

Date: 2026-05-05

## Phase 1: Core Application Issues

### Stage 1.1: Access and Authorization
- API request failed with status 403. This error appears on Attendance, Results, Assignments, Quizzes, and Attendance.

### Stage 1.2: Missing CRUD Options in Portal
- No option to create/edit/delete Departments.
- No option to create/edit/delete Courses and Offerings.
- No option to create/edit/delete Enrollments.
- No option to create/edit/delete FYP Management.

### Stage 1.3: Report and Runtime Errors
- System.InvalidOperationException error on Result Summary.
- Not all reports are visible in Report Center menu.

### Stage 1.4: Menu and Navigation Cleanup
- Remove Module Settings from menu and scripts as no modules are there.
- The top items in sidebar still have hyperlinks and should not be clickable:
  - TE
  - Tabsan EduSphere
  - Campus Portal

### Stage 1.5: Student Lifecycle Error
- Error when clicking Promote button:
  - {"message":"Student profile 00000000-0000-0000-0000-000000000000 not found."}

### Stage 1.6: Themes and Branding Enhancements
- Create 10 more themes with different color combinations.
- No option to upload Logo or add Privacy Policy in Dashboard Settings.
- No text style option available in Dashboard Settings.

## Phase 2: App License

### Stage 2.1: User Count Based Concurrency Restriction
- Update app license import to enforce concurrent usage based on number of users allowed in the license.
- SuperAdmin must remain unrestricted and can always log in.

### Stage 2.2: Unlimited Mode
- Support "All Users" as number of users to remove concurrency restriction.

### Stage 2.3: License Locking and Reuse Prevention
- Once a license is used, it must be treated as closed and cannot be reused for another app deployment.
- Update code to always ask for license when uploaded to a new domain.
- Strengthen implementation so the license app/file cannot be recreated to scam or bypass licensing.

## Phase 3: License App

### Stage 3.1: Generator Alignment
- Update License App to support all App License requirements in Phase 2.

### Stage 3.2: File Security
- License file created by License App must be encrypted to prevent tampering.
- If someone decrypts/modifies the file, modified license must fail validation and not work.

## Phase 4: Data Import

### Stage 4.1: CSV User Import Feature
- Create an option to import users through CSV file.

### Stage 4.2: First Login Password Flow
- On import, system should assign username as initial password.
- On first login, system must force user to create a new password.

### Stage 4.3: Import Template Assets
- Create a folder named User Import Sheets.
- Create a CSV template with all required import columns.
- Add 1 sample row showing how user details should be entered.

## Implementation Checklist

Status Legend: Not Started | In Progress | Blocked | Done

| ID | Phase | Stage | Work Item | Priority | Owner | Status |
|---|---|---|---|---|---|---|
| P1-S1-01 | Phase 1 | Stage 1.1 | Fix 403 on Attendance, Results, Assignments, Quizzes, Student Attendance by correcting API authorization and role mapping. | P0 | Backend | Done |
| P1-S1-02 | Phase 1 | Stage 1.1 | Add regression tests for protected endpoints across Admin, Faculty, Student roles. | P1 | QA | Done |
| P1-S2-01 | Phase 1 | Stage 1.2 | Add Departments CRUD UI and API integration. | P0 | Frontend + Backend | Done |
| P1-S2-02 | Phase 1 | Stage 1.2 | Add Courses and Offerings CRUD UI and API integration. | P0 | Frontend + Backend | Done |
| P1-S2-03 | Phase 1 | Stage 1.2 | Add Enrollments CRUD UI and API integration. | P0 | Frontend + Backend | Done |
| P1-S2-04 | Phase 1 | Stage 1.2 | Add FYP Management CRUD UI and API integration. | P1 | Frontend + Backend | Done |
| P1-S3-01 | Phase 1 | Stage 1.3 | Fix System.InvalidOperationException in Result Summary and add error-safe handling. | P0 | Backend | Done |
| P1-S3-02 | Phase 1 | Stage 1.3 | Ensure all report definitions are visible in Report Center by role and active state. | P1 | Backend | Done |
| P1-S4-01 | Phase 1 | Stage 1.4 | Remove Module Settings from sidebar and related seed scripts. | P1 | Backend + DB | Done |
| P1-S4-02 | Phase 1 | Stage 1.4 | Remove hyperlink behavior from sidebar brand header items: TE, Tabsan EduSphere, Campus Portal. | P2 | Frontend | Done |
| P1-S5-01 | Phase 1 | Stage 1.5 | Fix Promote flow to pass valid student profile ID (avoid Guid.Empty). | P0 | Frontend + Backend | Done |
| P1-S6-01 | Phase 1 | Stage 1.6 | Add 10 additional themes with distinct color combinations. | P2 | Frontend | Done |
| P1-S6-02 | Phase 1 | Stage 1.6 | Add logo upload option in Dashboard Settings. | P1 | Frontend + Backend | Done |
| P1-S6-03 | Phase 1 | Stage 1.6 | Add Privacy Policy editor/link field in Dashboard Settings. | P1 | Frontend + Backend | Done |
| P1-S6-04 | Phase 1 | Stage 1.6 | Add text style options in Dashboard Settings. | P2 | Frontend | Done |
| P2-S1-01 | Phase 2 | Stage 2.1 | Implement concurrent user limit based on license user count. | P0 | Backend | Not Started |
| P2-S1-02 | Phase 2 | Stage 2.1 | Exempt SuperAdmin from license concurrency restrictions. | P0 | Backend | Not Started |
| P2-S2-01 | Phase 2 | Stage 2.2 | Implement All Users mode to disable concurrency cap. | P0 | Backend | Not Started |
| P2-S3-01 | Phase 2 | Stage 2.3 | Add one-time license activation binding to prevent reuse in another deployment/domain. | P0 | Backend + Security | Not Started |
| P2-S3-02 | Phase 2 | Stage 2.3 | Enforce license prompt and validation when app is deployed on a new domain. | P1 | Backend | Not Started |
| P2-S3-03 | Phase 2 | Stage 2.3 | Harden anti-tamper checks to prevent recreated/forged license files from passing validation. | P0 | Security | Not Started |
| P3-S1-01 | Phase 3 | Stage 3.1 | Update License App schema and generator logic to match Phase 2 constraints. | P0 | Tools Team | Not Started |
| P3-S2-01 | Phase 3 | Stage 3.2 | Encrypt generated license files and validate signature/integrity at load time. | P0 | Tools Team + Security | Not Started |
| P3-S2-02 | Phase 3 | Stage 3.2 | Reject modified license payload even if decrypted/repacked. | P0 | Tools Team + Security | Not Started |
| P4-S1-01 | Phase 4 | Stage 4.1 | Add CSV import feature for user creation in portal. | P1 | Frontend + Backend | Not Started |
| P4-S2-01 | Phase 4 | Stage 4.2 | Set initial password = username for imported users. | P1 | Backend | Not Started |
| P4-S2-02 | Phase 4 | Stage 4.2 | Force password change on first login for imported users. | P1 | Backend + Frontend | Not Started |
| P4-S3-01 | Phase 4 | Stage 4.3 | Create folder User Import Sheets with CSV template and one sample row. | P2 | PM + QA | Not Started |

## Delivery Order

1. Wave 1 (Critical): P1-S1-01, P1-S3-01, P1-S5-01, P2-S1-01, P2-S1-02, P2-S2-01, P2-S3-01, P3-S1-01, P3-S2-01, P3-S2-02
2. Wave 2 (Functional Coverage): P1-S2-01, P1-S2-02, P1-S2-03, P1-S2-04, P1-S3-02, P1-S6-02, P1-S6-03, P4-S1-01, P4-S2-01, P4-S2-02
3. Wave 3 (UX and Supporting): P1-S4-01, P1-S4-02, P1-S6-01, P1-S6-04, P4-S3-01
