using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Notifications;
using Tabsan.EduSphere.BackgroundJobs;
using Tabsan.EduSphere.Domain.Interfaces;
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

// ── Background jobs ───────────────────────────────────────────────────────────
builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<LicenseExpiryWarningJob>();

var host = builder.Build();
host.Run();
