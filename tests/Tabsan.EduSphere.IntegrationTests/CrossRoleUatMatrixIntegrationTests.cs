using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Tabsan.EduSphere.Domain.Settings;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class CrossRoleUatMatrixIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public CrossRoleUatMatrixIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string role, int institutionType)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateToken(
                role: role,
                userId: Guid.NewGuid().ToString(),
                institutionType: institutionType));
        return client;
    }

    [Theory]
    [InlineData("SuperAdmin", 0)]
    [InlineData("SuperAdmin", 1)]
    [InlineData("SuperAdmin", 2)]
    [InlineData("Admin", 0)]
    [InlineData("Admin", 1)]
    [InlineData("Admin", 2)]
    [InlineData("Faculty", 0)]
    [InlineData("Faculty", 1)]
    [InlineData("Faculty", 2)]
    [InlineData("Student", 0)]
    [InlineData("Student", 1)]
    [InlineData("Student", 2)]
    public async Task ReportCatalog_AcrossRoleAndInstitutionMatrix_ReturnsRoleExpectedVisibility(string role, int institutionType)
    {
        using var client = CreateClient(role, institutionType);

        var response = await client.GetAsync("api/v1/reports");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var keys = doc.RootElement
            .GetProperty("reports")
            .EnumerateArray()
            .Select(x => x.GetProperty("key").GetString() ?? string.Empty)
            .ToList();

        if (string.Equals(role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Contains(ReportKeys.StudentTranscript, keys, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(ReportKeys.AttendanceSummary, keys, StringComparer.OrdinalIgnoreCase);
            Assert.DoesNotContain(ReportKeys.ResultSummary, keys, StringComparer.OrdinalIgnoreCase);
            return;
        }

        Assert.Contains(ReportKeys.AttendanceSummary, keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(ReportKeys.ResultSummary, keys, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(ReportKeys.GpaReport, keys, StringComparer.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("SuperAdmin", 0, HttpStatusCode.OK)]
    [InlineData("SuperAdmin", 1, HttpStatusCode.OK)]
    [InlineData("SuperAdmin", 2, HttpStatusCode.OK)]
    [InlineData("Admin", 0, HttpStatusCode.OK)]
    [InlineData("Admin", 1, HttpStatusCode.OK)]
    [InlineData("Admin", 2, HttpStatusCode.OK)]
    [InlineData("Faculty", 0, HttpStatusCode.Forbidden)]
    [InlineData("Faculty", 1, HttpStatusCode.Forbidden)]
    [InlineData("Faculty", 2, HttpStatusCode.Forbidden)]
    [InlineData("Student", 0, HttpStatusCode.Forbidden)]
    [InlineData("Student", 1, HttpStatusCode.Forbidden)]
    [InlineData("Student", 2, HttpStatusCode.Forbidden)]
    public async Task AccountSecurityLocked_AcrossRoleAndInstitutionMatrix_RespectsRoleBoundaries(
        string role,
        int institutionType,
        HttpStatusCode expectedStatus)
    {
        using var client = CreateClient(role, institutionType);

        var response = await client.GetAsync("api/v1/account-security/locked");

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Theory]
    [InlineData("SuperAdmin", 0)]
    [InlineData("SuperAdmin", 1)]
    [InlineData("SuperAdmin", 2)]
    [InlineData("Admin", 0)]
    [InlineData("Admin", 1)]
    [InlineData("Admin", 2)]
    [InlineData("Faculty", 0)]
    [InlineData("Faculty", 1)]
    [InlineData("Faculty", 2)]
    public async Task AttendanceByOffering_AcrossPrivilegedRolesAndInstitutionMatrix_DeniesOnlyAuthFailures(string role, int institutionType)
    {
        using var client = CreateClient(role, institutionType);

        var response = await client.GetAsync($"api/v1/attendance/by-offering/{Guid.NewGuid()}");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task AttendanceByOffering_StudentAcrossInstitutionMatrix_ReturnsForbidden(int institutionType)
    {
        using var client = CreateClient("Student", institutionType);

        var response = await client.GetAsync($"api/v1/attendance/by-offering/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
