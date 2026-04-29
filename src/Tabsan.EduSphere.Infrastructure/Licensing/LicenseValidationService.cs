// ╔══════════════════════════════════════════════════════════════════╗
// ║  REPLACED IN PHASE 7 — now handles binary .tablic format        ║
// ╚══════════════════════════════════════════════════════════════════╝
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Licensing;

namespace Tabsan.EduSphere.Infrastructure.Licensing;

/// <summary>
/// Validates binary .tablic license files and manages the stored <see cref="LicenseState"/>.
///
/// .tablic binary layout
/// ─────────────────────
/// Offset   0 –   6 : Magic "TABLIC\x01" (7 bytes)
/// Offset   7 – 262 : RSA-2048 PKCS#1 v1.5 signature of SHA-256(IV + ciphertext) (256 bytes)
/// Offset 263 – 278 : AES-256-CBC IV (16 bytes)
/// Offset 279+      : AES-256-CBC encrypted JSON payload
///
/// Activation rules
/// ────────────────
/// 1. Magic header must match.
/// 2. RSA signature over SHA-256(IV + ciphertext) must verify with the embedded public key.
/// 3. AES-256-CBC decryption must succeed.
/// 4. verificationKeyHash must not already exist in the ConsumedVerificationKeys table.
/// </summary>
public class LicenseValidationService
{
    private readonly ILicenseRepository _licenseRepo;
    private readonly ILogger<LicenseValidationService> _logger;

    private static readonly byte[] _magic = "TABLIC\x01"u8.ToArray();
    private const int SignatureOffset  = 7;
    private const int SignatureLength  = 256;
    private const int IvOffset         = SignatureOffset + SignatureLength; // 263
    private const int IvLength         = 16;
    private const int CiphertextOffset = IvOffset + IvLength;              // 279
    private const int MinFileLength    = CiphertextOffset + 16;

    public LicenseValidationService(
        ILicenseRepository licenseRepo,
        ILogger<LicenseValidationService> logger)
    {
        _licenseRepo = licenseRepo;
        _logger      = logger;
    }

    /// <summary>
    /// Reads the .tablic file at <paramref name="licenseFilePath"/>, verifies the RSA
    /// signature, decrypts the AES payload, checks the VerificationKey has not been
    /// consumed, and then creates or replaces the <see cref="LicenseState"/> record.
    /// Returns true on success; false on any verification or format failure.
    /// </summary>
    public async Task<bool> ActivateFromFileAsync(string licenseFilePath, CancellationToken ct = default)
    {
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(licenseFilePath, ct);

            // 1. Magic header
            if (fileBytes.Length < MinFileLength || !fileBytes[..7].SequenceEqual(_magic))
            {
                _logger.LogWarning("License file has invalid magic header or is too short.");
                return false;
            }

            // 2. Extract components
            var signature  = fileBytes[SignatureOffset..IvOffset];
            var iv         = fileBytes[IvOffset..CiphertextOffset];
            var ciphertext = fileBytes[CiphertextOffset..];

            // 3. Verify RSA signature over SHA-256(IV + ciphertext)
            var signedData = new byte[iv.Length + ciphertext.Length];
            iv.CopyTo(signedData, 0);
            ciphertext.CopyTo(signedData, iv.Length);

            if (!VerifyRsaSignature(signedData, signature))
            {
                _logger.LogWarning("License file RSA signature verification failed. File may be tampered.");
                return false;
            }

            // 4. Decrypt payload
            byte[] plaintext;
            try { plaintext = DecryptAes(ciphertext, iv); }
            catch (CryptographicException)
            {
                _logger.LogWarning("License file AES decryption failed.");
                return false;
            }

            var json    = Encoding.UTF8.GetString(plaintext);
            var payload = JsonSerializer.Deserialize<TablicPayload>(json, _jsonOptions);

            if (payload is null || string.IsNullOrWhiteSpace(payload.LicenseType) ||
                string.IsNullOrWhiteSpace(payload.VerificationKeyHash))
            {
                _logger.LogWarning("License payload is missing required fields.");
                return false;
            }

            // 5. VerificationKey replay guard
            if (await _licenseRepo.IsVerificationKeyConsumedAsync(payload.VerificationKeyHash, ct))
            {
                _logger.LogWarning(
                    "VerificationKey '{Hash}' already consumed. Activation rejected.",
                    payload.VerificationKeyHash);
                return false;
            }

            // 6. Apply license state
            var fileHash    = ComputeFileHash(fileBytes);
            var licenseType = Enum.Parse<LicenseType>(payload.LicenseType, ignoreCase: true);

            var existing = await _licenseRepo.GetCurrentAsync(ct);
            if (existing is null)
                await _licenseRepo.AddAsync(new LicenseState(fileHash, licenseType, payload.ExpiresAt), ct);
            else
            {
                existing.Replace(fileHash, licenseType, payload.ExpiresAt);
                _licenseRepo.Update(existing);
            }

            await _licenseRepo.AddConsumedKeyAsync(
                new ConsumedVerificationKey(payload.VerificationKeyHash), ct);
            await _licenseRepo.SaveChangesAsync(ct);

            _logger.LogInformation(
                "License activated. Type={Type}, ExpiresAt={Expiry}",
                licenseType, payload.ExpiresAt?.ToString("O") ?? "never");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during license activation.");
            return false;
        }
    }

    /// <summary>
    /// Checks the stored <see cref="LicenseState"/> and refreshes its status.
    /// Called on startup, Super Admin login, and daily by the background job.
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

    // ── Crypto helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the RSA-PKCS#1 v1.5 SHA-256 signature over <paramref name="data"/>
    /// using the compile-time embedded public key from <see cref="EmbeddedKeys"/>.
    /// </summary>
    private static bool VerifyRsaSignature(byte[] data, byte[] signature)
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(EmbeddedKeys.RsaPublicKeyPem);
            var hash = SHA256.HashData(data);
            return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch { return false; }
    }

    /// <summary>
    /// Decrypts AES-256-CBC <paramref name="ciphertext"/> using the embedded AES key
    /// and the supplied <paramref name="iv"/>.
    /// </summary>
    private static byte[] DecryptAes(byte[] ciphertext, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key     = Convert.FromBase64String(EmbeddedKeys.AesKeyBase64);
        aes.IV      = iv;
        aes.Mode    = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        using var dec = aes.CreateDecryptor();
        return dec.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
    }

    /// <summary>Computes a SHA-256 hex hash of the raw file bytes for tamper detection.</summary>
    private static string ComputeFileHash(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    // ── Internal DTO ──────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Maps directly to the decrypted JSON payload inside a .tablic file.</summary>
    private sealed class TablicPayload
    {
        public string LicenseType { get; set; } = default!;
        public DateTime IssuedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string VerificationKeyHash { get; set; } = default!;
    }
}
