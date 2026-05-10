namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 30 Stage 30.1 - provider-agnostic outbound integration gateway.

public interface IOutboundIntegrationGateway
{
    Task ExecuteAsync(
        string channel,
        string operation,
        Func<CancellationToken, Task> action,
        CancellationToken ct = default);

    Task<T> ExecuteAsync<T>(
        string channel,
        string operation,
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default);

    Task<IReadOnlyList<IntegrationDeadLetterEntry>> GetRecentDeadLettersAsync(int take = 50, CancellationToken ct = default);

    Task<int> GetDeadLetterCountAsync(CancellationToken ct = default);

    IntegrationChannelPolicySnapshot GetPolicySnapshot(string channel);
}

public sealed record IntegrationDeadLetterEntry(
    Guid Id,
    string Channel,
    string Operation,
    string ErrorType,
    string ErrorMessage,
    int Attempts,
    DateTime OccurredAtUtc,
    string? CorrelationId = null);

public sealed record IntegrationChannelPolicySnapshot(
    string Channel,
    int MaxRetries,
    int TimeoutSeconds,
    int BaseDelayMilliseconds,
    bool ExponentialBackoffEnabled);
