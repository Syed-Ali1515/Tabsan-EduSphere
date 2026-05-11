# UAT

## Purpose
User acceptance testing confirms that the product behaves correctly from an operator perspective before the deployment is handed off.

## EduSphere UAT

1. Start the API and web application.
2. Sign in as SuperAdmin.
3. Open institution policy settings.
4. Enable any valid combination of School, College, and University.
5. Save the policy and confirm it persists after reload.
6. Upload a valid `.tablic` file from Tabsan.Lic.
7. Confirm the license becomes active.
8. Verify a domain-locked license is rejected from the wrong host.
9. Verify a license with `MaxUsers` greater than zero enforces the limit.

## Tabsan.Lic UAT

1. Start the console tool.
2. Generate a verification key.
3. Choose one of the available expiry choices.
4. Build a `.tablic` file.
5. Select the institution scope.
6. Set max users and allowed domain.
7. Confirm the tool writes the file and marks the key as license-generated.

## Acceptance Notes

- The license tool now supports 1 month, 1 year, 2 years, 3 years, and Permanent expiry choices.
- The license payload now carries institution-scope flags and the app applies them on activation.
- The unit-test build for the main application passed during this session.