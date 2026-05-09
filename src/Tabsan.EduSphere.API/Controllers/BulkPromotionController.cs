using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 26 — Stage 26.2

[ApiController]
[Route("api/v1/bulk-promotion")]
[Authorize]
public class BulkPromotionController : ControllerBase
{
    private readonly IBulkPromotionService _service;

    public BulkPromotionController(IBulkPromotionService service)
    {
        _service = service;
    }

    [HttpPost("batch")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateBulkPromotionBatchRequest request, CancellationToken ct)
        => Ok(await _service.CreateBatchAsync(request, ct));

    [HttpPost("entries")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AddEntries([FromBody] AddBulkPromotionEntriesRequest request, CancellationToken ct)
        => Ok(await _service.AddEntriesAsync(request, ct));

    [HttpPost("submit/{batchId:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Submit(Guid batchId, CancellationToken ct)
        => Ok(await _service.SubmitAsync(batchId, ct));

    [HttpPost("review")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Review([FromBody] ReviewBulkPromotionBatchRequest request, CancellationToken ct)
        => Ok(await _service.ReviewAsync(request, ct));

    [HttpPost("apply")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Apply([FromBody] ApplyBulkPromotionBatchRequest request, CancellationToken ct)
        => Ok(await _service.ApplyAsync(request, ct));

    [HttpGet("{batchId:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Get(Guid batchId, CancellationToken ct)
    {
        var batch = await _service.GetByIdAsync(batchId, ct);
        return batch is null ? NotFound() : Ok(batch);
    }
}
