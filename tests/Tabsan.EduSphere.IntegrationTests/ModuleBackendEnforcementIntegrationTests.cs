using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;
using Xunit;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class ModuleBackendEnforcementIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public ModuleBackendEnforcementIntegrationTests(EduSphereWebFactory factory)
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

    private static async Task<bool> GetModuleStatusAsync(HttpClient client, string moduleKey)
    {
        var response = await client.GetAsync($"api/v1/module/{moduleKey}/status");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("isActive").GetBoolean();
    }

    private static async Task SetModuleStatusAsync(HttpClient client, string moduleKey, bool active)
    {
        var endpoint = active
            ? $"api/v1/module/{moduleKey}/activate"
            : $"api/v1/module/{moduleKey}/deactivate";

        var response = await client.PostAsync(endpoint, content: null);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task DisabledCoursesModule_BlocksCourseApiWithForbidden()
    {
        using var client = CreateClient();
        var moduleKey = "courses";
        var original = await GetModuleStatusAsync(client, moduleKey);

        try
        {
            await SetModuleStatusAsync(client, moduleKey, active: false);

            var response = await client.GetAsync("api/v1/course");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await SetModuleStatusAsync(client, moduleKey, original);
        }
    }

    [Fact]
    public async Task DisabledReportsModule_BlocksReportsCatalogWithForbidden()
    {
        using var client = CreateClient();
        var moduleKey = "reports";
        var original = await GetModuleStatusAsync(client, moduleKey);

        try
        {
            await SetModuleStatusAsync(client, moduleKey, active: false);

            var response = await client.GetAsync("api/v1/reports");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await SetModuleStatusAsync(client, moduleKey, original);
        }
    }

    [Fact]
    public async Task DisabledAiChatModule_BlocksAiEndpointsWithForbidden()
    {
        using var client = CreateClient();
        var moduleKey = "ai_chat";
        var original = await GetModuleStatusAsync(client, moduleKey);

        try
        {
            await SetModuleStatusAsync(client, moduleKey, active: false);

            var response = await client.GetAsync("api/ai/conversations");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await SetModuleStatusAsync(client, moduleKey, original);
        }
    }

    [Fact]
    public async Task DisabledFypModule_BlocksFypEndpointsWithForbidden()
    {
        using var client = CreateClient();
        var moduleKey = "fyp";
        var original = await GetModuleStatusAsync(client, moduleKey);

        try
        {
            await SetModuleStatusAsync(client, moduleKey, active: false);

            var response = await client.GetAsync($"api/v1/fyp/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        finally
        {
            await SetModuleStatusAsync(client, moduleKey, original);
        }
    }

}
