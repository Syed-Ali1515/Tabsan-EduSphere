# Phase 4 Validation Runbook (Institution + Role UI/Report Coverage)

This runbook is for Phase 4 in [Docs/Institution-License-Validation-Phases.md](Institution-License-Validation-Phases.md).

Goal:
- Validate charts, tables, menus, and reports by institution mode and role.
- Capture consistent evidence for documentation closure.

---

## Inputs Required

1. Active API base URL (example: http://localhost:5181)
2. Active Web base URL (example: https://localhost:7160)
3. Test users for each role:
- SuperAdmin
- Admin
- Faculty
- Student
4. License files for each institution mode:
- School only
- College only
- University only

---

## Evidence Folder Structure

Create this folder layout before testing:

- Artifacts/Phase4/
- Artifacts/Phase4/School/
- Artifacts/Phase4/College/
- Artifacts/Phase4/University/
- Artifacts/Phase4/Exports/
- Artifacts/Phase4/Api/

Store:
- Screenshots by role/mode in mode folders.
- Exported report files in Exports.
- API responses in Api.

---

## Execution Sequence

1. Activate a mode license (School, then College, then University).
2. Verify mode state via API:
- GET /api/v1/institution-policy
- GET /api/v1/labels
- GET /api/v1/portal-capabilities/matrix
3. Log in as each role and capture:
- Dashboard screenshot
- Main menu screenshot
- One table screenshot
- One chart screenshot (if available for that role)
- One report page screenshot
4. Export one role-appropriate report and save output.
5. Run one negative check per role/mode (out-of-scope route/report).
6. Save API response evidence for each negative check.

Repeat for all three institution modes.

---

## Required Checks Matrix (Minimum)

For each mode (School/College/University):
- SuperAdmin: full menu/report visibility.
- Admin: scoped management visibility.
- Faculty: teaching scope visibility.
- Student: own-data-only visibility.

Mandatory negative checks:
- Role attempts access to out-of-scope report route.
- Mode attempts access to disabled-mode route.

Expected outcome:
- Access denied or filtered output with no cross-scope data.

---

## Naming Convention for Evidence Files

Use this naming format:
- <Mode>_<Role>_<Area>_<Timestamp>.png
- <Mode>_<Role>_Negative_<Route>_<Timestamp>.json
- <Mode>_<Role>_ReportExport_<Timestamp>.<ext>

Example:
- School_Admin_Menu_20260512-1845.png
- University_Student_Negative_AdminReport_20260512-1922.json

---

## Completion Criteria for Phase 4

All must be true:
- Charts validated by mode and role.
- Tables validated by mode and role.
- Menus validated by mode and role.
- Reports validated by mode and role.
- Negative checks captured with API evidence.

Then update:
- [Docs/Institution-License-Validation-Phases.md](Institution-License-Validation-Phases.md)
- [Docs/Command.md](Command.md)
- [Docs/Function-List.md](Function-List.md)
- [Docs/Functionality.md](Functionality.md)
- [Project startup Docs/PRD.md](../Project%20startup%20Docs/PRD.md)
- [Project startup Docs/Database Schema.md](../Project%20startup%20Docs/Database%20Schema.md)
- [Project startup Docs/Development Plan - ASP.NET.md](../Project%20startup%20Docs/Development%20Plan%20-%20ASP.NET.md)

---

## Optional Automation

Use [Scripts/Phase4-Validate-Institution-Role.ps1](../Scripts/Phase4-Validate-Institution-Role.ps1) to auto-collect policy/labels/matrix and negative API checks into JSON evidence files.
