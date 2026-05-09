# Final Touches Plan

**Date:** 2026-05-03  
**Owner:** Engineering  
**Status:** In Progress

## Execution Rule (Mandatory)
For **every completed phase**:
1. Update [Docs/Function-List.md](../Docs/Function-List.md)
2. Update [Project startup Docs/PRD.md](PRD.md)
3. Update this file with:
- Completion mark
- Implementation summary
- Validation summary

---

## Phase 28 — Scalability Architecture (Stage 28.1)
**Status:** ✅ Stage 28.1 Complete (2026-05-09)

### Completion Mark
- [x] Enable response compression on API and Web.
- [x] Apply JSON null-field omission for lighter payloads.
- [x] Remove Web dependence on ASP.NET session for portal/API auth state.
- [x] Add optional shared data-protection key-ring support for multi-node Web deployments.
- [x] Preserve load-balancer forwarded-header support across API and Web.
- [x] Confirm no database migration is required.

### Implementation Summary
- API startup now enables Brotli/Gzip response compression, omits null JSON properties, and accepts forwarded host metadata behind reverse proxies.
- Web startup now enables response compression, forwarded headers, and optional shared data-protection key-ring persistence.
- `EduApiClient` now persists API base URL, token, department, session identity, and forced-password-change state in protected HttpOnly cookies rather than server session, allowing stateless Web nodes.
- Logout/connection reset now clears protected cookies instead of depending on session teardown.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — passed.
- Automated tests — **160/160 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.2 Foundation Batch)
**Status:** ✅ Foundation Delivered (2026-05-09)

### Completion Mark
- [x] Add optional Redis-backed distributed cache registration with local fallback.
- [x] Move hot-read cache paths onto distributed cache for scale-out sharing.
- [x] Add hosted background worker for large notification fan-out batches.
- [x] Add focused unit tests for deferred fan-out behavior.
- [x] Confirm no database migration is required.

### Implementation Summary
- API startup now supports `ScaleOut:RedisConnectionString` and `ScaleOut:RedisInstanceName`, using distributed memory when Redis is not configured.
- `ModuleEntitlementResolver` now uses local memory as L1 cache and distributed cache as the shared cross-node layer.
- `ReportService.GetCatalogAsync` now caches per-role report catalogs in distributed cache.
- `NotificationService` now defers large recipient batches to `NotificationFanoutWorker`, which persists recipients in chunks in the background.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — passed.
- Automated tests — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.2 Completion)
**Status:** ✅ Complete (2026-05-10)

### Completion Mark
- [x] Add queued report-generation endpoint coverage for result-summary exports.
- [x] Add queued large recalculation endpoint coverage for result publish-all workflows.
- [x] Add background workers and distributed-cache-backed job-state tracking for both queued workloads.
- [x] Keep existing synchronous endpoints intact for compatibility.
- [x] Confirm no database migration is required.

### Implementation Summary
- `ReportController` now supports asynchronous result-summary export jobs (excel/csv/pdf) with queue, status polling, and download endpoints.
- `ResultController` now supports asynchronous publish-all jobs for large offering-level publication/recalculation workloads.
- New hosted workers (`ResultPublishJobWorker`, `ReportExportJobWorker`) process queued jobs and persist status/results through distributed cache.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — passed.
- Automated tests — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 1)
**Status:** ✅ Slice 1 Delivered (2026-05-10)

### Completion Mark
- [x] Add configurable media-storage abstraction in API for local/provider-based persistence.
- [x] Add storage settings section to appsettings for external storage/CDN readiness.
- [x] Migrate payment-proof upload flow to provider-backed storage.
- [x] Preserve metadata-only persistence model in existing database schema.
- [x] Confirm no database migration is required.

### Implementation Summary
- Added `IMediaStorageService`, `MediaStorageOptions`, and `LocalMediaStorageService` in API services.
- Registered media storage options + provider in `API/Program.cs`.
- Updated `PaymentReceiptController.SubmitProof` to validate uploads and save via storage abstraction, then persist returned storage object key.
- Added `MediaStorage` settings in API appsettings files for local root, optional key prefix, and optional public base URL.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 2)
**Status:** ✅ Slice 2 Delivered (2026-05-10)

### Completion Mark
- [x] Move media storage contract into Application layer for cross-layer use.
- [x] Add provider-backed read path for stored media.
- [x] Migrate graduation certificate generation to provider-backed persistence.
- [x] Preserve backward compatibility for legacy certificate path records.
- [x] Confirm no database migration is required.

### Implementation Summary
- Added `Application/Interfaces/IMediaStorageService.cs` and moved storage result contract to the Application layer.
- Updated `LocalMediaStorageService` to implement the Application-layer contract and support `ReadAsBytesAsync`.
- Refactored `GraduationService` certificate generation and download methods to use storage-provider save/read operations.
- Added compatibility fallback in certificate download for legacy `/certificates/*` path-based records.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 3)
**Status:** ✅ Slice 3 Delivered (2026-05-10)

### Completion Mark
- [x] Migrate license upload temporary file handling to storage-provider flow.
- [x] Add in-memory license activation path to remove filesystem-path coupling.
- [x] Extend storage provider contract with deletion support for temporary object cleanup.
- [x] Confirm no database migration is required.

### Implementation Summary
- Updated `LicenseController.Upload` to save uploaded license bytes via `IMediaStorageService`, read them back by key, and always clean up with provider delete.
- Added `LicenseValidationService.ActivateFromBytesAsync` and made file-based activation delegate to it.
- Extended storage contract (`IMediaStorageService.DeleteAsync`) and local provider implementation to support deletion.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 4)
**Status:** ✅ Slice 4 Delivered (2026-05-10)

### Completion Mark
- [x] Add config-driven storage provider selection.
- [x] Add second storage provider implementation (`BlobMediaStorageService`).
- [x] Add blob-root configuration key across API environments.
- [x] Preserve local-provider default for backward-compatible runtime behavior.
- [x] Confirm no database migration is required.

### Implementation Summary
- Added `MediaStorageServiceCollectionExtensions.AddConfiguredMediaStorage` to bind `MediaStorageOptions` and choose provider by `MediaStorage:Provider`.
- Added `BlobMediaStorageService` implementing the existing storage contract with object-key persistence semantics.
- Updated API `Program.cs` to use configuration-driven storage registration.
- Added `MediaStorage:BlobRootPath` to API `appsettings*.json` files.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 5)
**Status:** ✅ Slice 5 Delivered (2026-05-10)

### Completion Mark
- [x] Migrate portal logo upload to provider-backed object persistence.
- [x] Add key-based logo streaming endpoint for branding rendering.
- [x] Add guardrails to keep anonymous logo streaming scoped to branding-logo objects.
- [x] Preserve backward compatibility for existing `data:image/*` logo settings values.
- [x] Confirm no database migration is required.

### Implementation Summary
- `PortalSettingsController.UploadLogo` now validates and persists uploaded logos via `IMediaStorageService` and returns a provider-backed URL.
- Added `PortalSettingsController.GetLogoFile` at `GET /api/v1/portal-settings/logo-files/{**storageKey}` to stream stored logos without bearer headers.
- Added `ResolveImageContentType` helper and a `portal-branding/logo` key-category guard for controlled anonymous access.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 6)
**Status:** ✅ Slice 6 Delivered (2026-05-10)

### Completion Mark
- [x] Extend media storage abstraction with temporary read URL support.
- [x] Add provider-level temporary signed URL generation support.
- [x] Upgrade portal logo read flow to redirect-first temporary URL behavior with byte-stream fallback.
- [x] Add `SignedUrlSecret` placeholders in API configuration.
- [x] Confirm no database migration is required.

### Implementation Summary
- Added `GenerateTemporaryReadUrlAsync` to `IMediaStorageService`.
- Implemented temporary URL generation in `LocalMediaStorageService` and `BlobMediaStorageService`, with optional HMAC signature based on `MediaStorage:SignedUrlSecret`.
- Updated `PortalSettingsController.GetLogoFile` to redirect to provider temporary URL when available and keep fallback streaming path for compatibility.
- Added `SignedUrlSecret` keys to API `appsettings.json`, `appsettings.Development.json`, and `appsettings.Production.json`.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 7)
**Status:** ✅ Slice 7 Delivered (2026-05-10)

### Completion Mark
- [x] Enforce local signed URL validation (`exp`/`sig`) for portal logo reads when signing is configured.
- [x] Add unsigned legacy-link compatibility redirect to short-lived signed local URLs.
- [x] Add fixed-time signature verification and strict expiry checks.
- [x] Keep provider temporary URL redirect-first behavior plus local byte-stream fallback.
- [x] Confirm no database migration is required.

### Implementation Summary
- Updated `PortalSettingsController` to read `MediaStorageOptions` and enforce signed local logo reads when `SignedUrlSecret` is set.
- Added helpers to build signed local URLs, validate signatures with `CryptographicOperations.FixedTimeEquals`, and enforce expiration.
- Added compatibility redirect so existing unsigned `/logo-files/{key}` links upgrade to signed URLs automatically.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 8)
**Status:** ✅ Slice 8 Delivered (2026-05-10)

### Completion Mark
- [x] Add authenticated storage-key based certificate streaming endpoint.
- [x] Migrate certificate download flow to redirect-first provider/signed URL pattern for storage-backed records.
- [x] Enforce local signed URL validation for certificate-file reads when signing is configured.
- [x] Preserve legacy `/certificates/*` compatibility for existing records.
- [x] Confirm no database migration is required.

### Implementation Summary
- `GraduationController` now injects `IMediaStorageService` + `MediaStorageOptions` for certificate read orchestration.
- Added `GET /api/v1/graduation/certificate-files/{**storageKey}` with role guard and `exp`/`sig` validation.
- Updated `GET /api/v1/graduation/{id}/certificate` to enforce caller ownership checks, preserve legacy path behavior, and redirect storage-backed certificates to temporary or signed URLs.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 9)
**Status:** ✅ Slice 9 Delivered (2026-05-10)

### Completion Mark
- [x] Add storage metadata lookup support to media abstraction.
- [x] Enrich storage save results with content type and object length.
- [x] Implement metadata resolution in local and blob providers.
- [x] Use provider metadata in current logo/certificate streaming endpoints.
- [x] Confirm no database migration is required.

### Implementation Summary
- Added `GetMetadataAsync` and `MediaStorageObjectMetadata` to the storage abstraction.
- Updated `MediaStorageSaveResult` to include `ContentType` and `Length`.
- Implemented metadata lookup plus canonical content-type resolution in `LocalMediaStorageService` and `BlobMediaStorageService`.
- Updated `PortalSettingsController` and `GraduationController` to use provider metadata when streaming files.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture (Stage 28.3 Slice 10)
**Status:** ✅ Slice 10 Delivered (2026-05-10)

### Completion Mark
- [x] Extend storage contract with content hash and download filename metadata.
- [x] Persist integrity/disposition sidecar metadata in local and blob providers.
- [x] Propagate content type and filename metadata from current upload/certificate generation flows.
- [x] Preserve filename-aware certificate downloads across signed local and redirect-first reads.
- [x] Confirm no database migration is required.

### Implementation Summary
- `IMediaStorageService` save and metadata contracts now include SHA-256 content hash and optional download filename metadata.
- `LocalMediaStorageService` and `BlobMediaStorageService` now compute hashes during writes and persist sidecar `.meta.json` metadata for later reads.
- Logo, payment proof, license upload, and certificate generation flows now pass content type and original/download filename metadata into storage.
- `GraduationController` now preserves filename-aware certificate downloads when serving local signed reads.

### Validation Summary
- `dotnet build src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj` — passed.
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Phase 28 — Scalability Architecture
**Status:** ✅ Complete (2026-05-10)

### Completion Mark
- [x] Stage 28.1 — API and App Tier Scaling complete.
- [x] Stage 28.2 — Caching and Background Workloads complete.
- [x] Stage 28.3 — File and Media Strategy complete.

### Implementation Summary
- Phase 28 now provides stateless/load-balanced app behavior, distributed cache and background workload offload, and provider-backed media persistence with signed reads and metadata hardening.
- The full phase completed without introducing database schema changes.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — passed.
- `dotnet test Tabsan.EduSphere.sln --no-build` — **162/162 passed**.

---

## Refactoring-Hosting-Security — Part A + Part B
**Status:** ✅ Fully Complete (2026-05-07) | Commits: f56ccd9, 5e80bc9

### Completion Mark
- [x] Create `appsettings.Production.json` for API, Web, BackgroundJobs
- [x] Update `API/appsettings.Development.json` — Debug logging, CORS origins, EnableSwagger/EnableDetailedErrors flags
- [x] Update `BackgroundJobs/appsettings.Development.json` — add dev connection string
- [x] Add `AppSettings` section to `API/appsettings.json`
- [x] DB retry on failure — `EnableRetryOnFailure(3, 30s, null)` in AddDbContext
- [x] CORS from config — reads `AppSettings:CorsOrigins`; `AddCors` + `UseCors` in pipeline
- [x] `ForwardedHeaders` middleware — registered for non-dev (IIS/nginx/Cloudflare)
- [x] Health check endpoint `/health`
- [x] 5 MB request body limits — Kestrel + IIS + FormOptions
- [x] Startup environment log line
- [x] Swagger gated by `AppSettings:EnableSwagger`
- [x] WeatherForecast boilerplate removed
- [x] `ExceptionHandlingMiddleware` — global handler, no stack traces in prod, TraceId in response
- [x] `FileUploadValidator` — magic bytes, MIME, extension, 5 MB limit
- [x] Web session cookie — `SameSite=Strict` + `SecurePolicy=Always`
- [x] `.gitignore` — `*.pfx`, `*.key`, `logs/`, `appsettings.*.local.json`, `secrets/`
- [x] Serilog file sink — rolling daily log `logs/app-.log`; 30-file retention; env-aware min level (Debug dev / Warning prod)
- [x] `UserSecretsId` in API `.csproj` (`tabsan-edusphere-api-dev`)
- [x] `FileUploadValidator.ValidateImageAsync` added; wired into `PortalSettingsController.UploadLogo`; inline validation guard in `Web/PortalController.SubmitAssignment`

### Implementation Summary
- **Part A — Hosting:** Production config files for all 3 projects; Development config enriched with debug logging and CORS origins; base `appsettings.json` has `AppSettings` section; DB has transient-retry policy; CORS reads from config; ForwardedHeaders for reverse-proxy deployments; health check at `/health`; 5 MB body limits on all hosting stacks; startup env log; Swagger controlled by flag.
- **Part B — Security:** `ExceptionHandlingMiddleware` first in pipeline — maps exception types to HTTP codes, returns `application/problem+json`, no exception details in production; `FileUploadValidator` static helper with magic-bytes + MIME + extension + size checks; session cookie now `SameSite=Strict` + `SecurePolicy=Always`; `.gitignore` excludes certificates, secrets, and log directories.

### Validation Summary
- `dotnet build Tabsan.EduSphere.API.csproj` — **0 errors, 0 warnings**
- Integration test suite — **69/69 passed**
- Commit: `f56ccd9` + `5e80bc9` pushed to `main`; 69/69 tests passed

---

## Issue-Fix Phase 3 — Faculty Workflow Repair
**Status:** ✅ Complete (2026-05-07)

**Stages completed:**
- 3.1 — Faculty Courses/Offerings 403: replaced `Forbid()` with empty-list response in `CourseController.GetAll` and `GetOfferings`
- 3.2 — Faculty Assignments empty dropdown: `GetMyOfferings` now filters all offerings by faculty's assigned dept IDs
- 3.3 — Faculty Enrollments 403: same API fix; `PortalController.Enrollments` dead branch removed
- 3.4 — Faculty Students 403: `StudentController.GetAll` no longer returns 403, silently scopes by dept
- 3.5/3.6/3.7 — Attendance/Results/Quizzes empty dropdowns: covered by Stage 3.2 fix
- 3.8 — Faculty FYP 403 / can't create: `FypController.admin-create` policy → `"Faculty"`; portal Fyp action loads students for faculty; Fyp view shows Create button for Faculty role

**Validation:**
- `dotnet build` — 0 errors
- 78/78 tests passed (70 integration + 7 unit + 1 contract)

---

## Issue-Fix Phase 4 — Student Workflow Repair
**Status:** ✅ Complete (2026-05-07)

### Completion Mark
- [x] Stage 4.1 — Assignment submission end-to-end (file upload + text, status merge, submit modal)
- [x] Stage 4.2 — Timetable department auto-resolved from student profile; `Guid.Empty` guard added
- [x] Stage 4.3 — Assignments semester filter + semester-scoped offering dropdown
- [x] Stage 4.4 — Results semester filter + fallback to student-safe endpoints on 403
- [x] Stage 4.5 — Quizzes semester filter + Upcoming/Pending/Completed status badges
- [x] Stage 4.6 — FYP menu gated to ≥8th semester; student completion-request; faculty approval; auto-complete; FYP result row in Results

### Implementation Summary
- **Stage 4.1**: `AssignmentController.Submit` + `PortalController.SubmitAssignment` (file → GUID path + API call); `Assignments.cshtml` submit modal; `EduApiClient.SubmitAssignmentAsync`.
- **Stage 4.2**: `PortalController.Timetable` student branch resolves `DepartmentId` from `GetMyStudentProfileAsync`; `Guid.Empty` guard prevents bad API calls; falls back to dashboard config.
- **Stage 4.3/4.4/4.5**: `Assignments.cshtml`, `Results.cshtml`, `Quizzes.cshtml` each have a semester selector that persists via route/query; offering dropdowns filtered to selected semester; Results falls back to student-safe endpoint on 403; Quizzes derive status from availability window dates.
- **Stage 4.6**: `FypController.RequestCompletion` (student) + `FypController.ApproveCompletion` (faculty); `FypCompletionApproval` domain entity + EF migration `Phase4FypCompletionApprovalFlow`; auto-complete when all approvers approve; FYP row rendered in `Results.cshtml` for completed projects. FYP sidebar menu hidden until `CurrentSemesterNumber >= 8`.
- **Auth consistency**: `EduApiClient` login flow resolves API base URL before token acquisition, removing intermittent student 401s.

### Validation Summary
- 12/12 assignment integration tests passed.
- 78/78 full integration suite passed.
- 0 build errors across all projects.

---

## Issue-Fix Phase 5 — Reporting and Export Center
**Status:** ✅ Complete (2026-05-07)

### Completion Mark
- [x] Stage 5.1 — Assignment and Quiz summary report APIs + portal pages
- [x] Stage 5.2 — CSV/PDF export for Attendance, Results, Assignments, Quizzes (Excel retained)
- [x] Stage 5.3 — SuperAdmin unrestricted report scope verified
- [x] Stage 5.4 — Admin report scope bounded by Phase 6 assigned departments (closed)
- [x] Stage 5.5 — Faculty scope enforced on department/offering filters and report data/export endpoints

### Implementation Summary
- **Stage 5.1**: Added `GET /api/v1/reports/assignment-summary` and `GET /api/v1/reports/quiz-summary` with matching export endpoints. Added `ReportAssignments.cshtml` and `ReportQuizzes.cshtml` portal pages.
- **Stage 5.2**: Added `/export/csv` and `/export/pdf` variants for all four report types in `ReportController` and `ReportService`. Web portal proxy actions + Excel/CSV/PDF export buttons on each report page.
- **Stage 5.3**: SuperAdmin retains unrestricted catalog, data, and export scope.
- **Stage 5.4**: Admin report scope enforced via Phase 6 `AdminDepartmentAssignment` model; `EnforceAdminDepartmentScopeAsync` guard in `ReportController`. All 9 report portal pages now show a friendly guidance message for Admin when the required department or offering filter is missing (mirrors Faculty guidance pattern).
- **Stage 5.5**: `DepartmentController.GetAll`, `CourseController.GetAll/GetOfferings/GetMyOfferings` return faculty-scoped data; `EnforceFacultyOfferingScopeAsync` guard rejects report requests for unowned offerings.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` succeeded after all report + scope changes.
- CSV export returns `text/csv`; PDF export returns `application/pdf` across all four report types.

---

## Phase 1 - Navigation, Session Stability, Sidebar Structure
**Status:** ✅ Complete

---

## Issue-Fix Phase 6.1 - Dedicated Admin User Management Extension
**Status:** ✅ Complete

### Completion Mark
- [x] Added dedicated SuperAdmin Admin Users management page.
- [x] Added Admin create/update API endpoints.
- [x] Added inline multi-department assignment sync workflow for Admin users.
- [x] Added search/filter and select-all/clear UX controls for assignment operations.

### Implementation Summary
- New API controller `AdminUserController` added with SuperAdmin-only endpoints for listing, creating, and updating Admin users.
- User repository enhanced with `GetUsersByRolesAsync(..., includeInactive)` to support management use-cases.
- Web layer now includes:
  - `PortalController.AdminUsers` page flow
  - create/update actions for Admin users
  - shared assignment sync helper
  - new `Views/Portal/AdminUsers.cshtml`
- Existing Departments assignment panel retained and linked to dedicated Admin Users page.

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` passed.
- Focused integration tests for new flow were added; execution currently blocked by pre-existing migration setup issue in integration environment (`license_state` duplicate `ActivatedDomain` column).

---

## Issue-Fix Phase 4 Option A/C - Web User Import + Forced Password Change
**Status:** ✅ Complete

### Completion Mark
- [x] User Import portal page and CSV upload flow available and validated.
- [x] Forced password change page/action implemented and enforced from login flow.
- [x] Integration tests added for import authorization and force-change-password end-to-end behavior.

### Implementation Summary
- Confirmed/kept User Import portal implementation (`UserImport` + `ImportUsersCsv`) with summary output.
- Added forced password change session flow in Web:
  - login captures `MustChangePassword` and sets session flag
  - portal action guard redirects to force-change page until reset is completed
  - force-change page posts to API `POST /api/v1/auth/force-change-password`
  - success clears session flag and unlocks normal portal navigation
- Added integration test file: `UserImportAndForceChangeIntegrationTests.cs`.

### Validation Summary
- Focused tests passed: `UserImportAndForceChangeIntegrationTests` (`2/2`).
- Full integration suite passed: `Tabsan.EduSphere.IntegrationTests` (`70/70`).

### Stage 1.1 - Fix Session/Sidebar Reset Bug
- [x] Fix issue where opening Buildings causes sidebar to reset to legacy menu and forces re-login.
- [x] Ensure sidebar remains dynamic and role-driven across all portal pages.

### Stage 1.2 - Sidebar Grouping and SuperAdmin Coverage
- [x] Group sidebar by:
  - Student Related
  - Faculty Related
  - Finance Related
  - Settings (at bottom)
- [x] Ensure all menus are visible to SuperAdmin.
- [x] Ensure all menus appear in Sidebar Settings for role assignment.

### Stage 1.3 - Add Dashboard Settings Menu
- [x] Add new Settings item: Dashboard Settings.
- [x] Support university name text, brand initials, subtitle text, footer text.
- [x] Layout brand section reads from DB branding values (with session cache fallback).
- [x] Footer text driven by DB setting.

### Implementation Summary
- Hardened sidebar loading in `_Layout.cshtml` by caching dynamic menu payload in session (`VisibleSidebarMenusCache`) and reusing it on API failure.
- Removed layout-level redirect-return behavior that could break rendering.
- Implemented grouped dynamic sidebar rendering (`Overview`, `Faculty Related`, `Student Related`, `Finance Related`, `Settings`).
- Added `portal_settings` key-value table in DB with EF migration `Phase1DashboardBranding`.
- Added `PortalSetting` domain entity, `IPortalBrandingService` / `PortalBrandingService`, `PortalSettingsController` API endpoint, `GetPortalBrandingAsync` / `SavePortalBrandingAsync` in `EduApiClient`.
- Added `DashboardSettings` action + view in `PortalController`; seeded `dashboard_settings` sidebar menu item.
- Layout brand area (initials, name, subtitle, footer) now rendered from DB settings with session-cached fallback.

### Validation Summary
- Verified SuperAdmin login renders grouped dynamic sidebar with full menu set.
- Verified opening Buildings keeps full grouped sidebar visible with no forced sign-out.
- Verified Sidebar Settings page shows 29 items including Report Center, Payments, Enrollments.
- Verified Dashboard Settings page renders with form, default branding values pre-filled, live preview, and footer text from settings.

---

## Phase 2 - Timetable and Core Lookup Data Visibility
**Status:** ✅ Complete

### Stage 2.1 - Faculty/Student Timetable Data
- [x] Fix My Timetable (Faculty) data binding.
- [x] Fix My Timetable (Student) data binding.
- [x] Confirm Timetable Admin, Faculty, Student views all load expected rows.

### Stage 2.2 - Building, Student, Department, Course Visibility
- [x] Fix Buildings list retrieval.
- [x] Fix Students list retrieval (names visible).
- [x] Fix Departments list retrieval (names visible).
- [x] Fix Courses page active offering retrieval.

**Implementation Summary (Stage 2.2)**

**Problem:** Portal pages for Buildings, Students, Departments, and Courses existed but were not showing proper related entity data (e.g., missing student names, course department names, course offering faculty).

**Fix Applied:**
1. **StudentProfileRepository**: Ensured `Program` and `Department` navigation properties are loaded via `.Include()` statements.
2. **StudentController.GetAll()**: Updated API response to map `ProgramName`, `DepartmentName`, and `Status` from included entities.
3. **CourseRepository**: Added new `GetOfferingsByDepartmentAsync()` method to retrieve offerings filtered by department. Updated existing `GetOfferingsBySemesterAsync()` and `GetOfferingsByFacultyAsync()` with proper Course and Semester includes.
4. **ICourseRepository interface**: Added `GetOfferingsByDepartmentAsync()` method signature for consistency.
5. **CourseController**: Updated `GetAll()` to include `DepartmentName` mapping. Refactored `GetOfferings()` endpoint to accept both `semesterId` and `departmentId` query parameters, supporting department-filtered course offerings.

**Files Modified:**
- [src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs)
- [src/Tabsan.EduSphere.API/Controllers/StudentController.cs](../../src/Tabsan.EduSphere.API/Controllers/StudentController.cs)
- [src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs)
- [src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs](../../src/Tabsan.EduSphere.Domain/Interfaces/ICourseRepository.cs)
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](../../src/Tabsan.EduSphere.API/Controllers/CourseController.cs)

**Validation Summary**
- ✓ Build succeeded with all fixes applied
- ✓ StudentController.GetAll() returns Program and Department names for each student profile
- ✓ CourseController.GetAll() returns Department name for each course
- ✓ CourseController.GetOfferings() endpoint now supports both `?semesterId=...` and `?departmentId=...` filters
- ✓ Portal views (Buildings, Students, Departments, Courses) ready to consume updated API responses
- ✓ Commit: e15e0b6

---

### Stage 2.3 - CRUD Entry Points
- [x] Add Students create flow.
- [x] Add Departments create flow.
- [x] Add Active Offerings create/edit/delete flow.

### Implementation Summary
**Problem:** Timetable API endpoints were returning incomplete data due to missing EF Include statements in repository queries, causing null references during DTO mapping.

**Fix Applied:**
1. **TimetableRepository.GetTeacherEntriesAsync()**: Added `.Include(e => e.Building)` to include the Building navigation property alongside existing Room.Building include.
2. **TimetableRepository.GetByDepartmentAsync()**: Added `.Include(t => t.Department)`, `.Include(t => t.AcademicProgram)`, `.Include(t => t.Semester)` for proper DTO mapping.
3. **TimetableRepository.GetPublishedByDepartmentAsync()**: Added `.Include(t => t.Department)`, `.Include(t => t.AcademicProgram)`, `.Include(t => t.Semester)` for proper DTO mapping.
4. **TimetableRepository.GetByIdWithEntriesAsync()**: Added separate `.Include(t => t.Entries).ThenInclude(e => e.Building)` to ensure Building data is loaded for all entries.

**Files Modified:**
- [src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/TimetableRepository.cs)

### Validation Summary
- ✓ Build succeeded with all fixes applied
- ✓ Faculty timetable query includes Department, AcademicProgram, Semester, Building, and Room.Building
- ✓ Student timetable query includes all required related data for complete DTO mapping
- ✓ Test data is seeded in MinimalSeed.sql: 1 published timetable for CS dept with 2 entries assigned to faculty.test
- ✓ API endpoints ready to return complete timetable data without null reference errors

---

### Stage 2.3 - CRUD Entry Points
- [x] Add Students create flow.
- [x] Add Departments create flow.
- [x] Add Active Offerings create/edit/delete flow.

**Implementation Summary (Stage 2.3)**

**New CourseOffering API Endpoints:**
1. **PUT /api/v1/course/offerings/{id}/maxenrollment** - Update max enrollment with validation
2. **PUT /api/v1/course/offerings/{id}/close** - Close enrollment for an offering
3. **PUT /api/v1/course/offerings/{id}/reopen** - Re-open enrollment for an offering
4. **DELETE /api/v1/course/offerings/{id}** - Soft-delete offering (AuditableEntity)

**Portal Page Enhancements:**
1. **Students.cshtml** - Added "Add Student" button (Admin/SuperAdmin only), modal form with fields:
   - Registration Number, Program, Department, Admission Date
2. **Departments.cshtml** - Added "Add Department" button, modal form with fields:
   - Department Code, Department Name
3. **Courses.cshtml** - Added "Add Course" and "Add Offering" buttons on respective panels:
   - Course modal: Code, Title, Credit Hours, Department
   - Offering modal: Course, Semester, Faculty (optional), Max Enrollment

**Supporting Changes:**
- Added `UpdateMaxEnrollmentRequest` DTO to `AcademicDtos.cs`
- All CRUD endpoints leveraged: StudentController.Create, DepartmentController.Create/Update/Delete, CourseController.Create/Update/Delete, CourseController.CreateOffering

**Files Modified:**
- [src/Tabsan.EduSphere.API/Controllers/CourseController.cs](../../src/Tabsan.EduSphere.API/Controllers/CourseController.cs): 4 new offering endpoints
- [src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs](../../src/Tabsan.EduSphere.Application/DTOs/Academic/AcademicDtos.cs): UpdateMaxEnrollmentRequest
- [src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Students.cshtml): Create button and modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Departments.cshtml): Create button and modal
- [src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml](../../src/Tabsan.EduSphere.Web/Views/Portal/Courses.cshtml): Create buttons and modals

**Validation Summary**
- ✓ Build succeeded (0 errors, 2 MailKit warnings)
- ✓ CourseOffering endpoints support full lifecycle: create, assign faculty, update enrollment, close/reopen, soft-delete
- ✓ Portal pages show create buttons/modals for Students, Departments, Courses, Offerings (role-gated)
- ✓ Modal forms include proper field labels and validation
- ✓ Commit: 7f3330b

---

## Phase 3 - Assignment, Attendance, Results, Quizzes, FYP Access and Workflows
**Status:** ✅ Complete

### Stage 3.1 - 403 Authorization Fixes
- [x] Resolve 403 in Assignments.
- [x] Resolve 403 in Attendance.
- [x] Resolve 403 in Results.
- [x] Resolve 403 in Quizzes.
- [x] Resolve 403 in FYP.

**Implementation Summary (Stage 3.1)**

**Root Cause:** Five module controllers used `[Route("api/[controller]")]` (no `v1` prefix) while `EduApiClient.cs` in the Web project consistently calls `api/v1/` prefixed paths. This caused 404 (not 403) at the HTTP level — ASP.NET then returns 400/404 which the portal surfaces as access errors.

**Additionally:** `EduApiClient.GetMyAttemptsAsync()` calls `api/v1/quiz/my-attempts` (flat path) but `QuizController` only had `{id:guid}/my-attempts` — no flat endpoint existed.

**Fix Applied:**
1. **AssignmentController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
2. **AttendanceController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
3. **ResultController**: Changed `[Route("api/[controller]")]` → `[Route("api/v1/[controller]")]`
4. **QuizController**: Changed `[Route("api/quiz")]` → `[Route("api/v1/quiz")]`; added `GET my-attempts` flat endpoint calling `IQuizService.GetAllMyAttemptsAsync()`
5. **FypController**: Changed `[Route("api/fyp")]` → `[Route("api/v1/fyp")]`
6. **IQuizRepository + QuizRepository**: Added `GetAllAttemptsForStudentAsync(Guid studentProfileId)` returning all attempts across all quizzes
7. **IQuizService + QuizService**: Added `GetAllMyAttemptsAsync(Guid studentProfileId)` service method

**Files Modified:**
- [src/Tabsan.EduSphere.API/Controllers/AssignmentController.cs](../../src/Tabsan.EduSphere.API/Controllers/AssignmentController.cs)
- [src/Tabsan.EduSphere.API/Controllers/AttendanceController.cs](../../src/Tabsan.EduSphere.API/Controllers/AttendanceController.cs)
- [src/Tabsan.EduSphere.API/Controllers/ResultController.cs](../../src/Tabsan.EduSphere.API/Controllers/ResultController.cs)
- [src/Tabsan.EduSphere.API/Controllers/QuizController.cs](../../src/Tabsan.EduSphere.API/Controllers/QuizController.cs)
- [src/Tabsan.EduSphere.API/Controllers/FypController.cs](../../src/Tabsan.EduSphere.API/Controllers/FypController.cs)
- [src/Tabsan.EduSphere.Domain/Interfaces/IQuizRepository.cs](../../src/Tabsan.EduSphere.Domain/Interfaces/IQuizRepository.cs)
- [src/Tabsan.EduSphere.Infrastructure/Repositories/QuizFypRepositories.cs](../../src/Tabsan.EduSphere.Infrastructure/Repositories/QuizFypRepositories.cs)
- [src/Tabsan.EduSphere.Application/Interfaces/IQuizService.cs](../../src/Tabsan.EduSphere.Application/Interfaces/IQuizService.cs)
- [src/Tabsan.EduSphere.Application/Quizzes/QuizService.cs](../../src/Tabsan.EduSphere.Application/Quizzes/QuizService.cs)

**Validation Summary (Stage 3.1)**
- ✓ Build succeeded (0 errors, 2 pre-existing MailKit warnings)
- ✓ All 5 module controllers now at `api/v1/` prefix matching EduApiClient call paths
- ✓ Authorization policies (Faculty/Admin/Student) are valid in Program.cs — no policy changes needed
- ✓ `GET api/v1/quiz/my-attempts` endpoint added for student portal summary view

### Stage 3.2 - Data Entry Workflows
**Status:** ✅ Complete

- [x] Add Assignments create/publish/delete + grade submissions workflow.
- [x] Add Attendance bulk-mark workflow (Faculty sees enrolled students roster, selects status per student).
- [x] Add Results enter result (Faculty selects student from roster, enters type/marks) + publish-all workflow.
- [x] Add Quizzes create/publish/delete workflow.
- [x] Add FYP propose (Student), approve/reject with remarks (Admin) workflow.

### Implementation Summary
- **EduApiClient** — Added 13 write methods to `IEduApiClient` interface and `EduApiClient` class: `CreateAssignmentAsync`, `PublishAssignmentAsync`, `DeleteAssignmentAsync`, `GradeSubmissionAsync`, `BulkMarkAttendanceAsync`, `CreateResultAsync`, `PublishAllResultsAsync`, `CreateQuizAsync`, `PublishQuizAsync`, `DeleteQuizAsync`, `ProposeFypProjectAsync`, `ApproveFypProjectAsync`, `RejectFypProjectAsync`. Added private `DeleteAsync` HTTP helper.
- **PortalController** — Added 13 corresponding `[HttpPost, ValidateAntiForgeryToken]` actions for all 5 modules.
- **PortalViewModels** — Added `Roster: List<EnrollmentRosterItem>` to `AttendancePageModel` and `ResultsPageModel`.
- **PortalController (GET)** — Attendance + Results GET actions now load enrollment roster via `GetEnrollmentRosterAsync` when offeringId is selected and user is Faculty/Admin.
- **Assignments.cshtml** — Added "Create Assignment" button + Bootstrap 5 modal, Publish/Delete inline forms per row, Grade modal triggered from submissions table.
- **Attendance.cshtml** — Added "Mark Attendance" panel (Faculty/Admin) showing enrolled students grid with per-student date + status select, posts to `BulkMarkAttendance`.
- **Results.cshtml** — Added "Enter Result" button + modal (student select from roster, result type, marks), "Publish All" button.
- **Quizzes.cshtml** — Added "Create Quiz" button + modal, Publish/Delete inline forms per quiz card (Faculty/Admin gated).
- **Fyp.cshtml** — Added "Propose Project" modal (Student), Approve inline form + Reject modal with remarks (Admin gated).

### Validation Summary
- Solution builds with 0 errors (1 warning: MailKit vulnerability unrelated to this stage).
- All Razor views compile successfully with role-gated write UI.
- CSRF protection (`[ValidateAntiForgeryToken]` + `@Html.AntiForgeryToken()`) applied to all write forms.

### Stage 3.3 - Result-Driven Promotion Logic
**Status:** ✅ Complete

- [x] Add Promote column in result entry with Yes/No option for failed students.
- [x] Implement automatic promotion to next semester based on entered result decision.
- [x] Remove/replace manual promotion dependency where required.

### Implementation Summary
- **ResultItem / ResultApiDto** — Added `StudentProfileId` field so the Results view can identify each student per row.
- **MapResult** — Updated to pass `StudentProfileId` from API response to web model.
- **PortalController.CreateResult** — Added `bool promote` parameter; when checked, automatically calls `PromoteStudentAsync(studentProfileId)` after result creation.
- **PortalController.PromoteStudentFromResult** — New `[HttpPost]` standalone action for per-row Promote button; calls existing `EduApiClient.PromoteStudentAsync`; redirects to Results page with success message.
- **Results.cshtml** — Added "Promote" column (Faculty/Admin only) with a per-student inline POST form; added "Promote to next semester" checkbox in the Enter Result modal (only visible when ResultType = "Final"); JavaScript toggles checkbox visibility on type change.

### Validation Summary
- Solution builds with 0 errors.
- Razor view compiles successfully.
- Promotion uses existing `POST api/v1/student-lifecycle/{id}/promote` endpoint — no new API routes needed.

---

## Phase 4 - Reporting and Export Completion
**Status:** ✅ Complete

### Stage 4.1 - Report Center Functional Completeness
**Status:** ✅ Complete

- [x] Ensure Report Center menu is visible in sidebar by role and opens correctly.
- [x] Fix Department Summary report.
- [x] Fix Result Summary report.
- [x] Fix Semester Result report.
- [x] Ensure role/department/subject/semester filters work end-to-end.

### Stage 4.2 - Add Additional Reports
**Status:** ✅ Complete

- [x] Student Transcript — full academic record per student with Excel export
- [x] Low Attendance Warning — students below configurable attendance threshold
- [x] FYP Status Report — Final Year Project status overview with dept/status filters
- [x] All 6 infrastructure layers implemented (DTOs, Domain, Repository, Service interface, Service impl, API controller, EduApiClient, PortalController, ViewModels, Razor views)
- [x] ReportCenter.cshtml switch updated with 3 new keys
- [x] DatabaseSeeder + ReportKeys constants updated; Student Transcript adds Student role assignment

### Implementation Summary (Stage 4.1)
- **Root cause identified**: DB-seeded report keys (`attendance-report`, `results-report`, `dept-summary`) used hyphens while `ReportCenter.cshtml` switch used underscores — every catalog card resolved to `"#"`.
- **ReportCenter.cshtml** — Updated switch to handle both old hyphenated and new underscore keys; added `dept-summary` → ReportEnrollment, `semester-results` → ReportSemesterResults.
- **Static sidebar (_Layout.cshtml)** — Added "Report Center" link inside the `Admin Tools` section (Faculty/Admin) in the static fallback menu.
- **Semester Results report** — Full chain added: `SemesterResultsRowItem`/`SemesterResultsWebModel`/`ReportSemesterResultsPageModel` in PortalViewModels; `GetSemesterResultsReportAsync` in IEduApiClient + EduApiClient; `ReportSemesterResults` GET action in PortalController; `ReportSemesterResults.cshtml` view with semester/department filters.
- **Excel export actions** — Added `ExportAttendanceSummary`, `ExportResultSummary`, `ExportGpaReport` GET actions to PortalController; proxied through new `ExportAttendanceSummaryAsync`, `ExportResultSummaryAsync`, `ExportGpaReportAsync` methods in IEduApiClient + EduApiClient (uses new `GetBytesAsync` private helper).
- **DB Seeds** — Both `1-MinimalSeed.sql` and `2-FullDummyData.sql` updated to seed `semester-results` report definition with Admin + Faculty role assignments.

### Implementation Summary (Stage 4.2)
- **DTOs** — 3 new request/response record sets in `ReportDtos.cs`
- **Domain** — 3 new method signatures + 4 raw row record types in `IReportRepository.cs`
- **Repository** — 3 new EF Core query methods in `ReportRepository.cs`
- **Service interface** — 4 new method signatures in `IReportService.cs`
- **Service impl** — 4 new methods (including Excel export) in `ReportService.cs`
- **API** — 5 new endpoints in `ReportController.cs` (transcript, transcript/export, low-attendance, fyp-status)
- **EduApiClient** — 4 impl methods + 6 private sealed DTO classes added; interface signatures previously added
- **ViewModels** — 9 new classes in `PortalViewModels.cs` (3 row items, 3 web models, 3 page models)
- **PortalController** — 3 GET actions + 1 export action added
- **Views** — `ReportTranscript.cshtml`, `ReportLowAttendance.cshtml`, `ReportFypStatus.cshtml` created
- **ReportCenter.cshtml** — 3 new switch cases added
- **ReportKeys.cs** — 3 new constants: `StudentTranscript`, `LowAttendanceWarning`, `FypStatus`
- **DatabaseSeeder.cs** — 3 new `ReportDefinition` rows seeded with role assignments

### Validation Summary
- Solution builds with 0 errors, 0 warnings.
- All 8 reports in the catalog now resolve to their correct views.
- Export buttons call working Portal proxy actions.
- Semester Results view requires a semester selection before querying (SemesterId is required by the API).

---

## Phase 5 - Settings Pages Functional Save Actions
**Status:** ✅ Complete

### Stage 5.1 - Report Settings Save
- [x] Add/repair save action and success/error feedback in Report Settings.
  - All save actions already wired (CreateReport, ToggleReport, UpdateReportRoles POST actions).
  - Fixed alert styling: success messages show `alert-success`, error messages show `alert-danger` with matching icons.

### Stage 5.2 - Module Settings Save
- [x] If modules exist: render all modules and support activate/deactivate + save.
  - 14 modules seeded (Authentication, Departments, SIS, Courses, Assignments, Quizzes, Attendance, Results, Notifications, FYP, AI Chat, Reports, Themes, Advanced Audit).
  - ToggleModule and UpdateModuleRoles POST actions confirmed working. Mandatory modules cannot be deactivated.
- [x] If modules do not exist: remove Module Settings menu and related dead routes.
  - Not applicable — modules exist and are properly seeded.

### Stage 5.3 - Sidebar Settings Save
- [x] Add/repair save action for role assignments and visibility toggles.
  - Role checkboxes auto-submit via JS `change` event handler (updates hidden fields then submits).
  - Status checkboxes use `onchange="this.form.submit()"` for instant toggle.
  - TempData feedback already differentiated (success via TempData, error via Model.Message with alert-danger).

### Stage 5.4 - Theme Settings Expansion
- [x] Add more themes.
  - Added 5 new themes: Steel Blue, Forest Green, Amber Gold, Warm Copper, Indigo Dusk.
  - Total themes: 20 (15 existing + 5 new).
  - CSS `data-theme` blocks added to `wwwroot/css/site.css` for all 5 new themes.
  - `ThemeSettingsPageModel.Themes` updated with 5 new entries.
- [x] Ensure themes persist and apply consistently.
  - Fixed: `_Layout.cshtml` now loads the current user's theme from API on every page request (with session fallback).
  - `<html>` tag dynamically sets `data-theme` attribute from saved theme key.
  - Theme is cached in session under key `CurrentThemeCache` to minimise API calls.

### Implementation Summary
- **Files changed:**
  - `src/Tabsan.EduSphere.Web/Views/Shared/_Layout.cshtml` — theme loading + `data-theme` on `<html>` tag
  - `src/Tabsan.EduSphere.Web/wwwroot/css/site.css` — 5 new theme blocks
  - `src/Tabsan.EduSphere.Web/Models/Portal/PortalViewModels.cs` — 5 new `ThemeOption` entries
  - `src/Tabsan.EduSphere.Web/Views/Portal/ThemeSettings.cshtml` — contextual success/danger alerts
  - `src/Tabsan.EduSphere.Web/Views/Portal/ReportSettings.cshtml` — contextual success/danger alerts
  - `src/Tabsan.EduSphere.Web/Views/Portal/ModuleSettings.cshtml` — contextual success/danger alerts

### Validation Summary
- Build: `dotnet build Tabsan.EduSphere.sln` → **0 errors, 0 warnings**
- Themes: 20 themes available in Theme Settings; CSS variables defined for all; layout applies saved theme on every page load
- Settings feedback: success = green alert + check icon; error = red alert + X icon

---

## Phase 6 - Notifications and Analytics
**Status:** ✅ Complete

### Stage 6.1 - Notifications 404 Fix
- [x] Fix Notifications endpoint mismatch or missing route.
- [x] Verify notification list, read state, and mark-all-read behavior.

### Stage 6.2 - Analytics Data Rendering
- [x] Replace random/static code output with real analytics data.
- [x] Validate Performance, Attendance, Assignment analytics cards and sections.

### Implementation Summary
- **Stage 6.1**: `NotificationController` had `[Route("api/[controller]")]` resolving to `api/notification`, while `EduApiClient` called `api/v1/notification/...`. Fixed by changing the controller route to `[Route("api/v1/[controller]")]`.
- **Stage 6.2**: `EduApiClient` analytics methods returned raw JSON strings; the view displayed them in `<pre><code>` blocks. Fixed by:
  1. Updating `IEduApiClient` interface: replaced `GetPerformanceAnalyticsJsonAsync`, `GetAttendanceAnalyticsJsonAsync`, `GetAssignmentAnalyticsJsonAsync` (returning `string?`) with typed versions returning `DepartmentPerformanceReport?`, `DepartmentAttendanceReport?`, `AssignmentStatsReport?`.
  2. Updating `EduApiClient` implementation to use `GetAsync<T>()` helper.
  3. Replacing `PerformanceJson`, `AttendanceJson`, `AssignmentJson` string fields in `AnalyticsPageModel` with typed `Performance`, `Attendance`, `Assignments` DTO properties.
  4. Updating `PortalController.Analytics` to call typed methods and populate summary cards from real data.
  5. Rewrote `Analytics.cshtml`: accordion panels now render Bootstrap 5 tables with real student/course rows instead of raw JSON.

### Validation Summary
- Build: ✅ 0 errors, 0 warnings
- Notifications: Route mismatch resolved — inbox, badge, mark-all-read all route correctly to `api/v1/notification/...`; per-notification mark-as-read button added to inbox view, posts to new `MarkNotificationRead` action
- Analytics: Performance, Attendance, Assignment sections render as proper responsive tables with per-row data; summary cards display average marks, attendance %, and assignment count from live API data.

---

## Phase 7 - Finance and Payments Module Completion
**Status:** ✅ Complete

### Stage 7.1 - Finance Sidebar and Navigation
- [x] Add Finance-related menus and grouping in sidebar.
- [x] Fixed URL bug in `GetPaymentsByStudentAsync` (was `api/v1/payment-receipt/…`, corrected to `api/v1/payments/…`).

### Stage 7.2 - Fees and Receipts Admin Workflows
- [x] Add create/edit/delete fee receipt setup (admin Create + Confirm + Cancel).
- [x] Admin can create receipts per student with amount, description, and due date.
- [x] Admin can confirm (mark Paid) or cancel any non-terminal receipt.

### Stage 7.3 - Student Payment Flow
- [x] Students can view their own receipts (GET /mine via JWT).
- [x] Students can submit proof (transaction ID / reference note) via POST /mark-submitted.
- [x] Admin verification workflow: Submitted → Paid via Confirm action.
- [x] Notifications sent on receipt creation, proof submission, confirmation, and cancellation.

### Implementation Summary
- **Domain**: `PaymentReceipt` state machine unchanged (Pending → Submitted → Paid/Cancelled).
- **Infrastructure**: Added `GetAllReceiptsAsync` and `GetStudentProfileByUserIdAsync` to `StudentLifecycleRepository`.
- **Application**: Added `GetAllReceiptsAsync`, `GetReceiptsByUserAsync` to `IStudentLifecycleService` / `StudentLifecycleService`. Injected `INotificationService`; notifications fire on Create, SubmitProof, Confirm, and Cancel.
- **API**: Added `GET api/v1/payments` (admin all), `GET api/v1/payments/mine` (student by JWT), `POST api/v1/payments/{id}/mark-submitted` (text proof).
- **Web (EduApiClient)**: Added `GetAllPaymentsAsync`, `GetMyPaymentsAsync`, `CreatePaymentAsync`, `ConfirmPaymentAsync`, `CancelPaymentAsync`, `SubmitProofAsync`. Expanded `PaymentApiDto` and `MapPayment`.
- **Web (PortalController)**: `Payments(GET)` branches on `IsStudent`; added `CreatePayment`, `ConfirmPayment`, `CancelPayment`, `SubmitProof` POST actions.
- **Web (Payments.cshtml)**: Rebuilt — admin sees Create Receipt form + filter + Confirm/Cancel per row; student sees own receipts + Submit Proof collapse form.

### Validation Summary
- All layers build with 0 CS/RZ errors (file-lock MSB warnings from running processes only).
- `StudentLifecycleService` constructor now takes `INotificationService`; registered via DI.
- Razor view fixed: `StudentItem.FullName` used correctly; `selected` attribute valid HTML.

---

## Phase 8 - Enrollments Completion
**Status:** ✅ Complete

### Stage 8.1 - Data and Dropdown Fixes
- [x] Fix empty enrollments data grid.
- [x] Fix empty dropdown data sources.

### Stage 8.2 - Enrollments CRUD
- [x] Add create/edit/delete enrollment workflows.

### Implementation Summary

**Root Causes Fixed (Stage 8.1):**
1. **Empty dropdown**: `CourseController.GetOfferings()` returned an empty list when no `semesterId` or `departmentId` filter was provided. Added `ICourseRepository.GetAllOfferingsAsync()` (with `CourseRepository` implementation) and updated the controller else-branch to call it. Also fixed field name mismatches: `CourseName` → `CourseTitle`, `IsOpen` → `IsActive` — matching `OfferingApiDto` in EduApiClient.
2. **Empty roster grid**: `EnrollmentController.GetRoster()` returned `{StudentProfileId, RegNo, EnrolledAt}` which did not match `RosterApiDto` fields (`Id, StudentName, RegistrationNumber, ProgramName, SemesterNumber`). Fixed the response mapping. Added `.ThenInclude(sp => sp.Program)` to `EnrollmentRepository.GetByOfferingAsync()` so `ProgramName` is available.

**New CRUD Workflows (Stage 8.2):**
- **Admin — Enroll Student**: New `POST /api/v1/enrollment/admin` endpoint (reuses `EnrollmentService.EnrollAsync`). Portal action `EnrollStudent` + modal in Enrollments.cshtml.
- **Admin — Drop Enrollment**: New `DELETE /api/v1/enrollment/admin/{enrollmentId}` endpoint backed by new `IEnrollmentService.AdminDropByIdAsync` (requires new `IEnrollmentRepository.GetByIdAsync`). Portal action `AdminDropEnrollment` + per-row Drop button.
- **Student — My Courses View**: `Enrollments GET` now branches on `IsStudent`; loads `GetMyEnrollmentsAsync()` → `GET api/v1/enrollment/my-courses`. `MyCourses` endpoint updated to also return `CourseOfferingId`.
- **Student — Self-Enroll**: `StudentEnroll` portal action + "Enroll in Course" modal.
- **Student — Drop Own Enrollment**: `StudentDropEnrollment` portal action + per-row Drop button for active enrollments.

**Files Modified:**
- `Domain/Interfaces/ICourseRepository.cs` — `GetAllOfferingsAsync`
- `Domain/Interfaces/IEnrollmentRepository.cs` — `GetByIdAsync`
- `Application/Interfaces/IEnrollmentService.cs` — `AdminDropByIdAsync`
- `Application/DTOs/Academic/AcademicDtos.cs` — `AdminEnrollRequest`
- `Application/Academic/EnrollmentService.cs` — `AdminDropByIdAsync`
- `Infrastructure/Repositories/CourseRepository.cs` — `GetAllOfferingsAsync`
- `Infrastructure/Repositories/AcademicSupportRepositories.cs` — fixed `GetByOfferingAsync` includes + `GetByIdAsync`
- `API/Controllers/CourseController.cs` — `GetOfferings` fixes
- `API/Controllers/EnrollmentController.cs` — `GetRoster` fix, `MyCourses` fix, `AdminEnroll`, `AdminDrop`
- `Web/Services/EduApiClient.cs` — 5 new methods + `MyCourseApiDto`
- `Web/Models/Portal/PortalViewModels.cs` — `MyEnrollmentItem`, expanded `EnrollmentsPageModel`
- `Web/Controllers/PortalController.cs` — `Enrollments` GET update + 4 new POST actions
- `Web/Views/Portal/Enrollments.cshtml` — rebuilt with admin roster + student courses view

### Validation Summary
- Build: ✅ `dotnet build Tabsan.EduSphere.sln` → **0 errors, 0 warnings**
- Enrollment dropdown now populated from all offerings (no filter required).
- Roster returns correct fields: `Id`, `RegistrationNumber`, `ProgramName`, `SemesterNumber`.
- Admin can select an offering, view roster, enroll a student, and drop any enrollment.
- Student view shows own courses, active/dropped status, and can enroll or drop.
- All CSRF tokens (`[ValidateAntiForgeryToken]` + `@Html.AntiForgeryToken()`) applied to all write forms.

---

## Phase 9 - Documentation and Script Regeneration
**Status:** ✅ Complete

### Stage 9.1 - Script Modernization
- [x] Remove obsolete scripts.
- [x] Create new scripts aligned with updated app behavior and schema.
- [x] Validate fresh environment setup using new scripts.

### Stage 9.2 - Documentation Refresh
- [x] Update all applicable files in:
  - Project startup Docs
  - Docs
  - Scripts
  - User Guide

### Stage 9.3 - Mandatory Completion Artifacts Per Phase
- [x] For each completed phase, record:
  - What was implemented
  - How it was validated
  - Links to updated functions and PRD sections

### Implementation Summary

**Stage 9.1 — Script Modernization:**
- `Scripts/1-MinimalSeed.sql` — §15 expanded: added 16 missing sidebar menu items (`result_calculation`, `notifications`, `students`, `departments`, `courses`, `assignments`, `attendance`, `results`, `quizzes`, `fyp`, `analytics`, `ai_chat`, `student_lifecycle`, `payments`, `enrollments`, `report_center`, `dashboard_settings`) with correct role accesses matching `DatabaseSeeder.cs`.
- `Scripts/1-MinimalSeed.sql` — §17 replaced: updated report definition keys from old hyphen-style (`attendance-report`, `results-report`, `dept-summary`, `semester-results`) to canonical underscore keys matching `ReportKeys.cs` (`attendance_summary`, `result_summary`, `gpa_report`, `enrollment_summary`, `semester_results`, `student_transcript`, `low_attendance_warning`, `fyp_status`). Added 4 missing reports.
- `Scripts/2-FullDummyData.sql` — same §15 and §17 changes applied (script is self-contained).

**Stage 9.2 — Documentation Refresh:**
- `User Guide/Student-Guide.md` — bumped to v1.1; added Section 12: Enrollments (self-enroll, drop, view status).
- `User Guide/Admin-Guide.md` — bumped to v1.1; updated Section 6: Enrollment and SIS Oversight (admin enroll/drop/roster workflows).
- `User Guide/Faculty-Guide.md` — bumped to v1.1; updated Section 4: added Enrollments roster view path.
- `User Guide/SuperAdmin-Guide.md` — bumped to v1.1.
- `User Guide/License-KeyGen-Guide.md` — bumped to v1.1.
- `User Guide/README.md` — added version note.

**Stage 9.3 — Completion Artifacts:**
- PRD.md updated to v1.22 with Phase 9 log entry.
- Command.md execution pointer updated to Phase 9 Complete.
- Function-List.md already updated at end of each prior phase (Phases 7 and 8 functions recorded).

**Files Modified:**
- `Scripts/1-MinimalSeed.sql`
- `Scripts/2-FullDummyData.sql`
- `User Guide/Student-Guide.md`
- `User Guide/Admin-Guide.md`
- `User Guide/Faculty-Guide.md`
- `User Guide/SuperAdmin-Guide.md`
- `User Guide/License-KeyGen-Guide.md`
- `User Guide/README.md`

### Validation Summary
- All SQL scripts remain syntactically valid SQL Server T-SQL — all new inserts use IF NOT EXISTS guards (idempotent).
- Role accesses in scripts now match `DatabaseSeeder.SeedSidebarMenusAsync` exactly.
- Report keys now match `ReportKeys.cs` constants exactly.
- User guides reflect current feature set including Enrollment CRUD workflows.
- Build: ✅ 0 errors, 0 warnings (no C# changes in this phase).

---

## Progress Tracker
- [x] Phase 1 complete
- [x] Phase 2 complete
- [x] Phase 3 complete
- [x] Phase 4 complete
- [x] Phase 5 complete
- [x] Phase 6 complete
- [x] Phase 7 complete
- [x] Phase 8 complete
- [x] Phase 9 complete

## Next Phase To Execute
All phases complete. Project documentation and scripts are fully up to date.


---

## Phase 14 � Helpdesk / Support Ticketing System
**Status:** ? Complete (2026-05-09) | Commit: 8576e44

### Completion Mark
- [x] `SupportTicket` + `SupportTicketMessage` domain entities
- [x] `IHelpdeskRepository` + `HelpdeskRepository` (EF Core; tables `support_tickets`, `support_ticket_messages`; dept-scoped)
- [x] `IHelpdeskService` + `HelpdeskService` � create, list, get, add-message, assign, status transitions, reopen window
- [x] `HelpdeskController` (all CRUD + lifecycle endpoints); `HelpdeskDTOs`
- [x] EF migration `20260507_Phase14_Helpdesk`
- [x] `Helpdesk.cshtml` list, `HelpdeskCreate.cshtml` form, `HelpdeskDetail.cshtml` thread view, `_TicketStatusBadge.cshtml` partial
- [x] Sidebar link (Student, Faculty, Admin, SuperAdmin); route + group maps in `_Layout.cshtml`
- [x] Phase 14 DI registration in `Program.cs`

### Implementation Summary
Full three-stage ticket lifecycle: students/faculty raise tickets (categorised); admins assign and resolve (dept-scoped); faculty reply via thread messages; submitters can reopen within configurable window; status changes trigger in-app notifications.

### Validation Summary
- Build: 0 errors, 0 warnings
- Tests: 78/78 passed
- Commit: 8576e44 pushed to main

---

## Phase 15 � Enrollment Rules Engine
**Status:** ? Complete (2026-05-08) | Commit: 42f0993

### Completion Mark
- [x] `CoursePrerequisite` entity (`CourseId`, `PrerequisiteCourseId`); soft-delete-safe unique composite index
- [x] `IPrerequisiteRepository` + `PrerequisiteRepository` (EF Core; table `course_prerequisites`)
- [x] `EnrollmentService.TryEnrollAsync` � prerequisite pass check + timetable clash detection
- [x] `AdminEnrollRequest` updated: `OverrideClash` (bool) + `OverrideReason` (string?) fields; clash override audit-logged
- [x] `PrerequisiteController` (GET / POST / DELETE `api/v1/prerequisite`); Admin/SuperAdmin write; read open to all authenticated
- [x] EF migration `20260507133254_Phase15_EnrollmentRules` (`course_prerequisites` + unique index)
- [x] `PrerequisitesPageModel`, `PrerequisiteWebItem`, `CoursePrerequisiteGroup` web view models
- [x] `Prerequisites` / `PrerequisiteAdd` / `PrerequisiteRemove` portal controller actions + `Prerequisites.cshtml` view
- [x] `GetPrerequisitesAsync` / `AddPrerequisiteAsync` / `RemovePrerequisiteAsync` in `EduApiClient`
- [x] Sidebar link (Admin/SuperAdmin only); route + group maps in `_Layout.cshtml`
- [x] Phase 15 DI registration in `Program.cs`
- [x] Migration applied: `dotnet ef database update` � Done ?

### Implementation Summary
Stage 15.1 adds prerequisite-based enrollment blocking with detailed unmet-prerequisite reporting. Stage 15.2 adds timetable-clash detection with admin override capability. Stage 15.3 (capacity limits) was already in place. The web portal exposes a Prerequisites management page visible to Admin/SuperAdmin for managing course prerequisite links.

### Validation Summary
- Build: 0 errors, 0 warnings
- Tests: 7/7 passed
- Migration applied to local DB ?
- Commit: 42f0993 pushed to main

---

## Progress Tracker
- [x] Phase 1 complete
- [x] Phase 2 complete
- [x] Phase 3 complete
- [x] Phase 4 complete
- [x] Phase 5 complete
- [x] Phase 6 complete
- [x] Phase 7 complete
- [x] Phase 8 complete
- [x] Phase 9 complete
- [x] Phase 12 complete
- [x] Phase 13 complete
- [x] Phase 14 complete
- [x] Phase 15 complete
- [x] Phase 16 complete
- [x] Phase 17 complete

## Phase 16 — Faculty Grading System ✅ (2026-05-08)

### Stage 16.1 — Gradebook Grid View
- [x] `GradebookGridResponse` DTO with columns (component/weightage) + student rows with per-cell marks
- [x] `GradebookRepository.GetStudentsForOfferingAsync` — 3-way join: Enrollments → StudentProfiles → Users
- [x] `GradebookService.GetGradebookAsync` — builds weighted grid from results
- [x] `GradebookService.UpsertEntryAsync` — creates or corrects a result cell (ExistsAsync → CorrectMarks / new Result)
- [x] `GradebookService.PublishAllAsync` — publishes all unpublished results for an offering
- [x] `GradebookController` — GET grid, PUT entry, POST publish-all
- [x] `Gradebook.cshtml` — inline cell editing (JS fetch auto-save on blur), publish-all button, offering dropdown

### Stage 16.2 — Rubric-Based Grading
- [x] Domain: `Rubric`, `RubricCriterion`, `RubricLevel`, `RubricStudentGrade` entities
- [x] EF configs: `rubrics`, `rubric_criteria`, `rubric_levels`, `rubric_student_grades` tables
- [x] `RubricRepository` — includes Criteria → Levels navigation, upsert rubric student grades
- [x] `RubricService.CreateAsync` — builds full rubric graph; `GradeSubmissionAsync` — upserts per-criterion grades
- [x] `RubricController` — CRUD + grade endpoints
- [x] `RubricManage.cshtml` — dynamic criterion/level builder; rubric detail with delete
- [x] `RubricView.cshtml` — student/faculty rubric grade scorecard

### Stage 16.3 — Bulk Grading via CSV
- [x] `GradebookService.GetCsvTemplateAsync` — UTF8 CSV template with student rows
- [x] `GradebookService.ParseBulkCsvAsync` — validates CSV, returns preview with per-row validation errors
- [x] `GradebookService.ConfirmBulkGradeAsync` — applies valid rows via UpsertEntryAsync
- [x] `GradebookController` — GET template, POST preview, POST confirm
- [x] `Gradebook.cshtml` — CSV upload section with preview table + confirm form

### Phase 16 DI + Migration
- [x] Phase 16 DI in `Program.cs`: IGradebookRepository, IGradebookService, IRubricRepository, IRubricService
- [x] Migration `Phase16_FacultyGrading` created and applied
- [x] 78/78 unit tests passing

## Phase 17 — Degree Audit System ✅ (2026-05-08)

### Stage 17.1 — Credit Completion Tracking
- [x] `DegreeRule` entity + `DegreeRuleRequiredCourse` join entity
- [x] `IDegreeAuditRepository` with `GetEarnedCreditsAsync` (3-way join: Results → CourseOfferings → Courses)
- [x] `DegreeAuditRepository` EF implementation
- [x] `DegreeAuditService.GetAuditAsync` — deduplicates credits by CourseId (highest GradePoint wins), aggregates totals
- [x] `DegreeAuditController` — `GET /api/v1/degree-audit/me` (Student), `GET /{studentProfileId}` (Admin/Faculty/SuperAdmin)
- [x] `DegreeAudit.cshtml` — credit breakdown cards, completed courses table, eligibility badge

### Stage 17.2 — Graduation Eligibility Checker
- [x] `DegreeAuditService.GetEligibilityListAsync` — evaluates all students in a program against DegreeRule
- [x] `DegreeAuditController` — `GET /eligible` (Admin/SuperAdmin)
- [x] `GraduationEligibility.cshtml` — eligibility list table with View Audit links

### Stage 17.3 — Elective vs Core Course Tagging
- [x] `CourseType` enum (`Core=1, Elective=2`) added to `Course` entity
- [x] `Course.SetCourseType(courseType)` method
- [x] `CourseConfiguration` — `course_type` column, default `Core`
- [x] `DegreeAuditController` — `PUT /course/{courseId}/type` (Admin/SuperAdmin)
- [x] `DegreeRules.cshtml` — SuperAdmin rule management with create form

### Phase 17 DI + Migration
- [x] Phase 17 DI in `Program.cs`: `IDegreeAuditRepository`, `IDegreeAuditService`
- [x] Migration `Phase17_DegreeAudit` created and applied
- [x] 78/78 unit tests passing

## Phase 18 — Graduation Workflow ✅ Complete

- [x] Stage 18.1: `GraduationApplication` entity + `GraduationApplicationApproval`; multi-stage approval flow (Faculty → Admin → SuperAdmin); `GraduationController` REST API (10 endpoints); `GraduationService`; `GraduationRepository`.
- [x] Stage 18.2: `ICertificateGenerator` / `CertificateGenerator` (QuestPDF A4 Landscape); certificate stored under `wwwroot/certificates/`; `GET /api/v1/graduation/{id}/certificate` download; `POST .../regenerate-certificate` admin action.
- [x] Web portal: `GraduationApply.cshtml`, `GraduationApplications.cshtml`, `GraduationApplicationDetail.cshtml`.
- [x] EF Migration `Phase18_GraduationWorkflow` — tables `graduation_applications`, `graduation_application_approvals`.
- [x] 78/78 unit tests passing

## Phase 19 — Advanced Course Creation & Result Configuration ✅ Complete

- [x] Stage 19.1: `Course` entity extended — `HasSemesters`, `TotalSemesters`; domain methods `SetSemesterBased`, `SetNonSemesterBased`; `AutoCreateSemestersAsync` creates standalone semester rows for semester-based courses on creation.
- [x] Stage 19.2: `Course` entity extended — `DurationValue`, `DurationUnit`, `GradingType` for non-semester courses.
- [x] Stage 19.3: `ResultCalculation.cshtml` updated with Course Type (Semester-Based / Non-Semester-Based) and Course filter dropdowns (AJAX-powered via `GET /api/v1/course?hasSemesters=`).
- [x] Stage 19.4: `CourseGradingConfig` entity; `ICourseGradingRepository` + `CourseGradingRepository`; `ICourseGradingService` + `CourseGradingService`; `GradingConfigController` (`GET/PUT /api/v1/grading-config/{courseId}`); `GradingConfig.cshtml` SuperAdmin page with grade-range builder.
- [x] `Courses.cshtml` modal form updated with HasSemesters toggle, semester count / duration fields, grading type selector; course table shows Type badge.
- [x] EF Migration `Phase19_CourseTypeAndGrading` — adds columns to `courses` table, new table `course_grading_configs`.
- [x] 78/78 unit tests passing

## Next Phase To Execute
Phase 20 — (see Docs/Enhancements.md for full spec).

## Phase 20 — Learning Management System (LMS) ✅ Complete

- [x] Stage 20.1: `CourseContentModule` + `ContentVideo` domain entities; `ILmsRepository` + `LmsRepository`; `ILmsService` + `LmsService`; `LmsController` (`GET/POST/PUT/DELETE /api/v1/lms/...`); `CourseLms.cshtml` (student view) + `LmsManage.cshtml` (faculty view).
- [x] Stage 20.2: `LmsConfigurations.cs` — EF table/FK/query-filter configs for CourseContentModule and ContentVideo.
- [x] Stage 20.3: `DiscussionThread` + `DiscussionReply` domain entities; `IDiscussionRepository` + `DiscussionRepository`; `IDiscussionService` + `DiscussionService`; `DiscussionController`; `Discussion.cshtml` + `DiscussionThread.cshtml` portal views.
- [x] Stage 20.4: `CourseAnnouncement` domain entity; `IAnnouncementRepository` + `AnnouncementRepository`; `IAnnouncementService` + `AnnouncementService` (with fan-out notification to enrolled students); `AnnouncementController`; `Announcements.cshtml` portal view.
- [x] ApplicationDbContext updated with 5 new DbSets; `_Layout.cshtml` sidebar entries added (`lms_manage`, `discussion`, `announcements`).
- [x] EF Migration `Phase20_LMS` — tables `course_content_modules`, `content_videos`, `discussion_threads`, `discussion_replies`, `course_announcements`.
- [x] 7/7 unit tests passing (build clean; only pre-existing nullability warnings).

## Phase 21 — Study Planner ✅ Complete (2026-05-08)

- [x] Stage 21.1: `StudyPlan` aggregate (AuditableEntity, `StudyPlanStatus` enum, `Endorse/Reject/ResetAdvisorStatus` methods); `StudyPlanCourse` child entity (BaseEntity, physical delete).
- [x] `AcademicProgram.MaxCreditLoadPerSemester` property + `SetMaxCreditLoad()` method added.
- [x] `IStudyPlanRepository` interface + `StudyPlanRepository` EF Core implementation.
- [x] `StudyPlannerDTOs.cs` — 4 request + 4 response records.
- [x] `IStudyPlanService` + `StudyPlanService`: CRUD; prerequisite validation (Phase 15); credit-load validation; `AdvisePlanAsync` (Faculty/Admin workflow).
- [x] Stage 21.2: `GetRecommendationsAsync` — degree audit gap detection + eligible electives; prerequisite-gated; credit-load-capped; per-course `Reason`.
- [x] `StudyPlanConfigurations.cs` — `StudyPlanConfiguration` + `StudyPlanCourseConfiguration`; `AcademicProgramConfiguration` updated for `MaxCreditLoadPerSemester`.
- [x] `ApplicationDbContext` — `StudyPlans` + `StudyPlanCourses` DbSets added.
- [x] `StudyPlanController` (`api/v1/study-plan`) — 9 endpoints.
- [x] `API/Program.cs` Phase 21 DI block — 2 scoped registrations.
- [x] `EduApiClient` — 9 new methods + 4 API response models.
- [x] `PortalController` — 9 new actions + `MapStudyPlanItem` helper.
- [x] `PortalViewModels.cs` — `StudyPlanCourseItem`, `StudyPlanItem`, `StudyPlanPageModel`, `StudyPlanDetailPageModel`, `RecommendationItem`, `RecommendationsPageModel`.
- [x] Portal views: `StudyPlan.cshtml`, `StudyPlanDetail.cshtml`, `StudyPlanRecommendations.cshtml`.
- [x] `_Layout.cshtml` sidebar: `study_plan` → `(Portal, StudyPlan)` (group: Student Related, weight 3).
- [x] EF Migration `Phase21_StudyPlanner` applied — tables `study_plans`, `study_plan_courses`; `MaxCreditLoadPerSemester` column on `academic_programs`.
- [x] 7/7 unit tests passing (build clean).

---

## Phase 22 — External Integrations ✅ Complete (2026-05-08) | Commit: `dddee69`

### Completion Mark
- [x] Stage 22.1 — Library system integration: `LibraryConfig` stored in `portal_settings`; `LibraryController` (config GET/PUT, loans GET by self and by student ID); `LibraryService` proxies external library API; portal `LibraryConfig.cshtml`.
- [x] Stage 22.2 — Accreditation reporting: `AccreditationTemplate` entity + EF migration `Phase22_ExternalIntegrations`; `AccreditationController` (CRUD + generate/stream); `AccreditationService.GenerateAsync` formats as CSV/PDF and writes audit-log entry; portal `AccreditationTemplates.cshtml`; `IAccreditationRepository` + `AccreditationRepository`.

### Implementation Summary
- **Stage 22.1**: `ILibraryService` + `LibraryService` (scoped); `LibraryController` at `api/v1/library`; `Web/PortalController` 2 actions + `LibraryConfig.cshtml`; `EduApiClient` 3 new methods; sidebar entry `library_config` (Settings).
- **Stage 22.2**: `AccreditationTemplate` domain entity with `AccreditationTemplateConfiguration` EF config; `IAccreditationRepository` + `AccreditationRepository`; `IAccreditationService` + `AccreditationService`; `AccreditationController` at `api/v1/accreditation`; `Web/PortalController` 7 new actions + `AccreditationTemplates.cshtml`; `EduApiClient` 8 new methods; sidebar entry `accreditation` (Settings).

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — 0 errors, 0 warnings.
- EF Migration `Phase22_ExternalIntegrations` applied successfully.

---

## Phase 23 — Core Policy Foundation ✅ Complete (2026-05-09) | Commit: `28cac36`

### Completion Mark
- [x] Stage 23.1 — `InstitutionType` enum (`University=0`, `School=1`, `College=2`) in `Domain/Enums/`.
- [x] Stage 23.1 — `InstitutionPolicySnapshot` sealed record + `SaveInstitutionPolicyCommand` + `IInstitutionPolicyService` + `InstitutionPolicyService` (10-min IMemoryCache; `portal_settings` persistence; throws when all flags false).
- [x] Stage 23.1 — `Microsoft.Extensions.Caching.Memory 8.0.1` added to `Application.csproj`.
- [x] Stage 23.2 — `InstitutionContextMiddleware` stores snapshot per-request in `HttpContext.Items`; extension method `GetInstitutionPolicy()` returns `Default` if absent.
- [x] Stage 23.3 — `InstitutionPolicyController` (`GET` all authenticated + `PUT` SuperAdmin); registered in `Program.cs` after `UseAuthorization`.
- [x] Web: `PortalController.InstitutionPolicy` GET/POST; `InstitutionPolicy.cshtml`; `EduApiClient` 2 new methods; sidebar seed `institution_policy` (sort 33, SuperAdmin).
- [x] Tests: 27/27 unit tests passed (13 new Phase-23 tests in `InstitutionPolicyTests.cs`).

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — 0 errors, 5 pre-existing nullable warnings.
- No EF migration required (uses existing `portal_settings` table).
- 27/27 unit tests passed.

---

## Phase 24 — Dynamic Module and UI Composition ✅ Complete (2026-05-09) | Commit: `391ac45`

### Completion Mark
- [x] Stage 24.1 — `ModuleDescriptor` sealed record (`Domain/Modules/`); `ModuleRegistry` static catalogue (14 modules); `IModuleRegistryService` + `ModuleRegistryService`; `ModuleRegistryController` `GET api/v1/module-registry/visible`.
- [x] Stage 24.2 — `AcademicVocabulary` sealed record; `ILabelService` + `LabelService` (singleton); `LabelController` `GET api/v1/labels`.
- [x] Stage 24.3 — `WidgetDescriptor` sealed record; `IDashboardCompositionService` + `DashboardCompositionService` (singleton); `DashboardCompositionController` `GET api/v1/dashboard/composition`.
- [x] Web: `PortalController.ModuleComposition` (parallel `Task.WhenAll`); `ModuleComposition.cshtml`; `EduApiClient` 3 methods + 3 API models; sidebar seed `module_composition` (sort 34, SuperAdmin).
- [x] Tests: 44/44 unit tests passed (17 new Phase-24 tests in `Phase24Tests.cs`).

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — 0 errors, 5 pre-existing nullable warnings.
- No EF migration required.
- 44/44 unit tests passed.

---

## Phase 25 — Academic Engine Unification ✅ Complete (2026-05-09) | Commit: `d2aabd3`

### Completion Mark
- [x] Stage 25.1 — `IResultCalculationStrategy` interface + `ComponentMark`, `ResultSummary`, `GpaScaleRuleEntry`, `GradeBandEntry` value types (`Application/Interfaces/IResultCalculationStrategy.cs`).
- [x] Stage 25.1 — `GpaResultStrategy` (University GPA 0.0–4.0); `PercentageResultStrategy` (School/College % + grade bands; custom JSON or built-in defaults).
- [x] Stage 25.1 — `IResultStrategyResolver` + `ResultStrategyResolver` (singleton). Zero changes to existing `ResultService`.
- [x] Stage 25.2 — `InstitutionGradingProfile` entity; `IInstitutionGradingProfileRepository` + `InstitutionGradingProfileRepository`.
- [x] Stage 25.2 — `IInstitutionGradingService` + `InstitutionGradingService` (upsert semantics); DTOs `InstitutionGradingProfileDto` / `SaveInstitutionGradingProfileRequest`.
- [x] Stage 25.2 — `InstitutionGradingProfileConfiguration` EF config (`institution_grading_profiles`, `decimal(5,2)`, unique index on `InstitutionType`).
- [x] Stage 25.2 — Migration `20260508152906_Phase25_AcademicEngineUnification` applied.
- [x] Stage 25.2 — `InstitutionGradingProfileController` (`GET /`, `GET /{type}` Admin+; `PUT /{type}` SuperAdmin).
- [x] Stage 25.3 — `IProgressionService` + `ProgressionService` (University CGPA; School/College %; default thresholds 2.0/40).
- [x] Stage 25.3 — `ProgressionDecision` + `ProgressionEvaluationRequest` DTOs.
- [x] Stage 25.3 — `ProgressionController` (`POST /evaluate` Admin+, `POST /promote` Admin+, `GET /me/{type}` Student+).
- [x] DI: `IResultStrategyResolver` (singleton), `IInstitutionGradingProfileRepository` (scoped), `IInstitutionGradingService` (scoped), `IProgressionService` (scoped).
- [x] Tests: 144/144 unit tests passed (29 new Phase-25 tests in `Phase25Tests.cs` covering strategy, resolver, entity, and progression service).

### New Files (Phase 25)
| File | Description |
|---|---|
| `Application/Interfaces/IResultCalculationStrategy.cs` | Strategy interface + value types |
| `Application/Academic/GpaResultStrategy.cs` | University GPA strategy |
| `Application/Academic/PercentageResultStrategy.cs` | School/College percentage strategy |
| `Application/Interfaces/IResultStrategyResolver.cs` | Resolver interface |
| `Application/Academic/ResultStrategyResolver.cs` | Resolver implementation |
| `Domain/Academic/InstitutionGradingProfile.cs` | Grading profile entity |
| `Domain/Interfaces/IInstitutionGradingProfileRepository.cs` | Repository interface |
| `Application/Interfaces/IInstitutionGradingService.cs` | Service interface |
| `Application/Academic/InstitutionGradingService.cs` | Service implementation |
| `Application/DTOs/Academic/InstitutionGradingDtos.cs` | Grading profile DTOs |
| `Application/Interfaces/IProgressionService.cs` | Progression service interface |
| `Application/Academic/ProgressionService.cs` | Progression/promotion implementation |
| `Application/DTOs/Academic/ProgressionDtos.cs` | Progression DTOs |
| `Infrastructure/Repositories/InstitutionGradingProfileRepository.cs` | EF repository |
| `Infrastructure/Persistence/Configurations/InstitutionGradingProfileConfiguration.cs` | EF config |
| `Infrastructure/Migrations/20260508152906_Phase25_AcademicEngineUnification.cs` | EF migration |
| `API/Controllers/InstitutionGradingProfileController.cs` | Grading profile API |
| `API/Controllers/ProgressionController.cs` | Progression API |
| `tests/.../Phase25Tests.cs` | 29 unit tests |

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — 0 errors, 5 pre-existing nullable warnings.
- EF Migration `20260508152906_Phase25_AcademicEngineUnification` applied.
- 144/144 unit tests passed (29 new).

---

## Phase 26 — School and College Functional Expansion ✅ Complete (2026-05-09) | Commit: `4c0904c`

### Completion Mark
- [x] Stage 26.1 — `SchoolStream` + `StudentStreamAssignment` domain entities created.
- [x] Stage 26.1 — `ISchoolStreamRepository` + `SchoolStreamRepository`; `ISchoolStreamService` + `SchoolStreamService`.
- [x] Stage 26.1 — `SchoolStreamController` endpoints for list/upsert/assign/get-student-assignment.
- [x] Stage 26.2 — `StudentReportCard`, `BulkPromotionBatch`, `BulkPromotionEntry` domain entities.
- [x] Stage 26.2 — enums `BulkPromotionStatus` and `EntryDecision` added.
- [x] Stage 26.2 — `IReportCardRepository`/`ReportCardRepository` + `IReportCardService`/`ReportCardService` + `ReportCardController`.
- [x] Stage 26.2 — `IBulkPromotionRepository`/`BulkPromotionRepository` + `IBulkPromotionService`/`BulkPromotionService` + `BulkPromotionController`.
- [x] Stage 26.2 — approval safeguard workflow implemented (Draft → AwaitingApproval → Approved/Rejected → Applied).
- [x] Stage 26.3 — `ParentStudentLink` entity + `IParentStudentLinkRepository`/`ParentStudentLinkRepository`.
- [x] Stage 26.3 — `IParentPortalService`/`ParentPortalService` + `ParentPortalController` parent-linked student read endpoint.
- [x] Migration `20260509044437_Phase26_SchoolCollegeExpansion` created.
- [x] Tests: `Phase26Tests.cs` added; total suite now 152/152 passing.

### New Files (Phase 26)
| File | Description |
|---|---|
| `Domain/Academic/SchoolStream.cs` | School stream master entity |
| `Domain/Academic/StudentStreamAssignment.cs` | Student-to-stream assignment entity |
| `Domain/Academic/StudentReportCard.cs` | Report-card snapshot entity |
| `Domain/Academic/BulkPromotionBatch.cs` | Bulk promotion workflow header |
| `Domain/Academic/BulkPromotionEntry.cs` | Per-student bulk promotion row |
| `Domain/Academic/ParentStudentLink.cs` | Parent-to-student mapping entity |
| `Domain/Enums/BulkPromotionStatus.cs` | Batch workflow status enum |
| `Domain/Enums/EntryDecision.cs` | Promote/Hold decision enum |
| `Domain/Interfaces/ISchoolStreamRepository.cs` | Stream repository contract |
| `Domain/Interfaces/IReportCardRepository.cs` | Report card repository contract |
| `Domain/Interfaces/IBulkPromotionRepository.cs` | Bulk promotion repository contract |
| `Domain/Interfaces/IParentStudentLinkRepository.cs` | Parent-link repository contract |
| `Application/DTOs/Academic/Phase26Dtos.cs` | Phase 26 DTO contracts |
| `Application/Interfaces/ISchoolStreamService.cs` | Stream service contract |
| `Application/Interfaces/IReportCardService.cs` | Report card service contract |
| `Application/Interfaces/IBulkPromotionService.cs` | Bulk promotion service contract |
| `Application/Interfaces/IParentPortalService.cs` | Parent portal read-model service contract |
| `Application/Academic/SchoolStreamService.cs` | Stream orchestration service |
| `Application/Academic/ReportCardService.cs` | Report card snapshot service |
| `Application/Academic/BulkPromotionService.cs` | Approval-based bulk promotion service |
| `Application/Academic/ParentPortalService.cs` | Parent-linked student read service |
| `API/Controllers/SchoolStreamController.cs` | Stream API endpoints |
| `API/Controllers/ReportCardController.cs` | Report card API endpoints |
| `API/Controllers/BulkPromotionController.cs` | Bulk promotion API endpoints |
| `API/Controllers/ParentPortalController.cs` | Parent portal API endpoint |
| `Infrastructure/Persistence/Configurations/SchoolStreamConfiguration.cs` | EF config |
| `Infrastructure/Persistence/Configurations/StudentStreamAssignmentConfiguration.cs` | EF config |
| `Infrastructure/Persistence/Configurations/StudentReportCardConfiguration.cs` | EF config |
| `Infrastructure/Persistence/Configurations/BulkPromotionBatchConfiguration.cs` | EF config |
| `Infrastructure/Persistence/Configurations/BulkPromotionEntryConfiguration.cs` | EF config |
| `Infrastructure/Persistence/Configurations/ParentStudentLinkConfiguration.cs` | EF config |
| `Infrastructure/Repositories/Phase26Repositories.cs` | Phase 26 repository implementations |
| `Infrastructure/Migrations/20260509044437_Phase26_SchoolCollegeExpansion.cs` | EF migration |
| `tests/Tabsan.EduSphere.UnitTests/Phase26Tests.cs` | 8 Phase 26 unit tests |

### Validation Summary
- `dotnet build Tabsan.EduSphere.sln` — 0 errors.
- `runTests` — 152/152 tests passed.
- Migration listed: `20260509044437_Phase26_SchoolCollegeExpansion`.

---

## Phase 27 — University Portal Parity and Student Experience ✅ Complete (2026-05-09)

### Completion Mark
- [x] Stage 27.1 — `IPortalCapabilityMatrixService` + `PortalCapabilityMatrixService` implemented.
- [x] Stage 27.1 — `PortalCapabilitiesController` added with `GET /api/v1/portal-capabilities/matrix`.
- [x] Stage 27.1 — web wiring complete (`PortalController.PortalCapabilityMatrix`, new view models, `PortalCapabilityMatrix.cshtml`).
- [x] Stage 27.2 — `AuthSecurityOptions` added and bound in API (`AuthSecurity` section).
- [x] Stage 27.2 — `AuthController` extended with `GET /api/v1/auth/security-profile` and richer login failure handling.
- [x] Stage 27.2 — `AuthService` extended for MFA toggle enforcement, session-risk controls, and auth audit improvements.
- [x] Stage 27.2 — login UX updated for MFA/SSO/risk messaging and request payload support.
- [x] Stage 27.3 — provider contracts added for ticketing/announcement/email (`ICommunicationIntegrationContracts`).
- [x] Stage 27.3 — default adapters added (`InAppSupportTicketingProvider`, `InAppAnnouncementBroadcastProvider`, `SmtpEmailDeliveryProvider`).
- [x] Stage 27.3 — core service consumers refactored to provider contracts (`HelpdeskService`, `AnnouncementService`, `LicenseExpiryWarningJob`).
- [x] Stage 27.3 — integration profile API added (`GET /api/v1/communication-integrations/profile`).
- [x] Unit tests added and passing for stages 27.1/27.2/27.3 (`Phase27Tests`, `Phase27Stage2Tests`, `Phase27Stage3Tests`).

### Validation Summary
- `dotnet test tests/Tabsan.EduSphere.UnitTests/Tabsan.EduSphere.UnitTests.csproj` — 89/89 passed.
- `dotnet build Tabsan.EduSphere.sln` — success.
- No EF migration required for Phase 27.
- Commits pushed: `fd3b137`, `20dba8d`, `56cf1dd`.
