using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 26 — Stage 26.1

public class SchoolStreamService : ISchoolStreamService
{
    private readonly ISchoolStreamRepository _streamRepo;
    private readonly IStudentProfileRepository _studentRepo;

    public SchoolStreamService(
        ISchoolStreamRepository streamRepo,
        IStudentProfileRepository studentRepo)
    {
        _streamRepo = streamRepo;
        _studentRepo = studentRepo;
    }

    public async Task<IReadOnlyList<SchoolStreamDto>> GetAllStreamsAsync(CancellationToken ct = default)
    {
        var streams = await _streamRepo.GetAllStreamsAsync(ct);
        return streams.Select(s => new SchoolStreamDto(s.Id, s.Name, s.Description, s.IsActive)).ToList();
    }

    public async Task<SchoolStreamDto> UpsertStreamAsync(Guid? id, SaveSchoolStreamRequest request, CancellationToken ct = default)
    {
        if (id is null || id == Guid.Empty)
        {
            var stream = new SchoolStream(request.Name, request.Description);
            if (!request.IsActive)
                stream.Update(request.Name, request.Description, false);
            await _streamRepo.AddStreamAsync(stream, ct);
            await _streamRepo.SaveChangesAsync(ct);
            return new SchoolStreamDto(stream.Id, stream.Name, stream.Description, stream.IsActive);
        }

        var existing = await _streamRepo.GetStreamByIdAsync(id.Value, ct)
            ?? throw new KeyNotFoundException($"Stream {id.Value} not found.");

        existing.Update(request.Name, request.Description, request.IsActive);
        _streamRepo.UpdateStream(existing);
        await _streamRepo.SaveChangesAsync(ct);

        return new SchoolStreamDto(existing.Id, existing.Name, existing.Description, existing.IsActive);
    }

    public async Task<StudentStreamAssignmentDto> AssignStudentAsync(
        AssignStudentStreamRequest request,
        CancellationToken ct = default)
    {
        _ = await _studentRepo.GetByIdAsync(request.StudentProfileId, ct)
            ?? throw new KeyNotFoundException($"Student profile {request.StudentProfileId} not found.");

        var stream = await _streamRepo.GetStreamByIdAsync(request.StreamId, ct)
            ?? throw new KeyNotFoundException($"Stream {request.StreamId} not found.");

        var assignment = new StudentStreamAssignment(request.StudentProfileId, request.StreamId, request.AssignedByUserId);
        await _streamRepo.UpsertStudentAssignmentAsync(assignment, ct);
        await _streamRepo.SaveChangesAsync(ct);

        return new StudentStreamAssignmentDto(
            assignment.StudentProfileId,
            assignment.SchoolStreamId,
            stream.Name,
            assignment.AssignedAt,
            assignment.AssignedByUserId);
    }

    public async Task<StudentStreamAssignmentDto?> GetStudentAssignmentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var assignment = await _streamRepo.GetStudentAssignmentAsync(studentProfileId, ct);
        if (assignment is null)
            return null;

        var stream = await _streamRepo.GetStreamByIdAsync(assignment.SchoolStreamId, ct)
            ?? throw new KeyNotFoundException($"Stream {assignment.SchoolStreamId} not found.");

        return new StudentStreamAssignmentDto(
            assignment.StudentProfileId,
            assignment.SchoolStreamId,
            stream.Name,
            assignment.AssignedAt,
            assignment.AssignedByUserId);
    }
}
