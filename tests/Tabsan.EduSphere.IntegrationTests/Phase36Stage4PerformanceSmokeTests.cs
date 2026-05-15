using System.Diagnostics;
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
public class Phase36Stage4PerformanceSmokeTests
{
    private readonly EduSphereWebFactory _factory;

    public Phase36Stage4PerformanceSmokeTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CriticalPortalAndApiPaths_RemainWithinSmokeLatencyBudget()
    {
        using var client = CreateClient("Admin");

        var restoredModuleStates = new List<(string ModuleKey, bool WasActive)>();

        try
        {
            await EnsureModuleActiveAsync(restoredModuleStates, "reports");
            await EnsureModuleActiveAsync(restoredModuleStates, "attendance");

            var targets = new[]
            {
                "api/analytics/assignments",
                "api/v1/reports",
                "api/v1/attendance/below-threshold"
            };

            foreach (var target in targets)
            {
                using var warmup = await client.GetAsync(target);
                warmup.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
                warmup.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);

                var elapsed = new List<long>(capacity: 4);
                for (var i = 0; i < 4; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    using var response = await client.GetAsync(target);
                    stopwatch.Stop();

                    response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
                    response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
                    ((int)response.StatusCode).Should().BeLessThan(500, because: $"{target} should remain reachable during smoke validation");

                    elapsed.Add(stopwatch.ElapsedMilliseconds);
                }

                elapsed.Average().Should().BeLessThan(2500, because: $"average latency for {target} should remain in smoke budget");
                elapsed.Max().Should().BeLessThan(6000, because: $"peak latency for {target} should remain in smoke budget");
            }
        }
        finally
        {
            await RestoreModuleStatesAsync(restoredModuleStates);
        }
    }

    private async Task EnsureModuleActiveAsync(List<(string ModuleKey, bool WasActive)> restoredModuleStates, string moduleKey)
    {
        var isActive = await IsModuleActiveAsync(moduleKey);
        restoredModuleStates.Add((moduleKey, isActive));

        if (!isActive)
        {
            await SetModuleActiveStateAsync(moduleKey, isActive: true);
        }
    }

    private async Task RestoreModuleStatesAsync(List<(string ModuleKey, bool WasActive)> restoredModuleStates)
    {
        foreach (var (moduleKey, wasActive) in restoredModuleStates)
        {
            var isActive = await IsModuleActiveAsync(moduleKey);

            if (wasActive && !isActive)
            {
                await SetModuleActiveStateAsync(moduleKey, isActive: true);
            }

            if (!wasActive && isActive)
            {
                await SetModuleActiveStateAsync(moduleKey, isActive: false);
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