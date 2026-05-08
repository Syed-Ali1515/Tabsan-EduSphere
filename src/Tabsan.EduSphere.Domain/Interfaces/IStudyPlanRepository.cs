using Tabsan.EduSphere.Domain.StudyPlanner;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Final-Touches Phase 21 Stage 21.1 — Study Planner: repository interface

/// <summary>Repository contract for StudyPlan and StudyPlanCourse persistence.</summary>
public interface IStudyPlanRepository
{
    /// <summary>Returns all study plans for the given student profile (with courses loaded).</summary>
    Task<IReadOnlyList<StudyPlan>> GetByStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns study plans for all students in a department (for faculty advisors).</summary>
    Task<IReadOnlyList<StudyPlan>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns the study plan by ID (with courses loaded), or null.</summary>
    Task<StudyPlan?> GetByIdAsync(Guid planId, CancellationToken ct = default);

    /// <summary>Queues the study plan for insertion.</summary>
    Task AddAsync(StudyPlan plan, CancellationToken ct = default);

    /// <summary>Marks the plan as modified.</summary>
    void Update(StudyPlan plan);

    /// <summary>Returns the total credit hours already planned by a student across all active plans for a semester name.</summary>
    Task<int> GetPlannedCreditHoursAsync(Guid studentProfileId, string semesterName, CancellationToken ct = default);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
