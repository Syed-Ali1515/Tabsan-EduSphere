using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.2

/// <summary>
/// Represents a batch bulk-promotion operation for a group of students.
/// Admin creates a draft batch, optionally seeks approval, then applies it — which
/// advances all "Promote" entries by one semester/grade/year.
/// </summary>
public class BulkPromotionBatch : AuditableEntity
{
    private readonly List<BulkPromotionEntry> _entries = new();

    /// <summary>Human-readable label for this batch (e.g. "Grade 9 → 10 — 2026 Promotion").</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Current lifecycle status of the batch.</summary>
    public BulkPromotionStatus Status { get; private set; } = BulkPromotionStatus.Draft;

    /// <summary>User ID of the Admin who created this batch.</summary>
    public Guid CreatedByUserId { get; private set; }

    /// <summary>User ID of the approver. Null until approved or rejected.</summary>
    public Guid? ApprovedByUserId { get; private set; }

    /// <summary>UTC timestamp when the batch was approved or rejected.</summary>
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>UTC timestamp when the batch was applied to the database.</summary>
    public DateTime? AppliedAt { get; private set; }

    /// <summary>Optional note from the approver / rejector.</summary>
    public string? ReviewNote { get; private set; }

    /// <summary>Read-only view of the individual student entries.</summary>
    public IReadOnlyList<BulkPromotionEntry> Entries => _entries.AsReadOnly();

    private BulkPromotionBatch() { }

    public BulkPromotionBatch(string title, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Batch title is required.", nameof(title));

        Title = title.Trim();
        CreatedByUserId = createdByUserId;
    }

    /// <summary>Adds a student entry to a draft batch. Duplicate student IDs are rejected.</summary>
    public void AddEntry(Guid studentProfileId, EntryDecision decision)
    {
        if (Status != BulkPromotionStatus.Draft)
            throw new InvalidOperationException("Entries can only be added to a Draft batch.");

        if (_entries.Any(e => e.StudentProfileId == studentProfileId))
            throw new InvalidOperationException($"Student {studentProfileId} is already in this batch.");

        _entries.Add(new BulkPromotionEntry(Id, studentProfileId, decision));
    }

    /// <summary>Submits the draft batch for approval.</summary>
    public void Submit()
    {
        if (Status != BulkPromotionStatus.Draft)
            throw new InvalidOperationException("Only a Draft batch can be submitted.");
        if (_entries.Count == 0)
            throw new InvalidOperationException("Cannot submit an empty batch.");

        Status = BulkPromotionStatus.AwaitingApproval;
        Touch();
    }

    /// <summary>Approves the batch. Called by Admin/SuperAdmin.</summary>
    public void Approve(Guid approverUserId, string? note)
    {
        if (Status != BulkPromotionStatus.AwaitingApproval)
            throw new InvalidOperationException("Only a batch awaiting approval can be approved.");

        Status = BulkPromotionStatus.Approved;
        ApprovedByUserId = approverUserId;
        ReviewNote = note;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Rejects the batch. Batch returns to Draft state for amendment.</summary>
    public void Reject(Guid reviewerUserId, string reason)
    {
        if (Status != BulkPromotionStatus.AwaitingApproval)
            throw new InvalidOperationException("Only a batch awaiting approval can be rejected.");

        Status = BulkPromotionStatus.Rejected;
        ApprovedByUserId = reviewerUserId;
        ReviewNote = reason;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Marks the batch as Applied after all Promote entries have been processed.</summary>
    public void MarkApplied()
    {
        if (Status != BulkPromotionStatus.Approved)
            throw new InvalidOperationException("Only an Approved batch can be marked as Applied.");

        Status = BulkPromotionStatus.Applied;
        AppliedAt = DateTime.UtcNow;
        Touch();
    }
}
