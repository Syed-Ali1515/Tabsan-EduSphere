using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class ParentPortalIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public ParentPortalIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string role, string userId = "00000000-0000-0000-0000-000000000001")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(role, userId));
        return client;
    }

    [Fact]
    public async Task MeStudents_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("api/v1/parent-portal/me/students");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MeStudents_StudentRole_Returns403()
    {
        using var client = CreateClient("Student");

        var response = await client.GetAsync("api/v1/parent-portal/me/students");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task MeStudents_ParentRole_Returns200()
    {
        using var client = CreateClient("Parent");

        var response = await client.GetAsync("api/v1/parent-portal/me/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task MeStudents_WithSeededLinkedStudent_ReturnsLinkedStudent()
    {
        var seeded = await SeedLinkedParentStudentAsync();
        using var client = CreateClient("Admin", seeded.ParentUserId.ToString());

        var response = await client.GetAsync("api/v1/parent-portal/me/students");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
        Assert.Contains(
            body.RootElement.EnumerateArray().Select(x => x.GetProperty("studentProfileId").GetGuid()),
            id => id == seeded.StudentProfileId);
    }

    [Fact]
    public async Task LinkedStudentResults_WithSeededLinkedParent_Returns200Array()
    {
        var seeded = await SeedLinkedParentStudentAsync();
        using var client = CreateClient("Admin", seeded.ParentUserId.ToString());

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{seeded.StudentProfileId}/results");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
    }

    [Fact]
    public async Task LinkedStudentAttendance_WithSeededLinkedParent_Returns200Array()
    {
        var seeded = await SeedLinkedParentStudentAsync();
        using var client = CreateClient("Admin", seeded.ParentUserId.ToString());

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{seeded.StudentProfileId}/attendance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
    }

    [Fact]
    public async Task LinkedStudentAnnouncements_WithSeededLinkedParent_Returns200Array()
    {
        var seeded = await SeedLinkedParentStudentAsync();
        using var client = CreateClient("Admin", seeded.ParentUserId.ToString());

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{seeded.StudentProfileId}/announcements");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(JsonValueKind.Array, body.RootElement.ValueKind);
    }

    [Fact]
    public async Task LinkedStudentTimetable_WithSeededLinkedParent_WithoutPublishedTimetable_Returns204()
    {
        var seeded = await SeedLinkedParentStudentAsync();
        using var client = CreateClient("Admin", seeded.ParentUserId.ToString());

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{seeded.StudentProfileId}/timetable");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task LinkedStudentResults_ParentWithoutLink_Returns403()
    {
        using var client = CreateClient("Parent", userId: "00000000-0000-0000-0000-0000000000AA");

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{Guid.NewGuid()}/results");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task LinkedStudentTimetable_ParentWithoutLink_Returns403()
    {
        using var client = CreateClient("Parent", userId: "00000000-0000-0000-0000-0000000000BB");

        var response = await client.GetAsync($"api/v1/parent-portal/me/students/{Guid.NewGuid()}/timetable");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<(Guid ParentUserId, Guid StudentProfileId)> SeedLinkedParentStudentAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var parentRole = db.Roles.First(r => r.Name == "Admin");
        var studentRole = db.Roles.First(r => r.Name == "Student");

        var suffix = Guid.NewGuid().ToString("N")[..6];
        var department = new Department($"Parent Portal Dept {suffix}", $"PPD{suffix}", InstitutionType.School);
        db.Departments.Add(department);

        var program = new AcademicProgram($"Parent Portal Program {suffix}", $"PPP{suffix}", department.Id, 12);
        db.AcademicPrograms.Add(program);

        var parentUser = new User(
            username: $"pp_admin_{suffix}",
            passwordHash: "integration-hash",
            roleId: parentRole.Id,
            email: $"pp_admin_{suffix}@tabsan.local",
            institutionType: InstitutionType.School);
        db.Users.Add(parentUser);

        var studentUser = new User(
            username: $"pp_student_{suffix}",
            passwordHash: "integration-hash",
            roleId: studentRole.Id,
            email: $"pp_student_{suffix}@tabsan.local");
        db.Users.Add(studentUser);

        var studentProfile = new StudentProfile(
            studentUser.Id,
            $"PP-{suffix}",
            program.Id,
            department.Id,
            DateTime.UtcNow.Date);
        db.StudentProfiles.Add(studentProfile);

        db.ParentStudentLinks.Add(new ParentStudentLink(parentUser.Id, studentProfile.Id, "Guardian"));
        await db.SaveChangesAsync();

        return (parentUser.Id, studentProfile.Id);
    }
}