using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>EF Core implementation of IDepartmentRepository.</summary>
public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _db;
    public DepartmentRepository(ApplicationDbContext db) => _db = db;

    /// <summary>Returns all non-deleted departments ordered by name.</summary>
    public async Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken ct = default)
        => await _db.Departments.OrderBy(d => d.Name).ToListAsync(ct);

    /// <summary>Returns the department with the given ID, or null.</summary>
    public Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Departments.FirstOrDefaultAsync(d => d.Id == id, ct);

    /// <summary>Returns true when the code is already in use (case-insensitive).</summary>
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
        => _db.Departments.AnyAsync(d => d.Code == code.ToUpperInvariant(), ct);

    /// <summary>Queues the department for insertion.</summary>
    public async Task AddAsync(Department department, CancellationToken ct = default)
        => await _db.Departments.AddAsync(department, ct);

    /// <summary>Marks the department as modified.</summary>
    public void Update(Department department) => _db.Departments.Update(department);

    /// <summary>Commits pending changes.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
