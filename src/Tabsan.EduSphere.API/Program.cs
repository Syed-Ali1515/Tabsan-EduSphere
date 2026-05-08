// ── Builder configuration ──────────────────────────────────────────────────────

using System.Text;
using System.Threading.RateLimiting;using FluentValidation;
using FluentValidation.AspNetCore;using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog (structured logging — console + rolling file) ───────────────────────
builder.Host.UseSerilog((ctx, services, config) =>
{
    var isDev = ctx.HostingEnvironment.IsDevelopment();
    var minLevel = isDev ? LogEventLevel.Debug : LogEventLevel.Warning;

    config
        .MinimumLevel.Is(minLevel)
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "Tabsan.EduSphere.API")
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "logs/app-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            outputTemplate:
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
});

// ── Database ────────────────────────────────────────────────────────────────────
// Reads the connection string from appsettings.json → ConnectionStrings:DefaultConnection.
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql =>
        {
            sql.MigrationsAssembly("Tabsan.EduSphere.Infrastructure");
            sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(30), null);
        }));

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
builder.Services.AddScoped<IAdminAssignmentRepository, AdminAssignmentRepository>();

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

// ── Phase 4: CSV User Import (P4-S1-01) ──────────────────────────────────
builder.Services.AddScoped<IUserImportService, UserImportService>();

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

// ── Phase 12: Academic Calendar ───────────────────────────────────────────
builder.Services.AddScoped<IAcademicDeadlineRepository, AcademicDeadlineRepository>();
builder.Services.AddScoped<IAcademicCalendarService, AcademicCalendarService>();

// ── Phase 13: Global Search ───────────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ISearchRepository, Tabsan.EduSphere.Infrastructure.Repositories.SearchRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ISearchService, Tabsan.EduSphere.Application.Search.SearchService>();

// ── Phase 14: Helpdesk / Support Ticketing ────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IHelpdeskRepository, Tabsan.EduSphere.Infrastructure.Repositories.HelpdeskRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IHelpdeskService, Tabsan.EduSphere.Application.Helpdesk.HelpdeskService>();

// ── Phase 15: Enrollment Rules Engine ────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IPrerequisiteRepository, Tabsan.EduSphere.Infrastructure.Repositories.PrerequisiteRepository>();

// ── Phase 16: Faculty Grading System ─────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IGradebookRepository, Tabsan.EduSphere.Infrastructure.Repositories.GradebookRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IGradebookService, Tabsan.EduSphere.Application.Assignments.GradebookService>();
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IRubricRepository, Tabsan.EduSphere.Infrastructure.Repositories.RubricRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IRubricService, Tabsan.EduSphere.Application.Assignments.RubricService>();

// ── Phase 17: Degree Audit System ─────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IDegreeAuditRepository, Tabsan.EduSphere.Infrastructure.Repositories.DegreeAuditRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IDegreeAuditService, Tabsan.EduSphere.Application.Academic.DegreeAuditService>();

// ── Phase 18: Graduation Workflow ──────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IGraduationRepository, Tabsan.EduSphere.Infrastructure.Repositories.GraduationRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ICertificateGenerator, Tabsan.EduSphere.Infrastructure.Services.CertificateGenerator>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IGraduationService, Tabsan.EduSphere.Application.Academic.GraduationService>();

// ── Phase 19: Advanced Course Creation & Grading Config ────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ICourseService, Tabsan.EduSphere.Application.Academic.CourseService>();
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.ICourseGradingRepository, Tabsan.EduSphere.Infrastructure.Repositories.CourseGradingRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ICourseGradingService, Tabsan.EduSphere.Application.Academic.CourseGradingService>();

// ── Phase 20: Learning Management System (LMS) ─────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.ILmsRepository, Tabsan.EduSphere.Infrastructure.Repositories.LmsRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IDiscussionRepository, Tabsan.EduSphere.Infrastructure.Repositories.DiscussionRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IAnnouncementRepository, Tabsan.EduSphere.Infrastructure.Repositories.AnnouncementRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.ILmsService, Tabsan.EduSphere.Application.Lms.LmsService>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IDiscussionService, Tabsan.EduSphere.Application.Lms.DiscussionService>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IAnnouncementService, Tabsan.EduSphere.Application.Lms.AnnouncementService>();

// ── Phase 21: Study Planner ──────────────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IStudyPlanRepository, Tabsan.EduSphere.Infrastructure.Repositories.StudyPlanRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IStudyPlanService, Tabsan.EduSphere.Application.StudyPlanner.StudyPlanService>();

// ── Phase 22: External Integrations ──────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Domain.Interfaces.IAccreditationRepository, Tabsan.EduSphere.Infrastructure.Repositories.AccreditationRepository>();
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IAccreditationService, Tabsan.EduSphere.Application.Services.AccreditationService>();
builder.Services.AddHttpClient<Tabsan.EduSphere.Application.Interfaces.ILibraryService, Tabsan.EduSphere.Application.Services.LibraryService>();
// ── Phase 23: Core Policy Foundation ────────────────────────────────────────────
builder.Services.AddScoped<Tabsan.EduSphere.Application.Interfaces.IInstitutionPolicyService, Tabsan.EduSphere.Application.Services.InstitutionPolicyService>();
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

// ── CORS (configured from AppSettings:CorsOrigins) ───────────────────────────
var corsOrigins = builder.Configuration.GetSection("AppSettings:CorsOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length > 0)
{
    builder.Services.AddCors(corsOpts =>
        corsOpts.AddPolicy("AllowConfiguredOrigins", policy =>
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()));
}

// ── Forwarded headers (reverse proxy: IIS / nginx / Cloudflare) ──────────────
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<ForwardedHeadersOptions>(fwdOpts =>
    {
        fwdOpts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        fwdOpts.KnownNetworks.Clear();
        fwdOpts.KnownProxies.Clear();
    });
}

// ── Health checks ─────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ── Request body size limits (5 MB max — OWASP resource protection) ──────────
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(
    opts => opts.Limits.MaxRequestBodySize = 5 * 1024 * 1024);
builder.Services.Configure<IISServerOptions>(
    opts => opts.MaxRequestBodySize = 5 * 1024 * 1024);
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(
    opts => opts.MultipartBodyLengthLimit = 5 * 1024 * 1024);

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

Console.WriteLine($"[Startup] Environment: {builder.Environment.EnvironmentName} | App: {builder.Environment.ApplicationName}");

var app = builder.Build();

// ── Seed database on startup ─────────────────────────────────────────────────────
await DatabaseSeeder.SeedAsync(app.Services);

// ── HTTP pipeline ────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("AppSettings:EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Serve uploaded branding assets (e.g., /portal-uploads/logo.png) from API wwwroot.
var apiWebRoot = app.Environment.WebRootPath;
if (string.IsNullOrWhiteSpace(apiWebRoot))
{
    apiWebRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");
}
Directory.CreateDirectory(apiWebRoot);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(apiWebRoot)
});
app.UseSecurityHeaders();
app.UseRateLimiter();
if (corsOrigins.Length > 0)
{
    app.UseCors("AllowConfiguredOrigins");
}
// P2-S3-02: Reject requests from domains that do not match the activated license domain.
app.UseMiddleware<LicenseDomainMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
// Phase 23 — resolve institution policy snapshot once per request (after auth)
app.UseMiddleware<Tabsan.EduSphere.API.Middleware.InstitutionContextMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
