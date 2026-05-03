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
        return View(model);
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
            model.Departments = await _api.GetDepartmentsAsync(ct);
            model.Courses     = await _api.GetCourseDetailsAsync(departmentId, ct);
            model.Offerings   = await _api.GetCourseOfferingsAsync(departmentId, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
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
                model.Assignments = await _api.GetMyAssignmentsAsync(ct);
            }
            else if (offeringId.HasValue)
            {
                model.Assignments = await _api.GetAssignmentsByOfferingAsync(offeringId.Value, ct);
                if (selectedAssignmentId.HasValue)
                    model.Submissions = await _api.GetSubmissionsForAssignmentAsync(selectedAssignmentId.Value, ct);
            }

            model.CourseOfferings = await _api.GetMyOfferingsAsync(ct);
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
            }

            model.CourseOfferings = await _api.GetMyOfferingsAsync(ct);
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
            if (sessionId?.IsStudent == true)
            {
                model.Results = await _api.GetMyResultsAsync(ct);
            }
            else if (offeringId.HasValue)
            {
                model.Results   = await _api.GetResultsByOfferingAsync(offeringId.Value, ct);
                model.Offerings = await _api.GetMyOfferingsAsync(ct);
            }
            else
            {
                model.Offerings = await _api.GetMyOfferingsAsync(ct);
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
            model.CourseOfferings = await _api.GetMyOfferingsAsync(ct);

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
            model.Departments       = await _api.GetDepartmentsAsync(ct);
            model.UpcomingMeetings  = await _api.GetUpcomingMeetingsAsync(ct);

            var sessionId = _api.GetSessionIdentity();
            if (sessionId?.IsStudent == true)
                model.Projects = await _api.GetMyFypProjectsAsync(ct);
            else if (sessionId?.IsFaculty == true)
                model.Projects = await _api.GetMySupervisedProjectsAsync(ct);
            else if (departmentId.HasValue)
                model.Projects = await _api.GetFypByDepartmentAsync(departmentId.Value, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Analytics ──────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Analytics(CancellationToken ct)
    {
        ViewData["Title"] = "Analytics";
        var model = new AnalyticsPageModel { IsConnected = _api.IsConnected() };
        if (!model.IsConnected) return View(model);
        try
        {
            model.PerformanceJson = await _api.GetPerformanceAnalyticsJsonAsync(ct);
            model.AttendanceJson  = await _api.GetAttendanceAnalyticsJsonAsync(ct);
            model.AssignmentJson  = await _api.GetAssignmentAnalyticsJsonAsync(ct);
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

    [HttpGet]
    public async Task<IActionResult> Payments(Guid? studentId, CancellationToken ct)
    {
        ViewData["Title"] = "Payments";
        var model = new PaymentsPageModel
        {
            IsConnected    = _api.IsConnected(),
            SelectedStudentId = studentId,
            Message        = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Departments = await _api.GetDepartmentsAsync(ct);
            if (studentId.HasValue)
                model.Payments = await _api.GetPaymentsByStudentAsync(studentId.Value, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
    }

    // ── Enrollments ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Enrollments(Guid? offeringId, CancellationToken ct)
    {
        ViewData["Title"] = "Enrollments";
        var model = new EnrollmentsPageModel
        {
            IsConnected      = _api.IsConnected(),
            SelectedOfferingId = offeringId,
            Message          = TempData["PortalMessage"]?.ToString()
        };
        if (!model.IsConnected) return View(model);
        try
        {
            model.Offerings = await _api.GetCourseOfferingsAsync(null, ct);
            if (offeringId.HasValue)
                model.Roster = await _api.GetEnrollmentRosterAsync(offeringId.Value, ct);
        }
        catch (Exception ex) { model.Message = ex.Message; }
        return View(model);
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
}
