namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 34 Stage 7.1 — queue contract for offloading account-security transactional emails.
public sealed record AccountSecurityEmailWorkItem(
    string To,
    string Subject,
    string HtmlBody,
    string Reason,
    DateTime RequestedAtUtc);

public interface IAccountSecurityEmailQueue
{
    ValueTask EnqueueAsync(AccountSecurityEmailWorkItem workItem, CancellationToken ct = default);
}
