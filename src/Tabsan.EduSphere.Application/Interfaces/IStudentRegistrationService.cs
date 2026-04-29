using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for student self-registration and Admin-managed student account creation.
/// Checks the registration whitelist and creates the User + StudentProfile atomically.
/// </summary>
public interface IStudentRegistrationService
{
    /// <summary>
    /// Self-registration flow: validates the whitelist, creates a User with Student role,
    /// creates a StudentProfile, and marks the whitelist entry as consumed.
    /// Returns null when the identifier is not on the whitelist or is already used.
    /// </summary>
    Task<Guid?> SelfRegisterAsync(StudentSelfRegisterRequest request, CancellationToken ct = default);

    /// <summary>
    /// Admin-managed flow: creates a StudentProfile for an existing User account.
    /// Used when the Admin creates the account first and then links the academic profile.
    /// </summary>
    Task<Guid> CreateProfileAsync(CreateStudentProfileRequest request, CancellationToken ct = default);
}
