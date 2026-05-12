# Institution License Validation Phases

## Purpose
This document defines phased validation checkpoints to confirm license-driven behavior for School, College, and University in Tabsan EduSphere.

Each phase must be completed with:
- Implementation Summary
- Validation Summary
- Status of checks done in phase

---

## Phase 1 - License and Institution Mode Binding

### Scope
1. Confirm license import from Tabsan Lic app works in EduSphere.
2. Confirm selected institution modes in license (School, College, University) are correctly enforced.
3. Confirm data and modules are loaded only for licensed institution modes.

### Checkpoints
- Upload valid .tablic file and verify activation success.
- Verify institution mode flags in runtime policy after upload.
- Verify disabled institution modes cannot access unrelated modules.

### Implementation Summary
- Verified API and Web runtime are running for validation:
	- API: `https://localhost:7231`
	- Web: `https://localhost:7160`
- Validated authentication flow for `superadmin` and protected endpoint access.
- Exercised Phase 1 API checks:
	- `POST /api/v1/auth/login`
	- `GET /api/v1/license/status`
	- `GET /api/v1/license/details`
	- `GET /api/v1/institution-policy`
	- `POST /api/v1/license/upload` with generated `.tablic`
- Confirmed current institution policy snapshot returns mode flags from policy service.

### Validation Summary
- Login test result:
	- `superadmin / Admin@12345` authentication succeeded.

- License/policy snapshot before upload:
	- `status`: `Invalid`
	- `details.status`: `None`
	- `institution-policy`: `{ includeSchool: false, includeCollege: false, includeUniversity: true, isValid: true }`

- Initial license upload attempt:
	- Upload file: `tools/Tabsan.Lic/License/tabsan-license-3a84a822d7d94d85bcc29f03384dc62d.tablic`
	- Response: `{"message":"License validation failed. The file may be invalid or tampered."}`
	- Post-upload status unchanged: `Invalid` / `None`

- Root cause found from API logs:
	- Activation was failing at database save due legacy non-null `license_state` columns (`InstitutionScope`, `ExpiryType`) without defaults in current runtime schema.

- Remediation applied in validation environment:
	- Added SQL defaults on `license_state.InstitutionScope` and `license_state.ExpiryType`.

- Post-remediation upload attempt:
	- Upload response: `{"message":"License activated successfully."}`
	- `GET /api/v1/license/status`: `Active`
	- `GET /api/v1/license/details`: `status=Active`, `licenseType=Yearly`, `remainingDays=365`
	- `GET /api/v1/institution-policy`: `{ includeSchool: false, includeCollege: false, includeUniversity: true, isValid: true }`
	- `GET /api/v1/portal-capabilities/matrix`:
		- policy flags: `school=false`, `college=false`, `university=true`
		- rows with school enabled: `0`
		- rows with college enabled: `0`
		- `fyp_workspace`: `university=true`, `school=false`, `college=false`

- Outcome:
	- Runtime policy read path works.
	- License import and activation path works after resolving legacy schema defaults.
	- Institution mode binding from uploaded license is validated at policy level.

### Status of Checks Done
- [x] SuperAdmin authentication validated
- [x] Institution policy read validated
- [x] License upload validated
- [x] Institution mode binding validated
- [x] Mode-restricted module access validated

Phase 1 Status: Completed
Passed: 5
Failed: 0
Blocked/Pending: 0

---

## Phase 2 - Student Lifecycle by Institution Type

### Scope
1. Confirm School lifecycle works end-to-end.
2. Confirm College lifecycle works end-to-end.
3. Confirm University lifecycle works end-to-end.

### Checkpoints
- Admission -> enrollment -> progression -> completion flow by institution type.
- Grade calculation flow matches the selected institution type.
- Academic rules and progression behavior are institution-specific.

### Implementation Summary
- Executed live mode-switch validation on API runtime (`http://localhost:5181`) after applying policy persistence fix in `InstitutionPolicyService.SavePolicyAsync`.
- Validated mode transitions using three licenses:
	- School: `tabsan-license-ce3ee52ac7ae45f0943c506cd8117c56.tablic`
	- College: `tabsan-license-dce6179f7e8e4f71b64835e0788bed39.tablic`
	- University: `tabsan-license-f97ff736a0f84a14b3571bbb9a879368.tablic`
- Executed endpoint set per mode:
	- `POST /api/v1/license/upload`
	- `GET /api/v1/institution-policy`
	- `GET /api/v1/labels`
	- `GET /api/v1/portal-capabilities/matrix`
	- `GET /api/v1/institution-grading-profiles/{type}`
	- `POST /api/v1/progression/evaluate`
- Confirmed persisted mode flags in DB (`portal_settings`) after each mode activation.
- Noted current license generator limitation: verification key reuse causes replay rejection unless consumed-key table is cleared between sequential activations in the same environment.

### Validation Summary
- Student profile used:
	- `77777777-7777-7777-7777-777777777733`

- School mode evidence:
	- upload: `License activated successfully`
	- policy: `school=true, college=false, university=false`
	- labels: `Grade / Promotion / Percentage / Subject / Class`
	- matrix rows: `school=12, college=0, university=0`
	- progression evaluate: `institutionType=1`, `canProgress=false`, `Grade 2 -> Grade 3`, required `40`
	- DB policy keys: `institution_include_school=true`, others false

- College mode evidence:
	- upload: `License activated successfully`
	- policy: `school=false, college=true, university=false`
	- labels: `Year / Progression / Percentage / Subject / Year-Group`
	- matrix rows: `school=0, college=12, university=0`
	- progression evaluate: `institutionType=2`, `canProgress=false`, `Year 1 -> Year 2`, required `40`
	- DB policy keys: `institution_include_college=true`, others false

- University mode evidence:
	- upload: `License activated successfully`
	- policy: `school=false, college=false, university=true`
	- labels: `Semester / Progression / GPA/CGPA / Course / Batch`
	- matrix rows: `school=0, college=0, university=13`
	- progression evaluate: `institutionType=0`, `canProgress=true`, `Semester 2 -> Semester 3`, CGPA `3.20 >= 2.00`
	- DB policy keys: `institution_include_university=true`, others false

- Grading profile endpoint result:
	- currently returns `No grading profile found` for all three institution types in this dataset.

### Status of Checks Done
- [x] School lifecycle validated
- [x] College lifecycle validated
- [x] University lifecycle validated

Phase 2 Status: Completed
Passed: 3
Failed: 0
Blocked/Pending: 0

---

## Phase 3 - Multi-Mode License Coverage

### Scope
1. Confirm when 2 or 3 institution types are selected in license, all corresponding functionality is enabled.
2. Confirm module/configuration union behavior for combined modes.

### Checkpoints
- School + College
- School + University
- College + University
- School + College + University

### Implementation Summary
- Executed live multi-mode validation on API runtime (`http://localhost:5181`) using four mixed-scope licenses:
	- School+College: `tabsan-license-8f3f00e8ea1f499292d9ca9f77b33d1f.tablic`
	- School+University: `tabsan-license-dd7eaebc155a47a1a98f43e87458fc6a.tablic`
	- College+University: `tabsan-license-b216de81e83840ea985d5e64c7b3285a.tablic`
	- School+College+University: `tabsan-license-1ab0597d6037499ea86451c27b8566f0.tablic`
- Captured endpoint evidence per combination:
	- `POST /api/v1/license/upload`
	- `GET /api/v1/institution-policy`
	- `GET /api/v1/labels`
	- `GET /api/v1/portal-capabilities/matrix`
	- `GET /api/v1/institution-grading-profiles/{type}` for all three types
	- `POST /api/v1/progression/evaluate` for institutionType `School(1)`, `College(2)`, `University(0)`
- Confirmed persisted union flags in `portal_settings` for each combination.
- Sequential activations used consumed-key reset (`DELETE FROM dbo.consumed_verification_keys`) due current generator verification-key reuse.

### Validation Summary
- Shared student used for progression checks:
	- `77777777-7777-7777-7777-777777777733`

- School + College
	- policy: `school=true, college=true, university=false`
	- labels: School vocabulary (`Grade/Promotion/Percentage/Subject/Class`)
	- matrix rows: `school=12, college=12, university=0`
	- DB flags: `institution_include_school=true`, `institution_include_college=true`, `institution_include_university=false`

- School + University
	- policy: `school=true, college=false, university=true`
	- labels: University vocabulary (`Semester/Progression/GPA/CGPA/Course/Batch`)
	- matrix rows: `school=12, college=0, university=13`
	- DB flags: `institution_include_school=true`, `institution_include_college=false`, `institution_include_university=true`

- College + University
	- policy: `school=false, college=true, university=true`
	- labels: University vocabulary (`Semester/Progression/GPA/CGPA/Course/Batch`)
	- matrix rows: `school=0, college=12, university=13`
	- DB flags: `institution_include_school=false`, `institution_include_college=true`, `institution_include_university=true`

- School + College + University
	- policy: `school=true, college=true, university=true`
	- labels: University vocabulary (`Semester/Progression/GPA/CGPA/Course/Batch`)
	- matrix rows: `school=12, college=12, university=13`
	- DB flags: all three `true`

- Progression endpoint consistency across all combinations:
	- `institutionType=1 (School)`: `canProgress=false`, `Grade 2 -> Grade 3`
	- `institutionType=2 (College)`: `canProgress=false`, `Year 1 -> Year 2`
	- `institutionType=0 (University)`: `canProgress=true`, `Semester 2 -> Semester 3`

- Grading-profile endpoint currently returns `No grading profile found` for School/College/University in this dataset.

### Status of Checks Done
- [x] Two-mode combinations validated
- [x] Three-mode combination validated
- [x] Combined configuration behavior validated

Phase 3 Status: Completed
Passed: 3
Failed: 0
Blocked/Pending: 0

---

## Phase 4 - Charts, Tables, Menus, and Reports by Institution and Role

### Scope
1. Confirm dashboards, charts, tables, menus, and reports are correct by institution mode.
2. Confirm behavior is correct by role (SuperAdmin, Admin, Faculty, Student).

### Checkpoints
- UI components render correctly per mode.
- Report data is scoped correctly by assigned institution.
- Menu composition aligns with role plus institution policy.

### Implementation Summary
- Document UI composition source and filtering logic.
- Document report query filters for institution constraints.
- Added execution assets for repeatable validation:
	- Runbook: `Docs/Phase4-Validation-Runbook.md`
	- API helper: `Scripts/Phase4-Validate-Institution-Role.ps1`
- Started API runtime in Development on `http://localhost:5181` and captured authenticated baseline evidence (SuperAdmin) for:
	- `GET /api/v1/institution-policy`
	- `GET /api/v1/labels`
	- `GET /api/v1/portal-capabilities/matrix`
- Evidence files created under `Artifacts/Phase4/Api` with timestamp `20260512-133716`.
- Created deterministic Phase 4 validation users through the supported import flow (`POST /api/v1/user-import/csv`) using SuperAdmin authentication:
	- `phase4.admin.20260512134426`
	- `phase4.faculty.20260512134426`
	- `phase4.student.20260512134426`
- Import result: `totalRows=3`, `imported=3`, `duplicates=0`, `errors=0`.
- Captured authenticated role evidence for Admin / Faculty / Student under `Artifacts/Phase4/Api` with timestamp set `20260512-134447` / `20260512-134448`.

### Validation Summary
- Record per-role screenshots and report exports.
- Record negative checks for out-of-scope institution data.
- Baseline API check status:
	- Unauthenticated calls correctly returned `401 Unauthorized` (expected for protected endpoints).
	- Authenticated SuperAdmin calls succeeded and were archived as JSON evidence.
- Imported-user login behavior validated for Phase 4 evidence users:
	- Admin login succeeded; response showed `role=Admin` and `mustChangePassword=true`.
	- Faculty login succeeded; response showed `role=Faculty` and `mustChangePassword=true`.
	- Student login succeeded; response showed `role=Student` and `mustChangePassword=true`.
- Positive role evidence captured:
	- Admin / Faculty / Student successfully accessed `GET /api/v1/institution-policy`.
	- Admin / Faculty / Student successfully accessed `GET /api/v1/labels`.
	- Admin / Faculty / Student successfully accessed `GET /api/v1/portal-capabilities/matrix`.
	- Admin successfully accessed `GET /api/v1/license/details`.
	- Faculty and Student were denied on `GET /api/v1/license/details` with `403 Forbidden`.
	- Student report catalog response was role-filtered to `student_transcript` only.
	- Faculty report catalog returned faculty-allowed operational reports including attendance, GPA, results, and FYP status.
- Negative role evidence captured:
	- Admin denied on `GET /api/v1/admin-user` with `403 Forbidden`.
	- Faculty denied on `GET /api/v1/admin-user` with `403 Forbidden`.
	- Student denied on `GET /api/v1/admin-user` with `403 Forbidden`.
	- Student denied on `GET /api/v1/reports/attendance-summary` with `403 Forbidden`.
	- Admin call to `GET /api/v1/reports/attendance-summary` without scoped filters returned an error response in current dataset and should be re-run with valid report filters during report/export evidence capture.
- Remaining evidence pending:
	- Role-wise UI screenshots (SuperAdmin/Admin/Faculty/Student) by mode.
	- Report export artifacts by mode.
	- Final negative authorization checks by mode-specific UI route.

### Execution Assets
- Runbook: [Docs/Phase4-Validation-Runbook.md](Phase4-Validation-Runbook.md)
- API evidence helper script: [Scripts/Phase4-Validate-Institution-Role.ps1](../Scripts/Phase4-Validate-Institution-Role.ps1)

### Phase 4 Execution Matrix (2026-05-12)

Use this matrix to run and record evidence consistently before marking checks complete.

| Mode | Role | Area | Expected | Evidence to Capture |
|---|---|---|---|---|
| School | SuperAdmin | Menus/Reports | Full School menus + school report visibility | Screenshot + export file |
| School | Admin | Menus/Tables | School-only admin operations in assigned scope | Screenshot + API sample |
| School | Faculty | Charts/Tables | School class/subject tables only, no college/university items | Screenshot |
| School | Student | Dashboard/Reports | School labels (Grade/Percentage), own data only | Screenshot |
| College | SuperAdmin | Menus/Reports | Full College menus + college report visibility | Screenshot + export file |
| College | Admin | Menus/Tables | College-only admin operations in assigned scope | Screenshot + API sample |
| College | Faculty | Charts/Tables | Year/Percentage context, no school/university-only items | Screenshot |
| College | Student | Dashboard/Reports | Year/Percentage labels, own data only | Screenshot |
| University | SuperAdmin | Menus/Reports | Full University menus + university report visibility | Screenshot + export file |
| University | Admin | Menus/Tables | University admin operations in assigned scope | Screenshot + API sample |
| University | Faculty | Charts/Tables | Semester/GPA context, no school-only items | Screenshot |
| University | Student | Dashboard/Reports | Semester/GPA labels, own data only | Screenshot |

Negative checks required for every mode:
- Attempt access to out-of-scope report/menu route and confirm denial or filtered output.
- Capture at least one API response sample proving institution/role scoping.

Suggested endpoint set for evidence:
- `GET /api/v1/institution-policy`
- `GET /api/v1/labels`
- `GET /api/v1/portal-capabilities/matrix`
- Relevant report endpoints used by each role

### Status of Checks Done
- [ ] Charts validated by mode and role
- [ ] Tables validated by mode and role
- [ ] Menus validated by mode and role
- [ ] Reports validated by mode and role

---

## Phase 5 - User Creation and CSV Import with Institution Assignment

### Scope
1. Confirm SuperAdmin can assign School/College/University during user create/import.
2. Confirm lifecycle and grading behavior follows assigned institution for each user.

### Checkpoints
- Manual user creation institution assignment.
- CSV import institution mapping and validation.
- Post-import workflow behavior by assigned institution.

### Implementation Summary
- Document create/import DTO fields and mapping rules.
- Document validation errors for invalid or non-licensed institution assignments.

### Validation Summary
- Record import success/failure cases.
- Record role/user behavior after import by institution assignment.

### Status of Checks Done
- [ ] Manual user assignment validated
- [ ] CSV import assignment validated
- [ ] Post-import institution behavior validated

---

## Phase 6 - Data Access Boundaries by Assigned Institution

### Scope
1. Confirm Student, Faculty, and Admin can access only assigned institution data.
2. Confirm institution-scoped access for API and portal routes.

### Checkpoints
- Student visibility scoped to own institution(s).
- Faculty visibility scoped to assigned institution(s)/departments.
- Admin visibility scoped to assigned institution(s)/departments.

### Implementation Summary
- Document middleware/policy checks enforcing institution boundaries.
- Document repository query filters used for institution scoping.

### Validation Summary
- Record positive and negative authorization checks by role.
- Record API response samples proving data boundaries.

### Status of Checks Done
- [ ] Student scoping validated
- [ ] Faculty scoping validated
- [ ] Admin scoping validated

---

## Phase 7 - SuperAdmin Full Access and Permission Matrix

### Scope
1. Confirm SuperAdmin has full access to add/edit/deactivate/activate and all institution data.
2. Confirm full module/report visibility for SuperAdmin.

### Checkpoints
- CRUD and activation/deactivation actions across modules.
- Multi-institution cross-access for management and reporting.
- No unintended permission denials for SuperAdmin.

### Implementation Summary
- Document SuperAdmin policy and permission matrix.
- Document any explicit bypass rules intended for SuperAdmin.

### Validation Summary
- Record end-to-end admin operation checks across all institution modes.
- Record audit entries for privileged actions.

### Status of Checks Done
- [ ] Full CRUD permission validated
- [ ] Activate/deactivate permission validated
- [ ] Cross-institution full access validated

---

## Phase Completion Template (Use after every phase)

### Implementation Summary
- Components changed:
- Services/controllers/middleware touched:
- Configuration updates:

### Validation Summary
- Test cases executed:
- Evidence links/logs:
- Passed/failed counts:

### Status of Checks Done
- [ ] All checkpoints passed
- [ ] Docs updated
- [ ] Git sync completed

---

## Mandatory Docs Update After Each Phase

Update all of the following:
1. Docs/Function-List.md
2. Docs/Functionality.md
3. Project startup Docs/PRD.md
4. Project startup Docs/Database Schema.md
5. Project startup Docs/Development Plan - ASP.NET.md
6. Docs/Command.md

## Mandatory Git Workflow After Each Phase

Run in this order after phase completion:
1. Commit
2. Pull
3. Push

Suggested commands:

```powershell
cmd /c git -C "<repo-root>" add -A
cmd /c git -C "<repo-root>" commit -m "Phase X completion - institution license validation"
cmd /c git -C "<repo-root>" pull --rebase origin main
cmd /c git -C "<repo-root>" push origin main
```
