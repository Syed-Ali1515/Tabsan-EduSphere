using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 25 — Academic Engine Unification — Stage 25.1

/// <summary>
/// An individual component mark used as input to a result calculation strategy.
/// </summary>
/// <param name="Name">The assessment component name (e.g. "Quiz", "Midterm", "Final").</param>
/// <param name="MarksObtained">Raw marks obtained by the student for this component.</param>
/// <param name="MaxMarks">Maximum possible marks for this component.</param>
/// <param name="Weightage">Configured percentage weighting for this component (sums to 100 across all active components).</param>
public sealed record ComponentMark(string Name, decimal MarksObtained, decimal MaxMarks, decimal Weightage);

/// <summary>
/// The computed result summary produced by a calculation strategy.
/// </summary>
/// <param name="TotalScore">Weighted total score as a percentage (0–100).</param>
/// <param name="GradePoint">GPA on 0.0–4.0 scale; null for Percentage-mode strategies.</param>
/// <param name="Percentage">Percentage score; null for pure GPA strategies that do not expose it.</param>
/// <param name="GradeLabel">Display label such as "A+", "B", "Distinction" or "3.5 GPA".</param>
/// <param name="IsPassing">True when the score meets or exceeds the applicable pass threshold.</param>
public sealed record ResultSummary(
    decimal TotalScore,
    decimal? GradePoint,
    decimal? Percentage,
    string GradeLabel,
    bool IsPassing);

/// <summary>
/// Strategy contract for computing a result summary from component marks.
/// Each institution mode (University / School / College) provides an implementation.
/// The existing <see cref="ResultService"/> is NOT changed — strategies are consumed by
/// the <see cref="IProgressionService"/> and future orchestration services.
/// </summary>
public interface IResultCalculationStrategy
{
    /// <summary>The institution type this strategy is designed for.</summary>
    InstitutionType AppliesTo { get; }

    /// <summary>
    /// Computes a result summary from the provided component marks and grading profile.
    /// </summary>
    /// <param name="marks">The assessed component marks for the student in one course offering.</param>
    /// <param name="gpaScaleRules">GPA scale rules (only consumed by GPA-mode strategies).</param>
    /// <param name="passThreshold">Pass threshold — interpretation depends on the strategy mode (GPA or percentage).</param>
    /// <param name="gradeRangesJson">
    ///   Optional JSON array of grade band objects:
    ///   [{"From":90,"To":100,"Label":"A+"},{"From":80,"To":89,"Label":"A"},...]
    ///   Null means the strategy uses its own default bands.
    /// </param>
    ResultSummary Calculate(
        IReadOnlyList<ComponentMark> marks,
        IReadOnlyList<GpaScaleRuleEntry> gpaScaleRules,
        decimal passThreshold,
        string? gradeRangesJson);
}

/// <summary>Lightweight GPA scale rule used by strategy calculations (no EF dependency).</summary>
/// <param name="GradePoint">GPA value (0.0–4.0).</param>
/// <param name="MinimumScore">Minimum percentage score to achieve this grade point.</param>
public sealed record GpaScaleRuleEntry(decimal GradePoint, decimal MinimumScore);

/// <summary>Lightweight grade band used by percentage-mode strategies.</summary>
/// <param name="From">Minimum score (inclusive) for this band.</param>
/// <param name="To">Maximum score (inclusive) for this band.</param>
/// <param name="Label">Display label for the band (e.g. "A+", "Distinction").</param>
public sealed record GradeBandEntry(decimal From, decimal To, string Label);
