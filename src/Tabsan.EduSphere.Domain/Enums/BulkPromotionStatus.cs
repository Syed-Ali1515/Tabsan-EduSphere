namespace Tabsan.EduSphere.Domain.Enums;

// Phase 26 — School and College Functional Expansion — Stage 26.2

/// <summary>Lifecycle status of a bulk promotion batch.</summary>
public enum BulkPromotionStatus
{
    /// <summary>Batch is being composed; entries can still be added or removed.</summary>
    Draft = 0,

    /// <summary>Batch has been submitted for Admin/SuperAdmin approval.</summary>
    AwaitingApproval = 1,

    /// <summary>Batch has been approved and is ready to be applied.</summary>
    Approved = 2,

    /// <summary>Batch was rejected by an approver.</summary>
    Rejected = 3,

    /// <summary>Batch has been applied — students' semester counters have been advanced.</summary>
    Applied = 4,
}
