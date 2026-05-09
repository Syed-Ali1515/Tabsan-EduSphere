using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

/// <summary>
/// Object-storage style adapter that uses a dedicated root path and key semantics.
/// This keeps the contract ready for real cloud backends without changing callers.
/// </summary>
public sealed class BlobMediaStorageService : IMediaStorageService
{
    private readonly MediaStorageOptions _options;
    private readonly string _contentRootPath;

    public BlobMediaStorageService(
        IOptions<MediaStorageOptions> options,
        IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _contentRootPath = hostEnvironment.ContentRootPath;
    }

    public async Task<MediaStorageSaveResult> SaveAsync(
        Stream content,
        string category,
        string fileExtension,
        CancellationToken ct = default)
    {
        if (content is null) throw new ArgumentNullException(nameof(content));
        if (string.IsNullOrWhiteSpace(category)) throw new ArgumentException("Category is required.", nameof(category));

        var extension = NormalizeExtension(fileExtension);
        var keyPrefix = NormalizePrefix(_options.KeyPrefix);
        var segment = NormalizePathSegment(category);
        var key = $"{keyPrefix}{segment}/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}{extension}";

        var fullPath = ResolveFullPath(key);
        var parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(parent))
            Directory.CreateDirectory(parent);

        await using var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(destination, ct);
        await destination.FlushAsync(ct);

        return new MediaStorageSaveResult(
            key,
            BuildReference(key),
            ResolveContentType(key),
            destination.Length);
    }

    public async Task<byte[]?> ReadAsBytesAsync(string storageKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) return null;

        var fullPath = ResolveFullPath(storageKey);
        if (!File.Exists(fullPath)) return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }

    public Task<MediaStorageObjectMetadata?> GetMetadataAsync(string storageKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return Task.FromResult<MediaStorageObjectMetadata?>(null);

        var fullPath = ResolveFullPath(storageKey);
        if (!File.Exists(fullPath))
            return Task.FromResult<MediaStorageObjectMetadata?>(null);

        var info = new FileInfo(fullPath);
        var metadata = new MediaStorageObjectMetadata(storageKey, ResolveContentType(storageKey), info.Length);
        return Task.FromResult<MediaStorageObjectMetadata?>(metadata);
    }

    public Task<string?> GenerateTemporaryReadUrlAsync(
        string storageKey,
        TimeSpan ttl,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return Task.FromResult<string?>(null);

        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return Task.FromResult<string?>(null);

        // Final-Touches Phase 28 Stage 28.3 — generate provider-backed temporary signed URL.
        var baseUrl = _options.PublicBaseUrl!.TrimEnd('/');
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl <= TimeSpan.Zero ? TimeSpan.FromMinutes(5) : ttl).ToUnixTimeSeconds();
        var unsignedUrl = $"{baseUrl}/{storageKey}?exp={expiresAt}";
        var signature = CreateSignature(storageKey, expiresAt);

        var url = signature is null
            ? unsignedUrl
            : $"{unsignedUrl}&sig={Uri.EscapeDataString(signature)}";

        return Task.FromResult<string?>(url);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) return Task.CompletedTask;

        var fullPath = ResolveFullPath(storageKey);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string ResolveFullPath(string key)
    {
        var root = GetRootPath();
        return Path.Combine(root, key.Replace('/', Path.DirectorySeparatorChar));
    }

    private string GetRootPath()
    {
        var configured = _options.BlobRootPath;
        if (string.IsNullOrWhiteSpace(configured))
            configured = "blob-storage";

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(_contentRootPath, configured);
    }

    private string BuildReference(string key)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return key;

        var baseUrl = _options.PublicBaseUrl!.TrimEnd('/');
        return $"{baseUrl}/{key}";
    }

    private static string NormalizeExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension)) return string.Empty;
        return extension.StartsWith('.') ? extension.ToLowerInvariant() : $".{extension.ToLowerInvariant()}";
    }

    private static string NormalizePathSegment(string value)
    {
        var normalized = value.Trim().Replace('\\', '/').Trim('/');
        return string.IsNullOrWhiteSpace(normalized) ? "misc" : normalized;
    }

    private static string NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return string.Empty;
        var normalized = prefix.Trim().Replace('\\', '/').Trim('/');
        return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized + "/";
    }

    private string? CreateSignature(string storageKey, long expiresAt)
    {
        var secret = _options.SignedUrlSecret;
        if (string.IsNullOrWhiteSpace(secret))
            return null;

        var payload = $"{storageKey}|{expiresAt}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string ResolveContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".tablic" => "application/octet-stream",
            _ => "application/octet-stream"
        };
    }
}
