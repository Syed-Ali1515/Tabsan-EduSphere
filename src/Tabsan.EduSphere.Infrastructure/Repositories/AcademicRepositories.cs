using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>EF Core implementation of IAcademicProgramRepository.</summary>
public class AcademicProgramRepository : IAcademicProgramRepository
{
    private readonly ApplicationDbContext _db;
    public AcademicProgramRepository(ApplicationDbContext db) => _db = db;

    /// <summary>Returns all programmes, optionally scoped to a specific department.</summary>
    public async Task<IReadOnlyList<AcademicProgram>> GetAllAsync(Guid? departmentId = null, CancellationToken ct = default)
    {
        var query = _db.AcademicPrograms.Include(p => p.Department).AsQueryable();
        if (departmentId.HasValue)
            query = query.Where(p => p.DepartmentId == departmentId.Value);
        return await query.OrderBy(p => p.Name).ToListAsync(ct);
    }

    /// <summary>Returns the programme with the given ID (with Department loaded), or null.</summary>
    public Task<AcademicProgram?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AcademicPrograms.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id, ct);

    /// <summary>Returns true when the uppercase code is already taken inside the given department.</summary>
    public Task<bool> CodeExistsAsync(string code, Guid departmentId, CancellationToken ct = default)
        => _db.AcademicPrograms.AnyAsync(p => p.Code == code.ToUpperInvariant() && p.DepartmentId == departmentId, ct);

    /// <summary>Queues the programme for insertion.</summary>
    public async Task AddAsync(AcademicProgram program, CancellationToken ct = default)
        => await _db.AcademicPrograms.AddAsync(program, ct);

    /// <summary>Marks the programme as modified.</summary>
    public void Update(AcademicProgram program) => _db.AcademicPrograms.Update(program);

    /// <summary>Commits pending changes.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}

/// <summary>EF Core implementation of ISemesterRepository.</summary>
public class SemesterRepository : ISemesterRepository
{
    private readonly ApplicationDbContext _db;
    public SemesterRepository(ApplicationDbContext db) => _db = db;

    /// <summary>Returns all semesters ordered by start date descending (most recent first).</summary>
    public async Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default)
        => await _db.Semesters.OrderByDescending(s => s.StartDate).ToListAsync(ct);

    /// <summary>Returns the semester with the given ID, or null.</summary>
    public Task<Semester?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Semesters.FirstOrDefaultAsync(s => s.Id == id, ct);

    /// <summary>Returns the most recent semester that is not yet closed, or null if all are closed.</summary>
    public Task<Semester?> GetCurrentOpenAsync(CancellationToken ct = default)
        => _db.Semesters.Where(s => !s.IsClosed)
                        .OrderByDescending(s => s.StartDate)
                        .FirstOrDefaultAsync(ct);

    /// <summary>Queues the semester for insertion.</summary>
    public async Task AddAsync(Semester semester, CancellationToken ct = default)
        => await _db.Semesters.AddAsync(semester, ct);

    /// <summary>Marks the semester as modified.</summary>
    public void Update(Semester semester) => _db.Semesters.Update(semester);

    /// <summary>Commits pending changes.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
