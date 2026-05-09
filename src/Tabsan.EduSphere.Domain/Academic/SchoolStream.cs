using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.1

/// <summary>
/// Academic stream for school-mode institutions (e.g. Science, Commerce, Arts).
/// Streams are typically assigned to students in Grades 9–12 and constrain which
/// subjects are available for timetabling and result calculation.
/// </summary>
public class SchoolStream : AuditableEntity
{
    /// <summary>Human-readable stream name (e.g. "Science", "Commerce", "Arts").</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description of the stream and its subject set.</summary>
    public string? Description { get; private set; }

    /// <summary>Whether this stream is currently available for assignment.</summary>
    public bool IsActive { get; private set; } = true;

    private SchoolStream() { }

    public SchoolStream(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Stream name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
    }

    /// <summary>Updates the stream name, description, and active flag. Called by Admin/SuperAdmin.</summary>
    public void Update(string name, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Stream name is required.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        IsActive = isActive;
        Touch();
    }
}
