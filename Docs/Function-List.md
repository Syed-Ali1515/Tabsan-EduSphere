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

## Phase 3 ï¿½ Assignments and Results

### Domain ï¿½ Assignment

| Function Name | Purpose | Location |
|---|---|---|
| `Assignment(courseOfferingId, title, description, dueDate, maxMarks)` | Constructor ï¿½ creates an unpublished assignment. | `Domain/Assignments/Assignment.cs` |
| `Publish()` | Marks the assignment as published (visible to students). Throws if already published. | `Domain/Assignments/Assignment.cs` |
| `Retract()` | Withdraws a published assignment. Throws if not published. | `Domain/Assignments/Assignment.cs` |
| `Update(title, description, dueDate, maxMarks)` | Updates editable fields. Throws if already published. | `Domain/Assignments/Assignment.cs` |

### Domain ï¿½ AssignmentSubmission

| Function Name | Purpose | Location |
|---|---|---|
| `AssignmentSubmission(assignmentId, studentProfileId, fileUrl, textContent)` | Constructor ï¿½ requires at least one of fileUrl/textContent. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Grade(marksAwarded, feedback, gradedByUserId)` | Records marks and feedback. Throws if submission was Rejected. | `Domain/Assignments/AssignmentSubmission.cs` |
| `Reject()` | Marks submission as Rejected and clears awarded marks. | `Domain/Assignments/AssignmentSubmission.cs` |

### Domain ï¿½ Result

| Function Name | Purpose | Location |
|---|---|---|
| `Result(studentProfileId, courseOfferingId, resultType, marksObtained, maxMarks)` | Constructor ï¿½ validates marks range. | `Domain/Assignments/Result.cs` |
| `Publish(publishedByUserId)` | One-way publication. Throws if already published. | `Domain/Assignments/Result.cs` |
| `CorrectMarks(newMarksObtained, newMaxMarks)` | Admin-only correction of a published result. Validates range. | `Domain/Assignments/Result.cs` |

### Domain ï¿½ TranscriptExportLog

| Function Name | Purpose | Location |
|---|---|---|
| `TranscriptExportLog(studentProfileId, requestedByUserId, format, documentUrl?, ipAddress?)` | Constructor ï¿½ append-only, immutable after creation. | `Domain/Assignments/TranscriptExportLog.cs` |

### Infrastructure ï¿½ AssignmentRepository

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

### Infrastructure ï¿½ ResultRepository

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

### Application ï¿½ AssignmentService

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
| `ToResponse(assignment, submissionCount)` | Private ï¿½ maps Assignment to AssignmentResponse DTO. | `Application/Assignments/AssignmentService.cs` |
| `ToSubmissionResponse(submission, assignmentTitle)` | Private ï¿½ maps AssignmentSubmission to SubmissionResponse DTO. | `Application/Assignments/AssignmentService.cs` |

### Application ï¿½ ResultService

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
| `ToResponse(result)` | Private ï¿½ maps Result to ResultResponse DTO including percentage. | `Application/Assignments/ResultService.cs` |

### API ï¿½ AssignmentController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/assignment` ï¿½ creates an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Update(id, request, ct)` | `PUT /api/assignment/{id}` ï¿½ updates a draft assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Publish(id, ct)` | `POST /api/assignment/{id}/publish` ï¿½ publishes an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Retract(id, ct)` | `POST /api/assignment/{id}/retract` ï¿½ retracts a published assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Delete(id, ct)` | `DELETE /api/assignment/{id}` ï¿½ soft-deletes when no submissions exist. Admin. | `API/Controllers/AssignmentController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/assignment/by-offering/{id}` ï¿½ lists assignments for an offering. | `API/Controllers/AssignmentController.cs` |
| `GetById(id, ct)` | `GET /api/assignment/{id}` ï¿½ returns a single assignment. | `API/Controllers/AssignmentController.cs` |
| `Submit(request, ct)` | `POST /api/assignment/submit` ï¿½ student submission. Student. | `API/Controllers/AssignmentController.cs` |
| `GetMySubmissions(ct)` | `GET /api/assignment/my-submissions` ï¿½ student's own submissions. Student. | `API/Controllers/AssignmentController.cs` |
| `GetSubmissions(id, ct)` | `GET /api/assignment/{id}/submissions` ï¿½ all submissions for an assignment. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Grade(request, ct)` | `PUT /api/assignment/submissions/grade` ï¿½ grades a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `Reject(assignmentId, studentProfileId, ct)` | `POST /api/assignment/{id}/submissions/{studentId}/reject` ï¿½ rejects a submission. Faculty/Admin. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentUserId()` | Private ï¿½ extracts user ID from JWT NameIdentifier claim. | `API/Controllers/AssignmentController.cs` |
| `GetCurrentStudentProfileId()` | Private ï¿½ extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/AssignmentController.cs` |

### API ï¿½ ResultController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | `POST /api/result` ï¿½ creates a draft result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `BulkCreate(request, ct)` | `POST /api/result/bulk` ï¿½ bulk-creates draft results for a class. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Publish(studentProfileId, courseOfferingId, resultType, ct)` | `POST /api/result/publish` ï¿½ publishes a single result. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `PublishAll(courseOfferingId, ct)` | `POST /api/result/publish-all` ï¿½ publishes all drafts for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `Correct(studentProfileId, courseOfferingId, resultType, request, ct)` | `PUT /api/result/correct` ï¿½ Admin correction of a published result. Admin only. | `API/Controllers/ResultController.cs` |
| `GetMyResults(ct)` | `GET /api/result/my-results` ï¿½ student's own published results. Student. | `API/Controllers/ResultController.cs` |
| `GetByStudent(studentProfileId, ct)` | `GET /api/result/by-student/{id}` ï¿½ all results for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetByOffering(courseOfferingId, ct)` | `GET /api/result/by-offering/{id}` ï¿½ all results for an offering. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetTranscript(studentProfileId, format, ct)` | `GET /api/result/transcript/{id}` ï¿½ exports transcript, logs request. All roles. | `API/Controllers/ResultController.cs` |
| `GetTranscriptHistory(studentProfileId, ct)` | `GET /api/result/transcript/{id}/history` ï¿½ export history for a student. Faculty/Admin. | `API/Controllers/ResultController.cs` |
| `GetCurrentUserId()` | Private ï¿½ extracts user ID from JWT NameIdentifier claim. | `API/Controllers/ResultController.cs` |
| `GetCurrentStudentProfileId()` | Private ï¿½ extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/ResultController.cs` |

---

## Phase 4 ï¿½ Notifications and Attendance

### Domain ï¿½ Notification

| Function Name | Purpose | Location |
|---|---|---|
| `Notification(title, body, type, senderUserId)` | Constructor ï¿½ user-authored notification. | `Domain/Notifications/Notification.cs` |
| `Notification(title, body, type)` | Constructor ï¿½ system-generated notification (no human sender). | `Domain/Notifications/Notification.cs` |
| `Deactivate()` | Hides the notification from all inboxes. Does not delete it. | `Domain/Notifications/Notification.cs` |

### Domain ï¿½ NotificationRecipient

| Function Name | Purpose | Location |
|---|---|---|
| `NotificationRecipient(notificationId, recipientUserId)` | Constructor ï¿½ creates an unread delivery record for the user. | `Domain/Notifications/NotificationRecipient.cs` |
| `MarkRead()` | Marks the notification as read. Idempotent. | `Domain/Notifications/NotificationRecipient.cs` |

### Domain ï¿½ AttendanceRecord

| Function Name | Purpose | Location |
|---|---|---|
| `AttendanceRecord(studentProfileId, courseOfferingId, date, status, markedByUserId, remarks?)` | Constructor ï¿½ normalises date to UTC date only. | `Domain/Attendance/AttendanceRecord.cs` |
| `Correct(newStatus, correctedByUserId, remarks?)` | Corrects status and records the correcting user. | `Domain/Attendance/AttendanceRecord.cs` |

### Infrastructure ï¿½ NotificationRepository

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

### Infrastructure ï¿½ AttendanceRepository

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

### Application ï¿½ NotificationService

| Function Name | Purpose | Location |
|---|---|---|
| `SendAsync(request, senderUserId, ct)` | Creates notification and fans out to recipient list. Returns notification ID. | `Application/Notifications/NotificationService.cs` |
| `SendSystemAsync(title, body, type, recipientUserIds, ct)` | System-generated fan-out (no human sender). Returns notification ID. | `Application/Notifications/NotificationService.cs` |
| `DeactivateAsync(notificationId, ct)` | Deactivates notification from all inboxes. Returns false if not found. | `Application/Notifications/NotificationService.cs` |
| `GetInboxAsync(userId, unreadOnly, page, pageSize, ct)` | Returns paged inbox for a user with optional unread filter. | `Application/Notifications/NotificationService.cs` |
| `GetBadgeAsync(userId, ct)` | Returns unread count for the notification bell badge. | `Application/Notifications/NotificationService.cs` |
| `MarkReadAsync(notificationId, userId, ct)` | Marks a specific notification as read. Idempotent. | `Application/Notifications/NotificationService.cs` |
| `MarkAllReadAsync(userId, ct)` | Marks all unread notifications as read for the user. | `Application/Notifications/NotificationService.cs` |
| `ToResponse(recipient)` | Private ï¿½ maps NotificationRecipient (with navigation) to DTO. | `Application/Notifications/NotificationService.cs` |

### Application ï¿½ AttendanceService

| Function Name | Purpose | Location |
|---|---|---|
| `MarkAsync(request, markedByUserId, ct)` | Records single attendance. Returns false on duplicate. | `Application/Attendance/AttendanceService.cs` |
| `BulkMarkAsync(request, markedByUserId, ct)` | Bulk-marks a class; skips duplicates. Returns inserted count. | `Application/Attendance/AttendanceService.cs` |
| `CorrectAsync(request, correctedByUserId, ct)` | Corrects an existing record. Returns false if not found. | `Application/Attendance/AttendanceService.cs` |
| `GetByOfferingAsync(courseOfferingId, from?, to?, ct)` | Returns records for an offering with optional date filter. | `Application/Attendance/AttendanceService.cs` |
| `GetByStudentAsync(studentProfileId, courseOfferingId?, ct)` | Returns records for a student, optionally scoped to one offering. | `Application/Attendance/AttendanceService.cs` |
| `GetSummaryAsync(studentProfileId, courseOfferingId, ct)` | Returns attendance percentage summary for a student in an offering. | `Application/Attendance/AttendanceService.cs` |
| `GetBelowThresholdAsync(thresholdPercent, ct)` | Returns all student-offering pairs below the threshold. | `Application/Attendance/AttendanceService.cs` |
| `ToResponse(record)` | Private ï¿½ maps AttendanceRecord to AttendanceResponse DTO. | `Application/Attendance/AttendanceService.cs` |

### API ï¿½ NotificationController

| Function Name | Purpose | Location |
|---|---|---|
| `Send(request, ct)` | `POST /api/notification` ï¿½ dispatches notification to a user list. Admin/Faculty. | `API/Controllers/NotificationController.cs` |
| `Deactivate(id, ct)` | `DELETE /api/notification/{id}` ï¿½ deactivates a notification. Admin. | `API/Controllers/NotificationController.cs` |
| `GetInbox(unreadOnly, page, pageSize, ct)` | `GET /api/notification/inbox` ï¿½ paged inbox for current user. | `API/Controllers/NotificationController.cs` |
| `GetBadge(ct)` | `GET /api/notification/badge` ï¿½ unread count for the bell icon. | `API/Controllers/NotificationController.cs` |
| `MarkRead(id, ct)` | `POST /api/notification/{id}/read` ï¿½ marks one notification read. | `API/Controllers/NotificationController.cs` |
| `MarkAllRead(ct)` | `POST /api/notification/read-all` ï¿½ marks all unread as read. | `API/Controllers/NotificationController.cs` |
| `GetCurrentUserId()` | Private ï¿½ extracts user ID from JWT NameIdentifier claim. | `API/Controllers/NotificationController.cs` |

### API ï¿½ AttendanceController

| Function Name | Purpose | Location |
|---|---|---|
| `Mark(request, ct)` | `POST /api/attendance` ï¿½ marks attendance for one student. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `BulkMark(request, ct)` | `POST /api/attendance/bulk` ï¿½ bulk-marks a full class. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `Correct(request, ct)` | `PUT /api/attendance/correct` ï¿½ corrects an existing record. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetByOffering(courseOfferingId, from, to, ct)` | `GET /api/attendance/by-offering/{id}` ï¿½ records for an offering. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetByStudent(studentProfileId, courseOfferingId, ct)` | `GET /api/attendance/by-student/{id}` ï¿½ records for a student. Faculty/Admin. | `API/Controllers/AttendanceController.cs` |
| `GetMyAttendance(courseOfferingId, ct)` | `GET /api/attendance/my-attendance` ï¿½ student's own records. Student. | `API/Controllers/AttendanceController.cs` |
| `GetSummary(studentProfileId, courseOfferingId, ct)` | `GET /api/attendance/summary/{studentId}/{offeringId}` ï¿½ percentage summary. All roles. | `API/Controllers/AttendanceController.cs` |
| `GetBelowThreshold(threshold, ct)` | `GET /api/attendance/below-threshold` ï¿½ students below threshold. Admin. | `API/Controllers/AttendanceController.cs` |
| `GetCurrentUserId()` | Private ï¿½ extracts user ID from JWT NameIdentifier claim. | `API/Controllers/AttendanceController.cs` |
| `GetCurrentStudentProfileId()` | Private ï¿½ extracts student profile ID from "studentProfileId" JWT claim. | `API/Controllers/AttendanceController.cs` |

### Background Job ï¿½ AttendanceAlertJob

| Function Name | Purpose | Location |
|---|---|---|
| `ExecuteAsync(stoppingToken)` | Main hosted service loop ï¿½ waits 60 s startup delay then runs on configured interval. | `BackgroundJobs/AttendanceAlertJob.cs` |
| `RunCheckAsync(ct)` | Resolves scoped services, finds below-threshold students, dispatches alert notifications. | `BackgroundJobs/AttendanceAlertJob.cs` |

---

## Phase 5 ï¿½ Quizzes and FYP (Sprints 10ï¿½11)

### Domain ï¿½ Quiz

| Function Name | Purpose | Location |
|---|---|---|
| Quiz(courseOfferingId, title, createdByUserId, instructions, timeLimitMinutes, maxAttempts, availableFrom, availableUntil) | Creates a new quiz in unpublished state. | Domain/Quizzes/Quiz.cs |
| Publish() | Marks the quiz as published so students can view and attempt it. | Domain/Quizzes/Quiz.cs |
| Unpublish() | Reverts the quiz to draft/unpublished state. | Domain/Quizzes/Quiz.cs |
| Deactivate() | Soft-deletes the quiz by setting IsActive=false. | Domain/Quizzes/Quiz.cs |
| Update(title, instructions, timeLimitMinutes, maxAttempts, availableFrom, availableUntil) | Updates editable quiz metadata. | Domain/Quizzes/Quiz.cs |

### Domain ï¿½ QuizQuestion

| Function Name | Purpose | Location |
|---|---|---|
| QuizQuestion(quizId, text, type, marks, orderIndex) | Creates a new question within a quiz. | Domain/Quizzes/Quiz.cs |
| Update(text, marks, orderIndex) | Updates the question text, marks, and display order. | Domain/Quizzes/Quiz.cs |

### Domain ï¿½ QuizOption

| Function Name | Purpose | Location |
|---|---|---|
| QuizOption(quizQuestionId, text, isCorrect, orderIndex) | Creates an answer option for a MCQ or TrueFalse question. | Domain/Quizzes/Quiz.cs |

### Domain ï¿½ QuizAttempt

| Function Name | Purpose | Location |
|---|---|---|
| QuizAttempt(quizId, studentProfileId) | Starts a new attempt, setting status=InProgress and StartedAt=UtcNow. | Domain/Quizzes/QuizAttempt.cs |
| Submit() | Marks the attempt as Submitted and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| TimeOut() | Marks the attempt as TimedOut and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| Abandon() | Marks the attempt as Abandoned and records FinishedAt. | Domain/Quizzes/QuizAttempt.cs |
| RecordScore(score) | Sets the computed TotalScore on the attempt. | Domain/Quizzes/QuizAttempt.cs |

### Domain ï¿½ QuizAnswer

| Function Name | Purpose | Location |
|---|---|---|
| QuizAnswer(quizAttemptId, quizQuestionId, selectedOptionId) | Records an MCQ or TrueFalse answer by option ID. | Domain/Quizzes/QuizAttempt.cs |
| QuizAnswer(quizAttemptId, quizQuestionId, textResponse) | Records a ShortAnswer response as free text. | Domain/Quizzes/QuizAttempt.cs |
| AwardMarks(marks) | Sets the marks awarded for manually graded short answers. | Domain/Quizzes/QuizAttempt.cs |

### Domain ï¿½ FypProject

| Function Name | Purpose | Location |
|---|---|---|
| FypProject(studentProfileId, departmentId, title, description) | Creates a new FYP proposal in Proposed state. | Domain/Fyp/FypProject.cs |
| Approve(remarks) | Transitions the project to Approved with optional coordinator remarks. | Domain/Fyp/FypProject.cs |
| Reject(remarks) | Transitions the project to Rejected with mandatory remarks. | Domain/Fyp/FypProject.cs |
| AssignSupervisor(supervisorUserId) | Records the supervising faculty member and sets status to InProgress. | Domain/Fyp/FypProject.cs |
| Complete() | Marks the project as Completed. | Domain/Fyp/FypProject.cs |
| Update(title, description) | Updates the project title and description. | Domain/Fyp/FypProject.cs |

### Domain ï¿½ FypPanelMember

| Function Name | Purpose | Location |
|---|---|---|
| FypPanelMember(fypProjectId, userId, role) | Adds a faculty member to the project panel with a specified role. | Domain/Fyp/FypProject.cs |

### Domain ï¿½ FypMeeting

| Function Name | Purpose | Location |
|---|---|---|
| FypMeeting(fypProjectId, scheduledAt, venue, organiserUserId, agenda) | Schedules a new FYP meeting in Scheduled state. | Domain/Fyp/FypProject.cs |
| Complete(minutes) | Marks the meeting as Completed and records optional minutes. | Domain/Fyp/FypProject.cs |
| Cancel() | Cancels a scheduled meeting. | Domain/Fyp/FypProject.cs |
| Reschedule(scheduledAt, venue, agenda) | Updates the meeting time, venue, and agenda and resets status to Scheduled. | Domain/Fyp/FypProject.cs |

### Infrastructure ï¿½ QuizRepository

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

### Infrastructure ï¿½ FypRepository

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

### Application ï¿½ QuizService

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
| ToSummary(quiz) | Private ï¿½ maps Quiz to QuizSummaryResponse. | Application/Quizzes/QuizService.cs |
| ToDetail(quiz) | Private ï¿½ maps Quiz with questions to QuizDetailResponse. | Application/Quizzes/QuizService.cs |
| ToQuestionResponse(question, hideAnswers) | Private ï¿½ maps a QuizQuestion to QuestionResponse, optionally hiding correct answers. | Application/Quizzes/QuizService.cs |
| ToAttemptResponse(attempt) | Private ï¿½ maps QuizAttempt to AttemptResponse. | Application/Quizzes/QuizService.cs |
| ToAttemptDetail(attempt) | Private ï¿½ maps QuizAttempt with answers to AttemptDetailResponse. | Application/Quizzes/QuizService.cs |

### Application ï¿½ FypService

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
| ToSummary(project) | Private ï¿½ maps FypProject to FypProjectSummaryResponse. | Application/Fyp/FypService.cs |
| ToDetail(project) | Private ï¿½ maps FypProject with panel/meetings to FypProjectDetailResponse. | Application/Fyp/FypService.cs |
| ToMeetingResponse(meeting) | Private ï¿½ maps FypMeeting to MeetingResponse. | Application/Fyp/FypService.cs |

### API ï¿½ QuizController

| Function Name | Purpose | Location |
|---|---|---|
| Create(request, ct) | POST /api/quiz ï¿½ Creates a quiz (Faculty). | API/Controllers/QuizController.cs |
| Update(id, request, ct) | PUT /api/quiz/{id} ï¿½ Updates quiz metadata (Faculty). | API/Controllers/QuizController.cs |
| Publish(id, ct) | POST /api/quiz/{id}/publish ï¿½ Publishes a quiz (Faculty). | API/Controllers/QuizController.cs |
| Unpublish(id, ct) | POST /api/quiz/{id}/unpublish ï¿½ Unpublishes a quiz (Faculty). | API/Controllers/QuizController.cs |
| Deactivate(id, ct) | DELETE /api/quiz/{id} ï¿½ Soft-deletes a quiz (Admin). | API/Controllers/QuizController.cs |
| AddQuestion(request, ct) | POST /api/quiz/question ï¿½ Adds a question to a quiz (Faculty). | API/Controllers/QuizController.cs |
| UpdateQuestion(questionId, request, ct) | PUT /api/quiz/question/{questionId} ï¿½ Updates a question (Faculty). | API/Controllers/QuizController.cs |
| RemoveQuestion(questionId, ct) | DELETE /api/quiz/question/{questionId} ï¿½ Removes a question (Faculty). | API/Controllers/QuizController.cs |
| GetByOffering(courseOfferingId, ct) | GET /api/quiz/by-offering/{courseOfferingId} ï¿½ Lists quizzes for an offering (All). | API/Controllers/QuizController.cs |
| GetDetail(id, ct) | GET /api/quiz/{id} ï¿½ Returns full quiz detail (All). | API/Controllers/QuizController.cs |
| StartAttempt(id, ct) | POST /api/quiz/{id}/start ï¿½ Starts a student attempt; 409 if cap reached (Student). | API/Controllers/QuizController.cs |
| SubmitAttempt(request, ct) | POST /api/quiz/attempt/submit ï¿½ Submits answers and grades MCQ/TrueFalse (Student). | API/Controllers/QuizController.cs |
| GetMyAttempts(id, ct) | GET /api/quiz/{id}/my-attempts ï¿½ Returns student's own attempts (Student). | API/Controllers/QuizController.cs |
| GetAttemptDetail(attemptId, ct) | GET /api/quiz/attempt/{attemptId} ï¿½ Returns attempt detail with answers (All). | API/Controllers/QuizController.cs |
| GradeAnswer(request, ct) | POST /api/quiz/attempt/grade-answer ï¿½ Manually grades a short-answer response (Faculty). | API/Controllers/QuizController.cs |
| GetCurrentUserId() | Private ï¿½ Extracts authenticated user ID from JWT NameIdentifier claim. | API/Controllers/QuizController.cs |
| GetStudentProfileId() | Private ï¿½ Extracts student profile ID from the studentProfileId JWT claim. | API/Controllers/QuizController.cs |

### API ï¿½ FypController

| Function Name | Purpose | Location |
|---|---|---|
| Propose(request, ct) | POST /api/fyp ï¿½ Submits an FYP proposal (Student). | API/Controllers/FypController.cs |
| Update(id, request, ct) | PUT /api/fyp/{id} ï¿½ Updates project title/description (Student). | API/Controllers/FypController.cs |
| Approve(id, request, ct) | POST /api/fyp/{id}/approve ï¿½ Approves a proposal (Admin). | API/Controllers/FypController.cs |
| Reject(id, request, ct) | POST /api/fyp/{id}/reject ï¿½ Rejects a proposal with remarks (Admin). | API/Controllers/FypController.cs |
| AssignSupervisor(id, request, ct) | POST /api/fyp/{id}/assign-supervisor ï¿½ Assigns a supervisor (Admin). | API/Controllers/FypController.cs |
| Complete(id, ct) | POST /api/fyp/{id}/complete ï¿½ Marks a project as completed (Admin). | API/Controllers/FypController.cs |
| GetMyProjects(ct) | GET /api/fyp/my-projects ï¿½ Returns current student's projects (Student). | API/Controllers/FypController.cs |
| GetByDepartment(departmentId, status, ct) | GET /api/fyp/by-department/{departmentId} ï¿½ Returns department projects (Faculty). | API/Controllers/FypController.cs |
| GetMySupervised(ct) | GET /api/fyp/my-supervised ï¿½ Returns projects supervised by current user (Faculty). | API/Controllers/FypController.cs |
| GetDetail(id, ct) | GET /api/fyp/{id} ï¿½ Returns full project detail (All). | API/Controllers/FypController.cs |
| AddPanelMember(id, request, ct) | POST /api/fyp/{id}/panel ï¿½ Adds a panel member (Admin). | API/Controllers/FypController.cs |
| RemovePanelMember(id, userId, ct) | DELETE /api/fyp/{id}/panel/{userId} ï¿½ Removes a panel member (Admin). | API/Controllers/FypController.cs |
| ScheduleMeeting(request, ct) | POST /api/fyp/meeting ï¿½ Schedules a new FYP meeting (Faculty). | API/Controllers/FypController.cs |
| RescheduleMeeting(meetingId, request, ct) | PUT /api/fyp/meeting/{meetingId} ï¿½ Reschedules a meeting (Faculty). | API/Controllers/FypController.cs |
| CompleteMeeting(meetingId, request, ct) | POST /api/fyp/meeting/{meetingId}/complete ï¿½ Completes a meeting (Faculty). | API/Controllers/FypController.cs |
| CancelMeeting(meetingId, ct) | POST /api/fyp/meeting/{meetingId}/cancel ï¿½ Cancels a meeting (Faculty). | API/Controllers/FypController.cs |
| GetMeetings(id, ct) | GET /api/fyp/{id}/meetings ï¿½ Returns all meetings for a project (All). | API/Controllers/FypController.cs |
| GetUpcomingMeetings(ct) | GET /api/fyp/meeting/upcoming ï¿½ Returns upcoming meetings for current supervisor (Faculty). | API/Controllers/FypController.cs |
| GetCurrentUserId() | Private ï¿½ Extracts authenticated user ID from JWT NameIdentifier claim. | API/Controllers/FypController.cs |
| GetStudentProfileId() | Private ï¿½ Extracts student profile ID from the studentProfileId JWT claim. | API/Controllers/FypController.cs |

---

## Phase 5 ï¿½ Quizzes and FYP

### Domain ï¿½ Quiz

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

### Domain ï¿½ QuizAttempt

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

### Domain ï¿½ FypProject

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

### Infrastructure ï¿½ QuizRepository

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

### Infrastructure ï¿½ FypRepository

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

### Application ï¿½ QuizService

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

### Application ï¿½ FypService

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

### API ï¿½ QuizController

| Function Name | Purpose | Location |
|---|---|---|
| `Create(request, ct)` | POST /api/quiz ï¿½ Creates a new quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `Update(id, request, ct)` | PUT /api/quiz/{id} ï¿½ Updates quiz metadata. Faculty only. | `API/Controllers/QuizController.cs` |
| `Publish(id, ct)` | POST /api/quiz/{id}/publish ï¿½ Publishes a quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `Unpublish(id, ct)` | POST /api/quiz/{id}/unpublish ï¿½ Reverts quiz to draft. Faculty only. | `API/Controllers/QuizController.cs` |
| `Deactivate(id, ct)` | DELETE /api/quiz/{id} ï¿½ Soft-deletes a quiz. Admin only. | `API/Controllers/QuizController.cs` |
| `AddQuestion(request, ct)` | POST /api/quiz/question ï¿½ Adds a question to a quiz. Faculty only. | `API/Controllers/QuizController.cs` |
| `UpdateQuestion(questionId, request, ct)` | PUT /api/quiz/question/{questionId} ï¿½ Updates a question. Faculty only. | `API/Controllers/QuizController.cs` |
| `RemoveQuestion(questionId, ct)` | DELETE /api/quiz/question/{questionId} ï¿½ Removes a question. Faculty only. | `API/Controllers/QuizController.cs` |
| `GetByOffering(courseOfferingId, ct)` | GET /api/quiz/by-offering/{courseOfferingId} ï¿½ Lists quizzes for an offering. | `API/Controllers/QuizController.cs` |
| `GetDetail(id, ct)` | GET /api/quiz/{id} ï¿½ Returns quiz with questions and options. | `API/Controllers/QuizController.cs` |
| `StartAttempt(id, ct)` | POST /api/quiz/{id}/start ï¿½ Starts a new attempt. Student only. | `API/Controllers/QuizController.cs` |
| `SubmitAttempt(request, ct)` | POST /api/quiz/attempt/submit ï¿½ Submits and auto-grades an attempt. Student only. | `API/Controllers/QuizController.cs` |
| `GetMyAttempts(id, ct)` | GET /api/quiz/{id}/my-attempts ï¿½ Lists a student's own attempts. Student only. | `API/Controllers/QuizController.cs` |
| `GetAttemptDetail(attemptId, ct)` | GET /api/quiz/attempt/{attemptId} ï¿½ Returns attempt with answers. | `API/Controllers/QuizController.cs` |
| `GradeAnswer(request, ct)` | POST /api/quiz/attempt/grade-answer ï¿½ Awards marks to a short-answer. Faculty only. | `API/Controllers/QuizController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/QuizController.cs` |
| `GetStudentProfileId()` | Extracts studentProfileId from JWT claim. | `API/Controllers/QuizController.cs` |

### API ï¿½ FypController

| Function Name | Purpose | Location |
|---|---|---|
| `Propose(request, ct)` | POST /api/fyp ï¿½ Submits a new FYP project proposal. Student only. | `API/Controllers/FypController.cs` |
| `Update(id, request, ct)` | PUT /api/fyp/{id} ï¿½ Updates project title/description. Student only. | `API/Controllers/FypController.cs` |
| `Approve(id, request, ct)` | POST /api/fyp/{id}/approve ï¿½ Approves a proposal. Admin only. | `API/Controllers/FypController.cs` |
| `Reject(id, request, ct)` | POST /api/fyp/{id}/reject ï¿½ Rejects a proposal with remarks. Admin only. | `API/Controllers/FypController.cs` |
| `AssignSupervisor(id, request, ct)` | POST /api/fyp/{id}/assign-supervisor ï¿½ Assigns a supervisor. Admin only. | `API/Controllers/FypController.cs` |
| `Complete(id, ct)` | POST /api/fyp/{id}/complete ï¿½ Marks a project completed. Admin only. | `API/Controllers/FypController.cs` |
| `GetMyProjects(ct)` | GET /api/fyp/my-projects ï¿½ Returns the student's own projects. | `API/Controllers/FypController.cs` |
| `GetByDepartment(departmentId, status, ct)` | GET /api/fyp/by-department/{departmentId} ï¿½ Lists department projects. Faculty only. | `API/Controllers/FypController.cs` |
| `GetMySupervised(ct)` | GET /api/fyp/my-supervised ï¿½ Returns supervised projects. Faculty only. | `API/Controllers/FypController.cs` |
| `GetDetail(id, ct)` | GET /api/fyp/{id} ï¿½ Returns full project detail. | `API/Controllers/FypController.cs` |
| `AddPanelMember(id, request, ct)` | POST /api/fyp/{id}/panel ï¿½ Adds a panel member. Admin only. | `API/Controllers/FypController.cs` |
| `RemovePanelMember(id, userId, ct)` | DELETE /api/fyp/{id}/panel/{userId} ï¿½ Removes a panel member. Admin only. | `API/Controllers/FypController.cs` |
| `ScheduleMeeting(request, ct)` | POST /api/fyp/meeting ï¿½ Schedules an FYP meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `RescheduleMeeting(meetingId, request, ct)` | PUT /api/fyp/meeting/{meetingId} ï¿½ Reschedules a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `CompleteMeeting(meetingId, request, ct)` | POST /api/fyp/meeting/{meetingId}/complete ï¿½ Completes a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `CancelMeeting(meetingId, ct)` | POST /api/fyp/meeting/{meetingId}/cancel ï¿½ Cancels a meeting. Faculty only. | `API/Controllers/FypController.cs` |
| `GetMeetings(id, ct)` | GET /api/fyp/{id}/meetings ï¿½ Lists all meetings for a project. | `API/Controllers/FypController.cs` |
| `GetUpcomingMeetings(ct)` | GET /api/fyp/meeting/upcoming ï¿½ Returns upcoming supervisor meetings. Faculty only. | `API/Controllers/FypController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/FypController.cs` |
| `GetStudentProfileId()` | Extracts studentProfileId from JWT claim. | `API/Controllers/FypController.cs` |

---

## Phase 6 ï¿½ AI Chat Assistant & Analytics

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
| `SendMessage(request, ct)` | POST /api/ai/message ï¿½ Send message to AI. All authenticated roles. | `API/Controllers/AiChatController.cs` |
| `GetConversations(ct)` | GET /api/ai/conversations ï¿½ List user conversations. | `API/Controllers/AiChatController.cs` |
| `GetConversation(conversationId, ct)` | GET /api/ai/conversations/{id} ï¿½ Get conversation history. | `API/Controllers/AiChatController.cs` |
| `GetCurrentUserId()` | Extracts user ID from NameIdentifier JWT claim. | `API/Controllers/AiChatController.cs` |
| `GetDepartmentId()` | Extracts optional departmentId from JWT claim. | `API/Controllers/AiChatController.cs` |

### AnalyticsController (API/Controllers/AnalyticsController.cs)
| Function | Description | File |
|---|---|---|
| `GetPerformance(departmentId, ct)` | GET /api/analytics/performance ï¿½ Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetAttendance(departmentId, ct)` | GET /api/analytics/attendance ï¿½ Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetAssignmentStats(departmentId, ct)` | GET /api/analytics/assignments ï¿½ Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `GetQuizStats(departmentId, ct)` | GET /api/analytics/quizzes ï¿½ Faculty+ scoped. | `API/Controllers/AnalyticsController.cs` |
| `ExportPerformancePdf(departmentId, ct)` | GET /api/analytics/performance/export/pdf ï¿½ Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportPerformanceExcel(departmentId, ct)` | GET /api/analytics/performance/export/excel ï¿½ Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportAttendancePdf(departmentId, ct)` | GET /api/analytics/attendance/export/pdf ï¿½ Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ExportAttendanceExcelAsync(departmentId, ct)` | GET /api/analytics/attendance/export/excel ï¿½ Admin+ only. | `API/Controllers/AnalyticsController.cs` |
| `ResolveEffectiveDepartment(requested)` | Scopes Faculty to own dept; Admin/SuperAdmin see all. | `API/Controllers/AnalyticsController.cs` |

### SecurityHeadersMiddleware (API/Middleware/SecurityHeadersMiddleware.cs)
| Function | Description | File |
|---|---|---|
| `InvokeAsync(context)` | Adds HSTS, X-Content-Type-Options, X-Frame-Options, CSP, Referrer-Policy, Permissions-Policy headers. | `API/Middleware/SecurityHeadersMiddleware.cs` |
| `UseSecurityHeaders(app)` | Extension method to register the middleware. | `API/Middleware/SecurityHeadersMiddleware.cs` |

## Phase 7: Tabsan-Lic + License Import

### Tabsan.Lic ï¿½ KeyService (tools/Tabsan.Lic/Services/KeyService.cs)
| Function | Description | File |
|---|---|---|
| `GenerateAsync(expiry, label)` | Generates a random VerificationKey, stores SHA-256 hash in SQLite, returns (record, rawToken). | `tools/Tabsan.Lic/Services/KeyService.cs` |
| `GenerateBulkAsync(count, expiry, labelPrefix)` | Generates N keys at once with the same expiry type. | `tools/Tabsan.Lic/Services/KeyService.cs` |
| `ListAllAsync()` | Returns all issued keys ordered by IssuedAt desc. | `tools/Tabsan.Lic/Services/KeyService.cs` |
| `GetByIdAsync(id)` | Returns a key record by auto-increment Id. | `tools/Tabsan.Lic/Services/KeyService.cs` |
| `MarkLicenseGeneratedAsync(key)` | Sets IsLicenseGenerated=true on a key record after .tablic file is built. | `tools/Tabsan.Lic/Services/KeyService.cs` |
| `ExportCsvAsync()` | Exports all issued keys to CSV string (Id, KeyId, ExpiryType, dates, flags, label). | `tools/Tabsan.Lic/Services/KeyService.cs` |

---

## Phase 8: Student Lifecycle, Account Security & Finance (Sprints 15â€“16)

### Domain â€” StudentProfile new methods
| Function | Description | File |
|---|---|---|
| `Graduate(adminUserId)` | Sets status to `Graduated`; records graduation timestamp and acting admin. | `Domain/Entities/StudentProfile.cs` |
| `Deactivate(adminUserId, reason)` | Sets status to `Inactive`; blocks login for the student. | `Domain/Entities/StudentProfile.cs` |
| `Reactivate(adminUserId)` | Restores status to `Active` from `Inactive`. | `Domain/Entities/StudentProfile.cs` |

### Domain â€” User new methods
| Function | Description | File |
|---|---|---|
| `RecordFailedLoginAttempt()` | Increments `FailedLoginCount`; locks account (15-min window) after 5 consecutive failures. | `Domain/Entities/User.cs` |
| `UnlockAccount()` | Resets `FailedLoginCount` to 0 and clears `LockoutEnd`. | `Domain/Entities/User.cs` |
| `IsCurrentlyLockedOut()` | Returns true if `LockoutEnd` is set and has not yet elapsed. | `Domain/Entities/User.cs` |

### Domain â€” AdminChangeRequest entity
| Function | Description | File |
|---|---|---|
| `AdminChangeRequest(studentProfileId, requestedByUserId, field, oldValue, newValue)` | Constructor; creates a pending change request for a protected student field. | `Domain/Entities/AdminChangeRequest.cs` |
| `Approve(adminUserId)` | Sets status to `Approved`; applies the requested field value to the student profile. | `Domain/Entities/AdminChangeRequest.cs` |
| `Reject(adminUserId, remarks)` | Sets status to `Rejected` with rejection remarks. | `Domain/Entities/AdminChangeRequest.cs` |

### Domain â€” TeacherModificationRequest entity
| Function | Description | File |
|---|---|---|
| `TeacherModificationRequest(teacherUserId, field, oldValue, newValue)` | Constructor; creates a pending modification request for a teacher-editable field. | `Domain/Entities/TeacherModificationRequest.cs` |
| `Approve(adminUserId)` | Sets status to `Approved`; applies the requested change. | `Domain/Entities/TeacherModificationRequest.cs` |
| `Reject(adminUserId, remarks)` | Sets status to `Rejected`. | `Domain/Entities/TeacherModificationRequest.cs` |

### Domain â€” PaymentReceipt entity
| Function | Description | File |
|---|---|---|
| `PaymentReceipt(studentProfileId, createdByUserId, amount, description, dueDate)` | Constructor; creates a new fee receipt in `Pending` status. | `Domain/Entities/PaymentReceipt.cs` |
| `SubmitProof(proofFilePath)` | Student action â€” attaches proof of payment file path; sets status to `ProofSubmitted`. | `Domain/Entities/PaymentReceipt.cs` |
| `Confirm(financeUserId)` | Finance action â€” marks receipt as `Paid`; records confirmation timestamp. | `Domain/Entities/PaymentReceipt.cs` |
| `Cancel(cancelledByUserId, reason)` | Cancels the receipt; status set to `Cancelled`. | `Domain/Entities/PaymentReceipt.cs` |

### Infrastructure â€” StudentLifecycleRepository
| Function | Description | File |
|---|---|---|
| `GetStudentByIdAsync(studentProfileId, ct)` | Returns StudentProfile with User navigation. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetStudentsByDepartmentAsync(departmentId, status, ct)` | Lists students by department with optional status filter. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetFinalSemesterStudentsAsync(departmentId, ct)` | Returns students in their final semester, eligible for graduation. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GraduateAsync(studentProfileId, adminUserId, ct)` | Persists graduation; calls `StudentProfile.Graduate`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `DeactivateAsync(studentProfileId, adminUserId, reason, ct)` | Persists deactivation. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `ReactivateAsync(studentProfileId, adminUserId, ct)` | Persists reactivation. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `TransferDepartmentAsync(studentProfileId, newDeptId, newProgramId, newSemester, ct)` | Updates student department, program, and semester. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `CreateChangeRequestAsync(request, ct)` | Persists a new `AdminChangeRequest`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetChangeRequestByIdAsync(id, ct)` | Returns a change request by ID. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetChangeRequestsForStudentAsync(studentProfileId, ct)` | Returns all change requests for a student. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetPendingChangeRequestsAsync(ct)` | Returns all pending change requests across all students. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `ApproveChangeRequestAsync(id, adminUserId, ct)` | Calls `AdminChangeRequest.Approve`; persists. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `RejectChangeRequestAsync(id, adminUserId, remarks, ct)` | Calls `AdminChangeRequest.Reject`; persists. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `CreateTeacherModificationAsync(request, ct)` | Persists a new `TeacherModificationRequest`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetTeacherModificationByIdAsync(id, ct)` | Returns a teacher modification request by ID. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetTeacherModificationsByTeacherAsync(teacherUserId, ct)` | Returns all modification requests by a teacher. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetPendingTeacherModificationsAsync(ct)` | Returns all pending teacher modification requests. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `ApproveTeacherModificationAsync(id, adminUserId, ct)` | Approves a teacher modification request. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `RejectTeacherModificationAsync(id, adminUserId, remarks, ct)` | Rejects a teacher modification request. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `CreatePaymentReceiptAsync(receipt, ct)` | Persists a new `PaymentReceipt`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetPaymentReceiptByIdAsync(id, ct)` | Returns a payment receipt by ID. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetPaymentReceiptsForStudentAsync(studentProfileId, ct)` | Returns all receipts for a student. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `GetPaymentReceiptsByStatusAsync(status, ct)` | Returns all receipts matching a given status. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `SubmitProofAsync(id, proofFilePath, ct)` | Student submits payment proof; calls `PaymentReceipt.SubmitProof`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `ConfirmPaymentAsync(id, financeUserId, ct)` | Finance confirms payment; calls `PaymentReceipt.Confirm`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `CancelReceiptAsync(id, cancelledByUserId, reason, ct)` | Cancels a receipt; calls `PaymentReceipt.Cancel`. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `AddRegistrationNumberAsync(registrationNumber, ct)` | Adds a single registration number to the whitelist. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `BulkAddRegistrationNumbersAsync(numbers, ct)` | Adds a list of registration numbers from CSV import. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `RegistrationNumberExistsAsync(registrationNumber, ct)` | Returns true if a registration number is in the whitelist. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `AccountExistsForRegistrationAsync(registrationNumber, ct)` | Returns true if an account already exists for this registration number. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |
| `SaveChangesAsync(ct)` | Commits pending EF changes. | `Infrastructure/Repositories/StudentLifecycleRepository.cs` |

### Infrastructure â€” UserRepository (extended)
| Function | Description | File |
|---|---|---|
| `GetLockedAccountsAsync(ct)` | Returns all users with active lockouts (`LockoutEnd > UtcNow`). | `Infrastructure/Repositories/UserRepository.cs` |

### Application â€” StudentLifecycleService
| Function | Description | File |
|---|---|---|
| `GetStudentAsync(id, ct)` | Returns a student profile detail DTO. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetStudentsByDepartmentAsync(deptId, status, ct)` | Returns list of students by department. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetFinalSemesterStudentsAsync(deptId, ct)` | Returns eligible-for-graduation students. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GraduateStudentAsync(id, adminUserId, ct)` | Graduates the student. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `DeactivateStudentAsync(id, adminUserId, reason, ct)` | Deactivates a student account. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ReactivateStudentAsync(id, adminUserId, ct)` | Reactivates a student account. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `TransferStudentAsync(id, newDeptId, newProgramId, newSemester, ct)` | Transfers student to new department/program/semester. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetChangeRequestsAsync(studentId, ct)` | Returns all change requests for a student. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetPendingChangeRequestsAsync(ct)` | Returns all pending admin change requests. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `CreateChangeRequestAsync(request, ct)` | Creates a new admin change request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ApproveChangeRequestAsync(id, adminUserId, ct)` | Approves a change request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `RejectChangeRequestAsync(id, adminUserId, remarks, ct)` | Rejects a change request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `CreateTeacherModificationAsync(request, ct)` | Creates a teacher modification request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetTeacherModificationsAsync(teacherId, ct)` | Returns all modification requests by a teacher. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetPendingTeacherModificationsAsync(ct)` | Returns all pending teacher modification requests. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ApproveTeacherModificationAsync(id, adminUserId, ct)` | Approves a teacher modification request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `RejectTeacherModificationAsync(id, adminUserId, remarks, ct)` | Rejects a teacher modification request. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `CreatePaymentReceiptAsync(request, ct)` | Finance creates a new fee receipt. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetReceiptsForStudentAsync(studentId, ct)` | Returns all receipts for a student. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetReceiptsByStatusAsync(status, ct)` | Returns receipts by status (Pending/ProofSubmitted/Paid/Cancelled). | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetReceiptAsync(id, ct)` | Returns a single receipt. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `SubmitPaymentProofAsync(id, proofFilePath, ct)` | Student submits proof of payment. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ConfirmPaymentAsync(id, financeUserId, ct)` | Finance confirms payment. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `CancelReceiptAsync(id, cancelledByUserId, reason, ct)` | Cancels a payment receipt. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `GetFeeStatusAsync(studentId, ct)` | Returns outstanding fee status (any unpaid receipts). | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ImportRegistrationCsvAsync(csv, ct)` | Parses and bulk-imports registration numbers from CSV string. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `AddRegistrationNumberAsync(number, ct)` | Adds single registration number to whitelist. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `ValidateSignupRegistrationAsync(number, ct)` | Validates number exists in whitelist and has no existing account. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `MapToStudentSummary(profile)` | Maps StudentProfile to StudentSummaryDto. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `MapToStudentDetail(profile)` | Maps StudentProfile + User to StudentDetailDto. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `MapToChangeRequestDto(cr)` | Maps AdminChangeRequest to ChangeRequestDto. | `Application/StudentLifecycle/StudentLifecycleService.cs` |
| `MapToPaymentReceiptDto(receipt)` | Maps PaymentReceipt to PaymentReceiptDto. | `Application/StudentLifecycle/StudentLifecycleService.cs` |

### Application â€” AccountSecurityService
| Function | Description | File |
|---|---|---|
| `GetLockoutStatusAsync(userId, ct)` | Returns current lockout status and remaining time for a user. | `Application/AccountSecurity/AccountSecurityService.cs` |
| `UnlockAccountAsync(userId, adminUserId, ct)` | Unlocks a user account; validates admin privilege rules. | `Application/AccountSecurity/AccountSecurityService.cs` |
| `ResetPasswordAsync(userId, adminUserId, newPassword, ct)` | Resets a user's password; enforces password history policy. | `Application/AccountSecurity/AccountSecurityService.cs` |
| `GetLockedAccountsAsync(ct)` | Returns all currently locked-out accounts. | `Application/AccountSecurity/AccountSecurityService.cs` |

### Application â€” CsvRegistrationImportService
| Function | Description | File |
|---|---|---|
| `ParseAsync(csvContent, ct)` | Parses a CSV string; extracts and validates registration number rows; returns result with errors list. | `Application/Import/CsvRegistrationImportService.cs` |
| `ImportAsync(numbers, ct)` | Bulk-adds valid registration numbers; skips duplicates; returns import summary. | `Application/Import/CsvRegistrationImportService.cs` |

### API â€” StudentLifecycleController
| Function | Description | File |
|---|---|---|
| `GetStudents(deptId, status, ct)` | GET /api/v1/students â€” Lists students by dept/status. Admin+. | `API/Controllers/StudentLifecycleController.cs` |
| `GetStudent(id, ct)` | GET /api/v1/students/{id} â€” Returns student detail. Admin+. | `API/Controllers/StudentLifecycleController.cs` |
| `GetFinalSemester(deptId, ct)` | GET /api/v1/students/final-semester â€” Graduates-eligible list. Admin. | `API/Controllers/StudentLifecycleController.cs` |
| `Graduate(id, ct)` | POST /api/v1/students/{id}/graduate â€” Marks student graduated. Admin. | `API/Controllers/StudentLifecycleController.cs` |
| `Deactivate(id, request, ct)` | POST /api/v1/students/{id}/deactivate â€” Deactivates student. Admin. | `API/Controllers/StudentLifecycleController.cs` |
| `Reactivate(id, ct)` | POST /api/v1/students/{id}/reactivate â€” Reactivates student. Admin. | `API/Controllers/StudentLifecycleController.cs` |
| `Transfer(id, request, ct)` | POST /api/v1/students/{id}/transfer â€” Transfers to new dept/program. Admin. | `API/Controllers/StudentLifecycleController.cs` |
| `GetCurrentUserId()` | Extracts user ID from JWT NameIdentifier claim. | `API/Controllers/StudentLifecycleController.cs` |

### API â€” AdminChangeRequestController
| Function | Description | File |
|---|---|---|
| `GetPending(ct)` | GET /api/v1/admin-change-requests/pending â€” All pending requests. Admin. | `API/Controllers/AdminChangeRequestController.cs` |
| `GetForStudent(studentId, ct)` | GET /api/v1/admin-change-requests/student/{id} â€” Requests for student. Admin. | `API/Controllers/AdminChangeRequestController.cs` |
| `Create(request, ct)` | POST /api/v1/admin-change-requests â€” Creates a change request. Admin. | `API/Controllers/AdminChangeRequestController.cs` |
| `Approve(id, ct)` | POST /api/v1/admin-change-requests/{id}/approve â€” Approves request. Admin. | `API/Controllers/AdminChangeRequestController.cs` |
| `Reject(id, request, ct)` | POST /api/v1/admin-change-requests/{id}/reject â€” Rejects request. Admin. | `API/Controllers/AdminChangeRequestController.cs` |
| `GetCurrentUserId()` | Extracts user ID from JWT. | `API/Controllers/AdminChangeRequestController.cs` |

### API â€” TeacherModificationController
| Function | Description | File |
|---|---|---|
| `GetPending(ct)` | GET /api/v1/teacher-modifications/pending â€” All pending requests. Admin. | `API/Controllers/TeacherModificationController.cs` |
| `GetByTeacher(teacherId, ct)` | GET /api/v1/teacher-modifications/teacher/{id} â€” Requests by teacher. Admin+. | `API/Controllers/TeacherModificationController.cs` |
| `Create(request, ct)` | POST /api/v1/teacher-modifications â€” Submits modification request. Faculty. | `API/Controllers/TeacherModificationController.cs` |
| `Approve(id, ct)` | POST /api/v1/teacher-modifications/{id}/approve â€” Approves request. Admin. | `API/Controllers/TeacherModificationController.cs` |
| `Reject(id, request, ct)` | POST /api/v1/teacher-modifications/{id}/reject â€” Rejects request. Admin. | `API/Controllers/TeacherModificationController.cs` |
| `GetCurrentUserId()` | Extracts user ID from JWT. | `API/Controllers/TeacherModificationController.cs` |

### API â€” PaymentReceiptController
| Function | Description | File |
|---|---|---|
| `Create(request, ct)` | POST /api/v1/payment-receipts â€” Finance creates fee receipt. Finance role. | `API/Controllers/PaymentReceiptController.cs` |
| `GetForStudent(studentId, ct)` | GET /api/v1/payment-receipts/student/{id} â€” Receipts for student. Finance/Admin. | `API/Controllers/PaymentReceiptController.cs` |
| `GetByStatus(status, ct)` | GET /api/v1/payment-receipts/by-status â€” Receipts by status. Finance. | `API/Controllers/PaymentReceiptController.cs` |
| `GetReceipt(id, ct)` | GET /api/v1/payment-receipts/{id} â€” Single receipt. | `API/Controllers/PaymentReceiptController.cs` |
| `SubmitProof(id, file, ct)` | POST /api/v1/payment-receipts/{id}/proof â€” Student uploads proof; validated file upload. | `API/Controllers/PaymentReceiptController.cs` |
| `Confirm(id, ct)` | POST /api/v1/payment-receipts/{id}/confirm â€” Finance confirms. Finance. | `API/Controllers/PaymentReceiptController.cs` |
| `Cancel(id, request, ct)` | POST /api/v1/payment-receipts/{id}/cancel â€” Cancels receipt. Finance/Admin. | `API/Controllers/PaymentReceiptController.cs` |
| `GetFeeStatus(studentId, ct)` | GET /api/v1/payment-receipts/fee-status/{id} â€” Outstanding fee flag. | `API/Controllers/PaymentReceiptController.cs` |

### API â€” RegistrationImportController
| Function | Description | File |
|---|---|---|
| `ImportCsv(file, ct)` | POST /api/v1/registration-import/csv â€” Imports CSV of registration numbers. Admin. | `API/Controllers/RegistrationImportController.cs` |
| `AddSingle(request, ct)` | POST /api/v1/registration-import/single â€” Adds one registration number. Admin. | `API/Controllers/RegistrationImportController.cs` |

### API â€” AccountSecurityController
| Function | Description | File |
|---|---|---|
| `GetLocked(ct)` | GET /api/v1/account-security/locked â€” Returns all locked accounts. Admin+. | `API/Controllers/AccountSecurityController.cs` |
| `GetLockoutStatus(userId, ct)` | GET /api/v1/account-security/{userId}/lockout-status â€” Status for user. Admin+. | `API/Controllers/AccountSecurityController.cs` |
| `Unlock(userId, ct)` | POST /api/v1/account-security/{userId}/unlock â€” Unlocks an account. Admin/SuperAdmin; role rules enforced. | `API/Controllers/AccountSecurityController.cs` |
| `ResetPassword(userId, request, ct)` | POST /api/v1/account-security/{userId}/reset-password â€” Admin resets password. Admin+. | `API/Controllers/AccountSecurityController.cs` |

---

## Phase 9: Dashboard, Navigation & System Settings (Sprints 17â€“18)

### Domain â€” Timetable aggregate
| Function | Description | File |
|---|---|---|
| `Timetable(departmentId, semesterId, name)` | Constructor; creates a draft timetable for a dept/semester. | `Domain/Entities/Timetable.cs` |
| `Publish()` | Sets `IsPublished = true`; timetable becomes visible to students/faculty. | `Domain/Entities/Timetable.cs` |
| `Unpublish()` | Reverts `IsPublished = false`; hides from students/faculty. | `Domain/Entities/Timetable.cs` |
| `TimetableEntry(timetableId, courseOfferingId, roomId, dayOfWeek, startTime, endTime)` | Constructor; adds a single slot to a timetable. | `Domain/Entities/TimetableEntry.cs` |

### Domain â€” Settings entities
| Function | Description | File |
|---|---|---|
| `ReportDefinition(key, name, purpose)` | Constructor; creates a report definition record. | `Domain/Entities/ReportDefinition.cs` |
| `ReportRoleAssignment(reportKey, roleName, isAllowed)` | Constructor; grants or denies a role access to a report. | `Domain/Entities/ReportRoleAssignment.cs` |
| `ModuleRoleAssignment(moduleKey, roleName, isAllowed)` | Constructor; controls per-role module visibility. | `Domain/Entities/ModuleRoleAssignment.cs` |
| `SidebarMenuItem(key, name, purpose, displayOrder, parentId)` | Constructor; creates a navigable sidebar entry. | `Domain/Entities/SidebarMenuItem.cs` |
| `Activate()` | Sets `IsActive = true` on the sidebar menu item. | `Domain/Entities/SidebarMenuItem.cs` |
| `Deactivate()` | Sets `IsActive = false`; hidden from all non-SuperAdmin roles. | `Domain/Entities/SidebarMenuItem.cs` |
| `SidebarMenuRoleAccess(sidebarMenuItemId, roleName, isAllowed)` | Constructor; per-role access record for a menu item. | `Domain/Entities/SidebarMenuRoleAccess.cs` |
| `SetAllowed(isAllowed)` | Updates the `IsAllowed` flag for this role-access record. | `Domain/Entities/SidebarMenuRoleAccess.cs` |

### Infrastructure â€” TimetableRepository
| Function | Description | File |
|---|---|---|
| `GetByIdAsync(id, ct)` | Returns a timetable with entries. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `GetByDepartmentAsync(deptId, semesterId, ct)` | Lists timetables for a department and semester. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `CreateAsync(timetable, ct)` | Persists a new timetable. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `UpdateAsync(timetable, ct)` | Persists timetable metadata changes. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `DeleteAsync(id, ct)` | Deletes a timetable and its entries. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `PublishAsync(id, ct)` | Calls `Timetable.Publish`; persists. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `UnpublishAsync(id, ct)` | Calls `Timetable.Unpublish`; persists. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `AddEntryAsync(entry, ct)` | Persists a new `TimetableEntry`. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `UpdateEntryAsync(entry, ct)` | Updates a timetable entry (room, time, day). | `Infrastructure/Repositories/TimetableRepository.cs` |
| `DeleteEntryAsync(entryId, ct)` | Removes a single timetable slot. | `Infrastructure/Repositories/TimetableRepository.cs` |
| `ExportToExcelAsync(id, ct)` | Returns Excel byte array for a timetable (ClosedXML). | `Infrastructure/Repositories/TimetableRepository.cs` |
| `ExportToPdfAsync(id, ct)` | Returns PDF byte array for a timetable (QuestPDF). | `Infrastructure/Repositories/TimetableRepository.cs` |

### Infrastructure â€” BuildingRoomRepository
| Function | Description | File |
|---|---|---|
| `GetAllAsync(ct)` | Returns all buildings with rooms. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `GetBuildingAsync(id, ct)` | Returns a single building with rooms. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `CreateBuildingAsync(building, ct)` | Creates a new building. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `UpdateBuildingAsync(building, ct)` | Updates building name. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `DeleteBuildingAsync(id, ct)` | Deletes a building (and its rooms if empty). | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `CreateRoomAsync(room, ct)` | Creates a room inside a building. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `UpdateRoomAsync(room, ct)` | Updates room details (name, capacity). | `Infrastructure/Repositories/BuildingRoomRepository.cs` |
| `DeleteRoomAsync(id, ct)` | Deletes a room. | `Infrastructure/Repositories/BuildingRoomRepository.cs` |

### Infrastructure â€” SettingsRepository (sidebar & settings methods)
| Function | Description | File |
|---|---|---|
| `GetSidebarMenusAsync(ct)` | Returns all top-level sidebar items with their `SidebarMenuRoleAccess` collection. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `GetSubMenusAsync(parentId, ct)` | Returns sub-menu items for a given parent ID. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `GetSidebarMenuByIdAsync(id, ct)` | Returns a single sidebar menu item with role access. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `GetVisibleSidebarMenusForRoleAsync(roleName, ct)` | Returns sidebar items where `IsActive=true` and the role's `IsAllowed=true`; SuperAdmin bypasses filter. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `SetSidebarMenuRolesAsync(menuId, roleAssignments, ct)` | Replaces all `SidebarMenuRoleAccess` rows for a menu item. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `SetSidebarMenuStatusAsync(menuId, isActive, ct)` | Sets `IsActive` on a menu item; throws `DomainException` if `IsSystemMenu=true`. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `GetReportDefinitionsAsync(ct)` | Returns all report definitions with role assignments. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `SetReportRolesAsync(reportKey, roleAssignments, ct)` | Replaces role assignments for a report. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `GetModuleRolesAsync(moduleKey, ct)` | Returns role assignments for a module. | `Infrastructure/Repositories/SettingsRepository.cs` |
| `SetModuleRolesAsync(moduleKey, roleAssignments, ct)` | Replaces role assignments for a module. | `Infrastructure/Repositories/SettingsRepository.cs` |

### Infrastructure â€” TimetableExcelExporter / TimetablePdfExporter
| Function | Description | File |
|---|---|---|
| `ExportAsync(timetable, ct)` | Generates a ClosedXML Excel workbook for a timetable; returns byte array. | `Infrastructure/Exporters/TimetableExcelExporter.cs` |
| `ExportAsync(timetable, ct)` | Generates a QuestPDF landscape A4 PDF for a timetable; returns byte array. | `Infrastructure/Exporters/TimetablePdfExporter.cs` |

### Infrastructure â€” DatabaseSeeder (extended)
| Function | Description | File |
|---|---|---|
| `SeedSidebarMenusAsync(context, ct)` | Seeds default 11 sidebar menu items and their per-role `SidebarMenuRoleAccess` rows on first run. | `Infrastructure/Seeding/DatabaseSeeder.cs` |

### Application â€” TimetableService
| Function | Description | File |
|---|---|---|
| `GetTimetablesAsync(deptId, semesterId, ct)` | Returns timetables for a dept/semester. | `Application/Timetable/TimetableService.cs` |
| `GetTimetableAsync(id, ct)` | Returns full timetable detail with entries. | `Application/Timetable/TimetableService.cs` |
| `CreateTimetableAsync(request, ct)` | Creates a draft timetable. Admin. | `Application/Timetable/TimetableService.cs` |
| `UpdateTimetableAsync(id, request, ct)` | Updates timetable metadata. Admin. | `Application/Timetable/TimetableService.cs` |
| `DeleteTimetableAsync(id, ct)` | Deletes a timetable. Admin. | `Application/Timetable/TimetableService.cs` |
| `PublishAsync(id, ct)` | Publishes a timetable. Admin. | `Application/Timetable/TimetableService.cs` |
| `UnpublishAsync(id, ct)` | Unpublishes a timetable. Admin. | `Application/Timetable/TimetableService.cs` |
| `AddEntryAsync(id, request, ct)` | Adds a slot to a timetable. Admin. | `Application/Timetable/TimetableService.cs` |
| `UpdateEntryAsync(entryId, request, ct)` | Updates a timetable slot. Admin. | `Application/Timetable/TimetableService.cs` |
| `DeleteEntryAsync(entryId, ct)` | Removes a timetable slot. Admin. | `Application/Timetable/TimetableService.cs` |
| `ExportExcelAsync(id, ct)` | Returns Excel export for a timetable. | `Application/Timetable/TimetableService.cs` |
| `ExportPdfAsync(id, ct)` | Returns PDF export for a timetable. | `Application/Timetable/TimetableService.cs` |

### Application â€” BuildingRoomService
| Function | Description | File |
|---|---|---|
| `GetAllAsync(ct)` | Returns all buildings with rooms. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `GetBuildingAsync(id, ct)` | Returns a single building. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `CreateBuildingAsync(request, ct)` | Creates a new building. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `UpdateBuildingAsync(id, request, ct)` | Updates building name. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `DeleteBuildingAsync(id, ct)` | Deletes a building. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `CreateRoomAsync(buildingId, request, ct)` | Creates a room. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `UpdateRoomAsync(roomId, request, ct)` | Updates room details. | `Application/BuildingRoom/BuildingRoomService.cs` |
| `DeleteRoomAsync(roomId, ct)` | Deletes a room. | `Application/BuildingRoom/BuildingRoomService.cs` |

### Application â€” ReportSettingsService
| Function | Description | File |
|---|---|---|
| `GetAllReportsAsync(ct)` | Returns all report definitions with role assignments. | `Application/Settings/ReportSettingsService.cs` |
| `GetReportAsync(key, ct)` | Returns a single report definition. | `Application/Settings/ReportSettingsService.cs` |
| `SetReportRolesAsync(key, roleAssignments, ct)` | Updates role access for a report. SuperAdmin only. | `Application/Settings/ReportSettingsService.cs` |
| `ExportReportsExcelAsync(ct)` | Exports report definitions to Excel. | `Application/Settings/ReportSettingsService.cs` |
| `ExportReportsPdfAsync(ct)` | Exports report definitions to PDF. | `Application/Settings/ReportSettingsService.cs` |
| `GetActiveReportKeysForRoleAsync(roleName, ct)` | Returns report keys available for a given role. | `Application/Settings/ReportSettingsService.cs` |
| `MapToReportDto(report)` | Maps ReportDefinition to DTO. | `Application/Settings/ReportSettingsService.cs` |

### Application â€” ModuleRolesService
| Function | Description | File |
|---|---|---|
| `GetModuleRolesAsync(moduleKey, ct)` | Returns role assignments for a module. | `Application/Settings/ModuleRolesService.cs` |
| `SetModuleRolesAsync(moduleKey, roleAssignments, ct)` | Updates role access for a module. SuperAdmin only. | `Application/Settings/ModuleRolesService.cs` |

### Application â€” ThemeService
| Function | Description | File |
|---|---|---|
| `GetThemeAsync(userId, ct)` | Returns the user's current theme key. | `Application/Theme/ThemeService.cs` |
| `SetThemeAsync(userId, themeKey, ct)` | Persists the user's theme selection. | `Application/Theme/ThemeService.cs` |

### Application â€” SidebarMenuService
| Function | Description | File |
|---|---|---|
| `GetTopLevelMenusAsync(ct)` | Returns all top-level sidebar menu items with role access data. | `Application/Sidebar/SidebarMenuService.cs` |
| `GetSubMenusAsync(parentId, ct)` | Returns sub-menu items for a parent menu item. | `Application/Sidebar/SidebarMenuService.cs` |
| `GetByIdAsync(id, ct)` | Returns a single sidebar menu item with role access. | `Application/Sidebar/SidebarMenuService.cs` |
| `GetVisibleForRoleAsync(roleName, ct)` | Returns items visible to a role (active + allowed); SuperAdmin sees all. | `Application/Sidebar/SidebarMenuService.cs` |
| `SetRolesAsync(menuId, roleAssignments, ct)` | Replaces role assignments for a menu item; validates not locked system menu. | `Application/Sidebar/SidebarMenuService.cs` |
| `SetStatusAsync(menuId, isActive, ct)` | Toggles menu item status; returns 409 Conflict if `IsSystemMenu=true`. | `Application/Sidebar/SidebarMenuService.cs` |

### API â€” TimetableController
| Function | Description | File |
|---|---|---|
| `GetAll(deptId, semesterId, ct)` | GET /api/v1/timetables â€” Lists timetables. | `API/Controllers/TimetableController.cs` |
| `GetById(id, ct)` | GET /api/v1/timetables/{id} â€” Returns full timetable detail. | `API/Controllers/TimetableController.cs` |
| `Create(request, ct)` | POST /api/v1/timetables â€” Creates a draft timetable. Admin. | `API/Controllers/TimetableController.cs` |
| `Update(id, request, ct)` | PUT /api/v1/timetables/{id} â€” Updates timetable metadata. Admin. | `API/Controllers/TimetableController.cs` |
| `Delete(id, ct)` | DELETE /api/v1/timetables/{id} â€” Deletes timetable. Admin. | `API/Controllers/TimetableController.cs` |
| `Publish(id, ct)` | POST /api/v1/timetables/{id}/publish â€” Publishes. Admin. | `API/Controllers/TimetableController.cs` |
| `Unpublish(id, ct)` | POST /api/v1/timetables/{id}/unpublish â€” Unpublishes. Admin. | `API/Controllers/TimetableController.cs` |
| `AddEntry(id, request, ct)` | POST /api/v1/timetables/{id}/entries â€” Adds a slot. Admin. | `API/Controllers/TimetableController.cs` |
| `UpdateEntry(entryId, request, ct)` | PUT /api/v1/timetables/entries/{entryId} â€” Updates a slot. Admin. | `API/Controllers/TimetableController.cs` |
| `DeleteEntry(entryId, ct)` | DELETE /api/v1/timetables/entries/{entryId} â€” Removes a slot. Admin. | `API/Controllers/TimetableController.cs` |
| `ExportExcel(id, ct)` | GET /api/v1/timetables/{id}/export/excel â€” Excel export. | `API/Controllers/TimetableController.cs` |
| `ExportPdf(id, ct)` | GET /api/v1/timetables/{id}/export/pdf â€” PDF export. | `API/Controllers/TimetableController.cs` |

### API â€” BuildingRoomController
| Function | Description | File |
|---|---|---|
| `GetAll(ct)` | GET /api/v1/buildings â€” Returns all buildings with rooms. | `API/Controllers/BuildingRoomController.cs` |
| `GetBuilding(id, ct)` | GET /api/v1/buildings/{id} â€” Single building. | `API/Controllers/BuildingRoomController.cs` |
| `CreateBuilding(request, ct)` | POST /api/v1/buildings â€” Admin. | `API/Controllers/BuildingRoomController.cs` |
| `UpdateBuilding(id, request, ct)` | PUT /api/v1/buildings/{id} â€” Admin. | `API/Controllers/BuildingRoomController.cs` |
| `DeleteBuilding(id, ct)` | DELETE /api/v1/buildings/{id} â€” Admin. | `API/Controllers/BuildingRoomController.cs` |
| `CreateRoom(buildingId, request, ct)` | POST /api/v1/buildings/{id}/rooms â€” Admin. | `API/Controllers/BuildingRoomController.cs` |
| `UpdateRoom(roomId, request, ct)` | PUT /api/v1/buildings/rooms/{roomId} â€” Admin. | `API/Controllers/BuildingRoomController.cs` |
| `DeleteRoom(roomId, ct)` | DELETE /api/v1/buildings/rooms/{roomId} â€” Admin. | `API/Controllers/BuildingRoomController.cs` |

### API â€” ReportSettingsController
| Function | Description | File |
|---|---|---|
| `GetAll(ct)` | GET /api/v1/settings/reports â€” Lists all report definitions. SuperAdmin. | `API/Controllers/ReportSettingsController.cs` |
| `GetReport(key, ct)` | GET /api/v1/settings/reports/{key} â€” Single report. SuperAdmin. | `API/Controllers/ReportSettingsController.cs` |
| `SetRoles(key, request, ct)` | PUT /api/v1/settings/reports/{key}/roles â€” Updates role access. SuperAdmin. | `API/Controllers/ReportSettingsController.cs` |
| `ExportExcel(ct)` | GET /api/v1/settings/reports/export/excel â€” Excel export. SuperAdmin. | `API/Controllers/ReportSettingsController.cs` |
| `ExportPdf(ct)` | GET /api/v1/settings/reports/export/pdf â€” PDF export. SuperAdmin. | `API/Controllers/ReportSettingsController.cs` |
| `GetForRole(role, ct)` | GET /api/v1/settings/reports/for-role/{role} â€” Active reports for a role. | `API/Controllers/ReportSettingsController.cs` |
| `GetCurrentUserId()` | Extracts user ID from JWT. | `API/Controllers/ReportSettingsController.cs` |

### API â€” ModuleController (extended)
| Function | Description | File |
|---|---|---|
| `GetModuleRoles(key, ct)` | GET /api/v1/modules/{key}/roles â€” Returns role assignments for a module. SuperAdmin. | `API/Controllers/ModuleController.cs` |
| `SetModuleRoles(key, request, ct)` | PUT /api/v1/modules/{key}/roles â€” Updates role access for a module. SuperAdmin. | `API/Controllers/ModuleController.cs` |

### API â€” ThemeController
| Function | Description | File |
|---|---|---|
| `GetTheme(ct)` | GET /api/v1/theme â€” Returns current user's theme key. Authenticated. | `API/Controllers/ThemeController.cs` |
| `SetTheme(request, ct)` | PUT /api/v1/theme â€” Persists user's theme selection. Authenticated. | `API/Controllers/ThemeController.cs` |
| `GetCurrentUserId()` | Extracts user ID from JWT. | `API/Controllers/ThemeController.cs` |

### API â€” SidebarMenuController
| Function | Description | File |
|---|---|---|
| `GetMyVisible(ct)` | GET /api/v1/sidebar-menu/my-visible â€” Returns visible menus for the calling user's role. Authenticated. | `API/Controllers/SidebarMenuController.cs` |
| `GetAll(ct)` | GET /api/v1/sidebar-menu â€” Returns all top-level sidebar items with role access. SuperAdmin. | `API/Controllers/SidebarMenuController.cs` |
| `GetById(id, ct)` | GET /api/v1/sidebar-menu/{id} â€” Single menu item. SuperAdmin. | `API/Controllers/SidebarMenuController.cs` |
| `GetSubMenus(id, ct)` | GET /api/v1/sidebar-menu/{id}/sub-menus â€” Sub-menus for parent. SuperAdmin. | `API/Controllers/SidebarMenuController.cs` |
| `SetRoles(id, request, ct)` | PUT /api/v1/sidebar-menu/{id}/roles â€” Updates role visibility. SuperAdmin. | `API/Controllers/SidebarMenuController.cs` |
| `SetStatus(id, request, ct)` | PUT /api/v1/sidebar-menu/{id}/status â€” Toggles active/inactive; 409 if system menu. SuperAdmin. | `API/Controllers/SidebarMenuController.cs` |

### Web â€” EduApiClient (sidebar methods)
| Function | Description | File |
|---|---|---|
| `GetSidebarMenusAsync(ct)` | GET /api/v1/sidebar-menu â€” Fetches all top-level menus for the settings table. | `Web/Services/EduApiClient.cs` |
| `GetVisibleSidebarMenusForCurrentUserAsync(ct)` | GET /api/v1/sidebar-menu/my-visible â€” Fetches visible menus for layout rendering. | `Web/Services/EduApiClient.cs` |
| `GetSidebarSubMenusAsync(parentId, ct)` | GET /api/v1/sidebar-menu/{id}/sub-menus â€” Fetches sub-menus for a parent. | `Web/Services/EduApiClient.cs` |
| `SetSidebarMenuRolesAsync(menuId, request, ct)` | PUT /api/v1/sidebar-menu/{id}/roles â€” Updates role visibility settings. | `Web/Services/EduApiClient.cs` |
| `SetSidebarMenuStatusAsync(menuId, request, ct)` | PUT /api/v1/sidebar-menu/{id}/status â€” Toggles menu item active/inactive. | `Web/Services/EduApiClient.cs` |

### Web â€” PortalController (sidebar settings actions)
| Function | Description | File |
|---|---|---|
| `SidebarSettings(ct)` | GET /portal/settings/sidebar â€” Loads sidebar settings view with top-level menu table. SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `UpdateSidebarMenuRoles(id, request, ct)` | POST /portal/settings/sidebar/{id}/roles â€” Updates role access from form; CSRF-protected. SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `UpdateSidebarMenuStatus(id, request, ct)` | POST /portal/settings/sidebar/{id}/status â€” Toggles menu item status from form; CSRF-protected. SuperAdmin. | `Web/Controllers/PortalController.cs` |


---

## Integration Tests (tests/Tabsan.EduSphere.IntegrationTests)

### EduSphereWebFactory (Infrastructure/EduSphereWebFactory.cs)
| Function | Description | File |
|---|---|---|
| `InitializeAsync()` | `IAsyncLifetime` — drops `TabsanEduSphere_IntegrationTests` DB via standalone context before factory builds; ensures clean state for every run. | `tests/.../Infrastructure/EduSphereWebFactory.cs` |
| `DisposeAsync()` | Drops test DB after all tests in the fixture complete; releases resources. | `tests/.../Infrastructure/EduSphereWebFactory.cs` |
| `BuildStandaloneContext()` | Creates a standalone `ApplicationDbContext` targeting the test connection string; used for pre-run DB drop outside the factory lifecycle. | `tests/.../Infrastructure/EduSphereWebFactory.cs` |
| `ConfigureWebHost(builder)` | Overrides connection string to test DB; removes all `IHostedService` registrations to prevent background job interference. | `tests/.../Infrastructure/EduSphereWebFactory.cs` |

### JwtTestHelper (Infrastructure/JwtTestHelper.cs)
| Function | Description | File |
|---|---|---|
| `GenerateToken(role, userId, email)` | Generates a signed JWT for any system role using the same secret/issuer/audience as the API; returns Bearer token string for test HTTP client auth headers. | `tests/.../Infrastructure/JwtTestHelper.cs` |

### SidebarMenuIntegrationTests (SidebarMenuIntegrationTests.cs)
| Test | Assertion | File |
|---|---|---|
| `GetVisible_SuperAdmin_ReturnsAllMenus` | SuperAdmin receives all 13 seeded menu keys via `GET my-visible`. | `tests/.../SidebarMenuIntegrationTests.cs` |
| `GetVisible_Admin_ReturnsAdminMenusOnly` | Admin receives exactly 7 keys (dashboard, timetable_admin, lookups, buildings, rooms, system_settings, theme_settings). | `tests/.../SidebarMenuIntegrationTests.cs` |
| `GetVisible_Faculty_ReturnsFacultyMenusOnly` | Faculty receives exactly 4 keys (dashboard, timetable_teacher, system_settings, theme_settings). | `tests/.../SidebarMenuIntegrationTests.cs` |
| `GetVisible_Student_ReturnsStudentMenusOnly` | Student receives exactly 4 keys (dashboard, timetable_student, system_settings, theme_settings). | `tests/.../SidebarMenuIntegrationTests.cs` |
| `SetStatus_DisableTimetableTeacher_RemovesFromFaculty_ThenRestore` | Deactivating a menu item removes it from Faculty visible; re-activating restores it. | `tests/.../SidebarMenuIntegrationTests.cs` |
| `SetRoles_DenyStudent_RemovesFromStudentVisible_ThenRestore` | Revoking Student role access removes menu from student visible; restore re-adds it. | `tests/.../SidebarMenuIntegrationTests.cs` |
| `SetStatus_SystemMenu_DeactivateAttempt_Returns409Conflict` | Attempting to deactivate a system menu returns `409 Conflict`. | `tests/.../SidebarMenuIntegrationTests.cs` |
| `GetVisible_NoToken_Returns401` | Unauthenticated request to `my-visible` returns `401 Unauthorized`. | `tests/.../SidebarMenuIntegrationTests.cs` |

---

## Phase 9 Web UI — Web Layer (Completed 30 April 2026)

### API — LicenseController (Phase 9 additions)
| Function | Description | File |
|---|---|---|
| `Details(ct)` | GET /api/v1/license/details — Returns full license detail (status, licenseType, activatedAt, expiresAt, updatedAt, remainingDays). Roles: SuperAdmin, Admin. | `API/Controllers/LicenseController.cs` |
| `Upload(file, ct)` | POST /api/v1/license/upload — Accepts `.tablic` file upload and activates. Role: SuperAdmin only. | `API/Controllers/LicenseController.cs` |

### API — ModuleController (Phase 9 all-settings endpoint)
| Function | Description | File |
|---|---|---|
| `AllSettings(ct)` | GET /api/v1/modules/all-settings — Returns `List<ModuleSettingsDto>` with activation state + role assignments for all modules. Role: SuperAdmin. | `API/Controllers/ModuleController.cs` |

### Application — ModuleSettingsDto
| Type | Description | File |
|---|---|---|
| `ModuleSettingsDto` | Record: `(Id, Key, Name, IsMandatory, IsActive, AssignedRoles)` — used by the all-settings endpoint. | `Application/DTOs/SettingsDtos.cs` |

### Infrastructure — LicenseValidationService (Phase 9 addition)
| Function | Description | File |
|---|---|---|
| `GetCurrentStateAsync(ct)` | Returns the raw `LicenseState?` from the license repository; used by the details endpoint. | `Infrastructure/Licensing/LicenseValidationService.cs` |

### Infrastructure — DatabaseSeeder (Phase 9 idempotent update)
| Function | Description | File |
|---|---|---|
| `SeedSidebarMenusAsync(db)` | Rewritten to upsert-by-key (fully idempotent). Adds `license_update` (SuperAdmin only) and `theme_settings` (all roles) entries. Now seeds 13 sidebar items total. | `Infrastructure/Persistence/DatabaseSeeder.cs` |

### Web — EduApiClient (Phase 9 UI methods)
| Function | Description | File |
|---|---|---|
| `GetLicenseDetailsAsync(ct)` | GET /api/v1/license/details — Returns license status, type, dates, remaining days. | `Web/Services/EduApiClient.cs` |
| `UploadLicenseAsync(fileStream, fileName, ct)` | POST /api/v1/license/upload — Uploads a `.tablic` license file. | `Web/Services/EduApiClient.cs` |
| `GetCurrentThemeAsync(ct)` | GET /api/v1/theme — Returns the current user's theme key. | `Web/Services/EduApiClient.cs` |
| `SetThemeAsync(themeKey, ct)` | PUT /api/v1/theme — Persists the user's theme selection. | `Web/Services/EduApiClient.cs` |
| `GetReportDefinitionsAsync(ct)` | GET /api/v1/settings/reports — Returns all report definitions with role assignments. | `Web/Services/EduApiClient.cs` |
| `CreateReportDefinitionAsync(form, ct)` | POST /api/v1/settings/reports — Creates a new report definition. | `Web/Services/EduApiClient.cs` |
| `SetReportActiveAsync(id, activate, ct)` | POST /api/v1/settings/reports/{id}/activate or /deactivate — Toggles report visibility. | `Web/Services/EduApiClient.cs` |
| `SetReportRolesAsync(id, roles, ct)` | PUT /api/v1/settings/reports/{id}/roles — Updates role access for a report. | `Web/Services/EduApiClient.cs` |
| `GetModuleSettingsAsync(ct)` | GET /api/v1/modules/all-settings — Returns all modules with activation + role data. | `Web/Services/EduApiClient.cs` |
| `SetModuleActiveAsync(key, activate, ct)` | POST /api/v1/modules/{key}/activate or /deactivate — Toggles module activation. | `Web/Services/EduApiClient.cs` |
| `SetModuleRolesAsync(key, roles, ct)` | PUT /api/v1/modules/{key}/roles — Updates role assignments for a module. | `Web/Services/EduApiClient.cs` |

### Web — PortalViewModels (Phase 9 additions)
| Type | Description | File |
|---|---|---|
| `LicenseUpdatePageModel` | ViewModel for LicenseUpdate view: status, licenseType, activatedAt, expiresAt, updatedAt, remainingDays. | `Web/Models/Portal/PortalViewModels.cs` |
| `ThemeOption` | Value type: (Key, Label, PrimaryColor, SecondaryColor, AccentColor) for one theme swatch. | `Web/Models/Portal/PortalViewModels.cs` |
| `ThemeSettingsPageModel` | ViewModel for ThemeSettings view: CurrentThemeKey + list of 15 ThemeOption entries. | `Web/Models/Portal/PortalViewModels.cs` |
| `ReportDefinitionWebModel` | ViewModel for a single report row: Id, Key, Name, Purpose, IsActive, AdminAllowed, FacultyAllowed, StudentAllowed. | `Web/Models/Portal/PortalViewModels.cs` |
| `ReportSettingsPageModel` | ViewModel for ReportSettings view: list of ReportDefinitionWebModel. | `Web/Models/Portal/PortalViewModels.cs` |
| `CreateReportForm` | Form model: Name, Key, Purpose. | `Web/Models/Portal/PortalViewModels.cs` |
| `ModuleSettingsWebModel` | ViewModel for a single module row: Id, Key, Name, IsMandatory, IsActive, AdminAllowed, FacultyAllowed, StudentAllowed. | `Web/Models/Portal/PortalViewModels.cs` |
| `ModuleSettingsPageModel` | ViewModel for ModuleSettings view: list of ModuleSettingsWebModel. | `Web/Models/Portal/PortalViewModels.cs` |

### Web — PortalController (Phase 9 UI actions)
| Function | Description | File |
|---|---|---|
| `LicenseUpdate(ct)` | GET /portal/settings/license-update — Loads license status + upload form. Roles: SuperAdmin, Admin. | `Web/Controllers/PortalController.cs` |
| `UploadLicense(licenseFile, ct)` | POST /portal/settings/license-upload — Uploads `.tablic` file; max 64 KB. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `ThemeSettings(ct)` | GET /portal/settings/theme — Loads theme picker page with current selection. Authenticated. | `Web/Controllers/PortalController.cs` |
| `SetTheme(themeKey, ct)` | POST /portal/settings/set-theme — Persists theme selection via API. Authenticated. | `Web/Controllers/PortalController.cs` |
| `ReportSettings(ct)` | GET /portal/settings/reports — Loads report definitions list with role controls. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `CreateReport(form, ct)` | POST /portal/settings/reports/create — Creates a new report definition. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `ToggleReport(id, activate, ct)` | POST /portal/settings/reports/{id}/toggle — Activates or deactivates a report. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `UpdateReportRoles(id, adminAllowed, facultyAllowed, studentAllowed, ct)` | POST /portal/settings/reports/{id}/roles — Updates role access for a report. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `ModuleSettings(ct)` | GET /portal/settings/modules — Loads module settings list with activation + role controls. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `ToggleModule(key, activate, ct)` | POST /portal/settings/modules/{key}/toggle — Activates or deactivates a module (non-mandatory only). Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |
| `UpdateModuleRoles(key, adminAllowed, facultyAllowed, studentAllowed, ct)` | POST /portal/settings/modules/{key}/roles — Updates role assignments for a module. Role: SuperAdmin. | `Web/Controllers/PortalController.cs` |

### Web — _Layout.cshtml (Phase 9 sidebar routes)
| Addition | Description | File |
|---|---|---|
| `"license_update"` route | `ResolveRoute` switch case maps to `("Portal", "LicenseUpdate")`. | `Web/Views/Shared/_Layout.cshtml` |
| `"theme_settings"` route | `ResolveRoute` switch case maps to `("Portal", "ThemeSettings")`. | `Web/Views/Shared/_Layout.cshtml` |

### Web — Views (Phase 9 new views)
| View | Description | File |
|---|---|---|
| `LicenseUpdate.cshtml` | Displays license status table (Status badge, LicenseType, ActivatedAt, ExpiresAt + remaining days, UpdatedAt) and upload form (`.tablic` only, 64 KB max). SuperAdmin can upload; Admin sees read-only view. | `Web/Views/Portal/LicenseUpdate.cshtml` |
| `ThemeSettings.cshtml` | Color swatch grid (90×70 px buttons, one per theme). JS `previewTheme()` applies `data-theme` on click for live preview. Hidden form input saves selection on submit. | `Web/Views/Portal/ThemeSettings.cshtml` |
| `ReportSettings.cshtml` | Collapsible create-report form + accordion list of all report definitions. Each row has activate/deactivate toggle and role checkbox form (Admin / Faculty / Student). | `Web/Views/Portal/ReportSettings.cshtml` |
| `ModuleSettings.cshtml` | Accordion list of all modules. Each row shows Mandatory badge, activate/deactivate button (disabled for mandatory modules), and role checkbox form. | `Web/Views/Portal/ModuleSettings.cshtml` |

### CSS — site.css (Phase 9 themes)
| Addition | Description | File |
|---|---|---|
| 15 CSS theme definitions | Themes: `ocean_blue`, `emerald_forest`, `sunset_orange`, `royal_purple`, `midnight_dark`, `rose_gold`, `arctic_teal`, `sand_dune`, `slate_grey`, `crimson`, `ivory_classic`, `cobalt_night`, `olive_grove`, `cosmic_violet`, plus default. Each uses `[data-theme="key"]` with 15 CSS custom property overrides. | `Web/wwwroot/css/site.css` |
