using Tabsan.EduSphere.Domain.Auditing;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Service interface for writing audit log entries.
/// Abstracted here so the Application layer can fire audit events
/// without depending on any infrastructure concern.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Writes an audit log entry for a privileged action.
    /// This is a fire-and-forget async operation — callers do not need to await
    /// completion before returning a response to the client.
    /// </summary>
    Task LogAsync(AuditLog entry, CancellationToken ct = default);
}
