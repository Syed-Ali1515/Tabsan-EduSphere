using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Web.Models.Portal;
using Tabsan.EduSphere.Web.Services;

namespace Tabsan.EduSphere.Web.Controllers;

public class PortalController : Controller
{
    private readonly IEduApiClient _api;

    public PortalController(IEduApiClient api) => _api = api;

    // ── Dashboard / Connection ──────────────────────────────────────────────

    [HttpGet]
    public IActionResult Dashboard()
    {
        ViewData["Title"] = "Dashboard";
        var vm = _api.GetConnection();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveConnection(ApiConnectionModel model)
    {
        _api.SaveConnection(model);
        TempData["PortalMessage"] = "API connection saved in session.";
        return RedirectToAction(nameof(Dashboard));
    }

    // ── Timetable Admin ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> TimetableAdmin(Guid? timetableId, CancellationToken ct)
    {
        ViewData["Title"] = "Manage Timetables";
        var vm = new TimetableAdminPageModel
        {
            IsConnected = _api.IsConnected(),
            Connection = _api.GetConnection(),
            Message = TempData["PortalMessage"]?.ToString()
        };

        if (!vm.IsConnected)
        {
            vm.Message ??= "Configure API connection on Dashboard first.";
            return View(vm);
        }

        try
        {
            vm.Departments = await _api.GetDepartmentsAsync(ct);

            var selectedDepartment = vm.Connection.DefaultDepartmentId ?? vm.Departments.FirstOrDefault()?.Id;
            vm.CreateForm.DepartmentId = selectedDepartment ?? Guid.Empty;

            vm.Programs = await _api.GetProgramsAsync(selectedDepartment, ct);
            vm.Semesters = await _api.GetSemestersAsync(ct);
            vm.Courses = await _api.GetCoursesAsync(selectedDepartment, ct);
            vm.Faculty = await _api.GetFacultyAsync(ct);
            vm.Buildings = await _api.GetBuildingsAsync(ct);
            vm.Rooms = await _api.GetRoomsAsync(ct);

            if (selectedDepartment.HasValue)
                vm.Timetables = await _api.GetTimetablesByDepartmentAsync(selectedDepartment.Value, ct);

            var activeTimetableId = timetableId ?? vm.Timetables.FirstOrDefault()?.Id;
            if (activeTimetableId.HasValue)
            {
                vm.SelectedTimetable = await _api.GetTimetableByIdAsync(activeTimetableId.Value, ct);
                vm.EntryForm.TimetableId = activeTimetableId.Value;
            }
        }
        catch (Exception ex)
        {
            vm.Message = ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTimetable(CreateTimetableForm form, CancellationToken ct)
    {
        try
        {
            var id = await _api.CreateTimetableAsync(form, ct);
            TempData["PortalMessage"] = "Timetable created successfully.";
            return RedirectToAction(nameof(TimetableAdmin), new { timetableId = id });
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
            return RedirectToAction(nameof(TimetableAdmin));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTimetableEntry(AddTimetableEntryForm form, CancellationToken ct)
    {
        try
        {
            await _api.AddTimetableEntryAsync(form, ct);
            TempData["PortalMessage"] = "Timetable entry added.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(TimetableAdmin), new { timetableId = form.TimetableId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishTimetable(Guid timetableId, CancellationToken ct)
    {
        try
        {
            await _api.PublishTimetableAsync(timetableId, ct);
            TempData["PortalMessage"] = "Timetable published.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(TimetableAdmin), new { timetableId });
    }

    // ── Timetable Student ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> TimetableStudent(Guid? departmentId, Guid? timetableId, CancellationToken ct)
    {
        ViewData["Title"] = "Student Timetable";
        var vm = new TimetableStudentPageModel
        {
            IsConnected = _api.IsConnected(),
            Message = TempData["PortalMessage"]?.ToString()
        };

        if (!vm.IsConnected)
        {
            vm.Message ??= "Configure API connection on Dashboard first.";
            return View(vm);
        }

        try
        {
            var connection = _api.GetConnection();
            vm.DepartmentId = departmentId ?? connection.DefaultDepartmentId;
            if (!vm.DepartmentId.HasValue)
            {
                vm.Message = "Department is required. Set default department in Dashboard connection.";
                return View(vm);
            }

            vm.Timetables = await _api.GetTimetablesByDepartmentAsync(vm.DepartmentId.Value, ct);
            var activeTimetableId = timetableId ?? vm.Timetables.FirstOrDefault()?.Id;
            if (activeTimetableId.HasValue)
                vm.SelectedTimetable = await _api.GetTimetableByIdAsync(activeTimetableId.Value, ct);
        }
        catch (Exception ex)
        {
            vm.Message = ex.Message;
        }

        return View(vm);
    }

    // ── Timetable Teacher ───────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> TimetableTeacher(CancellationToken ct)
    {
        ViewData["Title"] = "Teacher Timetable";
        var vm = new TimetableTeacherPageModel
        {
            IsConnected = _api.IsConnected(),
            Message = TempData["PortalMessage"]?.ToString()
        };

        if (!vm.IsConnected)
        {
            vm.Message ??= "Configure API connection on Dashboard first.";
            return View(vm);
        }

        try
        {
            vm.Entries = await _api.GetTeacherEntriesAsync(ct);
        }
        catch (Exception ex)
        {
            vm.Message = ex.Message;
        }

        return View(vm);
    }

    // ── Buildings ───────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Buildings(Guid? selectedId, CancellationToken ct)
    {
        ViewData["Title"] = "Buildings";
        var vm = new BuildingsPageModel
        {
            IsConnected = _api.IsConnected(),
            Message = TempData["PortalMessage"]?.ToString()
        };

        if (!vm.IsConnected)
        {
            vm.Message ??= "Configure API connection on Dashboard first.";
            return View(vm);
        }

        try
        {
            vm.Buildings = await _api.GetAllBuildingsAsync(activeOnly: false, ct);
            if (selectedId.HasValue)
            {
                vm.SelectedBuilding = vm.Buildings.FirstOrDefault(b => b.Id == selectedId);
                if (vm.SelectedBuilding is not null)
                {
                    vm.EditForm.Name = vm.SelectedBuilding.Name;
                    vm.EditForm.Code = vm.SelectedBuilding.Code;
                }
            }
        }
        catch (Exception ex)
        {
            vm.Message = ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBuilding(BuildingFormModel form, CancellationToken ct)
    {
        try
        {
            var created = await _api.CreateBuildingAsync(form, ct);
            TempData["PortalMessage"] = $"Building '{created.Name}' created.";
            return RedirectToAction(nameof(Buildings), new { selectedId = created.Id });
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
            return RedirectToAction(nameof(Buildings));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBuilding(Guid id, BuildingFormModel form, CancellationToken ct)
    {
        try
        {
            var updated = await _api.UpdateBuildingAsync(id, form, ct);
            TempData["PortalMessage"] = $"Building '{updated.Name}' updated.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Buildings), new { selectedId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBuildingActive(Guid id, bool activate, CancellationToken ct)
    {
        try
        {
            if (activate)
                await _api.ActivateBuildingAsync(id, ct);
            else
                await _api.DeactivateBuildingAsync(id, ct);

            TempData["PortalMessage"] = activate ? "Building activated." : "Building deactivated.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Buildings), new { selectedId = id });
    }

    // ── Rooms ───────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Rooms(Guid? buildingId, Guid? selectedId, CancellationToken ct)
    {
        ViewData["Title"] = "Rooms";
        var vm = new RoomsPageModel
        {
            IsConnected = _api.IsConnected(),
            Message = TempData["PortalMessage"]?.ToString(),
            SelectedBuildingId = buildingId
        };

        if (!vm.IsConnected)
        {
            vm.Message ??= "Configure API connection on Dashboard first.";
            return View(vm);
        }

        try
        {
            vm.Buildings = await _api.GetAllBuildingsAsync(activeOnly: false, ct);

            var activeBuildingId = buildingId ?? vm.Buildings.FirstOrDefault()?.Id;
            vm.SelectedBuildingId = activeBuildingId;

            if (activeBuildingId.HasValue)
            {
                vm.Rooms = await _api.GetRoomsForBuildingAsync(activeBuildingId.Value, activeOnly: false, ct);
                vm.CreateForm.BuildingId = activeBuildingId.Value;
            }

            if (selectedId.HasValue)
            {
                vm.SelectedRoom = vm.Rooms.FirstOrDefault(r => r.Id == selectedId);
                if (vm.SelectedRoom is not null)
                {
                    vm.EditForm.BuildingId = vm.SelectedRoom.BuildingId;
                    vm.EditForm.Number     = vm.SelectedRoom.Number;
                    vm.EditForm.Capacity   = vm.SelectedRoom.Capacity;
                }
            }
        }
        catch (Exception ex)
        {
            vm.Message = ex.Message;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoom(RoomFormModel form, CancellationToken ct)
    {
        try
        {
            var created = await _api.CreateRoomAsync(form, ct);
            TempData["PortalMessage"] = $"Room '{created.Number}' created.";
            return RedirectToAction(nameof(Rooms), new { buildingId = created.BuildingId, selectedId = created.Id });
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
            return RedirectToAction(nameof(Rooms), new { buildingId = form.BuildingId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoom(Guid id, Guid buildingId, RoomFormModel form, CancellationToken ct)
    {
        try
        {
            var updated = await _api.UpdateRoomAsync(id, form, ct);
            TempData["PortalMessage"] = $"Room '{updated.Number}' updated.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Rooms), new { buildingId, selectedId = id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRoomActive(Guid id, Guid buildingId, bool activate, CancellationToken ct)
    {
        try
        {
            if (activate)
                await _api.ActivateRoomAsync(id, ct);
            else
                await _api.DeactivateRoomAsync(id, ct);

            TempData["PortalMessage"] = activate ? "Room activated." : "Room deactivated.";
        }
        catch (Exception ex)
        {
            TempData["PortalMessage"] = ex.Message;
        }
        return RedirectToAction(nameof(Rooms), new { buildingId, selectedId = id });
    }

    // ── License Update ─────────────────────────────────────────────────────

    public async Task<IActionResult> LicenseUpdate(CancellationToken ct)
    {
        var model = new LicenseUpdatePageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try
            {
                var details = await _api.GetLicenseDetailsAsync(ct);
                model.Status       = details.Status;
                model.LicenseType  = details.LicenseType;
                model.ActivatedAt  = details.ActivatedAt;
                model.ExpiresAt    = details.ExpiresAt;
                model.UpdatedAt    = details.UpdatedAt;
                model.RemainingDays= details.RemainingDays;
                model.Message      = details.Message;
            }
            catch (Exception ex)
            {
                model.Message = $"Error loading license details: {ex.Message}";
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadLicense(IFormFile licenseFile, CancellationToken ct)
    {
        if (licenseFile is null || licenseFile.Length == 0)
        {
            TempData["Message"] = "Please select a valid .tablic file.";
            return RedirectToAction(nameof(LicenseUpdate));
        }
        if (!licenseFile.FileName.EndsWith(".tablic", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Message"] = "Invalid file type. Only .tablic files are accepted.";
            return RedirectToAction(nameof(LicenseUpdate));
        }
        if (_api.IsConnected())
        {
            try
            {
                using var stream = licenseFile.OpenReadStream();
                var result = await _api.UploadLicenseAsync(stream, licenseFile.FileName, ct);
                TempData["Message"] = result;
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Upload error: {ex.Message}";
            }
        }
        return RedirectToAction(nameof(LicenseUpdate));
    }

    // ── Theme Settings ─────────────────────────────────────────────────────

    public async Task<IActionResult> ThemeSettings(CancellationToken ct)
    {
        var model = new ThemeSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try { model.CurrentTheme = await _api.GetCurrentThemeAsync(ct); }
            catch (Exception ex) { model.Message = $"Error loading theme: {ex.Message}"; }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetTheme(string? themeKey, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                await _api.SetThemeAsync(themeKey, ct);
                TempData["Message"] = "Theme updated.";
            }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ThemeSettings));
    }

    // ── Report Settings ────────────────────────────────────────────────────

    public async Task<IActionResult> ReportSettings(CancellationToken ct)
    {
        var model = new ReportSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try { model.Reports = await _api.GetReportDefinitionsAsync(ct); }
            catch (Exception ex) { model.Message = $"Error loading reports: {ex.Message}"; }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateReport(CreateReportForm form, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateReportDefinitionAsync(form, ct); TempData["Message"] = "Report created."; }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ReportSettings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleReport(Guid id, bool activate, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.SetReportActiveAsync(id, activate, ct); }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ReportSettings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReportRoles(Guid id, [FromForm] bool adminAllowed,
        [FromForm] bool facultyAllowed, [FromForm] bool studentAllowed, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                var roles = new List<string>();
                if (adminAllowed)   roles.Add("Admin");
                if (facultyAllowed) roles.Add("Faculty");
                if (studentAllowed) roles.Add("Student");
                await _api.SetReportRolesAsync(id, roles, ct);
                TempData["Message"] = "Roles updated.";
            }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ReportSettings));
    }

    // ── Module Settings ────────────────────────────────────────────────────

    public async Task<IActionResult> ModuleSettings(CancellationToken ct)
    {
        var model = new ModuleSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try { model.Modules = await _api.GetModuleSettingsAsync(ct); }
            catch (Exception ex) { model.Message = $"Error loading modules: {ex.Message}"; }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleModule(string key, bool activate, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.SetModuleActiveAsync(key, activate, ct); }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ModuleSettings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateModuleRoles(string key, [FromForm] bool adminAllowed,
        [FromForm] bool facultyAllowed, [FromForm] bool studentAllowed, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                var roles = new List<string>();
                if (adminAllowed)   roles.Add("Admin");
                if (facultyAllowed) roles.Add("Faculty");
                if (studentAllowed) roles.Add("Student");
                await _api.SetModuleRolesAsync(key, roles, ct);
                TempData["Message"] = "Roles updated.";
            }
            catch (Exception ex) { TempData["Message"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(ModuleSettings));
    }

    // ── Sidebar Settings ────────────────────────────────────────────────────

    public async Task<IActionResult> SidebarSettings(CancellationToken ct)
    {
        var model = new SidebarSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try
            {
                model.TopLevelMenus = await _api.GetSidebarMenusAsync(ct);
            }
            catch (Exception ex)
            {
                model.Message = $"Error loading sidebar menus: {ex.Message}";
            }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSidebarMenuRoles(Guid menuId, string menuName,
        [FromForm] bool adminAllowed, [FromForm] bool facultyAllowed, [FromForm] bool studentAllowed,
        CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                var roles = new Dictionary<string, bool>
                {
                    ["Admin"]   = adminAllowed,
                    ["Faculty"] = facultyAllowed,
                    ["Student"] = studentAllowed
                };
                await _api.SetSidebarMenuRolesAsync(menuId, roles, ct);
                TempData["Message"] = $"Roles updated for \"{menuName}\".";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
            }
        }
        return RedirectToAction(nameof(SidebarSettings));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSidebarMenuStatus(Guid menuId, string menuName,
        [FromForm] bool isActive, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                await _api.SetSidebarMenuStatusAsync(menuId, isActive, ct);
                TempData["Message"] = $"Status updated for \"{menuName}\".";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
            }
        }
        return RedirectToAction(nameof(SidebarSettings));
    }

    private IActionResult Section(string title, string description)
    {
        ViewData["Title"] = title;
        ViewData["SectionDescription"] = description;
        return View("Section");
    }
}
