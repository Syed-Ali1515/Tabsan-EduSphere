using Tabsan.EduSphere.Web.Services;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("EduApi");
builder.Services.AddScoped<IEduApiClient, EduApiClient>();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "Tabsan.EduSphere.Web.Session";
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();

// Build a request principal from session identity so User.IsInRole works in Razor/views.
app.Use(async (context, next) =>
{
    const string identityKey = "SessionIdentityJson";
    var raw = context.Session.GetString(identityKey);

    if (!string.IsNullOrWhiteSpace(raw))
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            var claims = new List<Claim>();

            if (root.TryGetProperty("UserName", out var userNameEl) &&
                userNameEl.GetString() is { Length: > 0 } userName)
            {
                claims.Add(new Claim(ClaimTypes.Name, userName));
            }

            if (root.TryGetProperty("Email", out var emailEl) &&
                emailEl.GetString() is { Length: > 0 } email)
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            if (root.TryGetProperty("Roles", out var rolesEl) && rolesEl.ValueKind == JsonValueKind.Array)
            {
                foreach (var roleEl in rolesEl.EnumerateArray())
                {
                    if (roleEl.GetString() is not { Length: > 0 } role)
                        continue;

                    claims.Add(new Claim(ClaimTypes.Role, role));
                    claims.Add(new Claim("role", role));
                }
            }

            if (claims.Count > 0)
            {
                var identity = new ClaimsIdentity(claims, authenticationType: "SessionJwt");
                context.User = new ClaimsPrincipal(identity);
            }
        }
        catch
        {
            // Ignore malformed session identity and continue without overriding principal.
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
