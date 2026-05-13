using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class StudentLifecycleIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public StudentLifecycleIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GraduationCandidates_WithAdminInstitutionMismatch_ReturnsForbidden()
    {
        var seeded = await SeedLifecycleScopeDataAsync(InstitutionType.University, InstitutionType.College);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.GetAsync($"api/v1/student-lifecycle/graduation-candidates/{seeded.DepartmentId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GraduationCandidates_WithAdminMatchingInstitution_ReturnsOk()
    {
        var seeded = await SeedLifecycleScopeDataAsync(InstitutionType.University, InstitutionType.University);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.GetAsync($"api/v1/student-lifecycle/graduation-candidates/{seeded.DepartmentId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PromoteStudent_WithAdminInstitutionMismatch_ReturnsForbidden()
    {
        var seeded = await SeedLifecycleScopeDataAsync(InstitutionType.College, InstitutionType.University);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.PostAsync($"api/v1/student-lifecycle/{seeded.StudentProfileId}/promote", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AcademicLevelStudents_WithAdminMatchingInstitution_ReturnsOk()
    {
        var seeded = await SeedLifecycleScopeDataAsync(InstitutionType.School, InstitutionType.School);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.GetAsync($"api/v1/student-lifecycle/academic-level-students/{seeded.DepartmentId}/1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private HttpClient CreateAdminClient(Guid adminUserId, int institutionType)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateToken(
                role: "Admin",
                userId: adminUserId.ToString(),
                institutionType: institutionType));
        return client;
    }

    private async Task<(Guid DepartmentId, Guid StudentProfileId, Guid AdminUserId, int AdminInstitutionType)> SeedLifecycleScopeDataAsync(
        InstitutionType departmentInstitutionType,
        InstitutionType adminInstitutionType)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminRole = db.Roles.First(r => r.Name == "Admin");
        var studentRole = db.Roles.First(r => r.Name == "Student");

        var suffix = Guid.NewGuid().ToString("N")[..6];
        var department = new Department($"Lifecycle Dept {suffix}", $"LFD{suffix}", departmentInstitutionType);
        db.Departments.Add(department);

        var program = new AcademicProgram($"Lifecycle Program {suffix}", $"LFP{suffix}", department.Id, 8);
        db.AcademicPrograms.Add(program);

        var admin = new User(
            username: $"lifecycle_admin_{suffix}",
            passwordHash: "integration-hash",
            roleId: adminRole.Id,
            email: $"lifecycle_admin_{suffix}@tabsan.local",
            institutionType: adminInstitutionType);
        db.Users.Add(admin);

        var studentUser = new User(
            username: $"lifecycle_student_{suffix}",
            passwordHash: "integration-hash",
            roleId: studentRole.Id,
            email: $"lifecycle_student_{suffix}@tabsan.local");
        db.Users.Add(studentUser);

        var studentProfile = new StudentProfile(
            studentUser.Id,
            $"LIFE-{suffix}",
            program.Id,
            department.Id,
            DateTime.UtcNow.Date);
        db.StudentProfiles.Add(studentProfile);

        db.AdminDepartmentAssignments.Add(new AdminDepartmentAssignment(admin.Id, department.Id));

        await db.SaveChangesAsync();

        return (department.Id, studentProfile.Id, admin.Id, (int)adminInstitutionType);
    }
}
