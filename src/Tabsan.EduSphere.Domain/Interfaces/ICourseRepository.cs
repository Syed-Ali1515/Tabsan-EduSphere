using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for Course catalogue and CourseOffering operations.</summary>
public interface ICourseRepository
{
    /// <summary>Returns all course definitions, optionally filtered by department.</summary>
    Task<IReadOnlyList<Course>> GetAllAsync(Guid? departmentId = null, CancellationToken ct = default);

    /// <summary>Returns the course with the given ID, or null.</summary>
    Task<Course?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns true when the course code is already taken within the given department.</summary>
    Task<bool> CodeExistsAsync(string code, Guid departmentId, CancellationToken ct = default);

    /// <summary>Queues the course for insertion.</summary>
    Task AddAsync(Course course, CancellationToken ct = default);

    /// <summary>Marks the course as modified.</summary>
    void Update(Course course);

    // ── Course Offerings ─────────────────────────────────────────────────────

    // Final-Touches Phase 8 Stage 8.1 — return all offerings when no filter applied
    /// <summary>Returns all course offerings with Course and Semester navigation loaded.</summary>
    Task<IReadOnlyList<CourseOffering>> GetAllOfferingsAsync(CancellationToken ct = default);

    /// <summary>Returns all offerings for the given semester, with Course and Semester loaded.</summary>
    Task<IReadOnlyList<CourseOffering>> GetOfferingsBySemesterAsync(Guid semesterId, CancellationToken ct = default);

    /// <summary>Returns all offerings for the given department (filtered by course.departmentId).</summary>
    Task<IReadOnlyList<CourseOffering>> GetOfferingsByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns all offerings assigned to the given faculty user.</summary>
    Task<IReadOnlyList<CourseOffering>> GetOfferingsByFacultyAsync(Guid facultyUserId, CancellationToken ct = default);

    /// <summary>Returns the offering with the given ID, or null.</summary>
    Task<CourseOffering?> GetOfferingByIdAsync(Guid offeringId, CancellationToken ct = default);

    /// <summary>Returns the current enrollment count for the given offering.</summary>
    Task<int> GetEnrollmentCountAsync(Guid offeringId, CancellationToken ct = default);

    /// <summary>Queues the offering for insertion.</summary>
    Task AddOfferingAsync(CourseOffering offering, CancellationToken ct = default);

    /// <summary>Marks the offering as modified.</summary>
    void UpdateOffering(CourseOffering offering);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
