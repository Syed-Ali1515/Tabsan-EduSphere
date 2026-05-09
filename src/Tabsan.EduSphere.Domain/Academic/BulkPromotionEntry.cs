using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.2

/// <summary>
/// Individual student row inside a <see cref="BulkPromotionBatch"/>.
/// Each entry captures whether the student is promoted or held back.
/// </summary>
public class BulkPromotionEntry : BaseEntity
{
    /// <summary>FK to the parent promotion batch.</summary>
    public Guid BatchId { get; private set; }

    /// <summary>FK to the student profile to be processed.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Promote or Hold decision for this student.</summary>
    public EntryDecision Decision { get; private set; }

    /// <summary>Optional explanation for a hold/review decision.</summary>
    public string? Reason { get; private set; }

    /// <summary>Whether this entry has been applied during batch execution.</summary>
    public bool IsApplied { get; private set; }

    /// <summary>UTC timestamp when this entry was applied.</summary>
    public DateTime? AppliedAt { get; private set; }

    private BulkPromotionEntry() { }

    internal BulkPromotionEntry(Guid batchId, Guid studentProfileId, EntryDecision decision)
    {
        BatchId = batchId;
        StudentProfileId = studentProfileId;
        Decision = decision;
    }

    /// <summary>Updates decision/reason while batch is still mutable.</summary>
    public void UpdateDecision(EntryDecision decision, string? reason)
    {
        Decision = decision;
        Reason = reason;
        Touch();
    }

    /// <summary>Marks this entry as applied after processing.</summary>
    public void MarkApplied()
    {
        IsApplied = true;
        AppliedAt = DateTime.UtcNow;
        Touch();
    }
}
