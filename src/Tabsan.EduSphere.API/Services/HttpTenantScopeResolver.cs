using System.Security.Claims;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

/// <summary>
/// Resolves tenant scope from authenticated claims or request headers.
/// Fallback is handled by consumers when no scope value is present.
/// </summary>
public sealed class HttpTenantScopeResolver : ITenantScopeResolver
{
    private static readonly string[] ClaimKeys =
    {
        "tenant_code",
        "tenantCode",
        "tenant",
        "tid"
    };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantScopeResolver(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetTenantScopeKey()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
            return null;

        var user = context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            foreach (var claimKey in ClaimKeys)
            {
                var value = user.FindFirstValue(claimKey);
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        if (context.Request.Headers.TryGetValue("X-Tenant-Code", out var headerValues))
        {
            var value = headerValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }
}
