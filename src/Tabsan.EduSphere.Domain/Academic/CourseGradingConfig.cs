using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Final-Touches Phase 19 Stage 19.4 — per-course grading configuration entity

/// <summary>
/// Stores the per-course grading configuration: pass threshold, grading type,
/// and JSON-serialised grade-range mapping (e.g. 90–100 → A+).
/// </summary>
public class CourseGradingConfig : BaseEntity
{
    /// <summary>FK to the course this configuration belongs to (unique one-to-one).</summary>
    public Guid CourseId { get; private set; }

    /// <summary>Navigation to the course.</summary>
    public Course Course { get; private set; } = default!;

    /// <summary>Minimum mark / GPA required to pass the course.</summary>
    public decimal PassThreshold { get; private set; }

    /// <summary>Grading method: "GPA", "Percentage", or "Grade".</summary>
    public string GradingType { get; private set; } = "GPA";

    /// <summary>
    /// JSON array of grade range objects, e.g.:
    /// [{"From":90,"To":100,"Label":"A+"},{"From":80,"To":89,"Label":"A"},...]
    /// Null means the global grading config applies.
    /// </summary>
    public string? GradeRangesJson { get; private set; }

    private CourseGradingConfig() { }

    public CourseGradingConfig(Guid courseId, decimal passThreshold, string gradingType, string? gradeRangesJson)
    {
        CourseId = courseId;
        PassThreshold = passThreshold;
        GradingType = gradingType;
        GradeRangesJson = gradeRangesJson;
    }

    // Final-Touches Phase 19 Stage 19.4 — update grading config
    /// <summary>Updates the grading configuration.</summary>
    public void Update(decimal passThreshold, string gradingType, string? gradeRangesJson)
    {
        PassThreshold = passThreshold;
        GradingType = gradingType;
        GradeRangesJson = gradeRangesJson;
        Touch();
    }
}
