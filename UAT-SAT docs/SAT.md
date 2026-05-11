# Site Acceptance Testing (SAT)

## Purpose
Site acceptance testing verifies the deployed application behaves correctly in its target environment after deployment.

## EduSphere SAT Steps

1. Deploy the API and web application to the target host.
2. Confirm the application starts cleanly and health checks are green.
3. Upload a valid license file from Tabsan.Lic.
4. Confirm the current license status is active.
5. Confirm the active host matches the allowed domain when one is configured.
6. Verify the institution policy stored in the application matches the license scope.
7. Confirm login and role-based navigation continue to work.
8. Confirm student lifecycle actions still function after deployment.
9. Test import/export (CSV, PDF, Excel) features for users and timetables.
10. Run Scripts/04-Maintenance-Indexes-And-Views.sql and confirm indexes/views are up to date.

## Tabsan.Lic SAT Steps

1. Deploy the standalone Tabsan.Lic tool to the operator workstation.
2. Confirm the database file is created in `%APPDATA%/Tabsan/tabsan_lic.db`.
3. Generate a key and build a license file.
4. Confirm the file can be uploaded successfully to the deployed EduSphere environment.
5. Confirm the same license file is rejected if the domain binding does not match.

## Exit Criteria

- Deployed services respond successfully.
- License activation succeeds only in the intended environment.
- Institution scope and user limits remain enforced after deployment.
- Import/export and index maintenance features are validated.
