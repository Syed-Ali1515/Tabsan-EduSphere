namespace Tabsan.EduSphere.Domain.AiChat;

/// <summary>
/// Represents a chat session initiated by a user with the AI assistant.
/// </summary>
public sealed class ChatConversation
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>The user who owns this conversation.</summary>
    public Guid UserId { get; private set; }

    /// <summary>The role context used when building the AI system prompt (e.g. Student, Faculty).</summary>
    public string UserRole { get; private set; } = string.Empty;

    /// <summary>Optional department context for scoping the AI responses.</summary>
    public Guid? DepartmentId { get; private set; }

    /// <summary>UTC timestamp when the conversation was started.</summary>
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>Navigation collection of messages in this conversation.</summary>
    public IReadOnlyCollection<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();

    // EF Core constructor
    private ChatConversation() { }

    /// <summary>Opens a new conversation for a user.</summary>
    public ChatConversation(Guid userId, string userRole, Guid? departmentId)
    {
        UserId       = userId;
        UserRole     = userRole;
        DepartmentId = departmentId;
    }
}

/// <summary>
/// A single turn (user or assistant) within a <see cref="ChatConversation"/>.
/// </summary>
public sealed class ChatMessage
{
    /// <summary>Unique identifier.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>FK to the owning conversation.</summary>
    public Guid ConversationId { get; private set; }

    /// <summary>Owning conversation navigation.</summary>
    public ChatConversation Conversation { get; private set; } = null!;

    /// <summary>"user" or "assistant".</summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>Message content text.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>UTC timestamp the message was recorded.</summary>
    public DateTime SentAt { get; private set; } = DateTime.UtcNow;

    /// <summary>Approximate token count returned by the LLM provider (0 if unavailable).</summary>
    public int TokensUsed { get; private set; }

    // EF Core constructor
    private ChatMessage() { }

    /// <summary>Creates a new message turn for a conversation.</summary>
    public ChatMessage(Guid conversationId, string role, string content, int tokensUsed = 0)
    {
        ConversationId = conversationId;
        Role           = role;
        Content        = content;
        TokensUsed     = tokensUsed;
    }
}
