using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

[Collection(EduSphereCollection.Name)]
public class AdminUserManagementIntegrationTests
{
    private readonly EduSphereWebFactory _factory;

    public AdminUserManagementIntegrationTests(EduSphereWebFactory factory)
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
    public async Task AdminUser_List_WithSuperAdminRole_ReturnsSuccess()
    {
        using var client = CreateClient("SuperAdmin");

        var response = await client.GetAsync("api/v1/admin-user");

        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminUser_List_WithAdminRole_ReturnsForbidden()
    {
        using var client = CreateClient("Admin");

        var response = await client.GetAsync("api/v1/admin-user");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AdminUser_CreateUpdate_AndDepartmentAssignmentRoundTrip_Works()
    {
        using var client = CreateClient("SuperAdmin", "00000000-0000-0000-0000-000000000010");

        var username = $"admin_it_{Guid.NewGuid():N}";
        var createPayload = new
        {
            username,
            email = $"{username}@tabsan.local",
            password = "Pass@123"
        };

        var createResponse = await client.PostAsJsonAsync("api/v1/admin-user", createPayload);
        Assert.Equal(HttpStatusCode.OK, createResponse.StatusCode);

        using var createDoc = JsonDocument.Parse(await createResponse.Content.ReadAsStringAsync());
        var adminUserId = createDoc.RootElement.GetProperty("id").GetGuid();

        var deptsResponse = await client.GetAsync("api/v1/department");
        Assert.Equal(HttpStatusCode.OK, deptsResponse.StatusCode);
        using var deptsDoc = JsonDocument.Parse(await deptsResponse.Content.ReadAsStringAsync());
        var departmentItems = deptsDoc.RootElement.EnumerateArray().ToList();
        Guid firstDepartmentId;
        if (departmentItems.Count > 0)
        {
            firstDepartmentId = departmentItems[0].GetProperty("id").GetGuid();
        }
        else
        {
            var suffix = Guid.NewGuid().ToString("N")[..6];
            var createDepartmentResponse = await client.PostAsJsonAsync("api/v1/department", new
            {
                name = $"Integration Dept {suffix}",
                code = $"INT{suffix}"
            });
            Assert.Equal(HttpStatusCode.Created, createDepartmentResponse.StatusCode);

            using var createDeptDoc = JsonDocument.Parse(await createDepartmentResponse.Content.ReadAsStringAsync());
            firstDepartmentId = createDeptDoc.RootElement.GetProperty("id").GetGuid();
        }

        var assignResponse = await client.PostAsJsonAsync("api/v1/department/admin-assignment", new
        {
            adminUserId,
            departmentId = firstDepartmentId
        });
        Assert.Equal(HttpStatusCode.NoContent, assignResponse.StatusCode);

        var listAssignmentsResponse = await client.GetAsync($"api/v1/department/admin-assignment/{adminUserId}");
        Assert.Equal(HttpStatusCode.OK, listAssignmentsResponse.StatusCode);
        using var assignmentsDoc = JsonDocument.Parse(await listAssignmentsResponse.Content.ReadAsStringAsync());
        Assert.Contains(assignmentsDoc.RootElement.EnumerateArray(), x => x.GetProperty("departmentId").GetGuid() == firstDepartmentId);

        var updateResponse = await client.PutAsJsonAsync($"api/v1/admin-user/{adminUserId}", new
        {
            email = $"updated_{username}@tabsan.local",
            isActive = false,
            newPassword = "Pass@1234"
        });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var removeRequest = new HttpRequestMessage(HttpMethod.Delete, "api/v1/department/admin-assignment")
        {
            Content = JsonContent.Create(new { adminUserId, departmentId = firstDepartmentId })
        };
        var removeResponse = await client.SendAsync(removeRequest);
        Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);
    }
}
