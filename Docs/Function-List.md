# Function List â€” Tabsan EduSphere

> **Maintenance rule**: Every function added to the codebase must be registered here with Name, Purpose, and Location.
> Format: `Name | Purpose | Location`

---

## Domain Layer

### `BaseEntity` â€” `src/Tabsan.EduSphere.Domain/Common/BaseEntity.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Touch()` | Updates the `UpdatedAt` timestamp to the current UTC time. Called by all domain mutation methods and by DbContext before SaveChanges. | `Domain/Common/BaseEntity.cs` |

---

### `AuditableEntity` â€” `src/Tabsan.EduSphere.Domain/Common/AuditableEntity.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SoftDelete()` | Marks the entity as deleted (`IsDeleted = true`, sets `DeletedAt`) without physically removing the database row. | `Domain/Common/AuditableEntity.cs` |
| `Restore()` | Reverses a soft delete â€” clears `IsDeleted` and `DeletedAt`. | `Domain/Common/AuditableEntity.cs` |

---

### `User` â€” `src/Tabsan.EduSphere.Domain/Identity/User.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RecordLogin()` | Sets `LastLoginAt` to the current UTC time after a successful authentication. | `Domain/Identity/User.cs` |
| `UpdatePasswordHash(newHash)` | Replaces the stored password hash when the user changes their password. | `Domain/Identity/User.cs` |
| `Deactivate()` | Sets `IsActive = false` to prevent the user from logging in without deleting the account. | `Domain/Identity/User.cs` |
| `Activate()` | Re-enables a previously deactivated user account. | `Domain/Identity/User.cs` |
| `UpdateEmail(email)` | Updates the user's email address with basic format validation. | `Domain/Identity/User.cs` |

---

### `UserSession` â€” `src/Tabsan.EduSphere.Domain/Identity/UserSession.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `IsActive` (computed property) | Returns true when the session has not been revoked and the expiry is in the future. | `Domain/Identity/UserSession.cs` |
| `Revoke()` | Stamps `RevokedAt` with the current UTC time, invalidating the session for all future refresh attempts. | `Domain/Identity/UserSession.cs` |
| `Rotate(newHash, newExpiry)` | Replaces the refresh token hash and extends the expiry â€” used during token rotation on refresh. | `Domain/Identity/UserSession.cs` |

---

### `Department` â€” `src/Tabsan.EduSphere.Domain/Academic/Department.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Rename(newName)` | Updates the display name of the department. | `Domain/Academic/Department.cs` |
| `Deactivate()` | Marks the department inactive so it is hidden from assignment dropdowns. | `Domain/Academic/Department.cs` |
| `Activate()` | Re-activates a previously deactivated department. | `Domain/Academic/Department.cs` |

---

### `LicenseState` â€” `src/Tabsan.EduSphere.Domain/Licensing/LicenseState.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RefreshStatus()` | Re-evaluates license validity against current UTC time and updates `Status` to Active or Expired. | `Domain/Licensing/LicenseState.cs` |
| `MarkInvalid()` | Forces the status to `Invalid` when the signature check fails. | `Domain/Licensing/LicenseState.cs` |
| `Replace(newHash, newType, newExpiry)` | Replaces the current license record with data from a newly uploaded and validated license file. | `Domain/Licensing/LicenseState.cs` |

---

### `ModuleStatus` â€” `src/Tabsan.EduSphere.Domain/Modules/ModuleStatus.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Activate(changedBy, source)` | Activates the module and records who changed it and when. | `Domain/Modules/ModuleStatus.cs` |
| `Deactivate(changedBy)` | Deactivates the module, preserving data but blocking UI and API access. | `Domain/Modules/ModuleStatus.cs` |

---

### `AuditLog` â€” `src/Tabsan.EduSphere.Domain/Auditing/AuditLog.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `AuditLog(action, entityName, ...)` | Constructor â€” creates an immutable audit record for a privileged action. All audit writes use this constructor to ensure no field is omitted. | `Domain/Auditing/AuditLog.cs` |

---

## Infrastructure Layer

### `ApplicationDbContext` â€” `src/Tabsan.EduSphere.Infrastructure/Persistence/ApplicationDbContext.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `OnModelCreating(modelBuilder)` | Discovers and applies all `IEntityTypeConfiguration` implementations in the assembly automatically. | `Infrastructure/Persistence/ApplicationDbContext.cs` |
| `SaveChangesAsync(cancellationToken)` | Overrides EF Core save to call `Touch()` on all modified entities before writing to the database. | `Infrastructure/Persistence/ApplicationDbContext.cs` |
| `SetAuditTimestamps()` | Iterates all tracked `BaseEntity` entries in Modified state and calls `Touch()`. Called by `SaveChangesAsync`. | `Infrastructure/Persistence/ApplicationDbContext.cs` |

---

### `UserRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/UserRepository.cs`

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

### `LicenseRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/LicenseRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetCurrentAsync(ct)` | Returns the most recently activated license row, or null. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `AddAsync(state, ct)` | Queues a new LicenseState record for insertion. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `Update(state)` | Marks the existing LicenseState as modified. | `Infrastructure/Repositories/LicenseRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/LicenseRepository.cs` |

---

### `ModuleRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/ModuleRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllWithStatusAsync(ct)` | Returns all module definitions. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `GetStatusByKeyAsync(moduleKey, ct)` | Returns the ModuleStatus row for the given module key, or null. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `IsActiveAsync(moduleKey, ct)` | Returns true when the named module is active (lightweight query, no nav props). | `Infrastructure/Repositories/ModuleRepository.cs` |
| `UpdateStatus(status)` | Marks a ModuleStatus entity as Modified. | `Infrastructure/Repositories/ModuleRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/ModuleRepository.cs` |

---

### `UserSessionRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/UserSessionRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetActiveByHashAsync(tokenHash, ct)` | Returns the non-revoked session matching the token hash, or null. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `AddAsync(session, ct)` | Queues a new UserSession for insertion. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `Update(session)` | Marks the session as Modified. | `Infrastructure/Repositories/UserSessionRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/UserSessionRepository.cs` |

---

### `TokenService` â€” `src/Tabsan.EduSphere.Infrastructure/Auth/TokenService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GenerateAccessToken(user)` | Builds and signs a JWT access token with user ID, username, role, and department ID claims. | `Infrastructure/Auth/TokenService.cs` |
| `GenerateRefreshToken()` | Generates a cryptographically random 64-byte Base64 refresh token. | `Infrastructure/Auth/TokenService.cs` |
| `HashRefreshToken(rawToken)` | Computes the SHA-256 hex hash of a raw refresh token for safe storage. | `Infrastructure/Auth/TokenService.cs` |
| `GetRefreshTokenExpiry()` | Returns the UTC expiry DateTime for a new refresh token based on configured days. | `Infrastructure/Auth/TokenService.cs` |

---

### `PasswordHasher` â€” `src/Tabsan.EduSphere.Infrastructure/Auth/PasswordHasher.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Hash(password)` | Produces a PBKDF2-HMACSHA512 hash of the plain-text password for storage. | `Infrastructure/Auth/PasswordHasher.cs` |
| `Verify(storedHash, providedPassword)` | Returns true when the plain-text password matches the stored hash. | `Infrastructure/Auth/PasswordHasher.cs` |

---

### `LicenseValidationService` â€” `src/Tabsan.EduSphere.Infrastructure/Licensing/LicenseValidationService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `ActivateFromFileAsync(filePath, ct)` | Reads, deserialises, and signature-verifies a license file; creates or replaces the LicenseState record. Returns true on success. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `ValidateCurrentAsync(ct)` | Refreshes the stored license status against the current time. Used at startup, on Super Admin login, and by the daily background job. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `VerifySignature(payload)` | Reconstructs the canonical signed string and verifies the RSA-SHA256 signature using the embedded public key. | `Infrastructure/Licensing/LicenseValidationService.cs` |
| `ComputeFileHash(bytes)` | Computes the SHA-256 hex hash of the raw license file bytes for change detection. | `Infrastructure/Licensing/LicenseValidationService.cs` |

---

### `ModuleEntitlementResolver` â€” `src/Tabsan.EduSphere.Infrastructure/Modules/ModuleEntitlementResolver.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `IsActiveAsync(moduleKey, ct)` | Returns true when the named module is active; uses a 60-second memory cache to avoid DB hits on every request. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |
| `InvalidateCache(moduleKey)` | Removes the cache entry for a single module after a Super Admin toggle. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |
| `InvalidateAll()` | Clears all module entitlement cache entries after bulk changes or a license update. | `Infrastructure/Modules/ModuleEntitlementResolver.cs` |

---

### `AuditService` â€” `src/Tabsan.EduSphere.Infrastructure/Auditing/AuditService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `LogAsync(entry, ct)` | Appends a new audit log entry to the database asynchronously. | `Infrastructure/Auditing/AuditService.cs` |

---

### `DatabaseSeeder` â€” `src/Tabsan.EduSphere.Infrastructure/Persistence/DatabaseSeeder.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SeedAsync(services)` | Entry point called from Program.cs; applies EF migrations then calls all seed methods in order. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedRolesAsync(db)` | Inserts the four system roles (SuperAdmin, Admin, Faculty, Student) if they do not already exist. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedModulesAsync(db)` | Inserts all known module definitions and creates a default ModuleStatus row for each. | `Infrastructure/Persistence/DatabaseSeeder.cs` |
| `SeedSuperAdminAsync(db, hasher)` | Creates the bootstrap Super Admin account from environment variables if no SuperAdmin user exists yet. | `Infrastructure/Persistence/DatabaseSeeder.cs` |

---

## Application Layer

### `AuthService` â€” `src/Tabsan.EduSphere.Application/Auth/AuthService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `LoginAsync(request, ipAddress, ct)` | Validates credentials, creates a UserSession, and returns JWT + refresh token. | `Application/Auth/AuthService.cs` |
| `RefreshAsync(rawRefreshToken, ipAddress, ct)` | Rotates the refresh token and returns a new token pair. | `Application/Auth/AuthService.cs` |
| `LogoutAsync(rawRefreshToken, ct)` | Revokes the session associated with the refresh token. | `Application/Auth/AuthService.cs` |
| `ChangePasswordAsync(userId, request, ct)` | Verifies current password and replaces it with the new hash. | `Application/Auth/AuthService.cs` |

---

### `ModuleService` â€” `src/Tabsan.EduSphere.Application/Modules/ModuleService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllAsync(ct)` | Returns all module definitions with their current activation status. | `Application/Modules/ModuleService.cs` |
| `ActivateAsync(moduleKey, changedByUserId, ct)` | Activates the named module, clears the entitlement cache, and writes an audit log. | `Application/Modules/ModuleService.cs` |
| `DeactivateAsync(moduleKey, changedByUserId, ct)` | Deactivates the named module (throws if mandatory), clears cache, writes audit log. | `Application/Modules/ModuleService.cs` |

---

## API Layer

### `AuthController` â€” `src/Tabsan.EduSphere.API/Controllers/AuthController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Login(request, ct)` | `POST /api/v1/auth/login` â€” authenticates the user and returns tokens. | `API/Controllers/AuthController.cs` |
| `Refresh(request, ct)` | `POST /api/v1/auth/refresh` â€” rotates the refresh token and returns a new pair. | `API/Controllers/AuthController.cs` |
| `Logout(request, ct)` | `POST /api/v1/auth/logout` â€” revokes the session. | `API/Controllers/AuthController.cs` |
| `ChangePassword(request, ct)` | `PUT /api/v1/auth/change-password` â€” changes the authenticated user's password. | `API/Controllers/AuthController.cs` |

---

### `ModuleController` â€” `src/Tabsan.EduSphere.API/Controllers/ModuleController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/modules` â€” returns all modules with status. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Activate(key, ct)` | `POST /api/v1/modules/{key}/activate` â€” activates the named module. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Deactivate(key, ct)` | `POST /api/v1/modules/{key}/deactivate` â€” deactivates the named module. Requires SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `Status(key, ct)` | `GET /api/v1/modules/{key}/status` â€” returns the current active/inactive state from cache. | `API/Controllers/ModuleController.cs` |
| `GetUserId()` | Private helper â€” extracts the authenticated user's GUID from the JWT sub claim. | `API/Controllers/ModuleController.cs` |

---

### `LicenseController` â€” `src/Tabsan.EduSphere.API/Controllers/LicenseController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Upload(file, ct)` | `POST /api/v1/license/upload` â€” saves and validates a new license file. Requires SuperAdmin. | `API/Controllers/LicenseController.cs` |
| `Status(ct)` | `GET /api/v1/license/status` â€” runs an on-demand license check and returns current status. Requires SuperAdmin. | `API/Controllers/LicenseController.cs` |

---

## Background Jobs

### `LicenseCheckWorker` â€” `src/Tabsan.EduSphere.BackgroundJobs/LicenseCheckWorker.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `ExecuteAsync(stoppingToken)` | Main hosted-service loop â€” waits 30 s on startup, then calls `RunCheckAsync` every 24 hours. | `BackgroundJobs/LicenseCheckWorker.cs` |
| `RunCheckAsync(ct)` | Opens a fresh DI scope, resolves `LicenseValidationService`, and runs a validation check. Exceptions are caught and logged. | `BackgroundJobs/LicenseCheckWorker.cs` |

---

## Phase 2 â€” Academic Core

### `AcademicProgram` â€” `src/Tabsan.EduSphere.Domain/Academic/AcademicProgram.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `AcademicProgram(name, code, departmentId, totalSemesters)` | Constructor â€” creates a new degree programme; normalises code to uppercase. | `Domain/Academic/AcademicProgram.cs` |
| `Rename(newName)` | Updates the display name of the programme. | `Domain/Academic/AcademicProgram.cs` |
| `Deactivate()` | Marks the programme inactive so it no longer appears in registration dropdowns. | `Domain/Academic/AcademicProgram.cs` |
| `Activate()` | Re-activates a previously deactivated programme. | `Domain/Academic/AcademicProgram.cs` |

---

### `Semester` â€” `src/Tabsan.EduSphere.Domain/Academic/Semester.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Semester(name, startDate, endDate)` | Constructor â€” creates a new open semester term. | `Domain/Academic/Semester.cs` |
| `Close()` | Permanently closes the semester. One-way: throws if already closed. | `Domain/Academic/Semester.cs` |

---

### `Course` â€” `src/Tabsan.EduSphere.Domain/Academic/Course.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Course(title, code, creditHours, departmentId)` | Constructor â€” creates a new course catalogue entry; normalises code to uppercase. | `Domain/Academic/Course.cs` |
| `UpdateTitle(newTitle)` | Updates the course display title. | `Domain/Academic/Course.cs` |
| `Deactivate()` | Soft-deactivates the course so it cannot be offered. | `Domain/Academic/Course.cs` |
| `Activate()` | Re-activates a deactivated course. | `Domain/Academic/Course.cs` |

---

### `CourseOffering` â€” `src/Tabsan.EduSphere.Domain/Academic/CourseOffering.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `CourseOffering(courseId, semesterId, maxEnrollment, facultyUserId?)` | Constructor â€” schedules a course for a semester. | `Domain/Academic/CourseOffering.cs` |
| `AssignFaculty(facultyUserId)` | Assigns or re-assigns a faculty member to this offering. | `Domain/Academic/CourseOffering.cs` |
| `Close()` | Closes the offering so no new enrollments are accepted. | `Domain/Academic/CourseOffering.cs` |
| `Reopen()` | Re-opens the offering to accept enrollments again. | `Domain/Academic/CourseOffering.cs` |
| `UpdateMaxEnrollment(max)` | Changes the maximum enrollment capacity. | `Domain/Academic/CourseOffering.cs` |

---

### `StudentProfile` â€” `src/Tabsan.EduSphere.Domain/Academic/StudentProfile.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `StudentProfile(userId, registrationNumber, programId, departmentId, admissionDate)` | Constructor â€” creates a student's academic profile. | `Domain/Academic/StudentProfile.cs` |
| `UpdateCgpa(newCgpa)` | Updates the cumulative GPA after result publication (0.0â€“4.0 range enforced). | `Domain/Academic/StudentProfile.cs` |
| `AdvanceSemester()` | Increments the student's current semester number. | `Domain/Academic/StudentProfile.cs` |

---

### `Enrollment` â€” `src/Tabsan.EduSphere.Domain/Academic/Enrollment.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Enrollment(studentProfileId, courseOfferingId)` | Constructor â€” records a new active enrollment. | `Domain/Academic/Enrollment.cs` |
| `Drop()` | Changes status to Dropped and sets DroppedAt. Throws if not Active. | `Domain/Academic/Enrollment.cs` |
| `Cancel()` | Changes status to Cancelled (used when the offering itself is cancelled). | `Domain/Academic/Enrollment.cs` |

---

### `RegistrationWhitelist` â€” `src/Tabsan.EduSphere.Domain/Academic/RegistrationWhitelist.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `RegistrationWhitelist(identifierType, identifierValue, departmentId, programId)` | Constructor â€” creates a pre-approved registration entry; normalises identifier to lowercase. | `Domain/Academic/RegistrationWhitelist.cs` |
| `MarkUsed(createdUserId)` | Marks the entry as consumed after a successful self-registration. Throws if already used. | `Domain/Academic/RegistrationWhitelist.cs` |

---

### `FacultyDepartmentAssignment` â€” `src/Tabsan.EduSphere.Domain/Academic/FacultyDepartmentAssignment.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `FacultyDepartmentAssignment(facultyUserId, departmentId)` | Constructor â€” creates an active assignment linking a faculty member to a department. | `Domain/Academic/FacultyDepartmentAssignment.cs` |
| `Remove()` | Marks the assignment as removed by setting RemovedAt. | `Domain/Academic/FacultyDepartmentAssignment.cs` |

---

### `DepartmentRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/DepartmentRepository.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAllAsync(ct)` | Returns all non-deleted departments ordered by name. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `GetByIdAsync(id, ct)` | Returns the department with the given ID, or null. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `CodeExistsAsync(code, ct)` | Returns true when the uppercase code is already in use. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `AddAsync(department, ct)` | Queues a new department for insertion. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `Update(department)` | Marks the department as modified. | `Infrastructure/Repositories/DepartmentRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes. | `Infrastructure/Repositories/DepartmentRepository.cs` |

---

### `AcademicProgramRepository` + `SemesterRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicRepositories.cs`

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

### `CourseRepository` â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/CourseRepository.cs`

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

### Support Repositories â€” `src/Tabsan.EduSphere.Infrastructure/Repositories/AcademicSupportRepositories.cs`

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

### `EnrollmentService` â€” `src/Tabsan.EduSphere.Application/Academic/EnrollmentService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `EnrollAsync(studentProfileId, request, ct)` | Validates offering state, checks duplicates and seat availability, creates an Enrollment row, and returns a response DTO. Returns null on rejection. | `Application/Academic/EnrollmentService.cs` |
| `DropAsync(studentProfileId, courseOfferingId, ct)` | Changes an active enrollment's status to Dropped. Returns false when no active enrollment exists. | `Application/Academic/EnrollmentService.cs` |
| `GetForStudentAsync(studentProfileId, ct)` | Returns all enrollment records for the student (full history). | `Application/Academic/EnrollmentService.cs` |
| `GetForOfferingAsync(courseOfferingId, ct)` | Returns active enrollments for the given offering (faculty roster). | `Application/Academic/EnrollmentService.cs` |

---

### `StudentRegistrationService` â€” `src/Tabsan.EduSphere.Application/Academic/StudentRegistrationService.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SelfRegisterAsync(request, ct)` | Whitelist-gated self-registration: validates identifier, creates User + StudentProfile atomically, marks whitelist entry consumed. Returns new User ID or null. | `Application/Academic/StudentRegistrationService.cs` |
| `CreateProfileAsync(request, ct)` | Admin-managed profile creation for an existing User â€” bypasses the whitelist gate. Throws on duplicate registration number. | `Application/Academic/StudentRegistrationService.cs` |

---

### `DepartmentController` â€” `src/Tabsan.EduSphere.API/Controllers/DepartmentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/department` â€” returns all active departments. Authenticated. | `API/Controllers/DepartmentController.cs` |
| `GetById(id, ct)` | `GET /api/v1/department/{id}` â€” returns a single department. | `API/Controllers/DepartmentController.cs` |
| `Create(request, ct)` | `POST /api/v1/department` â€” creates a new department. Admin+. | `API/Controllers/DepartmentController.cs` |
| `Update(id, request, ct)` | `PUT /api/v1/department/{id}` â€” renames the department. Admin+. | `API/Controllers/DepartmentController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/department/{id}` â€” soft-deactivates the department. SuperAdmin only. | `API/Controllers/DepartmentController.cs` |
| `GetUserId()` | Private helper â€” extracts the JWT sub claim as a GUID. | `API/Controllers/DepartmentController.cs` |

---

### `ProgramController` â€” `src/Tabsan.EduSphere.API/Controllers/ProgramController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(departmentId?, ct)` | `GET /api/v1/program` â€” returns programmes, optionally filtered by department. | `API/Controllers/ProgramController.cs` |
| `GetById(id, ct)` | `GET /api/v1/program/{id}` â€” returns a single programme. | `API/Controllers/ProgramController.cs` |
| `Create(request, ct)` | `POST /api/v1/program` â€” creates a new degree programme. Admin+. | `API/Controllers/ProgramController.cs` |
| `Update(id, request, ct)` | `PUT /api/v1/program/{id}` â€” renames the programme. Admin+. | `API/Controllers/ProgramController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/program/{id}` â€” soft-deactivates. SuperAdmin only. | `API/Controllers/ProgramController.cs` |

---

### `SemesterController` â€” `src/Tabsan.EduSphere.API/Controllers/SemesterController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(ct)` | `GET /api/v1/semester` â€” returns all semesters ordered by start date. | `API/Controllers/SemesterController.cs` |
| `GetCurrent(ct)` | `GET /api/v1/semester/current` â€” returns the current open semester. | `API/Controllers/SemesterController.cs` |
| `GetById(id, ct)` | `GET /api/v1/semester/{id}` â€” returns a single semester. | `API/Controllers/SemesterController.cs` |
| `Create(request, ct)` | `POST /api/v1/semester` â€” creates a new semester. Admin+. | `API/Controllers/SemesterController.cs` |
| `Close(id, ct)` | `POST /api/v1/semester/{id}/close` â€” permanently closes the semester. Admin+. One-way operation. | `API/Controllers/SemesterController.cs` |

---

### `CourseController` â€” `src/Tabsan.EduSphere.API/Controllers/CourseController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `GetAll(departmentId?, ct)` | `GET /api/v1/course` â€” returns the course catalogue, optionally filtered. | `API/Controllers/CourseController.cs` |
| `GetById(id, ct)` | `GET /api/v1/course/{id}` â€” returns a single course. | `API/Controllers/CourseController.cs` |
| `Create(request, ct)` | `POST /api/v1/course` â€” adds a course to the catalogue. Admin+. | `API/Controllers/CourseController.cs` |
| `UpdateTitle(id, request, ct)` | `PUT /api/v1/course/{id}/title` â€” updates the course title. Admin+. | `API/Controllers/CourseController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/v1/course/{id}` â€” soft-deactivates the course. SuperAdmin. | `API/Controllers/CourseController.cs` |
| `GetOfferings(semesterId, ct)` | `GET /api/v1/course/offerings?semesterId=` â€” returns offerings for a semester. | `API/Controllers/CourseController.cs` |
| `GetMyOfferings(ct)` | `GET /api/v1/course/offerings/my` â€” returns offerings assigned to the calling faculty, filtered to assigned departments. | `API/Controllers/CourseController.cs` |
| `CreateOffering(request, ct)` | `POST /api/v1/course/offerings` â€” creates a course offering. Admin+. | `API/Controllers/CourseController.cs` |
| `AssignFaculty(id, request, ct)` | `PUT /api/v1/course/offerings/{id}/faculty` â€” assigns faculty to an offering. Admin+. | `API/Controllers/CourseController.cs` |
| `GetUserId()` | Private helper â€” extracts the JWT sub claim as a GUID. | `API/Controllers/CourseController.cs` |

---

### `EnrollmentController` â€” `src/Tabsan.EduSphere.API/Controllers/EnrollmentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `Enroll(request, ct)` | `POST /api/v1/enrollment` â€” enrolls the calling student into a course offering. Student role. | `API/Controllers/EnrollmentController.cs` |
| `Drop(offeringId, ct)` | `DELETE /api/v1/enrollment/{offeringId}` â€” drops the student's active enrollment. Student role. | `API/Controllers/EnrollmentController.cs` |
| `MyCourses(ct)` | `GET /api/v1/enrollment/my-courses` â€” returns the student's full enrollment history. Student role. | `API/Controllers/EnrollmentController.cs` |
| `GetRoster(offeringId, ct)` | `GET /api/v1/enrollment/roster/{offeringId}` â€” returns active enrollments for an offering. Faculty/Admin+. | `API/Controllers/EnrollmentController.cs` |
| `GetUserId()` | Private helper â€” extracts the JWT sub claim as a GUID. | `API/Controllers/EnrollmentController.cs` |

---

### `StudentController` â€” `src/Tabsan.EduSphere.API/Controllers/StudentController.cs`

| Name | Purpose | Location |
|------|---------|----------|
| `SelfRegister(request, ct)` | `POST /api/v1/student/register` â€” public whitelist-gated self-registration. AllowAnonymous. | `API/Controllers/StudentController.cs` |
| `GetMyProfile(ct)` | `GET /api/v1/student/profile` â€” returns the calling student's academic profile. Student role. | `API/Controllers/StudentController.cs` |
| `GetAll(departmentId?, ct)` | `GET /api/v1/student` â€” returns all student profiles, optionally by department. Admin+. | `API/Controllers/StudentController.cs` |
| `Create(request, ct)` | `POST /api/v1/student` â€” Admin-managed student profile creation. Admin+. | `API/Controllers/StudentController.cs` |
| `AddWhitelistEntry(request, ct)` | `POST /api/v1/student/whitelist` â€” adds a single registration whitelist entry. Admin+. | `API/Controllers/StudentController.cs` |
| `BulkAddWhitelistEntries(requests, ct)` | `POST /api/v1/student/whitelist/bulk` â€” bulk-imports whitelist entries. Admin+. | `API/Controllers/StudentController.cs` |
| `GetUserId()` | Private helper â€” extracts the JWT sub claim as a GUID. | `API/Controllers/StudentController.cs` |

---

## Phase 3 — Assignments and Results

### Domain — Assignment

| Function Name | Purpose | Location |
|---|---|---|
| `Assignment(courseOfferingId, title, description, dueDate, maxMarks)` | Constructor — creates an unpublished assignment. | `Domain/Assignments/Assignment.cs` |
| `Publish()` | Marks the assignment as published (visible to students). Throws if already published. | `Domain/Assignments/Assignment.cs` |
| `Retract()` | Withdraws a published assignment. Throws if not published. | `Domain/Assignments/Assignment.cs` |
| `Update(title, description, dueDate, maxMarks)` | Updates editable fields. Throws if already published. | `Domain/Assignments/Assignment.cs` |

### Domain — AssignmentSubmission

| Function Name | Purpose | Location |
|---|---|---|
| `AssignmentSubmission(assignmentId, studentProfileId, fileUrl, textContent)` | Constructor — requires at least one of fileUrl/textContent. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Grade(marksAwarded, feedback, gradedByUserId)` | Records marks and feedback. Throws if submission was Rejected. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Reject()` | Marks submission as Rejected and clears awarded marks. | `Domain/Assignments/AssignmentSubmission.cs` |

### Domain — Result

| Function Name | Purpose | Location |
|---|---|---|
| `Result(studentProfileId, courseOfferingId, resultType, marksObtained, maxMarks)` | Constructor — validates marks range. | `Domain/Assignments/Result.cs` |
| `Publish(publishedByUserId)` | One-way publication. Throws if already published. | `Domain/Assignments/Result.cs` |
| `CorrectMarks(newMarksObtained, newMaxMarks)` | Admin-only correction of a published result. Validates range. | `Domain/Assignments/Result.cs` |

### Domain — TranscriptExportLog

| Function Name | Purpose | Location |
|---|---|---|
| `TranscriptExportLog(studentProfileId, requestedByUserId, format, documentUrl?, ipAddress?)` | Constructor — append-only, immutable after creation. | `Domain/Assignments/TranscriptExportLog.cs` |

### Infrastructure — AssignmentRepository

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

### Infrastructure — ResultRepository

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

### Application — AssignmentService

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
| `ToResponse(assignment, submissionCount)` | Private — maps Assignment to AssignmentResponse DTO. | `Application/Assignments/AssignmentService.cs` |
| `ToSubmissionResponse(submission, assignmentTitle)` | Private — maps AssignmentSubmission to SubmissionResponse DTO. | `Application/Assignments/AssignmentService.cs` |

### Application — ResultService

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
| `ToResponse(result)` | Private — maps Result to ResultResponse DTO including percentage. | `Application/Assignments/ResultService.cs` |

### API — AssignmentController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/assignment` — creates an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Update(id, request, ct)` | `PUT /api/assignment/{id}` — updates a draft assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Publish(id, ct)` | `POST /api/assignment/{id}/publish` — publishes an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Retract(id, ct)` | `POST /api/assignment/{id}/retract` — retracts a published assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Delete(id, ct)` | `DELETE /api/assignment/{id}` — soft-deletes when no submissions exist. Admin. | `API/Controllers/AssignmentController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/assignment/by-offering/{id}` — lists assignments for an offering. | `API/Controllers/AssignmentController.cs` |
| `GetById(id, ct)` | `GET /api/assignment/{id}` — returns a single assignment. | `API/Controllers/AssignmentController.cs` |
| `Submit(request, ct)` | `POST /api/assignment/submit` — student submission. Student. | `API/Controllers/AssignmentController.cs` |
| `GetMySubmissions(ct)` | `GET /api/assignment/my-submissions` — student's own submissions. Student. | `API/Controllers/AssignmentController.cs` |
| `GetSubmissions(id, ct)` | `GET /api/assignment/{id}/submissions` — all submissions for an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Grade(request, ct)` | `PUT /api/assignment/submissions/grade` — grades a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Reject(assignmentId, studentProfileId, ct)` | `POST /api/assignment/{id}/submissions/{studentId}/reject` — rejects a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentUserId()` | Private — extracts user ID from JWT NameIdentifier claim. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentStudentProfileId()` | Private — extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/AssignmentController.cs` |

### API — ResultController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/result` — creates a draft result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `BulkCreate(request, ct)` | `POST /api/result/bulk` — bulk-creates draft results for a class. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Publish(studentProfileId, courseOfferingId, resultType, ct)` | `POST /api/result/publish` — publishes a single result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `PublishAll(courseOfferingId, ct)` | `POST /api/result/publish-all` — publishes all drafts for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Correct(studentProfileId, courseOfferingId, resultType, request, ct)` | `PUT /api/result/correct` — Admin correction of a published result. Admin only. | `API/Controllers/ResultController.cs` |
| `GetMyResults(ct)` | `GET /api/result/my-results` — student's own published results. Student. | `API/Controllers/ResultController.cs` |
| `GetByStudent(studentProfileId, ct)` | `GET /api/result/by-student/{id}` — all results for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/result/by-offering/{id}` — all results for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetTranscript(studentProfileId, format, ct)` | `GET /api/result/transcript/{id}` — exports transcript, logs request. All roles. | `API/Controllers/ResultController.cs` |
| `GetTranscriptHistory(studentProfileId, ct)` | `GET /api/result/transcript/{id}/history` — export history for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetCurrentUserId()` | Private — extracts user ID from JWT NameIdentifier claim. | `API/Controllers/ResultController.cs` |
| `GetCurrentStudentProfileId()` | Private — extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/ResultController.cs` |

---

## Phase 4 — Notifications and Attendance

### Domain — Notification

| Function Name | Purpose | Location |
|---|---|---|
| `Notification(title, body, type, senderUserId)` | Constructor — user-authored notification. | `Domain/Notifications/Notification.cs` |
| `Notification(title, body, type)` | Constructor — system-generated notification (no human sender). | `Domain/Notifications/Notification.cs` |
| `Deactivate()` | Hides the notification from all inboxes. Does not delete it. | `Domain/Notifications/Notification.cs` |

### Domain — NotificationRecipient

| Function Name | Purpose | Location |
|---|---|---|
| `NotificationRecipient(notificationId, recipientUserId)` | Constructor — creates an unread delivery record for the user. | `Domain/Notifications/NotificationRecipient.cs` |
| `MarkRead()` | Marks the notification as read. Idempotent. | `Domain/Notifications/NotificationRecipient.cs` |

### Domain — AttendanceRecord

| Function Name | Purpose | Location |
|---|---|---|
| `AttendanceRecord(studentProfileId, courseOfferingId, date, status, markedByUserId, remarks?)` | Constructor — normalises date to UTC date only. | `Domain/Attendance/AttendanceRecord.cs` |
| `Correct(newStatus, correctedByUserId, remarks?)` | Corrects status and records the correcting user. | `Domain/Attendance/AttendanceRecord.cs` |

### Infrastructure — NotificationRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetByIdAsync(id, ct)` | Returns notification by ID or null. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `AddAsync(notification, ct)` | Queues a notification for insertion. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `Update(notification)` | Marks notification as modified. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetForUserAsync(userId, unreadOnly, skip, take, ct)` | Returns paged inbox for a user (active notifications only). | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetUnreadCountAsync(userId, ct)` | Returns unread active notification count for the user's badge. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetRecipientAsync(notificationId, userId, ct)` | Returns a specific delivery record or null. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `AddRecipientsAsync(recipients, ct)` | Bulk-inserts recipient rows for fan-out on dispatch. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `UpdateRecipient(recipient)` | Marks a recipient row as modified (read state). | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes (NotificationRepository). | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |

### Infrastructure — AttendanceRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetAsync(studentProfileId, courseOfferingId, date, ct)` | Returns the attendance record for the combination or null. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `ExistsAsync(studentProfileId, courseOfferingId, date, ct)` | Returns true when a record already exists. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetByOfferingAsync(courseOfferingId, from?, to?, ct)` | Returns records for an offering, optionally filtered by date range. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetByStudentAsync(studentProfileId, courseOfferingId?, ct)` | Returns records for a student, optionally scoped to one offering. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetAttendanceSummaryAsync(studentProfileId, courseOfferingId, ct)` | Returns (TotalSessions, AttendedSessions) for the student. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `GetBelowThresholdAsync(thresholdPercent, ct)` | Returns all student-offering pairs with attendance below threshold. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `AddAsync(record, ct)` | Queues a single attendance record for insertion. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `AddRangeAsync(records, ct)` | Queues multiple records for bulk insertion. | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `Update(record)` | Marks a record as modified (correction). | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |
| `SaveChangesAsync(ct)` | Commits pending changes (AttendanceRepository). | `Infrastructure/Repositories/NotificationAttendanceRepositories.cs` |

### Application — NotificationService

| Function Name | Purpose | Location |
|---|---|---|
| `SendAsync(request, senderUserId, ct)` | Creates notification and fans out to recipient list. Returns notification ID. | `Application/Notifications/NotificationService.cs` |
| `SendSystemAsync(title, body, type, recipientUserIds, ct)` | System-generated fan-out (no human sender). Returns notification ID. | `Application/Notifications/NotificationService.cs` |
| `DeactivateAsync(notificationId, ct)` | Deactivates notification from all inboxes. Returns false if not found. | `Application/Notifications/NotificationService.cs` |
| `GetInboxAsync(userId, unreadOnly, page, pageSize, ct)` | Returns paged inbox for a user with optional unread filter. | `Application/Notifications/NotificationService.cs` |
| `GetBadgeAsync(userId, ct)` | Returns unread count for the notification bell badge. | `Application/Notifications/NotificationService.cs` |
| `MarkReadAsync(notificationId, userId, ct)` | Marks a specific notification as read. Idempotent. | `Application/Notifications/NotificationService.cs` |
| `MarkAllReadAsync(userId, ct)` | Marks all unread notifications as read for the user. | `Application/Notifications/NotificationService.cs` |
| `ToResponse(recipient)` | Private — maps NotificationRecipient (with navigation) to DTO. | `Application/Notifications/NotificationService.cs` |

### Application — AttendanceService

| Function Name | Purpose | Location |
|---|---|---|
| `MarkAsync(request, markedByUserId, ct)` | Records single attendance. Returns false on duplicate. | `Application/Attendance/AttendanceService.cs` |
| `BulkMarkAsync(request, markedByUserId, ct)` | Bulk-marks a class; skips duplicates. Returns inserted count. | `Application/Attendance/AttendanceService.cs` |
| `CorrectAsync(request, correctedByUserId, ct)` | Corrects an existing record. Returns false if not found. | `Application/Attendance/AttendanceService.cs` |
| `GetByOfferingAsync(courseOfferingId, from?, to?, ct)` | Returns records for an offering with optional date filter. | `Application/Attendance/AttendanceService.cs` |
| `GetByStudentAsync(studentProfileId, courseOfferingId?, ct)` | Returns records for a student, optionally scoped to one offering. | `Application/Attendance/AttendanceService.cs` |
| `GetSummaryAsync(studentProfileId, courseOfferingId, ct)` | Returns attendance percentage summary for a student in an offering. | `Application/Attendance/AttendanceService.cs` |
| `GetBelowThresholdAsync(thresholdPercent, ct)` | Returns all student-offering pairs below the threshold. | `Application/Attendance/AttendanceService.cs` |
| `ToResponse(record)` | Private — maps AttendanceRecord to AttendanceResponse DTO. | `Application/Attendance/AttendanceService.cs` |

### API — NotificationController

| Function Name | Purpose | Location |
|---|---|---|
| `Send(request, ct)` | `POST /api/notification` — dispatches notification to a user list. Admin/Faculty. | `API/Controllers/NotificationController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/notification/{id}` — deactivates a notification. Admin. | `API/Controllers/NotificationController.cs` |
| `GetInbox(unreadOnly, page, pageSize, ct)` | `GET /api/notification/inbox` — paged inbox for current user. | `API/Controllers/NotificationController.cs` |
| `GetBadge(ct)` | `GET /api/notification/badge` — unread count for the bell icon. | `API/Controllers/NotificationController.cs` |
| `MarkRead(id, ct)` | `POST /api/notification/{id}/read` — marks one notification read. | `API/Controllers/NotificationController.cs` |
| `MarkAllRead(ct)` | `POST /api/notification/read-all` — marks all unread as read. | `API/Controllers/NotificationController.cs` |
| `GetCurrentUserId()` | Private — extracts user ID from JWT NameIdentifier claim. | `API/Controllers/NotificationController.cs` |

### API — AttendanceController

| Function Name | Purpose | Location |
|---|---|---|
| `Mark(request, ct)` | `POST /api/attendance` — marks attendance for one student. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `BulkMark(request, ct)` | `POST /api/attendance/bulk` — bulk-marks a full class. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `Correct(request, ct)` | `PUT /api/attendance/correct` — corrects an existing record. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetByOffering(courseOfferingId, from, to, ct)` | `GET /api/attendance/by-offering/{id}` — records for an offering. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetByStudent(studentProfileId, courseOfferingId, ct)` | `GET /api/attendance/by-student/{id}` — records for a student. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetMyAttendance(courseOfferingId, ct)` | `GET /api/attendance/my-attendance` — student's own records. Student. | `API/Controllers/AttendanceController.cs` |
| `GetSummary(studentProfileId, courseOfferingId, ct)` | `GET /api/attendance/summary/{studentId}/{offeringId}` — percentage summary. All roles. | `API/Controllers/AttendanceController.cs` |
| `GetBelowThreshold(threshold, ct)` | `GET /api/attendance/below-threshold` — students below threshold. Admin. | `API/Controllers/AttendanceController.cs` |
| `GetCurrentUserId()` | Private — extracts user ID from JWT NameIdentifier claim. | `API/Controllers/AttendanceController.cs` |
| `GetCurrentStudentProfileId()` | Private — extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/AttendanceController.cs` |

### Background Job — AttendanceAlertJob

| Function Name | Purpose | Location |
|---|---|---|
| `ExecuteAsync(stoppingToken)` | Main hosted service loop — waits 60 s startup delay then runs on configured interval. | `BackgroundJobs/AttendanceAlertJob.cs` |
| `RunCheckAsync(ct)` | Resolves scoped services, finds below-threshold students, dispatches alert notifications. | `BackgroundJobs/AttendanceAlertJob.cs` |

---

## Phase 5 — Quizzes and FYP (Sprints 10–11)

### Domain — Quiz

| Function Name | Purpose | Location |
|---|---|---|
| Quiz(courseOfferingId, title, createdByUserId, instructions, timeLimitMinutes, maxAttempts, availableFrom, availableUntil) | Creates a new quiz in unpublished state. | Domain/Quizzes/Quiz.cs |
| Publish() | Marks the quiz as published so students can view and attempt it. | Domain/Quizzes/Quiz.cs |
| Unpublish() | Reverts the quiz to draft/unpublished state. | Domain/Quizzes/Quiz.cs |
| Deactivate() | Soft-deletes the quiz by setting IsActive=false. | Domain/Quizzes/Quiz.cs |
| Update(title, instructions, timeLimitMinutes, maxAttempts, availableFrom, availableUntil) | Updates editable quiz metadata. | Domain/Quizzes/Quiz.cs |

### Domain — QuizQuestion

| Function Name | Purpose | Location |
|---|---|---|
| QuizQuestion(quizId, text, type, marks, orderIndex) | Creates a new question within a quiz. | Domain/Quizzes/Quiz.cs |
| Update(text, marks, orderIndex) | Updates the question text, marks, and display order. | Domain/Quizzes/Quiz.cs |

### Domain — QuizOption

| Function Name | Purpose | Location |
|---|---|---|
| QuizOption(quizQuestionId, text, isCorrect, orderIndex) | Creates an answer option for a MCQ or TrueFalse question. | Domain/Quizzes/Quiz.cs |

### Domain — QuizAttempt

| Function Name | Purpose | Location |
|---|---|---|
| QuizAttempt(quizId, studentProfileId) | Starts a new attempt, setting status=InProgress and StartedAt=UtcNow. | Domain/Quizzes/QuizAttempt.cs |
| Submit() | Marks the attempt as Submitted and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| TimeOut() | Marks the attempt as TimedOut and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| Abandon() | Marks the attempt as Abandoned and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| RecordScore(score) | Sets the computed TotalScore on the attempt. | Domain/Quizzes/QuizAttempt.cs |

### Domain — QuizAnswer

| Function Name | Purpose | Location |
|---|---|---|
| QuizAnswer(quizAttemptId, quizQuestionId, selectedOptionId) | Records an MCQ or TrueFalse answer by option ID. | Domain/Quizzes/QuizAttempt.cs |
| QuizAnswer(quizAttemptId, quizQuestionId, textResponse) | Records a ShortAnswer response as free text. | Domain/Quizzes/QuizAttempt.cs |
| AwardMarks(marks) | Sets the marks awarded for manually graded short answers. | Domain/Quizzes/QuizAttempt.cs |

### Domain — FypProject

| Function Name | Purpose | Location |
|---|---|---|
| FypProject(studentProfileId, departmentId, title, description) | Creates a new FYP proposal in Proposed state. | Domain/Fyp/FypProject.cs |
| Approve(remarks) | Transitions the project to Approved with optional coordinator remarks. | Domain/Fyp/FypProject.cs |
| Reject(remarks) | Transitions the project to Rejected with mandatory remarks. | Domain/Fyp/FypProject.cs |
| AssignSupervisor(supervisorUserId) | Records the supervising faculty member and sets status to InProgress. | Domain/Fyp/FypProject.cs |
| Complete() | Marks the project as Completed. | Domain/Fyp/FypProject.cs |
| Update(title, description) | Updates the project title and description. | Domain/Fyp/FypProject.cs |

### Domain — FypPanelMember

| Function Name | Purpose | Location |
|---|---|---|
| FypPanelMember(fypProjectId, userId, role) | Adds a faculty member to the project panel with a specified role. | Domain/Fyp/FypProject.cs |

### Domain — FypMeeting

| Function Name | Purpose | Location |
|---|---|---|
| FypMeeting(fypProjectId, scheduledAt, venue, organiserUserId, agenda) | Schedules a new FYP meeting in Scheduled state. | Domain/Fyp/FypProject.cs |
| Complete(minutes) | Marks the meeting as Completed and records optional minutes. | Domain/Fyp/FypProject.cs |
| Cancel() | Cancels a scheduled meeting. | Domain/Fyp/FypProject.cs |
| Reschedule(scheduledAt, venue, agenda) | Updates the meeting time, venue, and agenda and resets status to Scheduled. | Domain/Fyp/FypProject.cs |

### Infrastructure — QuizRepository

| Function Name | Purpose | Location |
|---|---|---|
| GetByIdAsync(id, ct) | Fetches a quiz by primary key. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetWithQuestionsAsync(id, ct) | Fetches a quiz with all questions and their options included. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetByOfferingAsync(courseOfferingId, ct) | Returns all published quizzes for a course offering. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddAsync(quiz, ct) | Persists a new quiz entity. | Infrastructure/Repositories/QuizFypRepositories.cs |
| Update(quiz) | Marks a quiz as modified in the EF change tracker. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetQuestionByIdAsync(questionId, ct) | Fetches a single quiz question by ID. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddQuestionAsync(question, ct) | Persists a new quiz question. | Infrastructure/Repositories/QuizFypRepositories.cs |
| UpdateQuestion(question) | Marks a question as modified. | Infrastructure/Repositories/QuizFypRepositories.cs |
| RemoveQuestion(question) | Removes a question from the context. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddOptionsAsync(options, ct) | Bulk-adds a collection of answer options. | Infrastructure/Repositories/QuizFypRepositories.cs |
| RemoveOptions(options) | Removes a collection of options from the context. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetAttemptByIdAsync(attemptId, ct) | Fetches an attempt with its answers included. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetAttemptsAsync(quizId, studentProfileId, ct) | Returns all attempts for a student on a quiz. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetAttemptCountAsync(quizId, studentProfileId, ct) | Returns the count of completed or timed-out attempts for cap checking. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetInProgressAttemptAsync(quizId, studentProfileId, ct) | Returns any in-progress attempt for a student on a quiz. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddAttemptAsync(attempt, ct) | Persists a new quiz attempt. | Infrastructure/Repositories/QuizFypRepositories.cs |
| UpdateAttempt(attempt) | Marks an attempt as modified. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddAnswersAsync(answers, ct) | Bulk-adds a collection of quiz answers. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetAnswerByIdAsync(answerId, ct) | Fetches a single answer by ID for manual grading. | Infrastructure/Repositories/QuizFypRepositories.cs |
| UpdateAnswer(answer) | Marks an answer as modified. | Infrastructure/Repositories/QuizFypRepositories.cs |
| SaveChangesAsync(ct) | Commits all pending changes to the database. | Infrastructure/Repositories/QuizFypRepositories.cs |

### Infrastructure — FypRepository

| Function Name | Purpose | Location |
|---|---|---|
| GetByIdAsync(id, ct) | Fetches an FYP project by primary key. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetWithDetailsAsync(id, ct) | Fetches a project with panel members and meetings included. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetByStudentAsync(studentProfileId, ct) | Returns all projects for a student. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetByDepartmentAsync(departmentId, status, ct) | Returns department projects optionally filtered by status. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetBySupervisorAsync(supervisorUserId, ct) | Returns all projects supervised by a given faculty member. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddAsync(project, ct) | Persists a new FYP project. | Infrastructure/Repositories/QuizFypRepositories.cs |
| Update(project) | Marks a project as modified. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetPanelMembersAsync(projectId, ct) | Returns all panel members for a project. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetPanelMemberAsync(projectId, userId, ct) | Returns a specific panel member by project and user ID. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddPanelMemberAsync(member, ct) | Persists a new panel member. | Infrastructure/Repositories/QuizFypRepositories.cs |
| RemovePanelMember(member) | Removes a panel member from the context. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetMeetingByIdAsync(meetingId, ct) | Fetches a meeting by primary key. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetMeetingsByProjectAsync(projectId, ct) | Returns all meetings for a project ordered by scheduled date. | Infrastructure/Repositories/QuizFypRepositories.cs |
| GetUpcomingMeetingsAsync(supervisorUserId, ct) | Returns upcoming scheduled meetings organised by a supervisor. | Infrastructure/Repositories/QuizFypRepositories.cs |
| AddMeetingAsync(meeting, ct) | Persists a new meeting. | Infrastructure/Repositories/QuizFypRepositories.cs |
| UpdateMeeting(meeting) | Marks a meeting as modified. | Infrastructure/Repositories/QuizFypRepositories.cs |
| SaveChangesAsync(ct) | Commits all pending changes to the database. | Infrastructure/Repositories/QuizFypRepositories.cs |

### Application — QuizService

| Function Name | Purpose | Location |
|---|---|---|
| CreateAsync(request, facultyUserId, ct) | Creates a new quiz and persists it. | Application/Quizzes/QuizService.cs |
| UpdateAsync(quizId, request, ct) | Updates quiz metadata; returns false if not found. | Application/Quizzes/QuizService.cs |
| PublishAsync(quizId, ct) | Publishes a quiz so students can access it. | Application/Quizzes/QuizService.cs |
| UnpublishAsync(quizId, ct) | Reverts a quiz to draft state. | Application/Quizzes/QuizService.cs |
| DeactivateAsync(quizId, ct) | Soft-deletes a quiz. | Application/Quizzes/QuizService.cs |
| AddQuestionAsync(request, ct) | Adds a question with its options to an existing quiz. | Application/Quizzes/QuizService.cs |
| UpdateQuestionAsync(questionId, request, ct) | Updates question text, marks, and order; replaces options if provided. | Application/Quizzes/QuizService.cs |
| RemoveQuestionAsync(questionId, ct) | Removes a question and all its options. | Application/Quizzes/QuizService.cs |
| GetByOfferingAsync(courseOfferingId, ct) | Returns summary list of published quizzes for a course offering. | Application/Quizzes/QuizService.cs |
| GetDetailAsync(quizId, ct) | Returns full quiz detail including questions and options. | Application/Quizzes/QuizService.cs |
| StartAttemptAsync(quizId, studentProfileId, ct) | Validates availability, attempt cap, and in-progress check, then creates a new attempt. | Application/Quizzes/QuizService.cs |
| SubmitAttemptAsync(request, studentProfileId, ct) | Records answers, auto-grades MCQ/TrueFalse, computes score, submits attempt. | Application/Quizzes/QuizService.cs |
| GetStudentAttemptsAsync(quizId, studentProfileId, ct) | Returns all attempts for a student on a quiz. | Application/Quizzes/QuizService.cs |
| GetAttemptDetailAsync(attemptId, ct) | Returns detailed attempt data including answer responses. | Application/Quizzes/QuizService.cs |
| GradeAnswerAsync(request, ct) | Awards marks to a short-answer response and updates attempt total score. | Application/Quizzes/QuizService.cs |
| ToSummary(quiz) | Private — maps Quiz to QuizSummaryResponse. | Application/Quizzes/QuizService.cs |
| ToDetail(quiz) | Private — maps Quiz with questions to QuizDetailResponse. | Application/Quizzes/QuizService.cs |
| ToQuestionResponse(question, hideAnswers) | Private — maps a QuizQuestion to QuestionResponse, optionally hiding correct answers. | Application/Quizzes/QuizService.cs |
| ToAttemptResponse(attempt) | Private — maps QuizAttempt to AttemptResponse. | Application/Quizzes/QuizService.cs |
| ToAttemptDetail(attempt) | Private — maps QuizAttempt with answers to AttemptDetailResponse. | Application/Quizzes/QuizService.cs |

### Application — FypService

| Function Name | Purpose | Location |
|---|---|---|
| ProposeAsync(request, studentProfileId, ct) | Creates a new FYP project proposal and returns its ID. | Application/Fyp/FypService.cs |
| UpdateAsync(projectId, request, ct) | Updates project title and description. | Application/Fyp/FypService.cs |
| ApproveAsync(projectId, request, ct) | Approves a proposal with optional coordinator remarks. | Application/Fyp/FypService.cs |
| RejectAsync(projectId, request, ct) | Rejects a proposal with mandatory remarks. | Application/Fyp/FypService.cs |
| AssignSupervisorAsync(projectId, request, ct) | Assigns a supervisor to the project. | Application/Fyp/FypService.cs |
| CompleteAsync(projectId, ct) | Marks a project as completed. | Application/Fyp/FypService.cs |
| GetByStudentAsync(studentProfileId, ct) | Returns all projects for a student as summary DTOs. | Application/Fyp/FypService.cs |
| GetByDepartmentAsync(departmentId, statusString, ct) | Returns department projects filtered by optional status string. | Application/Fyp/FypService.cs |
| GetBySupervisorAsync(supervisorUserId, ct) | Returns all projects supervised by a faculty user. | Application/Fyp/FypService.cs |
| GetDetailAsync(projectId, ct) | Returns full project detail with panel and meetings. | Application/Fyp/FypService.cs |
| AddPanelMemberAsync(projectId, request, ct) | Adds a faculty member to the project panel. | Application/Fyp/FypService.cs |
| RemovePanelMemberAsync(projectId, userId, ct) | Removes a panel member by user ID. | Application/Fyp/FypService.cs |
| ScheduleMeetingAsync(request, organiserUserId, ct) | Creates a new scheduled meeting for a project. | Application/Fyp/FypService.cs |
| RescheduleMeetingAsync(meetingId, request, ct) | Reschedules a meeting to a new time, venue, and agenda. | Application/Fyp/FypService.cs |
| CompleteMeetingAsync(meetingId, request, ct) | Marks a meeting as completed with optional minutes. | Application/Fyp/FypService.cs |
| CancelMeetingAsync(meetingId, ct) | Cancels a scheduled meeting. | Application/Fyp/FypService.cs |
| GetMeetingsByProjectAsync(projectId, ct) | Returns all meetings for a project as response DTOs. | Application/Fyp/FypService.cs |
| GetUpcomingMeetingsAsync(supervisorUserId, ct) | Returns upcoming meetings organised by the supervisor. | Application/Fyp/FypService.cs |
| ToSummary(project) | Private — maps FypProject to FypProjectSummaryResponse. | Application/Fyp/FypService.cs |
| ToDetail(project) | Private — maps FypProject with panel/meetings to FypProjectDetailResponse. | Application/Fyp/FypService.cs |
| ToMeetingResponse(meeting) | Private — maps FypMeeting to MeetingResponse. | Application/Fyp/FypService.cs |

### API — QuizController

| Function Name | Purpose | Location |
|---|---|---|
| Create(request, ct) | POST /api/quiz — Creates a quiz (Faculty). | API/Controllers/QuizController.cs |
| Update(id, request, ct) | PUT /api/quiz/{id} — Updates quiz metadata (Faculty). | API/Controllers/QuizController.cs |
| Publish(id, ct) | POST /api/quiz/{id}/publish — Publishes a quiz (Faculty). | API/Controllers/QuizController.cs |
| Unpublish(id, ct) | POST /api/quiz/{id}/unpublish — Unpublishes a quiz (Faculty). | API/Controllers/QuizController.cs |
| Deactivate(id, ct) | DELETE /api/quiz/{id} — Soft-deletes a quiz (Admin). | API/Controllers/QuizController.cs |
| AddQuestion(request, ct) | POST /api/quiz/question — Adds a question to a quiz (Faculty). | API/Controllers/QuizController.cs |
| UpdateQuestion(questionId, request, ct) | PUT /api/quiz/question/{questionId} — Updates a question (Faculty). | API/Controllers/QuizController.cs |
| RemoveQuestion(questionId, ct) | DELETE /api/quiz/question/{questionId} — Removes a question (Faculty). | API/Controllers/QuizController.cs |
| GetByOffering(courseOfferingId, ct) | GET /api/quiz/by-offering/{courseOfferingId} — Lists quizzes for an offering (All). | API/Controllers/QuizController.cs |
| GetDetail(id, ct) | GET /api/quiz/{id} — Returns full quiz detail (All). | API/Controllers/QuizController.cs |
| StartAttempt(id, ct) | POST /api/quiz/{id}/start — Starts a student attempt; 409 if cap reached (Student). | API/Controllers/QuizController.cs |
| SubmitAttempt(request, ct) | POST /api/quiz/attempt/submit — Submits answers and grades MCQ/TrueFalse (Student). | API/Controllers/QuizController.cs |
| GetMyAttempts(id, ct) | GET /api/quiz/{id}/my-attempts — Returns student's own attempts (Student). | API/Controllers/QuizController.cs |
| GetAttemptDetail(attemptId, ct) | GET /api/quiz/attempt/{attemptId} — Returns attempt detail with answers (All). | API/Controllers/QuizController.cs |
| GradeAnswer(request, ct) | POST /api/quiz/attempt/grade-answer — Manually grades a short-answer response (Faculty). | API/Controllers/QuizController.cs |
| GetCurrentUserId() | Private — Extracts authenticated user ID from JWT NameIdentifier claim. | API/Controllers/QuizController.cs |
| GetStudentProfileId() | Private — Extracts student profile ID from the studentProfileId JWT claim. | API/Controllers/QuizController.cs |

### API — FypController

| Function Name | Purpose | Location |
|---|---|---|
| Propose(request, ct) | POST /api/fyp — Submits an FYP proposal (Student). | API/Controllers/FypController.cs |
| Update(id, request, ct) | PUT /api/fyp/{id} — Updates project title/description (Student). | API/Controllers/FypController.cs |
| Approve(id, request, ct) | POST /api/fyp/{id}/approve — Approves a proposal (Admin). | API/Controllers/FypController.cs |
| Reject(id, request, ct) | POST /api/fyp/{id}/reject — Rejects a proposal with remarks (Admin). | API/Controllers/FypController.cs |
| AssignSupervisor(id, request, ct) | POST /api/fyp/{id}/assign-supervisor — Assigns a supervisor (Admin). | API/Controllers/FypController.cs |
| Complete(id, ct) | POST /api/fyp/{id}/complete — Marks a project as completed (Admin). | API/Controllers/FypController.cs |
| GetMyProjects(ct) | GET /api/fyp/my-projects — Returns current student's projects (Student). | API/Controllers/FypController.cs |
| GetByDepartment(departmentId, status, ct) | GET /api/fyp/by-department/{departmentId} — Returns department projects (Faculty). | API/Controllers/FypController.cs |
| GetMySupervised(ct) | GET /api/fyp/my-supervised — Returns projects supervised by current user (Faculty). | API/Controllers/FypController.cs |
| GetDetail(id, ct) | GET /api/fyp/{id} — Returns full project detail (All). | API/Controllers/FypController.cs |
| AddPanelMember(id, request, ct) | POST /api/fyp/{id}/panel — Adds a panel member (Admin). | API/Controllers/FypController.cs |
| RemovePanelMember(id, userId, ct) | DELETE /api/fyp/{id}/panel/{userId} — Removes a panel member (Admin). | API/Controllers/FypController.cs |
| ScheduleMeeting(request, ct) | POST /api/fyp/meeting — Schedules a new FYP meeting (Faculty). | API/Controllers/FypController.cs |
| RescheduleMeeting(meetingId, request, ct) | PUT /api/fyp/meeting/{meetingId} — Reschedules a meeting (Faculty). | API/Controllers/FypController.cs |
| CompleteMeeting(meetingId, request, ct) | POST /api/fyp/meeting/{meetingId}/complete — Completes a meeting (Faculty). | API/Controllers/FypController.cs |
| CancelMeeting(meetingId, ct) | POST /api/fyp/meeting/{meetingId}/cancel — Cancels a meeting (Faculty). | API/Controllers/FypController.cs |
| GetMeetings(id, ct) | GET /api/fyp/{id}/meetings — Returns all meetings for a project (All). | API/Controllers/FypController.cs |
| GetUpcomingMeetings(ct) | GET /api/fyp/meeting/upcoming — Returns upcoming meetings for current supervisor (Faculty). | API/Controllers/FypController.cs |
| GetCurrentUserId() | Private — Extracts authenticated user ID from JWT NameIdentifier claim. | API/Controllers/FypController.cs |
| GetStudentProfileId() | Private — Extracts student profile ID from the studentProfileId JWT claim. | API/Controllers/FypController.cs |

---

## Phase 5 — Quizzes and FYP

### Domain — Quiz

| Function Name | Purpose | Location |
|---|---|---|
| `Quiz(courseOfferingId, title, createdByUserId, ...)` | Creates a new quiz in unpublished state. | `Domain/Quizzes/Quiz.cs` |
| `Publish()` | Marks the quiz as published and available to students. | `Domain/Quizzes/Quiz.cs` |
| `Unpublish()` | Reverts the quiz to draft state. | `Domain/Quizzes/Quiz.cs` |
| `Deactivate()` | Soft-deletes the quiz (IsActive=false). | `Domain/Quizzes/Quiz.cs` |
| `Update(title, instructions, timeLimitMinutes, maxAttempts, availableFrom, availableUntil)` | Updates mutable quiz metadata. | `Domain/Quizzes/Quiz.cs` |
| `QuizQuestion(quizId, text, type, marks, orderIndex)` | Creates a new question attached to a quiz. | `Domain/Quizzes/Quiz.cs` |
| `QuizQuestion.Update(text, marks, orderIndex)` | Updates question text and grading details. | `Domain/Quizzes/Quiz.cs` |
| `QuizOption(quizQuestionId, text, isCorrect, orderIndex)` | Creates an answer option for a question. | `Domain/Quizzes/Quiz.cs` |

### Domain — QuizAttempt

| Function Name | Purpose | Location |
|---|---|---|
| `QuizAttempt(quizId, studentProfileId)` | Starts a new in-progress attempt with StartedAt=UtcNow. | `Domain/Quizzes/QuizAttempt.cs` |
| `Submit()` | Finalises the attempt and records FinishedAt. | `Domain/Quizzes/QuizAttempt.cs` |
| `TimeOut()` | Marks the attempt as timed-out and records FinishedAt. | `Domain/Quizzes/QuizAttempt.cs` |
| `Abandon()` | Marks the attempt as abandoned and records FinishedAt. | `Domain/Quizzes/QuizAttempt.cs` |
| `RecordScore(score)` | Stores the computed total score after grading. | `Domain/Quizzes/QuizAttempt.cs` |
| `QuizAnswer(quizAttemptId, quizQuestionId, selectedOptionId)` | Records an MCQ/TrueFalse answer with chosen option. | `Domain/Quizzes/QuizAttempt.cs` |
| `QuizAnswer(quizAttemptId, quizQuestionId, textResponse)` | Records a short-answer textual response. | `Domain/Quizzes/QuizAttempt.cs` |
| `AwardMarks(marks)` | Stores instructor-awarded marks for a short-answer response. | `Domain/Quizzes/QuizAttempt.cs` |

### Domain — FypProject

| Function Name | Purpose | Location |
|---|---|---|
| `FypProject(studentProfileId, departmentId, title, description)` | Proposes a new FYP project in Proposed state. | `Domain/Fyp/FypProject.cs` |
| `Approve(remarks)` | Moves project to Approved state and stores coordinator remarks. | `Domain/Fyp/FypProject.cs` |
| `Reject(remarks)` | Moves project to Rejected state with mandatory remarks. | `Domain/Fyp/FypProject.cs` |
| `AssignSupervisor(supervisorUserId)` | Links a supervisor and transitions project to InProgress. | `Domain/Fyp/FypProject.cs` |
| `Complete()` | Marks the project as Completed. | `Domain/Fyp/FypProject.cs` |
| `FypProject.Update(title, description)` | Updates project title and description. | `Domain/Fyp/FypProject.cs` |
| `FypPanelMember(fypProjectId, userId, role)` | Assigns a user to the evaluation panel with a given role. | `Domain/Fyp/FypProject.cs` |
| `FypMeeting(fypProjectId, scheduledAt, venue, organiserUserId, agenda)` | Schedules a new meeting in Scheduled state. | `Domain/Fyp/FypProject.cs` |
| `FypMeeting.Complete(minutes)` | Marks meeting as completed and records minutes. | `Domain/Fyp/FypProject.cs` |
| `FypMeeting.Cancel()` | Cancels a scheduled meeting. | `Domain/Fyp/FypProject.cs` |
| `FypMeeting.Reschedule(scheduledAt, venue, agenda)` | Updates meeting time, venue and agenda. | `Domain/Fyp/FypProject.cs` |

### Infrastructure — QuizRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetByIdAsync(id, ct)` | Fetches a quiz by primary key. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetWithQuestionsAsync(id, ct)` | Fetches quiz with questions and options eager-loaded. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetByOfferingAsync(courseOfferingId, ct)` | Lists all quizzes for a course offering. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddAsync(quiz, ct)` | Inserts a new quiz. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `Update(quiz)` | Marks quiz entity as modified. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetQuestionByIdAsync(questionId, ct)` | Fetches a question by primary key. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddQuestionAsync(question, ct)` | Inserts a new question. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `UpdateQuestion(question)` | Marks question as modified. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `RemoveQuestion(question)` | Removes a question from the context. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddOptionsAsync(options, ct)` | Bulk inserts a collection of options. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `RemoveOptions(options)` | Bulk removes a collection of options. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetAttemptByIdAsync(attemptId, ct)` | Fetches an attempt with its answers. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetAttemptsAsync(quizId, studentProfileId, ct)` | Lists all attempts for a student on a quiz. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetAttemptCountAsync(quizId, studentProfileId, ct)` | Counts completed/timed-out attempts for cap validation. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetInProgressAttemptAsync(quizId, studentProfileId, ct)` | Returns an active (InProgress) attempt if one exists. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddAttemptAsync(attempt, ct)` | Inserts a new attempt. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `UpdateAttempt(attempt)` | Marks attempt as modified. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddAnswersAsync(answers, ct)` | Bulk inserts submitted answers. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetAnswerByIdAsync(answerId, ct)` | Fetches a single answer for manual grading. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `UpdateAnswer(answer)` | Marks answer as modified after manual grading. | `Infrastructure/Repositories/QuizFypRepositories.cs` |

### Infrastructure — FypRepository

| Function Name | Purpose | Location |
|---|---|---|
| `GetByIdAsync(id, ct)` | Fetches a project by primary key. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetWithDetailsAsync(id, ct)` | Fetches project with panel members and meetings eager-loaded. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetByStudentAsync(studentProfileId, ct)` | Lists all projects for a student. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetByDepartmentAsync(departmentId, status, ct)` | Lists department projects, optionally filtered by status. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetBySupervisorAsync(supervisorUserId, ct)` | Lists projects supervised by a faculty user. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddAsync(project, ct)` | Inserts a new FYP project. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `Update(project)` | Marks project as modified. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetPanelMembersAsync(projectId, ct)` | Lists all panel members for a project. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetPanelMemberAsync(projectId, userId, ct)` | Fetches a specific panel member record. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddPanelMemberAsync(member, ct)` | Inserts a panel member assignment. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `RemovePanelMember(member)` | Removes a panel member from the context. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetMeetingByIdAsync(meetingId, ct)` | Fetches a meeting by primary key. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetMeetingsByProjectAsync(projectId, ct)` | Lists all meetings for a project ordered by scheduled date. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `GetUpcomingMeetingsAsync(supervisorUserId, ct)` | Returns future scheduled meetings for a supervisor. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `AddMeetingAsync(meeting, ct)` | Inserts a new meeting. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `UpdateMeeting(meeting)` | Marks meeting as modified. | `Infrastructure/Repositories/QuizFypRepositories.cs` |
| `SaveChangesAsync(ct)` | Flushes all pending changes to the database. | `Infrastructure/Repositories/QuizFypRepositories.cs` |

### Application — QuizService

| Function Name | Purpose | Location |
|---|---|---|
| `CreateAsync(request, createdByUserId, ct)` | Creates a new quiz entity and persists it. | `Application/Quizzes/QuizService.cs` |
| `UpdateAsync(quizId, request, ct)` | Applies metadata updates to an existing quiz. | `Application/Quizzes/QuizService.cs` |
| `PublishAsync(quizId, ct)` | Publishes a quiz so students can see it. | `Application/Quizzes/QuizService.cs` |
| `UnpublishAsync(quizId, ct)` | Reverts a quiz to draft. | `Application/Quizzes/QuizService.cs` |
| `DeactivateAsync(quizId, ct)` | Soft-deletes a quiz. | `Application/Quizzes/QuizService.cs` |
| `AddQuestionAsync(request, ct)` | Adds a question with options to a quiz. | `Application/Quizzes/QuizService.cs` |
| `UpdateQuestionAsync(questionId, request, ct)` | Updates question text and grading. | `Application/Quizzes/QuizService.cs` |
| `RemoveQuestionAsync(questionId, ct)` | Removes a question and its options. | `Application/Quizzes/QuizService.cs` |
| `GetByOfferingAsync(courseOfferingId, ct)` | Returns summary list of quizzes for a course offering. | `Application/Quizzes/QuizService.cs` |
| `GetDetailAsync(quizId, ct)` | Returns full quiz detail with questions and options. | `Application/Quizzes/QuizService.cs` |
| `StartAttemptAsync(quizId, studentProfileId, ct)` | Validates and starts a new quiz attempt. | `Application/Quizzes/QuizService.cs` |
| `SubmitAttemptAsync(request, ct)` | Records answers, auto-grades MCQ/TrueFalse, computes score. | `Application/Quizzes/QuizService.cs` |
| `GetStudentAttemptsAsync(quizId, studentProfileId, ct)` | Lists a student's attempts on a quiz. | `Application/Quizzes/QuizService.cs` |
| `GetAttemptDetailAsync(attemptId, ct)` | Returns full attempt detail with answers. | `Application/Quizzes/QuizService.cs` |
| `GradeAnswerAsync(request, ct)` | Awards marks to a short-answer response. | `Application/Quizzes/QuizService.cs` |
| `ToSummary(quiz)` | Maps Quiz entity to QuizSummaryResponse DTO. | `Application/Quizzes/QuizService.cs` |
| `ToDetail(quiz)` | Maps Quiz with questions to QuizDetailResponse DTO. | `Application/Quizzes/QuizService.cs` |
| `ToQuestionResponse(q, revealAnswers)` | Maps QuizQuestion to QuestionResponse DTO, optionally revealing correct options. | `Application/Quizzes/QuizService.cs` |
| `ToAttemptResponse(attempt)` | Maps QuizAttempt to AttemptResponse DTO. | `Application/Quizzes/QuizService.cs` |
| `ToAttemptDetail(attempt)` | Maps QuizAttempt with answers to AttemptDetailResponse DTO. | `Application/Quizzes/QuizService.cs` |

### Application — FypService

| Function Name | Purpose | Location |
|---|---|---|
| `ProposeAsync(request, studentProfileId, ct)` | Creates a new FYP project proposal. | `Application/Fyp/FypService.cs` |
| `UpdateAsync(projectId, request, ct)` | Updates project title and description. | `Application/Fyp/FypService.cs` |
| `ApproveAsync(projectId, request, ct)` | Approves a project proposal. | `Application/Fyp/FypService.cs` |
| `RejectAsync(projectId, request, ct)` | Rejects a project proposal with remarks. | `Application/Fyp/FypService.cs` |
| `AssignSupervisorAsync(projectId, request, ct)` | Assigns a faculty supervisor to a project. | `Application/Fyp/FypService.cs` |
| `CompleteAsync(projectId, ct)` | Marks a project as completed. | `Application/Fyp/FypService.cs` |
| `GetByStudentAsync(studentProfileId, ct)` | Returns all FYP projects for a student. | `Application/Fyp/FypService.cs` |
| `GetByDepartmentAsync(departmentId, status, ct)` | Returns department projects filtered by optional status. | `Application/Fyp/FypService.cs` |
| `GetBySupervisorAsync(supervisorUserId, ct)` | Returns projects assigned to a supervisor. | `Application/Fyp/FypService.cs` |
| `GetDetailAsync(projectId, ct)` | Returns full project detail including panel and meetings. | `Application/Fyp/FypService.cs` |
| `AddPanelMemberAsync(projectId, request, ct)` | Adds a user to the FYP evaluation panel. | `Application/Fyp/FypService.cs` |
| `RemovePanelMemberAsync(projectId, userId, ct)` | Removes a user from the evaluation panel. | `Application/Fyp/FypService.cs` |
| `ScheduleMeetingAsync(request, organiserUserId, ct)` | Schedules a new FYP meeting. | `Application/Fyp/FypService.cs` |
| `RescheduleMeetingAsync(meetingId, request, ct)` | Reschedules an existing meeting. | `Application/Fyp/FypService.cs` |
| `CompleteMeetingAsync(meetingId, request, ct)` | Marks a meeting as completed and stores minutes. | `Application/Fyp/FypService.cs` |
| `CancelMeetingAsync(meetingId, ct)` | Cancels a scheduled meeting. | `Application/Fyp/FypService.cs` |
| `GetMeetingsByProjectAsync(projectId, ct)` | Returns all meetings for a project. | `Application/Fyp/FypService.cs` |
| `GetUpcomingMeetingsAsync(supervisorUserId, ct)` | Returns future meetings for a supervisor. | `Application/Fyp/FypService.cs` |
| `ToSummary(project)` | Maps FypProject to FypProjectSummaryResponse DTO. | `Application/Fyp/FypService.cs` |
| `ToDetail(project)` | Maps FypProject to FypProjectDetailResponse DTO. | `Application/Fyp/FypService.cs` |
| `ToMeetingResponse(meeting)` | Maps FypMeeting to MeetingResponse DTO. | `Application/Fyp/FypService.cs` |

### API — QuizController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | POST /api/quiz — Creates a new quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `Update(id, request, ct)` | PUT /api/quiz/{id} — Updates quiz metadata. Faculty only. | `API/Controllers/QuizController.cs` |
| `Publish(id, ct)` | POST /api/quiz/{id}/publish — Publishes a quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `Unpublish(id, ct)` | POST /api/quiz/{id}/unpublish — Reverts quiz to draft. Faculty only. | `API/Controllers/QuizController.cs` |
| `Deactivate(id, ct)` | DELETE /api/quiz/{id} — Soft-deletes a quiz. Admin only. | `API/Controllers/QuizController.cs` |
| `AddQuestion(request, ct)` | POST /api/quiz/question — Adds a question to a quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `UpdateQuestion(questionId, request, ct)` | PUT /api/quiz/question/{questionId} — Updates a question. Faculty only. | `API/Controllers/QuizController.cs` |
| `RemoveQuestion(questionId, ct)` | DELETE /api/quiz/question/{questionId} — Removes a question. Faculty only. | `API/Controllers/QuizController.cs` |
| `GetByOffering(courseOfferingId, ct)` | GET /api/quiz/by-offering/{courseOfferingId} — Lists quizzes for an offering. | `API/Controllers/QuizController.cs` |
| `GetDetail(id, ct)` | GET /api/quiz/{id} — Returns quiz with questions and options. | `API/Controllers/QuizController.cs` |
| `StartAttempt(id, ct)` | POST /api/quiz/{id}/start — Starts a new attempt. Student only. | `API/Controllers/QuizController.cs` |
| `SubmitAttempt(request, ct)` | POST /api/quiz/attempt/submit — Submits and auto-grades an attempt. Student only. | `API/Controllers/QuizController.cs` |
| `GetMyAttempts(id, ct)` | GET /api/quiz/{id}/my-attempts — Lists a student's own attempts. Student only. | `API/Controllers/QuizController.cs` |
| `GetAttemptDetail(attemptId, ct)` | GET /api/quiz/attempt/{attemptId} — Returns attempt with answers. | `API/Controllers/QuizController.cs` |
| `GradeAnswer(request, ct)` | POST /api/quiz/attempt/grade-answer — Awards marks to a short-answer. Faculty only. | `API/Controllers/QuizController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/QuizController.cs` |
| `GetStudentProfileId()` | Extracts studentProfileId from JWT claim. | `API/Controllers/QuizController.cs` |

### API — FypController

| Function Name | Purpose | Location |
|---|---|---|
| `Propose(request, ct)` | POST /api/fyp — Submits a new FYP project proposal. Student only. | `API/Controllers/FypController.cs` |
| `Update(id, request, ct)` | PUT /api/fyp/{id} — Updates project title/description. Student only. | `API/Controllers/FypController.cs` |
| `Approve(id, request, ct)` | POST /api/fyp/{id}/approve — Approves a proposal. Admin only. | `API/Controllers/FypController.cs` |
| `Reject(id, request, ct)` | POST /api/fyp/{id}/reject — Rejects a proposal with remarks. Admin only. | `API/Controllers/FypController.cs` |
| `AssignSupervisor(id, request, ct)` | POST /api/fyp/{id}/assign-supervisor — Assigns a supervisor. Admin only. | `API/Controllers/FypController.cs` |
| `Complete(id, ct)` | POST /api/fyp/{id}/complete — Marks a project completed. Admin only. | `API/Controllers/FypController.cs` |
| `GetMyProjects(ct)` | GET /api/fyp/my-projects — Returns the student's own projects. | `API/Controllers/FypController.cs` |
| `GetByDepartment(departmentId, status, ct)` | GET /api/fyp/by-department/{departmentId} — Lists department projects. Faculty only. | `API/Controllers/FypController.cs` |
| `GetMySupervised(ct)` | GET /api/fyp/my-supervised — Returns supervised projects. Faculty only. | `API/Controllers/FypController.cs` |
| `GetDetail(id, ct)` | GET /api/fyp/{id} — Returns full project detail. | `API/Controllers/FypController.cs` |
| `AddPanelMember(id, request, ct)` | POST /api/fyp/{id}/panel — Adds a panel member. Admin only. | `API/Controllers/FypController.cs` |
| `RemovePanelMember(id, userId, ct)` | DELETE /api/fyp/{id}/panel/{userId} — Removes a panel member. Admin only. | `API/Controllers/FypController.cs` |
| `ScheduleMeeting(request, ct)` | POST /api/fyp/meeting — Schedules an FYP meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `RescheduleMeeting(meetingId, request, ct)` | PUT /api/fyp/meeting/{meetingId} — Reschedules a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `CompleteMeeting(meetingId, request, ct)` | POST /api/fyp/meeting/{meetingId}/complete — Completes a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `CancelMeeting(meetingId, ct)` | POST /api/fyp/meeting/{meetingId}/cancel — Cancels a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `GetMeetings(id, ct)` | GET /api/fyp/{id}/meetings — Lists all meetings for a project. | `API/Controllers/FypController.cs` |
| `GetUpcomingMeetings(ct)` | GET /api/fyp/meeting/upcoming — Returns upcoming supervisor meetings. Faculty only. | `API/Controllers/FypController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/FypController.cs` |
| `GetStudentProfileId()` | Extracts studentProfileId from JWT claim. | `API/Controllers/FypController.cs` |

---

## Phase 6 — AI Chat Assistant & Analytics

### AiChatService (Application/AiChat/AiChatService.cs)
| Function | Description | File |
|---|---|---|
| `SendMessageAsync(userId, userRole, departmentId, request, ct)` | Sends user message to LLM; guards module status; creates/fetches conversation; persists messages. | `Application/AiChat/AiChatService.cs` |
| `GetConversationsAsync(userId, ct)` | Returns summary list of past conversations for a user. | `Application/AiChat/AiChatService.cs` |
| `GetConversationAsync(conversationId, ct)` | Returns full conversation with message history. | `Application/AiChat/AiChatService.cs` |
| `BuildSystemPrompt(userRole, departmentId)` | Builds role-aware system prompt (Student/Faculty/Admin/SuperAdmin/Finance). | `Application/AiChat/AiChatService.cs` |
| `ToMessageResponse(message)` | Maps ChatMessage domain entity to DTO. | `Application/AiChat/AiChatService.cs` |

### AnalyticsService (Infrastructure/Analytics/AnalyticsService.cs)
| Function | Description | File |
|---|---|---|
| `GetPerformanceReportAsync(departmentId, ct)` | Aggregates student results/submissions per department or all. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `GetAttendanceReportAsync(departmentId, ct)` | Attendance summary per student per course; supports dept filter. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `GetAssignmentStatsAsync(departmentId, ct)` | Assignment submission/grading stats per assignment. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `GetQuizStatsAsync(departmentId, ct)` | Quiz attempt/score stats per quiz. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `ExportPerformancePdfAsync(departmentId, ct)` | Exports performance report as QuestPDF A4 Landscape PDF. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `ExportAttendancePdfAsync(departmentId, ct)` | Exports attendance report as QuestPDF A4 Landscape PDF. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `ExportPerformanceExcelAsync(departmentId, ct)` | Exports performance report as ClosedXML Excel workbook. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `ExportAttendanceExcelAsync(departmentId, ct)` | Exports attendance report as ClosedXML Excel workbook. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `ResolveDeptNameAsync(departmentId, ct)` | Resolves department name from ID; returns "All Departments" for null. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `AddPdfHeader(table, headers)` | Adds styled blue header row to QuestPDF table. | `Infrastructure/Analytics/AnalyticsService.cs` |
| `AddPdfRow(table, values)` | Adds data row with bottom border to QuestPDF table. | `Infrastructure/Analytics/AnalyticsService.cs` |

### AiChatRepository (Infrastructure/Repositories/AiChatRepository.cs)
| Function | Description | File |
|---|---|---|
| `GetByIdAsync(conversationId, ct)` | Fetches a conversation by ID. | `Infrastructure/Repositories/AiChatRepository.cs` |
| `GetByUserAsync(userId, ct)` | Fetches all conversations for a user with messages. | `Infrastructure/Repositories/AiChatRepository.cs` |
| `GetWithMessagesAsync(conversationId, ct)` | Fetches conversation with full message history. | `Infrastructure/Repositories/AiChatRepository.cs` |
| `AddConversationAsync(conversation, ct)` | Persists a new conversation. | `Infrastructure/Repositories/AiChatRepository.cs` |
| `AddMessageAsync(message, ct)` | Persists a new chat message. | `Infrastructure/Repositories/AiChatRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending DbContext changes. | `Infrastructure/Repositories/AiChatRepository.cs` |

### OpenAiLlmClient (Infrastructure/AiChat/OpenAiLlmClient.cs)
| Function | Description | File |
|---|---|---|
| `SendAsync(systemPrompt, messages, ct)` | Calls OpenAI-compatible /v1/chat/completions; returns reply + token count. | `Infrastructure/AiChat/OpenAiLlmClient.cs` |

### AiChatController (API/Controllers/AiChatController.cs)
| Function | Description | File |
|---|---|---|
| `SendMessage(request, ct)` | POST /api/ai/message — Send message to AI. All authenticated roles. | `API/Controllers/AiChatController.cs` |
| `GetConversations(ct)` | GET /api/ai/conversations — List user conversations. | `API/Controllers/AiChatController.cs` |
| `GetConversation(conversationId, ct)` | GET /api/ai/conversations/{id} — Get conversation history. | `API/Controllers/AiChatController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/AiChatController.cs` |
| `GetDepartmentId()` | Extracts optional departmentId from JWT claim. | `API/Controllers/AiChatController.cs` |

### AnalyticsController (API/Controllers/AnalyticsController.cs)
| Function | Description | File |
|---|---|---|
| `GetPerformance(departmentId, ct)` | GET /api/analytics/performance — Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetAttendance(departmentId, ct)` | GET /api/analytics/attendance — Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetAssignmentStats(departmentId, ct)` | GET /api/analytics/assignments — Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetQuizStats(departmentId, ct)` | GET /api/analytics/quizzes — Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `ExportPerformancePdf(departmentId, ct)` | GET /api/analytics/performance/export/pdf — Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportPerformanceExcel(departmentId, ct)` | GET /api/analytics/performance/export/excel — Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportAttendancePdf(departmentId, ct)` | GET /api/analytics/attendance/export/pdf — Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportAttendanceExcelAsync(departmentId, ct)` | GET /api/analytics/attendance/export/excel — Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ResolveEffectiveDepartment(requested)` | Scopes Faculty to own dept; Admin/SuperAdmin see all. | `API/Controllers/AnalyticsController.cs` |

### SecurityHeadersMiddleware (API/Middleware/SecurityHeadersMiddleware.cs)
| Function | Description | File |
|---|---|---|
| `InvokeAsync(context)` | Adds HSTS, X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy, Permissions-Policy headers. | `API/Middleware/SecurityHeadersMiddleware.cs` |
| `UseSecurityHeaders(app)` | Extension method to register the middleware. | `API/Middleware/SecurityHeadersMiddleware.cs` |
