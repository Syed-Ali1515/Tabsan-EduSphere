using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Identity;

/// <summary>
/// Core user aggregate — represents every person who can authenticate with the portal
/// (Student, Faculty, Admin, SuperAdmin).
/// Password hashing, JWT, and session management are handled by the Infrastructure layer;
/// this entity only stores the hashed credential and profile state.
/// </summary>
public class User : AuditableEntity
{
    /// <summary>Unique login handle chosen during account creation.</summary>
    public string Username { get; private set; } = default!;

    /// <summary>Optional email address. Unique when provided (filtered index).</summary>
    public string? Email { get; private set; }

    /// <summary>BCrypt / ASP.NET Identity hashed password. Never stored in plain text.</summary>
    public string PasswordHash { get; private set; } = default!;

    /// <summary>FK to the Roles table. Determines all policy decisions for this user.</summary>
    public int RoleId { get; private set; }

    /// <summary>Navigation property for the user's assigned role.</summary>
    public Role Role { get; private set; } = default!;

    /// <summary>
    /// Optional FK to a department. Faculty accounts are always department-scoped;
    /// Admins and SuperAdmins have this set to null (they see everything).
    /// </summary>
    public Guid? DepartmentId { get; private set; }

    /// <summary>Controls login access without deleting the account or its data.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>UTC timestamp of the most recent successful authentication. Used in audit.</summary>
    public DateTime? LastLoginAt { get; private set; }

    private User() { }

    /// <summary>Creates a new user. Password must already be hashed by the caller.</summary>
    public User(string username, string passwordHash, int roleId, string? email = null, Guid? departmentId = null)
    {
        Username = username;
        PasswordHash = passwordHash;
        RoleId = roleId;
        Email = email;
        DepartmentId = departmentId;
    }

    /// <summary>Records a successful login by updating the LastLoginAt timestamp.</summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Replaces the stored hash with a newly generated hash after a password change.</summary>
    public void UpdatePasswordHash(string newHash)
    {
        PasswordHash = newHash;
        Touch();
    }

    /// <summary>Prevents the user from logging in. Their data is fully preserved.</summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>Re-enables login access for a previously deactivated account.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    /// <summary>Updates the user's email address. Pass null to clear it.</summary>
    public void UpdateEmail(string? email)
    {
        Email = email;
        Touch();
    }
}
