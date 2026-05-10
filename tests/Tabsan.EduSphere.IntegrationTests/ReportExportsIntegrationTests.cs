using System.Net;
using System.Net.Http.Headers;
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
}
