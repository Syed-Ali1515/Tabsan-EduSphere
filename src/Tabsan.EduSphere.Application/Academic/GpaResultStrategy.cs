using System.Text.Json;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.1

/// <summary>
/// GPA/CGPA result calculation strategy for University-mode institutions.
/// Converts weighted component marks into a GPA on a 0.0–4.0 scale using
/// the configured GPA scale rules. The pass threshold is interpreted as a
/// minimum GPA (e.g. 2.0 = D).
/// </summary>
public sealed class GpaResultStrategy : IResultCalculationStrategy
{
    public InstitutionType AppliesTo => InstitutionType.University;

    public ResultSummary Calculate(
        IReadOnlyList<ComponentMark> marks,
        IReadOnlyList<GpaScaleRuleEntry> gpaScaleRules,
        decimal passThreshold,
        string? gradeRangesJson)
    {
        if (marks.Count == 0)
            return new ResultSummary(0m, null, 0m, "N/A", false);

        // Compute weighted total score as a percentage.
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

        // Map percentage to GPA using the configured scale.
        var gradePoint = ResolveGradePoint(percentage, gpaScaleRules);

        // Grade label: show GPA or "F" if below scale.
        var gradeLabel = gradePoint.HasValue
            ? gradePoint.Value.ToString("F2")
            : "F";

        var isPassing = gradePoint.HasValue && gradePoint.Value >= passThreshold;

        return new ResultSummary(totalScore, gradePoint, percentage, gradeLabel, isPassing);
    }

    private static decimal? ResolveGradePoint(decimal percentage, IReadOnlyList<GpaScaleRuleEntry> rules)
    {
        if (rules.Count == 0)
            return null;

        return rules
            .OrderByDescending(r => r.MinimumScore)
            .FirstOrDefault(r => percentage >= r.MinimumScore)
            ?.GradePoint;
    }
}
