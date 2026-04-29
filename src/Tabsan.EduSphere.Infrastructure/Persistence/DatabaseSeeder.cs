using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Modules;
using Tabsan.EduSphere.Infrastructure.Modules;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.Infrastructure.Auth;

namespace Tabsan.EduSphere.Infrastructure.Persistence;

/// <summary>
/// Runs on application startup to ensure the database contains required seed data.
/// Idempotent — all inserts are guarded by existence checks so re-running is safe.
///
/// Seed order:
/// 1. Roles         (lookup rows — must exist before Users)
/// 2. Modules       (feature module definitions)
/// 3. ModuleStatus  (one status row per module)
/// 4. Super Admin   (bootstrap account from environment variables)
/// </summary>
public static class DatabaseSeeder
{
    /// <summary>
    /// Entry point called from Program.cs after EF migrations have been applied.
    /// Resolves all dependencies from the DI container via a scoped service provider.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<PasswordHasher>();

        await db.Database.MigrateAsync();

        await SeedRolesAsync(db);
        await SeedModulesAsync(db);
        await SeedSuperAdminAsync(db, hasher);

        await db.SaveChangesAsync();
    }

    // ── Roles ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts the four system roles if they do not already exist.
    /// Roles use an int PK so their IDs are stable across environments.
    /// </summary>
    private static async Task SeedRolesAsync(ApplicationDbContext db)
    {
        var existing = await db.Roles.Select(r => r.Name).ToListAsync();

        var seed = new[]
        {
            new Role("SuperAdmin", "Full platform access — manages license and all settings.", isSystemRole: true),
            new Role("Admin",      "Department-level admin — manages users and courses.",       isSystemRole: true),
            new Role("Faculty",    "Teaches courses and manages academic content.",              isSystemRole: true),
            new Role("Student",    "Enrolled student — accesses course and academic content.",  isSystemRole: true),
        };

        foreach (var role in seed)
        {
            if (!existing.Contains(role.Name))
                db.Roles.Add(role);
        }
    }

    // ── Modules ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Inserts all known module definitions and creates a default ModuleStatus row
    /// for each one. Mandatory modules are activated by default; optional modules
    /// start inactive so the Super Admin explicitly enables them.
    /// </summary>
    private static async Task SeedModulesAsync(ApplicationDbContext db)
    {
        var existingKeys = await db.Modules.Select(m => m.Key).ToListAsync();

        var definitions = new[]
        {
            // key,                      name,                   isMandatory
            (KnownModuleKeys.Authentication, "Authentication",           true),
            (KnownModuleKeys.Departments,    "Departments",              true),
            (KnownModuleKeys.Sis,            "Student Information",      true),
            (KnownModuleKeys.Courses,        "Courses",                  false),
            (KnownModuleKeys.Assignments,    "Assignments",              false),
            (KnownModuleKeys.Quizzes,        "Quizzes",                  false),
            (KnownModuleKeys.Attendance,     "Attendance",               false),
            (KnownModuleKeys.Results,        "Results / Grades",         false),
            (KnownModuleKeys.Notifications,  "Notifications",            false),
            (KnownModuleKeys.Fyp,            "Final Year Projects",      false),
            (KnownModuleKeys.AiChat,         "AI Chatbot",               false),
            (KnownModuleKeys.Reports,        "Reports",                  false),
            (KnownModuleKeys.Themes,         "UI Themes",                false),
            (KnownModuleKeys.AdvancedAudit,  "Advanced Audit Logging",   false),
        };

        foreach (var (key, name, mandatory) in definitions)
        {
            if (existingKeys.Contains(key)) continue;

            var module = new Module(key, name, mandatory);
            db.Modules.Add(module);

            // Create a matching status row.
            // Mandatory modules start active; optional modules start inactive.
            var status = new ModuleStatus(module.Id, mandatory, source: mandatory ? "mandatory" : "seed");
            db.ModuleStatuses.Add(status);
        }
    }

    // ── Super Admin Bootstrap ─────────────────────────────────────────────────

    /// <summary>
    /// Creates the initial Super Admin account from environment variables:
    ///   TABSAN_SUPER_USERNAME   — username (defaults to "superadmin")
    ///   TABSAN_SUPER_PASSWORD   — plain-text password (REQUIRED in production)
    ///   TABSAN_SUPER_EMAIL      — optional email address
    ///
    /// The account is only created when no user with the SuperAdmin role exists,
    /// so this is safe to run on every startup.
    /// </summary>
    private static async Task SeedSuperAdminAsync(ApplicationDbContext db, PasswordHasher hasher)
    {
        var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        if (superAdminRole is null) return; // roles not seeded yet — will run on next start

        var alreadyExists = await db.Users.AnyAsync(u => u.RoleId == superAdminRole.Id);
        if (alreadyExists) return;

        var username = Environment.GetEnvironmentVariable("TABSAN_SUPER_USERNAME") ?? "superadmin";
        var password = Environment.GetEnvironmentVariable("TABSAN_SUPER_PASSWORD")
            ?? throw new InvalidOperationException(
                "TABSAN_SUPER_PASSWORD environment variable is required for initial seeding.");
        var email    = Environment.GetEnvironmentVariable("TABSAN_SUPER_EMAIL");

        var hash = hasher.Hash(password);
        var superAdmin = new User(username, hash, superAdminRole.Id);

        if (!string.IsNullOrWhiteSpace(email))
            superAdmin.UpdateEmail(email);

        db.Users.Add(superAdmin);
    }
}
