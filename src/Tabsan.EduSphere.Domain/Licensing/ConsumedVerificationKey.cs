using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Licensing;

/// <summary>
/// Records a VerificationKey hash that has already been used to activate a license
/// in this EduSphere installation.
///
/// Prevents replay attacks: a .tablic file whose VerificationKeyHash is already present
/// in this table is rejected on upload.
///
/// There is one row per consumed key; rows are never deleted.
/// </summary>
public class ConsumedVerificationKey : BaseEntity
{
    /// <summary>
    /// SHA-256 hex-string of the VerificationKey token embedded in the .tablic payload.
    /// Matches <c>verificationKeyHash</c> field from the license payload.
    /// </summary>
    public string KeyHash { get; private set; } = default!;

    /// <summary>UTC timestamp when this key was consumed during license activation.</summary>
    public DateTime ConsumedAt { get; private set; }

    private ConsumedVerificationKey() { }

    /// <summary>Creates a new record marking the given hash as consumed.</summary>
    public ConsumedVerificationKey(string keyHash)
    {
        KeyHash    = keyHash;
        ConsumedAt = DateTime.UtcNow;
    }
}
