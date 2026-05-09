using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Phase 26 — Stage 26.1

/// <summary>Repository abstraction for school streams and student stream assignments.</summary>
public interface ISchoolStreamRepository
{
    Task<IReadOnlyList<SchoolStream>> GetAllStreamsAsync(CancellationToken ct = default);
    Task<SchoolStream?> GetStreamByIdAsync(Guid id, CancellationToken ct = default);
    Task AddStreamAsync(SchoolStream stream, CancellationToken ct = default);
    void UpdateStream(SchoolStream stream);

    Task<StudentStreamAssignment?> GetStudentAssignmentAsync(Guid studentProfileId, CancellationToken ct = default);
    Task UpsertStudentAssignmentAsync(StudentStreamAssignment assignment, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
