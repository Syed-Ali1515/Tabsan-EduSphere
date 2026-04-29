using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository interface for reading module definitions and their activation states.
/// </summary>
public interface IModuleRepository
{
    /// <summary>Returns all modules with their current ModuleStatus, including mandatory ones.</summary>
    Task<IReadOnlyList<Module>> GetAllWithStatusAsync(CancellationToken ct = default);

    /// <summary>Returns the ModuleStatus for a given module key (e.g. "assignment", "quiz").</summary>
    Task<ModuleStatus?> GetStatusByKeyAsync(string moduleKey, CancellationToken ct = default);

    /// <summary>Returns true when the named module is currently active.</summary>
    Task<bool> IsActiveAsync(string moduleKey, CancellationToken ct = default);

    /// <summary>Persists changes to a ModuleStatus row.</summary>
    void UpdateStatus(ModuleStatus status);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
