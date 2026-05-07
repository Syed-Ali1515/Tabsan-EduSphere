using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Application.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Notifications;
using Tabsan.EduSphere.BackgroundJobs;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Email;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.Infrastructure.Repositories;

var builder = Host.CreateApplicationBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection is required.");
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));

// ── Repository + notification services ───────────────────────────────────────
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
builder.Services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();

// ── Phase 12: Academic Calendar ───────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademicDeadlineRepository, AcademicDeadlineRepository>();
builder.Services.AddScoped<IAcademicCalendarService, AcademicCalendarService>();

// ── Background jobs ───────────────────────────────────────────────────────────
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<LicenseExpiryWarningJob>();
builder.Services.AddHostedService<DeadlineReminderJob>();

var host = builder.Build();
host.Run();
