using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;
using Xunit;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class InstitutionPolicyLicenseFlagsIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public InstitutionPolicyLicenseFlagsIntegrationTests(EduSphereWebFactory factory)
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

    private static async Task<(bool IncludeSchool, bool IncludeCollege, bool IncludeUniversity)> GetPolicySnapshotAsync(HttpClient client)
    {
        var response = await client.GetAsync("api/v1/institution-policy");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return (
            doc.RootElement.GetProperty("includeSchool").GetBoolean(),
            doc.RootElement.GetProperty("includeCollege").GetBoolean(),
            doc.RootElement.GetProperty("includeUniversity").GetBoolean());
    }

    private static Task<HttpResponseMessage> SavePolicyAsync(HttpClient client, bool includeSchool, bool includeCollege, bool includeUniversity)
        => client.PutAsJsonAsync("api/v1/institution-policy", new
        {
            includeSchool,
            includeCollege,
            includeUniversity
        });

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Admin")]
    [InlineData("Faculty")]
    [InlineData("Student")]
    public async Task InstitutionPolicy_Get_IsAccessibleToAllAuthenticatedRoles(string role)
    {
        using var client = CreateClient(role);

        var response = await client.GetAsync("api/v1/institution-policy");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Faculty")]
    [InlineData("Student")]
    public async Task InstitutionPolicy_Save_IsForbiddenForNonSuperAdminRoles(string role)
    {
        using var client = CreateClient(role);

        var response = await SavePolicyAsync(client, includeSchool: true, includeCollege: false, includeUniversity: true);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task InstitutionPolicy_Save_WithAllFlagsDisabled_ReturnsBadRequest()
    {
        using var client = CreateClient("SuperAdmin");

        var response = await SavePolicyAsync(client, includeSchool: false, includeCollege: false, includeUniversity: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InstitutionPolicy_Save_WithValidFlags_PersistsAndReturnsNoContent()
    {
        using var superAdminClient = CreateClient("SuperAdmin");
        var original = await GetPolicySnapshotAsync(superAdminClient);

        try
        {
            var saveResponse = await SavePolicyAsync(superAdminClient, includeSchool: true, includeCollege: true, includeUniversity: false);
            Assert.Equal(HttpStatusCode.NoContent, saveResponse.StatusCode);

            var updated = await GetPolicySnapshotAsync(superAdminClient);
            Assert.True(updated.IncludeSchool);
            Assert.True(updated.IncludeCollege);
            Assert.False(updated.IncludeUniversity);
        }
        finally
        {
            var restoreResponse = await SavePolicyAsync(
                superAdminClient,
                original.IncludeSchool,
                original.IncludeCollege,
                original.IncludeUniversity);

            restoreResponse.EnsureSuccessStatusCode();
        }
    }
}
