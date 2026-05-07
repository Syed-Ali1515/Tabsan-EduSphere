using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Helpdesk;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>EF Core implementation of IHelpdeskRepository (Phase 14).</summary>
public class HelpdeskRepository : IHelpdeskRepository
{
    private readonly ApplicationDbContext _db;
    public HelpdeskRepository(ApplicationDbContext db) => _db = db;

    // ── Tickets ──────────────────────────────────────────────────────────────

    public Task<SupportTicket?> GetTicketByIdAsync(Guid id, CancellationToken ct = default)
        => _db.SupportTickets
              .Include(t => t.Messages)
              .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsBySubmitterAsync(Guid submitterId, CancellationToken ct = default)
        => await _db.SupportTickets
                    .Where(t => t.SubmitterId == submitterId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsByDepartmentAsync(
        IReadOnlyList<Guid>? departmentIds, TicketStatus? status, CancellationToken ct = default)
    {
        var query = _db.SupportTickets.AsQueryable();

        if (departmentIds is { Count: > 0 })
            query = query.Where(t => t.DepartmentId.HasValue && departmentIds.Contains(t.DepartmentId.Value));

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SupportTicket>> GetTicketsByAssigneeAsync(Guid assignedToId, CancellationToken ct = default)
        => await _db.SupportTickets
                    .Where(t => t.AssignedToId == assignedToId)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync(ct);

    public async Task AddTicketAsync(SupportTicket ticket, CancellationToken ct = default)
        => await _db.SupportTickets.AddAsync(ticket, ct);

    // ── Messages ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SupportTicketMessage>> GetMessagesAsync(
        Guid ticketId, bool includeInternalNotes, CancellationToken ct = default)
    {
        var query = _db.SupportTicketMessages.Where(m => m.TicketId == ticketId);
        if (!includeInternalNotes)
            query = query.Where(m => !m.IsInternalNote);
        return await query.OrderBy(m => m.CreatedAt).ToListAsync(ct);
    }

    public async Task AddMessageAsync(SupportTicketMessage message, CancellationToken ct = default)
        => await _db.SupportTicketMessages.AddAsync(message, ct);

    // ── Persistence ──────────────────────────────────────────────────────────

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
