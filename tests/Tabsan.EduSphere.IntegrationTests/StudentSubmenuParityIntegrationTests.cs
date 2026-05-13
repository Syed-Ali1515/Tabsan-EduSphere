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
public class StudentSubmenuParityIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public StudentSubmenuParityIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task StudentList_WithAdminInstitutionMismatchAndDepartmentFilter_ReturnsForbidden()
    {
        var seeded = await SeedStudentsForInstitutionScopeAsync(
            adminInstitutionType: InstitutionType.College,
            requestedDepartmentInstitutionType: InstitutionType.University,
            secondaryDepartmentInstitutionType: InstitutionType.College);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.GetAsync($"api/v1/student?departmentId={seeded.RequestedDepartmentId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task StudentList_WithAdminInstitutionScope_ReturnsOnlyMatchingInstitutionStudents()
    {
        var seeded = await SeedStudentsForInstitutionScopeAsync(
            adminInstitutionType: InstitutionType.College,
            requestedDepartmentInstitutionType: InstitutionType.University,
            secondaryDepartmentInstitutionType: InstitutionType.College);

        using var client = CreateAdminClient(seeded.AdminUserId, seeded.AdminInstitutionType);

        var response = await client.GetAsync("api/v1/student");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var items = doc.RootElement.EnumerateArray().ToList();

        Assert.NotEmpty(items);
        Assert.All(items, item =>
        {
            var departmentId = item.GetProperty("departmentId").GetGuid();
            Assert.Equal(seeded.SecondaryDepartmentId, departmentId);
        });
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

    private async Task<(Guid AdminUserId, int AdminInstitutionType, Guid RequestedDepartmentId, Guid SecondaryDepartmentId)> SeedStudentsForInstitutionScopeAsync(
        InstitutionType adminInstitutionType,
        InstitutionType requestedDepartmentInstitutionType,
        InstitutionType secondaryDepartmentInstitutionType)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var adminRole = db.Roles.First(r => r.Name == "Admin");
        var studentRole = db.Roles.First(r => r.Name == "Student");

        var suffix = Guid.NewGuid().ToString("N")[..6];

        var requestedDepartment = new Department($"StudentScope Uni {suffix}", $"SSU{suffix}", requestedDepartmentInstitutionType);
        var scopedDepartment = new Department($"StudentScope Col {suffix}", $"SSC{suffix}", secondaryDepartmentInstitutionType);
        db.Departments.AddRange(requestedDepartment, scopedDepartment);

        var requestedProgram = new AcademicProgram($"Program Uni {suffix}", $"PU{suffix}", requestedDepartment.Id, 8);
        var scopedProgram = new AcademicProgram($"Program Col {suffix}", $"PC{suffix}", scopedDepartment.Id, 4);
        db.AcademicPrograms.AddRange(requestedProgram, scopedProgram);

        var admin = new User(
            username: $"submenu_admin_{suffix}",
            passwordHash: "integration-hash",
            roleId: adminRole.Id,
            email: $"submenu_admin_{suffix}@tabsan.local",
            institutionType: adminInstitutionType);
        db.Users.Add(admin);

        var uniStudentUser = new User(
            username: $"submenu_uni_student_{suffix}",
            passwordHash: "integration-hash",
            roleId: studentRole.Id,
            email: $"submenu_uni_student_{suffix}@tabsan.local");
        var colStudentUser = new User(
            username: $"submenu_col_student_{suffix}",
            passwordHash: "integration-hash",
            roleId: studentRole.Id,
            email: $"submenu_col_student_{suffix}@tabsan.local");
        db.Users.AddRange(uniStudentUser, colStudentUser);

        db.StudentProfiles.Add(new StudentProfile(
            uniStudentUser.Id,
            $"SSU-{suffix}",
            requestedProgram.Id,
            requestedDepartment.Id,
            DateTime.UtcNow.Date));

        db.StudentProfiles.Add(new StudentProfile(
            colStudentUser.Id,
            $"SSC-{suffix}",
            scopedProgram.Id,
            scopedDepartment.Id,
            DateTime.UtcNow.Date));

        // Seed both assignments to prove institute-claim filtering still applies.
        db.AdminDepartmentAssignments.Add(new AdminDepartmentAssignment(admin.Id, requestedDepartment.Id));
        db.AdminDepartmentAssignments.Add(new AdminDepartmentAssignment(admin.Id, scopedDepartment.Id));

        await db.SaveChangesAsync();

        return (admin.Id, (int)adminInstitutionType, requestedDepartment.Id, scopedDepartment.Id);
    }
}
