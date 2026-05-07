# ASP.NET Core Refactoring: Hosting Configuration & Security Hardening

**Date Created:** May 7, 2026  
**Goal:** Prepare Tabsan EduSphere for production deployment with proper environment-specific configuration and comprehensive security hardening.

---

## Overview

This refactoring is divided into two major sections:

- **Part A:** Hosting Configuration — Support Development (localhost) and Production environments
- **Part B:** Security Hardening — Implement industry best practices to protect against common web vulnerabilities

Both parts must work together seamlessly. The application should automatically detect its environment and apply appropriate settings without manual code changes.

---

# PART A: HOSTING CONFIGURATION

## Objective

Enable the ASP.NET Core application to:
1. Run locally with Development settings
2. Automatically switch to Production settings when deployed
3. Centralize all environment-specific configuration
4. Use IConfiguration for all settings (no hardcoded values)
5. Support multiple hosting scenarios (IIS, reverse proxy, standalone)

---

## Phase 1: Configuration Files Setup

### Stage 1.1 — Create Configuration Files

**Files to Create/Update:**

- `src/Tabsan.EduSphere.API/appsettings.json` (shared, base settings)
- `src/Tabsan.EduSphere.API/appsettings.Development.json` (localhost)
- `src/Tabsan.EduSphere.API/appsettings.Production.json` (live server)
- `src/Tabsan.EduSphere.BackgroundJobs/appsettings.json`
- `src/Tabsan.EduSphere.BackgroundJobs/appsettings.Development.json`
- `src/Tabsan.EduSphere.BackgroundJobs/appsettings.Production.json`

**Structure:**

```json
// appsettings.json (shared/common)
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "NOT_SET_USE_ENVIRONMENT_OVERRIDE"
  },
  "AppSettings": {
    "AppName": "Tabsan EduSphere",
    "AppVersion": "1.0.0"
  },
  "AllowedHosts": "*"
}
```

```json
// appsettings.Development.json (localhost)
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TabsanEduSphere;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "AppSettings": {
    "ApiBaseUrl": "https://localhost:5181",
    "WebBaseUrl": "https://localhost:5063",
    "EnableDetailedErrors": true,
    "EnableSwagger": true,
    "CorsOrigins": ["https://localhost:5063", "https://localhost:7063"]
  },
  "AllowedHosts": "localhost,127.0.0.1"
}
```

```json
// appsettings.Production.json (live server)
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "REPLACE_WITH_PRODUCTION_CONNECTION_STRING"
  },
  "AppSettings": {
    "ApiBaseUrl": "https://yourdomain.com/api",
    "WebBaseUrl": "https://yourdomain.com",
    "EnableDetailedErrors": false,
    "EnableSwagger": false,
    "CorsOrigins": ["https://yourdomain.com"],
    "AllowedHostsProduction": "yourdomain.com,www.yourdomain.com"
  },
  "AllowedHosts": "yourdomain.com;www.yourdomain.com"
}
```

**Key Settings:**
- Connection strings should NOT contain sensitive credentials in shared files
- Development uses `Trusted_Connection=true` for Windows auth (localhost)
- Production uses parameterized connection string (credentials via secrets/env vars)
- `EnableDetailedErrors` and `EnableSwagger` are disabled in production
- Different CORS origins for each environment

**Files Affected:**
- API: `appsettings*.json`
- BackgroundJobs: `appsettings*.json`
- Web: Create if not present

---

### Stage 1.2 — Configure Program.cs to Load Correct Environment Settings

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
var builder = WebApplicationBuilder.CreateBuilder(args);

// Explicitly set environment from ASPNETCORE_ENVIRONMENT
var env = builder.Environment;
Console.WriteLine($"🔧 Running in: {env.EnvironmentName}");

// Load configuration files in order of precedence
builder.Configuration
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();  // Override with environment variables

// Log which config files are loaded
Console.WriteLine($"✓ Configuration loaded from:");
Console.WriteLine($"  - appsettings.json");
Console.WriteLine($"  - appsettings.{env.EnvironmentName}.json");
Console.WriteLine($"  - Environment variables");

// Validate critical configuration is present
var connString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connString) || connString.Contains("NOT_SET"))
{
    throw new InvalidOperationException(
        "❌ DefaultConnection string is missing or not set. " +
        "Check appsettings.json and environment variables.");
}

Console.WriteLine($"✓ Connection string configured: {(connString.Contains("localhost") ? "Development (localhost)" : "Production")}");

// Add services
builder.Services.AddControllers();
builder.Services.AddScoped<ApplicationDbContext>();

// ... rest of service configuration
```

**Key Points:**
- Use `builder.Environment.EnvironmentName` to detect environment
- Load `appsettings.{EnvironmentName}.json` conditionally
- Add environment variables layer (highest precedence)
- Validate critical configuration on startup

---

### Stage 1.3 — Remove Hardcoded Values from Codebase

**Search for & Replace:**

| Hardcoded Value | Replacement | File(s) |
|---|---|---|
| `"localhost"` | `Configuration["AppSettings:ApiBaseUrl"]` | Controllers, Services |
| `"https://localhost:5181"` | `Configuration["AppSettings:ApiBaseUrl"]` | Web project, API client |
| `"Data Source=localhost"` | `Configuration["ConnectionStrings:DefaultConnection"]` | Program.cs only |
| File paths (e.g., `C:\uploads\`) | `Path.Combine(env.WebRootPath, "uploads")` | File services |
| API endpoints | `Configuration["AppSettings:ApiBaseUrl"]` | Web project |
| Domain names | `Configuration["AppSettings:WebBaseUrl"]` | Email services, links |

**Affected Files:**
- `src/Tabsan.EduSphere.API/Program.cs`
- `src/Tabsan.EduSphere.API/Controllers/*`
- `src/Tabsan.EduSphere.Web/Services/EduApiClient.cs`
- `src/Tabsan.EduSphere.Web/Controllers/*`
- `src/Tabsan.EduSphere.Application/Services/*`
- `src/Tabsan.EduSphere.Infrastructure/Persistence/ApplicationDbContext.cs`

**Example Changes:**

```csharp
// BEFORE
private readonly string _apiUrl = "https://localhost:5181/api";

// AFTER
private readonly string _apiUrl;

public MyService(IConfiguration configuration)
{
    _apiUrl = configuration["AppSettings:ApiBaseUrl"] + "/api";
}
```

---

## Phase 2: Database Connection & Environment Awareness

### Stage 2.1 — Configure Database Connection String

**File:** `src/Tabsan.EduSphere.Infrastructure/Persistence/ApplicationDbContext.cs`

**Changes:**

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    if (!optionsBuilder.IsConfigured)
    {
        // This should not be called in normal flow (configured in Program.cs)
        // But keep for backward compatibility
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=TabsanEduSphere;Trusted_Connection=true;TrustServerCertificate=true;"
        );
    }
}
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Add DbContext with configuration-based connection string
builder.Services.AddDbContext<ApplicationDbContext>((options) =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(
        connectionString,
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelaySeconds: 30,
                errorNumbersToAdd: null
            );
            sqlOptions.MigrationsAssembly("Tabsan.EduSphere.Infrastructure");
        }
    );
});

// Log database connection info (non-sensitive)
var dbConnString = builder.Configuration.GetConnectionString("DefaultConnection");
if (dbConnString.Contains("localhost") || dbConnString.Contains("(local)"))
{
    Console.WriteLine("✓ Database: Development (localhost)");
}
else
{
    Console.WriteLine("✓ Database: Production");
}
```

**Key Points:**
- Remove hardcoded connection strings from DbContext
- Use `GetConnectionString("DefaultConnection")` from configuration
- Add retry policy for production resilience
- Specify migrations assembly for EF Core

---

### Stage 2.2 — Environment-Based Migrations & Seeding

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Apply migrations and seed database if needed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("🔄 Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("✓ Migrations applied successfully");
        
        // Only seed on Development
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("🌱 Seeding development data...");
            // Call seed method here
            // await SeedData.InitializeAsync(db);
            logger.LogInformation("✓ Development data seeded");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error during database initialization");
        if (app.Environment.IsDevelopment())
        {
            throw;  // Fail fast in development
        }
    }
}
```

---

## Phase 3: HTTPS, Security Policies & Environment-Specific Middleware

### Stage 3.1 — Configure HTTPS Redirection

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Configure HTTPS
if (!app.Environment.IsDevelopment())
{
    // Production: enforce HTTPS
    app.UseHsts();  // HTTP Strict Transport Security
    app.UseHttpsRedirection();  // Redirect HTTP -> HTTPS
}
else
{
    // Development: optional HTTPS
    // Allow non-HTTPS for local development if needed
}

// Add security headers middleware (see Part B)
app.UseMiddleware<SecurityHeadersMiddleware>();
```

**Web Project (Kestrel Configuration):**

**File:** `src/Tabsan.EduSphere.API/appsettings.Production.json`

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:80"
      },
      "Https": {
        "Url": "https://0.0.0.0:443",
        "Certificate": {
          "Path": "/etc/ssl/certs/yourdomain.pfx",
          "Password": "REPLACE_WITH_ENV_VAR"
        }
      }
    }
  }
}
```

**For IIS/Reverse Proxy:**

```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      }
    }
  },
  "UseForwardedHeaders": true
}
```

---

### Stage 3.2 — Configure ForwardedHeaders (Reverse Proxy Support)

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Add and configure ForwardedHeaders (for IIS, Nginx, Cloudflare)
if (!app.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | 
                                   ForwardedHeaders.XForwardedProto;
        options.TrustedProxies.Clear();
        options.TrustedNetworks.Clear();
        
        // Trust reverse proxy if behind one
        // For Cloudflare, add their IPs
        options.TrustedProxies.Add("127.0.0.1");
    });
    
    app.UseForwardedHeaders();
}
```

---

### Stage 3.3 — Configure CORS for Each Environment

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Add CORS with environment-specific origins
var allowedCorsOrigins = builder.Configuration
    .GetSection("AppSettings:CorsOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        policy.WithOrigins(allowedCorsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

app.UseCors("AllowConfiguredOrigins");

if (app.Environment.IsDevelopment())
{
    Console.WriteLine($"✓ CORS Allowed Origins: {string.Join(", ", allowedCorsOrigins)}");
}
```

---

## Phase 4: File Paths & IWebHostEnvironment

### Stage 4.1 — Replace Absolute File Paths

**File:** `src/Tabsan.EduSphere.Web/Services/FileService.cs`

**Changes:**

```csharp
public class FileService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public string GetUploadsDirectory()
    {
        // BEFORE: return "C:\\uploads\\";
        // AFTER:
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        
        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
            _logger.LogInformation($"Created uploads directory: {uploadsPath}");
        }
        
        return uploadsPath;
    }

    public string SaveUploadedFile(IFormFile file)
    {
        var uploadsDir = GetUploadsDirectory();
        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        var filePath = Path.Combine(uploadsDir, fileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }
        
        return $"/uploads/{fileName}";  // Return relative URL
    }
}
```

**Affected Files:**
- `src/Tabsan.EduSphere.Web/Services/FileService.cs`
- `src/Tabsan.EduSphere.API/Services/FileService.cs`
- Any controller handling file uploads
- Email template services

---

### Stage 4.2 — Update Content Root & Web Root Usage

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
// Make IWebHostEnvironment available to services
builder.Services.AddSingleton(builder.Environment);

// Use for configuration file locations
var contentRoot = builder.Environment.ContentRootPath;
var webRoot = builder.Environment.WebRootPath;

Console.WriteLine($"✓ Content Root: {contentRoot}");
Console.WriteLine($"✓ Web Root: {webRoot}");
```

---

## Phase 5: Environment-Based Logging

### Stage 5.1 — Configure Serilog (Structured Logging)

**Install NuGet Package:**
```
Serilog
Serilog.AspNetCore
Serilog.Sinks.File
Serilog.Sinks.Console
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
using Serilog;
using Serilog.Events;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Tabsan.EduSphere.API")
    .Enrich.WithEnvironmentName()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

try
{
    Log.Information("🚀 Starting Tabsan EduSphere API");
    
    var builder = WebApplicationBuilder.CreateBuilder(args);
    builder.Host.UseSerilog();  // Use Serilog
    
    // ... rest of configuration
    
    var app = builder.Build();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
```

**appsettings.Development.json:**

```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "logs/development-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

**appsettings.Production.json:**

```json
{
  "Serilog": {
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "/var/log/tabsan-edusphere/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

---

## Phase 6: Configuration Secrets & Environment Variables

### Stage 6.1 — Use User Secrets (Development)

**File:** `src/Tabsan.EduSphere.API/Tabsan.EduSphere.API.csproj`

```xml
<ItemGroup>
    <UserSecretsId>tabsan-edusphere-api-dev</UserSecretsId>
</ItemGroup>
```

**Command (Development Only):**

```powershell
# Store sensitive values in User Secrets (local machine only)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=TabsanEduSphere;..."
dotnet user-secrets set "AppSettings:JwtSecret" "your-super-secret-jwt-key"
dotnet user-secrets set "AppSettings:SmtpPassword" "your-email-password"

# List secrets
dotnet user-secrets list
```

### Stage 6.2 — Use Environment Variables (Production)

**File:** `.env` (for development reference, NOT committed to git)

```bash
# Development
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5181

# Production (set on hosting server)
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-db.yourhost.com;Database=TabsanEduSphere;User Id=sa;Password=****;
AppSettings__JwtSecret=your-production-secret
AppSettings__SmtpPassword=your-email-password
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
// Secrets from environment variables
builder.Configuration.AddEnvironmentVariables();

// Override individual settings
var jwtSecret = builder.Configuration["AppSettings:JwtSecret"];
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("AppSettings:JwtSecret environment variable is required");
}
```

### Stage 6.3 — `.gitignore` Updates

**File:** `.gitignore`

```
# Configuration secrets
appsettings.Production.json
appsettings.*.local.json
.env
.env.local
*.pfx
*.key

# User secrets
[Uu]ser[Ss]ecrets/

# Logs
logs/
*.log
```

---

## Phase 7: Verification & Testing

### Stage 7.1 — Verify Development Environment

**Commands:**

```powershell
# Set environment to Development
$env:ASPNETCORE_ENVIRONMENT="Development"

# Run application
dotnet run --project src/Tabsan.EduSphere.API

# Expected output:
# 🔧 Running in: Development
# ✓ Configuration loaded from:
#   - appsettings.json
#   - appsettings.Development.json
#   - Environment variables
# ✓ Connection string configured: Development (localhost)
# ✓ Database: Development (localhost)
# ✓ CORS Allowed Origins: https://localhost:5063, https://localhost:7063
```

### Stage 7.2 — Verify Production Environment

**Commands:**

```powershell
# Set environment to Production
$env:ASPNETCORE_ENVIRONMENT="Production"

# Set connection string
$env:ConnectionStrings__DefaultConnection="Server=prod-server;Database=TabsanEduSphere;..."

# Run application
dotnet run --project src/Tabsan.EduSphere.API

# Expected output:
# 🔧 Running in: Production
# ✓ Configuration loaded from:
#   - appsettings.json
#   - appsettings.Production.json
#   - Environment variables
# ✓ Connection string configured: Production
# ✓ Database: Production
# ✓ CORS Allowed Origins: https://yourdomain.com
```

### Stage 7.3 — Checklist

- [ ] appsettings.json, appsettings.Development.json, appsettings.Production.json created
- [ ] Program.cs loads configuration correctly
- [ ] All hardcoded values replaced with IConfiguration
- [ ] Connection string is environment-aware
- [ ] Database migrations run on startup
- [ ] HTTPS redirection works
- [ ] CORS is environment-specific
- [ ] File paths use IWebHostEnvironment
- [ ] Logging is structured (Serilog)
- [ ] Secrets are handled via User Secrets (dev) or environment variables (prod)
- [ ] Application runs locally without issues
- [ ] Application switches to production settings when ASPNETCORE_ENVIRONMENT=Production

---

# PART B: SECURITY HARDENING

## Objective

Harden the ASP.NET Core application against common web vulnerabilities and attacks:

- Brute-force & spam attacks
- SQL injection & XSS
- Unauthorized access
- DDoS & bot abuse
- Configuration leaks
- Unvalidated uploads

---

## Phase 1: Input Validation & Injection Protection

### Stage 1.1 — Data Annotations & Server-Side Validation

**File:** `src/Tabsan.EduSphere.Application/DTOs/LoginRequest.cs`

**Changes:**

```csharp
using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, 
        ErrorMessage = "Username must be between 3 and 100 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", 
        ErrorMessage = "Username can only contain letters, numbers, dots, dashes, and underscores")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, MinimumLength = 8, 
        ErrorMessage = "Password must be at least 8 characters")]
    public string Password { get; set; }

    [Range(typeof(bool), "false", "true")]
    public bool RememberMe { get; set; }
}

public class UserUpdateRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256)]
    public string Email { get; set; }

    [StringLength(200)]
    public string FullName { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Invalid department")]
    public int? DepartmentId { get; set; }
}
```

**File:** API/Web Controllers

```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // ModelState validation automatically runs
        if (!ModelState.IsValid)
        {
            _logger.LogWarning($"Invalid login attempt: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors))}");
            return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors) });
        }

        // Additional server-side validation
        if (request.Username.Length < 3)
        {
            return BadRequest("Username too short");
        }

        // Proceed with authentication
        return Ok();
    }
}
```

**Affected Files:**
- All DTOs in `src/Tabsan.EduSphere.Application/DTOs/`
- All API controllers in `src/Tabsan.EduSphere.API/Controllers/`
- All Web controllers in `src/Tabsan.EduSphere.Web/Controllers/`

---

### Stage 1.2 — Prevent SQL Injection (EF Core)

**Ensure all queries use:**

```csharp
// ✓ SAFE — Parameterized query
var users = await _context.Users
    .Where(u => u.Username == username)  // Parameterized
    .ToListAsync();

var user = await _context.Users
    .FromSqlInterpolated($"SELECT * FROM users WHERE username = {username}")  // Interpolated
    .FirstOrDefaultAsync();

// ✗ UNSAFE — String concatenation (FORBIDDEN)
var query = $"SELECT * FROM users WHERE username = '{username}'";  // NEVER DO THIS
```

**File:** `src/Tabsan.EduSphere.Infrastructure/Persistence/Repositories/UserRepository.cs`

```csharp
public class UserRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<User> GetUserByUsernameAsync(string username)
    {
        // Use parameterized query
        return await _context.Users
            .Where(u => u.Username == username)
            .FirstOrDefaultAsync();
    }

    // ✗ NEVER implement raw SQL queries like this:
    // var users = _context.Users.FromSqlRaw($"SELECT * FROM users WHERE username = '{username}'");
}
```

**Audit all Repository methods** to ensure no raw SQL with string concatenation.

---

### Stage 1.3 — Output Encoding & XSS Prevention

**File:** `src/Tabsan.EduSphere.Web/Views/Shared/DisplayTemplates/User.cshtml`

**Changes:**

```html
<!-- ✓ SAFE — ASP.NET MVC automatically encodes -->
<h2>@Model.FullName</h2>
<p>Email: @Model.Email</p>

<!-- ✗ UNSAFE — Raw HTML (only use with trusted content) -->
<!-- @Html.Raw(userContent)  <-- NEVER for user input -->

<!-- For displaying user-generated HTML safely, sanitize first -->
@if (!string.IsNullOrEmpty(Model.Bio))
{
    <!-- Use HtmlSanitizer library -->
    @Html.Raw(SanitizeHtml(Model.Bio))
}
```

**Install NuGet Package:**
```
HtmlSanitizer
```

**File:** `src/Tabsan.EduSphere.Web/Services/HtmlSanitizationService.cs`

```csharp
using HtmlSanitizer;

public class HtmlSanitizationService
{
    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return string.Empty;

        var sanitizer = new HtmlSanitizer();
        
        // Allow only safe tags
        sanitizer.AllowedTags.Add("b");
        sanitizer.AllowedTags.Add("i");
        sanitizer.AllowedTags.Add("u");
        sanitizer.AllowedTags.Add("p");
        sanitizer.AllowedTags.Add("br");
        sanitizer.AllowedTags.Add("a");
        
        // Remove dangerous attributes
        sanitizer.AllowedAttributes.Remove("onclick");
        sanitizer.AllowedAttributes.Remove("onload");
        
        return sanitizer.Sanitize(html);
    }
}
```

---

## Phase 2: Authentication, Authorization & Account Lockout

### Stage 2.1 — Account Lockout Policy

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password policy
    options.Password.RequiredLength = 12;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // Lockout policy
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User policy
    options.User.RequireUniqueEmail = true;
});
```

### Stage 2.2 — Login Attempt Logging & Alerting

**File:** `src/Tabsan.EduSphere.API/Controllers/AuthController.cs`

**Changes:**

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    var user = await _userManager.FindByNameAsync(request.Username);

    if (user == null)
    {
        _logger.LogWarning($"⚠️ Failed login attempt: username '{request.Username}' not found");
        await _auditService.LogLoginFailureAsync(request.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Unauthorized("Invalid credentials");
    }

    // Check if account is locked
    if (await _userManager.IsLockedOutAsync(user))
    {
        _logger.LogWarning($"🔒 Login attempt on locked account: {user.Username}");
        return Unauthorized("Account locked due to too many failed login attempts. Try again later.");
    }

    var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);

    if (!result.Succeeded)
    {
        _logger.LogWarning($"⚠️ Failed login: {user.Username} from {HttpContext.Connection.RemoteIpAddress}");
        await _auditService.LogLoginFailureAsync(user.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
        return Unauthorized("Invalid credentials");
    }

    _logger.LogInformation($"✓ Successful login: {user.Username}");
    await _auditService.LogLoginSuccessAsync(user.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

    return Ok(new { token = GenerateJwt(user) });
}
```

### Stage 2.3 — Role-Based Authorization

**File:** Controllers requiring authorization

```csharp
[Authorize(Roles = "Admin,SuperAdmin")]
[HttpDelete("users/{id}")]
public async Task<IActionResult> DeleteUser(Guid id)
{
    // Only Admin or SuperAdmin can delete users
    return Ok();
}

[Authorize(Roles = "Faculty")]
[HttpPost("courses/{courseId}/grades")]
public async Task<IActionResult> PostGrades(Guid courseId, [FromBody] GradesRequest request)
{
    // Only Faculty can post grades
    return Ok();
}
```

---

## Phase 3: HTTPS, Secure Cookies & Transport Security

### Stage 3.1 — Secure Cookie Configuration

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
// Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
    options.Secure = CookieSecurePolicy.Always;  // HTTPS only
});

// Add authentication with secure cookie
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Oidc";
})
.AddCookie(options =>
{
    options.Cookie.Name = "AuthToken";
    options.Cookie.HttpOnly = true;        // Prevent JS access
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;  // HTTPS only
    options.Cookie.SameSite = SameSiteMode.Strict;  // CSRF protection
    options.Cookie.Expiration = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// In the pipeline:
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();
```

---

## Phase 4: Rate Limiting & Anti-Spam

### Stage 4.1 — Implement Rate Limiting Middleware

**Install NuGet Package:**
```
AspNetCoreRateLimit
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

**Changes:**

```csharp
using AspNetCoreRateLimit;

// Add memory cache
builder.Services.AddMemoryCache();

// Load rate limiting configuration from appsettings
builder.Services.AddInMemoryRateLimiting();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimit"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));

// Add rate limiting service
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IIpPolicyStore, MemoryIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Add middleware
app.UseIpRateLimiting();
```

**File:** `appsettings.Development.json`

```json
{
  "IpRateLimit": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "IpWhitelist": [ "127.0.0.1", "::1/10" ],
    "EndpointWhitelist": [
      "GET:/api/health"
    ],
    "ClientWhitelist": [],
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 100
      }
    ]
  },
  "IpRateLimitPolicies": {
    "IpRules": [
      {
        "Ip": "192.168.1.1",
        "Rules": [
          {
            "Endpoint": "*",
            "Period": "1m",
            "Limit": 1000
          }
        ]
      }
    ]
  }
}
```

**File:** `appsettings.Production.json`

```json
{
  "IpRateLimit": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 60
      },
      {
        "Endpoint": "POST:/api/auth/login",
        "Period": "1m",
        "Limit": 5
      },
      {
        "Endpoint": "POST:/api/auth/register",
        "Period": "1h",
        "Limit": 3
      }
    ]
  }
}
```

---

### Stage 4.2 — Custom Rate Limiting Attribute

**File:** `src/Tabsan.EduSphere.API/Attributes/RateLimitAttribute.cs`

```csharp
using System;

[AttributeUsage(AttributeTargets.Method)]
public class RateLimitAttribute : Attribute
{
    public string Key { get; set; }
    public int Limit { get; set; }
    public int WindowSeconds { get; set; }

    public RateLimitAttribute(string key, int limit = 10, int windowSeconds = 60)
    {
        Key = key;
        Limit = limit;
        WindowSeconds = windowSeconds;
    }
}

// Usage in controller:
[HttpPost("login")]
[RateLimit("login", limit: 5, windowSeconds: 60)]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Only 5 login attempts per 60 seconds per IP
}
```

---

### Stage 4.3 — CAPTCHA Integration (Cloudflare Turnstile)

**File:** `appsettings.json`

```json
{
  "Cloudflare": {
    "TurnstileSecretKey": "REPLACE_WITH_SECRET_KEY",
    "TurnstileSiteKey": "REPLACE_WITH_SITE_KEY"
  }
}
```

**File:** `src/Tabsan.EduSphere.API/Services/CaptchaService.cs`

```csharp
using HttpClient = System.Net.Http.HttpClient;

public class CaptchaService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CaptchaService> _logger;

    public CaptchaService(HttpClient httpClient, IConfiguration configuration, ILogger<CaptchaService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("CAPTCHA token is empty");
            return false;
        }

        var secretKey = _configuration["Cloudflare:TurnstileSecretKey"];
        var request = new { secret = secretKey, response = token };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "https://challenges.cloudflare.com/turnstile/v0/siteverify",
                request
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("CAPTCHA verification failed");
                return false;
            }

            var result = await response.Content.ReadAsAsync<dynamic>();
            return result.success == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying CAPTCHA");
            return false;
        }
    }
}
```

**File:** `src/Tabsan.EduSphere.API/Controllers/AuthController.cs`

```csharp
[HttpPost("login")]
[RateLimit("login", limit: 5, windowSeconds: 60)]
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // After 3 failed attempts, require CAPTCHA
    var failedAttempts = await _auditService.GetRecentFailedLoginsAsync(
        HttpContext.Connection.RemoteIpAddress?.ToString(),
        minutes: 15
    );

    if (failedAttempts >= 3 && string.IsNullOrEmpty(request.CaptchaToken))
    {
        return BadRequest("CAPTCHA required");
    }

    if (failedAttempts >= 3)
    {
        var captchaValid = await _captchaService.VerifyTokenAsync(request.CaptchaToken);
        if (!captchaValid)
        {
            return BadRequest("Invalid CAPTCHA");
        }
    }

    // Proceed with login
    // ...
}
```

---

## Phase 5: Security Headers

### Stage 5.1 — Security Headers Middleware

**File:** `src/Tabsan.EduSphere.API/Middleware/SecurityHeadersMiddleware.cs`

```csharp
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    public SecurityHeadersMiddleware(RequestDelegate next, ILogger<SecurityHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        // Content Security Policy
        response.Headers.Add("Content-Security-Policy", 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' https://challenges.cloudflare.com; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self'; " +
            "connect-src 'self' https://challenges.cloudflare.com; " +
            "frame-src 'self' https://challenges.cloudflare.com"
        );

        // Prevent clickjacking
        response.Headers.Add("X-Frame-Options", "DENY");

        // Prevent MIME type sniffing
        response.Headers.Add("X-Content-Type-Options", "nosniff");

        // Referrer Policy
        response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions Policy (formerly Feature Policy)
        response.Headers.Add("Permissions-Policy", 
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=()"
        );

        // Remove server header
        response.Headers.Remove("Server");

        await _next(context);
    }
}
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
```

---

## Phase 6: File Upload Security

### Stage 6.1 — Validate Upload Files

**File:** `src/Tabsan.EduSphere.API/Services/FileUploadValidator.cs`

```csharp
public class FileUploadValidator
{
    private readonly ILogger<FileUploadValidator> _logger;
    private readonly string[] _allowedMimeTypes = new[]
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    private readonly string[] _allowedExtensions = new[] { ".pdf", ".jpg", ".png", ".doc", ".docx" };
    private const long MaxFileSize = 5 * 1024 * 1024;  // 5 MB

    public FileUploadValidator(ILogger<FileUploadValidator> logger)
    {
        _logger = logger;
    }

    public (bool isValid, string errorMessage) ValidateUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return (false, "File is empty");

        // Check file size
        if (file.Length > MaxFileSize)
        {
            _logger.LogWarning($"File too large: {file.FileName} ({file.Length} bytes)");
            return (false, $"File size exceeds limit of {MaxFileSize / (1024 * 1024)}MB");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLower();
        if (!_allowedExtensions.Contains(extension))
        {
            _logger.LogWarning($"Invalid file extension: {extension}");
            return (false, $"File type '{extension}' is not allowed");
        }

        // Check MIME type
        if (!_allowedMimeTypes.Contains(file.ContentType))
        {
            _logger.LogWarning($"Invalid MIME type: {file.ContentType}");
            return (false, $"File MIME type '{file.ContentType}' is not allowed");
        }

        // Verify file signature (magic bytes)
        if (!VerifyFileSignature(file))
        {
            _logger.LogWarning($"Invalid file signature: {file.FileName}");
            return (false, "File content does not match declared type");
        }

        return (true, string.Empty);
    }

    private bool VerifyFileSignature(IFormFile file)
    {
        // Read first few bytes
        using (var stream = file.OpenReadStream())
        {
            byte[] fileSignature = new byte[4];
            stream.Read(fileSignature, 0, 4);

            // PDF signature
            if (fileSignature[0] == 0x25 && fileSignature[1] == 0x50 && 
                fileSignature[2] == 0x44 && fileSignature[3] == 0x46)  // %PDF
                return file.ContentType == "application/pdf";

            // JPEG signature
            if (fileSignature[0] == 0xFF && fileSignature[1] == 0xD8 && 
                fileSignature[2] == 0xFF)
                return file.ContentType.Contains("jpeg");

            // PNG signature
            if (fileSignature[0] == 0x89 && fileSignature[1] == 0x50 && 
                fileSignature[2] == 0x4E && fileSignature[3] == 0x47)  // PNG
                return file.ContentType == "image/png";

            return true;  // Allow other types, but in production be more strict
        }
    }
}
```

### Stage 6.2 — Secure File Renaming

**File:** `src/Tabsan.EduSphere.API/Services/FileService.cs`

```csharp
public class FileService
{
    private readonly IWebHostEnvironment _env;
    private readonly FileUploadValidator _validator;
    private readonly ILogger<FileService> _logger;

    public FileService(IWebHostEnvironment env, FileUploadValidator validator, ILogger<FileService> logger)
    {
        _env = env;
        _validator = validator;
        _logger = logger;
    }

    public async Task<(bool success, string fileName, string errorMessage)> SaveUploadedFileAsync(IFormFile file)
    {
        // Validate
        var (isValid, errorMessage) = _validator.ValidateUpload(file);
        if (!isValid)
        {
            return (false, null, errorMessage);
        }

        // Generate safe filename
        var extension = Path.GetExtension(file.FileName).ToLower();
        var safeFileName = $"{Guid.NewGuid()}{extension}";
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");

        if (!Directory.Exists(uploadsPath))
        {
            Directory.CreateDirectory(uploadsPath);
        }

        var filePath = Path.Combine(uploadsPath, safeFileName);

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"File uploaded successfully: {safeFileName}");
            return (true, safeFileName, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return (false, null, "Error uploading file");
        }
    }
}
```

---

## Phase 7: Error Handling & Exception Management

### Stage 7.1 — Global Exception Handler Middleware

**File:** `src/Tabsan.EduSphere.API/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Unhandled exception");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "An error occurred processing your request",
            // Only show details in development
            details = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment() 
                ? exception.Message 
                : null,
            traceId = context.TraceIdentifier
        };

        context.Response.StatusCode = exception switch
        {
            ArgumentException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
```

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

## Phase 8: Logging & Monitoring

### Stage 8.1 — Suspicious Activity Logging

**File:** `src/Tabsan.EduSphere.Infrastructure/Services/AuditService.cs`

```csharp
public class AuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public async Task LogLoginFailureAsync(string username, string ipAddress)
    {
        _logger.LogWarning($"⚠️ Login failure: {username} from {ipAddress}");
        
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            EventType = "LoginFailure",
            Username = username,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // Alert if multiple failures from same IP
        var recentFailures = await _context.AuditLogs
            .Where(a => a.IpAddress == ipAddress && 
                        a.EventType == "LoginFailure" &&
                        a.Timestamp > DateTime.UtcNow.AddMinutes(-15))
            .CountAsync();

        if (recentFailures > 5)
        {
            _logger.LogError($"🚨 ALERT: High login failure rate from {ipAddress}");
            // Send alert to admin
        }
    }

    public async Task LogLoginSuccessAsync(string username, string ipAddress)
    {
        _logger.LogInformation($"✓ Login success: {username} from {ipAddress}");
        
        await _context.AuditLogs.AddAsync(new AuditLog
        {
            EventType = "LoginSuccess",
            Username = username,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}
```

---

## Phase 9: Configuration Security

### Stage 9.1 — Move Secrets Out of Code

**Files to Clean:**

- `appsettings.json` — Only non-sensitive config
- `appsettings.Development.json` — Use User Secrets override
- `appsettings.Production.json` — Use environment variables

**Secrets to Move:**

| Secret | Location | How to Provide |
|---|---|---|
| Database password | appsettings.Production.json | Environment variable: `ConnectionStrings__DefaultConnection` |
| JWT secret | appsettings.json | User Secret (dev) or Env Var (prod) |
| SMTP password | appsettings.json | User Secret (dev) or Env Var (prod) |
| API keys | appsettings.json | User Secret (dev) or Env Var (prod) |
| Cloudflare Turnstile secret | appsettings.json | Env Var (prod only) |

**File:** `.gitignore`

```
# Sensitive files
appsettings.Production.json
appsettings.*.local.json
appsettings.*.secret.json
.env
.env.local
*.pfx
secrets/
```

---

## Phase 10: API Security

### Stage 10.1 — Protect All Endpoints

**File:** `src/Tabsan.EduSphere.API/Controllers/UserController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]  // Require authentication for all endpoints
public class UserController : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,SuperAdmin")]  // Only admins
    public async Task<IActionResult> GetUser(Guid id)
    {
        // Only admins can view any user
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]  // Any authenticated user
    public async Task<IActionResult> GetCurrentUser()
    {
        // User can view only their own profile
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Ok();
    }
}
```

### Stage 10.2 — Validate All Request Data

```csharp
[HttpPost]
[Authorize]
public async Task<IActionResult> Create([FromBody] UserCreateRequest request)
{
    // ModelState validation runs automatically
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Additional business logic validation
    if (await _userService.UserExistsAsync(request.Email))
    {
        return BadRequest("Email already registered");
    }

    return Ok();
}
```

---

## Phase 11: Request Size & Resource Protection

### Stage 11.1 — Limit Request Body Size

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
// Limit request body size
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 5_242_880;  // 5 MB
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 5_242_880;  // 5 MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 5_242_880;  // 5 MB
});
```

---

## Phase 12: Production Readiness

### Stage 12.1 — Environment-Based Behavior

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
var app = builder.Build();

// Development features
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("⚙️ Development mode: Detailed errors enabled, Swagger available");
}
else
{
    // Production hardening
    app.UseExceptionHandler("/error");
    app.UseHsts();
    app.UseHttpsRedirection();
    Console.WriteLine("🔒 Production mode: Error handling enabled, HTTPS enforced");
}

// Always apply security features
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseIpRateLimiting();

app.Run();
```

### Stage 12.2 — Health Check Endpoint

**File:** `src/Tabsan.EduSphere.API/Program.cs`

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

app.MapHealthChecks("/health");
```

---

## Verification Checklist

- [ ] **Phase 1:** Configuration files created and working
- [ ] **Phase 2:** Database connection environment-aware
- [ ] **Phase 3:** HTTPS redirection and secure cookies working
- [ ] **Phase 4:** File paths use IWebHostEnvironment
- [ ] **Phase 5:** Structured logging (Serilog) configured
- [ ] **Phase 6:** Secrets removed from code, using env vars/user secrets
- [ ] **Phase 7:** Input validation on all DTOs
- [ ] **Phase 8:** SQL injection prevented (EF Core parameterized)
- [ ] **Phase 9:** Output encoding prevents XSS
- [ ] **Phase 10:** Account lockout policy enforced
- [ ] **Phase 11:** Secure cookies (HttpOnly, Secure, SameSite)
- [ ] **Phase 12:** Rate limiting implemented and tested
- [ ] **Phase 13:** Security headers middleware applied
- [ ] **Phase 14:** File upload validation and safe renaming
- [ ] **Phase 15:** Global exception handler prevents stack trace exposure
- [ ] **Phase 16:** Suspicious activity logging (audit logs)
- [ ] **Phase 17:** Role-based authorization on all endpoints
- [ ] **Phase 18:** Request body size limits enforced
- [ ] **Phase 19:** CAPTCHA integration (Cloudflare Turnstile)
- [ ] **Phase 20:** Production vs Development behavior differences verified
- [ ] **Phase 21:** Health check endpoint available
- [ ] **Phase 22:** All secrets moved to environment variables/user secrets
- [ ] **Phase 23:** Application tested locally (Development mode)
- [ ] **Phase 24:** Application tested with Production settings
- [ ] **Phase 25:** HTTPS works correctly
- [ ] **Phase 26:** CORS configured correctly per environment
- [ ] **Phase 27:** Logging outputs correctly per environment

---

## Deployment Checklist

Before deploying to production:

- [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
- [ ] Provide all required environment variables (database, secrets, API keys)
- [ ] Configure reverse proxy (IIS/Nginx) for HTTPS termination
- [ ] Install SSL certificate
- [ ] Set up Cloudflare (if using) with Turnstile
- [ ] Configure logging output directory (writable by app)
- [ ] Test all endpoints with authentication
- [ ] Test rate limiting
- [ ] Monitor error logs for issues
- [ ] Verify HTTPS redirection works
- [ ] Test from different IPs (rate limiting)
- [ ] Confirm no sensitive data in logs

---

## Related Resources

- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [Serilog Documentation](https://github.com/serilog/serilog/wiki)
- [Cloudflare Turnstile](https://developers.cloudflare.com/turnstile/)
