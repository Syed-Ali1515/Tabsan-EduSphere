using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;
using FluentAssertions;
using System.Net;
using Tabsan.EduSphere.Domain.Enums;
using Xunit;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class DynamicLabelIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public DynamicLabelIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string role = "SuperAdmin", string userId = "00000000-0000-0000-0000-000000000001")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(role, userId));
        return client;
    }

    private HttpClient CreateUnauthenticatedClient()
    {
        return _factory.CreateClient();
    }

    private async Task EnableInstitutionTypesAsync(HttpClient client, bool university = true, bool school = false, bool college = false)
    {
        var response = await client.PutAsJsonAsync("api/v1/institution-policy", new
        {
            includeSchool = school,
            includeCollege = college,
            includeUniversity = university
        });
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Verifies GET /api/v1/labels returns University vocabulary when only University is enabled.
    /// </summary>
    [Fact]
    public async Task GetLabels_University_ReturnsSemesterGpaCourseTerminology()
    {
        // Setup: Policy with University enabled
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: true, school: false, college: false);

        // Act
        var response = await client.GetAsync("api/v1/labels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vocab = doc.RootElement;

        // Assert: University vocabulary
        vocab.GetProperty("periodLabel").GetString().Should().Be("Semester");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("GPA/CGPA");
        vocab.GetProperty("courseLabel").GetString().Should().Be("Course");
        vocab.GetProperty("progressionLabel").GetString().Should().Be("Progression");
        vocab.GetProperty("studentGroupLabel").GetString().Should().Be("Batch");
    }

    /// <summary>
    /// Verifies GET /api/v1/labels returns School vocabulary when only School is enabled.
    /// </summary>
    [Fact]
    public async Task GetLabels_School_ReturnsGradePercentageSubjectTerminology()
    {
        // Setup: Policy with School enabled
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: false, school: true, college: false);

        // Act
        var response = await client.GetAsync("api/v1/labels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vocab = doc.RootElement;

        // Assert: School vocabulary
        vocab.GetProperty("periodLabel").GetString().Should().Be("Grade");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("Percentage");
        vocab.GetProperty("courseLabel").GetString().Should().Be("Subject");
        vocab.GetProperty("progressionLabel").GetString().Should().Be("Promotion");
        vocab.GetProperty("studentGroupLabel").GetString().Should().Be("Class");
    }

    /// <summary>
    /// Verifies GET /api/v1/labels returns College vocabulary when only College is enabled.
    /// </summary>
    [Fact]
    public async Task GetLabels_College_ReturnsYearPercentageSubjectTerminology()
    {
        // Setup: Policy with College enabled
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: false, school: false, college: true);

        // Act
        var response = await client.GetAsync("api/v1/labels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vocab = doc.RootElement;

        // Assert: College vocabulary
        vocab.GetProperty("periodLabel").GetString().Should().Be("Year");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("Percentage");
        vocab.GetProperty("courseLabel").GetString().Should().Be("Subject");
        vocab.GetProperty("progressionLabel").GetString().Should().Be("Progression");
        vocab.GetProperty("studentGroupLabel").GetString().Should().Be("Year-Group");
    }

    /// <summary>
    /// Verifies GET /api/v1/labels prefers University vocabulary when University is enabled
    /// alongside School and College (common-denominator behavior).
    /// </summary>
    [Fact]
    public async Task GetLabels_MixedWithUniversity_PrefersUniversityVocabulary()
    {
        // Setup: Policy with all types enabled
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: true, school: true, college: true);

        // Act
        var response = await client.GetAsync("api/v1/labels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vocab = doc.RootElement;

        // Assert: University vocabulary (common denominator when mixed)
        vocab.GetProperty("periodLabel").GetString().Should().Be("Semester");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("GPA/CGPA");
    }

    /// <summary>
    /// Verifies that the DashboardComposition endpoint includes vocabulary in its response.
    /// This ensures the web layer can consume labels for dashboard rendering.
    /// </summary>
    [Fact]
    public async Task DashboardComposition_IncludesVocabularyForWebLayerConsumption()
    {
        // Setup: Policy with School enabled
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: false, school: true, college: false);

        // Act
        var response = await client.GetAsync("api/v1/dashboard/context");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var context = doc.RootElement;

        // Assert: Vocabulary is included in dashboard context
        var vocab = context.GetProperty("vocabulary");
        vocab.GetProperty("periodLabel").GetString().Should().Be("Grade");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("Percentage");
    }

    /// <summary>
    /// Verifies that unauthenticated requests to /api/v1/labels are denied.
    /// Labels are institution-scoped and require authentication context.
    /// </summary>
    [Fact]
    public async Task GetLabels_Unauthenticated_Returns401()
    {
        // Act: Request without authentication
        using var unauthClient = CreateUnauthenticatedClient();
        var response = await unauthClient.GetAsync("api/v1/labels");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// Verifies that multiple consecutive requests return consistent vocabulary
    /// (no caching inconsistencies or policy changes mid-session).
    /// </summary>
    [Fact]
    public async Task GetLabels_MultipleRequests_ReturnConsistentVocabulary()
    {
        // Setup
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: false, school: false, college: true);

        // Act: Multiple requests
        var response1 = await client.GetAsync("api/v1/labels");
        using var doc1 = JsonDocument.Parse(await response1.Content.ReadAsStringAsync());
        var vocab1 = doc1.RootElement;

        var response2 = await client.GetAsync("api/v1/labels");
        using var doc2 = JsonDocument.Parse(await response2.Content.ReadAsStringAsync());
        var vocab2 = doc2.RootElement;

        // Assert: Consistent results
        vocab1.GetProperty("periodLabel").GetString().Should().Be("Year");
        vocab2.GetProperty("periodLabel").GetString().Should().Be("Year");
    }

    /// <summary>
    /// Verifies that vocabulary labels use the common-denominator policy behavior:
    /// When School and College are both enabled (without University), School vocab takes precedence.
    /// Labels are policy-based (tenant-wide), not caller-based.
    /// </summary>
    [Fact]
    public async Task GetLabels_SchoolAndCollege_PrefersSchoolVocabulary()
    {
        // Setup: School and College enabled (but not University)
        using var client = CreateClient();
        await EnableInstitutionTypesAsync(client, university: false, school: true, college: true);

        // Act
        var response = await client.GetAsync("api/v1/labels");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var vocab = doc.RootElement;

        // Assert: School vocabulary is returned (first in precedence)
        vocab.GetProperty("periodLabel").GetString().Should().Be("Grade");
        vocab.GetProperty("gradingLabel").GetString().Should().Be("Percentage");
        vocab.GetProperty("studentGroupLabel").GetString().Should().Be("Class");
    }
}
