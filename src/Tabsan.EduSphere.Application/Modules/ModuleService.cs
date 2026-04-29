using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Application.Modules;

/// <summary>
/// Orchestrates module activation / deactivation from the Super Admin panel.
/// Enforces the mandatory-module guard before any toggle.
/// </summary>
public class ModuleService : IModuleService
{
    private readonly IModuleRepository _moduleRepo;
    private readonly IModuleEntitlementResolver _resolver;
    private readonly IAuditService _audit;

    public ModuleService(
        IModuleRepository moduleRepo,
        IModuleEntitlementResolver resolver,
        IAuditService audit)
    {
        _moduleRepo = moduleRepo;
        _resolver = resolver;
        _audit = audit;
    }

    /// <summary>Returns all module definitions with their current activation status loaded.</summary>
    public Task<IReadOnlyList<Module>> GetAllAsync(CancellationToken ct = default)
        => _moduleRepo.GetAllWithStatusAsync(ct);

    /// <summary>
    /// Activates the named module if it is currently inactive.
    /// Writes an audit log entry and clears the entitlement cache.
    /// </summary>
    public async Task ActivateAsync(string moduleKey, Guid changedByUserId, CancellationToken ct = default)
    {
        var status = await _moduleRepo.GetStatusByKeyAsync(moduleKey, ct)
            ?? throw new InvalidOperationException($"Module '{moduleKey}' not found.");

        status.Activate(changedByUserId);
        _moduleRepo.UpdateStatus(status);
        await _moduleRepo.SaveChangesAsync(ct);
        _resolver.InvalidateCache(moduleKey);

        await _audit.LogAsync(new AuditLog("ActivateModule", "ModuleStatus", status.Id.ToString(),
            actorUserId: changedByUserId), ct);
    }

    /// <summary>
    /// Deactivates the named module.
    /// Throws InvalidOperationException when the module is mandatory and cannot be toggled.
    /// </summary>
    public async Task DeactivateAsync(string moduleKey, Guid changedByUserId, CancellationToken ct = default)
    {
        var status = await _moduleRepo.GetStatusByKeyAsync(moduleKey, ct)
            ?? throw new InvalidOperationException($"Module '{moduleKey}' not found.");

        if (status.Module.IsMandatory)
            throw new InvalidOperationException($"Module '{moduleKey}' is mandatory and cannot be deactivated.");

        status.Deactivate(changedByUserId);
        _moduleRepo.UpdateStatus(status);
        await _moduleRepo.SaveChangesAsync(ct);
        _resolver.InvalidateCache(moduleKey);

        await _audit.LogAsync(new AuditLog("DeactivateModule", "ModuleStatus", status.Id.ToString(),
            actorUserId: changedByUserId), ct);
    }
}
