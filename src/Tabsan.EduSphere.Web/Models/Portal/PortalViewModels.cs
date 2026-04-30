namespace Tabsan.EduSphere.Web.Models.Portal;

public class ApiConnectionModel
{
    public string ApiBaseUrl { get; set; } = "https://localhost:5001";
    public string AccessToken { get; set; } = string.Empty;
    public Guid? DefaultDepartmentId { get; set; }
}

public class LookupItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class FacultyLookupItem
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}

public class RoomLookupItem
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
}

public class TimetableSummaryItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}

public class TimetableEntryItem
{
    public Guid Id { get; set; }
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public string? BuildingName { get; set; }
    public string? RoomNumber { get; set; }
}

public class TimetableDetailsItem
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public Guid AcademicProgramId { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public Guid SemesterId { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public List<TimetableEntryItem> Entries { get; set; } = new();
}

public class TeacherTimetableEntryItem
{
    public Guid EntryId { get; set; }
    public string TimetableTitle { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string DayName { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string ProgramCode { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public int SemesterNumber { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? BuildingName { get; set; }
    public string? RoomNumber { get; set; }
}

public class CreateTimetableForm
{
    public Guid DepartmentId { get; set; }
    public Guid AcademicProgramId { get; set; }
    public Guid SemesterId { get; set; }
    public int SemesterNumber { get; set; } = 1;
    public DateTime EffectiveDate { get; set; } = DateTime.Today;
}

public class AddTimetableEntryForm
{
    public Guid TimetableId { get; set; }
    public int DayOfWeek { get; set; } = 1;
    public TimeOnly StartTime { get; set; } = new(9, 0);
    public TimeOnly EndTime { get; set; } = new(10, 0);
    public Guid? CourseId { get; set; }
    public Guid? FacultyUserId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? BuildingId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string? FacultyName { get; set; }
    public string? RoomNumber { get; set; }
}

public class TimetableAdminPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public ApiConnectionModel Connection { get; set; } = new();
    public CreateTimetableForm CreateForm { get; set; } = new();
    public AddTimetableEntryForm EntryForm { get; set; } = new();

    public List<LookupItem> Departments { get; set; } = new();
    public List<LookupItem> Programs { get; set; } = new();
    public List<LookupItem> Semesters { get; set; } = new();
    public List<LookupItem> Courses { get; set; } = new();
    public List<FacultyLookupItem> Faculty { get; set; } = new();
    public List<LookupItem> Buildings { get; set; } = new();
    public List<RoomLookupItem> Rooms { get; set; } = new();

    public List<TimetableSummaryItem> Timetables { get; set; } = new();
    public TimetableDetailsItem? SelectedTimetable { get; set; }
}

public class TimetableStudentPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public Guid? DepartmentId { get; set; }
    public List<TimetableSummaryItem> Timetables { get; set; } = new();
    public TimetableDetailsItem? SelectedTimetable { get; set; }
}

public class TimetableTeacherPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public List<TeacherTimetableEntryItem> Entries { get; set; } = new();
}

// =============================================================================
// Building / Room view models
// =============================================================================

public class BuildingItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class RoomItem
{
    public Guid Id { get; set; }
    public Guid BuildingId { get; set; }
    public string BuildingName { get; set; } = string.Empty;
    public string BuildingCode { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public bool IsActive { get; set; }
}

public class BuildingFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

public class RoomFormModel
{
    public Guid BuildingId { get; set; }
    public string Number { get; set; } = string.Empty;
    public int? Capacity { get; set; }
}

public class BuildingsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public List<BuildingItem> Buildings { get; set; } = new();
    public BuildingFormModel CreateForm { get; set; } = new();
    public BuildingItem? SelectedBuilding { get; set; }
    public BuildingFormModel EditForm { get; set; } = new();
}

public class RoomsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public Guid? SelectedBuildingId { get; set; }
    public List<BuildingItem> Buildings { get; set; } = new();
    public List<RoomItem> Rooms { get; set; } = new();
    public RoomFormModel CreateForm { get; set; } = new();
    public RoomItem? SelectedRoom { get; set; }
    public RoomFormModel EditForm { get; set; } = new();
}

// =============================================================================
// Session identity (decoded from JWT on connection save)
// =============================================================================

public class SessionIdentity
{
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();

    public bool IsAdmin => Roles.Contains("Admin") || Roles.Contains("SuperAdmin");
    public bool IsSuperAdmin => Roles.Contains("SuperAdmin");
    public bool IsFaculty => Roles.Contains("Faculty");
    public bool IsStudent => Roles.Contains("Student");
}

// ── Sidebar Settings ──────────────────────────────────────────────────────────

public class SidebarMenuItemWebModel
{
    public Guid   Id            { get; set; }
    public string Key           { get; set; } = string.Empty;
    public string Name          { get; set; } = string.Empty;
    public string Purpose       { get; set; } = string.Empty;
    public Guid?  ParentId      { get; set; }
    public int    DisplayOrder  { get; set; }
    public bool   IsActive      { get; set; }
    public bool   IsSystemMenu  { get; set; }

    /// <summary>Keyed by role name.</summary>
    public Dictionary<string, bool> RoleAccesses { get; set; } = new();
    public List<SidebarMenuItemWebModel> SubMenus { get; set; } = new();
}

public class SidebarSettingsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message  { get; set; }
    public List<SidebarMenuItemWebModel> TopLevelMenus { get; set; } = new();
}
