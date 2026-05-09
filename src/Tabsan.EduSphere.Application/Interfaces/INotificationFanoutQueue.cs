namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Queues large notification fan-out workloads so API requests do not block on bulk recipient inserts.
/// </summary>
public interface INotificationFanoutQueue
{
    void Enqueue(NotificationFanoutWorkItem workItem);
}

/// <summary>
/// Represents a deferred notification recipient fan-out batch.
/// </summary>
public sealed record NotificationFanoutWorkItem(Guid NotificationId, IReadOnlyList<Guid> RecipientUserIds);