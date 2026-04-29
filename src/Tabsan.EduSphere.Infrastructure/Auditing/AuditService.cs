using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Auditing;

/// <summary>
/// Writes audit log entries to the database.
/// The AuditLog table is append-only — entries are never updated or deleted.
/// Uses a separate DbContext scope so audit writes do not interfere with
/// the main request's unit of work.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Appends a new audit log entry to the database.
    /// Runs asynchronously and does not block the caller's response pipeline.
    /// </summary>
    public async Task LogAsync(AuditLog entry, CancellationToken ct = default)
    {
        await _db.AuditLogs.AddAsync(entry, ct);
        await _db.SaveChangesAsync(ct);
    }
}
