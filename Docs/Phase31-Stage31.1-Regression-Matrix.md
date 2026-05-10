# Phase 31 Stage 31.1 - Regression Matrix

Date: 2026-05-10
Status: Completed

## Scope

Stage 31.1 validates non-regression across the required release dimensions:
- Institution mode (School, College, University)
- License combinations
- Roles (SuperAdmin, Admin, Faculty, Student)
- Tenant isolation

## Automated Matrix Coverage

## Dimension A: Institution Mode x Role x License

Automated matrix scenarios:
- Institution modes: 3
- Roles: 4
- License profile variants: 2
- Total scenarios validated: 24

License variants used in tests:
- Licensed: ai_chat active
- Restricted: ai_chat inactive

Automated assertions executed for all 24 scenarios:
- SuperAdmin-only access remains enforced for advanced_audit
- fyp remains accessible only in University mode
- ai_chat activation follows license profile state
- Registry visibility pipeline remains stable and non-empty

Primary test asset:
- tests/Tabsan.EduSphere.UnitTests/Phase31Stage1RegressionMatrixTests.cs

## Dimension B: Existing Regression Anchors

Existing tests retained and mapped as supporting matrix anchors:
- Institution policy + role/module constraints:
  - tests/Tabsan.EduSphere.UnitTests/InstitutionPolicyTests.cs
  - tests/Tabsan.EduSphere.UnitTests/Phase24Tests.cs
- Authorization and role access regressions:
  - tests/Tabsan.EduSphere.IntegrationTests/AuthorizationRegressionTests.cs
- Navigation and menu visibility by role:
  - tests/Tabsan.EduSphere.IntegrationTests/SidebarMenuIntegrationTests.cs

## Dimension C: Tenant Isolation Baseline

Baseline isolation check added:
- Separate tenant settings stores retain independent tenant profile values without leakage.
- Test validates distinct tenant code persistence across isolated repositories.

Primary test asset:
- tests/Tabsan.EduSphere.UnitTests/Phase31Stage1RegressionMatrixTests.cs

## Execution Summary

Validation run:
- Targeted: dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj --filter FullyQualifiedName~Phase31Stage1RegressionMatrixTests
  - Result: 25/25 passed
- Full regression: dotnet test Tabsan.EduSphere.sln --no-build
  - Result: 197/197 passed

## Outcome

Stage 31.1 regression matrix baseline is now in place with executable coverage for all required dimensions and explicit linkage to supporting existing test suites.
