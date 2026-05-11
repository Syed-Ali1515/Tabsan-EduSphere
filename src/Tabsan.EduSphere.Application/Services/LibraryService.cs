// Final-Touches Phase 22 Stage 22.1 — LibraryService: library system integration
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
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
    private const string LibraryLoanCacheKeyPrefix = "library:loan-lookup";

    // Final-Touches Phase 34 Stage 6.1 — short external-call cache window for repeated safe read lookups.
    private static readonly TimeSpan LibraryLoanCacheTtl = TimeSpan.FromSeconds(30);

    private readonly ISettingsRepository _settings;
    private readonly HttpClient          _http;
    private readonly IOutboundIntegrationGateway _gateway;
    private readonly IDistributedCache _distributedCache;

    public LibraryService(
        ISettingsRepository settings,
        HttpClient http,
        IOutboundIntegrationGateway gateway,
        IDistributedCache distributedCache)
    {
        _settings = settings;
        _http     = http;
        _gateway  = gateway;
        _distributedCache = distributedCache;
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
        var all = await _settings.GetAllPortalSettingsAsync(ct);
        all.TryGetValue(KeyLoanApiUrl, out var loanApiUrl);
        all.TryGetValue(KeyApiToken,   out var apiToken);

        if (string.IsNullOrWhiteSpace(loanApiUrl))
            return new LibraryLoansResponse(false, null, Array.Empty<LibraryLoanItem>());

        // Final-Touches Phase 34 Stage 6.1 — cache safe external loan read responses per student + integration config hash.
        var cacheKey = BuildLoanLookupCacheKey(studentIdentifier, loanApiUrl, apiToken);
        var cachedJson = await _distributedCache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cachedJson))
        {
            var cachedResponse = JsonSerializer.Deserialize<LibraryLoansResponse>(cachedJson);
            if (cachedResponse is not null)
            {
                return cachedResponse;
            }
        }

        try
        {
            var url = $"{loanApiUrl.TrimEnd('/')}?id={Uri.EscapeDataString(studentIdentifier)}";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (!string.IsNullOrWhiteSpace(apiToken))
                request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var raw = await _gateway.ExecuteAsync(
                channel: "lms-external-api",
                operation: "library.loan-lookup",
                action: async gatewayCt =>
                {
                    using var response = await _http.SendAsync(request, gatewayCt);
                    response.EnsureSuccessStatusCode();
                    return await response.Content.ReadFromJsonAsync<IEnumerable<ExternalLoanDto>>(cancellationToken: gatewayCt)
                           ?? Enumerable.Empty<ExternalLoanDto>();
                },
                ct);

            var loans = raw.Select(r => new LibraryLoanItem(
                r.Title   ?? "",
                r.Author,
                r.DueDate,
                r.Status  ?? "Unknown",
                r.DueDate.HasValue && r.DueDate.Value < DateTime.UtcNow)).ToList();

            var response = new LibraryLoansResponse(true, null, loans);
            await _distributedCache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = LibraryLoanCacheTtl
                },
                ct);

            return response;
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

    private static string BuildLoanLookupCacheKey(string studentIdentifier, string loanApiUrl, string? apiToken)
    {
        var normalizedStudent = (studentIdentifier ?? string.Empty).Trim().ToUpperInvariant();
        var normalizedUrl = loanApiUrl.Trim();
        var tokenHashSource = apiToken ?? string.Empty;
        var configFingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{normalizedUrl}|{tokenHashSource}")));
        return $"{LibraryLoanCacheKeyPrefix}:{configFingerprint}:{normalizedStudent}";
    }
}
