using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Modules;

/// <summary>
/// Records the current activation state of one module.
/// There is exactly one ModuleStatus row per Module (enforced by a unique index).
/// Super Admin toggles go through the Application layer which updates this record.
/// </summary>
public class ModuleStatus : BaseEntity
{
    /// <summary>FK to the module whose status this row describes.</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>Navigation property.</summary>
    public Module Module { get; private set; } = default!;

    /// <summary>
    /// Controls UI visibility, API access, and background job execution for this module.
    /// Toggling to false preserves all existing data but makes it inaccessible.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>UTC timestamp of the last activation event.</summary>
    public DateTime? ActivatedAt { get; private set; }

    /// <summary>
    /// Tracks whether the status was set by a mandatory rule, a license entitlement,
    /// or a manual Super Admin action. Values: "mandatory", "license", "manual".
    /// </summary>
    public string Source { get; private set; } = "manual";

    /// <summary>FK of the Super Admin user who last changed this status. Null for seed/system changes.</summary>
    public Guid? ChangedBy { get; private set; }

    private ModuleStatus() { }

    public ModuleStatus(Guid moduleId, bool isActive, string source = "mandatory", Guid? changedBy = null)
    {
        ModuleId = moduleId;
        IsActive = isActive;
        Source = source;
        ChangedBy = changedBy;
        if (isActive) ActivatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the module and records who performed the action and when.
    /// </summary>
    public void Activate(Guid changedBy, string source = "manual")
    {
        IsActive = true;
        ActivatedAt = DateTime.UtcNow;
        Source = source;
        ChangedBy = changedBy;
        Touch();
    }

    /// <summary>
    /// Deactivates the module. Data is preserved; UI and API access are blocked.
    /// Throws if the parent module is mandatory — guard must be enforced before calling.
    /// </summary>
    public void Deactivate(Guid changedBy)
    {
        IsActive = false;
        Source = "manual";
        ChangedBy = changedBy;
        Touch();
    }
}
