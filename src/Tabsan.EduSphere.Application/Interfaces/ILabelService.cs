using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Institution-mode-aware academic vocabulary.
/// Labels change based on whether the tenant operates as University, School, or College.
/// </summary>
public sealed record AcademicVocabulary(
    /// <summary>Semester / Grade / Year</summary>
    string PeriodLabel,
    /// <summary>Progression / Promotion</summary>
    string ProgressionLabel,
    /// <summary>GPA/CGPA / Percentage</summary>
    string GradingLabel,
    /// <summary>Course / Subject</summary>
    string CourseLabel,
    /// <summary>Batch / Class / Year-Group</summary>
    string StudentGroupLabel
)
{
    /// <summary>Default vocabulary (University mode).</summary>
    public static AcademicVocabulary Default { get; } =
        new("Semester", "Progression", "GPA/CGPA", "Course", "Batch");
}

/// <summary>
/// Returns the academic vocabulary appropriate for the current institution policy.
/// Used to replace hardcoded labels in views and API responses.
/// </summary>
public interface ILabelService
{
    /// <summary>
    /// Returns vocabulary terms for the given institution policy.
    /// When both School and College are enabled alongside University the University
    /// vocabulary is returned (broadest common-denominator).
    /// </summary>
    AcademicVocabulary GetVocabulary(InstitutionPolicySnapshot policy);
}
