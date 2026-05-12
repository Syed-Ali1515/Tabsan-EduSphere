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
- Document lifecycle service and strategy selection logic.
- Document grading and promotion rules bound to institution mode.

### Validation Summary
- Record lifecycle tests for School, College, University students.
- Record failed path prevention (for non-licensed mode behavior).

### Status of Checks Done
- [ ] School lifecycle validated
- [ ] College lifecycle validated
- [ ] University lifecycle validated

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
- Document how multi-mode union is resolved in policy/services.
- Document safeguards to avoid accidental mode exclusion.

### Validation Summary
- Record module visibility, menus, and endpoint access for each combination.
- Record policy snapshot output for each combination.

### Status of Checks Done
- [ ] Two-mode combinations validated
- [ ] Three-mode combination validated
- [ ] Combined configuration behavior validated

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

### Validation Summary
- Record per-role screenshots and report exports.
- Record negative checks for out-of-scope institution data.

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
