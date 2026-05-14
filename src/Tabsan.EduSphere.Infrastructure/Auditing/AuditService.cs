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

    public async Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
        string? query = null,
        Guid? actorUserId = null,
        string? action = null,
        string? entityName = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, 200);

        IQueryable<AuditLog> auditQuery = _db.AuditLogs.AsNoTracking();

        if (actorUserId.HasValue)
            auditQuery = auditQuery.Where(x => x.ActorUserId == actorUserId.Value);

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionFilter = action.Trim();
            auditQuery = auditQuery.Where(x => x.Action == actionFilter);
        }

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            var entityFilter = entityName.Trim();
            auditQuery = auditQuery.Where(x => x.EntityName == entityFilter);
        }

        if (fromUtc.HasValue)
            auditQuery = auditQuery.Where(x => x.OccurredAt >= fromUtc.Value);

        if (toUtc.HasValue)
            auditQuery = auditQuery.Where(x => x.OccurredAt <= toUtc.Value);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim();
            var like = $"%{q}%";
            auditQuery = auditQuery.Where(x =>
                EF.Functions.Like(x.Action, like)
                || EF.Functions.Like(x.EntityName, like)
                || (x.EntityId != null && EF.Functions.Like(x.EntityId, like))
                || (x.IpAddress != null && EF.Functions.Like(x.IpAddress, like)));
        }

        var totalCount = await auditQuery.CountAsync(ct);
        var items = await auditQuery
            .OrderByDescending(x => x.OccurredAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
