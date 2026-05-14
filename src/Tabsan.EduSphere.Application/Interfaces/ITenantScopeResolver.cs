namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Resolves the current tenant scope key for SaaS-safe settings isolation.
/// Returns null/empty when no explicit tenant scope is available.
/// </summary>
public interface ITenantScopeResolver
{
    string? GetTenantScopeKey();
}
