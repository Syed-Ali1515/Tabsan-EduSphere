namespace Tabsan.EduSphere.API.Services;

/// <summary>
/// Abstraction for persisting media/files so local disk can be swapped with object storage.
/// </summary>
public interface IMediaStorageService
{
    Task<MediaStorageSaveResult> SaveAsync(
        Stream content,
        string category,
        string fileExtension,
        CancellationToken ct = default);
}

public sealed record MediaStorageSaveResult(string StorageKey, string Reference);
