using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for module management operations available to the Super Admin.
/// </summary>
public interface IModuleService
{
    /// <summary>Returns all module definitions with their current activation status.</summary>
    Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Activates the named module and records the change in the audit log.</summary>
    Task ActivateAsync(string moduleKey, Guid changedByUserId, CancellationToken ct = default);

    /// <summary>
    /// Deactivates the named module.
    /// Throws InvalidOperationException if the module is mandatory.
    /// </summary>
    Task DeactivateAsync(string moduleKey, Guid changedByUserId, CancellationToken ct = default);
}
