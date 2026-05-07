using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.DTOs.Helpdesk;

// ── Requests ─────────────────────────────────────────────────────────────────

public record CreateTicketRequest(
    Guid             SubmitterId,
    Guid?            DepartmentId,
    TicketCategory   Category,
    string           Subject,
    string           Body
);

public record AddMessageRequest(
    Guid   TicketId,
    Guid   AuthorId,
    string Body,
    bool   IsInternalNote = false
);

public record AssignTicketRequest(
    Guid TicketId,
    Guid AssignedToId
);

public record UpdateTicketStatusRequest(
    Guid         TicketId,
    TicketStatus Status
);

// ── Responses ────────────────────────────────────────────────────────────────

public record TicketSummaryDto(
    Guid           Id,
    string         Subject,
    TicketCategory Category,
    TicketStatus   Status,
    Guid           SubmitterId,
    string         SubmitterName,
    Guid?          AssignedToId,
    string?        AssigneeName,
    Guid?          DepartmentId,
    DateTime       CreatedAt,
    DateTime?      ResolvedAt,
    int            MessageCount
);

public record TicketDetailDto(
    Guid                          Id,
    string                        Subject,
    string                        Body,
    TicketCategory                Category,
    TicketStatus                  Status,
    Guid                          SubmitterId,
    string                        SubmitterName,
    Guid?                         AssignedToId,
    string?                       AssigneeName,
    Guid?                         DepartmentId,
    DateTime                      CreatedAt,
    DateTime?                     ResolvedAt,
    int                           ReopenWindowDays,
    bool                          CanReopen,
    IReadOnlyList<TicketMessageDto> Messages
);

public record TicketMessageDto(
    Guid     Id,
    Guid     AuthorId,
    string   AuthorName,
    string   Body,
    bool     IsInternalNote,
    DateTime CreatedAt
);
