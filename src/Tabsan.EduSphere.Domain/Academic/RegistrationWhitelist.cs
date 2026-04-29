using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// The type of identifier stored on a RegistrationWhitelist entry.
/// </summary>
public enum WhitelistIdentifierType
{
    /// <summary>Email address match — used when the institution issues pre-admission emails.</summary>
    Email = 1,

    /// <summary>Student roll/registration number — used when the student ID is known before account creation.</summary>
    RegistrationNumber = 2
}

/// <summary>
/// Controls which prospective students are permitted to self-register on the portal.
/// An Admin pre-loads this table with the incoming cohort's identifiers.
/// During self-registration the system checks that the provided email or
/// registration number exists and has not already been consumed.
/// </summary>
public class RegistrationWhitelist : BaseEntity
{
    /// <summary>Whether the identifier is an email or a registration number.</summary>
    public WhitelistIdentifierType IdentifierType { get; private set; }

    /// <summary>The actual email or registration number string (case-insensitive lookup).</summary>
    public string IdentifierValue { get; private set; } = default!;

    /// <summary>FK — limits this whitelist entry to a specific department.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>FK to the programme the registering student should be placed in.</summary>
    public Guid ProgramId { get; private set; }

    /// <summary>True once the student has successfully registered using this entry.</summary>
    public bool IsUsed { get; private set; }

    /// <summary>UTC timestamp of when the entry was consumed by a successful registration.</summary>
    public DateTime? UsedAt { get; private set; }

    /// <summary>FK to the User record created from this entry. Set when IsUsed becomes true.</summary>
    public Guid? CreatedUserId { get; private set; }

    private RegistrationWhitelist() { }

    public RegistrationWhitelist(WhitelistIdentifierType identifierType, string identifierValue,
                                  Guid departmentId, Guid programId)
    {
        IdentifierType = identifierType;
        IdentifierValue = identifierValue.Trim().ToLowerInvariant();
        DepartmentId = departmentId;
        ProgramId = programId;
    }

    /// <summary>
    /// Marks this entry as consumed after a successful student self-registration.
    /// Once marked, the entry cannot be reused (prevents duplicate accounts).
    /// </summary>
    public void MarkUsed(Guid createdUserId)
    {
        if (IsUsed)
            throw new InvalidOperationException("This whitelist entry has already been used.");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        CreatedUserId = createdUserId;
    }
}
