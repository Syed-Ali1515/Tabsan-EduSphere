using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 26 — Stage 26.3

public class ParentPortalService : IParentPortalService
{
    private readonly IParentStudentLinkRepository _linkRepo;
    private readonly IStudentProfileRepository _studentRepo;
    private readonly IUserRepository _userRepo;

    public ParentPortalService(
        IParentStudentLinkRepository linkRepo,
        IStudentProfileRepository studentRepo,
        IUserRepository userRepo)
    {
        _linkRepo = linkRepo;
        _studentRepo = studentRepo;
        _userRepo = userRepo;
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

    public async Task<IReadOnlyList<ParentStudentLinkDto>> GetLinksByParentAsync(
        Guid parentUserId,
        CancellationToken ct = default)
    {
        var links = await _linkRepo.GetByParentUserIdAsync(parentUserId, ct);
        return links
            .Select(l => new ParentStudentLinkDto(l.ParentUserId, l.StudentProfileId, l.Relationship, l.IsActive))
            .ToList();
    }

    public async Task<ParentStudentLinkDto> UpsertLinkAsync(
        UpsertParentStudentLinkRequest request,
        CancellationToken ct = default)
    {
        var parentUser = await _userRepo.GetByIdAsync(request.ParentUserId, ct)
            ?? throw new KeyNotFoundException($"Parent user {request.ParentUserId} not found.");

        if (!string.Equals(parentUser.Role?.Name, "Parent", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Target user must have Parent role.");

        var student = await _studentRepo.GetByIdAsync(request.StudentProfileId, ct)
            ?? throw new KeyNotFoundException($"Student profile {request.StudentProfileId} not found.");

        if (student.Department?.InstitutionType != InstitutionType.School)
            throw new InvalidOperationException("Parent-portal linking is only allowed for School students.");

        var existing = await _linkRepo.GetByParentAndStudentAsync(request.ParentUserId, request.StudentProfileId, ct);
        if (existing is null)
        {
            var created = new Domain.Academic.ParentStudentLink(
                request.ParentUserId,
                request.StudentProfileId,
                request.Relationship);
            if (!request.IsActive)
                created.Update(request.Relationship, false);

            await _linkRepo.AddAsync(created, ct);
            await _linkRepo.SaveChangesAsync(ct);
            return new ParentStudentLinkDto(created.ParentUserId, created.StudentProfileId, created.Relationship, created.IsActive);
        }

        existing.Update(request.Relationship, request.IsActive);
        _linkRepo.Update(existing);
        await _linkRepo.SaveChangesAsync(ct);
        return new ParentStudentLinkDto(existing.ParentUserId, existing.StudentProfileId, existing.Relationship, existing.IsActive);
    }

    public async Task<bool> DeactivateLinkAsync(Guid parentUserId, Guid studentProfileId, CancellationToken ct = default)
    {
        var existing = await _linkRepo.GetByParentAndStudentAsync(parentUserId, studentProfileId, ct);
        if (existing is null)
            return false;

        existing.Update(existing.Relationship, false);
        _linkRepo.Update(existing);
        await _linkRepo.SaveChangesAsync(ct);
        return true;
    }
}
