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
public class ReportExportsIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public ReportExportsIntegrationTests(EduSphereWebFactory factory)
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
    public async Task AttendanceSummary_Export_Unauthenticated_Returns401()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("api/v1/reports/attendance-summary/export");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("api/v1/reports/attendance-summary/export", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance-summary.xlsx")]
    [InlineData("api/v1/reports/attendance-summary/export/csv", "text/csv", "attendance-summary.csv")]
    [InlineData("api/v1/reports/attendance-summary/export/pdf", "application/pdf", "attendance-summary.pdf")]
    [InlineData("api/v1/reports/result-summary/export", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "result-summary.xlsx")]
    [InlineData("api/v1/reports/result-summary/export/csv", "text/csv", "result-summary.csv")]
    [InlineData("api/v1/reports/result-summary/export/pdf", "application/pdf", "result-summary.pdf")]
    [InlineData("api/v1/reports/assignment-summary/export", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "assignment-summary.xlsx")]
    [InlineData("api/v1/reports/assignment-summary/export/csv", "text/csv", "assignment-summary.csv")]
    [InlineData("api/v1/reports/assignment-summary/export/pdf", "application/pdf", "assignment-summary.pdf")]
    [InlineData("api/v1/reports/quiz-summary/export", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "quiz-summary.xlsx")]
    [InlineData("api/v1/reports/quiz-summary/export/csv", "text/csv", "quiz-summary.csv")]
    [InlineData("api/v1/reports/quiz-summary/export/pdf", "application/pdf", "quiz-summary.pdf")]
    public async Task ReportExports_WithSuperAdmin_ReturnExpectedFileMetadata(string route, string expectedContentType, string expectedFileName)
    {
        // Final-Touches Phase 32 Stage 32.2 — export endpoint guardrails for content-type and filename contracts.
        using var client = CreateClient("SuperAdmin", "00000000-0000-0000-0000-000000000099");

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedContentType, response.Content.Headers.ContentType?.MediaType);

        var disposition = response.Content.Headers.ContentDisposition?.ToString() ?? string.Empty;
        Assert.Contains(expectedFileName, disposition, StringComparison.OrdinalIgnoreCase);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public async Task EnrollmentSummary_WithAdminInstitutionMismatch_ReturnsForbidden()
    {
        Guid adminUserId;
        Guid departmentId;
        int mismatchedInstitutionType;

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var adminRole = db.Roles.First(r => r.Name == "Admin");

            var department = db.Departments.FirstOrDefault();
            if (department is null)
            {
                var suffix = Guid.NewGuid().ToString("N")[..6];
                department = new Department($"Report Scope Dept {suffix}", $"RSD{suffix}", InstitutionType.University);
                db.Departments.Add(department);
                await db.SaveChangesAsync();
            }

            departmentId = department.Id;
            mismatchedInstitutionType = (((int)department.InstitutionType) + 1) % 3;

            var admin = new User(
                username: $"report_admin_{Guid.NewGuid():N}",
                passwordHash: "integration-hash",
                roleId: adminRole.Id,
                email: $"report_admin_{Guid.NewGuid():N}@tabsan.local",
                departmentId: null,
                mustChangePassword: false,
                institutionType: (InstitutionType)mismatchedInstitutionType);

            db.Users.Add(admin);
            await db.SaveChangesAsync();

            db.AdminDepartmentAssignments.Add(new AdminDepartmentAssignment(admin.Id, departmentId));
            await db.SaveChangesAsync();

            adminUserId = admin.Id;
        }

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(
                role: "Admin",
                userId: adminUserId.ToString(),
                institutionType: mismatchedInstitutionType));

        var response = await client.GetAsync($"api/v1/reports/enrollment-summary?departmentId={departmentId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
