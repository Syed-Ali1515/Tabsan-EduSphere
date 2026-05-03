namespace Tabsan.EduSphere.Application.DTOs.Academic;

// ── Programme DTOs ────────────────────────────────────────────────────────────

/// <summary>Request body for creating a new degree programme.</summary>
public sealed record CreateProgramRequest(string Name, string Code, Guid DepartmentId, int TotalSemesters);

/// <summary>Request body for updating a programme's name.</summary>
public sealed record UpdateProgramRequest(string Name);

// ── Semester DTOs ─────────────────────────────────────────────────────────────

/// <summary>Request body for creating a new semester.</summary>
public sealed record CreateSemesterRequest(string Name, DateTime StartDate, DateTime EndDate);

// ── Course DTOs ───────────────────────────────────────────────────────────────

/// <summary>Request body for adding a course to the catalogue.</summary>
public sealed record CreateCourseRequest(string Title, string Code, int CreditHours, Guid DepartmentId);

/// <summary>Request body for updating a course title.</summary>
public sealed record UpdateCourseTitleRequest(string NewTitle);

/// <summary>Request body for creating a course offering within a semester.</summary>
public sealed record CreateOfferingRequest(Guid CourseId, Guid SemesterId, int MaxEnrollment, Guid? FacultyUserId);

/// <summary>Request body for assigning faculty to an existing offering.</summary>
public sealed record AssignFacultyRequest(Guid FacultyUserId);

/// <summary>Request body for updating max enrollment for a course offering.</summary>
public sealed record UpdateMaxEnrollmentRequest(int NewMaxEnrollment);

// ── Enrollment DTOs ───────────────────────────────────────────────────────────

/// <summary>Request body for enrolling a student into a course offering.</summary>
public sealed record EnrollRequest(Guid CourseOfferingId);

/// <summary>Response returned after a successful enrollment action.</summary>
public sealed record EnrollmentResponse(
    Guid EnrollmentId,
    Guid CourseOfferingId,
    string CourseName,
    string SemesterName,
    string Status,
    DateTime EnrolledAt);

// ── Student DTOs ──────────────────────────────────────────────────────────────

/// <summary>Request body for creating a student profile (used by Admin after account creation).</summary>
public sealed record CreateStudentProfileRequest(
    Guid UserId,
    string RegistrationNumber,
    Guid ProgramId,
    Guid DepartmentId,
    DateTime AdmissionDate);

/// <summary>Request body for student self-registration — checked against the whitelist.</summary>
public sealed record StudentSelfRegisterRequest(
    string Username,
    string Password,
    string RegistrationNumberOrEmail,
    string? Email = null);

// ── Faculty Assignment DTOs ───────────────────────────────────────────────────

/// <summary>Request body for assigning a faculty member to a department.</summary>
public sealed record AssignFacultyToDepartmentRequest(Guid FacultyUserId, Guid DepartmentId);

// ── Whitelist DTOs ────────────────────────────────────────────────────────────

/// <summary>Single whitelist entry — used for both single add and bulk import.</summary>
public sealed record WhitelistEntryRequest(
    string IdentifierType,   // "Email" or "RegistrationNumber"
    string IdentifierValue,
    Guid DepartmentId,
    Guid ProgramId);
