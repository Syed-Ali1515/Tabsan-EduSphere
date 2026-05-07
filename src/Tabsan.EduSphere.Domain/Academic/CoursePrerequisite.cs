using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Final-Touches Phase 15 Stage 15.1 — CoursePrerequisite entity: prerequisite relationship between courses
/// <summary>
/// Links a Course to another Course that must be passed before enrollment is allowed.
/// A course may have multiple prerequisites; each is stored as a separate row.
/// </summary>
public class CoursePrerequisite : BaseEntity
{
    /// <summary>FK to the course that requires the prerequisite.</summary>
    public Guid CourseId { get; private set; }

    /// <summary>Navigation to the course that requires the prerequisite.</summary>
    public Course Course { get; private set; } = default!;

    /// <summary>FK to the course that must be passed before enrollment in CourseId is permitted.</summary>
    public Guid PrerequisiteCourseId { get; private set; }

    /// <summary>Navigation to the prerequisite course.</summary>
    public Course PrerequisiteCourse { get; private set; } = default!;

    private CoursePrerequisite() { }

    public CoursePrerequisite(Guid courseId, Guid prerequisiteCourseId)
    {
        if (courseId == prerequisiteCourseId)
            throw new ArgumentException("A course cannot be its own prerequisite.", nameof(prerequisiteCourseId));

        CourseId = courseId;
        PrerequisiteCourseId = prerequisiteCourseId;
    }
}
