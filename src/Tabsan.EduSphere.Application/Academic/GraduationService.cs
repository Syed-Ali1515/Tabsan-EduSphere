// Final-Touches Phase 18 Stage 18.1/18.2 — Graduation workflow service implementation

using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.Academic;

/// <summary>
/// Orchestrates the graduation application lifecycle (Stage 18.1)
/// and certificate PDF generation (Stage 18.2).
/// </summary>
public class GraduationService : IGraduationService
{
    private readonly IGraduationRepository  _repo;
    private readonly INotificationService   _notifications;
    private readonly ISettingsRepository    _settings;
    private readonly IStudentLifecycleRepository _lifecycleRepo;
    private readonly ICertificateGenerator  _certGenerator;

    // Final-Touches Phase 18 Stage 18.2 — portal setting key for the certificate body text
    private const string CertTemplateSetting = "graduation_certificate_template";

    // Final-Touches Phase 18 Stage 18.2 — subfolder within wwwroot for generated certificates
    private const string CertFolder = "certificates";

    public GraduationService(
        IGraduationRepository      repo,
        INotificationService       notifications,
        ISettingsRepository        settings,
        IStudentLifecycleRepository lifecycleRepo,
        ICertificateGenerator      certGenerator)
    {
        _repo          = repo;
        _notifications = notifications;
        _settings      = settings;
        _lifecycleRepo = lifecycleRepo;
        _certGenerator = certGenerator;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static GraduationApplicationSummary ToSummary(GraduationApplication a, string name, string regNo, string program)
        => new(a.Id, a.StudentProfileId, name, regNo, program, a.Status.ToString(),
               a.SubmittedAt, a.UpdatedAt, a.CertificatePath is not null);

    // Final-Touches Phase 18 Stage 18.1 — map full detail
    private static GraduationApplicationDetail ToDetail(
        GraduationApplication a, string name, string regNo, string program,
        IReadOnlyList<ApprovalHistoryItem> history)
        => new(a.Id, a.StudentProfileId, name, regNo, program, a.Status.ToString(),
               a.StudentNote, a.SubmittedAt, a.UpdatedAt, a.CertificatePath is not null,
               a.CertificatePath, history);

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<GraduationApplicationSummary>> GetMyApplicationsAsync(
        Guid studentProfileId, CancellationToken ct = default)
    {
        var apps  = await _repo.GetByStudentAsync(studentProfileId, ct);
        var name  = await _repo.GetStudentDisplayNameAsync(studentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(studentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(studentProfileId, ct);
        return apps.Select(a => ToSummary(a, name, regNo, prog)).ToList();
    }

    public async Task<GraduationApplicationDetail> GetApplicationDetailAsync(
        Guid applicationId, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        var name  = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);

        // Final-Touches Phase 18 Stage 18.1 — build approval history
        var history = app.Approvals
            .OrderBy(a => a.ActedAt)
            .Select(a => new ApprovalHistoryItem(
                a.Stage.ToString(),
                "User",   // approver name: kept simple; controller can enrich if needed
                a.IsApproved,
                a.Note,
                a.ActedAt))
            .ToList();

        return ToDetail(app, name, regNo, prog, history);
    }

    public async Task<IReadOnlyList<GraduationApplicationSummary>> GetApplicationsAsync(
        Guid? departmentId, string? statusFilter, CancellationToken ct = default)
    {
        GraduationApplicationStatus? status = statusFilter is null ? null
            : Enum.TryParse<GraduationApplicationStatus>(statusFilter, true, out var s) ? s : null;

        IReadOnlyList<GraduationApplication> apps;
        if (departmentId.HasValue)
            apps = await _repo.GetByDepartmentAsync(departmentId.Value, status, ct);
        else
            apps = await _repo.GetAllAsync(status, ct);

        var result = new List<GraduationApplicationSummary>();
        foreach (var a in apps)
        {
            var name  = await _repo.GetStudentDisplayNameAsync(a.StudentProfileId, ct);
            var regNo = await _repo.GetStudentRegistrationNumberAsync(a.StudentProfileId, ct);
            var prog  = await _repo.GetStudentProgramNameAsync(a.StudentProfileId, ct);
            result.Add(ToSummary(a, name, regNo, prog));
        }
        return result;
    }

    // ── Commands ──────────────────────────────────────────────────────────────

    // Final-Touches Phase 18 Stage 18.1 — student submits application
    public async Task<GraduationApplicationSummary> SubmitApplicationAsync(
        Guid studentProfileId, SubmitGraduationApplicationRequest request, CancellationToken ct = default)
    {
        // Guard: only one active application at a time
        var existing = await _repo.GetActiveByStudentAsync(studentProfileId, ct);
        if (existing is not null &&
            existing.Status != GraduationApplicationStatus.Rejected)
            throw new InvalidOperationException("An active graduation application already exists.");

        var app = GraduationApplication.Create(studentProfileId, request.StudentNote);
        app.Submit();
        await _repo.AddAsync(app, ct);
        await _repo.SaveChangesAsync(ct);

        // Final-Touches Phase 18 Stage 18.1 — notify faculty of new application
        var student     = await _lifecycleRepo.GetByIdAsync(studentProfileId, ct);
        if (student is not null)
        {
            var facultyIds = await _repo.GetFacultyUserIdsByDepartmentAsync(student.DepartmentId, ct);
            if (facultyIds.Count > 0)
                await _notifications.SendSystemAsync(
                    "Graduation Application Submitted",
                    $"A graduation application has been submitted and is awaiting your review.",
                    NotificationType.General,
                    facultyIds, ct);
        }

        var name  = await _repo.GetStudentDisplayNameAsync(studentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(studentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(studentProfileId, ct);
        return ToSummary(app, name, regNo, prog);
    }

    // Final-Touches Phase 18 Stage 18.1 — faculty approves
    public async Task<GraduationApplicationSummary> FacultyApproveAsync(
        Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        if (!request.IsApproved)
            return await RejectInternalAsync(app, approverUserId, ApprovalStage.Faculty, request.Note, ct);

        app.FacultyApprove(approverUserId, request.Note);
        _repo.Update(app);
        await _repo.SaveChangesAsync(ct);

        // Final-Touches Phase 18 Stage 18.1 — notify admin
        var student  = await _lifecycleRepo.GetByIdAsync(app.StudentProfileId, ct);
        if (student is not null)
        {
            var adminIds = await _repo.GetAdminUserIdsByDepartmentAsync(student.DepartmentId, ct);
            if (adminIds.Count > 0)
                await _notifications.SendSystemAsync(
                    "Graduation Application Pending Admin Approval",
                    "A graduation application has been approved by Faculty and requires your review.",
                    NotificationType.General, adminIds, ct);
        }

        var name  = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);
        return ToSummary(app, name, regNo, prog);
    }

    // Final-Touches Phase 18 Stage 18.1 — admin approves
    public async Task<GraduationApplicationSummary> AdminApproveAsync(
        Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        if (!request.IsApproved)
            return await RejectInternalAsync(app, approverUserId, ApprovalStage.Admin, request.Note, ct);

        app.AdminApprove(approverUserId, request.Note);
        _repo.Update(app);
        await _repo.SaveChangesAsync(ct);

        // Final-Touches Phase 18 Stage 18.1 — notify superadmin
        var superAdminIds = await _repo.GetSuperAdminUserIdsAsync(ct);
        if (superAdminIds.Count > 0)
            await _notifications.SendSystemAsync(
                "Graduation Application Pending Final Approval",
                "A graduation application has been approved by Admin and requires your final confirmation.",
                NotificationType.General, superAdminIds, ct);

        var name  = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);
        return ToSummary(app, name, regNo, prog);
    }

    // Final-Touches Phase 18 Stage 18.1/18.2 — superadmin final approval + certificate + graduation
    public async Task<GraduationApplicationSummary> FinalApproveAsync(
        Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        if (!request.IsApproved)
            return await RejectInternalAsync(app, approverUserId, ApprovalStage.SuperAdmin, request.Note, ct);

        app.FinalApprove(approverUserId, request.Note);
        _repo.Update(app);
        await _repo.SaveChangesAsync(ct);

        // Final-Touches Phase 18 Stage 18.2 — generate certificate
        await GenerateCertificateAsync(applicationId, ct);

        // Final-Touches Phase 18 Stage 18.1 — trigger student graduation lifecycle
        var student = await _lifecycleRepo.GetByIdAsync(app.StudentProfileId, ct);
        if (student is not null)
        {
            student.Graduate();
            await _lifecycleRepo.UpdateAsync(student, ct);

            // Notify student
            await _notifications.SendSystemAsync(
                "Congratulations! Graduation Approved",
                "Your graduation application has been fully approved. Your certificate is ready for download.",
                NotificationType.General,
                new[] { student.UserId }, ct);
        }

        var name  = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);
        return ToSummary(app, name, regNo, prog);
    }

    public async Task<GraduationApplicationSummary> RejectAsync(
        Guid applicationId, Guid approverUserId, string approverRole,
        GraduationApprovalRequest request, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        var stage = approverRole switch
        {
            "Faculty"    => ApprovalStage.Faculty,
            "Admin"      => ApprovalStage.Admin,
            _            => ApprovalStage.SuperAdmin
        };

        return await RejectInternalAsync(app, approverUserId, stage, request.Note, ct);
    }

    private async Task<GraduationApplicationSummary> RejectInternalAsync(
        GraduationApplication app, Guid approverUserId, ApprovalStage stage, string? note, CancellationToken ct)
    {
        app.Reject(approverUserId, stage, note);
        _repo.Update(app);
        await _repo.SaveChangesAsync(ct);

        // Final-Touches Phase 18 Stage 18.1 — notify student of rejection
        var student = await _lifecycleRepo.GetByIdAsync(app.StudentProfileId, ct);
        if (student is not null)
            await _notifications.SendSystemAsync(
                "Graduation Application Rejected",
                $"Your graduation application was rejected at the {stage} review stage. Reason: {note ?? "No reason provided."}",
                NotificationType.General,
                new[] { student.UserId }, ct);

        var name  = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var prog  = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);
        return ToSummary(app, name, regNo, prog);
    }

    // ── Certificate Generation ────────────────────────────────────────────────

    // Final-Touches Phase 18 Stage 18.2 — generate QuestPDF certificate via infrastructure abstraction
    public async Task<string> GenerateCertificateAsync(Guid applicationId, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct)
            ?? throw new KeyNotFoundException($"Application {applicationId} not found.");

        if (app.Status != GraduationApplicationStatus.Approved)
            throw new InvalidOperationException("Certificate can only be generated for Approved applications.");

        var studentName = await _repo.GetStudentDisplayNameAsync(app.StudentProfileId, ct);
        var regNo       = await _repo.GetStudentRegistrationNumberAsync(app.StudentProfileId, ct);
        var programName = await _repo.GetStudentProgramNameAsync(app.StudentProfileId, ct);

        // Final-Touches Phase 18 Stage 18.2 — optional custom headline from portal settings
        var headline = await _settings.GetPortalSettingAsync(CertTemplateSetting, ct);

        byte[] pdfBytes = await _certGenerator.GeneratePdfAsync(studentName, regNo, programName, headline, ct);

        // Final-Touches Phase 18 Stage 18.2 — write to wwwroot/certificates/
        var wwwRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", CertFolder);
        Directory.CreateDirectory(wwwRoot);
        var fileName     = $"certificate_{app.StudentProfileId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var fullPath     = Path.Combine(wwwRoot, fileName);
        var relativePath = $"/{CertFolder}/{fileName}";

        await File.WriteAllBytesAsync(fullPath, pdfBytes, ct);

        app.AttachCertificate(relativePath);
        _repo.Update(app);
        await _repo.SaveChangesAsync(ct);

        return relativePath;
    }

    // Final-Touches Phase 18 Stage 18.2 — return PDF bytes for download
    public async Task<byte[]?> DownloadCertificateAsync(
        Guid applicationId, Guid requestingStudentProfileId, CancellationToken ct = default)
    {
        var app = await _repo.GetByIdAsync(applicationId, ct);
        if (app is null) return null;

        // Students can only download their own certificate
        if (app.StudentProfileId != requestingStudentProfileId) return null;
        if (app.CertificatePath is null) return null;

        var wwwRoot  = Directory.GetCurrentDirectory();
        var fullPath = Path.Combine(wwwRoot, "wwwroot", app.CertificatePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath)) return null;

        return await File.ReadAllBytesAsync(fullPath, ct);
    }
}
