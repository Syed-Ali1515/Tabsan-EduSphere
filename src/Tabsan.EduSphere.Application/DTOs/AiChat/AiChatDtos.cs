namespace Tabsan.EduSphere.Application.DTOs.AiChat;

// ── Request records ──────────────────────────────────────────────────────────

/// <summary>Sends a message to the AI assistant, optionally continuing an existing conversation.</summary>
public record SendMessageRequest(
    /// <summary>Existing conversation ID; null to start a new conversation.</summary>
    Guid? ConversationId,
    /// <summary>User's message text.</summary>
    string Message);

// ── Response records ─────────────────────────────────────────────────────────

/// <summary>Summary of a chat conversation.</summary>
public record ConversationResponse(
    Guid   Id,
    string UserRole,
    DateTime StartedAt,
    int    MessageCount,
    DateTime? LastMessageAt);

/// <summary>A single message turn within a conversation.</summary>
public record ChatMessageResponse(
    Guid     Id,
    string   Role,
    string   Content,
    DateTime SentAt,
    int      TokensUsed);

/// <summary>Full conversation detail with message history.</summary>
public record ConversationDetailResponse(
    Guid   Id,
    string UserRole,
    DateTime StartedAt,
    IReadOnlyList<ChatMessageResponse> Messages);

/// <summary>The assistant's reply to a sent message.</summary>
public record SendMessageResponse(
    Guid   ConversationId,
    ChatMessageResponse AssistantMessage);
