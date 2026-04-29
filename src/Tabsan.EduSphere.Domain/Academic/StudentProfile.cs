using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Extended academic profile for a Student user.
/// Holds registration number, programme, department, CGPA, and enrolment history.
/// There is exactly one StudentProfile per User account with the Student role.
/// </summary>
public class StudentProfile : AuditableEntity
{
    /// <summary>FK to the User identity record.</summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Unique registration / roll number issued at admission.
    /// Format is institution-defined (e.g. "FA22-BSCS-001").
    /// </summary>
    public string RegistrationNumber { get; private set; } = default!;

    /// <summary>FK to the degree programme the student is enrolled in.</summary>
    public Guid ProgramId { get; private set; }

    /// <summary>Navigation to the academic programme.</summary>
    public AcademicProgram Program { get; private set; } = default!;

    /// <summary>FK to the student's home department (denormalized from programme for query performance).</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>UTC date the student was formally admitted.</summary>
    public DateTime AdmissionDate { get; private set; }

    /// <summary>Current cumulative GPA. Updated by the grading service each time results are published.</summary>
    public decimal Cgpa { get; private set; }

    /// <summary>Current semester number the student is in (1-based).</summary>
    public int CurrentSemesterNumber { get; private set; } = 1;

    private StudentProfile() { }

    public StudentProfile(Guid userId, string registrationNumber, Guid programId, Guid departmentId, DateTime admissionDate)
    {
        UserId = userId;
        RegistrationNumber = registrationNumber;
        ProgramId = programId;
        DepartmentId = departmentId;
        AdmissionDate = admissionDate;
    }

    /// <summary>
    /// Updates the student's CGPA after result publication.
    /// CGPA must be between 0.0 and 4.0 on a standard scale.
    /// </summary>
    public void UpdateCgpa(decimal newCgpa)
    {
        if (newCgpa < 0 || newCgpa > 4.0m)
            throw new ArgumentOutOfRangeException(nameof(newCgpa), "CGPA must be between 0.0 and 4.0.");
        Cgpa = newCgpa;
        Touch();
    }

    /// <summary>Advances the student to the next semester number after semester completion.</summary>
    public void AdvanceSemester()
    {
        CurrentSemesterNumber++;
        Touch();
    }
}
