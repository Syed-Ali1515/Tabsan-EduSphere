using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.IntegrationTests.Infrastructure;

/// <summary>
/// Shared WebApplicationFactory that targets a dedicated LocalDB integration-test
/// database (<c>TabsanEduSphere_IntegrationTests</c>).
/// The database is dropped and recreated via EF migrations on first use, ensuring
/// a clean, reproducible baseline. All background hosted services are removed to
/// prevent interference with the isolated test database.
/// </summary>
public sealed class EduSphereWebFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    /// <summary>
    /// Dedicated database name — separate from the development database so tests
    /// never touch real data.
    /// </summary>
    private const string TestDbName = "TabsanEduSphere_IntegrationTests";

    private const string TestConnectionString =
        $@"Server=(localdb)\mssqllocaldb;Database={TestDbName};Trusted_Connection=True;MultipleActiveResultSets=true";

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    public async Task InitializeAsync()
    {
        // Drop any leftover test database BEFORE the factory first builds the app.
        // (The factory builds lazily on first CreateClient() call, at which point
        // DatabaseSeeder.SeedAsync runs MigrateAsync + seeds all default data.)
        await using var db = BuildStandaloneContext();
        await db.Database.EnsureDeletedAsync();
    }

    public new async Task DisposeAsync()
    {
        // Drop the test database after the full test run so nothing persists.
        await using var db = BuildStandaloneContext();
        await db.Database.EnsureDeletedAsync();

        await base.DisposeAsync();
    }

    private static ApplicationDbContext BuildStandaloneContext()
    {
        var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(TestConnectionString,
                sql => sql.MigrationsAssembly("Tabsan.EduSphere.Infrastructure"))
            .Options;
        return new ApplicationDbContext(opts);
    }

    // ── WebApplicationFactory override ─────────────────────────────────────────

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Swap out the production DbContext options for the test database.
            var dbOpts = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbOpts is not null)
                services.Remove(dbOpts);

            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseSqlServer(
                    TestConnectionString,
                    sql => sql.MigrationsAssembly("Tabsan.EduSphere.Infrastructure")));

            // Remove all background hosted services (LicenseCheckWorker, AttendanceAlertJob, etc.)
            // so they do not interfere with the isolated test database.
            var hosted = services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList();
            foreach (var d in hosted)
                services.Remove(d);
        });
    }
}
