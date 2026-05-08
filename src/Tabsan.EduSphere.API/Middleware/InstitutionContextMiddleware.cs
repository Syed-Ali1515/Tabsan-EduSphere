using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Middleware;

/// <summary>
/// Phase 23 Stage 23.2 — Institution Context Resolution.
/// Resolves the current <see cref="InstitutionPolicySnapshot"/> once per HTTP request
/// and stores it in <c>HttpContext.Items["InstitutionPolicy"]</c>.
/// All downstream controllers and services can read the resolved context without
/// triggering additional database or cache lookups.
/// </summary>
public sealed class InstitutionContextMiddleware
{
    private readonly RequestDelegate _next;

    public InstitutionContextMiddleware(RequestDelegate next)
        => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var policyService = context.RequestServices
            .GetService<IInstitutionPolicyService>();

        if (policyService is not null)
        {
            var snapshot = await policyService.GetPolicyAsync(context.RequestAborted);
            context.Items[InstitutionContextKeys.PolicyKey] = snapshot;
        }

        await _next(context);
    }
}

/// <summary>Well-known keys used to read institution context from <c>HttpContext.Items</c>.</summary>
public static class InstitutionContextKeys
{
    /// <summary>Key under which the <see cref="InstitutionPolicySnapshot"/> is stored.</summary>
    public const string PolicyKey = "InstitutionPolicy";

    /// <summary>
    /// Extension method for clean access from controllers and other middleware.
    /// Returns <see cref="InstitutionPolicySnapshot.Default"/> when the middleware
    /// has not populated the item (e.g. during unit tests).
    /// </summary>
    public static InstitutionPolicySnapshot GetInstitutionPolicy(this HttpContext context)
        => context.Items.TryGetValue(PolicyKey, out var value) && value is InstitutionPolicySnapshot snapshot
            ? snapshot
            : InstitutionPolicySnapshot.Default;
}
