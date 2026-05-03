using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages student profiles and the self-registration flow.
/// Self-registration is open (AllowAnonymous). All other operations require authentication.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IStudentRegistrationService _registrationService;
    private readonly IStudentProfileRepository _studentRepo;
    private readonly IRegistrationWhitelistRepository _whitelistRepo;

    public StudentController(
        IStudentRegistrationService registrationService,
        IStudentProfileRepository studentRepo,
        IRegistrationWhitelistRepository whitelistRepo)
    {
        _registrationService = registrationService;
        _studentRepo = studentRepo;
        _whitelistRepo = whitelistRepo;
    }

    // ── POST /api/v1/student/register (public) ────────────────────────────────

    /// <summary>
    /// Allows a new student to self-register using a pre-loaded whitelist identifier
    /// (email or registration number). Creates both the User account and the StudentProfile.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> SelfRegister([FromBody] StudentSelfRegisterRequest request, CancellationToken ct)
    {
        var userId = await _registrationService.SelfRegisterAsync(request, ct);
        return userId is null
            ? BadRequest("Registration identifier not found or already used. Contact your department administrator.")
            : Ok(new { UserId = userId.Value, Message = "Account created. Please log in." });
    }

    // ── GET /api/v1/student/profile ───────────────────────────────────────────

    /// <summary>Returns the student profile for the currently authenticated user.</summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = GetUserId();
        var profile = await _studentRepo.GetByUserIdAsync(userId, ct);
        if (profile is null) return NotFound("Student profile not found.");

        return Ok(new
        {
            profile.Id, profile.RegistrationNumber, profile.ProgramId,
            ProgramName = profile.Program?.Name,
            profile.DepartmentId, DeptName = profile.Department?.Name,
            profile.AdmissionDate, profile.Cgpa, profile.CurrentSemesterNumber
        });
    }

    // ── GET /api/v1/student ───────────────────────────────────────────────────

    /// <summary>Returns all student profiles. Admin and SuperAdmin only.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken ct)
    {
        var students = await _studentRepo.GetAllAsync(departmentId, ct);
        return Ok(students.Select(sp => new
        {
            sp.Id, sp.UserId, sp.RegistrationNumber, sp.ProgramId,
            ProgramName = sp.Program?.Name,
            sp.DepartmentId, DepartmentName = sp.Department?.Name ?? "",
            sp.Cgpa, sp.CurrentSemesterNumber, sp.AdmissionDate,
            Status = sp.Status.ToString()
        }));
    }

    // ── POST /api/v1/student (admin-managed) ──────────────────────────────────

    /// <summary>
    /// Admin-managed student profile creation for an existing User account.
    /// Used when the Admin created the user directly and now attaches the academic profile.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStudentProfileRequest request, CancellationToken ct)
    {
        try
        {
            var profileId = await _registrationService.CreateProfileAsync(request, ct);
            return Created($"/api/v1/student/{profileId}", new { Id = profileId });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // ── POST /api/v1/student/whitelist ────────────────────────────────────────

    /// <summary>Adds a single entry to the registration whitelist. Admin and SuperAdmin only.</summary>
    [HttpPost("whitelist")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AddWhitelistEntry([FromBody] WhitelistEntryRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<WhitelistIdentifierType>(request.IdentifierType, ignoreCase: true, out var idType))
            return BadRequest($"Invalid identifier type '{request.IdentifierType}'. Use 'Email' or 'RegistrationNumber'.");

        var entry = new RegistrationWhitelist(idType, request.IdentifierValue, request.DepartmentId, request.ProgramId);
        await _whitelistRepo.AddAsync(entry, ct);
        await _whitelistRepo.SaveChangesAsync(ct);
        return Created(string.Empty, new { entry.Id });
    }

    // ── POST /api/v1/student/whitelist/bulk ───────────────────────────────────

    /// <summary>Bulk-loads whitelist entries (cohort import). Admin and SuperAdmin only.</summary>
    [HttpPost("whitelist/bulk")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> BulkAddWhitelistEntries([FromBody] IEnumerable<WhitelistEntryRequest> requests, CancellationToken ct)
    {
        var entries = new List<RegistrationWhitelist>();
        foreach (var r in requests)
        {
            if (!Enum.TryParse<WhitelistIdentifierType>(r.IdentifierType, ignoreCase: true, out var idType))
                return BadRequest($"Invalid identifier type '{r.IdentifierType}'.");
            entries.Add(new RegistrationWhitelist(idType, r.IdentifierValue, r.DepartmentId, r.ProgramId));
        }

        await _whitelistRepo.AddRangeAsync(entries, ct);
        await _whitelistRepo.SaveChangesAsync(ct);
        return Ok(new { Added = entries.Count });
    }

    // ── Helper ─────────────────────────────────────────────────────────────────
    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
