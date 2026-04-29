using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Implements account lockout management and admin-driven password reset.
/// Lockout is applied by AuthService on failed login; this service provides
/// admin visibility and unlock/reset capabilities.
/// Only non-admin accounts are subject to automated lockout policy.
/// </summary>
public class AccountSecurityService : IAccountSecurityService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;

    public AccountSecurityService(IUserRepository userRepo, IPasswordHasher passwordHasher)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
    }

    public async Task<AccountLockoutStatusDto?> GetLockoutStatusAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null)
            return null;

        return new AccountLockoutStatusDto(
            user.Id,
            user.Username,
            user.IsLockedOut,
            user.FailedLoginAttempts,
            user.LockedOutUntil
        );
    }

    public async Task UnlockAccountAsync(Guid targetUserId, Guid adminUserId, CancellationToken ct = default)
    {
        var target = await _userRepo.GetByIdAsync(targetUserId, ct);
        if (target is null)
            throw new KeyNotFoundException($"User {targetUserId} not found.");

        // Admin accounts cannot be managed through this endpoint
        if (target.Role?.Name is "Admin" or "SuperAdmin")
            throw new InvalidOperationException("Admin accounts cannot be unlocked through this endpoint.");

        target.UnlockAccount();
        _userRepo.Update(target);
        await _userRepo.SaveChangesAsync(ct);
    }

    public async Task ResetPasswordAsync(
        AdminResetPasswordRequest request,
        Guid adminUserId,
        CancellationToken ct = default)
    {
        var target = await _userRepo.GetByIdAsync(request.TargetUserId, ct);
        if (target is null)
            throw new KeyNotFoundException($"User {request.TargetUserId} not found.");

        if (target.Role?.Name is "Admin" or "SuperAdmin")
            throw new InvalidOperationException("Admin account passwords cannot be reset through this endpoint.");

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
            throw new ArgumentException("New password must be at least 8 characters.");

        var newHash = _passwordHasher.Hash(request.NewPassword);
        target.UpdatePasswordHash(newHash);

        // Also unlock if locked
        if (target.IsLockedOut)
            target.UnlockAccount();

        _userRepo.Update(target);
        await _userRepo.SaveChangesAsync(ct);
    }

    public async Task<IList<AccountLockoutStatusDto>> GetLockedAccountsAsync(CancellationToken ct = default)
    {
        var locked = await _userRepo.GetLockedAccountsAsync(ct);
        return locked.Select(u => new AccountLockoutStatusDto(
            u.Id,
            u.Username,
            u.IsLockedOut,
            u.FailedLoginAttempts,
            u.LockedOutUntil
        )).ToList();
    }
}
