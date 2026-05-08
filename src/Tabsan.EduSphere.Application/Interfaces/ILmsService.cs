using Tabsan.EduSphere.Application.DTOs.Lms;

namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 20 Stage 20.1/20.2 — LMS content service contract

/// <summary>Service for managing course content modules and video attachments.</summary>
public interface ILmsService
{
    // Modules
    Task<List<CourseContentModuleDto>> GetModulesAsync(Guid offeringId, bool publishedOnly, CancellationToken ct = default);
    Task<CourseContentModuleDto?> GetModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<CourseContentModuleDto> CreateModuleAsync(CreateModuleRequest request, CancellationToken ct = default);
    Task UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request, CancellationToken ct = default);
    Task PublishModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task UnpublishModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task DeleteModuleAsync(Guid moduleId, CancellationToken ct = default);

    // Videos
    Task<ContentVideoDto> AddVideoAsync(AddVideoRequest request, CancellationToken ct = default);
    Task DeleteVideoAsync(Guid videoId, CancellationToken ct = default);
}
