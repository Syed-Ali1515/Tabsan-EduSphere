// Final-Touches Phase 16 Stage 16.2 — DTOs for rubric CRUD and student grading

namespace Tabsan.EduSphere.Application.DTOs.Assignments;

// ── Rubric read DTOs ──────────────────────────────────────────────────────────

public sealed class RubricResponse
{
    public Guid                       RubricId     { get; init; }
    public Guid                       AssignmentId { get; init; }
    public string                     Title        { get; init; } = string.Empty;
    public bool                       IsActive     { get; init; }
    public List<RubricCriterionDto>   Criteria     { get; init; } = new();
}

public sealed class RubricCriterionDto
{
    public Guid                 CriterionId  { get; init; }
    public string               Name         { get; init; } = string.Empty;
    public decimal              MaxPoints    { get; init; }
    public int                  DisplayOrder { get; init; }
    public List<RubricLevelDto> Levels       { get; init; } = new();
}

public sealed class RubricLevelDto
{
    public Guid    LevelId      { get; init; }
    public string  Label        { get; init; } = string.Empty;
    public decimal PointsAwarded{ get; init; }
    public int     DisplayOrder { get; init; }
}

// ── Rubric write DTOs ─────────────────────────────────────────────────────────

public sealed class CreateRubricRequest
{
    public Guid                          AssignmentId { get; init; }
    public string                        Title        { get; init; } = string.Empty;
    public List<CreateCriterionRequest>  Criteria     { get; init; } = new();
}

public sealed class CreateCriterionRequest
{
    public string                      Name         { get; init; } = string.Empty;
    public decimal                     MaxPoints    { get; init; }
    public int                         DisplayOrder { get; init; }
    public List<CreateLevelRequest>    Levels       { get; init; } = new();
}

public sealed class CreateLevelRequest
{
    public string  Label         { get; init; } = string.Empty;
    public decimal PointsAwarded { get; init; }
    public int     DisplayOrder  { get; init; }
}

public sealed class UpdateRubricRequest
{
    public string Title { get; init; } = string.Empty;
}

// ── Student rubric grading DTOs ────────────────────────────────────────────────

public sealed class RubricGradeRequest
{
    // Final-Touches Phase 16 Stage 16.2 — submit per-criterion level selections
    public Guid                           SubmissionId { get; init; }
    public List<RubricCriterionGradeDto>  Grades       { get; init; } = new();
}

public sealed class RubricCriterionGradeDto
{
    public Guid CriterionId { get; init; }
    public Guid LevelId     { get; init; }
}

/// <summary>Returns the complete rubric grade breakdown for a student submission.</summary>
public sealed class RubricGradeResponse
{
    public Guid                               SubmissionId    { get; init; }
    public Guid                               RubricId        { get; init; }
    public string                             RubricTitle     { get; init; } = string.Empty;
    public decimal                            TotalPoints     { get; init; }
    public decimal                            MaxTotalPoints  { get; init; }
    public List<RubricCriterionGradeResult>   CriteriaResults { get; init; } = new();
}

public sealed class RubricCriterionGradeResult
{
    public Guid    CriterionId   { get; init; }
    public string  CriterionName { get; init; } = string.Empty;
    public decimal MaxPoints     { get; init; }
    public Guid?   ChosenLevelId { get; init; }
    public string? ChosenLabel   { get; init; }
    public decimal PointsAwarded { get; init; }
}
