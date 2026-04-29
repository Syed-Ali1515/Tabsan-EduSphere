namespace Tabsan.EduSphere.Domain.Common;

/// <summary>
/// Extends BaseEntity with soft-delete capability.
/// Soft-deleted records are hidden from normal queries but never physically removed,
/// satisfying the requirement that academic data is never permanently deleted.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>When true the record is logically deleted and excluded from normal queries.</summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>UTC timestamp of the soft-delete operation. Null when not deleted.</summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>Marks the entity as soft-deleted. Data is preserved in the database.</summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Restores a previously soft-deleted entity back to active state.</summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        Touch();
    }
}
