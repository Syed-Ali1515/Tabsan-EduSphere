using FluentAssertions;
using Tabsan.EduSphere.Application.DTOs.Notifications;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Notifications;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.UnitTests;

public class Phase28Stage2Tests
{
    [Fact]
    public async Task SendSystemAsync_DefersLargeFanOut_ToBackgroundQueue()
    {
        var repo = new StubNotificationRepository();
        var queue = new StubNotificationFanoutQueue();
        var sut = new NotificationService(repo, queue);
        var recipients = Enumerable.Range(0, 260).Select(_ => Guid.NewGuid()).ToList();

        var notificationId = await sut.SendSystemAsync(
            "Scale-out test",
            "Deferred fan-out",
            NotificationType.System,
            recipients);

        notificationId.Should().NotBeEmpty();
        repo.AddRecipientsCalls.Should().Be(0);
        queue.Items.Should().ContainSingle();
        queue.Items[0].NotificationId.Should().Be(notificationId);
        queue.Items[0].RecipientUserIds.Should().HaveCount(260);
    }

    [Fact]
    public async Task SendAsync_PersistsRecipientsInline_WhenFanOutIsSmall()
    {
        var repo = new StubNotificationRepository();
        var queue = new StubNotificationFanoutQueue();
        var sut = new NotificationService(repo, queue);
        var recipientA = Guid.NewGuid();
        var recipientB = Guid.NewGuid();

        var notificationId = await sut.SendAsync(
            new SendNotificationRequest(
                "Portal update",
                "Inline fan-out",
                NotificationType.Announcement,
                new[] { recipientA, recipientA, recipientB }),
            Guid.NewGuid());

        notificationId.Should().NotBeEmpty();
        repo.AddRecipientsCalls.Should().Be(1);
        repo.LastRecipientBatch.Should().HaveCount(2);
        queue.Items.Should().BeEmpty();
    }
}

file sealed class StubNotificationFanoutQueue : INotificationFanoutQueue
{
    public List<NotificationFanoutWorkItem> Items { get; } = new();

    public void Enqueue(NotificationFanoutWorkItem workItem)
    {
        Items.Add(workItem);
    }
}

file sealed class StubNotificationRepository : INotificationRepository
{
    public int AddRecipientsCalls { get; private set; }
    public List<NotificationRecipient> LastRecipientBatch { get; private set; } = new();

    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<Notification?>(null);

    public Task AddAsync(Notification notification, CancellationToken ct = default) => Task.CompletedTask;

    public void Update(Notification notification)
    {
    }

    public Task<IReadOnlyList<NotificationRecipient>> GetForUserAsync(Guid userId, bool unreadOnly = false, int skip = 0, int take = 20, bool asNoTracking = false, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<NotificationRecipient>>(Array.Empty<NotificationRecipient>());

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default) => Task.FromResult(0);

    public Task<NotificationRecipient?> GetRecipientAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
        => Task.FromResult<NotificationRecipient?>(null);

    public Task<IReadOnlyList<string>> GetActiveUserEmailsAsync(IReadOnlyList<Guid> userIds, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

    public Task<IReadOnlyList<string>> GetActiveUserPhoneNumbersAsync(IReadOnlyList<Guid> userIds, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());

    public Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients, CancellationToken ct = default)
    {
        AddRecipientsCalls++;
        LastRecipientBatch = recipients.ToList();
        return Task.CompletedTask;
    }

    public void UpdateRecipient(NotificationRecipient recipient)
    {
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => Task.FromResult(0);
}