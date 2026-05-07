using Tabsan.EduSphere.Application.DTOs.Search;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Repository contract for Phase 13 — Global Search.
/// Performs cross-entity full-text style queries directly in the database layer.
/// All overloads apply partial-match (LIKE) on the supplied term.
/// </summary>
public interface ISearchRepository
{
    /// <summary>
    /// Searches students by username (display name) or registration number.
    /// Optionally scoped to a set of department IDs.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchStudentsAsync(
        string                term,
        IReadOnlyList<Guid>?  departmentIds,
        int                   limit,
        CancellationToken     ct = default);

    /// <summary>
    /// Searches courses by title or code.
    /// Optionally scoped to a set of department IDs.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchCoursesAsync(
        string                term,
        IReadOnlyList<Guid>?  departmentIds,
        int                   limit,
        CancellationToken     ct = default);

    /// <summary>
    /// Searches course offerings by course title/code or semester name.
    /// Optionally scoped to a set of department IDs or to a single faculty's offerings.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchOfferingsAsync(
        string                term,
        IReadOnlyList<Guid>?  departmentIds,
        Guid?                 facultyUserId,
        int                   limit,
        CancellationToken     ct = default);

    /// <summary>
    /// Searches faculty users by username.
    /// Optionally scoped to a set of department IDs.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchFacultyAsync(
        string                term,
        IReadOnlyList<Guid>?  departmentIds,
        int                   limit,
        CancellationToken     ct = default);

    /// <summary>
    /// Searches departments by name or code.
    /// Optionally restricted to a supplied set of department IDs.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchDepartmentsAsync(
        string                term,
        IReadOnlyList<Guid>?  allowedIds,
        int                   limit,
        CancellationToken     ct = default);

    /// <summary>
    /// Returns the student profile ID for the given user ID, or null.
    /// </summary>
    Task<Guid?> GetStudentProfileIdByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns offerings that the given student is enrolled in, filtered by term.
    /// </summary>
    Task<IReadOnlyList<SearchResultItem>> SearchStudentEnrolledOfferingsAsync(
        Guid              studentProfileId,
        string            term,
        int               limit,
        CancellationToken ct = default);
}
