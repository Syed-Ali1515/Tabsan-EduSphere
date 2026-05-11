using Tabsan.EduSphere.Web.Services;
using System.Security.Claims;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment;
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

Console.WriteLine($"[Web] Environment: {env.EnvironmentName} | App: {env.ApplicationName}");

var eduApiBaseUrl = builder.Configuration["EduApi:BaseUrl"];
if (string.IsNullOrWhiteSpace(eduApiBaseUrl))
{
    throw new InvalidOperationException("EduApi:BaseUrl is required for Tabsan.EduSphere.Web startup.");
}
var useForwardedHeaders = builder.Configuration.GetValue<bool>("ReverseProxy:Enabled");
var configuredKnownProxies = builder.Configuration.GetSection("ReverseProxy:KnownProxies").Get<string[]>() ?? [];
if (useForwardedHeaders && !builder.Environment.IsDevelopment() && configuredKnownProxies.Length == 0)
{
    throw new InvalidOperationException("ReverseProxy is enabled but no known proxy IPs are configured in ReverseProxy:KnownProxies.");
}

// Add services to the container.
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});
// Final-Touches Phase 34 Stage 2.3 — require shared data-protection keys in production so auth cookies work across web nodes.
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("Tabsan.EduSphere.Web");
var sharedKeyRingPath = builder.Configuration["ScaleOut:SharedDataProtectionKeyRingPath"];
if (!string.IsNullOrWhiteSpace(sharedKeyRingPath))
{
    Directory.CreateDirectory(sharedKeyRingPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(sharedKeyRingPath));
}
else if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing"))
{
    throw new InvalidOperationException("ScaleOut:SharedDataProtectionKeyRingPath is required outside Development/Testing to keep web auth cookies valid across instances.");
}

// Final-Touches Phase 34 Stage 3.3 — transport tuning for keep-alive and HTTP/2-friendly connection handling.
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(20);
    options.Limits.Http2.KeepAlivePingDelay = TimeSpan.FromSeconds(30);
    options.Limits.Http2.KeepAlivePingTimeout = TimeSpan.FromSeconds(10);
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("EduApi");
builder.Services.AddScoped<IEduApiClient, EduApiClient>();

if (useForwardedHeaders)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.RequireHeaderSymmetry = builder.Configuration.GetValue("ReverseProxy:RequireHeaderSymmetry", true);
        options.ForwardLimit = builder.Configuration.GetValue<int?>("ReverseProxy:ForwardLimit") ?? 2;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();

        foreach (var proxyIp in configuredKnownProxies)
        {
            if (IPAddress.TryParse(proxyIp, out var parsedIp))
            {
                options.KnownProxies.Add(parsedIp);
            }
        }
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (useForwardedHeaders)
{
    app.UseForwardedHeaders();
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseRouting();

// Build a request principal from protected cookie identity so User.IsInRole works across stateless web nodes.
app.Use(async (context, next) =>
{
    var api = context.RequestServices.GetRequiredService<IEduApiClient>();
    var identity = api.GetSessionIdentity();

    if (identity is not null)
    {
        try
        {
            var claims = new List<Claim>();

            if (!string.IsNullOrWhiteSpace(identity.UserName))
            {
                claims.Add(new Claim(ClaimTypes.Name, identity.UserName));
            }

            if (!string.IsNullOrWhiteSpace(identity.Email))
            {
                claims.Add(new Claim(ClaimTypes.Email, identity.Email));
            }

            foreach (var role in identity.Roles)
            {
                if (string.IsNullOrWhiteSpace(role))
                    continue;

                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }

            if (claims.Count > 0)
            {
                var principalIdentity = new ClaimsIdentity(claims, authenticationType: "SessionJwt");
                context.User = new ClaimsPrincipal(principalIdentity);
            }
        }
        catch
        {
            // Ignore malformed identity cookies and continue without overriding principal.
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
