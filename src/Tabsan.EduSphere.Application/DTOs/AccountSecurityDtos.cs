namespace Tabsan.EduSphere.Application.Dtos;

/// <summary>Request body for admin to unlock a user account.</summary>
public record UnlockAccountRequest(Guid TargetUserId);

/// <summary>Request body for admin to reset a user's password.</summary>
public record AdminResetPasswordRequest(
    Guid TargetUserId,
    string NewPassword
);

/// <summary>Account lockout status for a user.</summary>
public record AccountLockoutStatusDto(
    Guid UserId,
    string Username,
    bool IsLockedOut,
    int FailedAttempts,
    DateTime? LockedOutUntil
);
