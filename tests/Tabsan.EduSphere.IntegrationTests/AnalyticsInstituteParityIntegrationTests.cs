using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Assignments;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class AnalyticsInstituteParityIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public AnalyticsInstituteParityIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnalyticsAssignments_WithMismatchedInstitutionQueryForAdminClaim_ReturnsForbidden()
    {
        using var client = CreateAdminClient(institutionType: (int)InstitutionType.College);

        var response = await client.GetAsync("api/analytics/assignments?institutionType=0");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AnalyticsAssignments_WithoutFilters_AutoScopesToAdminInstitutionClaim()
    {
        var seeded = await SeedAssignmentAnalyticsAcrossInstitutesAsync();

        using var client = CreateAdminClient(institutionType: (int)InstitutionType.College);

        var response = await client.GetAsync("api/analytics/assignments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var assignments = doc.RootElement
            .GetProperty("assignments")
            .EnumerateArray()
            .Select(x => x.GetProperty("title").GetString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        Assert.Contains(seeded.CollegeAssignmentTitle, assignments);
        Assert.DoesNotContain(seeded.UniversityAssignmentTitle, assignments);
    }

    private HttpClient CreateAdminClient(int institutionType)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateToken(
                role: "Admin",
                userId: Guid.NewGuid().ToString(),
                institutionType: institutionType));
        return client;
    }

    private async Task<(string CollegeAssignmentTitle, string UniversityAssignmentTitle)> SeedAssignmentAnalyticsAcrossInstitutesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var suffix = Guid.NewGuid().ToString("N")[..6];

        var collegeDepartment = new Department($"Analytics College {suffix}", $"AC{suffix}", InstitutionType.College);
        var universityDepartment = new Department($"Analytics University {suffix}", $"AU{suffix}", InstitutionType.University);
        db.Departments.AddRange(collegeDepartment, universityDepartment);

        var semester = new Semester($"Analytics Sem {suffix}", DateTime.UtcNow.Date.AddDays(-15), DateTime.UtcNow.Date.AddDays(75));
        db.Semesters.Add(semester);

        var collegeCourse = new Course($"Analytics College Course {suffix}", $"ACC{suffix}", 3, collegeDepartment.Id);
        var universityCourse = new Course($"Analytics University Course {suffix}", $"AUC{suffix}", 3, universityDepartment.Id);
        db.Courses.AddRange(collegeCourse, universityCourse);

        var collegeOffering = new CourseOffering(collegeCourse.Id, semester.Id, 50);
        var universityOffering = new CourseOffering(universityCourse.Id, semester.Id, 50);
        db.CourseOfferings.AddRange(collegeOffering, universityOffering);

        var collegeAssignmentTitle = $"College Assignment {suffix}";
        var universityAssignmentTitle = $"University Assignment {suffix}";
        db.Assignments.Add(new Assignment(collegeOffering.Id, collegeAssignmentTitle, "seed", DateTime.UtcNow.Date.AddDays(10), 100));
        db.Assignments.Add(new Assignment(universityOffering.Id, universityAssignmentTitle, "seed", DateTime.UtcNow.Date.AddDays(10), 100));

        await db.SaveChangesAsync();

        return (collegeAssignmentTitle, universityAssignmentTitle);
    }
}