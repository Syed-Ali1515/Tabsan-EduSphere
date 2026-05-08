using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.StudyPlanner;

// Final-Touches Phase 21 Stage 21.1 — Study Planner: student semester study plan entity

/// <summary>
/// Lifecycle state of a study plan as seen by a faculty advisor.
/// </summary>
public enum StudyPlanStatus
{
    /// <summary>Awaiting faculty advisor review.</summary>
    Pending  = 0,
    /// <summary>Faculty advisor has endorsed the plan.</summary>
    Endorsed = 1,
    /// <summary>Faculty advisor has rejected the plan and left notes.</summary>
    Rejected = 2
}

/// <summary>
/// A semester-level study plan created by a student.
/// Lists the courses the student intends to take in a future semester.
/// Faculty advisors can endorse or reject the plan and leave notes.
/// </summary>
public class StudyPlan : AuditableEntity
{
    /// <summary>FK to the student profile who owns this plan.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Human-readable semester label (e.g. "Fall 2026", "Spring 2027").</summary>
    public string PlannedSemesterName { get; private set; } = default!;

    /// <summary>Optional notes from the student about this plan.</summary>
    public string? Notes { get; private set; }

    /// <summary>Endorsement status: Pending / Endorsed / Rejected.</summary>
    public StudyPlanStatus AdvisorStatus { get; private set; } = StudyPlanStatus.Pending;

    /// <summary>Notes left by the faculty advisor when endorsing or rejecting.</summary>
    public string? AdvisorNotes { get; private set; }

    /// <summary>UserId of the faculty member who reviewed this plan. Null until reviewed.</summary>
    public Guid? ReviewedByUserId { get; private set; }

    private readonly List<StudyPlanCourse> _courses = new();

    /// <summary>Courses included in this study plan.</summary>
    public IReadOnlyList<StudyPlanCourse> Courses => _courses.AsReadOnly();

    private StudyPlan() { }

    public StudyPlan(Guid studentProfileId, string plannedSemesterName, string? notes = null)
    {
        StudentProfileId   = studentProfileId;
        PlannedSemesterName = plannedSemesterName.Trim();
        Notes              = notes?.Trim();
    }

    /// <summary>Adds a course to the plan. Silently ignores duplicates.</summary>
    public void AddCourse(Guid courseId)
    {
        if (_courses.Any(c => c.CourseId == courseId)) return;
        _courses.Add(new StudyPlanCourse(Id, courseId));
        Touch();
    }

    /// <summary>Removes a course from the plan by its course ID.</summary>
    public bool RemoveCourse(Guid courseId)
    {
        var item = _courses.FirstOrDefault(c => c.CourseId == courseId);
        if (item is null) return false;
        _courses.Remove(item);
        Touch();
        return true;
    }

    /// <summary>Updates the student-written notes.</summary>
    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        Touch();
    }

    /// <summary>Faculty advisor endorses the plan.</summary>
    public void Endorse(Guid advisorUserId, string? advisorNotes)
    {
        AdvisorStatus      = StudyPlanStatus.Endorsed;
        ReviewedByUserId   = advisorUserId;
        AdvisorNotes       = advisorNotes?.Trim();
        Touch();
    }

    /// <summary>Faculty advisor rejects the plan with mandatory notes.</summary>
    public void Reject(Guid advisorUserId, string advisorNotes)
    {
        AdvisorStatus      = StudyPlanStatus.Rejected;
        ReviewedByUserId   = advisorUserId;
        AdvisorNotes       = advisorNotes.Trim();
        Touch();
    }

    /// <summary>Resets advisor decision back to Pending (e.g. after student modifies plan).</summary>
    public void ResetAdvisorStatus()
    {
        AdvisorStatus    = StudyPlanStatus.Pending;
        AdvisorNotes     = null;
        ReviewedByUserId = null;
        Touch();
    }
}
