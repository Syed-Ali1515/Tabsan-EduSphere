namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Abstraction for file/media persistence so local disk can be swapped with object storage.
/// </summary>
public interface IMediaStorageService
{
    Task<MediaStorageSaveResult> SaveAsync(
        Stream content,
        string category,
        string fileExtension,
        CancellationToken ct = default);

    Task<byte[]?> ReadAsBytesAsync(string storageKey, CancellationToken ct = default);
}

public sealed record MediaStorageSaveResult(string StorageKey, string Reference);
