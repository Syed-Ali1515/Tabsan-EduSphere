namespace Tabsan.EduSphere.Application.DTOs.Assignments;

public sealed record GpaScaleRuleDto(
    Guid? Id,
    decimal GradePoint,
    decimal MinimumScore,
    int DisplayOrder);

public sealed record ResultComponentRuleDto(
    Guid? Id,
    string Name,
    decimal Weightage,
    int DisplayOrder,
    bool IsActive = true);

public sealed record ResultCalculationSettingsResponse(
    IReadOnlyList<GpaScaleRuleDto> GpaScaleRules,
    IReadOnlyList<ResultComponentRuleDto> ComponentRules);

public sealed record SaveResultCalculationSettingsRequest(
    IReadOnlyList<GpaScaleRuleDto> GpaScaleRules,
    IReadOnlyList<ResultComponentRuleDto> ComponentRules);