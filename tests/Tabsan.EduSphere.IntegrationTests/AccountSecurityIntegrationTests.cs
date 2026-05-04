using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

/// <summary>
/// Integration tests for the AccountSecurity controller — verifies that the
/// locked-accounts list and lockout-status endpoints enforce authentication
/// and authorization correctly.
/// These tests exercise the live API stack against the integration-test database.
/// </summary>
[Collection(EduSphereCollection.Name)]
public class AccountSecurityIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public AccountSecurityIntegrationTests(EduSphereWebFactory factory)
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

    private HttpClient CreateUnauthenticatedClient() => _factory.CreateClient();

    // ── GET /api/v1/account-security/locked ─────────────────────────────────

    [Fact]
    public async Task GetLocked_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var response = await client.GetAsync("api/v1/account-security/locked");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLocked_StudentRole_Returns403()
    {
        var client = CreateClient("Student");
        var response = await client.GetAsync("api/v1/account-security/locked");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetLocked_AdminRole_ReturnsOk()
    {
        var client = CreateClient("Admin");
        var response = await client.GetAsync("api/v1/account-security/locked");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLocked_SuperAdminRole_ReturnsOk()
    {
        var client = CreateClient("SuperAdmin");
        var response = await client.GetAsync("api/v1/account-security/locked");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetLocked_FreshDatabase_ReturnsEmptyList()
    {
        var client = CreateClient("Admin");
        var response = await client.GetAsync("api/v1/account-security/locked");
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        // Should return an empty array (no locked accounts in fresh seeded DB)
        Assert.Contains("[]", body.Replace(" ", "").Replace("\n", "").Replace("\r", ""));
    }

    // ── GET /api/v1/account-security/{userId}/status ─────────────────────────

    [Fact]
    public async Task GetStatus_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var userId = Guid.NewGuid();
        var response = await client.GetAsync($"api/v1/account-security/{userId}/status");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetStatus_NonExistentUser_Returns404()
    {
        var client = CreateClient("Admin");
        var userId = Guid.NewGuid(); // random non-existent user
        var response = await client.GetAsync($"api/v1/account-security/{userId}/status");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── POST /api/v1/account-security/{userId}/unlock ────────────────────────

    [Fact]
    public async Task Unlock_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var userId = Guid.NewGuid();
        var response = await client.PostAsync($"api/v1/account-security/{userId}/unlock", null);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Unlock_StudentRole_Returns403()
    {
        var client = CreateClient("Student");
        var userId = Guid.NewGuid();
        var response = await client.PostAsync($"api/v1/account-security/{userId}/unlock", null);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ── POST /api/v1/account-security/{userId}/reset-password ────────────────

    [Fact]
    public async Task ResetPassword_Unauthenticated_Returns401()
    {
        var client = CreateUnauthenticatedClient();
        var userId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync(
            $"api/v1/account-security/{userId}/reset-password",
            new { newPassword = "NewPass123!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_FacultyRole_Returns403()
    {
        var client = CreateClient("Faculty");
        var userId = Guid.NewGuid();
        var response = await client.PostAsJsonAsync(
            $"api/v1/account-security/{userId}/reset-password",
            new { newPassword = "NewPass123!" });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
