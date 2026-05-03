using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

[ApiController]
[Route("api/v1/result-calculation")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class ResultCalculationController : ControllerBase
{
    private readonly IResultCalculationService _service;

    public ResultCalculationController(IResultCalculationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var settings = await _service.GetSettingsAsync(ct);
        return Ok(settings);
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveResultCalculationSettingsRequest request, CancellationToken ct)
    {
        try
        {
            await _service.SaveSettingsAsync(request, ct);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}