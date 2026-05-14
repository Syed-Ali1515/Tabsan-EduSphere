using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Tabsan.EduSphere.Application.Academic;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;
using Xunit;

namespace Tabsan.EduSphere.UnitTests;

// Phase 26 — School and College Functional Expansion Unit Tests

public class SchoolStreamServiceTests
{
    [Fact]
    public async Task UpsertStream_Create_ReturnsCreatedDto()
    {
        var streamRepo = new StubSchoolStreamRepository();
        var sut = new SchoolStreamService(streamRepo, new StubStudentProfileRepository([]));

        var dto = await sut.UpsertStreamAsync(null, new SaveSchoolStreamRequest("Science", "Bio/Math", false));

        dto.Name.Should().Be("Science");
        dto.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task AssignStudent_WhenStudentMissing_Throws()
    {
        var streamRepo = new StubSchoolStreamRepository();
        var created = new SchoolStream("Commerce", null);
        await streamRepo.AddStreamAsync(created);

        var sut = new SchoolStreamService(streamRepo, new StubStudentProfileRepository([]));

        var act = () => sut.AssignStudentAsync(new AssignStudentStreamRequest(Guid.NewGuid(), created.Id, Guid.NewGuid()));
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AssignAndGetStudentAssignment_ReturnsMappedDto()
    {
        var student = TestData.MakeStudent(semester: 9, institutionType: InstitutionType.School);
        var streamRepo = new StubSchoolStreamRepository();
        var stream = new SchoolStream("Science", null);
        await streamRepo.AddStreamAsync(stream);

        var sut = new SchoolStreamService(streamRepo, new StubStudentProfileRepository([student]));

        await sut.AssignStudentAsync(new AssignStudentStreamRequest(student.Id, stream.Id, Guid.NewGuid()));
        var assignment = await sut.GetStudentAssignmentAsync(student.Id);

        assignment.Should().NotBeNull();
        assignment!.StreamName.Should().Be("Science");
    }

    [Fact]
    public async Task AssignStudent_WhenDepartmentIsNotSchool_ThrowsInvalidOperation()
    {
        var student = TestData.MakeStudent(semester: 9, institutionType: InstitutionType.University);
        var streamRepo = new StubSchoolStreamRepository();
        var stream = new SchoolStream("Science", null);
        await streamRepo.AddStreamAsync(stream);

        var sut = new SchoolStreamService(streamRepo, new StubStudentProfileRepository([student]));

        var act = () => sut.AssignStudentAsync(new AssignStudentStreamRequest(student.Id, stream.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*School institution*");
    }

    [Fact]
    public async Task AssignStudent_WhenGradeOutsideRange_ThrowsInvalidOperation()
    {
        var student = TestData.MakeStudent(semester: 8, institutionType: InstitutionType.School);
        var streamRepo = new StubSchoolStreamRepository();
        var stream = new SchoolStream("Biology", null);
        await streamRepo.AddStreamAsync(stream);

        var sut = new SchoolStreamService(streamRepo, new StubStudentProfileRepository([student]));

        var act = () => sut.AssignStudentAsync(new AssignStudentStreamRequest(student.Id, stream.Id, Guid.NewGuid()));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Grades 9-12*");
    }
}

public class ReportCardServiceTests
{
    [Fact]
    public async Task GenerateAndGetLatest_ReturnsSavedCard()
    {
        var repo = new StubReportCardRepository();
        var sut = new ReportCardService(repo);
        var studentId = Guid.NewGuid();

        var created = await sut.GenerateAsync(new GenerateReportCardRequest(
            studentId,
            InstitutionType.School,
            "Grade 9",
            "{\"Total\":87.5}",
            Guid.NewGuid()));

        var latest = await sut.GetLatestAsync(studentId);

        latest.Should().NotBeNull();
        latest!.Id.Should().Be(created.Id);
        latest.PeriodLabel.Should().Be("Grade 9");
    }
}

public class BulkPromotionServiceTests
{
    [Fact]
    public async Task ApplyBatch_AdvancesOnlyPromoteEntries()
    {
        var s1 = TestData.MakeStudent(semester: 9);
        var s2 = TestData.MakeStudent(semester: 9);

        var studentRepo = new StubStudentProfileRepository([s1, s2]);
        var repo = new StubBulkPromotionRepository();
        var progression = new ProgressionService(
            studentRepo,
            new StubInstitutionGradingProfileRepository(null));
        var sut = new BulkPromotionService(repo, studentRepo, progression);

        var batch = await sut.CreateBatchAsync(new CreateBulkPromotionBatchRequest("Grade 9 Promotion", Guid.NewGuid()));
        await sut.AddEntriesAsync(new AddBulkPromotionEntriesRequest(batch.Id,
        [
            new BulkPromotionEntryInput(s1.Id, EntryDecision.Promote),
            new BulkPromotionEntryInput(s2.Id, EntryDecision.Hold, "Below threshold")
        ]));
        await sut.SubmitAsync(batch.Id);
        await sut.ReviewAsync(new ReviewBulkPromotionBatchRequest(batch.Id, Guid.NewGuid(), true, "Approved"));

        var applied = await sut.ApplyAsync(new ApplyBulkPromotionBatchRequest(batch.Id));

        applied.Status.Should().Be(BulkPromotionStatus.Applied);
        studentRepo.GetRequired(s1.Id).CurrentSemesterNumber.Should().Be(10);
        studentRepo.GetRequired(s2.Id).CurrentSemesterNumber.Should().Be(9);
        applied.Entries.Count(e => e.IsApplied).Should().Be(1);
    }

    [Fact]
    public async Task ApplyBatch_WhenNotApproved_Throws()
    {
        var s1 = TestData.MakeStudent(semester: 9);
        var studentRepo = new StubStudentProfileRepository([s1]);
        var repo = new StubBulkPromotionRepository();
        var progression = new ProgressionService(
            studentRepo,
            new StubInstitutionGradingProfileRepository(null));
        var sut = new BulkPromotionService(repo, studentRepo, progression);

        var batch = await sut.CreateBatchAsync(new CreateBulkPromotionBatchRequest("X", Guid.NewGuid()));

        var act = () => sut.ApplyAsync(new ApplyBulkPromotionBatchRequest(batch.Id));
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public void BulkPromotionBatch_SubmitWithoutEntries_Throws()
    {
        var batch = new BulkPromotionBatch("Promotion", Guid.NewGuid());
        var act = batch.Submit;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task ApplyBatch_CollegePromoteEntryWithoutPassingScore_BecomesHoldWithSupplementaryReason()
    {
        var collegeStudent = TestData.MakeStudent(semester: 1, institutionType: InstitutionType.College);
        TestData.Set(collegeStudent, nameof(StudentProfile.CurrentSemesterGpa), 1.2m); // 30%

        var studentRepo = new StubStudentProfileRepository([collegeStudent]);
        var repo = new StubBulkPromotionRepository();
        var progression = new ProgressionService(
            studentRepo,
            new StubInstitutionGradingProfileRepository(null));
        var sut = new BulkPromotionService(repo, studentRepo, progression);

        var batch = await sut.CreateBatchAsync(new CreateBulkPromotionBatchRequest("College Y1", Guid.NewGuid()));
        await sut.AddEntriesAsync(new AddBulkPromotionEntriesRequest(batch.Id,
        [
            new BulkPromotionEntryInput(collegeStudent.Id, EntryDecision.Promote)
        ]));
        await sut.SubmitAsync(batch.Id);
        await sut.ReviewAsync(new ReviewBulkPromotionBatchRequest(batch.Id, Guid.NewGuid(), true, "Approved"));

        var applied = await sut.ApplyAsync(new ApplyBulkPromotionBatchRequest(batch.Id));
        var entry = applied.Entries.Single();

        studentRepo.GetRequired(collegeStudent.Id).CurrentSemesterNumber.Should().Be(1);
        entry.Decision.Should().Be(EntryDecision.Hold);
        entry.IsApplied.Should().BeFalse();
        entry.Reason.Should().Contain("Supplementary required");
    }

    [Fact]
    public async Task ApplyBatch_CollegePromoteEntryWithPassingScore_AdvancesByAcademicYear()
    {
        var collegeStudent = TestData.MakeStudent(semester: 1, institutionType: InstitutionType.College);
        TestData.Set(collegeStudent, nameof(StudentProfile.CurrentSemesterGpa), 2.4m); // 60%

        var studentRepo = new StubStudentProfileRepository([collegeStudent]);
        var repo = new StubBulkPromotionRepository();
        var progression = new ProgressionService(
            studentRepo,
            new StubInstitutionGradingProfileRepository(null));
        var sut = new BulkPromotionService(repo, studentRepo, progression);

        var batch = await sut.CreateBatchAsync(new CreateBulkPromotionBatchRequest("College Y1", Guid.NewGuid()));
        await sut.AddEntriesAsync(new AddBulkPromotionEntriesRequest(batch.Id,
        [
            new BulkPromotionEntryInput(collegeStudent.Id, EntryDecision.Promote)
        ]));
        await sut.SubmitAsync(batch.Id);
        await sut.ReviewAsync(new ReviewBulkPromotionBatchRequest(batch.Id, Guid.NewGuid(), true, "Approved"));

        var applied = await sut.ApplyAsync(new ApplyBulkPromotionBatchRequest(batch.Id));
        var entry = applied.Entries.Single();

        studentRepo.GetRequired(collegeStudent.Id).CurrentSemesterNumber.Should().Be(3);
        entry.Decision.Should().Be(EntryDecision.Promote);
        entry.IsApplied.Should().BeTrue();
    }
}

public class ParentPortalServiceTests
{
    [Fact]
    public async Task GetLinkedStudents_ReturnsOnlyActiveAndExistingStudents()
    {
        var parent = Guid.NewGuid();
        var student1 = TestData.MakeStudent(semester: 4);
        var student2 = TestData.MakeStudent(semester: 5);

        var linkRepo = new StubParentLinkRepository([
            new ParentStudentLink(parent, student1.Id, "Father"),
            new ParentStudentLink(parent, student2.Id, "Guardian")
        ]);
        // Deactivate second link to verify filtering.
        var second = (await linkRepo.GetByParentUserIdAsync(parent))[1];
        second.Update("Guardian", false);

        var sut = new ParentPortalService(
            linkRepo,
            new StubStudentProfileRepository([student1, student2]),
            new StubPortalUserRepository(parent, "Parent"));

        var result = await sut.GetLinkedStudentsAsync(parent);

        result.Should().HaveCount(1);
        result[0].StudentProfileId.Should().Be(student1.Id);
        result[0].Relationship.Should().Be("Father");
    }

    [Fact]
    public async Task UpsertLink_WithParentRoleAndSchoolStudent_CreatesLink()
    {
        var parent = Guid.NewGuid();
        var student = TestData.MakeStudent(semester: 6, institutionType: InstitutionType.School);
        var linkRepo = new StubParentLinkRepository([]);

        var sut = new ParentPortalService(
            linkRepo,
            new StubStudentProfileRepository([student]),
            new StubPortalUserRepository(parent, "Parent"));

        var dto = await sut.UpsertLinkAsync(new UpsertParentStudentLinkRequest(parent, student.Id, "Mother", true));

        dto.ParentUserId.Should().Be(parent);
        dto.StudentProfileId.Should().Be(student.Id);
        dto.Relationship.Should().Be("Mother");
        dto.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpsertLink_WithNonParentRole_ThrowsInvalidOperation()
    {
        var parent = Guid.NewGuid();
        var student = TestData.MakeStudent(semester: 6, institutionType: InstitutionType.School);

        var sut = new ParentPortalService(
            new StubParentLinkRepository([]),
            new StubStudentProfileRepository([student]),
            new StubPortalUserRepository(parent, "Student"));

        var act = () => sut.UpsertLinkAsync(new UpsertParentStudentLinkRequest(parent, student.Id, "Guardian", true));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Parent role*");
    }

    [Fact]
    public async Task UpsertLink_WithNonSchoolStudent_ThrowsInvalidOperation()
    {
        var parent = Guid.NewGuid();
        var student = TestData.MakeStudent(semester: 2, institutionType: InstitutionType.College);

        var sut = new ParentPortalService(
            new StubParentLinkRepository([]),
            new StubStudentProfileRepository([student]),
            new StubPortalUserRepository(parent, "Parent"));

        var act = () => sut.UpsertLinkAsync(new UpsertParentStudentLinkRequest(parent, student.Id, "Guardian", true));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*only allowed for School students*");
    }

    [Fact]
    public async Task DeactivateLink_WhenExists_SetsInactive()
    {
        var parent = Guid.NewGuid();
        var student = TestData.MakeStudent(semester: 7, institutionType: InstitutionType.School);
        var existing = new ParentStudentLink(parent, student.Id, "Father");

        var linkRepo = new StubParentLinkRepository([existing]);
        var sut = new ParentPortalService(
            linkRepo,
            new StubStudentProfileRepository([student]),
            new StubPortalUserRepository(parent, "Parent"));

        var changed = await sut.DeactivateLinkAsync(parent, student.Id);

        changed.Should().BeTrue();
        var link = await linkRepo.GetByParentAndStudentAsync(parent, student.Id);
        link.Should().NotBeNull();
        link!.IsActive.Should().BeFalse();
    }
}

file static class TestData
{
    internal static StudentProfile MakeStudent(int semester, InstitutionType institutionType = InstitutionType.University)
    {
        var department = new Department($"Dept-{Guid.NewGuid():N}"[..9], $"D{Guid.NewGuid():N}"[..5], institutionType);

        var profile = (StudentProfile)RuntimeHelpers.GetUninitializedObject(typeof(StudentProfile));
        Set(profile, nameof(StudentProfile.Id), Guid.NewGuid());
        Set(profile, nameof(StudentProfile.UserId), Guid.NewGuid());
        Set(profile, nameof(StudentProfile.RegistrationNumber), $"REG-{Guid.NewGuid():N}"[..12]);
        Set(profile, nameof(StudentProfile.ProgramId), Guid.NewGuid());
        Set(profile, nameof(StudentProfile.DepartmentId), department.Id);
        Set(profile, nameof(StudentProfile.Department), department);
        Set(profile, nameof(StudentProfile.AdmissionDate), DateTime.UtcNow.Date);
        Set(profile, nameof(StudentProfile.Cgpa), 3.0m);
        Set(profile, nameof(StudentProfile.CurrentSemesterGpa), 70m);
        Set(profile, nameof(StudentProfile.CurrentSemesterNumber), semester);
        Set(profile, nameof(StudentProfile.Status), StudentStatus.Active);
        return profile;
    }

    internal static void Set<T>(object target, string propertyName, T value)
    {
        var prop = target.GetType().GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (prop is not null)
        {
            prop.SetValue(target, value);
            return;
        }

        var field = target.GetType().GetField($"<{propertyName}>k__BackingField",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field?.SetValue(target, value);
    }
}

file sealed class StubStudentProfileRepository : IStudentProfileRepository
{
    private readonly Dictionary<Guid, StudentProfile> _students;

    public StubStudentProfileRepository(IEnumerable<StudentProfile> students)
    {
        _students = students.ToDictionary(s => s.Id, s => s);
    }

    public StudentProfile GetRequired(Guid id) => _students[id];

    public Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => Task.FromResult(_students.Values.FirstOrDefault(s => s.UserId == userId));

    public Task<StudentProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_students.GetValueOrDefault(id));

    public Task<StudentProfile?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default)
        => Task.FromResult(_students.Values.FirstOrDefault(s => s.RegistrationNumber == registrationNumber));

    public Task<IReadOnlyList<StudentProfile>> GetAllAsync(Guid? departmentId = null, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<StudentProfile>>(
            departmentId is null
                ? _students.Values.ToList()
                : _students.Values.Where(s => s.DepartmentId == departmentId.Value).ToList());

    public Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct = default)
        => Task.FromResult(_students.Values.Any(s => s.RegistrationNumber == registrationNumber));

    public Task AddAsync(StudentProfile profile, CancellationToken ct = default)
    {
        _students[profile.Id] = profile;
        return Task.CompletedTask;
    }

    public void Update(StudentProfile profile)
        => _students[profile.Id] = profile;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubInstitutionGradingProfileRepository : IInstitutionGradingProfileRepository
{
    private readonly InstitutionGradingProfile? _profile;

    public StubInstitutionGradingProfileRepository(InstitutionGradingProfile? profile)
    {
        _profile = profile;
    }

    public Task<IReadOnlyList<InstitutionGradingProfile>> GetAllAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<InstitutionGradingProfile>>(_profile is null ? [] : [_profile]);

    public Task<InstitutionGradingProfile?> GetByTypeAsync(InstitutionType institutionType, CancellationToken ct = default)
        => Task.FromResult(_profile?.InstitutionType == institutionType ? _profile : null);

    public Task<InstitutionGradingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_profile?.Id == id ? _profile : null);

    public Task AddAsync(InstitutionGradingProfile profile, CancellationToken ct = default)
        => Task.CompletedTask;

    public void Update(InstitutionGradingProfile profile)
    {
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubSchoolStreamRepository : ISchoolStreamRepository
{
    private readonly Dictionary<Guid, SchoolStream> _streams = new();
    private readonly Dictionary<Guid, StudentStreamAssignment> _assignments = new();

    public Task<IReadOnlyList<SchoolStream>> GetAllStreamsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<SchoolStream>>(_streams.Values.OrderBy(s => s.Name).ToList());

    public Task<SchoolStream?> GetStreamByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_streams.GetValueOrDefault(id));

    public Task AddStreamAsync(SchoolStream stream, CancellationToken ct = default)
    {
        _streams[stream.Id] = stream;
        return Task.CompletedTask;
    }

    public void UpdateStream(SchoolStream stream)
        => _streams[stream.Id] = stream;

    public Task<StudentStreamAssignment?> GetStudentAssignmentAsync(Guid studentProfileId, CancellationToken ct = default)
        => Task.FromResult(_assignments.GetValueOrDefault(studentProfileId));

    public Task UpsertStudentAssignmentAsync(StudentStreamAssignment assignment, CancellationToken ct = default)
    {
        _assignments[assignment.StudentProfileId] = assignment;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubReportCardRepository : IReportCardRepository
{
    private readonly List<StudentReportCard> _cards = [];

    public Task AddAsync(StudentReportCard reportCard, CancellationToken ct = default)
    {
        _cards.Add(reportCard);
        return Task.CompletedTask;
    }

    public Task<StudentReportCard?> GetLatestForStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => Task.FromResult(_cards
            .Where(c => c.StudentProfileId == studentProfileId)
            .OrderByDescending(c => c.GeneratedAt)
            .FirstOrDefault());

    public Task<IReadOnlyList<StudentReportCard>> GetForStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<StudentReportCard>>(_cards
            .Where(c => c.StudentProfileId == studentProfileId)
            .OrderByDescending(c => c.GeneratedAt)
            .ToList());

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubBulkPromotionRepository : IBulkPromotionRepository
{
    private readonly Dictionary<Guid, BulkPromotionBatch> _batches = new();
    private readonly Dictionary<Guid, List<BulkPromotionEntry>> _entries = new();

    public Task AddBatchAsync(BulkPromotionBatch batch, CancellationToken ct = default)
    {
        _batches[batch.Id] = batch;
        _entries.TryAdd(batch.Id, []);
        return Task.CompletedTask;
    }

    public Task<BulkPromotionBatch?> GetBatchByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_batches.GetValueOrDefault(id));

    public Task<IReadOnlyList<BulkPromotionBatch>> GetRecentBatchesAsync(int take = 20, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<BulkPromotionBatch>>(_batches.Values.Take(take).ToList());

    public void UpdateBatch(BulkPromotionBatch batch)
        => _batches[batch.Id] = batch;

    public Task AddEntryAsync(BulkPromotionEntry entry, CancellationToken ct = default)
    {
        _entries.TryAdd(entry.BatchId, []);
        _entries[entry.BatchId].Add(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<BulkPromotionEntry>> GetEntriesAsync(Guid batchId, CancellationToken ct = default)
    {
        _entries.TryAdd(batchId, []);
        return Task.FromResult<IReadOnlyList<BulkPromotionEntry>>(_entries[batchId].ToList());
    }

    public void UpdateEntry(BulkPromotionEntry entry)
    {
        _entries.TryAdd(entry.BatchId, []);
        var list = _entries[entry.BatchId];
        var idx = list.FindIndex(e => e.Id == entry.Id);
        if (idx >= 0) list[idx] = entry;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubParentLinkRepository : IParentStudentLinkRepository
{
    private readonly List<ParentStudentLink> _links;

    public StubParentLinkRepository(IEnumerable<ParentStudentLink> links)
    {
        _links = links.ToList();
    }

    public Task<IReadOnlyList<ParentStudentLink>> GetByParentUserIdAsync(Guid parentUserId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ParentStudentLink>>(_links.Where(l => l.ParentUserId == parentUserId).ToList());

    public Task<ParentStudentLink?> GetByParentAndStudentAsync(Guid parentUserId, Guid studentProfileId, CancellationToken ct = default)
        => Task.FromResult(_links.FirstOrDefault(l => l.ParentUserId == parentUserId && l.StudentProfileId == studentProfileId));

    public Task AddAsync(ParentStudentLink link, CancellationToken ct = default)
    {
        _links.Add(link);
        return Task.CompletedTask;
    }

    public void Update(ParentStudentLink link)
    {
        var idx = _links.FindIndex(l => l.Id == link.Id);
        if (idx >= 0) _links[idx] = link;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}

file sealed class StubPortalUserRepository : IUserRepository
{
    private readonly Dictionary<Guid, User> _users;

    public StubPortalUserRepository(Guid userId, string roleName)
    {
        var role = new Role(roleName);
        var user = (User)RuntimeHelpers.GetUninitializedObject(typeof(User));
        TestData.Set(user, nameof(User.Id), userId);
        TestData.Set(user, nameof(User.Username), $"u_{userId:N}"[..10]);
        TestData.Set(user, nameof(User.PasswordHash), "hash");
        TestData.Set(user, nameof(User.Role), role);
        TestData.Set(user, nameof(User.RoleId), 1);
        TestData.Set(user, nameof(User.IsActive), true);
        _users = new Dictionary<Guid, User> { [userId] = user };
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_users.GetValueOrDefault(id));

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => Task.FromResult(_users.Values.FirstOrDefault(u => u.Username == username));

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(_users.Values.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => Task.FromResult(_users.Values.Any(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)));

    public Task<IList<User>> GetLockedAccountsAsync(CancellationToken ct = default)
        => Task.FromResult<IList<User>>([]);

    public Task<IList<User>> GetFacultyUsersAsync(CancellationToken ct = default)
        => Task.FromResult<IList<User>>([]);

    public Task<IList<User>> GetActiveUsersByRolesAsync(IReadOnlyList<string> roleNames, CancellationToken ct = default)
        => GetUsersByRolesAsync(roleNames, includeInactive: false, ct);

    public Task<IList<User>> GetUsersByRolesAsync(IReadOnlyList<string> roleNames, bool includeInactive = false, CancellationToken ct = default)
    {
        var users = _users.Values
            .Where(u => roleNames.Any(r => string.Equals(r, u.Role.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();
        return Task.FromResult<IList<User>>(users);
    }

    public Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default)
        => Task.FromResult<Role?>(new Role(roleName));

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<User> users, CancellationToken ct = default)
    {
        foreach (var user in users)
            _users[user.Id] = user;
        return Task.CompletedTask;
    }

    public void Update(User user)
        => _users[user.Id] = user;

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => Task.FromResult(1);
}
