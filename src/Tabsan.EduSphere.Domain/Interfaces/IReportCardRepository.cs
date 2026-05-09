using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Phase 26 — Stage 26.2

/// <summary>Repository abstraction for report card snapshots.</summary>
public interface IReportCardRepository
{
    Task AddAsync(StudentReportCard reportCard, CancellationToken ct = default);

    Task<StudentReportCard?> GetLatestForStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    Task<IReadOnlyList<StudentReportCard>> GetForStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
