using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.3

/// <summary>
/// Links a parent user account to a student profile for read-only portal access.
/// A parent can be linked to multiple students; a student can be linked to multiple guardians.
/// </summary>
public class ParentStudentLink : AuditableEntity
{
    /// <summary>FK to the parent user account (Role = Parent).</summary>
    public Guid ParentUserId { get; private set; }

    /// <summary>FK to the linked student profile.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Optional relationship descriptor (Father/Mother/Guardian).</summary>
    public string? Relationship { get; private set; }

    /// <summary>Controls whether the link is currently active.</summary>
    public bool IsActive { get; private set; } = true;

    private ParentStudentLink() { }

    public ParentStudentLink(Guid parentUserId, Guid studentProfileId, string? relationship)
    {
        ParentUserId = parentUserId;
        StudentProfileId = studentProfileId;
        Relationship = relationship?.Trim();
    }

    public void Update(string? relationship, bool isActive)
    {
        Relationship = relationship?.Trim();
        IsActive = isActive;
        Touch();
    }
}
