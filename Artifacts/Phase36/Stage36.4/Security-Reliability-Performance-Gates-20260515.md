# Stage 36.4 Security, Reliability, and Performance Gates Report

- Generated (UTC): 2026-05-15 03:31:34
- Repository root: c:\Users\alin\Desktop\Prj\Tabsan-EduSphere
- Unit test project: c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.UnitTests\Tabsan.EduSphere.UnitTests.csproj
- Integration test project: c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.IntegrationTests\Tabsan.EduSphere.IntegrationTests.csproj
- Execute: False

| Gate | Kind | Result | Details |
|---|---|---|---|
[DryRun] dotnet test c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.UnitTests\Tabsan.EduSphere.UnitTests.csproj --filter FullyQualifiedName~Tabsan.EduSphere.UnitTests.Phase27Stage2Tests --verbosity minimal
| MfaSecurityUnitTests | Test | PASS | dotnet test completed successfully |
[DryRun] dotnet test c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.IntegrationTests\Tabsan.EduSphere.IntegrationTests.csproj --filter FullyQualifiedName~Tabsan.EduSphere.IntegrationTests.Phase31Stage2SecurityHardeningTests --verbosity minimal
| SecurityHardeningIntegration | Test | PASS | dotnet test completed successfully |
[DryRun] dotnet test c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.IntegrationTests\Tabsan.EduSphere.IntegrationTests.csproj --filter FullyQualifiedName~Tabsan.EduSphere.IntegrationTests.Phase36Stage4HealthAndLicenseGateTests --verbosity minimal
| HealthAndLicenseSmoke | Test | PASS | dotnet test completed successfully |
[DryRun] dotnet test c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\tests\Tabsan.EduSphere.IntegrationTests\Tabsan.EduSphere.IntegrationTests.csproj --filter FullyQualifiedName~Tabsan.EduSphere.IntegrationTests.Phase36Stage4PerformanceSmokeTests --verbosity minimal
| PerformanceSmoke | Test | PASS | dotnet test completed successfully |
[DryRun] powershell -ExecutionPolicy Bypass -File c:\Users\alin\Desktop\Prj\Tabsan-EduSphere\Scripts\Phase34-BackupRestore-Drill.ps1 -DryRun
| BackupRestoreEvidence | Script | PASS | Backup/restore dry-run completed successfully |

## Gate Summary
- Gates evaluated: 5
- Passed: 5
- Failed: 0
