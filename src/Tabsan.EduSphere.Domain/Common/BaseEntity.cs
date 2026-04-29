namespace Tabsan.EduSphere.Domain.Common;

/// <summary>
/// Base class for all domain entities that use a GUID as primary key.
/// Provides shared audit timestamps and a row version for optimistic concurrency.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Primary key — GUID ensures safe distributed generation.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>UTC timestamp recorded when the entity is first persisted.</summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp updated on every write. Nullable until first update.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// EF Core concurrency token.
    /// Automatically incremented by SQL Server on every UPDATE,
    /// preventing lost-update conflicts under concurrent access.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Timestamp]
    public byte[]? RowVersion { get; protected set; }

    /// <summary>Marks UpdatedAt to the current UTC time. Called before EF SaveChanges.</summary>
    public void Touch() => UpdatedAt = DateTime.UtcNow;
}
