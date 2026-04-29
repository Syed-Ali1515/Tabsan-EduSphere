using System.Text.Json;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.StudentLifecycle;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Implementation of IStudentLifecycleService.
/// Manages graduation, status changes, change requests, modification workflows, and payment receipts.
/// Notifications are sent via INotificationService after state changes are persisted.
/// </summary>
public class StudentLifecycleService : IStudentLifecycleService
{
    private readonly IStudentLifecycleRepository _repository;

    public StudentLifecycleService(IStudentLifecycleRepository repository)
    {
        _repository = repository;
    }

    // ── Graduation ────────────────────────────────────────────────────────
    public async Task<IList<GraduationSummaryDto>> GetGraduationCandidatesByDepartmentAsync(
        Guid departmentId,
        CancellationToken ct = default)
    {
        var students = await _repository.GetFinalSemesterStudentsByDepartmentAsync(departmentId, ct);
        
        return students.Select(s => new GraduationSummaryDto(
            s.Id,
            s.RegistrationNumber,
            "", // Student name loaded separately from User table via JOIN
            s.Program.Name,
            s.CurrentSemesterNumber,
            s.GraduatedDate
        )).ToList();
    }

    public async Task GraduateStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var student = await _repository.GetByIdAsync(studentProfileId, ct);
        if (student == null)
            throw new KeyNotFoundException($"Student profile {studentProfileId} not found.");

        student.Graduate();
        await _repository.UpdateAsync(student, ct);

        // TODO: Send graduation notification to student
    }

    public async Task GraduateStudentsBatchAsync(
        IList<Guid> studentProfileIds,
        CancellationToken ct = default)
    {
        foreach (var studentId in studentProfileIds)
        {
            await GraduateStudentAsync(studentId, ct);
        }
    }

    // ── Semester Completion & Promotion ───────────────────────────────────
    public async Task<IList<SemesterPromotionSummaryDto>> GetStudentsBySemesterAsync(
        Guid departmentId,
        int semesterNumber,
        CancellationToken ct = default)
    {
        var students = await _repository.GetActiveStudentsBySemesterAsync(departmentId, semesterNumber, ct);
        return students.Select(s => new SemesterPromotionSummaryDto(
            s.Id,
            s.RegistrationNumber,
            s.Program?.Name ?? string.Empty,
            s.CurrentSemesterNumber
        )).ToList();
    }

    public async Task PromoteStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var student = await _repository.GetByIdAsync(studentProfileId, ct);
        if (student == null)
            throw new KeyNotFoundException($"Student profile {studentProfileId} not found.");

        if (student.Status != Domain.Enums.StudentStatus.Active)
            throw new InvalidOperationException($"Only Active students can be promoted. Student {studentProfileId} has status {student.Status}.");

        student.AdvanceSemester();
        await _repository.UpdateAsync(student, ct);

        // TODO: Send semester promotion notification to student
    }

    public async Task<PromotionBatchResultDto> PromoteStudentsBatchAsync(
        IList<Guid> studentProfileIds,
        CancellationToken ct = default)
    {
        int promoted = 0;
        var errors = new List<string>();

        foreach (var id in studentProfileIds)
        {
            try
            {
                await PromoteStudentAsync(id, ct);
                promoted++;
            }
            catch (KeyNotFoundException e)
            {
                errors.Add(e.Message);
            }
            catch (InvalidOperationException e)
            {
                errors.Add(e.Message);
            }
        }

        return new PromotionBatchResultDto(promoted, errors.Count, errors);
    }

    // ── Student Status Management ──────────────────────────────────────────
    public async Task DeactivateStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var student = await _repository.GetByIdAsync(studentProfileId, ct);
        if (student == null)
            throw new KeyNotFoundException($"Student profile {studentProfileId} not found.");

        student.Deactivate();
        await _repository.UpdateAsync(student, ct);

        // TODO: Send deactivation notification to student
    }

    public async Task ReactivateStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var student = await _repository.GetByIdAsync(studentProfileId, ct);
        if (student == null)
            throw new KeyNotFoundException($"Student profile {studentProfileId} not found.");

        student.Reactivate();
        await _repository.UpdateAsync(student, ct);

        // TODO: Send reactivation notification to student
    }

    // ── Admin Change Requests ──────────────────────────────────────────────
    public async Task<AdminChangeRequestDto> CreateChangeRequestAsync(
        Guid requestorUserId,
        CreateChangeRequestCommand cmd,
        CancellationToken ct = default)
    {
        var request = new AdminChangeRequest(
            requestorUserId,
            cmd.ChangeDescription,
            cmd.NewData,
            cmd.Reason
        );

        await _repository.AddChangeRequestAsync(request, ct);

        // TODO: Send change request notification to admins
        return MapChangeRequest(request);
    }

    public async Task<IList<AdminChangeRequestDto>> GetPendingChangeRequestsAsync(
        CancellationToken ct = default)
    {
        var requests = await _repository.GetPendingChangeRequestsAsync(ct);
        return requests.Select(MapChangeRequest).ToList();
    }

    public async Task<IList<AdminChangeRequestDto>> GetChangeRequestsByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var requests = await _repository.GetAllChangeRequestsByUserAsync(userId, ct);
        return requests.Select(MapChangeRequest).ToList();
    }

    public async Task<AdminChangeRequestDto?> GetChangeRequestByIdAsync(Guid requestId, CancellationToken ct = default)
    {
        var request = await _repository.GetChangeRequestByIdAsync(requestId, ct);
        return request != null ? MapChangeRequest(request) : null;
    }

    public async Task ApproveChangeRequestAsync(
        Guid requestId,
        Guid adminUserId,
        string? notes = null,
        CancellationToken ct = default)
    {
        var request = await _repository.GetChangeRequestByIdAsync(requestId, ct);
        if (request == null)
            throw new KeyNotFoundException($"Change request {requestId} not found.");

        request.Approve(adminUserId, notes);
        await _repository.UpdateChangeRequestAsync(request, ct);

        // TODO: Send approval notification to requestor
    }

    public async Task RejectChangeRequestAsync(
        Guid requestId,
        Guid adminUserId,
        string? notes = null,
        CancellationToken ct = default)
    {
        var request = await _repository.GetChangeRequestByIdAsync(requestId, ct);
        if (request == null)
            throw new KeyNotFoundException($"Change request {requestId} not found.");

        request.Reject(adminUserId, notes);
        await _repository.UpdateChangeRequestAsync(request, ct);

        // TODO: Send rejection notification to requestor
    }

    // ── Teacher Modification Requests ──────────────────────────────────────
    public async Task<TeacherModificationRequestDto> CreateModificationRequestAsync(
        Guid teacherUserId,
        CreateModificationRequestCommand cmd,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<ModificationType>(cmd.ModificationType, true, out var modType))
            throw new ArgumentException($"Invalid modification type: {cmd.ModificationType}");

        var request = new TeacherModificationRequest(
            teacherUserId,
            modType,
            cmd.RecordId,
            cmd.Reason,
            cmd.ProposedData
        );

        await _repository.AddModificationRequestAsync(request, ct);

        // TODO: Send modification request notification to admins
        return MapModificationRequest(request);
    }

    public async Task<IList<TeacherModificationRequestDto>> GetPendingModificationRequestsAsync(
        CancellationToken ct = default)
    {
        var requests = await _repository.GetPendingModificationRequestsAsync(ct);
        return requests.Select(MapModificationRequest).ToList();
    }

    public async Task<IList<TeacherModificationRequestDto>> GetModificationRequestsByTeacherAsync(
        Guid teacherUserId,
        CancellationToken ct = default)
    {
        var requests = await _repository.GetAllModificationRequestsByTeacherAsync(teacherUserId, ct);
        return requests.Select(MapModificationRequest).ToList();
    }

    public async Task<TeacherModificationRequestDto?> GetModificationRequestByIdAsync(
        Guid requestId,
        CancellationToken ct = default)
    {
        var request = await _repository.GetModificationRequestByIdAsync(requestId, ct);
        return request != null ? MapModificationRequest(request) : null;
    }

    public async Task ApproveModificationRequestAsync(
        Guid requestId,
        Guid adminUserId,
        string? notes = null,
        CancellationToken ct = default)
    {
        var request = await _repository.GetModificationRequestByIdAsync(requestId, ct);
        if (request == null)
            throw new KeyNotFoundException($"Modification request {requestId} not found.");

        request.Approve(adminUserId, notes);
        await _repository.UpdateModificationRequestAsync(request, ct);

        // TODO: Send approval notification to teacher
    }

    public async Task RejectModificationRequestAsync(
        Guid requestId,
        Guid adminUserId,
        string? notes = null,
        CancellationToken ct = default)
    {
        var request = await _repository.GetModificationRequestByIdAsync(requestId, ct);
        if (request == null)
            throw new KeyNotFoundException($"Modification request {requestId} not found.");

        request.Reject(adminUserId, notes);
        await _repository.UpdateModificationRequestAsync(request, ct);

        // TODO: Send rejection notification to teacher
    }

    // ── Payment Receipts ───────────────────────────────────────────────────
    public async Task<PaymentReceiptDto> CreatePaymentReceiptAsync(
        Guid financeUserId,
        CreatePaymentReceiptCommand cmd,
        CancellationToken ct = default)
    {
        var receipt = new PaymentReceipt(
            cmd.StudentProfileId,
            financeUserId,
            cmd.Amount,
            cmd.Description,
            cmd.DueDate
        );

        await _repository.AddReceiptAsync(receipt, ct);

        // TODO: Send receipt creation notification to student
        var student = await _repository.GetByIdAsync(cmd.StudentProfileId, ct);
        return MapPaymentReceipt(receipt, student?.UserId.ToString() ?? "");
    }

    public async Task<IList<PaymentReceiptDto>> GetActiveReceiptsByStudentAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        var student = await _repository.GetByIdAsync(studentProfileId, ct);
        var receipts = await _repository.GetActiveReceiptsByStudentAsync(studentProfileId, ct);
        return receipts.Select(r => MapPaymentReceipt(r, student?.UserId.ToString() ?? "")).ToList();
    }

    public async Task<StudentFeeStatusDto> GetStudentFeeStatusAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        var unpaidReceipts = await _repository.GetUnpaidReceiptsByStudentAsync(studentProfileId, ct);
        var student = await _repository.GetByIdAsync(studentProfileId, ct);

        var dtos = unpaidReceipts.Select(r => MapPaymentReceipt(r, student?.UserId.ToString() ?? "")).ToList();

        return new StudentFeeStatusDto(
            studentProfileId,
            unpaidReceipts.Any(),
            unpaidReceipts.Count,
            unpaidReceipts.Sum(r => r.Amount),
            dtos
        );
    }

    public async Task<PaymentReceiptDto?> GetPaymentReceiptByIdAsync(Guid receiptId, CancellationToken ct = default)
    {
        var receipt = await _repository.GetReceiptByIdAsync(receiptId, ct);
        if (receipt == null)
            return null;

        var studentId = receipt.StudentProfile?.Id;
        return MapPaymentReceipt(receipt, studentId?.ToString() ?? "");
    }

    public async Task SubmitPaymentProofAsync(Guid receiptId, string proofPath, CancellationToken ct = default)
    {
        var receipt = await _repository.GetReceiptByIdAsync(receiptId, ct);
        if (receipt == null)
            throw new KeyNotFoundException($"Receipt {receiptId} not found.");

        receipt.SubmitProof(proofPath);
        await _repository.UpdateReceiptAsync(receipt, ct);

        // TODO: Send proof submission notification to finance/admins
    }

    public async Task ConfirmPaymentAsync(
        Guid receiptId,
        Guid financeUserId,
        string? notes = null,
        CancellationToken ct = default)
    {
        var receipt = await _repository.GetReceiptByIdAsync(receiptId, ct);
        if (receipt == null)
            throw new KeyNotFoundException($"Receipt {receiptId} not found.");

        receipt.ConfirmPayment(financeUserId, notes);
        await _repository.UpdateReceiptAsync(receipt, ct);

        // TODO: Send payment confirmation notification to student
    }

    public async Task CancelReceiptAsync(
        Guid receiptId,
        Guid financeUserId,
        string? reason = null,
        CancellationToken ct = default)
    {
        var receipt = await _repository.GetReceiptByIdAsync(receiptId, ct);
        if (receipt == null)
            throw new KeyNotFoundException($"Receipt {receiptId} not found.");

        receipt.Cancel(financeUserId, reason);
        await _repository.UpdateReceiptAsync(receipt, ct);

        // TODO: Send cancellation notification to student
    }

    // ── Mapping helpers ────────────────────────────────────────────────────
    private static AdminChangeRequestDto MapChangeRequest(AdminChangeRequest request)
    {
        return new AdminChangeRequestDto(
            request.Id,
            request.ChangeDescription,
            request.NewData,
            request.Reason,
            request.Status.ToString(),
            request.CreatedAt,
            request.AdminNotes,
            request.ReviewedAt
        );
    }

    private static TeacherModificationRequestDto MapModificationRequest(TeacherModificationRequest request)
    {
        return new TeacherModificationRequestDto(
            request.Id,
            request.ModificationType.ToString(),
            request.RecordId,
            request.Reason,
            request.ProposedData,
            request.Status.ToString(),
            request.CreatedAt,
            request.AdminNotes,
            request.ReviewedAt
        );
    }

    private static PaymentReceiptDto MapPaymentReceipt(PaymentReceipt receipt, string studentName = "")
    {
        return new PaymentReceiptDto(
            receipt.Id,
            receipt.StudentProfileId,
            studentName,
            receipt.Amount,
            receipt.Description,
            receipt.DueDate,
            receipt.Status.ToString(),
            receipt.ProofOfPaymentPath,
            receipt.ProofUploadedAt,
            receipt.ConfirmedAt,
            receipt.Notes,
            receipt.CreatedAt
        );
    }
}
