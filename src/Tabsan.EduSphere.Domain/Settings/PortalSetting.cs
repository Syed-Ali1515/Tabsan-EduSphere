using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// A key-value pair that stores institution-wide portal branding and configuration.
/// SuperAdmin can read and write any key.  All other roles only read (via the API).
/// </summary>
public class PortalSetting : BaseEntity
{
    /// <summary>Stable string key, e.g. "university_name", "brand_initials", "portal_subtitle".</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Stored value.  May be empty string (never null) when cleared.</summary>
    public string Value { get; private set; } = string.Empty;

    // Required by EF Core
    private PortalSetting() { }

    public PortalSetting(string key, string value)
    {
        Key   = key.Trim().ToLowerInvariant();
        Value = value ?? string.Empty;
    }

    public void SetValue(string value)
    {
        Value     = value ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }
}
