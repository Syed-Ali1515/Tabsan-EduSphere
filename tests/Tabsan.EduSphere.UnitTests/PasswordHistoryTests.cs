using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.UnitTests;

/// <summary>
/// Unit tests for password history enforcement (no-reuse of last 5 passwords).
/// Uses manual stubs — no mocking framework needed.
/// </summary>
public class PasswordHistoryTests
{
    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string Hash(string password) => $"HASH({password})";
        public bool Verify(string storedHash, string providedPassword) =>
            storedHash == $"HASH({providedPassword})";
    }

    private sealed class InMemoryPasswordHistoryRepository : IPasswordHistoryRepository
    {
        private readonly List<PasswordHistoryEntry> _entries = new();

        public Task<IList<PasswordHistoryEntry>> GetRecentAsync(
            Guid userId, int count, CancellationToken ct)
        {
            var all = _entries.FindAll(e => e.UserId == userId);
            var result = all.GetRange(0, Math.Min(count, all.Count));
            return Task.FromResult<IList<PasswordHistoryEntry>>(result);
        }

        public Task AddAsync(PasswordHistoryEntry entry, CancellationToken ct)
        {
            _entries.Insert(0, entry);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken ct) => Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly FakePasswordHasher Hasher = new();

    private static InMemoryPasswordHistoryRepository BuildRepoWithHistory(
        params string[] previousPasswords)
    {
        var repo = new InMemoryPasswordHistoryRepository();
        foreach (var pw in previousPasswords)
            repo.AddAsync(new PasswordHistoryEntry(UserId, Hasher.Hash(pw)),
                CancellationToken.None).GetAwaiter().GetResult();
        return repo;
    }

    // ── Tests ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task NewPassword_NotInHistory_IsAllowed()
    {
        var repo = BuildRepoWithHistory("OldPass1!", "OldPass2!", "OldPass3!");

        var recentHashes = await repo.GetRecentAsync(UserId, 5, CancellationToken.None);
        var newPassword = "BrandNew4!";

        var isReused = recentHashes.Any(h => Hasher.Verify(h.PasswordHash, newPassword));

        isReused.Should().BeFalse("brand new password should not match any history entry");
    }

    [Fact]
    public async Task NewPassword_MatchingRecentHistory_IsRejected()
    {
        var repo = BuildRepoWithHistory("OldPass1!", "OldPass2!", "OldPass3!");

        var recentHashes = await repo.GetRecentAsync(UserId, 5, CancellationToken.None);
        var reusedPassword = "OldPass2!";

        var isReused = recentHashes.Any(h => Hasher.Verify(h.PasswordHash, reusedPassword));

        isReused.Should().BeTrue("reusing a password from history should be detected");
    }

    [Fact]
    public async Task NewPassword_MatchingMostRecent_IsRejected()
    {
        var repo = BuildRepoWithHistory("MostRecentPass1!");

        var recentHashes = await repo.GetRecentAsync(UserId, 5, CancellationToken.None);
        var reusedPassword = "MostRecentPass1!";

        var isReused = recentHashes.Any(h => Hasher.Verify(h.PasswordHash, reusedPassword));

        isReused.Should().BeTrue("reusing the most recent password should be detected");
    }

    [Fact]
    public async Task NewPassword_Beyond5History_IsAllowed()
    {
        // Build 6 historical passwords (oldest first in params → newest inserted last at index 0).
        // After 6 inserts via Insert(0,...), list order is newest-first:
        //   [Pass1!, Pass2!, Pass3!, Pass4!, Pass5!, Pass6!]
        // GetRecentAsync(count:5) returns the first 5: Pass1!..Pass5!
        // Pass6! (the oldest) is at index 5 and is outside the 5-entry window.
        var repo = BuildRepoWithHistory(
            "Pass6!", "Pass5!", "Pass4!", "Pass3!", "Pass2!", "Pass1!");

        var recentHashes = await repo.GetRecentAsync(UserId, 5, CancellationToken.None);

        recentHashes.Count.Should().Be(5, "only 5 entries returned");

        // Pass6! is the oldest entry and should NOT appear in the 5-entry window.
        var isReused = recentHashes.Any(h => Hasher.Verify(h.PasswordHash, "Pass6!"));
        isReused.Should().BeFalse("Pass6! is beyond the 5-entry window and should be allowed");
    }

    [Fact]
    public async Task NoHistory_AnyPasswordIsAllowed()
    {
        var repo = new InMemoryPasswordHistoryRepository();

        var recentHashes = await repo.GetRecentAsync(UserId, 5, CancellationToken.None);

        recentHashes.Should().BeEmpty("no history entries exist");
        recentHashes.Any(h => Hasher.Verify(h.PasswordHash, "AnyPass1!")).Should().BeFalse();
    }

    [Fact]
    public void PasswordHistoryEntry_StoredHash_MatchesInput()
    {
        var hash = Hasher.Hash("TestPass1!");
        var entry = new PasswordHistoryEntry(UserId, hash);

        entry.UserId.Should().Be(UserId);
        entry.PasswordHash.Should().Be(hash);
        entry.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
