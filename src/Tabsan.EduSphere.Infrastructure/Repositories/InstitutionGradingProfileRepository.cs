using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>EF Core implementation of <see cref="IInstitutionGradingProfileRepository"/>.</summary>
public class InstitutionGradingProfileRepository : IInstitutionGradingProfileRepository
{
    private readonly ApplicationDbContext _db;

    public InstitutionGradingProfileRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<InstitutionGradingProfile>> GetAllAsync(CancellationToken ct = default)
        => await _db.InstitutionGradingProfiles.ToListAsync(ct);

    public Task<InstitutionGradingProfile?> GetByTypeAsync(InstitutionType institutionType, CancellationToken ct = default)
        => _db.InstitutionGradingProfiles.FirstOrDefaultAsync(p => p.InstitutionType == institutionType, ct);

    public Task<InstitutionGradingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.InstitutionGradingProfiles.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task AddAsync(InstitutionGradingProfile profile, CancellationToken ct = default)
    {
        _db.InstitutionGradingProfiles.Add(profile);
        return Task.CompletedTask;
    }

    public void Update(InstitutionGradingProfile profile)
        => _db.InstitutionGradingProfiles.Update(profile);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
