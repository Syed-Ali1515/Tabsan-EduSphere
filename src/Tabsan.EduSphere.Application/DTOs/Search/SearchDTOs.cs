namespace Tabsan.EduSphere.Application.DTOs.Search;

// ── Request ───────────────────────────────────────────────────────────────────

/// <summary>Validated search request passed to the search service.</summary>
public record SearchRequest(
    /// <summary>Caller's user ID — used for role-scoped filtering.</summary>
    Guid   CallerId,
    /// <summary>Caller's role name: SuperAdmin | Admin | Faculty | Student.</summary>
    string CallerRole,
    /// <summary>Raw search term (trimmed, min 2 chars enforced at controller).</summary>
    string Term,
    /// <summary>Maximum results to return across all categories combined.</summary>
    int    Limit = 20);

// ── Response ──────────────────────────────────────────────────────────────────

/// <summary>
/// A single search hit returned to the caller.
/// Intentionally flat so the portal can render any result without special-casing per type.
/// </summary>
public record SearchResultItem(
    /// <summary>
    /// Discriminator: Student | Course | CourseOffering | Faculty | Department.
    /// Used in the portal to group results by category tab.
    /// </summary>
    string Type,
    /// <summary>Underlying entity primary key.</summary>
    Guid   Id,
    /// <summary>Primary display text (e.g. student username / course title / faculty username / dept name).</summary>
    string Label,
    /// <summary>Secondary context text (e.g. registration number / course code / dept code / semester name).</summary>
    string SubLabel,
    /// <summary>Relative portal URL the result should link to (e.g. "/Portal/StudentProfile?id=...").</summary>
    string Url);

/// <summary>Top-level search response envelope.</summary>
public record SearchResponse(
    string                         Term,
    int                            TotalHits,
    IReadOnlyList<SearchResultItem> Results);
