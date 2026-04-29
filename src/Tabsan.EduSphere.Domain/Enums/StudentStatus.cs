namespace Tabsan.EduSphere.Domain.Enums;

/// <summary>
/// Represents the lifecycle status of a student in the system.
/// </summary>
public enum StudentStatus
{
    /// <summary>Student is actively enrolled and participating.</summary>
    Active = 1,

    /// <summary>Student has been marked as inactive (dropout, leave of absence, etc.); cannot login.</summary>
    Inactive = 2,

    /// <summary>Student has completed their program and been formally graduated; read-only access only.</summary>
    Graduated = 3
}
