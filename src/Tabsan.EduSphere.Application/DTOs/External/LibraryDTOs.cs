// Final-Touches Phase 22 Stage 22.1 — Library Integration DTOs
namespace Tabsan.EduSphere.Application.DTOs.External;

/// <summary>Library system connection configuration managed by SuperAdmin.</summary>
public sealed record LibraryConfigDto(
    string? CatalogueUrl,
    string? ApiToken,
    string? LoanApiUrl);

/// <summary>Request to update library settings.</summary>
public sealed record SaveLibraryConfigCommand(
    string? CatalogueUrl,
    string? ApiToken,
    string? LoanApiUrl);

/// <summary>A single loan record returned by the external library API proxy.</summary>
public sealed record LibraryLoanItem(
    string Title,
    string? Author,
    DateTime? DueDate,
    string Status,
    bool IsOverdue);

/// <summary>Response wrapping proxied loan data.</summary>
public sealed record LibraryLoansResponse(
    bool IsConfigured,
    string? ErrorMessage,
    IReadOnlyList<LibraryLoanItem> Loans);
