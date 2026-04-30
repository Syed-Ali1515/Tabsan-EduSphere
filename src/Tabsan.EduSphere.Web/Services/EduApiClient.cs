using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Tabsan.EduSphere.Web.Models.Portal;

namespace Tabsan.EduSphere.Web.Services;

public interface IEduApiClient
{
    bool IsConnected();
    ApiConnectionModel GetConnection();
    void SaveConnection(ApiConnectionModel model);
    SessionIdentity? GetSessionIdentity();

    Task<List<LookupItem>> GetDepartmentsAsync(CancellationToken ct);
    Task<List<LookupItem>> GetProgramsAsync(Guid? departmentId, CancellationToken ct);
    Task<List<LookupItem>> GetSemestersAsync(CancellationToken ct);
    Task<List<LookupItem>> GetCoursesAsync(Guid? departmentId, CancellationToken ct);
    Task<List<FacultyLookupItem>> GetFacultyAsync(CancellationToken ct);
    Task<List<LookupItem>> GetBuildingsAsync(CancellationToken ct);
    Task<List<RoomLookupItem>> GetRoomsAsync(CancellationToken ct);
    Task<List<RoomLookupItem>> GetRoomsByBuildingAsync(Guid buildingId, CancellationToken ct);

    Task<TimetableDetailsItem?> GetTimetableByIdAsync(Guid timetableId, CancellationToken ct);
    Task<List<TimetableSummaryItem>> GetTimetablesByDepartmentAsync(Guid departmentId, CancellationToken ct);
    Task<Guid> CreateTimetableAsync(CreateTimetableForm form, CancellationToken ct);
    Task AddTimetableEntryAsync(AddTimetableEntryForm form, CancellationToken ct);
    Task PublishTimetableAsync(Guid timetableId, CancellationToken ct);
    Task<List<TeacherTimetableEntryItem>> GetTeacherEntriesAsync(CancellationToken ct);

    // Buildings
    Task<List<BuildingItem>> GetAllBuildingsAsync(bool activeOnly, CancellationToken ct);
    Task<BuildingItem?> GetBuildingByIdAsync(Guid id, CancellationToken ct);
    Task<BuildingItem> CreateBuildingAsync(BuildingFormModel form, CancellationToken ct);
    Task<BuildingItem> UpdateBuildingAsync(Guid id, BuildingFormModel form, CancellationToken ct);
    Task ActivateBuildingAsync(Guid id, CancellationToken ct);
    Task DeactivateBuildingAsync(Guid id, CancellationToken ct);

    // Rooms
    Task<List<RoomItem>> GetAllRoomsAsync(bool activeOnly, CancellationToken ct);
    Task<List<RoomItem>> GetRoomsForBuildingAsync(Guid buildingId, bool activeOnly, CancellationToken ct);
    Task<RoomItem> CreateRoomAsync(RoomFormModel form, CancellationToken ct);
    Task<RoomItem> UpdateRoomAsync(Guid id, RoomFormModel form, CancellationToken ct);
    Task ActivateRoomAsync(Guid id, CancellationToken ct);
    Task DeactivateRoomAsync(Guid id, CancellationToken ct);

    // Sidebar Settings
    Task<List<SidebarMenuItemWebModel>> GetSidebarMenusAsync(CancellationToken ct);
    Task<List<SidebarMenuItemWebModel>> GetVisibleSidebarMenusForCurrentUserAsync(CancellationToken ct);
    Task SetSidebarMenuRolesAsync(Guid id, Dictionary<string, bool> roles, CancellationToken ct);
    Task SetSidebarMenuStatusAsync(Guid id, bool isActive, CancellationToken ct);

    // License
    Task<LicenseUpdatePageModel> GetLicenseDetailsAsync(CancellationToken ct);
    Task<string> UploadLicenseAsync(Stream fileStream, string fileName, CancellationToken ct);

    // Theme
    Task<string?> GetCurrentThemeAsync(CancellationToken ct);
    Task SetThemeAsync(string? themeKey, CancellationToken ct);

    // Report Settings
    Task<List<ReportDefinitionWebModel>> GetReportDefinitionsAsync(CancellationToken ct);
    Task CreateReportDefinitionAsync(CreateReportForm form, CancellationToken ct);
    Task SetReportActiveAsync(Guid id, bool activate, CancellationToken ct);
    Task SetReportRolesAsync(Guid id, List<string> roles, CancellationToken ct);

    // Module Settings
    Task<List<ModuleSettingsWebModel>> GetModuleSettingsAsync(CancellationToken ct);
    Task SetModuleActiveAsync(string key, bool activate, CancellationToken ct);
    Task SetModuleRolesAsync(string key, List<string> roles, CancellationToken ct);
}

public class EduApiClient : IEduApiClient
{
    private const string ApiUrlKey    = "ApiBaseUrl";
    private const string ApiTokenKey  = "ApiAccessToken";
    private const string DepartmentKey = "DefaultDepartmentId";
    private const string IdentityKey  = "SessionIdentityJson";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EduApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    // â”€â”€ Connection â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public bool IsConnected()
    {
        var c = GetConnection();
        return !string.IsNullOrWhiteSpace(c.ApiBaseUrl) && !string.IsNullOrWhiteSpace(c.AccessToken);
    }

    public ApiConnectionModel GetConnection()
    {
        var session = GetSession();
        var baseUrl = session.GetString(ApiUrlKey) ?? string.Empty;
        var token   = session.GetString(ApiTokenKey) ?? string.Empty;
        var rawDept = session.GetString(DepartmentKey);
        Guid? dept = Guid.TryParse(rawDept, out var parsed) ? parsed : null;

        return new ApiConnectionModel
        {
            ApiBaseUrl = baseUrl,
            AccessToken = token,
            DefaultDepartmentId = dept
        };
    }

    public void SaveConnection(ApiConnectionModel model)
    {
        var session = GetSession();
        session.SetString(ApiUrlKey,   model.ApiBaseUrl.TrimEnd('/'));
        session.SetString(ApiTokenKey, model.AccessToken.Trim());

        if (model.DefaultDepartmentId.HasValue)
            session.SetString(DepartmentKey, model.DefaultDepartmentId.Value.ToString());
        else
            session.Remove(DepartmentKey);

        // Decode JWT and persist identity claims into session
        var identity = DecodeJwtIdentity(model.AccessToken.Trim());
        var json = JsonSerializer.Serialize(identity, _jsonOptions);
        session.SetString(IdentityKey, json);
    }

    public SessionIdentity? GetSessionIdentity()
    {
        var raw = GetSession().GetString(IdentityKey);
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try { return JsonSerializer.Deserialize<SessionIdentity>(raw, _jsonOptions); }
        catch { return null; }
    }

    // â”€â”€ Lookup GETs â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<LookupItem>> GetDepartmentsAsync(CancellationToken ct)
        => await GetAsync<List<LookupItem>>("api/v1/department", ct) ?? new();

    public async Task<List<LookupItem>> GetProgramsAsync(Guid? departmentId, CancellationToken ct)
    {
        var path = departmentId.HasValue
            ? $"api/v1/program?departmentId={departmentId.Value}"
            : "api/v1/program";
        return await GetAsync<List<LookupItem>>(path, ct) ?? new();
    }

    public async Task<List<LookupItem>> GetSemestersAsync(CancellationToken ct)
        => await GetAsync<List<LookupItem>>("api/v1/semester", ct) ?? new();

    public async Task<List<LookupItem>> GetCoursesAsync(Guid? departmentId, CancellationToken ct)
    {
        var path = departmentId.HasValue
            ? $"api/v1/course?departmentId={departmentId.Value}"
            : "api/v1/course";
        return await GetAsync<List<LookupItem>>(path, ct) ?? new();
    }

    public async Task<List<FacultyLookupItem>> GetFacultyAsync(CancellationToken ct)
        => await GetAsync<List<FacultyLookupItem>>("api/v1/timetable/faculty", ct) ?? new();

    public async Task<List<LookupItem>> GetBuildingsAsync(CancellationToken ct)
        => await GetAsync<List<LookupItem>>("api/v1/building", ct) ?? new();

    public async Task<List<RoomLookupItem>> GetRoomsAsync(CancellationToken ct)
        => await GetAsync<List<RoomLookupItem>>("api/v1/room", ct) ?? new();

    public async Task<List<RoomLookupItem>> GetRoomsByBuildingAsync(Guid buildingId, CancellationToken ct)
        => await GetAsync<List<RoomLookupItem>>($"api/v1/room/building/{buildingId}", ct) ?? new();

    // â”€â”€ Timetable â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public Task<TimetableDetailsItem?> GetTimetableByIdAsync(Guid timetableId, CancellationToken ct)
        => GetAsync<TimetableDetailsItem>($"api/v1/timetable/{timetableId}", ct);

    public async Task<List<TimetableSummaryItem>> GetTimetablesByDepartmentAsync(Guid departmentId, CancellationToken ct)
        => await GetAsync<List<TimetableSummaryItem>>($"api/v1/timetable/department/{departmentId}", ct) ?? new();

    public async Task<Guid> CreateTimetableAsync(CreateTimetableForm form, CancellationToken ct)
    {
        var result = await PostAsync<CreateTimetableForm, TimetableCreateResponse>("api/v1/timetable", form, ct)
            ?? throw new InvalidOperationException("Timetable create API returned no body.");
        return result.Id;
    }

    public async Task AddTimetableEntryAsync(AddTimetableEntryForm form, CancellationToken ct)
    {
        var payload = new
        {
            form.DayOfWeek,
            form.StartTime,
            form.EndTime,
            form.SubjectName,
            form.CourseId,
            form.FacultyUserId,
            form.FacultyName,
            form.RoomId,
            form.RoomNumber,
            form.BuildingId
        };
        await PostAsync<object, object>($"api/v1/timetable/{form.TimetableId}/entries", payload, ct);
    }

    public async Task PublishTimetableAsync(Guid timetableId, CancellationToken ct)
        => await PostAsync<object, object>($"api/v1/timetable/{timetableId}/publish", new { }, ct);

    public async Task<List<TeacherTimetableEntryItem>> GetTeacherEntriesAsync(CancellationToken ct)
        => await GetAsync<List<TeacherTimetableEntryItem>>("api/v1/timetable/mine/teacher", ct) ?? new();

    // â”€â”€ Buildings â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<BuildingItem>> GetAllBuildingsAsync(bool activeOnly, CancellationToken ct)
        => await GetAsync<List<BuildingItem>>($"api/v1/building?activeOnly={activeOnly}", ct) ?? new();

    public Task<BuildingItem?> GetBuildingByIdAsync(Guid id, CancellationToken ct)
        => GetAsync<BuildingItem>($"api/v1/building/{id}", ct);

    public async Task<BuildingItem> CreateBuildingAsync(BuildingFormModel form, CancellationToken ct)
        => await PostAsync<BuildingFormModel, BuildingItem>("api/v1/building", form, ct)
           ?? throw new InvalidOperationException("Building create returned no body.");

    public async Task<BuildingItem> UpdateBuildingAsync(Guid id, BuildingFormModel form, CancellationToken ct)
        => await PutAsync<BuildingFormModel, BuildingItem>($"api/v1/building/{id}", form, ct)
           ?? throw new InvalidOperationException("Building update returned no body.");

    public Task ActivateBuildingAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/building/{id}/activate", new { }, ct);

    public Task DeactivateBuildingAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/building/{id}/deactivate", new { }, ct);

    // â”€â”€ Rooms â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public async Task<List<RoomItem>> GetAllRoomsAsync(bool activeOnly, CancellationToken ct)
        => await GetAsync<List<RoomItem>>($"api/v1/room?activeOnly={activeOnly}", ct) ?? new();

    public async Task<List<RoomItem>> GetRoomsForBuildingAsync(Guid buildingId, bool activeOnly, CancellationToken ct)
        => await GetAsync<List<RoomItem>>($"api/v1/room/building/{buildingId}?activeOnly={activeOnly}", ct) ?? new();

    public async Task<RoomItem> CreateRoomAsync(RoomFormModel form, CancellationToken ct)
        => await PostAsync<RoomFormModel, RoomItem>("api/v1/room", form, ct)
           ?? throw new InvalidOperationException("Room create returned no body.");

    public async Task<RoomItem> UpdateRoomAsync(Guid id, RoomFormModel form, CancellationToken ct)
        => await PutAsync<RoomFormModel, RoomItem>($"api/v1/room/{id}", form, ct)
           ?? throw new InvalidOperationException("Room update returned no body.");

    public Task ActivateRoomAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/room/{id}/activate", new { }, ct);

    public Task DeactivateRoomAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/room/{id}/deactivate", new { }, ct);

    // â”€â”€ HTTP helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private async Task<T?> GetAsync<T>(string path, CancellationToken ct)
    {
        using var request  = CreateRequest(HttpMethod.Get, path);
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);
        return string.IsNullOrWhiteSpace(body) ? default : JsonSerializer.Deserialize<T>(body, _jsonOptions);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(string path, TRequest payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        using var request = CreateRequest(HttpMethod.Post, path);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);
        return string.IsNullOrWhiteSpace(body) ? default : JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
    }

    private async Task<TResponse?> PutAsync<TRequest, TResponse>(string path, TRequest payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        using var request = CreateRequest(HttpMethod.Put, path);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);
        return string.IsNullOrWhiteSpace(body) ? default : JsonSerializer.Deserialize<TResponse>(body, _jsonOptions);
    }

    private HttpClient CreateClient() => _httpClientFactory.CreateClient("EduApi");

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var connection = GetConnection();
        if (string.IsNullOrWhiteSpace(connection.ApiBaseUrl) || string.IsNullOrWhiteSpace(connection.AccessToken))
            throw new InvalidOperationException("API connection is not configured. Set base URL and access token first.");

        var uri     = new Uri(new Uri(connection.ApiBaseUrl.TrimEnd('/') + "/"), path.TrimStart('/'));
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", connection.AccessToken);
        return request;
    }

    private static Exception BuildException(System.Net.HttpStatusCode statusCode, string body)
    {
        var message = string.IsNullOrWhiteSpace(body)
            ? $"API request failed with status {(int)statusCode}."
            : body;
        return new InvalidOperationException(message);
    }

    private ISession GetSession()
        => _httpContextAccessor.HttpContext?.Session
           ?? throw new InvalidOperationException("No active HTTP session found.");

    // â”€â”€ JWT identity decoding (no signature validation â€” display use only) â”€

    private static SessionIdentity DecodeJwtIdentity(string token)
    {
        var identity = new SessionIdentity();
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return identity;

            var payload = parts[1];
            // Pad base64 string
            payload = payload.Replace('-', '+').Replace('_', '/');
            payload += (payload.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.TryGetProperty("unique_name", out var un)) identity.UserName = un.GetString();
            else if (root.TryGetProperty("sub", out var sub)) identity.UserName = sub.GetString();

            if (root.TryGetProperty("email", out var em)) identity.Email = em.GetString();

            // Role claim may be a string or an array
            if (root.TryGetProperty("role", out var roleProp))
            {
                if (roleProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var r in roleProp.EnumerateArray())
                        if (r.GetString() is string s) identity.Roles.Add(s);
                }
                else if (roleProp.GetString() is string singleRole)
                    identity.Roles.Add(singleRole);
            }
        }
        catch { /* ignore decode errors â€“ identity stays default */ }

        return identity;
    }

    private sealed class TimetableCreateResponse { public Guid Id { get; set; } }

    // ── Sidebar Settings ──────────────────────────────────────────────────────

    public async Task<List<SidebarMenuItemWebModel>> GetSidebarMenusAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<SidebarMenuApiDto>>("api/v1/sidebar-menu", ct) ?? new();
        return raw.Select(MapSidebarItem).ToList();
    }

    public async Task<List<SidebarMenuItemWebModel>> GetVisibleSidebarMenusForCurrentUserAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<SidebarMenuApiDto>>("api/v1/sidebar-menu/my-visible", ct) ?? new();
        return raw.Select(MapSidebarItem).ToList();
    }

    public Task SetSidebarMenuRolesAsync(Guid id, Dictionary<string, bool> roles, CancellationToken ct)
    {
        var payload = new
        {
            entries = roles.Select(kv => new { roleName = kv.Key, isAllowed = kv.Value }).ToList()
        };
        return PutAsync<object, object>($"api/v1/sidebar-menu/{id}/roles", payload, ct);
    }

    public Task SetSidebarMenuStatusAsync(Guid id, bool isActive, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/sidebar-menu/{id}/status", new { isActive }, ct);

    private static SidebarMenuItemWebModel MapSidebarItem(SidebarMenuApiDto dto) => new()
    {
        Id           = dto.Id,
        Key          = dto.Key,
        Name         = dto.Name,
        Purpose      = dto.Purpose,
        ParentId     = dto.ParentId,
        DisplayOrder = dto.DisplayOrder,
        IsActive     = dto.IsActive,
        IsSystemMenu = dto.IsSystemMenu,
        RoleAccesses = dto.RoleAccesses?.ToDictionary(r => r.RoleName, r => r.IsAllowed) ?? new(),
        SubMenus     = dto.SubMenus?.Select(MapSidebarItem).ToList() ?? new()
    };

    private sealed class SidebarMenuApiDto
    {
        public Guid   Id           { get; set; }
        public string Key          { get; set; } = string.Empty;
        public string Name         { get; set; } = string.Empty;
        public string Purpose      { get; set; } = string.Empty;
        public Guid?  ParentId     { get; set; }
        public int    DisplayOrder { get; set; }
        public bool   IsActive     { get; set; }
        public bool   IsSystemMenu { get; set; }
        public List<SidebarRoleAccessApiDto> RoleAccesses { get; set; } = new();
        public List<SidebarMenuApiDto>       SubMenus     { get; set; } = new();
    }

    private sealed class SidebarRoleAccessApiDto
    {
        public string RoleName  { get; set; } = string.Empty;
        public bool   IsAllowed { get; set; }
    }

    // ── License ───────────────────────────────────────────────────────────────

    public async Task<LicenseUpdatePageModel> GetLicenseDetailsAsync(CancellationToken ct)
    {
        var req = CreateRequest(HttpMethod.Get, "api/v1/license/details");
        using var client = CreateClient();
        using var resp   = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
            return new LicenseUpdatePageModel { IsConnected = true, Message = $"API error: {(int)resp.StatusCode}" };

        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var dto = await JsonSerializer.DeserializeAsync<LicenseDetailsDto>(stream, _jsonOptions, ct);
        return new LicenseUpdatePageModel
        {
            IsConnected  = true,
            Status       = dto?.Status,
            LicenseType  = dto?.LicenseType,
            ActivatedAt  = dto?.ActivatedAt,
            ExpiresAt    = dto?.ExpiresAt,
            UpdatedAt    = dto?.UpdatedAt,
            RemainingDays= dto?.RemainingDays,
        };
    }

    public async Task<string> UploadLicenseAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        var req = CreateRequest(HttpMethod.Post, "api/v1/license/upload");
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        req.Content = content;
        using var client = CreateClient();
        using var resp   = await client.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        return resp.IsSuccessStatusCode ? "License uploaded successfully." : $"Upload failed: {body}";
    }

    private sealed class LicenseDetailsDto
    {
        public string?   Status        { get; set; }
        public string?   LicenseType   { get; set; }
        public DateTime? ActivatedAt   { get; set; }
        public DateTime? ExpiresAt     { get; set; }
        public DateTime? UpdatedAt     { get; set; }
        public int?      RemainingDays { get; set; }
    }

    // ── Theme ─────────────────────────────────────────────────────────────────

    public async Task<string?> GetCurrentThemeAsync(CancellationToken ct)
    {
        var req = CreateRequest(HttpMethod.Get, "api/v1/theme");
        using var client = CreateClient();
        using var resp   = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return null;
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var dto = await JsonSerializer.DeserializeAsync<ThemeDto>(stream, _jsonOptions, ct);
        return dto?.ThemeKey;
    }

    public Task SetThemeAsync(string? themeKey, CancellationToken ct)
        => PutAsync<object, object>("api/v1/theme", new { themeKey }, ct);

    private sealed class ThemeDto { public string? ThemeKey { get; set; } }

    // ── Report Settings ───────────────────────────────────────────────────────

    public async Task<List<ReportDefinitionWebModel>> GetReportDefinitionsAsync(CancellationToken ct)
    {
        var req = CreateRequest(HttpMethod.Get, "api/v1/report-settings");
        using var client = CreateClient();
        using var resp   = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return new();
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var dtos = await JsonSerializer.DeserializeAsync<List<ReportDefinitionApiDto>>(stream, _jsonOptions, ct);
        return dtos?.Select(d => new ReportDefinitionWebModel
        {
            Id            = d.Id,
            Key           = d.Key ?? "",
            Name          = d.Name ?? "",
            Purpose       = d.Purpose ?? "",
            IsActive      = d.IsActive,
            AssignedRoles = d.AssignedRoles ?? new()
        }).ToList() ?? new();
    }

    public Task CreateReportDefinitionAsync(CreateReportForm form, CancellationToken ct)
        => PostAsync<object, object>("api/v1/report-settings", new { form.Key, form.Name, form.Purpose }, ct);

    public Task SetReportActiveAsync(Guid id, bool activate, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/report-settings/{id}/{(activate ? "activate" : "deactivate")}", new { }, ct);

    public Task SetReportRolesAsync(Guid id, List<string> roles, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/report-settings/{id}/roles", new { roleNames = roles }, ct);

    private sealed class ReportDefinitionApiDto
    {
        public Guid         Id            { get; set; }
        public string?      Key           { get; set; }
        public string?      Name          { get; set; }
        public string?      Purpose       { get; set; }
        public bool         IsActive      { get; set; }
        public List<string> AssignedRoles { get; set; } = new();
    }

    // ── Module Settings ───────────────────────────────────────────────────────

    public async Task<List<ModuleSettingsWebModel>> GetModuleSettingsAsync(CancellationToken ct)
    {
        var req = CreateRequest(HttpMethod.Get, "api/v1/modules/all-settings");
        using var client = CreateClient();
        using var resp   = await client.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode) return new();
        using var stream = await resp.Content.ReadAsStreamAsync(ct);
        var dtos = await JsonSerializer.DeserializeAsync<List<ModuleSettingsApiDto>>(stream, _jsonOptions, ct);
        return dtos?.Select(d => new ModuleSettingsWebModel
        {
            Id            = d.Id,
            Key           = d.Key ?? "",
            Name          = d.Name ?? "",
            IsMandatory   = d.IsMandatory,
            IsActive      = d.IsActive,
            AssignedRoles = d.AssignedRoles ?? new()
        }).ToList() ?? new();
    }

    public Task SetModuleActiveAsync(string key, bool activate, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/modules/{key}/{(activate ? "activate" : "deactivate")}", new { }, ct);

    public Task SetModuleRolesAsync(string key, List<string> roles, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/modules/{key}/roles", new { roleNames = roles }, ct);

    private sealed class ModuleSettingsApiDto
    {
        public Guid         Id            { get; set; }
        public string?      Key           { get; set; }
        public string?      Name          { get; set; }
        public bool         IsMandatory   { get; set; }
        public bool         IsActive      { get; set; }
        public List<string> AssignedRoles { get; set; } = new();
    }
}

