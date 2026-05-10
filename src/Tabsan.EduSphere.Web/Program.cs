using Tabsan.EduSphere.Web.Services;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;

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
var dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("Tabsan.EduSphere.Web");
var sharedKeyRingPath = builder.Configuration["ScaleOut:SharedDataProtectionKeyRingPath"];
if (!string.IsNullOrWhiteSpace(sharedKeyRingPath))
{
    Directory.CreateDirectory(sharedKeyRingPath);
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(sharedKeyRingPath));
}
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("EduApi");
builder.Services.AddScoped<IEduApiClient, EduApiClient>();

if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
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

if (!app.Environment.IsDevelopment())
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
