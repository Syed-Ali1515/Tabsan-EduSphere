# Function List — Tabsan EduSphere

> **Maintenance rule**: Every function added to the codebase must be registered here with Name, Purpose, and Location.
> Format: `Name | Purpose | Location`

---

## Domain Layer

### `BaseEntity` — `src/Tabsan.EduSphere.Domain/Common/BaseEntity.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Touch()` | Updates the `UpdatedAt` timestamp to the current UTC time. Called by all domain mutation methods and by DbContext before SaveChanges. | `Domain/Common/BaseEntity.cs` |

---

### `AuditableEntity` — `src/Tabsan.EduSphere.Domain/Common/AuditableEntity.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SoftDelete()` | Marks the entity as deleted (`IsDeleted = true`, sets `DeletedAt`) without physically removing the database row. | `Domain/Common/AuditableEntity.cs` |
| `Restore()` | Reverses a soft delete — clears `IsDeleted` and `DeletedAt`. | `Domain/Common/AuditableEntity.cs` |

---

### `User` — `src/Tabsan.EduSphere.Domain/Identity/User.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RecordLogin()` | Sets `LastLoginAt` to the current UTC time after a successful authentication. | `Domain/Identity/User.cs` |
| `UpdatePasswordHash(newHash)` | Replaces the stored password hash when the user changes their password. | `Domain/Identity/User.cs` |
| `Deactivate()` | Sets `IsActive = false` to prevent the user from logging in without deleting the account. | `Domain/Identity/User.cs` |
| `Activate()` | Re-enables a previously deactivated user account. | `Domain/Identity/User.cs` |
| `UpdateEmail(email)` | Updates the user's email address with basic format validation. | `Domain/Identity/User.cs` |

---

### `UserSession` — `src/Tabsan.EduSphere.Domain/Identity/UserSession.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `IsActive` (computed property) | Returns true when the session has not been revoked and the expiry is in the future. | `Domain/Identity/UserSession.cs` |
| `Revoke()` | Stamps `RevokedAt` with the current UTC time, invalidating the session for all future refresh attempts. | `Domain/Identity/UserSession.cs` |
| `Rotate(newHash, newExpiry)` | Replaces the refresh token hash and extends the expiry — used during token rotation on refresh. | `Domain/Identity/UserSession.cs` |

---

### `Department` — `src/Tabsan.EduSphere.Domain/Academic/Department.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Rename(newName)` | Updates the display name of the department. | `Domain/Academic/Department.cs` |
| `Deactivate()` | Marks the department inactive so it is hidden from assignment dropdowns. | `Domain/Academic/Department.cs` |
| `Activate()` | Re-activates a previously deactivated department. | `Domain/Academic/Department.cs` |

---

### `LicenseState` — `src/Tabsan.EduSphere.Domain/Licensing/LicenseState.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RefreshStatus()` | Re-evaluates license validity against current UTC time and updates `Status` to Active or Expired. | `Domain/Licensing/LicenseState.cs` |
| `MarkInvalid()` | Forces the status to `Invalid` when the signature check fails. | `Domain/Licensing/LicenseState.cs` |
| `Replace(newHash, newType, newExpiry)` | Replaces the current license record with data from a newly uploaded and validated license file. | `Domain/Licensing/LicenseState.cs` |

---

### `ModuleStatus` — `src/Tabsan.EduSphere.Domain/Modules/ModuleStatus.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Activate(changedBy, source)` | Activates the module and records who changed it and when. | `Domain/Modules/ModuleStatus.cs` |
| `Deactivate(changedBy)` | Deactivates the module, preserving data but blocking UI and API access. | `Domain/Modules/ModuleStatus.cs` |

---

### `AuditLog` — `src/Tabsan.EduSphere.Domain/Auditing/AuditLog.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `AuditLog(action, entityName, ...)` | Constructor — creates an immutable audit record for a privileged action. All audit writes use this constructor to ensure no field is omitted. | `Domain/Auditing/AuditLog.cs` |

---

## Infrastructure Layer

### `ApplicationDbContext` — `src/Tabsan.EduSphere.Infrastructure/Persistence/ApplicationDbContext.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `OnModelCreating(modelBuilder)` | Discovers and applies all `IEntityTypeConfiguration` implementations in the assembly automatically. | `Infrastructure/Persistence/ApplicationDbContext.cs` |
| `SaveChangesAsync(cancellationToken)` | Overrides EF Core save to call `Touch()` on all modified entities before writing to the database. | `Infrastructure/Persistence/ApplicationDbContext.cs` |
| `SetAuditTimestamps()` | Iterates all tracked `BaseEntity` entries in Modified state and calls `Touch()`. Called by `SaveChangesAsync`. | `Infrastructure/Persistence/ApplicationDbContext.cs` |

---

### `UserRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/UserRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetByIdAsync(id, ct)` | Returns a user by GUID PK with Role loaded, or null. | `Infrastructure/Repositories/UserRepository.cs` |
| `GetByUsernameAsync(username, ct)` | Returns a user by username (case-insensitive) with Role loaded, or null. | `Infrastructure/Repositories/UserRepository.cs` |
| `GetByEmailAsync(email, ct)` | Returns a user by email address, or null. | `Infrastructure/Repositories/UserRepository.cs` |
| `UsernameExistsAsync(username, ct)` | Returns true when the username is already taken. | `Infrastructure/Repositories/UserRepository.cs` |
| `AddAsync(user, ct)` | Queues a new user entity for insertion. | `Infrastructure/Repositories/UserRepository.cs` |
| `Update(user)` | Marks the user entity as Modified for EF change tracking. | `Infrastructure/Repositories/UserRepository.cs` |
| `SaveChangesAsync(ct)` | Commits all pending changes to the database. | `Infrastructure/Repositories/UserRepository.cs` |

---

### `LicenseRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/LicenseRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetCurrentAsync(ct)` | Returns the most recently activated license row, or null. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `AddAsync(state, ct)` | Queues a new LicenseState record for insertion. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `Update(state)` | Marks the existing LicenseState as modified. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/LicenseRepository.cs` |

---

### `ModuleRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/ModuleRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllWithStatusAsync(ct)` | Returns all module definitions. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `GetStatusByKeyAsync(moduleKey, ct)` | Returns the ModuleStatus row for the given module key, or null. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `IsActiveAsync(moduleKey, ct)` | Returns true when the named module is active (lightweight query, no nav props). | `Infrastructure/Repositories/ModuleRepository.cs` |
| `UpdateStatus(status)` | Marks a ModuleStatus entity as Modified. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/ModuleRepository.cs` |

---

### `UserSessionRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/UserSessionRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetActiveByHashAsync(tokenHash, ct)` | Returns the non-revoked session matching the token hash, or null. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `AddAsync(session, ct)` | Queues a new UserSession for insertion. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `Update(session)` | Marks the session as Modified. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/UserSessionRepository.cs` |

---

### `TokenService` — `src/Tabsan.EduSphere.Infrastructure/Auth/TokenService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GenerateAccessToken(user)` | Builds and signs a JWT access token with user ID, username, role, and department ID claims. | `Infrastructure/Auth/TokenService.cs` |
| `GenerateRefreshToken()` | Generates a cryptographically random 64-byte Base64 refresh token. | `Infrastructure/Auth/TokenService.cs` |
| `HashRefreshToken(rawToken)` | Computes the SHA-256 hex hash of a raw refresh token for safe storage. | `Infrastructure/Auth/TokenService.cs` |
| `GetRefreshTokenExpiry()` | Returns the UTC expiry DateTime for a new refresh token based on configured days. | `Infrastructure/Auth/TokenService.cs` |

---

### `PasswordHasher` — `src/Tabsan.EduSphere.Infrastructure/Auth/PasswordHasher.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Hash(password)` | Produces a PBKDF2-HMACSHA512 hash of the plain-text password for storage. | `Infrastructure/Auth/PasswordHasher.cs` |
| `Verify(storedHash, providedPassword)` | Returns true when the plain-text password matches the stored hash. | `Infrastructure/Auth/PasswordHasher.cs` |

---

### `LicenseValidationService` — `src/Tabsan.EduSphere.Infrastructure/Licensing/LicenseValidationService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `ActivateFromFileAsync(filePath, ct)` | Reads, deserialises, and signature-verifies a license file; creates or replaces the LicenseState record. Returns true on success. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `ValidateCurrentAsync(ct)` | Refreshes the stored license status against the current time. Used at startup, on Super Admin login, and by the daily background job. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `VerifySignature(payload)` | Reconstructs the canonical signed string and verifies the RSA-SHA256 signature using the embedded public key. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `ComputeFileHash(bytes)` | Computes the SHA-256 hex hash of the raw license file bytes for change detection. | `Infrastructure/Licensing/LicenseValidationService.cs` |

---

### `ModuleEntitlementResolver` — `src/Tabsan.EduSphere.Infrastructure/Modules/ModuleEntitlementResolver.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `IsActiveAsync(moduleKey, ct)` | Returns true when the named module is active; uses a 60-second memory cache to avoid DB hits on every request. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |
| `InvalidateCache(moduleKey)` | Removes the cache entry for a single module after a Super Admin toggle. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |
| `InvalidateAll()` | Clears all module entitlement cache entries after bulk changes or a license update. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |

---

### `AuditService` — `src/Tabsan.EduSphere.Infrastructure/Auditing/AuditService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `LogAsync(entry, ct)` | Appends a new audit log entry to the database asynchronously. | `Infrastructure/Auditing/AuditService.cs` |

---

### `DatabaseSeeder` — `src/Tabsan.EduSphere.Infrastructure/Persistence/DatabaseSeeder.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SeedAsync(services)` | Entry point called from Program.cs; applies EF migrations then calls all seed methods in order. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedRolesAsync(db)` | Inserts the four system roles (SuperAdmin, Admin, Faculty, Student) if they do not already exist. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedModulesAsync(db)` | Inserts all known module definitions and creates a default ModuleStatus row for each. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedSuperAdminAsync(db, hasher)` | Creates the bootstrap Super Admin account from environment variables if no SuperAdmin user exists yet. | `Infrastructure/Persistence/DatabaseSeeder.cs` |

---

## Application Layer

### `AuthService` — `src/Tabsan.EduSphere.Application/Auth/AuthService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `LoginAsync(request, ipAddress, ct)` | Validates credentials, creates a UserSession, and returns JWT + refresh token. | `Application/Auth/AuthService.cs` |
| `RefreshAsync(rawRefreshToken, ipAddress, ct)` | Rotates the refresh token and returns a new token pair. | `Application/Auth/AuthService.cs` |
| `LogoutAsync(rawRefreshToken, ct)` | Revokes the session associated with the refresh token. | `Application/Auth/AuthService.cs` |
| `ChangePasswordAsync(userId, request, ct)` | Verifies current password and replaces it with the new hash. | `Application/Auth/AuthService.cs` |

---

### `ModuleService` — `src/Tabsan.EduSphere.Application/Modules/ModuleService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllAsync(ct)` | Returns all module definitions with their current activation status. | `Application/Modules/ModuleService.cs` |
| `ActivateAsync(moduleKey, changedByUserId, ct)` | Activates the named module, clears the entitlement cache, and writes an audit log. | `Application/Modules/ModuleService.cs` |
| `DeactivateAsync(moduleKey, changedByUserId, ct)` | Deactivates the named module (throws if mandatory), clears cache, writes audit log. | `Application/Modules/ModuleService.cs` |

---

## API Layer

### `AuthController` — `src/Tabsan.EduSphere.API/Controllers/AuthController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Login(request, ct)` | `POST /api/v1/auth/login` — authenticates the user and returns tokens. | `API/Controllers/AuthController.cs` |
| `Refresh(request, ct)` | `POST /api/v1/auth/refresh` — rotates the refresh token and returns a new pair. | `API/Controllers/AuthController.cs` |
| `Logout(request, ct)` | `POST /api/v1/auth/logout` — revokes the session. | `API/Controllers/AuthController.cs` |
| `ChangePassword(request, ct)` | `PUT /api/v1/auth/change-password` — changes the authenticated user's password. | `API/Controllers/AuthController.cs` |

---

### `ModuleController` — `src/Tabsan.EduSphere.API/Controllers/ModuleController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/modules` — returns all modules with status. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Activate(key, ct)` | `POST /api/v1/modules/{key}/activate` — activates the named module. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Deactivate(key, ct)` | `POST /api/v1/modules/{key}/deactivate` — deactivates the named module. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Status(key, ct)` | `GET /api/v1/modules/{key}/status` — returns the current active/inactive state from cache. | `API/Controllers/ModuleController.cs` |
| `GetUserId()` | Private helper — extracts the authenticated user's GUID from the JWT sub claim. | `API/Controllers/ModuleController.cs` |

---

### `LicenseController` — `src/Tabsan.EduSphere.API/Controllers/LicenseController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Upload(file, ct)` | `POST /api/v1/license/upload` — saves and validates a new license file. Requires SuperAdmin. | `API/Controllers/LicenseController.cs` |
| `Status(ct)` | `GET /api/v1/license/status` — runs an on-demand license check and returns current status. Requires SuperAdmin. | `API/Controllers/LicenseController.cs` |

---

## Background Jobs

### `LicenseCheckWorker` — `src/Tabsan.EduSphere.BackgroundJobs/LicenseCheckWorker.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `ExecuteAsync(stoppingToken)` | Main hosted-service loop — waits 30 s on startup, then calls `RunCheckAsync` every 24 hours. | `BackgroundJobs/LicenseCheckWorker.cs` |
| `RunCheckAsync(ct)` | Opens a fresh DI scope, resolves `LicenseValidationService`, and runs a validation check. Exceptions are caught and logged. | `BackgroundJobs/LicenseCheckWorker.cs` |
