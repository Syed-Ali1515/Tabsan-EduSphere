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

---

## Phase 2 — Academic Core

### `AcademicProgram` — `src/Tabsan.EduSphere.Domain/Academic/AcademicProgram.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `AcademicProgram(name, code, departmentId, totalSemesters)` | Constructor — creates a new degree programme; normalises code to uppercase. | `Domain/Academic/AcademicProgram.cs` |
| `Rename(newName)` | Updates the display name of the programme. | `Domain/Academic/AcademicProgram.cs` |
| `Deactivate()` | Marks the programme inactive so it no longer appears in registration dropdowns. | `Domain/Academic/AcademicProgram.cs` |
| `Activate()` | Re-activates a previously deactivated programme. | `Domain/Academic/AcademicProgram.cs` |

---

### `Semester` — `src/Tabsan.EduSphere.Domain/Academic/Semester.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Semester(name, startDate, endDate)` | Constructor — creates a new open semester term. | `Domain/Academic/Semester.cs` |
| `Close()` | Permanently closes the semester. One-way: throws if already closed. | `Domain/Academic/Semester.cs` |

---

### `Course` — `src/Tabsan.EduSphere.Domain/Academic/Course.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Course(title, code, creditHours, departmentId)` | Constructor — creates a new course catalogue entry; normalises code to uppercase. | `Domain/Academic/Course.cs` |
| `UpdateTitle(newTitle)` | Updates the course display title. | `Domain/Academic/Course.cs` |
| `Deactivate()` | Soft-deactivates the course so it cannot be offered. | `Domain/Academic/Course.cs` |
| `Activate()` | Re-activates a deactivated course. | `Domain/Academic/Course.cs` |

---

### `CourseOffering` — `src/Tabsan.EduSphere.Domain/Academic/CourseOffering.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `CourseOffering(courseId, semesterId, maxEnrollment, facultyUserId?)` | Constructor — schedules a course for a semester. | `Domain/Academic/CourseOffering.cs` |
| `AssignFaculty(facultyUserId)` | Assigns or re-assigns a faculty member to this offering. | `Domain/Academic/CourseOffering.cs` |
| `Close()` | Closes the offering so no new enrollments are accepted. | `Domain/Academic/CourseOffering.cs` |
| `Reopen()` | Re-opens the offering to accept enrollments again. | `Domain/Academic/CourseOffering.cs` |
| `UpdateMaxEnrollment(max)` | Changes the maximum enrollment capacity. | `Domain/Academic/CourseOffering.cs` |

---

### `StudentProfile` — `src/Tabsan.EduSphere.Domain/Academic/StudentProfile.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `StudentProfile(userId, registrationNumber, programId, departmentId, admissionDate)` | Constructor — creates a student's academic profile. | `Domain/Academic/StudentProfile.cs` |
| `UpdateCgpa(newCgpa)` | Updates the cumulative GPA after result publication (0.0–4.0 range enforced). | `Domain/Academic/StudentProfile.cs` |
| `AdvanceSemester()` | Increments the student's current semester number. | `Domain/Academic/StudentProfile.cs` |

---

### `Enrollment` — `src/Tabsan.EduSphere.Domain/Academic/Enrollment.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Enrollment(studentProfileId, courseOfferingId)` | Constructor — records a new active enrollment. | `Domain/Academic/Enrollment.cs` |
| `Drop()` | Changes status to Dropped and sets DroppedAt. Throws if not Active. | `Domain/Academic/Enrollment.cs` |
| `Cancel()` | Changes status to Cancelled (used when the offering itself is cancelled). | `Domain/Academic/Enrollment.cs` |

---

### `RegistrationWhitelist` — `src/Tabsan.EduSphere.Domain/Academic/RegistrationWhitelist.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RegistrationWhitelist(identifierType, identifierValue, departmentId, programId)` | Constructor — creates a pre-approved registration entry; normalises identifier to lowercase. | `Domain/Academic/RegistrationWhitelist.cs` |
| `MarkUsed(createdUserId)` | Marks the entry as consumed after a successful self-registration. Throws if already used. | `Domain/Academic/RegistrationWhitelist.cs` |

---

### `FacultyDepartmentAssignment` — `src/Tabsan.EduSphere.Domain/Academic/FacultyDepartmentAssignment.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `FacultyDepartmentAssignment(facultyUserId, departmentId)` | Constructor — creates an active assignment linking a faculty member to a department. | `Domain/Academic/FacultyDepartmentAssignment.cs` |
| `Remove()` | Marks the assignment as removed by setting RemovedAt. | `Domain/Academic/FacultyDepartmentAssignment.cs` |

---

### `DepartmentRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/DepartmentRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllAsync(ct)` | Returns all non-deleted departments ordered by name. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `GetByIdAsync(id, ct)` | Returns the department with the given ID, or null. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `CodeExistsAsync(code, ct)` | Returns true when the uppercase code is already in use. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `AddAsync(department, ct)` | Queues a new department for insertion. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `Update(department)` | Marks the department as modified. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/DepartmentRepository.cs` |

---

### `AcademicProgramRepository` + `SemesterRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicRepositories.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `AcademicProgramRepository.GetAllAsync(departmentId?, ct)` | Returns all programmes, optionally scoped to a department, with Department loaded. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `AcademicProgramRepository.GetByIdAsync(id, ct)` | Returns the programme by ID with Department loaded, or null. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `AcademicProgramRepository.CodeExistsAsync(code, ct)` | Returns true when the uppercase code is already taken. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `AcademicProgramRepository.AddAsync(program, ct)` | Queues the programme for insertion. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `AcademicProgramRepository.Update(program)` | Marks the programme as modified. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `AcademicProgramRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.GetAllAsync(ct)` | Returns all semesters ordered by start date descending. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.GetByIdAsync(id, ct)` | Returns the semester by ID, or null. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.GetCurrentOpenAsync(ct)` | Returns the most recent open semester, or null. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.AddAsync(semester, ct)` | Queues the semester for insertion. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.Update(semester)` | Marks the semester as modified. | `Infrastructure/Repositories/AcademicRepositories.cs` |
| `SemesterRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicRepositories.cs` |

---

### `CourseRepository` — `src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllAsync(departmentId?, ct)` | Returns all courses filtered by department if provided, ordered by code. | `Infrastructure/Repositories/CourseRepository.cs` |
| `GetByIdAsync(id, ct)` | Returns the course by ID, or null. | `Infrastructure/Repositories/CourseRepository.cs` |
| `CodeExistsAsync(code, departmentId, ct)` | Returns true when the code+department combination already exists. | `Infrastructure/Repositories/CourseRepository.cs` |
| `AddAsync(course, ct)` | Queues the course for insertion. | `Infrastructure/Repositories/CourseRepository.cs` |
| `Update(course)` | Marks the course as modified. | `Infrastructure/Repositories/CourseRepository.cs` |
| `GetOfferingsBySemesterAsync(semesterId, ct)` | Returns all offerings for a semester with Course and Semester loaded. | `Infrastructure/Repositories/CourseRepository.cs` |
| `GetOfferingsByFacultyAsync(facultyUserId, ct)` | Returns all offerings assigned to the faculty user. | `Infrastructure/Repositories/CourseRepository.cs` |
| `GetOfferingByIdAsync(offeringId, ct)` | Returns an offering by ID with navigations loaded, or null. | `Infrastructure/Repositories/CourseRepository.cs` |
| `GetEnrollmentCountAsync(offeringId, ct)` | Returns the count of active enrollments for the offering. | `Infrastructure/Repositories/CourseRepository.cs` |
| `AddOfferingAsync(offering, ct)` | Queues the offering for insertion. | `Infrastructure/Repositories/CourseRepository.cs` |
| `UpdateOffering(offering)` | Marks the offering as modified. | `Infrastructure/Repositories/CourseRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/CourseRepository.cs` |

---

### Support Repositories — `src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `EnrollmentRepository.GetByStudentAsync(studentProfileId, ct)` | Returns all enrollment records for the student with course/semester details loaded. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.GetByOfferingAsync(courseOfferingId, ct)` | Returns active enrollments for an offering with student profile loaded. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.GetAsync(studentProfileId, courseOfferingId, ct)` | Returns the enrollment for the given pair, or null. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.IsEnrolledAsync(studentProfileId, courseOfferingId, ct)` | Returns true when an active enrollment already exists. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.AddAsync(enrollment, ct)` | Queues a new enrollment for insertion. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.Update(enrollment)` | Marks the enrollment as modified (status change). | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `EnrollmentRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.GetByUserIdAsync(userId, ct)` | Returns the profile linked to the User ID with Program/Department loaded. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.GetByIdAsync(id, ct)` | Returns the profile by ID, or null. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.GetByRegistrationNumberAsync(regNo, ct)` | Returns the profile matching the registration number. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.GetAllAsync(departmentId?, ct)` | Returns all student profiles, optionally scoped to a department. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.RegistrationNumberExistsAsync(regNo, ct)` | Returns true when the registration number is already in use. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.AddAsync(profile, ct)` | Queues the profile for insertion. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.Update(profile)` | Marks the profile as modified. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `StudentProfileRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `RegistrationWhitelistRepository.FindUnusedAsync(identifierValue, ct)` | Returns an unused whitelist entry by identifier value (case-insensitive), or null. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `RegistrationWhitelistRepository.AddAsync(entry, ct)` | Queues a new whitelist entry for insertion. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `RegistrationWhitelistRepository.AddRangeAsync(entries, ct)` | Bulk-queues multiple whitelist entries for insertion. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `RegistrationWhitelistRepository.Update(entry)` | Marks the entry as modified. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `RegistrationWhitelistRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.GetByFacultyAsync(facultyUserId, ct)` | Returns active assignments for the faculty with Department loaded. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.GetByDepartmentAsync(departmentId, ct)` | Returns active faculty assignments for the department. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.GetAsync(facultyUserId, departmentId, ct)` | Returns the active assignment for the pair, or null. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.GetDepartmentIdsForFacultyAsync(facultyUserId, ct)` | Returns the list of department IDs the faculty is actively assigned to. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.AddAsync(assignment, ct)` | Queues the assignment for insertion. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.Update(assignment)` | Marks the assignment as modified. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |
| `FacultyAssignmentRepository.SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/AcademicSupportRepositories.cs` |

---

### `EnrollmentService` — `src/Tabsan.EduSphere.Application/Academic/EnrollmentService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `EnrollAsync(studentProfileId, request, ct)` | Validates offering state, checks duplicates and seat availability, creates an Enrollment row, and returns a response DTO. Returns null on rejection. | `Application/Academic/EnrollmentService.cs` |
| `DropAsync(studentProfileId, courseOfferingId, ct)` | Changes an active enrollment's status to Dropped. Returns false when no active enrollment exists. | `Application/Academic/EnrollmentService.cs` |
| `GetForStudentAsync(studentProfileId, ct)` | Returns all enrollment records for the student (full history). | `Application/Academic/EnrollmentService.cs` |
| `GetForOfferingAsync(courseOfferingId, ct)` | Returns active enrollments for the given offering (faculty roster). | `Application/Academic/EnrollmentService.cs` |

---

### `StudentRegistrationService` — `src/Tabsan.EduSphere.Application/Academic/StudentRegistrationService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SelfRegisterAsync(request, ct)` | Whitelist-gated self-registration: validates identifier, creates User + StudentProfile atomically, marks whitelist entry consumed. Returns new User ID or null. | `Application/Academic/StudentRegistrationService.cs` |
| `CreateProfileAsync(request, ct)` | Admin-managed profile creation for an existing User — bypasses the whitelist gate. Throws on duplicate registration number. | `Application/Academic/StudentRegistrationService.cs` |

---

### `DepartmentController` — `src/Tabsan.EduSphere.API/Controllers/DepartmentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/department` — returns all active departments. Authenticated. | `API/Controllers/DepartmentController.cs` |
| `GetById(id, ct)` | `GET /api/v1/department/{id}` — returns a single department. | `API/Controllers/DepartmentController.cs` |
| `Create(request, ct)` | `POST /api/v1/department` — creates a new department. Admin+. | `API/Controllers/DepartmentController.cs` |
| `Update(id, request, ct)` | `PUT /api/v1/department/{id}` — renames the department. Admin+. | `API/Controllers/DepartmentController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/department/{id}` — soft-deactivates the department. SuperAdmin only. | `API/Controllers/DepartmentController.cs` |
| `GetUserId()` | Private helper — extracts the JWT sub claim as a GUID. | `API/Controllers/DepartmentController.cs` |

---

### `ProgramController` — `src/Tabsan.EduSphere.API/Controllers/ProgramController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(departmentId?, ct)` | `GET /api/v1/program` — returns programmes, optionally filtered by department. | `API/Controllers/ProgramController.cs` |
| `GetById(id, ct)` | `GET /api/v1/program/{id}` — returns a single programme. | `API/Controllers/ProgramController.cs` |
| `Create(request, ct)` | `POST /api/v1/program` — creates a new degree programme. Admin+. | `API/Controllers/ProgramController.cs` |
| `Update(id, request, ct)` | `PUT /api/v1/program/{id}` — renames the programme. Admin+. | `API/Controllers/ProgramController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/program/{id}` — soft-deactivates. SuperAdmin only. | `API/Controllers/ProgramController.cs` |

---

### `SemesterController` — `src/Tabsan.EduSphere.API/Controllers/SemesterController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/semester` — returns all semesters ordered by start date. | `API/Controllers/SemesterController.cs` |
| `GetCurrent(ct)` | `GET /api/v1/semester/current` — returns the current open semester. | `API/Controllers/SemesterController.cs` |
| `GetById(id, ct)` | `GET /api/v1/semester/{id}` — returns a single semester. | `API/Controllers/SemesterController.cs` |
| `Create(request, ct)` | `POST /api/v1/semester` — creates a new semester. Admin+. | `API/Controllers/SemesterController.cs` |
| `Close(id, ct)` | `POST /api/v1/semester/{id}/close` — permanently closes the semester. Admin+. One-way operation. | `API/Controllers/SemesterController.cs` |

---

### `CourseController` — `src/Tabsan.EduSphere.API/Controllers/CourseController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(departmentId?, ct)` | `GET /api/v1/course` — returns the course catalogue, optionally filtered. | `API/Controllers/CourseController.cs` |
| `GetById(id, ct)` | `GET /api/v1/course/{id}` — returns a single course. | `API/Controllers/CourseController.cs` |
| `Create(request, ct)` | `POST /api/v1/course` — adds a course to the catalogue. Admin+. | `API/Controllers/CourseController.cs` |
| `UpdateTitle(id, request, ct)` | `PUT /api/v1/course/{id}/title` — updates the course title. Admin+. | `API/Controllers/CourseController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/course/{id}` — soft-deactivates the course. SuperAdmin. | `API/Controllers/CourseController.cs` |
| `GetOfferings(semesterId, ct)` | `GET /api/v1/course/offerings?semesterId=` — returns offerings for a semester. | `API/Controllers/CourseController.cs` |
| `GetMyOfferings(ct)` | `GET /api/v1/course/offerings/my` — returns offerings assigned to the calling faculty, filtered to assigned departments. | `API/Controllers/CourseController.cs` |
| `CreateOffering(request, ct)` | `POST /api/v1/course/offerings` — creates a course offering. Admin+. | `API/Controllers/CourseController.cs` |
| `AssignFaculty(id, request, ct)` | `PUT /api/v1/course/offerings/{id}/faculty` — assigns faculty to an offering. Admin+. | `API/Controllers/CourseController.cs` |
| `GetUserId()` | Private helper — extracts the JWT sub claim as a GUID. | `API/Controllers/CourseController.cs` |

---

### `EnrollmentController` — `src/Tabsan.EduSphere.API/Controllers/EnrollmentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Enroll(request, ct)` | `POST /api/v1/enrollment` — enrolls the calling student into a course offering. Student role. | `API/Controllers/EnrollmentController.cs` |
| `Drop(offeringId, ct)` | `DELETE /api/v1/enrollment/{offeringId}` — drops the student's active enrollment. Student role. | `API/Controllers/EnrollmentController.cs` |
| `MyCourses(ct)` | `GET /api/v1/enrollment/my-courses` — returns the student's full enrollment history. Student role. | `API/Controllers/EnrollmentController.cs` |
| `GetRoster(offeringId, ct)` | `GET /api/v1/enrollment/roster/{offeringId}` — returns active enrollments for an offering. Faculty/Admin+. | `API/Controllers/EnrollmentController.cs` |
| `GetUserId()` | Private helper — extracts the JWT sub claim as a GUID. | `API/Controllers/EnrollmentController.cs` |

---

### `StudentController` — `src/Tabsan.EduSphere.API/Controllers/StudentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SelfRegister(request, ct)` | `POST /api/v1/student/register` — public whitelist-gated self-registration. AllowAnonymous. | `API/Controllers/StudentController.cs` |
| `GetMyProfile(ct)` | `GET /api/v1/student/profile` — returns the calling student's academic profile. Student role. | `API/Controllers/StudentController.cs` |
| `GetAll(departmentId?, ct)` | `GET /api/v1/student` — returns all student profiles, optionally by department. Admin+. | `API/Controllers/StudentController.cs` |
| `Create(request, ct)` | `POST /api/v1/student` — Admin-managed student profile creation. Admin+. | `API/Controllers/StudentController.cs` |
| `AddWhitelistEntry(request, ct)` | `POST /api/v1/student/whitelist` — adds a single registration whitelist entry. Admin+. | `API/Controllers/StudentController.cs` |
| `BulkAddWhitelistEntries(requests, ct)` | `POST /api/v1/student/whitelist/bulk` — bulk-imports whitelist entries. Admin+. | `API/Controllers/StudentController.cs` |
| `GetUserId()` | Private helper — extracts the JWT sub claim as a GUID. | `API/Controllers/StudentController.cs` |

---

## Phase 3 � Assignments and Results

### Domain � Assignment

| Function Name | Purpose | Location |
|---|---|---|
| `Assignment(courseOfferingId, title, description, dueDate, maxMarks)` | Constructor � creates an unpublished assignment. | `Domain/Assignments/Assignment.cs` |
| `Publish()` | Marks the assignment as published (visible to students). Throws if already published. | `Domain/Assignments/Assignment.cs` |
| `Retract()` | Withdraws a published assignment. Throws if not published. | `Domain/Assignments/Assignment.cs` |
| `Update(title, description, dueDate, maxMarks)` | Updates editable fields. Throws if already published. | `Domain/Assignments/Assignment.cs` |

### Domain � AssignmentSubmission

| Function Name | Purpose | Location |
|---|---|---|
| `AssignmentSubmission(assignmentId, studentProfileId, fileUrl, textContent)` | Constructor � requires at least one of fileUrl/textContent. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Grade(marksAwarded, feedback, gradedByUserId)` | Records marks and feedback. Throws if submission was Rejected. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Reject()` | Marks submission as Rejected and clears awarded marks. | `Domain/Assignments/AssignmentSubmission.cs` |

### Domain � Result

| Function Name | Purpose | Location |
|---|---|---|
| `Result(studentProfileId, courseOfferingId, resultType, marksObtained, maxMarks)` | Constructor � validates marks range. | `Domain/Assignments/Result.cs` |
| `Publish(publishedByUserId)` | One-way publication. Throws if already published. | `Domain/Assignments/Result.cs` |
| `CorrectMarks(newMarksObtained, newMaxMarks)` | Admin-only correction of a published result. Validates range. | `Domain/Assignments/Result.cs` |

### Domain � TranscriptExportLog

| Function Name | Purpose | Location |
|---|---|---|
| `TranscriptExportLog(studentProfileId, requestedByUserId, format, documentUrl?, ipAddress?)` | Constructor � append-only, immutable after creation. | `Domain/Assignments/TranscriptExportLog.cs` |

### Infrastructure � AssignmentRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetByOfferingAsync(courseOfferingId, ct)` | Returns non-deleted assignments for the offering, ordered by due date. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetByIdAsync(id, ct)` | Returns assignment by ID (soft-delete filter applied). | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `TitleExistsAsync(courseOfferingId, title, ct)` | Returns true when the offering already has an assignment with that title. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `AddAsync(assignment, ct)` | Queues assignment for insertion. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `Update(assignment)` | Marks assignment as modified. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetSubmissionAsync(assignmentId, studentProfileId, ct)` | Returns the submission for a student+assignment pair or null. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetSubmissionsByAssignmentAsync(assignmentId, ct)` | Returns all submissions for an assignment, ordered by submission date. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetSubmissionsByStudentAsync(studentProfileId, ct)` | Returns all submissions by a student with assignment navigation loaded. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `HasSubmittedAsync(assignmentId, studentProfileId, ct)` | Returns true when the student has already submitted. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetSubmissionCountAsync(assignmentId, ct)` | Returns the total submission count for an assignment. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `AddSubmissionAsync(submission, ct)` | Queues submission for insertion. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `UpdateSubmission(submission)` | Marks submission as modified. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes (AssignmentRepository). | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |

### Infrastructure � ResultRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetAsync(studentProfileId, courseOfferingId, resultType, ct)` | Returns the specific result row for the combination or null. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetByStudentAsync(studentProfileId, ct)` | Returns all results for a student (draft + published). | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetPublishedByStudentAsync(studentProfileId, ct)` | Returns only published results for a student. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetByOfferingAsync(courseOfferingId, ct)` | Returns all results for a course offering. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `ExistsAsync(studentProfileId, courseOfferingId, resultType, ct)` | Returns true when a result row already exists for the combination. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `AddAsync(result, ct)` | Queues a result for insertion. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `AddRangeAsync(results, ct)` | Queues multiple results for bulk insertion. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `Update(result)` | Marks a result as modified. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `GetExportLogsAsync(studentProfileId, ct)` | Returns all export logs for a student, newest first. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `AddExportLogAsync(log, ct)` | Queues a transcript export log for insertion. | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes (ResultRepository). | `Infrastructure/Repositories/AssignmentResultRepositories.cs` |

### Application � AssignmentService

| Function Name | Purpose | Location |
|---|---|---|
| `CreateAsync(request, createdByUserId, ct)` | Creates an unpublished assignment and logs the action. | `Application/Assignments/AssignmentService.cs` |
| `UpdateAsync(assignmentId, request, ct)` | Updates draft assignment fields. Returns false if published. | `Application/Assignments/AssignmentService.cs` |
| `PublishAsync(assignmentId, ct)` | Publishes an assignment so students can submit. | `Application/Assignments/AssignmentService.cs` |
| `RetractAsync(assignmentId, ct)` | Retracts a published assignment. Fails if submissions exist. | `Application/Assignments/AssignmentService.cs` |
| `DeleteAsync(assignmentId, ct)` | Soft-deletes an assignment. Fails if submissions exist. | `Application/Assignments/AssignmentService.cs` |
| `GetByOfferingAsync(courseOfferingId, ct)` | Returns all assignments for an offering with submission counts. | `Application/Assignments/AssignmentService.cs` |
| `GetByIdAsync(assignmentId, ct)` | Returns a single assignment with submission count, or null. | `Application/Assignments/AssignmentService.cs` |
| `SubmitAsync(studentProfileId, request, ct)` | Submits student work; enforces published, not past due, no duplicate. | `Application/Assignments/AssignmentService.cs` |
| `GetMySubmissionsAsync(studentProfileId, ct)` | Returns all submissions by the student with assignment titles. | `Application/Assignments/AssignmentService.cs` |
| `GetSubmissionsByAssignmentAsync(assignmentId, ct)` | Returns all submissions for an assignment (faculty grading view). | `Application/Assignments/AssignmentService.cs` |
| `GradeSubmissionAsync(request, gradedByUserId, ct)` | Grades a submission; validates marks <= MaxMarks. | `Application/Assignments/AssignmentService.cs` |
| `RejectSubmissionAsync(assignmentId, studentProfileId, ct)` | Rejects a submission. Returns false if not found. | `Application/Assignments/AssignmentService.cs` |
| `ToResponse(assignment, submissionCount)` | Private � maps Assignment to AssignmentResponse DTO. | `Application/Assignments/AssignmentService.cs` |
| `ToSubmissionResponse(submission, assignmentTitle)` | Private � maps AssignmentSubmission to SubmissionResponse DTO. | `Application/Assignments/AssignmentService.cs` |

### Application � ResultService

| Function Name | Purpose | Location |
|---|---|---|
| `CreateAsync(request, ct)` | Creates a draft result entry. Throws on duplicate. | `Application/Assignments/ResultService.cs` |
| `BulkCreateAsync(request, ct)` | Bulk-inserts draft results; skips existing. Returns inserted count. | `Application/Assignments/ResultService.cs` |
| `PublishAsync(studentProfileId, courseOfferingId, resultType, publishedByUserId, ct)` | Publishes a single result. Returns false if not found or already published. | `Application/Assignments/ResultService.cs` |
| `PublishAllForOfferingAsync(courseOfferingId, publishedByUserId, ct)` | Bulk-publishes all draft results for an offering. Returns published count. | `Application/Assignments/ResultService.cs` |
| `CorrectAsync(studentProfileId, courseOfferingId, resultType, request, correctedByUserId, ct)` | Admin correction of a published result with audit logging. | `Application/Assignments/ResultService.cs` |
| `GetByStudentAsync(studentProfileId, ct)` | Returns all results for a student (draft + published). | `Application/Assignments/ResultService.cs` |
| `GetPublishedByStudentAsync(studentProfileId, ct)` | Returns only published results for a student. | `Application/Assignments/ResultService.cs` |
| `GetByOfferingAsync(courseOfferingId, ct)` | Returns all results for a course offering. | `Application/Assignments/ResultService.cs` |
| `ExportTranscriptAsync(request, requestedByUserId, ipAddress, ct)` | Exports transcript, logs to TranscriptExportLog and AuditLog. | `Application/Assignments/ResultService.cs` |
| `GetExportHistoryAsync(studentProfileId, ct)` | Returns transcript export history for a student. | `Application/Assignments/ResultService.cs` |
| `ToResponse(result)` | Private � maps Result to ResultResponse DTO including percentage. | `Application/Assignments/ResultService.cs` |

### API � AssignmentController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/assignment` � creates an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Update(id, request, ct)` | `PUT /api/assignment/{id}` � updates a draft assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Publish(id, ct)` | `POST /api/assignment/{id}/publish` � publishes an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Retract(id, ct)` | `POST /api/assignment/{id}/retract` � retracts a published assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Delete(id, ct)` | `DELETE /api/assignment/{id}` � soft-deletes when no submissions exist. Admin. | `API/Controllers/AssignmentController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/assignment/by-offering/{id}` � lists assignments for an offering. | `API/Controllers/AssignmentController.cs` |
| `GetById(id, ct)` | `GET /api/assignment/{id}` � returns a single assignment. | `API/Controllers/AssignmentController.cs` |
| `Submit(request, ct)` | `POST /api/assignment/submit` � student submission. Student. | `API/Controllers/AssignmentController.cs` |
| `GetMySubmissions(ct)` | `GET /api/assignment/my-submissions` � student's own submissions. Student. | `API/Controllers/AssignmentController.cs` |
| `GetSubmissions(id, ct)` | `GET /api/assignment/{id}/submissions` � all submissions for an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Grade(request, ct)` | `PUT /api/assignment/submissions/grade` � grades a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Reject(assignmentId, studentProfileId, ct)` | `POST /api/assignment/{id}/submissions/{studentId}/reject` � rejects a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentUserId()` | Private � extracts user ID from JWT NameIdentifier claim. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentStudentProfileId()` | Private � extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/AssignmentController.cs` |

### API � ResultController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/result` � creates a draft result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `BulkCreate(request, ct)` | `POST /api/result/bulk` � bulk-creates draft results for a class. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Publish(studentProfileId, courseOfferingId, resultType, ct)` | `POST /api/result/publish` � publishes a single result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `PublishAll(courseOfferingId, ct)` | `POST /api/result/publish-all` � publishes all drafts for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Correct(studentProfileId, courseOfferingId, resultType, request, ct)` | `PUT /api/result/correct` � Admin correction of a published result. Admin only. | `API/Controllers/ResultController.cs` |
| `GetMyResults(ct)` | `GET /api/result/my-results` � student's own published results. Student. | `API/Controllers/ResultController.cs` |
| `GetByStudent(studentProfileId, ct)` | `GET /api/result/by-student/{id}` � all results for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/result/by-offering/{id}` � all results for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetTranscript(studentProfileId, format, ct)` | `GET /api/result/transcript/{id}` � exports transcript, logs request. All roles. | `API/Controllers/ResultController.cs` |
| `GetTranscriptHistory(studentProfileId, ct)` | `GET /api/result/transcript/{id}/history` � export history for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetCurrentUserId()` | Private � extracts user ID from JWT NameIdentifier claim. | `API/Controllers/ResultController.cs` |
| `GetCurrentStudentProfileId()` | Private � extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/ResultController.cs` |
