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

    /// <summary>Number of consecutive failed login attempts. Reset to 0 on successful login.</summary>
    public int FailedLoginAttempts { get; private set; } = 0;

    /// <summary>Whether the user account is locked due to too many failed login attempts.</summary>
    public bool IsLockedOut { get; private set; } = false;

    /// <summary>UTC timestamp when the lock will automatically expire. Null if not locked or if unlocked manually.</summary>
    public DateTime? LockedOutUntil { get; private set; }

    /// <summary>
    /// The UI theme key selected by this user (e.g. "dark", "light", "high-contrast").
    /// Null means the system default theme is used.
    /// </summary>
    public string? ThemeKey { get; private set; }

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
        FailedLoginAttempts = 0;
        IsLockedOut = false;
        LockedOutUntil = null;
        Touch();
    }

    /// <summary>Records a failed login attempt and optionally locks the account if the threshold is exceeded.</summary>
    /// <param name="maxFailedAttempts">Maximum consecutive failed attempts before lockout (default 5).</param>
    /// <param name="lockoutDurationMinutes">Duration of lockout in minutes (default 15).</param>
    public void RecordFailedLoginAttempt(int maxFailedAttempts = 5, int lockoutDurationMinutes = 15)
    {
        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxFailedAttempts)
        {
            IsLockedOut = true;
            LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
        }
        Touch();
    }

    /// <summary>Manually unlocks the account and resets failed login attempts. Used by admin intervention.</summary>
    public void UnlockAccount()
    {
        IsLockedOut = false;
        FailedLoginAttempts = 0;
        LockedOutUntil = null;
        Touch();
    }

    /// <summary>Checks if the account is currently locked out (considering automatic expiration).</summary>
    public bool IsCurrentlyLockedOut()
    {
        if (!IsLockedOut)
            return false;

        if (LockedOutUntil.HasValue && DateTime.UtcNow >= LockedOutUntil)
        {
            // Auto-unlock after lockout duration expires
            IsLockedOut = false;
            FailedLoginAttempts = 0;
            LockedOutUntil = null;
            return false;
        }

        return true;
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

    /// <summary>Sets the user's preferred UI theme key. Pass null to revert to the system default.</summary>
    public void SetTheme(string? themeKey)
    {
        ThemeKey = themeKey?.Trim().ToLowerInvariant();
        Touch();
    }
}
