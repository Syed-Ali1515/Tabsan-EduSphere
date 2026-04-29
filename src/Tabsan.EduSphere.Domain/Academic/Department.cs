using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Represents a university department (e.g. Computer Science, Business Administration).
/// Department is the core organisational unit — faculty, courses, programs, and student
/// data are all scoped to a department.
/// </summary>
public class Department : AuditableEntity
{
    /// <summary>Full name of the department as displayed in the UI.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Short unique code used in registration numbers and reports (e.g. "CS", "BBA").
    /// Unique constraint is enforced at the database level.
    /// </summary>
    public string Code { get; private set; } = default!;

    /// <summary>Controls whether the department is available for assignment and enrolment.</summary>
    public bool IsActive { get; private set; } = true;

    private Department() { }

    public Department(string name, string code)
    {
        Name = name;
        Code = code.ToUpperInvariant();
    }

    /// <summary>Updates the display name of the department.</summary>
    public void Rename(string newName)
    {
        Name = newName;
        Touch();
    }

    /// <summary>Deactivates the department so it is hidden from assignment dropdowns.</summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>Re-activates a previously deactivated department.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
