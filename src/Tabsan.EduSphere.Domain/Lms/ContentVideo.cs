using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Lms;

// Final-Touches Phase 20 Stage 20.2 — video attachment for a course content module

/// <summary>
/// A video resource attached to a <see cref="CourseContentModule"/>.
/// Supports both a direct storage URL (uploaded file) and an embed URL (YouTube, Vimeo, etc.).
/// </summary>
public class ContentVideo : AuditableEntity
{
    /// <summary>FK to the parent module.</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>Display title shown below/beside the video.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Direct URL to the uploaded video file (blob storage, CDN, etc.).</summary>
    public string? StorageUrl { get; private set; }

    /// <summary>Embed URL (e.g. YouTube /embed/ link) rendered inside an iframe.</summary>
    public string? EmbedUrl { get; private set; }

    /// <summary>Approximate duration in seconds (optional).</summary>
    public int? DurationSeconds { get; private set; }

    // Navigation
    public CourseContentModule Module { get; private set; } = default!;

    private ContentVideo() { }

    /// <summary>Creates a new video entry attached to a module.</summary>
    public ContentVideo(Guid moduleId, string title, string? storageUrl, string? embedUrl, int? durationSeconds = null)
    {
        if (string.IsNullOrWhiteSpace(storageUrl) && string.IsNullOrWhiteSpace(embedUrl))
            throw new ArgumentException("Either a storage URL or an embed URL must be provided.");

        ModuleId        = moduleId;
        Title           = title.Trim();
        StorageUrl      = storageUrl;
        EmbedUrl        = embedUrl;
        DurationSeconds = durationSeconds;
    }

    /// <summary>Updates video metadata.</summary>
    public void Update(string title, string? storageUrl, string? embedUrl, int? durationSeconds)
    {
        Title           = title.Trim();
        StorageUrl      = storageUrl;
        EmbedUrl        = embedUrl;
        DurationSeconds = durationSeconds;
        Touch();
    }
}
