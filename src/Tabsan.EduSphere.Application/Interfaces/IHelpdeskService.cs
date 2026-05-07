using Tabsan.EduSphere.Application.DTOs.Helpdesk;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application service for Phase 14 — Helpdesk / Support Ticketing.
/// </summary>
public interface IHelpdeskService
{
    // ── Submission / Viewing ──────────────────────────────────────────────────

    Task<Guid> CreateTicketAsync(CreateTicketRequest request, CancellationToken ct = default);

    Task<TicketDetailDto?> GetTicketDetailAsync(Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default);

    /// <summary>Returns ticket summaries visible to the caller (role-scoped).</summary>
    Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(Guid callerId, string callerRole,
        IReadOnlyList<Guid>? departmentIds, TicketStatus? status, CancellationToken ct = default);

    // ── Messaging ────────────────────────────────────────────────────────────

    Task<Guid> AddMessageAsync(AddMessageRequest request, CancellationToken ct = default);

    // ── Case Management ───────────────────────────────────────────────────────

    Task AssignTicketAsync(AssignTicketRequest request, CancellationToken ct = default);

    Task ResolveTicketAsync(Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default);

    Task CloseTicketAsync(Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default);

    Task ReopenTicketAsync(Guid ticketId, Guid submitterId, CancellationToken ct = default);
}
