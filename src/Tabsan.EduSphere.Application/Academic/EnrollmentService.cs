using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

/// <summary>
/// Orchestrates student enrollment and drop operations.
/// Enforces all business rules:
///   - The course offering must be open.
///   - The parent semester must not be closed.
///   - The offering must have available seats.
///   - A student cannot enrol twice in the same offering.
/// Enrollment rows are never deleted — drops change status to Dropped.
/// </summary>
public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly ISemesterRepository _semesterRepo;
    private readonly IAuditService _audit;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepo,
        ICourseRepository courseRepo,
        ISemesterRepository semesterRepo,
        IAuditService audit)
    {
        _enrollmentRepo = enrollmentRepo;
        _courseRepo = courseRepo;
        _semesterRepo = semesterRepo;
        _audit = audit;
    }

    /// <summary>
    /// Attempts to enroll the student in the requested offering.
    /// Checks (in order): offering exists → semester not closed → offering open → not already enrolled → seat available.
    /// Returns null on any failure.
    /// </summary>
    public async Task<EnrollmentResponse?> EnrollAsync(Guid studentProfileId, EnrollRequest request, CancellationToken ct = default)
    {
        var offering = await _courseRepo.GetOfferingByIdAsync(request.CourseOfferingId, ct);
        if (offering is null) return null;

        // Guard: semester must not be closed.
        if (offering.Semester.IsClosed) return null;

        // Guard: offering must still be accepting enrollments.
        if (!offering.IsOpen) return null;

        // Guard: prevent duplicate active enrollments.
        if (await _enrollmentRepo.IsEnrolledAsync(studentProfileId, request.CourseOfferingId, ct))
            return null;

        // Guard: check seat availability.
        var currentCount = await _courseRepo.GetEnrollmentCountAsync(request.CourseOfferingId, ct);
        if (currentCount >= offering.MaxEnrollment) return null;

        var enrollment = new Enrollment(studentProfileId, request.CourseOfferingId);
        await _enrollmentRepo.AddAsync(enrollment, ct);
        await _enrollmentRepo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog(
            "Enroll", "Enrollment", enrollment.Id.ToString(),
            actorUserId: studentProfileId), ct);

        return new EnrollmentResponse(
            EnrollmentId:     enrollment.Id,
            CourseOfferingId: offering.Id,
            CourseName:       offering.Course.Title,
            SemesterName:     offering.Semester.Name,
            Status:           enrollment.Status.ToString(),
            EnrolledAt:       enrollment.EnrolledAt);
    }

    /// <summary>
    /// Drops the student's active enrollment in the given offering.
    /// Changes the status to Dropped; the row is preserved for academic history.
    /// Returns false when no active enrollment is found.
    /// </summary>
    public async Task<bool> DropAsync(Guid studentProfileId, Guid courseOfferingId, CancellationToken ct = default)
    {
        var enrollment = await _enrollmentRepo.GetAsync(studentProfileId, courseOfferingId, ct);
        if (enrollment is null || enrollment.Status != EnrollmentStatus.Active)
            return false;

        enrollment.Drop();
        _enrollmentRepo.Update(enrollment);
        await _enrollmentRepo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog(
            "Drop", "Enrollment", enrollment.Id.ToString(),
            actorUserId: studentProfileId), ct);

        return true;
    }

    /// <summary>Returns all enrollment records for the student (full history, not just active).</summary>
    public Task<IReadOnlyList<Enrollment>> GetForStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => _enrollmentRepo.GetByStudentAsync(studentProfileId, ct);

    /// <summary>Returns all active enrollments in the given course offering.</summary>
    public Task<IReadOnlyList<Enrollment>> GetForOfferingAsync(Guid courseOfferingId, CancellationToken ct = default)
        => _enrollmentRepo.GetByOfferingAsync(courseOfferingId, ct);
}
