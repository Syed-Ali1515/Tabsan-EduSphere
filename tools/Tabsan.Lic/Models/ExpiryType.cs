namespace Tabsan.Lic.Models;

/// <summary>
/// Expiry options available when generating a new license key from Tabsan-Lic.
/// Maps to a concrete ExpiresAt date relative to the IssuedAt timestamp.
/// </summary>
public enum ExpiryType
{
    /// <summary>License expires one year from the issue date.</summary>
    OneYear = 1,

    /// <summary>License expires two years from the issue date.</summary>
    TwoYears = 2,

    /// <summary>License expires three years from the issue date.</summary>
    ThreeYears = 3,

    /// <summary>License never expires.</summary>
    Permanent = 4
}
