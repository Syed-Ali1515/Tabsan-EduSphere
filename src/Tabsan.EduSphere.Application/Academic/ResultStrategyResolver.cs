using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.1

/// <summary>
/// Default implementation of <see cref="IResultStrategyResolver"/>.
/// Holds one pre-instantiated strategy per institution type and returns the
/// appropriate one based on the requested <see cref="InstitutionType"/>.
/// </summary>
public sealed class ResultStrategyResolver : IResultStrategyResolver
{
    private readonly GpaResultStrategy _universityStrategy = new();
    private readonly PercentageResultStrategy _schoolStrategy = new(InstitutionType.School);
    private readonly PercentageResultStrategy _collegeStrategy = new(InstitutionType.College);

    /// <summary>
    /// Returns the strategy for the given institution type.
    /// University → <see cref="GpaResultStrategy"/> (GPA/CGPA 0.0–4.0).
    /// School or College → <see cref="PercentageResultStrategy"/> (Percentage + grade bands).
    /// </summary>
    public IResultCalculationStrategy Resolve(InstitutionType institutionType)
        => institutionType switch
        {
            InstitutionType.University => _universityStrategy,
            InstitutionType.School    => _schoolStrategy,
            InstitutionType.College   => _collegeStrategy,
            _ => _universityStrategy // safe default
        };
}
