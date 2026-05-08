using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 25 — Academic Engine Unification — Stage 25.1

/// <summary>
/// Resolves the appropriate <see cref="IResultCalculationStrategy"/> for a given
/// institution type. The resolver is injected by DI and owns a strategy per type.
/// </summary>
public interface IResultStrategyResolver
{
    /// <summary>
    /// Returns the result calculation strategy that should be used for the given
    /// institution type.
    /// </summary>
    IResultCalculationStrategy Resolve(InstitutionType institutionType);
}
