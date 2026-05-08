using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Pure, stateless implementation of <see cref="ILabelService"/>.
/// No database access required — vocabulary is derived entirely from the policy snapshot.
/// </summary>
public sealed class LabelService : ILabelService
{
    /// <inheritdoc/>
    public AcademicVocabulary GetVocabulary(InstitutionPolicySnapshot policy)
    {
        // When University is enabled (alone or alongside others) keep University vocab
        // so existing modules that show "Semester", "GPA" etc. remain consistent.
        if (policy.IsEnabled(InstitutionType.University))
            return AcademicVocabulary.Default;

        if (policy.IsEnabled(InstitutionType.School))
            return new AcademicVocabulary(
                PeriodLabel:        "Grade",
                ProgressionLabel:   "Promotion",
                GradingLabel:       "Percentage",
                CourseLabel:        "Subject",
                StudentGroupLabel:  "Class");

        if (policy.IsEnabled(InstitutionType.College))
            return new AcademicVocabulary(
                PeriodLabel:        "Year",
                ProgressionLabel:   "Progression",
                GradingLabel:       "Percentage",
                CourseLabel:        "Subject",
                StudentGroupLabel:  "Year-Group");

        // Fallback to default (should not occur — policy IsValid check prevents all-false)
        return AcademicVocabulary.Default;
    }
}
