using Microsoft.Extensions.Options;

namespace Tabsan.EduSphere.API.Services;

public sealed class LocalMediaStorageService : IMediaStorageService
{
    private readonly MediaStorageOptions _options;
    private readonly string _contentRootPath;

    public LocalMediaStorageService(
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

        var normalizedExtension = NormalizeExtension(fileExtension);
        var normalizedCategory = NormalizePathSegment(category);
        var normalizedPrefix = NormalizePrefix(_options.KeyPrefix);

        var objectKey = $"{normalizedPrefix}{normalizedCategory}/{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid():N}{normalizedExtension}";
        var fullPath = Path.Combine(GetRootPath(), objectKey.Replace('/', Path.DirectorySeparatorChar));

        var parentDir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(parentDir))
        {
            Directory.CreateDirectory(parentDir);
        }

        await using var destination = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(destination, ct);

        var reference = BuildReference(objectKey);
        return new MediaStorageSaveResult(objectKey, reference);
    }

    private string GetRootPath()
    {
        var configured = _options.LocalRootPath;
        if (string.IsNullOrWhiteSpace(configured)) configured = "uploads";

        return Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(_contentRootPath, configured);
    }

    private string BuildReference(string objectKey)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return objectKey;

        var baseUrl = _options.PublicBaseUrl!.TrimEnd('/');
        return $"{baseUrl}/{objectKey}";
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
        if (string.IsNullOrWhiteSpace(normalized)) return string.Empty;
        return normalized + "/";
    }
}
