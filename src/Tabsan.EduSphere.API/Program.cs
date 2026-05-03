// ── Builder configuration ──────────────────────────────────────────────────────

using System.Text;
using System.Threading.RateLimiting;using FluentValidation;
using FluentValidation.AspNetCore;using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Application.Auth;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Modules;
using Tabsan.EduSphere.Application.Academic;
using Tabsan.EduSphere.Application.Assignments;
using Tabsan.EduSphere.Application.Attendance;
using Tabsan.EduSphere.Application.Fyp;
using Tabsan.EduSphere.Application.Notifications;
using Tabsan.EduSphere.Application.Quizzes;
using Tabsan.EduSphere.Application.AiChat;
using Tabsan.EduSphere.API.Middleware;
using Tabsan.EduSphere.Infrastructure.AiChat;
using Tabsan.EduSphere.Infrastructure.Analytics;
using Tabsan.EduSphere.BackgroundJobs;
using Tabsan.EduSphere.Infrastructure.Auditing;
using Tabsan.EduSphere.Infrastructure.Auth;
using Tabsan.EduSphere.Infrastructure.Licensing;
using Tabsan.EduSphere.Infrastructure.Modules;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.Infrastructure.Repositories;
using Tabsan.EduSphere.Application.Services;
using Tabsan.EduSphere.Infrastructure.Exporters;

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
builder.Services.AddScoped<IPasswordHasher, Argon2idPasswordHasher>();
builder.Services.AddScoped<LicenseValidationService>();

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
builder.Services.AddScoped<IResultCalculationService, ResultCalculationService>();

// ── Phase 4: Notifications and Attendance ─────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
// ── Phase 5: Quizzes and FYP ──────────────────────────────────────────
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IFypRepository, FypRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IFypService, FypService>();

// ── Phase 6: AI Chat and Analytics ───────────────────────────────────
builder.Services.AddScoped<IAiChatRepository, AiChatRepository>();
builder.Services.AddScoped<IAiChatService, AiChatService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
// LLM HTTP client — base URL and API key from AiChat:BaseUrl / AiChat:ApiKey in config.
builder.Services.AddHttpClient<ILlmClient, OpenAiLlmClient>((sp, client) =>
{
    var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
    client.BaseAddress = new Uri(cfg["AiChat:BaseUrl"] ?? "https://api.openai.com/");
    var apiKey = cfg["AiChat:ApiKey"] ?? string.Empty;
    if (!string.IsNullOrWhiteSpace(apiKey))
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
});

// ── Phase 8: Student Lifecycle & Account Security ───────────────────────
builder.Services.AddScoped<IStudentLifecycleRepository, StudentLifecycleRepository>();
builder.Services.AddScoped<IStudentLifecycleService, StudentLifecycleService>();
builder.Services.AddScoped<IAccountSecurityService, AccountSecurityService>();
builder.Services.AddScoped<ICsvRegistrationImportService, CsvRegistrationImportService>();

// ── Phase 9: Timetable, Report Settings, Module Roles, Theme ─────────────
builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<IBuildingRoomRepository, BuildingRoomRepository>();
builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
builder.Services.AddScoped<ITimetableExcelExporter, TimetableExcelExporter>();
builder.Services.AddScoped<ITimetablePdfExporter, TimetablePdfExporter>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<IBuildingRoomService, BuildingRoomService>();
builder.Services.AddScoped<IReportSettingsService, ReportSettingsService>();
builder.Services.AddScoped<IModuleRolesService, ModuleRolesService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<ISidebarMenuService, SidebarMenuService>();
builder.Services.AddScoped<IPortalBrandingService, PortalBrandingService>();

// ── Phase 10: Password history ────────────────────────────────────────────
builder.Services.AddScoped<IPasswordHistoryRepository, Tabsan.EduSphere.Infrastructure.Repositories.PasswordHistoryRepository>();

// ── Phase 12: Reporting ───────────────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IReportRepository, Tabsan.EduSphere.Infrastructure.Reporting.ReportRepository>();
builder.Services.AddScoped<IReportService, Tabsan.EduSphere.Infrastructure.Reporting.ReportService>();

// ── Rate limiting (OWASP hardening) ─────────────────────────────────────
builder.Services.AddRateLimiter(opts =>
{
    // Global sliding window: 100 requests per minute per IP.
    opts.AddSlidingWindowLimiter("global", o =>
    {
        o.PermitLimit       = 100;
        o.Window            = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
        o.QueueLimit        = 0;
    });
    // Stricter limit for authentication endpoints: 10 per minute per IP.
    opts.AddSlidingWindowLimiter("auth", o =>
    {
        o.PermitLimit       = 10;
        o.Window            = TimeSpan.FromMinutes(1);
        o.SegmentsPerWindow = 6;
        o.QueueLimit        = 0;
    });
    opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddHostedService<LicenseCheckWorker>();
builder.Services.AddHostedService<AttendanceAlertJob>();

// ── Email ─────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IEmailSender, Tabsan.EduSphere.Infrastructure.Email.MailKitEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, Tabsan.EduSphere.Infrastructure.Email.EmailTemplateRenderer>();

// ── FluentValidation ───────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Tabsan.EduSphere.Application.Validators.LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

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
app.UseSecurityHeaders();
app.UseRateLimiter();
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
