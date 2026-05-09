using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 26 — Stage 26.3

public class ParentPortalService : IParentPortalService
{
    private readonly IParentStudentLinkRepository _linkRepo;
    private readonly IStudentProfileRepository _studentRepo;

    public ParentPortalService(
        IParentStudentLinkRepository linkRepo,
        IStudentProfileRepository studentRepo)
    {
        _linkRepo = linkRepo;
        _studentRepo = studentRepo;
    }

    public async Task<IReadOnlyList<ParentLinkedStudentDto>> GetLinkedStudentsAsync(
        Guid parentUserId,
        CancellationToken ct = default)
    {
        var links = await _linkRepo.GetByParentUserIdAsync(parentUserId, ct);

        var result = new List<ParentLinkedStudentDto>(links.Count);
        foreach (var link in links.Where(l => l.IsActive))
        {
            var student = await _studentRepo.GetByIdAsync(link.StudentProfileId, ct);
            if (student is null)
                continue;

            result.Add(new ParentLinkedStudentDto(
                student.Id,
                student.RegistrationNumber,
                student.ProgramId,
                student.DepartmentId,
                student.CurrentSemesterNumber,
                student.Cgpa,
                student.CurrentSemesterGpa,
                link.Relationship));
        }

        return result;
    }
}
