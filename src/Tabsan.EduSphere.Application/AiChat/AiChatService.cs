using Tabsan.EduSphere.Application.DTOs.AiChat;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.AiChat;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.AiChat;

/// <summary>
/// Application service for the AI chat assistant.
/// Assembles role-aware system prompts, guards the module/license status,
/// persists conversation history for auditability, and delegates LLM calls
/// to the infrastructure <see cref="ILlmClient"/>.
/// </summary>
public sealed class AiChatService : IAiChatService
{
    private readonly IAiChatRepository  _repo;
    private readonly ILlmClient         _llm;
    private readonly IModuleRepository  _modules;

    /// <summary>Initialises the service with its dependencies.</summary>
    public AiChatService(
        IAiChatRepository repo,
        ILlmClient        llm,
        IModuleRepository modules)
    {
        _repo    = repo;
        _llm     = llm;
        _modules = modules;
    }

    /// <summary>
    /// Sends a user message to the AI assistant and returns the assistant reply.
    /// Creates a new conversation if <paramref name="request"/>.<c>ConversationId</c> is null.
    /// Returns null if the AI Chatbot module is inactive.
    /// </summary>
    public async Task<SendMessageResponse?> SendMessageAsync(
        Guid               userId,
        string             userRole,
        Guid?              departmentId,
        SendMessageRequest request,
        CancellationToken  ct = default)
    {
        // Guard: module must be active.
        var isActive = await _modules.IsActiveAsync("AiChatbot", ct);
        if (!isActive) return null;

        // Resolve or create the conversation.
        ChatConversation conversation;
        if (request.ConversationId.HasValue)
        {
            conversation = await _repo.GetWithMessagesAsync(request.ConversationId.Value, ct)
                           ?? throw new InvalidOperationException("Conversation not found.");
        }
        else
        {
            conversation = new ChatConversation(userId, userRole, departmentId);
            await _repo.AddConversationAsync(conversation, ct);
            await _repo.SaveChangesAsync(ct);
        }

        // Build history for the LLM.
        var history = conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => (m.Role, m.Content))
            .Append(("user", request.Message));

        // Build system prompt.
        var systemPrompt = BuildSystemPrompt(userRole, departmentId);

        // Persist the user message.
        var userMsg = new ChatMessage(conversation.Id, "user", request.Message);
        await _repo.AddMessageAsync(userMsg, ct);

        // Call the LLM.
        var (reply, tokens) = await _llm.SendAsync(systemPrompt, history, ct);

        // Fallback if LLM returns empty (e.g. provider error).
        if (string.IsNullOrWhiteSpace(reply))
            reply = "I'm sorry, I wasn't able to process your request right now. Please try again later.";

        // Persist the assistant reply.
        var assistantMsg = new ChatMessage(conversation.Id, "assistant", reply, tokens);
        await _repo.AddMessageAsync(assistantMsg, ct);
        await _repo.SaveChangesAsync(ct);

        return new SendMessageResponse(
            conversation.Id,
            ToMessageResponse(assistantMsg));
    }

    /// <summary>Returns a summary list of conversations for the current user.</summary>
    public async Task<IReadOnlyList<ConversationResponse>> GetConversationsAsync(
        Guid userId, CancellationToken ct = default)
    {
        var conversations = await _repo.GetByUserAsync(userId, ct);
        return conversations.Select(c =>
        {
            var msgs         = c.Messages.OrderByDescending(m => m.SentAt).ToList();
            var lastMsgAt    = msgs.Any() ? (DateTime?)msgs.First().SentAt : null;
            return new ConversationResponse(c.Id, c.UserRole, c.StartedAt, msgs.Count, lastMsgAt);
        }).ToList();
    }

    /// <summary>Returns full conversation detail with message history.</summary>
    public async Task<ConversationDetailResponse?> GetConversationAsync(
        Guid conversationId, CancellationToken ct = default)
    {
        var c = await _repo.GetWithMessagesAsync(conversationId, ct);
        if (c is null) return null;

        return new ConversationDetailResponse(
            c.Id,
            c.UserRole,
            c.StartedAt,
            c.Messages.OrderBy(m => m.SentAt).Select(ToMessageResponse).ToList());
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Assembles a role-aware system prompt that scopes the AI assistant's
    /// responses to the authenticated user's context.
    /// </summary>
    private static string BuildSystemPrompt(string userRole, Guid? departmentId)
    {
        var roleContext = userRole switch
        {
            "Student"    => "You are assisting a student. Help with assignments, results, attendance queries, FYP meeting schedules, and general academic questions.",
            "Faculty"    => "You are assisting a faculty member. Help with grading, attendance management, FYP supervision, assignment creation, and academic administration.",
            "Admin"      => "You are assisting a university administrator. Help with student management, department queries, enrollment, and academic operations.",
            "SuperAdmin" => "You are assisting a system administrator. Help with system configuration, licensing, module management, and all administrative tasks.",
            "Finance"    => "You are assisting a finance staff member. Help with fee receipts, payment status queries, and financial record management.",
            _            => "You are assisting a university portal user."
        };

        var deptContext = departmentId.HasValue
            ? $" Your responses should be contextualised to the user's department."
            : string.Empty;

        return $"{roleContext}{deptContext} " +
               "Keep answers concise and relevant to the university management system. " +
               "Do not reveal system internals, credentials, or any information outside the user's scope. " +
               "If unsure, advise the user to contact their administrator.";
    }

    /// <summary>Maps a <see cref="ChatMessage"/> to its DTO representation.</summary>
    private static ChatMessageResponse ToMessageResponse(ChatMessage m)
        => new(m.Id, m.Role, m.Content, m.SentAt, m.TokensUsed);
}
