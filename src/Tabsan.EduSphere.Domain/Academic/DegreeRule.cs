// Final-Touches Phase 17 Stage 17.2 — Degree rule entity defining graduation requirements per program

using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Defines the graduation requirements for an academic program.
/// One rule per program; evaluated by the Degree Audit service.
/// </summary>
public class DegreeRule : AuditableEntity
{
    // Final-Touches Phase 17 Stage 17.2 — program FK and requirement thresholds
    public Guid    AcademicProgramId  { get; private set; }
    public AcademicProgram AcademicProgram { get; private set; } = default!;

    /// <summary>Minimum total credit hours required to graduate.</summary>
    public int     MinTotalCredits    { get; private set; }

    /// <summary>Minimum core credit hours required.</summary>
    public int     MinCoreCredits     { get; private set; }

    /// <summary>Minimum elective credit hours required.</summary>
    public int     MinElectiveCredits { get; private set; }

    /// <summary>Minimum cumulative GPA required (0.0 – 4.0 scale).</summary>
    public decimal MinGpa             { get; private set; }

    private readonly List<DegreeRuleRequiredCourse> _requiredCourses = new();
    public IReadOnlyList<DegreeRuleRequiredCourse> RequiredCourses => _requiredCourses.AsReadOnly();

    protected DegreeRule() { }

    public static DegreeRule Create(
        Guid programId,
        int  minTotalCredits,
        int  minCoreCredits,
        int  minElectiveCredits,
        decimal minGpa)
    {
        if (minTotalCredits < 0)  throw new ArgumentOutOfRangeException(nameof(minTotalCredits));
        if (minCoreCredits  < 0)  throw new ArgumentOutOfRangeException(nameof(minCoreCredits));
        if (minElectiveCredits < 0) throw new ArgumentOutOfRangeException(nameof(minElectiveCredits));
        if (minGpa < 0m || minGpa > 4m) throw new ArgumentOutOfRangeException(nameof(minGpa));

        return new DegreeRule
        {
            Id                 = Guid.NewGuid(),
            AcademicProgramId  = programId,
            MinTotalCredits    = minTotalCredits,
            MinCoreCredits     = minCoreCredits,
            MinElectiveCredits = minElectiveCredits,
            MinGpa             = minGpa,
            CreatedAt          = DateTime.UtcNow
        };
    }

    public void Update(int minTotalCredits, int minCoreCredits, int minElectiveCredits, decimal minGpa)
    {
        if (minGpa < 0m || minGpa > 4m) throw new ArgumentOutOfRangeException(nameof(minGpa));
        MinTotalCredits    = minTotalCredits;
        MinCoreCredits     = minCoreCredits;
        MinElectiveCredits = minElectiveCredits;
        MinGpa             = minGpa;
        Touch();
    }

    public void AddRequiredCourse(Guid courseId)
    {
        if (_requiredCourses.Any(r => r.CourseId == courseId)) return;
        _requiredCourses.Add(DegreeRuleRequiredCourse.Create(Id, courseId));
    }

    public void RemoveRequiredCourse(Guid courseId)
    {
        var item = _requiredCourses.FirstOrDefault(r => r.CourseId == courseId);
        if (item is not null) _requiredCourses.Remove(item);
    }
}

/// <summary>
/// Specifies a single course that must be passed as part of a DegreeRule.
/// </summary>
public class DegreeRuleRequiredCourse : BaseEntity
{
    // Final-Touches Phase 17 Stage 17.2 — required course row
    public Guid   DegreeRuleId { get; private set; }
    public Guid   CourseId     { get; private set; }
    public Course Course       { get; private set; } = default!;

    protected DegreeRuleRequiredCourse() { }

    public static DegreeRuleRequiredCourse Create(Guid degreeRuleId, Guid courseId) => new()
    {
        Id          = Guid.NewGuid(),
        DegreeRuleId = degreeRuleId,
        CourseId    = courseId,
        CreatedAt   = DateTime.UtcNow
    };
}
