using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Identity;
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
    private readonly IPasswordHistoryRepository _passwordHistory;
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templateRenderer;

    public AccountSecurityService(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        IPasswordHistoryRepository passwordHistory,
        IEmailSender emailSender,
        IEmailTemplateRenderer templateRenderer)
    {
        _userRepo          = userRepo;
        _passwordHasher    = passwordHasher;
        _passwordHistory   = passwordHistory;
        _emailSender       = emailSender;
        _templateRenderer  = templateRenderer;
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

        // Notify user by email if address is on file.
        if (!string.IsNullOrWhiteSpace(target.Email))
        {
            var body = _templateRenderer.Render("account-unlocked", new Dictionary<string, string>
            {
                ["USERNAME"] = target.Username
            });
            try { await _emailSender.SendAsync(target.Email, "Your account has been unlocked", body, ct); }
            catch { /* non-fatal — unlock succeeded regardless */ }
        }
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

        // Record in password history for reuse-prevention.
        await _passwordHistory.AddAsync(new PasswordHistoryEntry(target.Id, newHash), ct);
        await _passwordHistory.SaveChangesAsync(ct);

        // Notify user by email if address is on file.
        if (!string.IsNullOrWhiteSpace(target.Email))
        {
            var body = _templateRenderer.Render("password-reset", new Dictionary<string, string>
            {
                ["USERNAME"] = target.Username
            });
            try { await _emailSender.SendAsync(target.Email, "Your password has been reset", body, ct); }
            catch { /* non-fatal — reset succeeded regardless */ }
        }
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

