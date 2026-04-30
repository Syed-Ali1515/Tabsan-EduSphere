using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tabsan.EduSphere.IntegrationTests.Infrastructure;

namespace Tabsan.EduSphere.IntegrationTests;

/// <summary>
/// Integration tests that codify the sidebar role-visibility matrix and mutation behaviour.
/// A single <see cref="EduSphereWebFactory"/> is shared across all tests in this class;
/// mutating tests restore the original state in their finally-blocks so later tests
/// always start from the seeded baseline.
/// </summary>
public class SidebarMenuIntegrationTests : IClassFixture<EduSphereWebFactory>
{
    private readonly EduSphereWebFactory _factory;

    public SidebarMenuIntegrationTests(EduSphereWebFactory factory)
    {
        _factory = factory;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private HttpClient CreateClient(string role)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", JwtTestHelper.GenerateToken(role));
        return client;
    }

    private static async Task<List<MenuDto>> GetVisibleAsync(HttpClient client)
    {
        var response = await client.GetAsync("api/v1/sidebar-menu/my-visible");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<MenuDto>>()
               ?? new List<MenuDto>();
    }

    /// <summary>Flattens top-level + sub-menu keys into a single set.</summary>
    private static HashSet<string> FlatKeys(IEnumerable<MenuDto> menus)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in menus)
        {
            keys.Add(m.Key);
            foreach (var s in m.SubMenus)
                keys.Add(s.Key);
        }
        return keys;
    }

    /// <summary>
    /// Flattens top-level + sub-menu items (preserving their full <see cref="MenuDto"/>)
    /// so tests can look up a menu by key regardless of nesting depth.
    /// </summary>
    private static IEnumerable<MenuDto> FlatItems(IEnumerable<MenuDto> menus)
    {
        foreach (var m in menus)
        {
            yield return m;
            foreach (var s in m.SubMenus)
                yield return s;
        }
    }

    // ── Role matrix ───────────────────────────────────────────────────────────

    /// <summary>SuperAdmin should see every seeded menu item (11 keys total).</summary>
    [Fact]
    public async Task GetVisible_SuperAdmin_ReturnsAllMenus()
    {
        using var client = CreateClient("SuperAdmin");
        var menus = await GetVisibleAsync(client);
        var keys  = FlatKeys(menus);

        Assert.Contains("dashboard",        keys);
        Assert.Contains("timetable_admin",  keys);
        Assert.Contains("timetable_teacher",keys);
        Assert.Contains("timetable_student",keys);
        Assert.Contains("lookups",          keys);
        Assert.Contains("buildings",        keys);
        Assert.Contains("rooms",            keys);
        Assert.Contains("system_settings",  keys);
        Assert.Contains("module_settings",  keys);
        Assert.Contains("report_settings",  keys);
        Assert.Contains("sidebar_settings", keys);
        Assert.Equal(11, keys.Count);
    }

    /// <summary>Admin should see: dashboard, timetable_admin, lookups, buildings, rooms.</summary>
    [Fact]
    public async Task GetVisible_Admin_ReturnsAdminMenusOnly()
    {
        using var client = CreateClient("Admin");
        var menus = await GetVisibleAsync(client);
        var keys  = FlatKeys(menus);

        Assert.Contains("dashboard",       keys);
        Assert.Contains("timetable_admin", keys);
        Assert.Contains("lookups",         keys);
        Assert.Contains("buildings",       keys);
        Assert.Contains("rooms",           keys);
        Assert.Equal(5, keys.Count);
    }

    /// <summary>Faculty should see: dashboard, timetable_teacher.</summary>
    [Fact]
    public async Task GetVisible_Faculty_ReturnsFacultyMenusOnly()
    {
        using var client = CreateClient("Faculty");
        var menus = await GetVisibleAsync(client);
        var keys  = FlatKeys(menus);

        Assert.Contains("dashboard",         keys);
        Assert.Contains("timetable_teacher", keys);
        Assert.Equal(2, keys.Count);
    }

    /// <summary>Student should see: dashboard, timetable_student.</summary>
    [Fact]
    public async Task GetVisible_Student_ReturnsStudentMenusOnly()
    {
        using var client = CreateClient("Student");
        var menus = await GetVisibleAsync(client);
        var keys  = FlatKeys(menus);

        Assert.Contains("dashboard",         keys);
        Assert.Contains("timetable_student", keys);
        Assert.Equal(2, keys.Count);
    }

    // ── Status toggle ─────────────────────────────────────────────────────────

    /// <summary>
    /// Disabling timetable_teacher removes it from the Faculty sidebar.
    /// Re-enabling restores it. State is always restored in the finally block.
    /// </summary>
    [Fact]
    public async Task SetStatus_DisableTimetableTeacher_RemovesFromFaculty_ThenRestore()
    {
        using var superClient   = CreateClient("SuperAdmin");
        using var facultyClient = CreateClient("Faculty");

        // Locate the menu item id via the SuperAdmin all-visible endpoint.
        var allMenus  = await GetVisibleAsync(superClient);
        var ttTeacher = FlatItems(allMenus).Single(m => m.Key == "timetable_teacher");

        try
        {
            // ── Disable ──
            var disableResp = await superClient.PutAsJsonAsync(
                $"api/v1/sidebar-menu/{ttTeacher.Id}/status",
                new { isActive = false });
            Assert.Equal(HttpStatusCode.NoContent, disableResp.StatusCode);

            // Faculty should now only see dashboard.
            var afterDisable = FlatKeys(await GetVisibleAsync(facultyClient));
            Assert.DoesNotContain("timetable_teacher", afterDisable);
            Assert.Contains("dashboard", afterDisable);
        }
        finally
        {
            // ── Restore ──
            await superClient.PutAsJsonAsync(
                $"api/v1/sidebar-menu/{ttTeacher.Id}/status",
                new { isActive = true });
        }

        // Confirm Faculty sees timetable_teacher again after restore.
        var afterRestore = FlatKeys(await GetVisibleAsync(facultyClient));
        Assert.Contains("timetable_teacher", afterRestore);
    }

    // ── Role access deny ──────────────────────────────────────────────────────

    /// <summary>
    /// Denying Student access to timetable_student removes it from the Student sidebar.
    /// Re-allowing restores it. State is always restored in the finally block.
    /// </summary>
    [Fact]
    public async Task SetRoles_DenyStudent_RemovesFromStudentVisible_ThenRestore()
    {
        using var superClient   = CreateClient("SuperAdmin");
        using var studentClient = CreateClient("Student");

        var allMenus  = await GetVisibleAsync(superClient);
        var ttStudent = FlatItems(allMenus).Single(m => m.Key == "timetable_student");

        try
        {
            // ── Deny ──
            var denyResp = await superClient.PutAsJsonAsync(
                $"api/v1/sidebar-menu/{ttStudent.Id}/roles",
                new { entries = new[] { new { roleName = "Student", isAllowed = false } } });
            Assert.Equal(HttpStatusCode.NoContent, denyResp.StatusCode);

            // Student should now only see dashboard.
            var afterDeny = FlatKeys(await GetVisibleAsync(studentClient));
            Assert.DoesNotContain("timetable_student", afterDeny);
            Assert.Contains("dashboard", afterDeny);
        }
        finally
        {
            // ── Restore ──
            await superClient.PutAsJsonAsync(
                $"api/v1/sidebar-menu/{ttStudent.Id}/roles",
                new { entries = new[] { new { roleName = "Student", isAllowed = true } } });
        }

        // Confirm Student sees timetable_student again after restore.
        var afterRestore = FlatKeys(await GetVisibleAsync(studentClient));
        Assert.Contains("timetable_student", afterRestore);
    }

    // ── System-menu protection ────────────────────────────────────────────────

    /// <summary>
    /// Attempting to deactivate a system menu (IsSystemMenu = true) must return 409 Conflict.
    /// sidebar_settings is seeded as IsSystemMenu = true.
    /// </summary>
    [Fact]
    public async Task SetStatus_SystemMenu_DeactivateAttempt_Returns409Conflict()
    {
        using var superClient = CreateClient("SuperAdmin");

        var allMenus       = await GetVisibleAsync(superClient);
        var sidebarSettings = FlatItems(allMenus).Single(m => m.Key == "sidebar_settings");

        var response = await superClient.PutAsJsonAsync(
            $"api/v1/sidebar-menu/{sidebarSettings.Id}/status",
            new { isActive = false });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // ── 401 for unauthenticated requests ─────────────────────────────────────

    /// <summary>Unauthenticated requests to my-visible must be rejected with 401.</summary>
    [Fact]
    public async Task GetVisible_NoToken_Returns401()
    {
        using var client   = _factory.CreateClient(); // no auth header
        var response       = await client.GetAsync("api/v1/sidebar-menu/my-visible");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ── Local DTOs (mirrors API response shape — case-insensitive deserialization) ──

    private sealed class MenuDto
    {
        public Guid         Id          { get; set; }
        public string       Key         { get; set; } = string.Empty;
        public string       Name        { get; set; } = string.Empty;
        public bool         IsActive    { get; set; }
        public bool         IsSystemMenu{ get; set; }
        public List<MenuDto>        SubMenus     { get; set; } = new();
        public List<RoleAccessDto>  RoleAccesses { get; set; } = new();
    }

    private sealed class RoleAccessDto
    {
        public string RoleName  { get; set; } = string.Empty;
        public bool   IsAllowed { get; set; }
    }
}
