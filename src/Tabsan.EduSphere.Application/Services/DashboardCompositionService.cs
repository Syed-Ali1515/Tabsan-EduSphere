using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Stateless implementation of <see cref="IDashboardCompositionService"/>.
/// Widget catalogue is a pure function of role + institution policy.
/// </summary>
public sealed class DashboardCompositionService : IDashboardCompositionService
{
    // ── full widget catalogue (key, title, icon, order) ──────────────────────
    private static readonly WidgetDescriptor[] _all =
    [
        new("system_health",       "System Health",          "bi-heart-pulse",        1),
        new("enrollment_stats",    "Enrolment Overview",     "bi-people",             2),
        new("academic_overview",   "Academic Overview",      "bi-mortarboard",        3),
        new("helpdesk_queue",      "Helpdesk Queue",         "bi-headset",            4),
        new("faculty_assignments", "My Teaching Load",       "bi-journal-text",       5),
        new("pending_results",     "Pending Results",        "bi-clipboard2-check",   6),
        new("fyp_panel",           "FYP / Projects",         "bi-diagram-3",          7),
        new("my_courses",          "My Courses",             "bi-book",               8),
        new("attendance_summary",  "Attendance Summary",     "bi-calendar2-check",    9),
        new("ai_assistant",        "AI Study Assistant",     "bi-robot",              10),
    ];

    /// <inheritdoc/>
    public IReadOnlyList<WidgetDescriptor> GetWidgets(
        string role,
        InstitutionPolicySnapshot policy)
    {
        bool isSuperAdmin = Is(role, "SuperAdmin");
        bool isAdmin      = Is(role, "Admin");
        bool isFaculty    = Is(role, "Faculty");
        bool isStudent    = Is(role, "Student");
        bool hasUniv      = policy.IsEnabled(InstitutionType.University);

        var result = new List<WidgetDescriptor>();

        foreach (var w in _all)
        {
            var include = w.Key switch
            {
                "system_health"       => isSuperAdmin,
                "enrollment_stats"    => isSuperAdmin || isAdmin,
                "academic_overview"   => isSuperAdmin || isAdmin,
                "helpdesk_queue"      => isSuperAdmin || isAdmin,
                "faculty_assignments" => isFaculty,
                "pending_results"     => isFaculty,
                "fyp_panel"           => (isFaculty || isStudent) && hasUniv,
                "my_courses"          => isStudent,
                "attendance_summary"  => isFaculty || isStudent,
                "ai_assistant"        => true,          // all roles
                _                     => false
            };

            if (include) result.Add(w);
        }

        return result.AsReadOnly();
    }

    private static bool Is(string role, string target)
        => string.Equals(role, target, StringComparison.OrdinalIgnoreCase);
}
