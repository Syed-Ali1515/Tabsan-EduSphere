// Final-Touches Phase 22 Stage 22.1 — Library Service interface
using Tabsan.EduSphere.Application.DTOs.External;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Service for managing the external library system integration.
/// Configuration (catalogue URL + optional API token) is stored in portal_settings.
/// The loan proxy fetches current student loans from the configured external endpoint.
/// </summary>
public interface ILibraryService
{
    /// <summary>Returns the current library connection configuration.</summary>
    Task<LibraryConfigDto> GetConfigAsync(CancellationToken ct = default);

    /// <summary>Saves (upserts) the library connection settings. SuperAdmin only.</summary>
    Task SaveConfigAsync(SaveLibraryConfigCommand cmd, CancellationToken ct = default);

    /// <summary>
    /// Proxies a loan-status request to the configured external library API.
    /// Uses the student identifier (registration number or email) as the lookup key.
    /// Returns an empty list with <c>IsConfigured = false</c> when no loan URL is set.
    /// </summary>
    Task<LibraryLoansResponse> GetLoansAsync(string studentIdentifier, CancellationToken ct = default);
}
