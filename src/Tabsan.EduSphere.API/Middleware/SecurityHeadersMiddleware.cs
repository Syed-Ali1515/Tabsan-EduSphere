namespace Tabsan.EduSphere.API.Middleware;

/// <summary>
/// Adds OWASP-recommended HTTP security headers to every response.
/// Headers applied:
/// - Strict-Transport-Security (HSTS)
/// - X-Content-Type-Options
/// - X-Frame-Options
/// - X-XSS-Protection
/// - Referrer-Policy
/// - Content-Security-Policy (restrictive default; relax per route if needed)
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Initialises the middleware with the next delegate in the pipeline.</summary>
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Adds security headers then calls the next middleware.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Enforce HTTPS for 1 year; include sub-domains.
        headers["Strict-Transport-Security"]  = "max-age=31536000; includeSubDomains";

        // Prevent MIME-type sniffing.
        headers["X-Content-Type-Options"]     = "nosniff";

        // Block the page from being embedded in frames (clickjacking protection).
        headers["X-Frame-Options"]            = "DENY";

        // Legacy XSS filter for older browsers.
        headers["X-XSS-Protection"]           = "1; mode=block";

        // Do not send a Referer header when navigating away from the site.
        headers["Referrer-Policy"]            = "strict-origin-when-cross-origin";

        // Restrictive CSP — allows same-origin resources only.
        // Adjust script-src / style-src if serving a Razor UI from the same origin.
        headers["Content-Security-Policy"]    =
            "default-src 'self'; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "form-action 'self'";

        // Disable features not needed in a university management portal.
        headers["Permissions-Policy"]         =
            "camera=(), microphone=(), geolocation=(), payment=()";

        await _next(context);
    }
}

/// <summary>Extension methods for registering <see cref="SecurityHeadersMiddleware"/>.</summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>Adds the security headers middleware to the application pipeline.</summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        => app.UseMiddleware<SecurityHeadersMiddleware>();
}
