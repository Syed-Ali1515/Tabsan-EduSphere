using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Web.Models;
using Tabsan.EduSphere.Web.Services;

namespace Tabsan.EduSphere.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEduApiClient _api;

    public HomeController(ILogger<HomeController> logger, IEduApiClient api)
    {
        _logger = logger;
        _api    = api;
    }

    public IActionResult Index()
    {
        // If already logged in, go straight to the dashboard
        if (_api.IsConnected())
            return RedirectToAction("Dashboard", "Portal");

        return RedirectToAction("Index", "Login");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
