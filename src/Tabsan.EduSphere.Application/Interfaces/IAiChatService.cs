using Tabsan.EduSphere.Application.DTOs.AiChat;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application service for the AI chat assistant.
/// </summary>
public interface IAiChatService
{
    /// <summary>
    /// Sends a user message to the AI assistant and returns the assistant reply.
    /// Creates a new conversation if <paramref name="request"/>.<c>ConversationId</c> is null.
    /// Returns null if the AI module is inactive or the license is invalid.
    /// </summary>
    Task<SendMessageResponse?> SendMessageAsync(
        Guid   userId,
        string userRole,
        Guid?  departmentId,
        SendMessageRequest request,
        CancellationToken  ct = default);

    /// <summary>Returns a summary list of conversations for the current user.</summary>
    Task<IReadOnlyList<ConversationResponse>> GetConversationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns full conversation detail with message history.</summary>
    Task<ConversationDetailResponse?> GetConversationAsync(Guid conversationId, CancellationToken ct = default);
}
