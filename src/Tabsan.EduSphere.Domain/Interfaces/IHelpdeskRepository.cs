using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Helpdesk;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository contract for Phase 14 — Helpdesk / Support Ticketing.
/// </summary>
public interface IHelpdeskRepository
{
    // ── Tickets ──────────────────────────────────────────────────────────────

    Task<SupportTicket?> GetTicketByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns tickets submitted by the given user.</summary>
    Task<IReadOnlyList<SupportTicket>> GetTicketsBySubmitterAsync(Guid submitterId, TicketStatus? status, int skip, int take, CancellationToken ct = default);

    Task<int> CountTicketsBySubmitterAsync(Guid submitterId, TicketStatus? status, CancellationToken ct = default);

    /// <summary>Returns tickets within the given department(s). Null = all departments (SuperAdmin).</summary>
    Task<IReadOnlyList<SupportTicket>> GetTicketsByDepartmentAsync(IReadOnlyList<Guid>? departmentIds, TicketStatus? status, int skip, int take, CancellationToken ct = default);

    Task<int> CountTicketsByDepartmentAsync(IReadOnlyList<Guid>? departmentIds, TicketStatus? status, CancellationToken ct = default);

    /// <summary>Returns tickets assigned to or submitted by the given faculty member.</summary>
    Task<IReadOnlyList<SupportTicket>> GetTicketsByAssigneeOrSubmitterAsync(Guid userId, TicketStatus? status, int skip, int take, CancellationToken ct = default);

    Task<int> CountTicketsByAssigneeOrSubmitterAsync(Guid userId, TicketStatus? status, CancellationToken ct = default);

    Task AddTicketAsync(SupportTicket ticket, CancellationToken ct = default);

    // ── Messages ─────────────────────────────────────────────────────────────

    Task<IReadOnlyList<SupportTicketMessage>> GetMessagesAsync(Guid ticketId, bool includeInternalNotes, CancellationToken ct = default);

    Task AddMessageAsync(SupportTicketMessage message, CancellationToken ct = default);

    // ── Persistence ──────────────────────────────────────────────────────────

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
