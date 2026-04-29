using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Modules;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IModuleRepository.
/// Loads modules with their status included so callers get a complete picture
/// in a single query.
/// </summary>
public class ModuleRepository : IModuleRepository
{
    private readonly ApplicationDbContext _db;

    public ModuleRepository(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Returns all module definitions. Navigation property (ModuleStatus) is loaded
    /// via a separate join so the full module + status picture is available.
    /// </summary>
    public async Task<IReadOnlyList<Module>> GetAllWithStatusAsync(CancellationToken ct = default)
    {
        var modules = await _db.Modules.ToListAsync(ct);
        return modules;
    }

    /// <summary>
    /// Looks up the ModuleStatus row by the module's string key (e.g. "assignment").
    /// Returns null if the module has not been seeded yet.
    /// </summary>
    public Task<ModuleStatus?> GetStatusByKeyAsync(string moduleKey, CancellationToken ct = default)
        => _db.ModuleStatuses.Include(ms => ms.Module)
                              .FirstOrDefaultAsync(ms => ms.Module.Key == moduleKey, ct);

    /// <summary>
    /// Returns true when the named module is active.
    /// Used as a lightweight check in policy filters — does not load navigation properties.
    /// </summary>
    public Task<bool> IsActiveAsync(string moduleKey, CancellationToken ct = default)
        => _db.ModuleStatuses.AnyAsync(
               ms => ms.Module.Key == moduleKey && ms.IsActive, ct);

    /// <summary>Marks the ModuleStatus entity as Modified.</summary>
    public void UpdateStatus(ModuleStatus status) => _db.ModuleStatuses.Update(status);

    /// <summary>Commits pending changes.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
