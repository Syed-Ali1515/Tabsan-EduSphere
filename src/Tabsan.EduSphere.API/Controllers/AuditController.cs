using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Stage 34.2: searchable audit history for compliance operations.
/// </summary>
[ApiController]
[Route("api/v1/audit")]
[Authorize(Roles = "SuperAdmin,Admin")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit)
    {
        _audit = audit;
    }

    [HttpGet("logs")]
    public async Task<IActionResult> SearchLogs(
        [FromQuery] string? query,
        [FromQuery] Guid? actorUserId,
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        if (toUtc.HasValue && fromUtc.HasValue && toUtc.Value < fromUtc.Value)
            return BadRequest(new { message = "toUtc must be greater than or equal to fromUtc." });

        var (items, totalCount) = await _audit.SearchAsync(
            query: query,
            actorUserId: actorUserId,
            action: action,
            entityName: entityName,
            fromUtc: fromUtc,
            toUtc: toUtc,
            page: page,
            pageSize: pageSize,
            ct: ct);

        var response = new
        {
            page = page < 1 ? 1 : page,
            pageSize = Math.Clamp(pageSize, 1, 200),
            totalCount,
            items = items.Select(x => new
            {
                x.Id,
                x.OccurredAt,
                x.Action,
                x.EntityName,
                x.EntityId,
                x.ActorUserId,
                x.IpAddress,
                x.OldValuesJson,
                x.NewValuesJson
            })
        };

        return Ok(response);
    }
}
