using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.StudyPlanner;

// Final-Touches Phase 21 Stage 21.1 — Study Planner: planned course line item

/// <summary>
/// A single course entry within a <see cref="StudyPlan"/>.
/// Represents the student's intent to take this course in the planned semester.
/// </summary>
public class StudyPlanCourse : BaseEntity
{
    /// <summary>FK to the parent study plan.</summary>
    public Guid StudyPlanId { get; private set; }

    /// <summary>FK to the course the student plans to take.</summary>
    public Guid CourseId { get; private set; }

    /// <summary>Navigation to the course definition.</summary>
    public Course Course { get; private set; } = default!;

    private StudyPlanCourse() { }

    internal StudyPlanCourse(Guid studyPlanId, Guid courseId)
    {
        StudyPlanId = studyPlanId;
        CourseId    = courseId;
    }
}
