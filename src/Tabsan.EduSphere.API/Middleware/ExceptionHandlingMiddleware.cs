using System.Net;
using System.Text.Json;

namespace Tabsan.EduSphere.API.Middleware;

/// <summary>
/// Global exception handler. Catches all unhandled exceptions, logs them fully,
/// and returns a sanitised JSON error response without stack traces in production.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next;
        _logger = logger;
        _env    = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException or ArgumentNullException  => (HttpStatusCode.BadRequest,         "Bad Request"),
            UnauthorizedAccessException                 => (HttpStatusCode.Unauthorized,        "Unauthorized"),
            KeyNotFoundException                        => (HttpStatusCode.NotFound,            "Not Found"),
            NotSupportedException                       => (HttpStatusCode.MethodNotAllowed,    "Method Not Allowed"),
            OperationCanceledException                  => (HttpStatusCode.ServiceUnavailable,  "Request Cancelled"),
            _                                           => (HttpStatusCode.InternalServerError, "An unexpected error occurred")
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode  = (int)statusCode;

        var response = new Dictionary<string, object?>
        {
            ["type"]    = $"https://httpstatuses.com/{(int)statusCode}",
            ["title"]   = title,
            ["status"]  = (int)statusCode,
            ["traceId"] = context.TraceIdentifier
        };

        if (_env.IsDevelopment())
        {
            response["detail"]    = exception.Message;
            response["exception"] = exception.ToString();
        }

        var json = JsonSerializer.Serialize(response,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        return context.Response.WriteAsync(json);
    }
}
