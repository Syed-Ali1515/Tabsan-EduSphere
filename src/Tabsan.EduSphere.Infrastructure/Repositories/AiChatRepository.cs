using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.AiChat;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for AI chat conversations and messages.
/// </summary>
public sealed class AiChatRepository : IAiChatRepository
{
    private readonly ApplicationDbContext _db;

    /// <summary>Initialises the repository with the application DbContext.</summary>
    public AiChatRepository(ApplicationDbContext db) => _db = db;

    /// <summary>Returns a conversation by its primary key.</summary>
    public Task<ChatConversation?> GetByIdAsync(Guid conversationId, CancellationToken ct = default)
        => _db.ChatConversations.FirstOrDefaultAsync(c => c.Id == conversationId, ct);

    /// <summary>Returns all conversations for a user, newest first.</summary>
    public async Task<IReadOnlyList<ChatConversation>> GetByUserAsync(Guid userId, CancellationToken ct = default)
        => await _db.ChatConversations
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.StartedAt)
                    .ToListAsync(ct);

    /// <summary>Returns a conversation with its messages eagerly loaded.</summary>
    public Task<ChatConversation?> GetWithMessagesAsync(Guid conversationId, CancellationToken ct = default)
        => _db.ChatConversations
              .Include(c => c.Messages)
              .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

    /// <summary>Inserts a new conversation.</summary>
    public async Task AddConversationAsync(ChatConversation conversation, CancellationToken ct = default)
        => await _db.ChatConversations.AddAsync(conversation, ct);

    /// <summary>Inserts a new message.</summary>
    public async Task AddMessageAsync(ChatMessage message, CancellationToken ct = default)
        => await _db.ChatMessages.AddAsync(message, ct);

    /// <summary>Persists all pending changes.</summary>
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
