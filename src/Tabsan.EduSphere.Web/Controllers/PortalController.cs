using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Web.Models.Portal;
using Tabsan.EduSphere.Web.Services;

namespace Tabsan.EduSphere.Web.Controllers;

public class PortalController : Controller
{
    private readonly IEduApiClient _api;

    public PortalController(IEduApiClient api) => _api = api;

    private async Task<List<LookupItem>> GetOfferingFilterOptionsAsync(SessionIdentity? sessionIdentity, CancellationToken ct)
    {
        if (sessionIdentity?.IsAdmin == true || sessionIdentity?.IsSuperAdmin == true)
        {
            var offerings = await _api.GetCourseOfferingsAsync(null, ct);
            return offerings.Select(o => new LookupItem
            {
                Id = o.Id,
                Name = string.IsNullOrWhiteSpace(o.CourseCode)
                    ? $"{o.CourseTitle} ({o.SemesterName})"
                    : $"{o.CourseCode} — {o.CourseTitle} ({o.SemesterName})"
            }).ToList();
        }

        return await _api.GetMyOfferingsAsync(ct);
    }

    private async Task<Guid?> GetEffectiveStudentDepartmentIdAsync(CancellationToken ct)
    {
        var sessionIdentity = _api.GetSessionIdentity();
        if (sessionIdentity?.IsStudent == true)
            return (await _api.GetMyStudentProfileAsync(ct))?.DepartmentId ?? _api.GetConnection().DefaultDepartmentId;

        return _api.GetConnection().DefaultDepartmentId;
    }

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
            vm.DepartmentId = departmentId ?? await GetEffectiveStudentDepartmentIdAsync(ct);
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

    // ── Result Calculation ────────────────────────────────────────────────

    public async Task<IActionResult> ResultCalculation(CancellationToken ct)
    {
        var model = new ResultCalculationSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try
            {
                model = await _api.GetResultCalculationSettingsAsync(ct);
                model.IsConnected = true;
            }
            catch (Exception ex)
            {
                model.Message = $"Error loading result calculation settings: {ex.Message}";
            }
        }

        if (model.GpaRules.Count == 0)
        {
            model.GpaRules.Add(new ResultCalculationGpaRuleItem { DisplayOrder = 1, GradePoint = 2.0m, MinimumScore = 60m });
            model.GpaRules.Add(new ResultCalculationGpaRuleItem { DisplayOrder = 2, GradePoint = 2.5m, MinimumScore = 65m });
        }

        if (model.ComponentRules.Count == 0)
        {
            model.ComponentRules.Add(new ResultCalculationComponentRuleItem { DisplayOrder = 1, Name = "Quizzes", Weightage = 20m, IsActive = true });
            model.ComponentRules.Add(new ResultCalculationComponentRuleItem { DisplayOrder = 2, Name = "Midterms", Weightage = 30m, IsActive = true });
            model.ComponentRules.Add(new ResultCalculationComponentRuleItem { DisplayOrder = 3, Name = "Finals", Weightage = 50m, IsActive = true });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResultCalculation(ResultCalculationSettingsPageModel model, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                model.GpaRules = model.GpaRules
                    .Where(r => r.GradePoint > 0 || r.MinimumScore > 0)
                    .ToList();
                model.ComponentRules = model.ComponentRules
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name) && r.Weightage > 0)
                    .ToList();

                await _api.SaveResultCalculationSettingsAsync(model, ct);
                TempData["Message"] = "Result calculation settings updated.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = $"Error: {ex.Message}";
            }
        }

        return RedirectToAction(nameof(ResultCalculation));
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

    // ── Notifications ──────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Notifications(CancellationToken ct)
    {
        ViewData["Title"] = "Notifications";
        var model = new NotificationsPageModel { IsConnected = _api.IsConnected() };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Notifications = await _api.GetNotificationsAsync(ct);
            model.UnreadCount   = await _api.GetUnreadNotificationCountAsync(ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.MarkAllNotificationsReadAsync(ct); }
            catch { /* swallow */ }
        }
        return RedirectToAction(nameof(Notifications));
    }

    // Final-Touches Phase 6 Stage 6.1 — mark individual notification as read
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkNotificationRead(Guid id, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.MarkNotificationReadAsync(id, ct); }
            catch { /* swallow */ }
        }
        return RedirectToAction(nameof(Notifications));
    }

    // ── Students ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Students(Guid? departmentId, CancellationToken ct)
    {
        ViewData["Title"] = "Students";
        var model = new StudentsPageModel { IsConnected = _api.IsConnected(), SelectedDepartmentId = departmentId };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Students    = await _api.GetStudentsAsync(departmentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Departments ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Departments(CancellationToken ct)
    {
        ViewData["Title"] = "Departments";
        var model = new DepartmentsPageModel { IsConnected = _api.IsConnected() };
        if (!model.IsConnected) return View(model);
        try { model.Departments = await _api.GetDepartmentDetailsAsync(ct); }
        catch (Exception ex) { model.Message = ex.Message; }
        model.Message ??= TempData["PortalMessage"]?.ToString();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(string name, string code, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateDepartmentAsync(name, code, ct); TempData["PortalMessage"] = $"Department '{name}' created."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateDepartment(Guid id, string newName, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.UpdateDepartmentAsync(id, newName, ct); TempData["PortalMessage"] = $"Department renamed to '{newName}'."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Departments));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateDepartment(Guid id, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.DeactivateDepartmentAsync(id, ct); TempData["PortalMessage"] = "Department deactivated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Departments));
    }

    // ── Courses ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Courses(Guid? departmentId, CancellationToken ct)
    {
        ViewData["Title"] = "Courses & Offerings";
        var model = new CoursesPageModel { IsConnected = _api.IsConnected(), SelectedDepartmentId = departmentId };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Semesters   = await _api.GetSemestersAsync(ct);
            if (sessionId?.IsAdmin == true || sessionId?.IsSuperAdmin == true)
                model.Faculty = await _api.GetFacultyAsync(ct);
            model.Courses     = await _api.GetCourseDetailsAsync(departmentId, ct);
            model.Offerings   = await _api.GetCourseOfferingsAsync(departmentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        model.Message ??= TempData["PortalMessage"]?.ToString();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(string code, string title, int creditHours, Guid departmentId, Guid? filterDepartmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateCourseAsync(code, title, creditHours, departmentId, ct); TempData["PortalMessage"] = $"Course '{code}' created."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Courses), new { departmentId = filterDepartmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOffering(Guid courseId, Guid semesterId, int maxEnrollment, Guid? facultyUserId, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateOfferingAsync(courseId, semesterId, maxEnrollment, facultyUserId == Guid.Empty ? null : facultyUserId, ct); TempData["PortalMessage"] = "Course offering created."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Courses), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateCourse(Guid id, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.DeactivateCourseAsync(id, ct); TempData["PortalMessage"] = "Course deactivated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Courses), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOffering(Guid id, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.DeleteOfferingAsync(id, ct); TempData["PortalMessage"] = "Offering deleted."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Courses), new { departmentId });
    }

    // ── Assignments ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Assignments(Guid? offeringId, Guid? selectedAssignmentId, CancellationToken ct)
    {
        ViewData["Title"] = "Assignments";
        var model = new AssignmentsPageModel
        {
            IsConnected          = _api.IsConnected(),
            SelectedOfferingId   = offeringId,
            SelectedAssignmentId = selectedAssignmentId,
            Message              = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            if (sessionId?.IsStudent == true)
            {
                model.Assignments = offeringId.HasValue
                    ? await _api.GetAssignmentsByOfferingAsync(offeringId.Value, ct)
                    : await _api.GetMyAssignmentsAsync(ct);

                var mySubmissions = await _api.GetMyAssignmentSubmissionsAsync(ct);
                var submissionByAssignment = mySubmissions
                    .OrderByDescending(s => s.SubmittedAt)
                    .GroupBy(s => s.AssignmentId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var assignment in model.Assignments)
                {
                    if (submissionByAssignment.TryGetValue(assignment.Id, out var submission))
                    {
                        assignment.IsSubmitted = true;
                        assignment.MarksAwarded = submission.MarksAwarded;
                    }
                }
            }
            else if (offeringId.HasValue)
            {
                model.Assignments = await _api.GetAssignmentsByOfferingAsync(offeringId.Value, ct);
                if (selectedAssignmentId.HasValue)
                    model.Submissions = await _api.GetSubmissionsForAssignmentAsync(selectedAssignmentId.Value, ct);
            }

            model.CourseOfferings = await GetOfferingFilterOptionsAsync(sessionId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Attendance ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Attendance(Guid? offeringId, CancellationToken ct)
    {
        ViewData["Title"] = "Attendance";
        var model = new AttendancePageModel
        {
            IsConnected      = _api.IsConnected(),
            SelectedOfferingId = offeringId,
            Message          = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            if (sessionId?.IsStudent == true)
            {
                model.Summary = await _api.GetMyAttendanceSummaryAsync(ct);
            }
            else if (offeringId.HasValue)
            {
                model.Records = await _api.GetAttendanceByOfferingAsync(offeringId.Value, ct);
                model.Roster  = await _api.GetEnrollmentRosterAsync(offeringId.Value, ct);
            }

            model.CourseOfferings = await GetOfferingFilterOptionsAsync(sessionId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Results ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Results(Guid? offeringId, CancellationToken ct)
    {
        ViewData["Title"] = "Results";
        var model = new ResultsPageModel
        {
            IsConnected      = _api.IsConnected(),
            SelectedOfferingId = offeringId,
            Message          = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            model.Offerings = await GetOfferingFilterOptionsAsync(sessionId, ct);
            if (sessionId?.IsStudent == true)
            {
                model.Results = await _api.GetMyResultsAsync(ct);
            }
            else if (offeringId.HasValue)
            {
                model.Results   = await _api.GetResultsByOfferingAsync(offeringId.Value, ct);
                model.Roster    = await _api.GetEnrollmentRosterAsync(offeringId.Value, ct);
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Quizzes ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Quizzes(Guid? offeringId, CancellationToken ct)
    {
        ViewData["Title"] = "Quizzes";
        var model = new QuizzesPageModel
        {
            IsConnected      = _api.IsConnected(),
            SelectedOfferingId = offeringId,
            Message          = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            model.CourseOfferings = await GetOfferingFilterOptionsAsync(sessionId, ct);

            if (offeringId.HasValue)
                model.Quizzes = await _api.GetQuizzesByOfferingAsync(offeringId.Value, ct);

            if (sessionId?.IsStudent == true)
                model.MyAttempts = await _api.GetMyAttemptsAsync(ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── FYP ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Fyp(Guid? departmentId, CancellationToken ct)
    {
        ViewData["Title"] = "FYP Management";
        var model = new FypPageModel
        {
            IsConnected         = _api.IsConnected(),
            SelectedDepartmentId = departmentId,
            Message             = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            if (sessionId?.IsStudent == true)
            {
                var profile = await _api.GetMyStudentProfileAsync(ct);
                if ((profile?.CurrentSemesterNumber ?? 0) < 8)
                {
                    TempData["PortalMessage"] = "FYP becomes available from semester 8.";
                    return RedirectToAction(nameof(Dashboard));
                }

                model.Projects = await _api.GetMyFypProjectsAsync(ct);
            }
            else if (sessionId?.IsFaculty == true)
            {
                model.Departments = await _api.GetDepartmentsAsync(ct);
                model.UpcomingMeetings = await _api.GetUpcomingMeetingsAsync(ct);
                model.Projects = await _api.GetMySupervisedProjectsAsync(ct);
            }
            else if (departmentId.HasValue)
            {
                model.Departments = await _api.GetDepartmentsAsync(ct);
                model.Faculty = await _api.GetFacultyAsync(ct);
                model.Students = await _api.GetStudentsAsync(departmentId, ct);
                model.UpcomingMeetings = await _api.GetUpcomingMeetingsAsync(ct);
                model.Projects = await _api.GetFypByDepartmentAsync(departmentId.Value, ct);
            }
            else if (sessionId?.IsAdmin == true || sessionId?.IsSuperAdmin == true)
            {
                model.Departments = await _api.GetDepartmentsAsync(ct);
                model.Faculty = await _api.GetFacultyAsync(ct);
                model.Students = await _api.GetStudentsAsync(null, ct);
                model.UpcomingMeetings = await _api.GetUpcomingMeetingsAsync(ct);
                model.Projects = await _api.GetAllFypProjectsAsync(ct);
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Analytics ──────────────────────────────────────────────────────────

    // ── Assignment write actions ────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitAssignment(
        Guid assignmentId, Guid? offeringId, string? textContent, IFormFile? submissionFile, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                string? fileUrl = null;

                if (submissionFile is { Length: > 0 })
                {
                    var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "assignment-submissions");
                    Directory.CreateDirectory(uploadsRoot);

                    var extension = Path.GetExtension(submissionFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var physicalPath = Path.Combine(uploadsRoot, fileName);

                    await using var stream = System.IO.File.Create(physicalPath);
                    await submissionFile.CopyToAsync(stream, ct);

                    fileUrl = $"/uploads/assignment-submissions/{fileName}";
                }

                if (string.IsNullOrWhiteSpace(fileUrl) && string.IsNullOrWhiteSpace(textContent))
                {
                    TempData["PortalMessage"] = "Attach a file or add submission text before submitting.";
                    return RedirectToAction(nameof(Assignments), new { offeringId });
                }

                await _api.SubmitAssignmentAsync(assignmentId, fileUrl, textContent, ct);
                TempData["PortalMessage"] = "Assignment submitted for faculty review.";
            }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }

        return RedirectToAction(nameof(Assignments), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssignment(
        Guid offeringId, string title, string? description,
        DateTime dueDate, decimal maxMarks, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateAssignmentAsync(offeringId, title, description, dueDate, maxMarks, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Assignments), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAssignment(
        Guid id, Guid offeringId, string title, string? description,
        DateTime dueDate, decimal maxMarks, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.UpdateAssignmentAsync(id, title, description, dueDate, maxMarks, ct); TempData["PortalMessage"] = "Assignment updated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Assignments), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishAssignment(Guid id, Guid? offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.PublishAssignmentAsync(id, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Assignments), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAssignment(Guid id, Guid? offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.DeleteAssignmentAsync(id, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Assignments), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GradeSubmission(
        Guid assignmentId, Guid studentProfileId, Guid? offeringId,
        decimal marksAwarded, string? feedback, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.GradeSubmissionAsync(assignmentId, studentProfileId, marksAwarded, feedback, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Assignments), new { offeringId, selectedAssignmentId = assignmentId });
    }

    // ── Attendance write actions ────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkMarkAttendance(
        Guid offeringId, DateTime date,
        [FromForm] Guid[] studentIds, [FromForm] string[] statuses,
        CancellationToken ct)
    {
        if (_api.IsConnected() && studentIds.Length > 0)
        {
            try
            {
                var entries = studentIds
                    .Zip(statuses, (sid, s) => (StudentProfileId: sid, Status: s));
                await _api.BulkMarkAttendanceAsync(offeringId, date, entries, ct);
                TempData["PortalMessage"] = "Attendance marked.";
            }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Attendance), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CorrectAttendance(
        Guid studentProfileId, Guid offeringId, DateTime date,
        string newStatus, string? remarks, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CorrectAttendanceAsync(studentProfileId, offeringId, date, newStatus, remarks, ct); TempData["PortalMessage"] = "Attendance corrected."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Attendance), new { offeringId });
    }

    // ── Result write actions ────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResult(
        Guid studentProfileId, Guid offeringId,
        string resultType, decimal marksObtained, decimal maxMarks,
        bool promote, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try
            {
                await _api.CreateResultAsync(studentProfileId, offeringId, resultType, marksObtained, maxMarks, ct);

                // Promotion is only offered in the UI for Final result type.
                // When the checkbox is checked the form sends promote=true;
                // unchecked checkboxes send nothing, so promote defaults to false.
                if (promote)
                    await _api.PromoteStudentAsync(studentProfileId, ct);
            }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Results), new { offeringId });
    }

    // Standalone per-row Promote button in the Results table.
    // Reuses the existing POST api/v1/student-lifecycle/{id}/promote endpoint
    // without requiring a new result entry.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteStudentFromResult(Guid studentProfileId, Guid? offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.PromoteStudentAsync(studentProfileId, ct); TempData["PortalMessage"] = "Student promoted to next semester."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Results), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CorrectResult(
        Guid studentProfileId, Guid offeringId, string resultType,
        decimal newMarksObtained, decimal newMaxMarks, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CorrectResultAsync(studentProfileId, offeringId, resultType, newMarksObtained, newMaxMarks, ct); TempData["PortalMessage"] = "Result corrected."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Results), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishAllResults(Guid offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.PublishAllResultsAsync(offeringId, ct); TempData["PortalMessage"] = "All results published."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Results), new { offeringId });
    }

    // ── Quiz write actions ──────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuiz(
        Guid offeringId, string title, string? instructions,
        int? timeLimitMinutes, int maxAttempts, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateQuizAsync(offeringId, title, instructions, timeLimitMinutes, maxAttempts, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Quizzes), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuiz(
        Guid id, Guid offeringId, string title, string? instructions,
        int? timeLimitMinutes, int maxAttempts, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.UpdateQuizAsync(id, title, instructions, timeLimitMinutes, maxAttempts, ct); TempData["PortalMessage"] = "Quiz updated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Quizzes), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishQuiz(Guid id, Guid? offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.PublishQuizAsync(id, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Quizzes), new { offeringId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuiz(Guid id, Guid? offeringId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.DeleteQuizAsync(id, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Quizzes), new { offeringId });
    }

    // ── FYP write actions ───────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ProposeFypProject(
        Guid departmentId, string title, string description, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.ProposeFypProjectAsync(departmentId, title, description, ct); }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFypProject(
        Guid studentProfileId, Guid departmentId, string title, string description, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CreateFypProjectAsync(studentProfileId, departmentId, title, description, ct); TempData["PortalMessage"] = "FYP project created."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateFypProject(Guid id, Guid? departmentId, string title, string description, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.UpdateFypProjectAsync(id, title, description, ct); TempData["PortalMessage"] = "FYP project updated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveFypProject(Guid id, string? remarks, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.ApproveFypProjectAsync(id, remarks, ct); TempData["PortalMessage"] = "Project approved."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectFypProject(Guid id, string remarks, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.RejectFypProjectAsync(id, remarks, ct); TempData["PortalMessage"] = "Project rejected."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignFypSupervisor(Guid id, Guid supervisorUserId, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.AssignFypSupervisorAsync(id, supervisorUserId, ct); TempData["PortalMessage"] = "Supervisor assigned."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteFypProject(Guid id, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.CompleteFypProjectAsync(id, ct); TempData["PortalMessage"] = "Project marked as complete."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(Fyp), new { departmentId });
    }

    [HttpGet]
    public async Task<IActionResult> Analytics(CancellationToken ct)
    {
        ViewData["Title"] = "Analytics";
        var model = new AnalyticsPageModel { IsConnected = _api.IsConnected() };
        if (!model.IsConnected) return View(model);
        try
        {
            // Final-Touches Phase 6 Stage 6.2 — fetch typed DTOs instead of raw JSON
            model.Performance = await _api.GetPerformanceAnalyticsAsync(ct);
            model.Attendance  = await _api.GetAttendanceAnalyticsAsync(ct);
            model.Assignments = await _api.GetAssignmentAnalyticsAsync(ct);

            // Populate summary cards from real data
            if (model.Performance is not null)
            {
                model.Cards.Add(new AnalyticsSummaryCard
                {
                    Label      = "Avg. Marks",
                    Value      = $"{model.Performance.AverageMarks:F1}%",
                    SubText    = $"{model.Performance.TotalStudents} students · {model.Performance.DepartmentName}",
                    ColorClass = "text-primary",
                    Icon       = "📊"
                });
            }
            if (model.Attendance is not null)
            {
                model.Cards.Add(new AnalyticsSummaryCard
                {
                    Label      = "Avg. Attendance",
                    Value      = $"{model.Attendance.OverallAttendancePercentage:F1}%",
                    SubText    = model.Attendance.DepartmentName,
                    ColorClass = "text-success",
                    Icon       = "📋"
                });
            }
            if (model.Assignments is not null)
            {
                model.Cards.Add(new AnalyticsSummaryCard
                {
                    Label      = "Assignments",
                    Value      = model.Assignments.Assignments.Count.ToString(),
                    SubText    = model.Assignments.DepartmentName,
                    ColorClass = "text-warning",
                    Icon       = "📝"
                });
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── AI Chat ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> AiChat(Guid? conversationId, CancellationToken ct)
    {
        ViewData["Title"] = "AI Assistant";
        var model = new AiChatPageModel
        {
            IsConnected          = _api.IsConnected(),
            ActiveConversationId = conversationId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Conversations = await _api.GetChatConversationsAsync(ct);
            if (conversationId.HasValue)
                model.CurrentMessages = await _api.GetChatMessagesAsync(conversationId.Value, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AiChatSend(Guid? conversationId, string message, CancellationToken ct)
    {
        if (!_api.IsConnected() || string.IsNullOrWhiteSpace(message))
            return RedirectToAction(nameof(AiChat), new { conversationId });
        try
        {
            var reply = await _api.SendChatMessageAsync(conversationId, message, ct);
            // The API returns the assistant reply; reload the conversation
        }
        catch { /* errors handled gracefully — just reload */ }
        return RedirectToAction(nameof(AiChat), new { conversationId });
    }

    // ── Student Lifecycle ──────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> StudentLifecycle(Guid? departmentId, int semester = 1, CancellationToken ct = default)
    {
        ViewData["Title"] = "Student Lifecycle";
        var model = new StudentLifecyclePageModel
        {
            IsConnected         = _api.IsConnected(),
            SelectedDepartmentId = departmentId,
            SelectedSemester    = semester,
            Message             = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments = await _api.GetDepartmentsAsync(ct);
            if (departmentId.HasValue)
            {
                model.GraduationCandidates = await _api.GetGraduationCandidatesAsync(departmentId.Value, ct);
                model.StudentsBySemester   = await _api.GetStudentsBySemesterAsync(departmentId.Value, semester, ct);
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GraduateStudent(Guid studentId, Guid? departmentId, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.GraduateStudentAsync(studentId, ct); TempData["PortalMessage"] = "Student graduated."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(StudentLifecycle), new { departmentId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteStudent(Guid studentId, Guid? departmentId, int semester, CancellationToken ct)
    {
        if (_api.IsConnected())
        {
            try { await _api.PromoteStudentAsync(studentId, ct); TempData["PortalMessage"] = "Student promoted."; }
            catch (Exception ex) { TempData["PortalMessage"] = $"Error: {ex.Message}"; }
        }
        return RedirectToAction(nameof(StudentLifecycle), new { departmentId, semester });
    }

    // ── Payments ───────────────────────────────────────────────────────────
    // Final-Touches Phase 7 — admin all-receipts view + student own receipts

    [HttpGet]
    public async Task<IActionResult> Payments(Guid? studentId, CancellationToken ct)
    {
        ViewData["Title"] = "Payments";
        var model = new PaymentsPageModel
        {
            IsConnected       = _api.IsConnected(),
            SelectedStudentId = studentId,
            Message           = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            // Student role: load their own receipts via JWT
            var identity = _api.GetSessionIdentity();
            if (identity?.IsStudent == true)
            {
                model.Payments = await _api.GetMyPaymentsAsync(ct);
            }
            else
            {
                // Admin / Finance: load all receipts + student list for create form
                model.Payments = await _api.GetAllPaymentsAsync(ct);
                model.Students = await _api.GetStudentsAsync(null, ct);
                if (studentId.HasValue)
                    model.Payments = await _api.GetPaymentsByStudentAsync(studentId.Value, ct);
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // Final-Touches Phase 7 Stage 7.2 — create receipt (Admin/Finance)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePayment(CreatePaymentForm form, CancellationToken ct)
    {
        try
        {
            await _api.CreatePaymentAsync(form.StudentProfileId, form.Amount, form.Description, form.DueDate, ct);
            TempData["PortalMessage"] = "Receipt created successfully.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Payments));
    }

    // Final-Touches Phase 7 Stage 7.2 — confirm payment (Admin/Finance)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmPayment(Guid receiptId, CancellationToken ct)
    {
        try
        {
            await _api.ConfirmPaymentAsync(receiptId, ct);
            TempData["PortalMessage"] = "Payment confirmed.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Payments));
    }

    // Final-Touches Phase 7 Stage 7.2 — cancel receipt (Admin/Finance)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelPayment(Guid receiptId, CancellationToken ct)
    {
        try
        {
            await _api.CancelPaymentAsync(receiptId, ct);
            TempData["PortalMessage"] = "Receipt cancelled.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Payments));
    }

    // Final-Touches Phase 7 Stage 7.3 — student marks receipt as submitted
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitProof(Guid receiptId, string proofNote, CancellationToken ct)
    {
        try
        {
            await _api.SubmitProofAsync(receiptId, proofNote, ct);
            TempData["PortalMessage"] = "Proof of payment submitted.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Payments));
    }

    // ── Enrollments ────────────────────────────────────────────────────────

    // Final-Touches Phase 8 Stage 8.1+8.2 — student sees own courses; admin sees offering roster + students list
    [HttpGet]
    public async Task<IActionResult> Enrollments(Guid? offeringId, CancellationToken ct)
    {
        ViewData["Title"] = "Enrollments";
        var isStudent = User.IsInRole("Student");
        var model = new EnrollmentsPageModel
        {
            IsConnected        = _api.IsConnected(),
            SelectedOfferingId = offeringId,
            IsStudent          = isStudent,
            Message            = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var sessionId = _api.GetSessionIdentity();
            var isAdmin = sessionId?.IsAdmin == true || sessionId?.IsSuperAdmin == true;

            model.Offerings = sessionId?.IsFaculty == true
                ? await _api.GetCourseOfferingsAsync(null, ct).ConfigureAwait(false)
                : await _api.GetCourseOfferingsAsync(null, ct).ConfigureAwait(false);

            if (isStudent)
            {
                model.MyCourses = await _api.GetMyEnrollmentsAsync(ct);
            }
            else
            {
                if (isAdmin)
                    model.Students = await _api.GetStudentsAsync(null, ct);

                if (offeringId.HasValue)
                    model.Roster = await _api.GetEnrollmentRosterAsync(offeringId.Value, ct);
            }
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // Final-Touches Phase 8 Stage 8.2 — admin enrolls a student
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrollStudent(Guid studentProfileId, Guid courseOfferingId, CancellationToken ct)
    {
        try
        {
            await _api.AdminEnrollStudentAsync(studentProfileId, courseOfferingId, ct);
            TempData["PortalMessage"] = "Student enrolled successfully.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Enrollments), new { offeringId = courseOfferingId });
    }

    // Final-Touches Phase 8 Stage 8.2 — admin drops any enrollment by ID
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminDropEnrollment(Guid enrollmentId, Guid offeringId, CancellationToken ct)
    {
        try
        {
            await _api.AdminDropEnrollmentAsync(enrollmentId, ct);
            TempData["PortalMessage"] = "Enrollment dropped.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Enrollments), new { offeringId });
    }

    // Final-Touches Phase 8 Stage 8.2 — student self-enrolls in a course offering
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StudentEnroll(Guid courseOfferingId, CancellationToken ct)
    {
        try
        {
            await _api.StudentEnrollAsync(courseOfferingId, ct);
            TempData["PortalMessage"] = "Enrolled successfully.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Enrollments));
    }

    // Final-Touches Phase 8 Stage 8.2 — student drops their own enrollment
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> StudentDropEnrollment(Guid courseOfferingId, CancellationToken ct)
    {
        try
        {
            await _api.StudentDropEnrollmentAsync(courseOfferingId, ct);
            TempData["PortalMessage"] = "Course dropped.";
        }
        catch (Exception ex) { TempData["PortalMessage"] = ex.Message; }
        return RedirectToAction(nameof(Enrollments));
    }

    // ── Reports ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ReportCenter(CancellationToken ct)
    {
        ViewData["Title"] = "Report Center";
        var model = new ReportCenterPageModel { IsConnected = _api.IsConnected() };
        if (!model.IsConnected) return View(model);
        try { model.Reports = await _api.GetReportCatalogAsync(ct); }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportAttendance(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        ViewData["Title"] = "Attendance Summary Report";
        var model = new ReportAttendancePageModel
        {
            IsConnected  = _api.IsConnected(),
            SemesterId   = semesterId,
            DepartmentId = departmentId,
            OfferingId   = offeringId,
            StudentId    = studentId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Semesters    = await _api.GetSemestersAsync(ct);
            model.Departments  = await _api.GetDepartmentsAsync(ct);
            model.Offerings    = await _api.GetCourseOfferingsAsync(null, ct);
            if (semesterId.HasValue || departmentId.HasValue || offeringId.HasValue || studentId.HasValue)
                model.Report = await _api.GetAttendanceSummaryReportAsync(semesterId, departmentId, offeringId, studentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportResults(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        ViewData["Title"] = "Result Summary Report";
        var model = new ReportResultsPageModel
        {
            IsConnected  = _api.IsConnected(),
            SemesterId   = semesterId,
            DepartmentId = departmentId,
            OfferingId   = offeringId,
            StudentId    = studentId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Semesters   = await _api.GetSemestersAsync(ct);
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Offerings   = await _api.GetCourseOfferingsAsync(null, ct);
            if (semesterId.HasValue || departmentId.HasValue || offeringId.HasValue || studentId.HasValue)
                model.Report = await _api.GetResultSummaryReportAsync(semesterId, departmentId, offeringId, studentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportGpa(Guid? departmentId, Guid? programId, CancellationToken ct)
    {
        ViewData["Title"] = "GPA & CGPA Report";
        var model = new ReportGpaPageModel
        {
            IsConnected  = _api.IsConnected(),
            DepartmentId = departmentId,
            ProgramId    = programId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Programs    = await _api.GetProgramsAsync(null, ct);
            if (departmentId.HasValue || programId.HasValue)
                model.Report = await _api.GetGpaReportAsync(departmentId, programId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportEnrollment(Guid? semesterId, Guid? departmentId, CancellationToken ct)
    {
        ViewData["Title"] = "Enrollment Summary Report";
        var model = new ReportEnrollmentPageModel
        {
            IsConnected  = _api.IsConnected(),
            SemesterId   = semesterId,
            DepartmentId = departmentId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Semesters   = await _api.GetSemestersAsync(ct);
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Report = await _api.GetEnrollmentSummaryReportAsync(semesterId, departmentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportSemesterResults(Guid? semesterId, Guid? departmentId, CancellationToken ct)
    {
        ViewData["Title"] = "Semester Results Report";
        var model = new ReportSemesterResultsPageModel
        {
            IsConnected  = _api.IsConnected(),
            SemesterId   = semesterId,
            DepartmentId = departmentId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Semesters   = await _api.GetSemestersAsync(ct);
            model.Departments = await _api.GetDepartmentsAsync(ct);

            // API requires a non-empty semesterId; do not query until one is selected.
            if (semesterId.HasValue)
                model.Report = await _api.GetSemesterResultsReportAsync(semesterId.Value, departmentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // Excel export actions — these act as portal-side proxies:
    // they call the API export endpoint, receive the .xlsx bytes, and
    // stream the file directly to the browser. On failure they fall back
    // to the report view with a TempData error message.

    [HttpGet]
    public async Task<IActionResult> ExportAttendanceSummary(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        if (!_api.IsConnected()) return RedirectToAction(nameof(ReportAttendance));
        try
        {
            var bytes = await _api.ExportAttendanceSummaryAsync(semesterId, departmentId, offeringId, studentId, ct);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance-summary.xlsx");
        }
        catch (Exception ex) { TempData["PortalMessage"] = $"Export failed: {ex.Message}"; }
        return RedirectToAction(nameof(ReportAttendance), new { semesterId, departmentId, offeringId, studentId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportResultSummary(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        if (!_api.IsConnected()) return RedirectToAction(nameof(ReportResults));
        try
        {
            var bytes = await _api.ExportResultSummaryAsync(semesterId, departmentId, offeringId, studentId, ct);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "result-summary.xlsx");
        }
        catch (Exception ex) { TempData["PortalMessage"] = $"Export failed: {ex.Message}"; }
        return RedirectToAction(nameof(ReportResults), new { semesterId, departmentId, offeringId, studentId });
    }

    [HttpGet]
    public async Task<IActionResult> ExportGpaReport(Guid? departmentId, Guid? programId, CancellationToken ct)
    {
        if (!_api.IsConnected()) return RedirectToAction(nameof(ReportGpa));
        try
        {
            var bytes = await _api.ExportGpaReportAsync(departmentId, programId, ct);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "gpa-report.xlsx");
        }
        catch (Exception ex) { TempData["PortalMessage"] = $"Export failed: {ex.Message}"; }
        return RedirectToAction(nameof(ReportGpa), new { departmentId, programId });
    }

    // ── Stage 4.2: Additional Reports ─────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> ReportTranscript(Guid? studentProfileId, CancellationToken ct)
    {
        ViewData["Title"] = "Student Transcript";
        var model = new ReportTranscriptPageModel
        {
            IsConnected      = _api.IsConnected(),
            StudentProfileId = studentProfileId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            var students = await _api.GetStudentsAsync(null, ct);
            model.Students = students.Select(s => new LookupItem { Id = s.Id, Name = $"{s.FullName} ({s.RegistrationNumber})" }).ToList();
            if (studentProfileId.HasValue)
                model.Report = await _api.GetStudentTranscriptReportAsync(studentProfileId.Value, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportStudentTranscript(Guid studentProfileId, CancellationToken ct)
    {
        if (!_api.IsConnected()) return RedirectToAction(nameof(ReportTranscript));
        try
        {
            var bytes = await _api.ExportStudentTranscriptAsync(studentProfileId, ct);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "student-transcript.xlsx");
        }
        catch (Exception ex) { TempData["PortalMessage"] = $"Export failed: {ex.Message}"; }
        return RedirectToAction(nameof(ReportTranscript), new { studentProfileId });
    }

    [HttpGet]
    public async Task<IActionResult> ReportLowAttendance(
        decimal threshold = 75m, Guid? departmentId = null, Guid? courseOfferingId = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "Low Attendance Warning";
        var model = new ReportLowAttendancePageModel
        {
            IsConnected      = _api.IsConnected(),
            Threshold        = threshold,
            DepartmentId     = departmentId,
            CourseOfferingId = courseOfferingId
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments     = await _api.GetDepartmentsAsync(ct);
            model.CourseOfferings = await _api.GetCoursesAsync(departmentId, ct);
            model.Report = await _api.GetLowAttendanceReportAsync(threshold, departmentId, courseOfferingId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ReportFypStatus(
        Guid? departmentId = null, string? status = null, CancellationToken ct = default)
    {
        ViewData["Title"] = "FYP Status Report";
        var model = new ReportFypStatusPageModel
        {
            IsConnected    = _api.IsConnected(),
            DepartmentId   = departmentId,
            SelectedStatus = status
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Report = await _api.GetFypStatusReportAsync(departmentId, status, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Dashboard Settings ────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> DashboardSettings(CancellationToken ct)
    {
        var model = new DashboardSettingsPageModel { IsConnected = _api.IsConnected() };
        if (model.IsConnected)
        {
            try { model.Branding = await _api.GetPortalBrandingAsync(ct); }
            catch (Exception ex) { model.Message = ex.Message; }
        }
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DashboardSettings(DashboardSettingsPageModel model, CancellationToken ct)
    {
        if (!_api.IsConnected())
        {
            TempData["Message"] = "Not connected to API.";
            return RedirectToAction(nameof(DashboardSettings));
        }
        try
        {
            // Handle logo file upload if provided
            if (model.LogoFile is { Length: > 0 })
            {
                await using var stream = model.LogoFile.OpenReadStream();
                var logoUrl = await _api.UploadLogoAsync(stream, model.LogoFile.FileName, ct);
                if (logoUrl is not null)
                    model.Branding.LogoImage = logoUrl;
            }

            await _api.SavePortalBrandingAsync(model.Branding, ct);
            TempData["Message"] = "Portal branding saved successfully.";
        }
        catch (Exception ex)
        {
            TempData["Message"] = "Error: " + ex.Message;
        }
        return RedirectToAction(nameof(DashboardSettings));
    }
}
