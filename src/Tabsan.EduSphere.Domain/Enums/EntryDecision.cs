namespace Tabsan.EduSphere.Domain.Enums;

// Phase 26 — School and College Functional Expansion — Stage 26.2

/// <summary>Per-student decision inside a bulk promotion batch.</summary>
public enum EntryDecision
{
    /// <summary>Student meets the criteria — advance to next grade/year/semester.</summary>
    Promote = 0,

    /// <summary>Student does not meet the criteria — retain in current grade/year/semester.</summary>
    Hold = 1,
}
