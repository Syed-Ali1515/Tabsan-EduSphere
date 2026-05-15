using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;
using Xunit;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class Phase36Stage4HealthAndLicenseGateTests
{
    private readonly EduSphereWebFactory _factory;

    public Phase36Stage4HealthAndLicenseGateTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicHealthAndMetricsEndpoints_ExposeOperationalSnapshots()
    {
        using var client = _factory.CreateClient();

        using var instanceResponse = await client.GetAsync("health/instance");
        instanceResponse.EnsureSuccessStatusCode();

        using var instanceDoc = JsonDocument.Parse(await instanceResponse.Content.ReadAsStringAsync());
        instanceDoc.RootElement.GetProperty("status").GetString().Should().Be("ok");
        instanceDoc.RootElement.GetProperty("processId").GetInt32().Should().BeGreaterThan(0);
        instanceDoc.RootElement.GetProperty("machine").GetString().Should().NotBeNullOrWhiteSpace();
        instanceDoc.RootElement.GetProperty("uptimeSeconds").GetInt64().Should().BeGreaterThanOrEqualTo(0);

        using var observabilityResponse = await client.GetAsync("health/observability");
        observabilityResponse.EnsureSuccessStatusCode();

        using var observabilityDoc = JsonDocument.Parse(await observabilityResponse.Content.ReadAsStringAsync());
        observabilityDoc.RootElement.GetProperty("totalRequests").GetInt64().Should().BeGreaterThanOrEqualTo(0);
        observabilityDoc.RootElement.GetProperty("errorResponses").GetInt64().Should().BeGreaterThanOrEqualTo(0);
        observabilityDoc.RootElement.GetProperty("sampleCount").GetInt32().Should().BeGreaterThanOrEqualTo(0);

        using var backgroundJobsResponse = await client.GetAsync("health/background-jobs");
        backgroundJobsResponse.EnsureSuccessStatusCode();

        using var backgroundJobsDoc = JsonDocument.Parse(await backgroundJobsResponse.Content.ReadAsStringAsync());
        backgroundJobsDoc.RootElement.GetProperty("reliability").GetProperty("maxRetryAttempts").GetInt32().Should().BeGreaterThan(0);
        backgroundJobsDoc.RootElement.GetProperty("metrics").GetProperty("resultPublish").GetProperty("processed").GetInt64().Should().BeGreaterThanOrEqualTo(0);

        using var metricsResponse = await client.GetAsync("metrics");
        metricsResponse.EnsureSuccessStatusCode();

        var metricsText = await metricsResponse.Content.ReadAsStringAsync();
        metricsText.Should().NotBeNullOrWhiteSpace();
        metricsText.Should().Contain("#");
    }

    [Fact]
    public async Task DashboardContext_ExposesSystemHealth_Widget_ForSuperAdmin()
    {
        using var client = CreateClient("SuperAdmin");

        var response = await client.GetAsync("api/v1/dashboard/context");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var widgets = doc.RootElement.GetProperty("widgets").EnumerateArray().Select(widget => widget.GetProperty("key").GetString()).ToArray();

        widgets.Should().Contain("system_health");
    }

    [Fact]
    public async Task DisabledModule_BlocksSensitiveRoute_With403()
    {
        using var superAdminClient = CreateClient("SuperAdmin");

        var wasActive = await IsModuleActiveAsync("reports");

        try
        {
            if (wasActive)
            {
                await SetModuleActiveStateAsync("reports", isActive: false);
            }

            using var adminClient = CreateClient("Admin");
            var blockedResponse = await adminClient.GetAsync("api/v1/reports");

            blockedResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            var blockedBody = await blockedResponse.Content.ReadAsStringAsync();
            blockedBody.Should().Contain("reports");
            blockedBody.Should().Contain("disabled by license or configuration");
        }
        finally
        {
            if (wasActive)
            {
                await SetModuleActiveStateAsync("reports", isActive: true);
            }
        }
    }

    private async Task<bool> IsModuleActiveAsync(string moduleKey)
    {
        using var scope = _factory.Services.CreateScope();
        var entitlementResolver = scope.ServiceProvider.GetRequiredService<IModuleEntitlementResolver>();
        return await entitlementResolver.IsActiveAsync(moduleKey);
    }

    private async Task SetModuleActiveStateAsync(string moduleKey, bool isActive)
    {
        using var scope = _factory.Services.CreateScope();
        var moduleService = scope.ServiceProvider.GetRequiredService<IModuleService>();
        var changedByUserId = Guid.NewGuid();

        if (isActive)
        {
            await moduleService.ActivateAsync(moduleKey, changedByUserId);
            return;
        }

        await moduleService.DeactivateAsync(moduleKey, changedByUserId);
    }

    private HttpClient CreateClient(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            JwtTestHelper.GenerateToken(role, userId: Guid.NewGuid().ToString()));
        return client;
    }
}