// Final-Touches Phase 17 Stage 17.1/17.2/17.3 — Degree Audit DTOs

namespace Tabsan.EduSphere.Application.DTOs.Academic;

// ── Responses ────────────────────────────────────────────────────────────────

// Final-Touches Phase 17 Stage 17.1 — degree audit summary for a student
public class DegreeAuditResponse
{
    public Guid    StudentProfileId    { get; set; }
    public string  StudentName         { get; set; } = "";
    public string  RegistrationNumber  { get; set; } = "";
    public string  ProgramName         { get; set; } = "";
    public decimal Cgpa                { get; set; }

    public int     TotalCreditsEarned    { get; set; }
    public int     CoreCreditsEarned     { get; set; }
    public int     ElectiveCreditsEarned { get; set; }

    public bool    IsEligible           { get; set; }
    public List<string> UnmetRequirements { get; set; } = new();

    public List<EarnedCourseRow> CompletedCourses { get; set; } = new();
}

public class EarnedCourseRow
{
    public Guid    CourseId    { get; set; }
    public string  CourseCode  { get; set; } = "";
    public string  CourseTitle { get; set; } = "";
    public int     CreditHours { get; set; }
    public string  CourseType  { get; set; } = ""; // "Core" or "Elective"
    public decimal? GradePoint { get; set; }
}

// Final-Touches Phase 17 Stage 17.2 — degree rule response
public class DegreeRuleResponse
{
    public Guid    RuleId             { get; set; }
    public Guid    AcademicProgramId  { get; set; }
    public string  ProgramName        { get; set; } = "";
    public int     MinTotalCredits    { get; set; }
    public int     MinCoreCredits     { get; set; }
    public int     MinElectiveCredits { get; set; }
    public decimal MinGpa             { get; set; }
    public List<RequiredCourseItem> RequiredCourses { get; set; } = new();
}

public class RequiredCourseItem
{
    public Guid   CourseId    { get; set; }
    public string CourseCode  { get; set; } = "";
    public string CourseTitle { get; set; } = "";
}

// ── Requests ─────────────────────────────────────────────────────────────────

// Final-Touches Phase 17 Stage 17.2 — create/update degree rule
public class CreateDegreeRuleRequest
{
    public Guid    AcademicProgramId  { get; set; }
    public int     MinTotalCredits    { get; set; }
    public int     MinCoreCredits     { get; set; }
    public int     MinElectiveCredits { get; set; }
    public decimal MinGpa             { get; set; }
    public List<Guid> RequiredCourseIds { get; set; } = new();
}

public class UpdateDegreeRuleRequest
{
    public int     MinTotalCredits    { get; set; }
    public int     MinCoreCredits     { get; set; }
    public int     MinElectiveCredits { get; set; }
    public decimal MinGpa             { get; set; }
    public List<Guid> RequiredCourseIds { get; set; } = new();
}

// Final-Touches Phase 17 Stage 17.3 — set course type on a course
public class SetCourseTypeRequest
{
    public string CourseType { get; set; } = "Core"; // "Core" or "Elective"
}

// Final-Touches Phase 17 Stage 17.2 — eligibility summary for admin list
public class EligibilityListItem
{
    public Guid    StudentProfileId   { get; set; }
    public string  StudentName        { get; set; } = "";
    public string  RegistrationNumber { get; set; } = "";
    public decimal Cgpa               { get; set; }
    public int     TotalCreditsEarned { get; set; }
    public bool    IsEligible         { get; set; }
    public int     UnmetCount         { get; set; }
}
