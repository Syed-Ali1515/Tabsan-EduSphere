using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Application.Academic;

/// <summary>
/// Applies school-stream subject filtering for Grade 9-12 student offering views.
/// Filtering is keyword-based to avoid schema changes while supporting stream-specific subject visibility.
/// </summary>
public static class SchoolStreamSubjectFilter
{
    private static readonly string[] CoreSubjectKeywords =
    [
        "english", "urdu", "islam", "pakistan", "civics"
    ];

    public static IReadOnlyList<CourseOffering> FilterOfferingsByStream(
        IReadOnlyList<CourseOffering> offerings,
        string? streamName)
    {
        if (offerings.Count == 0 || string.IsNullOrWhiteSpace(streamName))
            return offerings;

        var streamKeywords = ResolveStreamKeywords(streamName);
        if (streamKeywords.Count == 0)
            return offerings;

        var filtered = offerings
            .Where(o => MatchesAnyKeyword(o, streamKeywords) || MatchesAnyKeyword(o, CoreSubjectKeywords))
            .ToList();

        // Keep compatibility for datasets that do not yet include stream-specific naming.
        return filtered.Count > 0 ? filtered : offerings;
    }

    private static bool MatchesAnyKeyword(CourseOffering offering, IReadOnlyCollection<string> keywords)
    {
        var title = offering.Course.Title ?? string.Empty;
        var code = offering.Course.Code ?? string.Empty;
        return keywords.Any(k =>
            title.Contains(k, StringComparison.OrdinalIgnoreCase)
            || code.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<string> ResolveStreamKeywords(string streamName)
    {
        var name = streamName.Trim().ToLowerInvariant();

        if (name.Contains("science", StringComparison.Ordinal))
            return ["science", "physics", "chemistry", "biology", "math", "computer"];

        if (name.Contains("biology", StringComparison.Ordinal))
            return ["biology", "botany", "zoology", "chemistry", "science", "math"];

        if (name.Contains("computer", StringComparison.Ordinal))
            return ["computer", "ict", "informatics", "programming", "software", "it", "math"];

        if (name.Contains("commerce", StringComparison.Ordinal))
            return ["commerce", "account", "business", "economics", "finance", "management", "math"];

        if (name.Contains("arts", StringComparison.Ordinal))
            return ["arts", "history", "geography", "sociology", "literature", "humanities", "psychology"];

        return [];
    }
}