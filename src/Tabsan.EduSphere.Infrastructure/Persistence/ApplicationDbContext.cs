using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Assignments;
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

    // ── Phase 2: Academic Core ─────────────────────────────────────────────
    /// <summary>Degree programmes offered by departments.</summary>
    public DbSet<AcademicProgram> AcademicPrograms => Set<AcademicProgram>();

    /// <summary>Academic terms (semesters). Locked once closed.</summary>
    public DbSet<Semester> Semesters => Set<Semester>();

    /// <summary>Course catalogue definitions.</summary>
    public DbSet<Course> Courses => Set<Course>();

    /// <summary>Course offerings — a course scheduled for a specific semester and faculty.</summary>
    public DbSet<CourseOffering> CourseOfferings => Set<CourseOffering>();

    /// <summary>Extended academic profile for students.</summary>
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();

    /// <summary>Student enrollment records — permanent academic history, never deleted.</summary>
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    /// <summary>Pre-approved registration entries for student self-registration.</summary>
    public DbSet<RegistrationWhitelist> RegistrationWhitelist => Set<RegistrationWhitelist>();

    /// <summary>Faculty ↔ department access assignments.</summary>
    public DbSet<FacultyDepartmentAssignment> FacultyDepartmentAssignments => Set<FacultyDepartmentAssignment>();

    // ── Phase 3: Assignments and Results ───────────────────────────────────────
    /// <summary>Faculty-created assignments for course offerings.</summary>
    public DbSet<Assignment> Assignments => Set<Assignment>();

    /// <summary>Student submissions for assignments — permanent academic evidence.</summary>
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();

    /// <summary>Published and draft result marks per student per course offering.</summary>
    public DbSet<Result> Results => Set<Result>();

    /// <summary>Audit log of every transcript export request.</summary>
    public DbSet<TranscriptExportLog> TranscriptExportLogs => Set<TranscriptExportLog>();

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
