namespace Tabsan.EduSphere.API.Services;

public sealed class MediaStorageOptions
{
    public const string SectionName = "MediaStorage";

    public string Provider { get; set; } = "Local";

    // Relative paths are rooted to the API content root.
    public string LocalRootPath { get; set; } = "uploads";

    // Relative paths are rooted to the API content root.
    public string BlobRootPath { get; set; } = "blob-storage";

    // Optional external URL prefix for clients that should consume CDN/public URLs.
    public string? PublicBaseUrl { get; set; }

    // Optional key prefix (e.g., tenant or environment), applied to all stored objects.
    public string? KeyPrefix { get; set; }
}
