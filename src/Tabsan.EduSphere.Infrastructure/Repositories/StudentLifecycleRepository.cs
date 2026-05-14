using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.StudentLifecycle;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IStudentLifecycleRepository.
/// Handles graduation, semester completion/promotion, dropouts, change requests, and payment receipts.
/// </summary>
public class StudentLifecycleRepository : IStudentLifecycleRepository
{
    private readonly ApplicationDbContext _db;

    public StudentLifecycleRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── Graduation ────────────────────────────────────────────────────────
    public async Task<IList<StudentProfile>> GetFinalSemesterStudentsByDepartmentAsync(
        Guid departmentId,
        CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(sp => sp.DepartmentId == departmentId && sp.Status == StudentStatus.Active)
            .Include(sp => sp.Program)
            .ToListAsync(ct);
    }

    public async Task<IList<StudentProfile>> GetStudentsByStatusAsync(
        Guid departmentId,
        StudentStatus status,
        CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(sp => sp.DepartmentId == departmentId && sp.Status == status)
            .Include(sp => sp.Program)
            .Include(sp => sp.Department)
            .ToListAsync(ct);
    }

    public async Task<StudentProfile?> GetByIdAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .Include(sp => sp.Program)
            .Include(sp => sp.Department)
            .FirstOrDefaultAsync(sp => sp.Id == studentProfileId, ct);
    }

    // ── Semester Promotion ────────────────────────────────────────────────
    public async Task<IList<StudentProfile>> GetActiveStudentsBySemesterAsync(
        Guid departmentId,
        int semesterNumber,
        CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(sp => sp.DepartmentId == departmentId
                      && sp.Status == StudentStatus.Active
                      && sp.CurrentSemesterNumber == semesterNumber)
            .Include(sp => sp.Program)
            .ToListAsync(ct);
    }

    public async Task<IList<StudentProfile>> GetActiveStudentsBySemesterRangeAsync(
        Guid departmentId,
        int startSemesterNumber,
        int endSemesterNumber,
        CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(sp => sp.DepartmentId == departmentId
                      && sp.Status == StudentStatus.Active
                      && sp.CurrentSemesterNumber >= startSemesterNumber
                      && sp.CurrentSemesterNumber <= endSemesterNumber)
            .Include(sp => sp.Program)
            .OrderBy(sp => sp.CurrentSemesterNumber)
            .ThenBy(sp => sp.RegistrationNumber)
            .ToListAsync(ct);
    }

    public async Task UpdateAsync(StudentProfile student, CancellationToken ct = default)
    {
        _db.StudentProfiles.Update(student);
        await _db.SaveChangesAsync(ct);
    }

    // ── Admin Change Requests ──────────────────────────────────────────────
    public async Task<IList<AdminChangeRequest>> GetPendingChangeRequestsAsync(
        CancellationToken ct = default)
    {
        return await _db.AdminChangeRequests
            .AsNoTracking()
            .Where(acr => acr.Status == ChangeRequestStatus.Pending)
            .Include(acr => acr.Requestor)
            .OrderByDescending(acr => acr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<AdminChangeRequest>> GetPendingChangeRequestsByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _db.AdminChangeRequests
            .AsNoTracking()
            .Where(acr => acr.RequestorUserId == userId && acr.Status == ChangeRequestStatus.Pending)
            .OrderByDescending(acr => acr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<AdminChangeRequest?> GetChangeRequestByIdAsync(
        Guid requestId,
        CancellationToken ct = default)
    {
        return await _db.AdminChangeRequests
            .Include(acr => acr.Requestor)
            .Include(acr => acr.ReviewedByUser)
            .FirstOrDefaultAsync(acr => acr.Id == requestId, ct);
    }

    public async Task<IList<AdminChangeRequest>> GetAllChangeRequestsByUserAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return await _db.AdminChangeRequests
            .AsNoTracking()
            .Where(acr => acr.RequestorUserId == userId)
            .OrderByDescending(acr => acr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddChangeRequestAsync(AdminChangeRequest request, CancellationToken ct = default)
    {
        await _db.AdminChangeRequests.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateChangeRequestAsync(AdminChangeRequest request, CancellationToken ct = default)
    {
        _db.AdminChangeRequests.Update(request);
        await _db.SaveChangesAsync(ct);
    }

    // ── Teacher Modification Requests ──────────────────────────────────────
    public async Task<IList<TeacherModificationRequest>> GetPendingModificationRequestsAsync(
        CancellationToken ct = default)
    {
        return await _db.TeacherModificationRequests
            .AsNoTracking()
            .Where(tmr => tmr.Status == ModificationRequestStatus.Pending)
            .Include(tmr => tmr.Teacher)
            .OrderByDescending(tmr => tmr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<TeacherModificationRequest>> GetPendingModificationRequestsByTeacherAsync(
        Guid teacherUserId,
        CancellationToken ct = default)
    {
        return await _db.TeacherModificationRequests
            .AsNoTracking()
            .Where(tmr => tmr.TeacherUserId == teacherUserId && tmr.Status == ModificationRequestStatus.Pending)
            .OrderByDescending(tmr => tmr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<TeacherModificationRequest?> GetModificationRequestByIdAsync(
        Guid requestId,
        CancellationToken ct = default)
    {
        return await _db.TeacherModificationRequests
            .Include(tmr => tmr.Teacher)
            .Include(tmr => tmr.ReviewedByUser)
            .FirstOrDefaultAsync(tmr => tmr.Id == requestId, ct);
    }

    public async Task<IList<TeacherModificationRequest>> GetAllModificationRequestsByTeacherAsync(
        Guid teacherUserId,
        CancellationToken ct = default)
    {
        return await _db.TeacherModificationRequests
            .AsNoTracking()
            .Where(tmr => tmr.TeacherUserId == teacherUserId)
            .OrderByDescending(tmr => tmr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task AddModificationRequestAsync(TeacherModificationRequest request, CancellationToken ct = default)
    {
        await _db.TeacherModificationRequests.AddAsync(request, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateModificationRequestAsync(TeacherModificationRequest request, CancellationToken ct = default)
    {
        _db.TeacherModificationRequests.Update(request);
        await _db.SaveChangesAsync(ct);
    }

    // ── Payment Receipts ───────────────────────────────────────────────────
    public async Task<IList<PaymentReceipt>> GetActiveReceiptsByStudentAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.StudentProfileId == studentProfileId && pr.Status != PaymentReceiptStatus.Cancelled)
            .Include(pr => pr.CreatedByUser)
            .Include(pr => pr.ConfirmedByUser)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<PaymentReceipt>> GetActiveReceiptsByStudentPagedAsync(
        Guid studentProfileId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.StudentProfileId == studentProfileId && pr.Status != PaymentReceiptStatus.Cancelled)
            .Include(pr => pr.CreatedByUser)
            .Include(pr => pr.ConfirmedByUser)
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountActiveReceiptsByStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => _db.PaymentReceipts.AsNoTracking()
            .CountAsync(pr => pr.StudentProfileId == studentProfileId && pr.Status != PaymentReceiptStatus.Cancelled, ct);

    public async Task<IList<PaymentReceipt>> GetAllReceiptsByStudentAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.StudentProfileId == studentProfileId)
            .Include(pr => pr.CreatedByUser)
            .Include(pr => pr.ConfirmedByUser)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<PaymentReceipt>> GetAllReceiptsByStudentPagedAsync(
        Guid studentProfileId,
        int skip,
        int take,
        CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.StudentProfileId == studentProfileId)
            .Include(pr => pr.CreatedByUser)
            .Include(pr => pr.ConfirmedByUser)
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAllReceiptsByStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => _db.PaymentReceipts.AsNoTracking().CountAsync(pr => pr.StudentProfileId == studentProfileId, ct);

    public async Task<PaymentReceipt?> GetReceiptByIdAsync(Guid receiptId, CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .Include(pr => pr.StudentProfile)
            .Include(pr => pr.CreatedByUser)
            .Include(pr => pr.ConfirmedByUser)
            .FirstOrDefaultAsync(pr => pr.Id == receiptId, ct);
    }

    public async Task<IList<PaymentReceipt>> GetUnpaidReceiptsByStudentAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.StudentProfileId == studentProfileId &&
                         (pr.Status == PaymentReceiptStatus.Pending || pr.Status == PaymentReceiptStatus.Submitted))
            .OrderByDescending(pr => pr.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IList<PaymentReceipt>> GetAllUnpaidReceiptsAsync(CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.Status == PaymentReceiptStatus.Pending || pr.Status == PaymentReceiptStatus.Submitted)
            .Include(pr => pr.StudentProfile)
            .OrderByDescending(pr => pr.DueDate)
            .ToListAsync(ct);
    }

    public async Task<IList<PaymentReceipt>> GetAllUnpaidReceiptsPagedAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Where(pr => pr.Status == PaymentReceiptStatus.Pending || pr.Status == PaymentReceiptStatus.Submitted)
            .Include(pr => pr.StudentProfile)
            .OrderByDescending(pr => pr.DueDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAllUnpaidReceiptsAsync(CancellationToken ct = default)
        => _db.PaymentReceipts.AsNoTracking().CountAsync(pr => pr.Status == PaymentReceiptStatus.Pending || pr.Status == PaymentReceiptStatus.Submitted, ct);

    // Final-Touches Phase 7 Stage 7.2 — all receipts for admin + student profile by user ID
    public async Task<IList<PaymentReceipt>> GetAllReceiptsAsync(CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Include(pr => pr.StudentProfile)
            .OrderByDescending(pr => pr.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IList<PaymentReceipt>> GetAllReceiptsPagedAsync(int skip, int take, CancellationToken ct = default)
    {
        return await _db.PaymentReceipts
            .AsNoTracking()
            .Include(pr => pr.StudentProfile)
            .OrderByDescending(pr => pr.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public Task<int> CountAllReceiptsAsync(CancellationToken ct = default)
        => _db.PaymentReceipts.AsNoTracking().CountAsync(ct);

    public async Task<StudentProfile?> GetStudentProfileByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _db.StudentProfiles
            .AsNoTracking()
            .Where(sp => sp.UserId == userId)
            .Include(sp => sp.Program)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddReceiptAsync(PaymentReceipt receipt, CancellationToken ct = default)
    {
        await _db.PaymentReceipts.AddAsync(receipt, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateReceiptAsync(PaymentReceipt receipt, CancellationToken ct = default)
    {
        _db.PaymentReceipts.Update(receipt);
        await _db.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _db.SaveChangesAsync(ct);
    }
}
