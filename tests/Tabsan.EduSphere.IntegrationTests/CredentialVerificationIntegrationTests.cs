using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Infrastructure.Persistence;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class CredentialVerificationIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public CredentialVerificationIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient() => _factory.CreateClient();

    private async Task EnsureUserAsync(string username, string password, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var roleEntity = await db.Roles.SingleAsync(r => r.Name == role);
        var existing = await db.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (existing is null)
        {
            // Final-Touches Phase 32 Stage 32.5 — seed deterministic auth-smoke users for credential verification.
            db.Users.Add(new User(
                username,
                hasher.Hash(password),
                roleEntity.Id,
                email: $"{username}@integration.local",
                departmentId: null,
                mustChangePassword: false));
        }
        else
        {
            existing.UpdatePasswordHash(hasher.Hash(password));
            existing.Activate();
            existing.UpdateEmail($"{username}@integration.local");
        }

        await db.SaveChangesAsync();
    }

    [Theory]
    [InlineData("cred.superadmin", "CredPass#2026", "SuperAdmin")]
    [InlineData("cred.admin", "CredPass#2026", "Admin")]
    [InlineData("cred.faculty", "CredPass#2026", "Faculty")]
    [InlineData("cred.student", "CredPass#2026", "Student")]
    public async Task Login_WithVerifiedCredentials_ReturnsTokenAndExpectedRole(string username, string password, string expectedRole)
    {
        await EnsureUserAsync(username, password, expectedRole);

        using var client = CreateClient();

        var response = await client.PostAsJsonAsync("api/v1/auth/login", new
        {
            username,
            password
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var accessToken = ReadString(json.RootElement, "accessToken");
        var role = ReadString(json.RootElement, "role");

        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.Equal(expectedRole, role);
    }

    private static string ReadString(JsonElement root, string name)
    {
        return root.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? (p.GetString() ?? string.Empty)
            : string.Empty;
    }
}
