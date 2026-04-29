namespace Tabsan.EduSphere.Domain.Licensing;

/// <summary>
/// Defines the two supported license lifecycles.
/// Yearly licenses expire after 12 months and require renewal.
/// Permanent licenses never expire once activated.
/// </summary>
public enum LicenseType
{
    Yearly = 1,
    Permanent = 2
}

/// <summary>
/// Describes the current state of the activated license on the application side.
/// </summary>
public enum LicenseStatus
{
    /// <summary>License is valid and all licensed features are accessible.</summary>
    Active = 1,

    /// <summary>
    /// License has passed its expiry date.
    /// System enters graceful degradation (read-only) mode automatically.
    /// </summary>
    Expired = 2,

    /// <summary>License file failed signature verification or was tampered with.</summary>
    Invalid = 3
}
