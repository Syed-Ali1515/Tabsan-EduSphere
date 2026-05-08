using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Lms;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

// Final-Touches Phase 20 Stage 20.3 — discussion repository

/// <summary>EF Core implementation of IDiscussionRepository.</summary>
public sealed class DiscussionRepository : IDiscussionRepository
{
    private readonly ApplicationDbContext _db;
    public DiscussionRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<DiscussionThread>> GetThreadsByOfferingAsync(
        Guid offeringId, CancellationToken ct = default)
        => await _db.DiscussionThreads
                    .Where(t => t.OfferingId == offeringId)
                    .OrderByDescending(t => t.IsPinned)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToListAsync(ct);

    public async Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId, CancellationToken ct = default)
        => await _db.DiscussionThreads
                    .Include(t => t.Replies)
                    .FirstOrDefaultAsync(t => t.Id == threadId, ct);

    public async Task AddThreadAsync(DiscussionThread thread, CancellationToken ct = default)
        => await _db.DiscussionThreads.AddAsync(thread, ct);

    public void UpdateThread(DiscussionThread thread)
        => _db.DiscussionThreads.Update(thread);

    public void DeleteThread(DiscussionThread thread)
        => _db.DiscussionThreads.Remove(thread);

    public async Task<DiscussionReply?> GetReplyByIdAsync(Guid replyId, CancellationToken ct = default)
        => await _db.DiscussionReplies.FirstOrDefaultAsync(r => r.Id == replyId, ct);

    public async Task AddReplyAsync(DiscussionReply reply, CancellationToken ct = default)
        => await _db.DiscussionReplies.AddAsync(reply, ct);

    public void UpdateReply(DiscussionReply reply)
        => _db.DiscussionReplies.Update(reply);

    public void DeleteReply(DiscussionReply reply)
        => _db.DiscussionReplies.Remove(reply);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
