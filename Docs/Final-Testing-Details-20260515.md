# Final Testing Details - 2026-05-15

## Scope

This document records the manual validation sweep performed after the final Phase 37 and Phase 38 closeout work.

Goal:

- verify repository evidence against `Docs/Consolidated-Execution-Enhancements-Issues.md`,
- verify build/publish readiness for EduSphere and Tabsan.Lic,
- verify major role/license/module/institute enforcement areas through focused automated tests,
- capture the remaining production-environment prerequisites after code/test validation is clean.

## Ledger and Evidence Audit

Validated latest final phases directly against the repo:

- Phase 36 evidence exists for Stage 36.4, 36.5, and 36.6.
- Phase 37 execute evidence exists and shows PASS:
  - `Artifacts/Phase37/Publish-Separation-20260515.md`
- Phase 38 execute evidence exists and shows PASS:
  - `Artifacts/Phase38/NonRuntime-Asset-Separation-20260515.md`
- Stage 36.2 strict readiness evidence exists and shows PASS:
  - `Artifacts/Phase36/Stage36.2/Environment-Readiness-20260515-100417.md`

Important audit note:

- `Docs/Consolidated-Execution-Enhancements-Issues.md` still records Stage 36.6 as a dry-run execution artifact, not a real production go-live run.

## Code Error Scan

Checked workspace errors with `get_errors`.

Result:

- no compile/code errors were surfaced in the main code/docs trackers touched by the final phases,
- markdown lint findings remain in documentation such as `Scripts/README.md` and `User Import Sheets/README.md`,
- those findings are documentation-style issues, not C# compile blockers.

## Build Validation

### Main solution

Command:

```powershell
dotnet build "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Tabsan.EduSphere.sln" -v minimal
```

Result:

- passed after stopping local running app processes that were locking output DLLs,
- one earlier failure was environmental only: a running `Tabsan.EduSphere.Web` process locked `Tabsan.EduSphere.Application.dll`,
- one later successful build still emitted retry warnings because a local `Tabsan.EduSphere.API` process held a background-jobs DLL during copy,
- final outcome: build succeeded.

### License solution

Command:

```powershell
dotnet build "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tools\Tabsan.Lic\Tabsan.Lic.sln" -v minimal
```

Result:

- passed.

## Automated Test Validation

### Unit tests

Executed:

- `InstitutionPolicyTests.cs`
- `Phase24Tests.cs`
- `SecurityValidationTests.cs`

Result:

- 41 passed, 0 failed.

### Integration tests - authorization/module/menu/auth scope batch

Executed:

- `InstitutionPolicyLicenseFlagsIntegrationTests.cs`
- `ModuleBackendEnforcementIntegrationTests.cs`
- `SidebarMenuIntegrationTests.cs`
- `AuthorizationRegressionTests.cs`

Result:

- 45 passed, 0 failed.

Validated areas:

- unauthenticated requests correctly returned `401 Unauthorized`,
- role-protected attendance, assignment, quiz, and result routes returned the expected allow/deny outcomes,
- module enforcement no longer masked authorization results in the shared integration baseline.

Root-cause fix applied:

- the shared integration-test factory now activates all modules in the test baseline so module-license middleware does not convert expected auth outcomes into false `403 Forbidden` failures.

### Integration tests - admin/lifecycle/report/UAT batch

Executed:

- `AdminUserManagementIntegrationTests.cs`
- `StudentLifecycleIntegrationTests.cs`
- `StudentSubmenuParityIntegrationTests.cs`
- `ReportExportsIntegrationTests.cs`
- `CrossRoleUatMatrixIntegrationTests.cs`
- `Phase36Stage4HealthAndLicenseGateTests.cs`
- `UserImportAndForceChangeIntegrationTests.cs`

Result:

- 67 passed, 0 failed.

Validated areas:

- admin user management and institution-type flows passed,
- report export authorization passed,
- cross-role UAT permission matrix passed,
- user import and forced password-change flow passed.

Additional fixes applied:

- `api/v1/dashboard/context` cached responses were corrected to preserve camel-cased JSON properties on cache hits,
- user-import integration tests now arrange institution-policy state explicitly so they do not fail from shared suite state leakage.

### Full integration suite

Command:

```powershell
dotnet test "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.IntegrationTests\Tabsan.EduSphere.IntegrationTests.csproj" --no-restore
```

Result:

- 235 passed, 0 failed.

## Publish Validation

### Phase 37 publish separation script

Command:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
& 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Scripts\Phase37-Separate-App-And-License-Publish.ps1' -RepoRoot 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere' -Execute
```

Result:

- PASS (`4/4` targets)
- evidence file confirms separate publish outputs for API, Web, BackgroundJobs, and LicenseApp.

Artifacts:

- `Artifacts/Phase37/Tabsan.EduSphere-App-Publish-20260515.zip`
- `Artifacts/Phase37/Tabsan.Lic-Publish-20260515.zip`

### Phase 38 non-runtime separation script

Command:

```powershell
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
& 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Scripts\Phase38-Separate-NonRuntime-Assets.ps1' -RepoRoot 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere' -Execute
```

Result:

- PASS (`7/7` folders)

Artifact:

- `Artifacts/Phase38/NonRuntime-Assets-20260515.zip`

### Direct project publishes

Commands/results:

- API publish passed:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' publish "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\src\Tabsan.EduSphere.API\Tabsan.EduSphere.API.csproj" -c Release -o "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Artifacts\Validation\API-Publish" -p:ErrorOnDuplicatePublishOutputFiles=false
```

- Web publish passed:

```powershell
dotnet publish "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\src\Tabsan.EduSphere.Web\Tabsan.EduSphere.Web.csproj" -c Release -o "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Artifacts\Validation\Web-Publish"
```

- BackgroundJobs publish passed:

```powershell
dotnet publish "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\src\Tabsan.EduSphere.BackgroundJobs\Tabsan.EduSphere.BackgroundJobs.csproj" -c Release -o "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Artifacts\Validation\BackgroundJobs-Publish"
```

- Tabsan.Lic publish passed:

```powershell
& 'C:\Program Files\dotnet\dotnet.exe' publish "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tools\Tabsan.Lic\Tabsan.Lic.csproj" -c Release -o "c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Artifacts\Validation\Tabsan.Lic-Publish"
```

## Runtime Smoke Check

### Published API launch from repo root

Result:

- failed immediately because `appsettings.json` was resolved from the current working directory (`repo root`) instead of the publish directory.

Interpretation:

- launch-context sensitive,
- not enough to reject publishability by itself.

### Published API launch from publish directory

Command pattern:

```powershell
Set-Location 'c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Artifacts\Validation\API-Publish'
$env:ASPNETCORE_URLS='http://127.0.0.1:5085'
.\Tabsan.EduSphere.API.exe
```

Result:

- startup began,
- application then aborted by design in `Production` because `ConnectionStrings:DefaultConnection` still contained an unsafe placeholder/missing value.

Interpretation:

- startup safety guard is functioning correctly,
- production hosting requires real environment-specific secrets/config values before runtime launch can be considered valid.

## Deployment-Readiness Evidence

Validated existing strict readiness report:

- `Artifacts/Phase36/Stage36.2/Environment-Readiness-20260515-100417.md`

Result:

- 46 pass, 0 fail in the captured strict-readiness report.

Interpretation:

- the scripted readiness gate has a passing evidence artifact,
- current local ad hoc runtime smoke still requires real production-safe values at launch time.

## Bottom Line

What is validated:

- repository evidence exists for the latest deployment/publish phases,
- main solution and license solution build,
- publish separation works,
- direct project publishes work,
- strict readiness evidence exists,
- focused unit validation passes,
- targeted regression suites pass,
- the full integration suite passes (`235/235`).

What is not validated cleanly:

- production go-live (Stage 36.6) is still documented as dry-run evidence,
- runtime hosting still cannot be called validated in a production-like environment without real production-safe secrets and configuration values.
