// Final-Touches Phase 22 Stage 22.2 — AccreditationRepository: EF Core implementation
using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Settings;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>EF Core implementation of IAccreditationRepository.</summary>
public class AccreditationRepository : IAccreditationRepository
{
    private readonly ApplicationDbContext _db;

    public AccreditationRepository(ApplicationDbContext db) => _db = db;

    public Task<IList<AccreditationTemplate>> GetAllAsync(CancellationToken ct = default)
        => _db.AccreditationTemplates
              .OrderBy(t => t.Name)
              .ToListAsync(ct)
              .ContinueWith<IList<AccreditationTemplate>>(r => r.Result, ct);

    public Task<AccreditationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AccreditationTemplates.FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task AddAsync(AccreditationTemplate template, CancellationToken ct = default)
        => await _db.AccreditationTemplates.AddAsync(template, ct);

    public void Remove(AccreditationTemplate template)
        => _db.AccreditationTemplates.Remove(template);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
