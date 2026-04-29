// ── Builder configuration ──────────────────────────────────────────────────────

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tabsan.EduSphere.Application.Auth;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Modules;
using Tabsan.EduSphere.Application.Academic;
using Tabsan.EduSphere.Application.Assignments;
using Tabsan.EduSphere.Application.Attendance;
using Tabsan.EduSphere.Application.Notifications;
using Tabsan.EduSphere.BackgroundJobs;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Auditing;
using Tabsan.EduSphere.Infrastructure.Auth;
using Tabsan.EduSphere.Infrastructure.Licensing;
using Tabsan.EduSphere.Infrastructure.Modules;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ── Database ────────────────────────────────────────────────────────────────────
// Reads the connection string from appsettings.json → ConnectionStrings:DefaultConnection.
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly("Tabsan.EduSphere.Infrastructure")));

// ── JWT Authentication ──────────────────────────────────────────────────────────
// Binds JwtSettings section from appsettings.json so options are strongly-typed.
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings.Issuer,
            ValidAudience            = jwtSettings.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero // no extra leeway on token expiry
        };
    });

// ── Authorization policies ──────────────────────────────────────────────────────
builder.Services.AddAuthorization(opts =>
{
    opts.AddPolicy("SuperAdmin", p => p.RequireRole("SuperAdmin"));
    opts.AddPolicy("Admin",      p => p.RequireRole("SuperAdmin", "Admin"));
    opts.AddPolicy("Faculty",    p => p.RequireRole("SuperAdmin", "Admin", "Faculty"));
    opts.AddPolicy("Student",    p => p.RequireRole("SuperAdmin", "Admin", "Faculty", "Student"));
});

// ── Infrastructure services ─────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ILicenseRepository, LicenseRepository>();
builder.Services.AddScoped<IModuleRepository, ModuleRepository>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<TokenService>(); // also registered directly for AuthController resolving
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<LicenseValidationService>(sp =>
{
    var repo   = sp.GetRequiredService<ILicenseRepository>();
    var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<LicenseValidationService>>();
    // RSA public key is stored in configuration — never in source control.
    var pubKey = builder.Configuration["License:PublicKeyPem"] ?? string.Empty;
    return new LicenseValidationService(repo, logger, pubKey);
});

// ── Module entitlement ──────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IModuleEntitlementResolver, ModuleEntitlementResolver>();
builder.Services.AddScoped<ModuleEntitlementResolver>(); // concrete needed by LicenseController

// ── Phase 2: Academic repositories ─────────────────────────────────────────────
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IAcademicProgramRepository, AcademicProgramRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentProfileRepository, StudentProfileRepository>();
builder.Services.AddScoped<IRegistrationWhitelistRepository, RegistrationWhitelistRepository>();
builder.Services.AddScoped<IFacultyAssignmentRepository, FacultyAssignmentRepository>();

// ── Application services ────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IModuleService, ModuleService>();
// Phase 2 application services
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentRegistrationService, StudentRegistrationService>();

// ── Phase 3: Assignments and Results ───────────────────────────────────────────
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IResultService, ResultService>();

// ── Phase 4: Notifications and Attendance ─────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();

// ── Background jobs ──────────────────────────────────────────────────
builder.Services.AddHostedService<LicenseCheckWorker>();
builder.Services.AddHostedService<AttendanceAlertJob>();

// ── API infrastructure ──────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Tabsan EduSphere API", Version = "v1" });

    // Add JWT bearer button to Swagger UI for easy manual testing.
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description  = "Enter your JWT token (without the 'Bearer ' prefix)"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Seed database on startup ─────────────────────────────────────────────────────
await DatabaseSeeder.SeedAsync(app.Services);

// ── HTTP pipeline ────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// ─────────────────────────────────────────────────────────────────────────────────
// Placeholder kept to satisfy the default .NET scaffold test project reference.
// Safe to remove once WeatherForecast is replaced.
#pragma warning disable CS8321
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
