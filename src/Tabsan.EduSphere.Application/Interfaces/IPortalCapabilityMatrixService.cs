using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 27 — University Portal Parity and Student Experience — Stage 27.1

public sealed record PortalCapabilityMatrixRow(
    string CapabilityKey,
    string CapabilityName,
    string ModuleKey,
    string ModuleName,
    string Route,
    string Description,
    bool IsModuleActive,
    bool IsLicenseGated,
    bool Student,
    bool Faculty,
    bool Admin,
    bool SuperAdmin,
    bool University,
    bool School,
    bool College);

public sealed record PortalCapabilityMatrixResponse(
    bool IncludeSchool,
    bool IncludeCollege,
    bool IncludeUniversity,
    IReadOnlyList<PortalCapabilityMatrixRow> Rows);

public interface IPortalCapabilityMatrixService
{
    Task<PortalCapabilityMatrixResponse> GetMatrixAsync(InstitutionPolicySnapshot policy, CancellationToken ct = default);
}
