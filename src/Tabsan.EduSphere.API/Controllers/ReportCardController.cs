using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 26 — Stage 26.2

[ApiController]
[Route("api/v1/report-cards")]
[Authorize]
public class ReportCardController : ControllerBase
{
    private readonly IReportCardService _service;

    public ReportCardController(IReportCardService service)
    {
        _service = service;
    }

    [HttpPost("generate")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> Generate([FromBody] GenerateReportCardRequest request, CancellationToken ct)
        => Ok(await _service.GenerateAsync(request, ct));

    [HttpGet("latest/{studentProfileId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetLatest(Guid studentProfileId, CancellationToken ct)
    {
        var card = await _service.GetLatestAsync(studentProfileId, ct);
        return card is null ? NotFound() : Ok(card);
    }

    [HttpGet("history/{studentProfileId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetHistory(Guid studentProfileId, CancellationToken ct)
        => Ok(await _service.GetHistoryAsync(studentProfileId, ct));
}
