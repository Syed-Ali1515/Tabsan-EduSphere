using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for enrollment workflow operations.
/// Enforces seat-limit checks, duplicate enrollment guards, and semester-closed guards.
/// </summary>
public interface IEnrollmentService
{
    /// <summary>
    /// Enrolls the student into the requested course offering.
    /// Returns null when enrollment is rejected (offering full, closed semester, duplicate, etc.).
    /// </summary>
    Task<EnrollmentResponse?> EnrollAsync(Guid studentProfileId, EnrollRequest request, CancellationToken ct = default);

    /// <summary>
    /// Drops an active enrollment for the student.
    /// Returns false when the enrollment does not exist or is not currently active.
    /// </summary>
    Task<bool> DropAsync(Guid studentProfileId, Guid courseOfferingId, CancellationToken ct = default);

    /// <summary>Returns all enrollment records for the given student (history + active).</summary>
    Task<IReadOnlyList<Enrollment>> GetForStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns all active enrollments in the given course offering (faculty roster view).</summary>
    Task<IReadOnlyList<Enrollment>> GetForOfferingAsync(Guid courseOfferingId, CancellationToken ct = default);
}
