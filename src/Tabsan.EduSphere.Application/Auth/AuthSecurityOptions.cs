namespace Tabsan.EduSphere.Application.Auth;

// Phase 27.2 — Authentication and Security UX configuration contract.
public sealed class AuthSecurityOptions
{
    public const string SectionName = "AuthSecurity";

    public MfaSettings Mfa { get; init; } = new();
    public SsoSettings Sso { get; init; } = new();
    public SessionRiskSettings SessionRisk { get; init; } = new();
}

public sealed class MfaSettings
{
    public bool Enabled { get; init; }
    public bool RequireForPasswordLogin { get; init; }
    public string DemoCode { get; init; } = "000000";
}

public sealed class SsoSettings
{
    public bool Enabled { get; init; }
    public string Provider { get; init; } = "";
    public string LoginUrl { get; init; } = "";
}

public sealed class SessionRiskSettings
{
    public bool Enabled { get; init; } = true;
    public bool BlockHighRiskLogin { get; init; } = true;
    public bool AuditMediumRiskLogin { get; init; } = true;
}
