using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages student payment receipts and fee collection.
/// Finance creates receipts; students upload proof; finance confirms payment.
/// Until fees are Paid, student account is in read-only mode.
/// All payment records are permanent (no delete).
/// Routes: /api/v1/payments
/// </summary>
[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentReceiptController : ControllerBase
{
    private readonly IStudentLifecycleService _service;

    public PaymentReceiptController(IStudentLifecycleService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var val = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(val, out var id) ? id : Guid.Empty;
    }

    // ── POST /api/v1/payments ─────────────────────────────────────────────────

    /// <summary>Finance creates a new payment receipt for a student.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Finance")]
    public async Task<IActionResult> Create([FromBody] CreatePaymentReceiptCommand cmd, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var receipt = await _service.CreatePaymentReceiptAsync(userId, cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = receipt.Id }, receipt);
    }

    // ── GET /api/v1/payments/student/{studentProfileId} ──────────────────────

    /// <summary>Returns all active (non-cancelled) receipts for a student. Admin or Finance only.</summary>
    [HttpGet("student/{studentProfileId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Finance")]
    public async Task<IActionResult> GetByStudent(Guid studentProfileId, CancellationToken ct)
    {
        var receipts = await _service.GetActiveReceiptsByStudentAsync(studentProfileId, ct);
        return Ok(receipts);
    }

    // ── GET /api/v1/payments/my ───────────────────────────────────────────────

    /// <summary>Student views their own active payment receipts.</summary>
    [HttpGet("my/{studentProfileId:guid}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMine(Guid studentProfileId, CancellationToken ct)
    {
        var receipts = await _service.GetActiveReceiptsByStudentAsync(studentProfileId, ct);
        return Ok(receipts);
    }

    // ── GET /api/v1/payments/student/{studentProfileId}/fee-status ───────────

    /// <summary>Returns full fee status for a student (paid + unpaid summary).</summary>
    [HttpGet("student/{studentProfileId:guid}/fee-status")]
    public async Task<IActionResult> GetFeeStatus(Guid studentProfileId, CancellationToken ct)
    {
        var status = await _service.GetStudentFeeStatusAsync(studentProfileId, ct);
        return Ok(status);
    }

    // ── GET /api/v1/payments/{id} ─────────────────────────────────────────────

    /// <summary>Returns a specific payment receipt by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var receipt = await _service.GetPaymentReceiptByIdAsync(id, ct);
        if (receipt is null) return NotFound();
        return Ok(receipt);
    }

    // ── POST /api/v1/payments/{id}/submit-proof ───────────────────────────────

    /// <summary>
    /// Student uploads proof of payment. Accepts a multipart file upload.
    /// Sets receipt status to Submitted.
    /// </summary>
    [HttpPost("{id:guid}/submit-proof")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SubmitProof(Guid id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        // Validate extension — only images and PDFs accepted
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "Only PDF, JPG, or PNG files are accepted as payment proof." });

        // Save file to a secure upload directory — production should use blob storage
        var uploadsDir = Path.Combine("uploads", "payment-proofs");
        Directory.CreateDirectory(uploadsDir);
        var fileName = $"{id}_{DateTime.UtcNow:yyyyMMddHHmmss}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using var stream = System.IO.File.Create(filePath);
        await file.CopyToAsync(stream, ct);

        try
        {
            await _service.SubmitPaymentProofAsync(id, filePath, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // ── POST /api/v1/payments/{id}/confirm ───────────────────────────────────

    /// <summary>Finance confirms payment received. Sets receipt status to Paid (final state).</summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "SuperAdmin,Admin,Finance")]
    public async Task<IActionResult> ConfirmPayment(Guid id, [FromBody] string? notes, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            await _service.ConfirmPaymentAsync(id, userId, notes, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // ── POST /api/v1/payments/{id}/cancel ────────────────────────────────────

    /// <summary>Finance cancels a receipt (e.g., issued in error). Record is permanently preserved.</summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "SuperAdmin,Admin,Finance")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string? reason, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            await _service.CancelReceiptAsync(id, userId, reason, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
    }
}
