using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 26 — Stage 26.1

public interface ISchoolStreamService
{
    Task<IReadOnlyList<SchoolStreamDto>> GetAllStreamsAsync(CancellationToken ct = default);

    Task<SchoolStreamDto> UpsertStreamAsync(Guid? id, SaveSchoolStreamRequest request, CancellationToken ct = default);

    Task<StudentStreamAssignmentDto> AssignStudentAsync(AssignStudentStreamRequest request, CancellationToken ct = default);

    Task<StudentStreamAssignmentDto?> GetStudentAssignmentAsync(Guid studentProfileId, CancellationToken ct = default);
}
