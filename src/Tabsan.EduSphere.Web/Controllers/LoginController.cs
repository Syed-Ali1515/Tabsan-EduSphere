using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using Tabsan.EduSphere.Web.Models.Portal;
using Tabsan.EduSphere.Web.Services;

namespace Tabsan.EduSphere.Web.Controllers;

public class LoginController : Controller
{
    private readonly IEduApiClient _api;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public LoginController(IEduApiClient api, IConfiguration config, IHttpClientFactory http)
    {
        _api    = api;
        _config = config;
        _http   = http;
    }

    // GET /Login
    [HttpGet]
    public IActionResult Index(string? returnUrl = null)
    {
        if (_api.IsConnected())
            return RedirectToAction("Dashboard", "Portal");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST /Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string username, string password, string? returnUrl = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewData["Error"] = "Username and password are required.";
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        var apiBase = _config["EduApi:BaseUrl"]?.TrimEnd('/') ?? "http://localhost:5181";

        try
        {
            var client  = _http.CreateClient();
            var payload = JsonSerializer.Serialize(new { username, password });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");

            using var response = await client.PostAsync($"{apiBase}/api/v1/auth/login", content, ct);

            if (!response.IsSuccessStatusCode)
            {
                ViewData["Error"] = response.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Invalid username or password."
                    : $"Login failed (HTTP {(int)response.StatusCode}).";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            var body   = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<LoginApiResponse>(body, _json);

            if (result is null || string.IsNullOrWhiteSpace(result.AccessToken))
            {
                ViewData["Error"] = "Unexpected response from API.";
                ViewData["ReturnUrl"] = returnUrl;
                return View();
            }

            // Reuse existing connection machinery — stores token + identity in session
            _api.SaveConnection(new ApiConnectionModel
            {
                ApiBaseUrl  = apiBase,
                AccessToken = result.AccessToken
            });

            var redirect = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Dashboard", "Portal")!;
            return Redirect(redirect);
        }
        catch (HttpRequestException)
        {
            ViewData["Error"] = $"Cannot reach the API at {apiBase}. Make sure the API is running.";
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }

    // POST /Login/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Index));
    }

    // ── Private DTO ───────────────────────────────────────────────────────────
    private sealed record LoginApiResponse(
        string AccessToken,
        string RefreshToken,
        DateTime AccessTokenExpiry,
        string Role,
        System.Guid UserId,
        string Username);
}
