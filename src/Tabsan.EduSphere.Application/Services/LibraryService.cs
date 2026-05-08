// Final-Touches Phase 22 Stage 22.1 — LibraryService: library system integration
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Tabsan.EduSphere.Application.DTOs.External;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Manages the external library catalogue integration.
/// Configuration is stored in portal_settings under well-known keys.
/// Loan data is fetched by proxying to the configured external loan API.
/// </summary>
public class LibraryService : ILibraryService
{
    private const string KeyCatalogueUrl = "library_catalogue_url";
    private const string KeyApiToken     = "library_api_token";
    private const string KeyLoanApiUrl   = "library_api_loan_url";

    private readonly ISettingsRepository _settings;
    private readonly HttpClient          _http;

    public LibraryService(ISettingsRepository settings, HttpClient http)
    {
        _settings = settings;
        _http     = http;
    }

    // ── Configuration ─────────────────────────────────────────────────────────

    public async Task<LibraryConfigDto> GetConfigAsync(CancellationToken ct = default)
    {
        var all = await _settings.GetAllPortalSettingsAsync();
        all.TryGetValue(KeyCatalogueUrl, out var catalogueUrl);
        all.TryGetValue(KeyApiToken,     out var apiToken);
        all.TryGetValue(KeyLoanApiUrl,   out var loanApiUrl);
        return new LibraryConfigDto(
            string.IsNullOrWhiteSpace(catalogueUrl) ? null : catalogueUrl,
            string.IsNullOrWhiteSpace(apiToken)     ? null : apiToken,
            string.IsNullOrWhiteSpace(loanApiUrl)   ? null : loanApiUrl);
    }

    public async Task SaveConfigAsync(SaveLibraryConfigCommand cmd, CancellationToken ct = default)
    {
        await _settings.UpsertPortalSettingAsync(KeyCatalogueUrl, cmd.CatalogueUrl ?? "", ct);
        await _settings.UpsertPortalSettingAsync(KeyApiToken,     cmd.ApiToken     ?? "", ct);
        await _settings.UpsertPortalSettingAsync(KeyLoanApiUrl,   cmd.LoanApiUrl   ?? "", ct);
    }

    // ── Loan Proxy ────────────────────────────────────────────────────────────

    public async Task<LibraryLoansResponse> GetLoansAsync(string studentIdentifier, CancellationToken ct = default)
    {
        var all = await _settings.GetAllPortalSettingsAsync();
        all.TryGetValue(KeyLoanApiUrl, out var loanApiUrl);
        all.TryGetValue(KeyApiToken,   out var apiToken);

        if (string.IsNullOrWhiteSpace(loanApiUrl))
            return new LibraryLoansResponse(false, null, Array.Empty<LibraryLoanItem>());

        try
        {
            var url = $"{loanApiUrl.TrimEnd('/')}?id={Uri.EscapeDataString(studentIdentifier)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(apiToken))
                request.Headers.Add("Authorization", $"Bearer {apiToken}");

            using var response = await _http.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var raw = await response.Content.ReadFromJsonAsync<IEnumerable<ExternalLoanDto>>(cancellationToken: ct)
                      ?? Enumerable.Empty<ExternalLoanDto>();

            var loans = raw.Select(r => new LibraryLoanItem(
                r.Title   ?? "",
                r.Author,
                r.DueDate,
                r.Status  ?? "Unknown",
                r.DueDate.HasValue && r.DueDate.Value < DateTime.UtcNow)).ToList();

            return new LibraryLoansResponse(true, null, loans);
        }
        catch (Exception ex)
        {
            return new LibraryLoansResponse(true, $"Library service unavailable: {ex.Message}", Array.Empty<LibraryLoanItem>());
        }
    }

    // ── Private DTO for deserialising external loan API response ─────────────

    private sealed class ExternalLoanDto
    {
        [JsonPropertyName("title")]  public string?   Title   { get; set; }
        [JsonPropertyName("author")] public string?   Author  { get; set; }
        [JsonPropertyName("dueDate")]public DateTime? DueDate { get; set; }
        [JsonPropertyName("status")] public string?   Status  { get; set; }
    }
}
