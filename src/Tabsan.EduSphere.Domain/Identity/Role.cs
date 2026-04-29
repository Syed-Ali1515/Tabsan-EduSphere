namespace Tabsan.EduSphere.Domain.Identity;

/// <summary>
/// Represents a system-defined role (e.g. Student, Faculty, Admin, SuperAdmin).
/// Roles are seeded at startup and cannot be deleted once assigned to users.
/// </summary>
public class Role
{
    /// <summary>Integer PK — roles are a small, fixed set so sequential int is fine.</summary>
    public int Id { get; private set; }

    /// <summary>Human-readable role name. Must be unique across the roles table.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description of the role's purpose and permissions.</summary>
    public string? Description { get; private set; }

    /// <summary>
    /// When true this role is part of the core system and cannot be renamed or deleted
    /// through the UI. Protects the four base roles from accidental modification.
    /// </summary>
    public bool IsSystemRole { get; private set; }

    // EF Core requires a parameterless constructor for materialisation.
    private Role() { }

    /// <summary>Factory constructor used during seeding and role creation.</summary>
    public Role(string name, string? description = null, bool isSystemRole = false)
    {
        Name = name;
        Description = description;
        IsSystemRole = isSystemRole;
    }
}
