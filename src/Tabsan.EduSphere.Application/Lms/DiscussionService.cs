using Tabsan.EduSphere.Application.DTOs.Lms;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Lms;

namespace Tabsan.EduSphere.Application.Lms;

// Final-Touches Phase 20 Stage 20.3 — discussion forum service implementation

/// <summary>
/// Application service for Phase 20 — discussion threads and replies.
/// Author names are resolved via IUserRepository using the AuthorId stored on each entity.
/// </summary>
public sealed class DiscussionService : IDiscussionService
{
    private readonly IDiscussionRepository _repo;
    private readonly IUserRepository       _users;

    public DiscussionService(IDiscussionRepository repo, IUserRepository users)
    {
        _repo  = repo;
        _users = users;
    }

    public async Task<List<DiscussionThreadDto>> GetThreadsAsync(
        Guid offeringId, CancellationToken ct = default)
    {
        var threads = await _repo.GetThreadsByOfferingAsync(offeringId, ct);
        var dtos    = new List<DiscussionThreadDto>(threads.Count);
        foreach (var t in threads)
        {
            var author = await _users.GetByIdAsync(t.AuthorId, ct);
            dtos.Add(MapThread(t, author?.Username ?? "Unknown", []));
        }
        return dtos;
    }

    public async Task<DiscussionThreadDto?> GetThreadAsync(Guid threadId, CancellationToken ct = default)
    {
        var thread = await _repo.GetThreadByIdAsync(threadId, ct);
        if (thread is null) return null;

        var author  = await _users.GetByIdAsync(thread.AuthorId, ct);
        var replies = new List<DiscussionReplyDto>(thread.Replies.Count);
        foreach (var r in thread.Replies.Where(r => !r.IsDeleted))
        {
            var replyAuthor = await _users.GetByIdAsync(r.AuthorId, ct);
            replies.Add(MapReply(r, replyAuthor?.Username ?? "Unknown"));
        }
        return MapThread(thread, author?.Username ?? "Unknown", replies);
    }

    public async Task<DiscussionThreadDto> CreateThreadAsync(
        CreateThreadRequest request, CancellationToken ct = default)
    {
        var thread = new DiscussionThread(request.OfferingId, request.AuthorId, request.Title);
        await _repo.AddThreadAsync(thread, ct);
        await _repo.SaveChangesAsync(ct);
        var author = await _users.GetByIdAsync(thread.AuthorId, ct);
        return MapThread(thread, author?.Username ?? "Unknown", []);
    }

    public async Task SetPinnedAsync(Guid threadId, bool pinned, CancellationToken ct = default)
    {
        var thread = await _repo.GetThreadByIdAsync(threadId, ct)
            ?? throw new InvalidOperationException($"Thread {threadId} not found.");
        thread.SetPinned(pinned);
        _repo.UpdateThread(thread);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task CloseThreadAsync(Guid threadId, CancellationToken ct = default)
    {
        var thread = await _repo.GetThreadByIdAsync(threadId, ct)
            ?? throw new InvalidOperationException($"Thread {threadId} not found.");
        thread.Close();
        _repo.UpdateThread(thread);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task ReopenThreadAsync(Guid threadId, CancellationToken ct = default)
    {
        var thread = await _repo.GetThreadByIdAsync(threadId, ct)
            ?? throw new InvalidOperationException($"Thread {threadId} not found.");
        thread.Reopen();
        _repo.UpdateThread(thread);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeleteThreadAsync(Guid threadId, CancellationToken ct = default)
    {
        var thread = await _repo.GetThreadByIdAsync(threadId, ct)
            ?? throw new InvalidOperationException($"Thread {threadId} not found.");
        thread.SoftDelete();
        _repo.UpdateThread(thread);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task<DiscussionReplyDto> AddReplyAsync(AddReplyRequest request, CancellationToken ct = default)
    {
        var reply = new DiscussionReply(request.ThreadId, request.AuthorId, request.Body);
        await _repo.AddReplyAsync(reply, ct);
        await _repo.SaveChangesAsync(ct);
        var author = await _users.GetByIdAsync(reply.AuthorId, ct);
        return MapReply(reply, author?.Username ?? "Unknown");
    }

    public async Task DeleteReplyAsync(
        Guid replyId, Guid requesterId, bool isFaculty, CancellationToken ct = default)
    {
        var reply = await _repo.GetReplyByIdAsync(replyId, ct)
            ?? throw new InvalidOperationException($"Reply {replyId} not found.");
        if (!isFaculty && reply.AuthorId != requesterId)
            throw new UnauthorizedAccessException("Only the author or faculty can delete a reply.");
        reply.SoftDelete();
        _repo.UpdateReply(reply);
        await _repo.SaveChangesAsync(ct);
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static DiscussionThreadDto MapThread(
        DiscussionThread t, string authorName, List<DiscussionReplyDto> replies) => new()
    {
        Id         = t.Id,
        OfferingId = t.OfferingId,
        Title      = t.Title,
        AuthorId   = t.AuthorId,
        AuthorName = authorName,
        IsPinned   = t.IsPinned,
        IsClosed   = t.IsClosed,
        CreatedAt  = t.CreatedAt,
        ReplyCount = t.Replies.Count(r => !r.IsDeleted),
        Replies    = replies
    };

    private static DiscussionReplyDto MapReply(DiscussionReply r, string authorName) => new()
    {
        Id         = r.Id,
        ThreadId   = r.ThreadId,
        AuthorId   = r.AuthorId,
        AuthorName = authorName,
        Body       = r.Body,
        CreatedAt  = r.CreatedAt,
        UpdatedAt  = r.UpdatedAt
    };
}
