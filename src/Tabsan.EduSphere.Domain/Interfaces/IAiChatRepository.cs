using Tabsan.EduSphere.Domain.AiChat;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository contract for AI chat conversation and message persistence.
/// </summary>
public interface IAiChatRepository
{
    /// <summary>Returns a conversation by its primary key.</summary>
    Task<ChatConversation?> GetByIdAsync(Guid conversationId, CancellationToken ct = default);

    /// <summary>Returns all conversations for a user, newest first.</summary>
    Task<IReadOnlyList<ChatConversation>> GetByUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns a conversation with its messages eagerly loaded.</summary>
    Task<ChatConversation?> GetWithMessagesAsync(Guid conversationId, CancellationToken ct = default);

    /// <summary>Inserts a new conversation.</summary>
    Task AddConversationAsync(ChatConversation conversation, CancellationToken ct = default);

    /// <summary>Inserts a new message.</summary>
    Task AddMessageAsync(ChatMessage message, CancellationToken ct = default);

    /// <summary>Persists all pending changes.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
