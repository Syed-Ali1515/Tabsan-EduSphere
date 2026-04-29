using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Licensing;

namespace Tabsan.EduSphere.Infrastructure.Licensing;

/// <summary>
/// Validates the cryptographically signed license file and manages the stored license state.
///
/// License file format expected (JSON):
/// {
///   "licenseType": "Yearly" | "Permanent",
///   "issuedAt": "2026-01-01T00:00:00Z",
///   "expiresAt": "2027-01-01T00:00:00Z",  ← null for Permanent
///   "signature": "&lt;Base64 RSA-SHA256 signature of the payload&gt;"
/// }
///
/// The RSA public key is embedded in configuration (never the private key).
/// The private key lives only in the License Creation Tool.
/// </summary>
public class LicenseValidationService
{
    private readonly ILicenseRepository _licenseRepo;
    private readonly ILogger<LicenseValidationService> _logger;

    // The RSA public key PEM is loaded from configuration at startup.
    private readonly string _publicKeyPem;

    public LicenseValidationService(
        ILicenseRepository licenseRepo,
        ILogger<LicenseValidationService> logger,
        string publicKeyPem)
    {
        _licenseRepo = licenseRepo;
        _logger = logger;
        _publicKeyPem = publicKeyPem;
    }

    /// <summary>
    /// Reads the license file from the given path, verifies the RSA-SHA256 signature,
    /// and either creates or replaces the LicenseState record in the database.
    /// Returns true on success; false when the file is invalid or the signature fails.
    /// </summary>
    public async Task<bool> ActivateFromFileAsync(string licenseFilePath, CancellationToken ct = default)
    {
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(licenseFilePath, ct);
            var json = Encoding.UTF8.GetString(fileBytes);
            var payload = JsonSerializer.Deserialize<LicensePayload>(json);

            if (payload is null)
            {
                _logger.LogWarning("License file could not be deserialised.");
                return false;
            }

            if (!VerifySignature(payload))
            {
                _logger.LogWarning("License file signature verification failed. File may be tampered.");
                return false;
            }

            var hash = ComputeFileHash(fileBytes);
            var licenseType = Enum.Parse<LicenseType>(payload.LicenseType, ignoreCase: true);

            var existing = await _licenseRepo.GetCurrentAsync(ct);

            if (existing is null)
            {
                var newState = new LicenseState(hash, licenseType, payload.ExpiresAt);
                await _licenseRepo.AddAsync(newState, ct);
            }
            else
            {
                existing.Replace(hash, licenseType, payload.ExpiresAt);
                _licenseRepo.Update(existing);
            }

            await _licenseRepo.SaveChangesAsync(ct);
            _logger.LogInformation("License activated successfully. Type={Type}", licenseType);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during license activation.");
            return false;
        }
    }

    /// <summary>
    /// Checks the stored license state and refreshes its status based on expiry.
    /// Called on application startup, Super Admin login, and daily by the background job.
    /// Returns the current LicenseStatus after the check.
    /// </summary>
    public async Task<LicenseStatus> ValidateCurrentAsync(CancellationToken ct = default)
    {
        var state = await _licenseRepo.GetCurrentAsync(ct);

        if (state is null)
        {
            _logger.LogWarning("No license found. Portal will operate in read-only mode.");
            return LicenseStatus.Invalid;
        }

        state.RefreshStatus();
        _licenseRepo.Update(state);
        await _licenseRepo.SaveChangesAsync(ct);

        _logger.LogInformation("License validation complete. Status={Status}", state.Status);
        return state.Status;
    }

    /// <summary>
    /// Verifies the RSA-SHA256 signature embedded in the license payload.
    /// The payload (licenseType + issuedAt + expiresAt) is reconstructed as a
    /// canonical string and checked against the Base64-encoded signature using
    /// the embedded public key.
    /// </summary>
    private bool VerifySignature(LicensePayload payload)
    {
        try
        {
            var canonical = $"{payload.LicenseType}|{payload.IssuedAt:O}|{payload.ExpiresAt?.ToString("O") ?? "permanent"}";
            var dataBytes = Encoding.UTF8.GetBytes(canonical);
            var signatureBytes = Convert.FromBase64String(payload.Signature);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(_publicKeyPem);
            return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Computes a SHA-256 hash of the raw license file bytes.
    /// Stored in LicenseState to detect file replacement between validation runs.
    /// </summary>
    private static string ComputeFileHash(byte[] bytes)
    {
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    // ── Internal DTO ──────────────────────────────────────────────────────────
    /// <summary>Maps directly to the JSON structure of the license file.</summary>
    private sealed class LicensePayload
    {
        public string LicenseType { get; set; } = default!;
        public DateTime IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string Signature { get; set; } = default!;
    }
}
