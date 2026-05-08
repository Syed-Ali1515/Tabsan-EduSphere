using System.Text.Json;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.1

/// <summary>
/// Percentage-based result calculation strategy for School and College-mode institutions.
/// Converts weighted component marks into a percentage score and resolves a grade label
/// from the configured grade bands (or built-in defaults). The pass threshold is
/// interpreted as a minimum percentage (e.g. 40 = 40%).
/// </summary>
public sealed class PercentageResultStrategy : IResultCalculationStrategy
{
    // Default grade bands when no custom bands are supplied.
    private static readonly IReadOnlyList<GradeBandEntry> DefaultBands =
    [
        new(90m, 100m, "A+"),
        new(80m, 89.99m, "A"),
        new(70m, 79.99m, "B+"),
        new(60m, 69.99m, "B"),
        new(50m, 59.99m, "C"),
        new(40m, 49.99m, "D"),
        new(0m, 39.99m, "F"),
    ];

    private readonly InstitutionType _appliesTo;

    public PercentageResultStrategy(InstitutionType appliesTo)
    {
        if (appliesTo is not (InstitutionType.School or InstitutionType.College))
            throw new ArgumentException("PercentageResultStrategy applies only to School or College.", nameof(appliesTo));
        _appliesTo = appliesTo;
    }

    public InstitutionType AppliesTo => _appliesTo;

    public ResultSummary Calculate(
        IReadOnlyList<ComponentMark> marks,
        IReadOnlyList<GpaScaleRuleEntry> gpaScaleRules,
        decimal passThreshold,
        string? gradeRangesJson)
    {
        if (marks.Count == 0)
            return new ResultSummary(0m, null, 0m, "N/A", false);

        // Compute weighted total score.
        decimal totalWeightedScore = 0m;
        decimal totalWeight = 0m;

        foreach (var m in marks)
        {
            if (m.MaxMarks <= 0 || m.Weightage <= 0)
                continue;
            totalWeightedScore += (m.MarksObtained / m.MaxMarks) * m.Weightage;
            totalWeight += m.Weightage;
        }

        var percentage = totalWeight > 0
            ? Math.Round(totalWeightedScore / totalWeight * 100m, 2)
            : 0m;

        var totalScore = totalWeight > 0
            ? Math.Round(totalWeightedScore, 2)
            : 0m;

        // Resolve grade label from supplied or default bands.
        var bands = ParseBands(gradeRangesJson) ?? DefaultBands;
        var gradeLabel = ResolveBand(percentage, bands);
        var isPassing = percentage >= passThreshold;

        return new ResultSummary(totalScore, null, percentage, gradeLabel, isPassing);
    }

    private static IReadOnlyList<GradeBandEntry>? ParseBands(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var list = JsonSerializer.Deserialize<List<GradeBandJson>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return list?
                .Select(b => new GradeBandEntry(b.From, b.To, b.Label))
                .OrderByDescending(b => b.From)
                .ToList()
                .AsReadOnly();
        }
        catch (JsonException)
        {
            // Invalid JSON — fall back to defaults.
            return null;
        }
    }

    private static string ResolveBand(decimal percentage, IReadOnlyList<GradeBandEntry> bands)
    {
        foreach (var band in bands.OrderByDescending(b => b.From))
        {
            if (percentage >= band.From && percentage <= band.To)
                return band.Label;
        }
        return "F";
    }

    // Private deserialization model matching the JSON schema.
    private sealed record GradeBandJson(decimal From, decimal To, string Label);
}
