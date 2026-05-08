using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>
/// Institution-level grading profile that defines the default pass threshold and
/// grade band mapping for each institution type (University / School / College).
/// SuperAdmin manages these profiles; per-course overrides still use <see cref="CourseGradingConfig"/>.
/// There is at most one active profile per <see cref="InstitutionType"/>.
/// </summary>
public class InstitutionGradingProfile : BaseEntity
{
    /// <summary>The institution type this profile applies to.</summary>
    public InstitutionType InstitutionType { get; private set; }

    /// <summary>
    /// Minimum score required to pass a course under this profile.
    /// For University: interpreted as minimum GPA (0.0–4.0, e.g. 2.0).
    /// For School/College: interpreted as minimum percentage (0–100, e.g. 40).
    /// </summary>
    public decimal PassThreshold { get; private set; }

    /// <summary>
    /// Optional JSON array of grade band objects used by percentage-mode strategies:
    /// [{"From":90,"To":100,"Label":"A+"},{"From":80,"To":89,"Label":"A"},...]
    /// Null means the strategy uses its own built-in defaults.
    /// For University (GPA) mode this field is not used.
    /// </summary>
    public string? GradeRangesJson { get; private set; }

    /// <summary>Whether this profile is active and applied during result calculations.</summary>
    public bool IsActive { get; private set; } = true;

    private InstitutionGradingProfile() { }

    public InstitutionGradingProfile(InstitutionType institutionType, decimal passThreshold, string? gradeRangesJson)
    {
        InstitutionType = institutionType;
        ValidateAndSet(passThreshold, gradeRangesJson);
    }

    /// <summary>Updates the threshold and grade bands. Called by SuperAdmin.</summary>
    public void Update(decimal passThreshold, string? gradeRangesJson, bool isActive)
    {
        ValidateAndSet(passThreshold, gradeRangesJson);
        IsActive = isActive;
        Touch();
    }

    private void ValidateAndSet(decimal passThreshold, string? gradeRangesJson)
    {
        // University: GPA threshold 0.0–4.0. School/College: percentage 0–100.
        var maxThreshold = InstitutionType == InstitutionType.University ? 4.0m : 100m;
        if (passThreshold < 0 || passThreshold > maxThreshold)
            throw new ArgumentOutOfRangeException(nameof(passThreshold),
                $"Pass threshold must be between 0 and {maxThreshold} for {InstitutionType}.");

        PassThreshold = passThreshold;
        GradeRangesJson = gradeRangesJson;
    }
}
