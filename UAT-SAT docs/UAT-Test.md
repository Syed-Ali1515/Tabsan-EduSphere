# UAT Test Plan

## Scope
This checklist covers the user-acceptance flow for the main EduSphere app and the standalone Tabsan.Lic tool before deployment.

## Pre-Deployment Checks

| Area | Step | Expected Result |
|---|---|---|
| EduSphere app | Start the solution and open the login page | App starts without startup errors |
| EduSphere app | Verify institution policy endpoints are reachable | School, College, and University flags can be read and saved by SuperAdmin |
| EduSphere app | Upload a valid `.tablic` file | License activates successfully |
| EduSphere app | Upload a tampered `.tablic` file | Activation is rejected |
| EduSphere app | Test a license locked to a different domain | Activation is rejected on the wrong host |
| EduSphere app | Log in with a license that has `MaxUsers = 0` | Unlimited concurrent usage is allowed |
| Tabsan.Lic | Generate a single verification key | A raw key is shown once and the hash is stored |
| Tabsan.Lic | Generate a bulk batch of keys | Multiple keys are created with unique IDs |
| Tabsan.Lic | Build a `.tablic` file with School/College/University scope | Selected institution flags are embedded in the payload |
| Tabsan.Lic | Use 1 month / 1 year / 2 years / 3 years / Permanent expiry | The generated record reflects the selected expiry |
| Tabsan.Lic | Set max users and allowed domain | Values are persisted and written into the payload |

## Validation Results

- `dotnet build tools/Tabsan.Lic/Tabsan.Lic.sln` succeeded.
- `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj -v minimal` succeeded with 130/130 tests passing.

## Exit Criteria

- License generation works end-to-end.
- License activation updates the app policy and enforces the selected domain and user constraints.
- The app and the license tool both run without build or test failures.
