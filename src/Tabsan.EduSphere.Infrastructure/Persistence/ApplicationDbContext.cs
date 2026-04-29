using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Licensing;
using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Infrastructure.Persistence;

/// <summary>
/// Central EF Core DbContext for the University Portal application database.
/// All entity configurations are loaded from the Configurations sub-folder
/// using the fluent API (IEntityTypeConfiguration) rather than data annotations
/// to keep domain entities free of infrastructure concerns.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Identity ───────────────────────────────────────────────────────────
    /// <summary>All system users (students, faculty, admins, super admins).</summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>Predefined system roles seeded at startup.</summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary>Active refresh-token sessions per user.</summary>
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    // ── Academic Core ──────────────────────────────────────────────────────
    /// <summary>University departments — the root organisational unit.</summary>
    public DbSet<Department> Departments => Set<Department>();

    // ── Licensing ──────────────────────────────────────────────────────────
    /// <summary>Single-row table holding the current validated license state.</summary>
    public DbSet<LicenseState> LicenseStates => Set<LicenseState>();

    // ── Modules ────────────────────────────────────────────────────────────
    /// <summary>Module definitions seeded at startup.</summary>
    public DbSet<Module> Modules => Set<Module>();

    /// <summary>Per-module activation state managed by Super Admin.</summary>
    public DbSet<ModuleStatus> ModuleStatuses => Set<ModuleStatus>();

    // ── Audit ──────────────────────────────────────────────────────────────
    /// <summary>Append-only audit log for privileged actions.</summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    /// <summary>
    /// Scans the current assembly for all IEntityTypeConfiguration implementations
    /// and applies them automatically. This keeps OnModelCreating clean as the
    /// schema grows across phases.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Discover and apply all entity configurations in this assembly automatically.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>
    /// Intercepts SaveChanges to automatically update the UpdatedAt timestamp
    /// on any BaseEntity that has been modified.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Iterates all tracked entries that implement BaseEntity and updates
    /// their UpdatedAt timestamp before writing to the database.
    /// </summary>
    private void SetAuditTimestamps()
    {
        foreach (var entry in ChangeTracker.Entries<Domain.Common.BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.Touch();
        }
    }
}
