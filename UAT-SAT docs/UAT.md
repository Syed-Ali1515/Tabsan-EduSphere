# User Acceptance Testing (UAT)

## Purpose
User acceptance testing confirms the product behaves correctly from an operator perspective before deployment handoff.

## EduSphere UAT Steps

1. Start the API and web application.
2. Sign in as SuperAdmin.
3. Open institution policy settings.
4. Enable any valid combination of School, College, and University.
5. Save the policy and confirm it persists after reload.
6. Upload a valid `.tablic` file from Tabsan.Lic.
7. Confirm the license becomes active and the correct institution scope is enforced.
8. Verify a domain-locked license is rejected from the wrong host.
9. Verify a license with `MaxUsers` greater than zero enforces the user limit.
10. Test import/export (CSV, PDF, Excel) features for users and timetables.
11. Run Scripts/04-Maintenance-Indexes-And-Views.sql and confirm indexes/views are up to date.

## Tabsan.Lic UAT Steps

1. Start the console tool.
2. Generate a verification key.
3. Choose one of the available expiry choices (1 month, 1/2/3 years, Permanent).
4. Build a `.tablic` file.
5. Select the institution scope (School, College, University).
6. Set max users and allowed domain.
7. Confirm the tool writes the file and marks the key as license-generated.

## Acceptance Notes

- The license tool supports 1 month, 1 year, 2 years, 3 years, and Permanent expiry choices.
- The license payload carries institution-scope flags and the app applies them on activation.
- Import/export and index maintenance features are validated.
- The unit-test build for the main application passed during this session.
