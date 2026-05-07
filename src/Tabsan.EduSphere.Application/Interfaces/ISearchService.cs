using Tabsan.EduSphere.Application.DTOs.Search;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Cross-entity search service for Phase 13 — Global Search.
/// Queries students, courses, course offerings, faculty users, and departments
/// with results scoped to the caller's role.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Executes a global search and returns a flat list of typed result items.
    /// Results are scoped by the caller's role:
    ///   SuperAdmin — sees all data.
    ///   Admin       — sees data only within their assigned department(s).
    ///   Faculty     — sees data in their own department plus their own course offerings.
    ///   Student     — sees their own profile and their enrolled course offerings.
    /// </summary>
    Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken ct = default);
}
