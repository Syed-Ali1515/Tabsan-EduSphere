using Tabsan.EduSphere.Application.DTOs.Analytics;

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
    public bool MustChangePassword { get; set; }

    public bool IsAdmin => Roles.Contains("Admin") || Roles.Contains("SuperAdmin");
    public bool IsSuperAdmin => Roles.Contains("SuperAdmin");
    public bool IsFaculty => Roles.Contains("Faculty");
    public bool IsStudent => Roles.Contains("Student");
}

public class ForceChangePasswordPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
}

public class StudentProfileSummaryItem
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public string DepartmentName { get; set; } = "";
    public int CurrentSemesterNumber { get; set; }
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

// ── License Update ────────────────────────────────────────────────────────────

public class LicenseUpdatePageModel
{
    public bool    IsConnected   { get; set; }
    public string? Message       { get; set; }
    // Current license details (null when no license is loaded)
    public string? Status        { get; set; }
    public string? LicenseType   { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public DateTime? ExpiresAt   { get; set; }
    public DateTime? UpdatedAt   { get; set; }
    public int?    RemainingDays { get; set; }
}

// ── Theme Settings ────────────────────────────────────────────────────────────

public class ThemeOption
{
    public string Key         { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public string PreviewColor{ get; init; } = "#0d6efd"; // representative accent colour
}

public class ThemeSettingsPageModel
{
    public bool    IsConnected   { get; set; }
    public string? Message       { get; set; }
    public string? CurrentTheme  { get; set; }
    public List<ThemeOption> Themes { get; set; } = new()
    {
        new() { Key = "",              DisplayName = "Default (Bootstrap)",  PreviewColor = "#0d6efd" },
        new() { Key = "ocean_blue",    DisplayName = "Ocean Blue",           PreviewColor = "#0277bd" },
        new() { Key = "emerald_forest",DisplayName = "Emerald Forest",       PreviewColor = "#2e7d32" },
        new() { Key = "sunset_orange", DisplayName = "Sunset Orange",        PreviewColor = "#e64a19" },
        new() { Key = "royal_purple",  DisplayName = "Royal Purple",         PreviewColor = "#6a1b9a" },
        new() { Key = "midnight_dark", DisplayName = "Midnight Dark",        PreviewColor = "#1a1a2e" },
        new() { Key = "rose_gold",     DisplayName = "Rose Gold",            PreviewColor = "#c2185b" },
        new() { Key = "arctic_teal",   DisplayName = "Arctic Teal",          PreviewColor = "#00796b" },
        new() { Key = "sand_dune",     DisplayName = "Sand Dune",            PreviewColor = "#6d4c41" },
        new() { Key = "slate_grey",    DisplayName = "Slate Grey",           PreviewColor = "#455a64" },
        new() { Key = "crimson",       DisplayName = "Crimson",              PreviewColor = "#c62828" },
        new() { Key = "ivory_classic", DisplayName = "Ivory Classic",        PreviewColor = "#5d4037" },
        new() { Key = "cobalt_night",  DisplayName = "Cobalt Night",         PreviewColor = "#1565c0" },
        new() { Key = "olive_grove",   DisplayName = "Olive Grove",          PreviewColor = "#558b2f" },
        new() { Key = "cosmic_violet", DisplayName = "Cosmic Violet",        PreviewColor = "#7b1fa2" },
        // Final-Touches Phase 5 Stage 5.4 — 5 new themes added below
        new() { Key = "steel_blue",    DisplayName = "Steel Blue",           PreviewColor = "#2b6cb0" },
        new() { Key = "forest_green",  DisplayName = "Forest Green",         PreviewColor = "#15803d" },
        new() { Key = "amber_gold",    DisplayName = "Amber Gold",           PreviewColor = "#b45309" },
        new() { Key = "warm_copper",   DisplayName = "Warm Copper",          PreviewColor = "#c2410c" },
        new() { Key = "indigo_dusk",   DisplayName = "Indigo Dusk",          PreviewColor = "#4f46e5" },
        // P1-S6-01 — 10 additional themes
        new() { Key = "neon_mint",      DisplayName = "Neon Mint",           PreviewColor = "#059669" },
        new() { Key = "sakura_pink",    DisplayName = "Sakura Pink",         PreviewColor = "#db2777" },
        new() { Key = "golden_hour",    DisplayName = "Golden Hour",         PreviewColor = "#d97706" },
        new() { Key = "deep_navy",      DisplayName = "Deep Navy",           PreviewColor = "#0284c7" },
        new() { Key = "lavender_mist",  DisplayName = "Lavender Mist",       PreviewColor = "#7c3aed" },
        new() { Key = "rust_canyon",    DisplayName = "Rust Canyon",         PreviewColor = "#ea580c" },
        new() { Key = "glacier_ice",    DisplayName = "Glacier Ice",         PreviewColor = "#0891b2" },
        new() { Key = "graphite_pro",   DisplayName = "Graphite Pro",        PreviewColor = "#52525b" },
        new() { Key = "spring_blossom", DisplayName = "Spring Blossom",      PreviewColor = "#16a34a" },
        new() { Key = "dusk_fire",      DisplayName = "Dusk Fire",           PreviewColor = "#c2410c" },
    };
}

// ── Report Settings ───────────────────────────────────────────────────────────

public class ReportDefinitionWebModel
{
    public Guid          Id            { get; set; }
    public string        Key           { get; set; } = "";
    public string        Name          { get; set; } = "";
    public string        Purpose       { get; set; } = "";
    public bool          IsActive      { get; set; }
    public List<string>  AssignedRoles { get; set; } = new();
}

public class ReportSettingsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message  { get; set; }
    public List<ReportDefinitionWebModel> Reports { get; set; } = new();
}

public class CreateReportForm
{
    public string Key     { get; set; } = "";
    public string Name    { get; set; } = "";
    public string Purpose { get; set; } = "";
}

// ── Module Settings ───────────────────────────────────────────────────────────

public class ModuleSettingsWebModel
{
    public Guid         Id            { get; set; }
    public string       Key           { get; set; } = "";
    public string       Name          { get; set; } = "";
    public bool         IsMandatory   { get; set; }
    public bool         IsActive      { get; set; }
    public List<string> AssignedRoles { get; set; } = new();
}

public class ModuleSettingsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message  { get; set; }
    public List<ModuleSettingsWebModel> Modules { get; set; } = new();
}

// ── Result Calculation ───────────────────────────────────────────────────────

public class ResultCalculationGpaRuleItem
{
    public Guid? Id { get; set; }
    public decimal GradePoint { get; set; }
    public decimal MinimumScore { get; set; }
    public int DisplayOrder { get; set; }
}

public class ResultCalculationComponentRuleItem
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Weightage { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ResultCalculationSettingsPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public List<ResultCalculationGpaRuleItem> GpaRules { get; set; } = new();
    public List<ResultCalculationComponentRuleItem> ComponentRules { get; set; } = new();
}

// ── Notifications ─────────────────────────────────────────────────────────────

public class NotificationItem
{
    public Guid     Id               { get; set; }
    public string   Title            { get; set; } = "";
    public string?  Body             { get; set; }
    public string   NotificationType { get; set; } = "";
    public bool     IsRead           { get; set; }
    public DateTime CreatedAt        { get; set; }
}

public class NotificationsPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public List<NotificationItem> Notifications { get; set; } = new();
    public int    UnreadCount  { get; set; }
}

// ── Students ──────────────────────────────────────────────────────────────────

public class StudentItem
{
    public Guid   Id                 { get; set; }
    public string RegistrationNumber { get; set; } = "";
    public string FullName           { get; set; } = "";
    public string? Email             { get; set; }
    public string DepartmentName     { get; set; } = "";
    public string ProgramName        { get; set; } = "";
    public int    SemesterNumber     { get; set; }
    public string Status             { get; set; } = "Active";
}

public class StudentsPageModel
{
    public bool   IsConnected          { get; set; }
    public string? Message             { get; set; }
    public List<StudentItem>  Students    { get; set; } = new();
    public List<LookupItem>   Departments { get; set; } = new();
    public Guid?  SelectedDepartmentId { get; set; }
}

// ── Departments ───────────────────────────────────────────────────────────────

public class DepartmentItem
{
    public Guid   Id       { get; set; }
    public string Name     { get; set; } = "";
    public string Code     { get; set; } = "";
    public bool   IsActive { get; set; }
}

public class AdminUserLookupItem
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = "";
    public string? Email { get; set; }
    public bool IsActive { get; set; }
}

public class DepartmentsPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<DepartmentItem> Departments { get; set; } = new();
    public List<AdminUserLookupItem> AdminUsers { get; set; } = new();
    public Guid? SelectedAdminUserId { get; set; }
    public List<Guid> AssignedDepartmentIds { get; set; } = new();
}

public class AdminUsersPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public List<AdminUserLookupItem> AdminUsers { get; set; } = new();
    public List<DepartmentItem> Departments { get; set; } = new();
    public Guid? SelectedAdminUserId { get; set; }
    public List<Guid> AssignedDepartmentIds { get; set; } = new();
}

public class UserImportResultItem
{
    public int TotalRows { get; set; }
    public int Imported { get; set; }
    public int Duplicates { get; set; }
    public int Errors { get; set; }
    public List<string> ErrorDetails { get; set; } = new();
}

public class UserImportPageModel
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public UserImportResultItem? Result { get; set; }
}

// ── Courses ───────────────────────────────────────────────────────────────────

public class CourseItem
{
    public Guid   Id             { get; set; }
    public string Title          { get; set; } = "";
    public string Code           { get; set; } = "";
    public string DepartmentName { get; set; } = "";
    public int    CreditHours    { get; set; }
}

public class CourseOfferingItem
{
    public Guid   Id          { get; set; }
    public string CourseTitle { get; set; } = "";
    public string CourseCode  { get; set; } = "";
    public string FacultyName { get; set; } = "";
    public string SemesterName{ get; set; } = "";
    public bool   IsActive    { get; set; }
}

public class CoursesPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<CourseItem>         Courses     { get; set; } = new();
    public List<CourseOfferingItem> Offerings   { get; set; } = new();
    public List<LookupItem>         Departments { get; set; } = new();
    public List<LookupItem>         Semesters   { get; set; } = new();
    public List<FacultyLookupItem>  Faculty     { get; set; } = new();
    public Guid?  SelectedDepartmentId { get; set; }
}

// ── Assignments ───────────────────────────────────────────────────────────────

public class AssignmentItem
{
    public Guid      Id                   { get; set; }
    public string    Title                { get; set; } = "";
    public string?   Description          { get; set; }
    public DateTime? DueDate              { get; set; }
    public decimal   TotalMarks           { get; set; }
    public bool      IsPublished          { get; set; }
    public string    CourseOfferingTitle  { get; set; } = "";
    public int       SubmissionCount      { get; set; }
    // For student view
    public bool      IsSubmitted          { get; set; }
    public decimal?  MarksAwarded         { get; set; }
}

public class SubmissionItem
{
    public Guid     Id                   { get; set; }
    public string   StudentName          { get; set; } = "";
    public string   RegistrationNumber   { get; set; } = "";
    public string?  Comments             { get; set; }
    public DateTime SubmittedAt          { get; set; }
    public bool     IsGraded             { get; set; }
    public decimal? MarksAwarded         { get; set; }
    public string?  FeedbackFromFaculty  { get; set; }
}

public class MyAssignmentSubmissionItem
{
    public Guid      AssignmentId   { get; set; }
    public string    Status         { get; set; } = "";
    public decimal?  MarksAwarded   { get; set; }
    public DateTime  SubmittedAt    { get; set; }
}

public class AssignmentsPageModel
{
    public bool   IsConnected          { get; set; }
    public string? Message             { get; set; }
    public List<AssignmentItem>  Assignments     { get; set; } = new();
    public List<SubmissionItem>  Submissions     { get; set; } = new();
    public List<LookupItem>      CourseOfferings { get; set; } = new();
    public List<LookupItem>      SemesterOptions { get; set; } = new();
    public Guid?  SelectedAssignmentId  { get; set; }
    public Guid?  SelectedOfferingId   { get; set; }
    public string? SelectedSemesterName { get; set; }
}

// ── Attendance ────────────────────────────────────────────────────────────────

public class AttendanceRecordItem
{
    public Guid     Id                { get; set; }
    public Guid     StudentProfileId  { get; set; }
    public string   StudentName       { get; set; } = "";
    public string   RegistrationNumber { get; set; } = "";
    public DateTime Date              { get; set; }
    public string   Status            { get; set; } = ""; // Present / Absent / Late / Excused
    public bool     IsCorrected       { get; set; }
}

public class AttendanceSummaryItem
{
    public Guid   StudentId            { get; set; }
    public string StudentName          { get; set; } = "";
    public string RegistrationNumber   { get; set; } = "";
    public string CourseName           { get; set; } = "";
    public int    TotalClasses         { get; set; }
    public int    PresentCount         { get; set; }
    public double AttendancePercentage { get; set; }
}

public class AttendancePageModel
{
    public bool   IsConnected        { get; set; }
    public string? Message           { get; set; }
    public List<AttendanceRecordItem>  Records         { get; set; } = new();
    public List<AttendanceSummaryItem> Summary         { get; set; } = new();
    public List<LookupItem>            CourseOfferings { get; set; } = new();
    public List<EnrollmentRosterItem>  Roster          { get; set; } = new();
    public Guid?  SelectedOfferingId { get; set; }
}

// ── Results ───────────────────────────────────────────────────────────────────

public class ResultItem
{
    public Guid   Id                 { get; set; }
    // Used by the per-row Promote button in the Results table to identify the student.
    public Guid   StudentProfileId   { get; set; }
    public string ResultType         { get; set; } = "";
    public string CourseName         { get; set; } = "";
    public string CourseCode         { get; set; } = "";
    public decimal? MarksObtained    { get; set; }
    public int    TotalMarks         { get; set; }
    public string? LetterGrade       { get; set; }
    public bool   IsPublished        { get; set; }
    public string SemesterName       { get; set; } = "";
    public string StudentName        { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
}

public class ResultsPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<ResultItem>              Results  { get; set; } = new();
    public List<LookupItem>              Offerings { get; set; } = new();
    public List<LookupItem>              SemesterOptions { get; set; } = new();
    public List<EnrollmentRosterItem>    Roster    { get; set; } = new();
    public double? Cgpa       { get; set; }
    public Guid?  SelectedOfferingId { get; set; }
    public string? SelectedSemesterName { get; set; }
}

// ── Quizzes ───────────────────────────────────────────────────────────────────

public class QuizItem
{
    public Guid      Id                   { get; set; }
    public string    Title                { get; set; } = "";
    public string?   Description          { get; set; }
    public bool      IsPublished          { get; set; }
    public bool      IsActive             { get; set; }
    public DateTime? AvailableFrom        { get; set; }
    public DateTime? AvailableTo          { get; set; }
    public int       MaxAttempts          { get; set; }
    public int?      TimeLimitMinutes     { get; set; }
    public int       QuestionCount        { get; set; }
    public string    CourseOfferingTitle  { get; set; } = "";
    // Student view extras
    public int       MyAttemptCount       { get; set; }
    public int?      BestScore            { get; set; }
}

public class QuizAttemptItem
{
    public Guid      Id           { get; set; }
    public string    QuizTitle    { get; set; } = "";
    public DateTime  StartedAt    { get; set; }
    public DateTime? SubmittedAt  { get; set; }
    public string    Status       { get; set; } = "";
    public decimal?  TotalScore   { get; set; }
    public int       MaxScore     { get; set; }
}

public class QuizzesPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<QuizItem>        Quizzes        { get; set; } = new();
    public List<QuizAttemptItem> MyAttempts     { get; set; } = new();
    public List<LookupItem>      CourseOfferings { get; set; } = new();
    public List<LookupItem>      SemesterOptions { get; set; } = new();
    public Guid?  SelectedOfferingId { get; set; }
    public string? SelectedSemesterName { get; set; }
}

// ── FYP ───────────────────────────────────────────────────────────────────────

public class FypProjectItem
{
    public Guid   Id             { get; set; }
    public string Title          { get; set; } = "";
    public string? Description   { get; set; }
    public string Status         { get; set; } = "";
    public string StudentName    { get; set; } = "";
    public string? SupervisorName{ get; set; }
    public string DepartmentName { get; set; } = "";
    public int    MeetingCount   { get; set; }
    public bool   IsCompletionRequested { get; set; }
    public int    CompletionApprovalCount { get; set; }
    public int    RequiredApprovalCount { get; set; }
    public List<Guid> CompletionApprovedByUserIds { get; set; } = new();
}

public class FypMeetingItem
{
    public Guid     Id          { get; set; }
    public string   Title       { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public string   Status      { get; set; } = "";
    public string?  Location    { get; set; }
    public string?  Notes       { get; set; }
    public string   ProjectTitle { get; set; } = "";
}

public class FypPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<FypProjectItem>    Projects         { get; set; } = new();
    public List<FypMeetingItem>    UpcomingMeetings { get; set; } = new();
    public List<LookupItem>        Departments      { get; set; } = new();
    public List<FacultyLookupItem> Faculty          { get; set; } = new();
    public List<StudentItem>       Students         { get; set; } = new();
    public Guid?  SelectedDepartmentId { get; set; }
}

// ── Analytics ─────────────────────────────────────────────────────────────────

public class AnalyticsSummaryCard
{
    public string  Label      { get; set; } = "";
    public string  Value      { get; set; } = "";
    public string? SubText    { get; set; }
    public string  ColorClass { get; set; } = "text-primary";
    public string  Icon       { get; set; } = "📊";
}

// Final-Touches Phase 6 Stage 6.2 — replaced JSON string fields with typed DTO properties
public class AnalyticsPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<AnalyticsSummaryCard>   Cards       { get; set; } = new();
    public DepartmentPerformanceReport? Performance { get; set; }
    public DepartmentAttendanceReport?  Attendance  { get; set; }
    public AssignmentStatsReport?       Assignments { get; set; }
}

// ── AI Chat ───────────────────────────────────────────────────────────────────

public class AiChatMessageItem
{
    public Guid     Id        { get; set; }
    public string   Role      { get; set; } = "user"; // "user" | "assistant"
    public string   Content   { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class AiChatConversationItem
{
    public Guid      Id            { get; set; }
    public string    Title         { get; set; } = "";
    public DateTime  CreatedAt     { get; set; }
    public DateTime? LastMessageAt { get; set; }
}

public class AiChatPageModel
{
    public bool   IsConnected          { get; set; }
    public string? Message             { get; set; }
    public List<AiChatConversationItem> Conversations     { get; set; } = new();
    public List<AiChatMessageItem>      CurrentMessages   { get; set; } = new();
    public Guid?  ActiveConversationId { get; set; }
}

// ── Student Lifecycle ─────────────────────────────────────────────────────────

public class GraduationCandidateItem
{
    public Guid   Id                 { get; set; }
    public string FullName           { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string ProgramName        { get; set; } = "";
    public int    SemesterNumber     { get; set; }
    public double? Cgpa              { get; set; }
}

public class StudentLifecyclePageModel
{
    public bool   IsConnected          { get; set; }
    public string? Message             { get; set; }
    public List<GraduationCandidateItem> GraduationCandidates { get; set; } = new();
    public List<StudentItem>             StudentsBySemester   { get; set; } = new();
    public List<LookupItem>              Departments          { get; set; } = new();
    public Guid?  SelectedDepartmentId  { get; set; }
    public int    SelectedSemester      { get; set; } = 1;
}

// ── Payments ──────────────────────────────────────────────────────────────────

public class PaymentReceiptItem
{
    public Guid     Id                 { get; set; }
    public Guid     StudentProfileId   { get; set; }
    public string   StudentName        { get; set; } = "";
    public string   RegistrationNumber { get; set; } = "";
    public decimal  Amount             { get; set; }
    public string   FeeType            { get; set; } = "";
    public string   Status             { get; set; } = "";
    public DateTime DueDate            { get; set; }
    public DateTime? PaidDate          { get; set; }
    public string?  ProofOfPaymentPath { get; set; }
    public string?  Notes              { get; set; }
}

// Final-Touches Phase 7 Stage 7.2 — form for creating a new receipt
public class CreatePaymentForm
{
    public Guid     StudentProfileId { get; set; }
    public decimal  Amount           { get; set; }
    public string   Description      { get; set; } = "";
    public DateTime DueDate          { get; set; } = DateTime.Today.AddDays(30);
}

public class PaymentsPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<PaymentReceiptItem> Payments    { get; set; } = new();
    public List<LookupItem>         Departments { get; set; } = new();
    public List<StudentItem>         Students    { get; set; } = new();
    public Guid?  SelectedStudentId { get; set; }
    public CreatePaymentForm CreateForm { get; set; } = new();
}

// ── Enrollments ───────────────────────────────────────────────────────────────

public class EnrollmentRosterItem
{
    public Guid   Id                 { get; set; }
    public string StudentName        { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string ProgramName        { get; set; } = "";
    public int    SemesterNumber     { get; set; }
}

// Final-Touches Phase 8 Stage 8.1 — student's own enrolled courses list
public class MyEnrollmentItem
{
    public Guid     EnrollmentId     { get; set; }
    public Guid     CourseOfferingId { get; set; }
    public string   CourseCode       { get; set; } = "";
    public string   CourseTitle      { get; set; } = "";
    public string   SemesterName     { get; set; } = "";
    public string   Status           { get; set; } = "";
    public DateTime EnrolledAt       { get; set; }
}

// Final-Touches Phase 8 Stage 8.2 — expanded with student list and student own-courses view
public class EnrollmentsPageModel
{
    public bool   IsConnected        { get; set; }
    public string? Message           { get; set; }
    public bool   IsStudent          { get; set; }
    public List<EnrollmentRosterItem> Roster         { get; set; } = new();
    public List<CourseOfferingItem>   Offerings      { get; set; } = new();
    public Guid?  SelectedOfferingId { get; set; }
    public List<StudentItem>          Students       { get; set; } = new();
    public List<MyEnrollmentItem>     MyCourses      { get; set; } = new();
}

// ── Phase 12: Reports ─────────────────────────────────────────────────────────

public class ReportCatalogItem
{
    public Guid         Id           { get; set; }
    public string       Key          { get; set; } = "";
    public string       Name         { get; set; } = "";
    public string       Purpose      { get; set; } = "";
    public bool         IsActive     { get; set; }
    public List<string> AllowedRoles { get; set; } = new();
}

public class ReportCenterPageModel
{
    public bool   IsConnected { get; set; }
    public string? Message    { get; set; }
    public List<ReportCatalogItem> Reports { get; set; } = new();
}

// Row types returned by EduApiClient for each report
public class AttendanceSummaryRowItem
{
    public string  RegistrationNumber   { get; set; } = "";
    public string  StudentName          { get; set; } = "";
    public string  CourseCode           { get; set; } = "";
    public string  CourseTitle          { get; set; } = "";
    public int     TotalSessions        { get; set; }
    public int     AttendedSessions     { get; set; }
    public decimal AttendancePercentage { get; set; }
}

public class AttendanceSummaryWebModel
{
    public int                          TotalStudents { get; set; }
    public DateTime                     GeneratedAt   { get; set; }
    public List<AttendanceSummaryRowItem> Rows         { get; set; } = new();
}

public class ReportAttendancePageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public Guid?  OfferingId   { get; set; }
    public Guid?  StudentId    { get; set; }
    public List<LookupItem>          Semesters   { get; set; } = new();
    public List<LookupItem>          Departments { get; set; } = new();
    public List<CourseOfferingItem>  Offerings   { get; set; } = new();
    public AttendanceSummaryWebModel? Report      { get; set; }
}

public class ResultSummaryRowItem
{
    public string   RegistrationNumber { get; set; } = "";
    public string   StudentName        { get; set; } = "";
    public string   CourseCode         { get; set; } = "";
    public string   CourseTitle        { get; set; } = "";
    public string   ResultType         { get; set; } = "";
    public decimal  MarksObtained      { get; set; }
    public decimal  MaxMarks           { get; set; }
    public decimal  Percentage         { get; set; }
    public DateTime? PublishedAt       { get; set; }
}

public class ResultSummaryWebModel
{
    public int                        TotalRecords { get; set; }
    public DateTime                   GeneratedAt  { get; set; }
    public List<ResultSummaryRowItem> Rows         { get; set; } = new();
}

public class ReportResultsPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public Guid?  OfferingId   { get; set; }
    public Guid?  StudentId    { get; set; }
    public List<LookupItem>         Semesters   { get; set; } = new();
    public List<LookupItem>         Departments { get; set; } = new();
    public List<CourseOfferingItem> Offerings   { get; set; } = new();
    public ResultSummaryWebModel?   Report      { get; set; }
}

public class AssignmentSummaryRowItem
{
    public string   RegistrationNumber { get; set; } = "";
    public string   StudentName        { get; set; } = "";
    public string   CourseCode         { get; set; } = "";
    public string   CourseTitle        { get; set; } = "";
    public string   AssignmentTitle    { get; set; } = "";
    public DateTime DueDate            { get; set; }
    public DateTime SubmittedAt        { get; set; }
    public string   Status             { get; set; } = "";
    public decimal? MarksAwarded       { get; set; }
}

public class AssignmentSummaryWebModel
{
    public int                            TotalSubmissions { get; set; }
    public DateTime                       GeneratedAt      { get; set; }
    public List<AssignmentSummaryRowItem> Rows             { get; set; } = new();
}

public class ReportAssignmentsPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public Guid?  OfferingId   { get; set; }
    public Guid?  StudentId    { get; set; }
    public List<LookupItem>         Semesters   { get; set; } = new();
    public List<LookupItem>         Departments { get; set; } = new();
    public List<CourseOfferingItem> Offerings   { get; set; } = new();
    public AssignmentSummaryWebModel? Report    { get; set; }
}

public class QuizSummaryRowItem
{
    public string   RegistrationNumber { get; set; } = "";
    public string   StudentName        { get; set; } = "";
    public string   CourseCode         { get; set; } = "";
    public string   CourseTitle        { get; set; } = "";
    public string   QuizTitle          { get; set; } = "";
    public DateTime StartedAt          { get; set; }
    public DateTime? FinishedAt        { get; set; }
    public string   AttemptStatus      { get; set; } = "";
    public decimal? TotalScore         { get; set; }
}

public class QuizSummaryWebModel
{
    public int                      TotalAttempts { get; set; }
    public DateTime                 GeneratedAt   { get; set; }
    public List<QuizSummaryRowItem> Rows          { get; set; } = new();
}

public class ReportQuizzesPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public Guid?  OfferingId   { get; set; }
    public Guid?  StudentId    { get; set; }
    public List<LookupItem>         Semesters   { get; set; } = new();
    public List<LookupItem>         Departments { get; set; } = new();
    public List<CourseOfferingItem> Offerings   { get; set; } = new();
    public QuizSummaryWebModel?     Report      { get; set; }
}

public class GpaReportRowItem
{
    public string  RegistrationNumber { get; set; } = "";
    public string  StudentName        { get; set; } = "";
    public string  ProgramName        { get; set; } = "";
    public string  DepartmentName     { get; set; } = "";
    public int     CurrentSemester    { get; set; }
    public decimal Cgpa               { get; set; }
    public decimal CurrentSemesterGpa { get; set; }
}

public class GpaReportWebModel
{
    public decimal               AverageCgpa   { get; set; }
    public int                   TotalStudents { get; set; }
    public DateTime              GeneratedAt   { get; set; }
    public List<GpaReportRowItem> Rows         { get; set; } = new();
}

public class ReportGpaPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  DepartmentId { get; set; }
    public Guid?  ProgramId    { get; set; }
    public List<LookupItem>   Departments { get; set; } = new();
    public List<LookupItem>   Programs    { get; set; } = new();
    public GpaReportWebModel? Report      { get; set; }
}

public class EnrollmentSummaryRowItem
{
    public string CourseCode     { get; set; } = "";
    public string CourseTitle    { get; set; } = "";
    public string SemesterName   { get; set; } = "";
    public int    MaxEnrollment  { get; set; }
    public int    EnrolledCount  { get; set; }
    public int    AvailableSeats { get; set; }
}

public class EnrollmentSummaryWebModel
{
    public int                            TotalOfferings { get; set; }
    public DateTime                       GeneratedAt    { get; set; }
    public List<EnrollmentSummaryRowItem> Rows           { get; set; } = new();
}

public class ReportEnrollmentPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public List<LookupItem>          Semesters   { get; set; } = new();
    public List<LookupItem>          Departments { get; set; } = new();
    public EnrollmentSummaryWebModel? Report      { get; set; }
}


// ── Semester Results Report ──────────────────────────────────────────────────

// One row in the semester results report: a student's result for a single course.
public class SemesterResultsRowItem
{
    public string  RegistrationNumber { get; set; } = "";
    public string  StudentName        { get; set; } = "";
    public string  CourseCode         { get; set; } = "";
    public string  CourseTitle        { get; set; } = "";
    public string  ResultType         { get; set; } = "";
    public decimal MarksObtained      { get; set; }
    public decimal MaxMarks           { get; set; }
    public decimal Percentage         { get; set; }
}

// Aggregated report returned by GET api/v1/reports/semester-results.
// TotalStudents is the count of distinct students (not the number of result rows).
public class SemesterResultsWebModel
{
    public int                           TotalStudents { get; set; }
    public DateTime                      GeneratedAt   { get; set; }
    public List<SemesterResultsRowItem>  Rows          { get; set; } = new();
}

// Page model for the ReportSemesterResults view.
// SemesterId is treated as required by the action — the report is only fetched
// when a semester is selected; otherwise only the filter dropdowns are rendered.
public class ReportSemesterResultsPageModel
{
    public bool   IsConnected  { get; set; }
    public string? Message     { get; set; }
    public Guid?  SemesterId   { get; set; }
    public Guid?  DepartmentId { get; set; }
    public List<LookupItem> Semesters   { get; set; } = new();
    public List<LookupItem> Departments { get; set; } = new();
    public SemesterResultsWebModel? Report { get; set; }
}

// ── Stage 4.2: Additional Report Web Models ───────────────────────────────────

public class TranscriptRowItem
{
    public string    CourseCode    { get; set; } = "";
    public string    CourseTitle   { get; set; } = "";
    public string    SemesterName  { get; set; } = "";
    public string    ResultType    { get; set; } = "";
    public decimal   MarksObtained { get; set; }
    public decimal   MaxMarks      { get; set; }
    public decimal   Percentage    { get; set; }
    public decimal?  GradePoint    { get; set; }
    public DateTime? PublishedAt   { get; set; }
}
public class TranscriptWebModel
{
    public Guid     StudentProfileId   { get; set; }
    public string   RegistrationNumber { get; set; } = "";
    public string   StudentName        { get; set; } = "";
    public string   ProgramName        { get; set; } = "";
    public string   DepartmentName     { get; set; } = "";
    public decimal  Cgpa               { get; set; }
    public DateTime GeneratedAt        { get; set; }
    public List<TranscriptRowItem> Rows { get; set; } = new();
}
public class ReportTranscriptPageModel
{
    public bool             IsConnected      { get; set; }
    public string?          Message          { get; set; }
    public List<LookupItem> Students         { get; set; } = new();
    public Guid?            StudentProfileId { get; set; }
    public TranscriptWebModel? Report        { get; set; }
}

public class LowAttendanceRowItem
{
    public string  RegistrationNumber   { get; set; } = "";
    public string  StudentName          { get; set; } = "";
    public string  CourseCode           { get; set; } = "";
    public string  CourseTitle          { get; set; } = "";
    public string  SemesterName         { get; set; } = "";
    public string  DepartmentName       { get; set; } = "";
    public int     TotalSessions        { get; set; }
    public int     AttendedSessions     { get; set; }
    public decimal AttendancePercentage { get; set; }
}
public class LowAttendanceWebModel
{
    public decimal  ThresholdPercent    { get; set; }
    public int      TotalStudentsAtRisk { get; set; }
    public DateTime GeneratedAt         { get; set; }
    public List<LowAttendanceRowItem> Rows { get; set; } = new();
}
public class ReportLowAttendancePageModel
{
    public bool             IsConnected      { get; set; }
    public string?          Message          { get; set; }
    public List<LookupItem> Departments      { get; set; } = new();
    public List<LookupItem> CourseOfferings  { get; set; } = new();
    public decimal          Threshold        { get; set; } = 75m;
    public Guid?            DepartmentId     { get; set; }
    public Guid?            CourseOfferingId { get; set; }
    public LowAttendanceWebModel? Report     { get; set; }
}

public class FypStatusRowItem
{
    public string    Title              { get; set; } = "";
    public string    StudentName        { get; set; } = "";
    public string    RegistrationNumber { get; set; } = "";
    public string    DepartmentName     { get; set; } = "";
    public string?   SupervisorName     { get; set; }
    public string    Status             { get; set; } = "";
    public DateTime  ProposedAt         { get; set; }
    public int       MeetingCount       { get; set; }
}
public class FypStatusWebModel
{
    public int      TotalProjects { get; set; }
    public DateTime GeneratedAt   { get; set; }
    public List<FypStatusRowItem> Rows { get; set; } = new();
}
public class ReportFypStatusPageModel
{
    public bool             IsConnected    { get; set; }
    public string?          Message        { get; set; }
    public List<LookupItem> Departments    { get; set; } = new();
    public string?          SelectedStatus { get; set; }
    public Guid?            DepartmentId   { get; set; }
    public FypStatusWebModel? Report       { get; set; }
}

// ── Dashboard Settings ────────────────────────────────────────────────────────

public class PortalBrandingWebModel
{
    public string UniversityName  { get; set; } = "Tabsan EduSphere";
    public string PortalSubtitle  { get; set; } = "Campus Portal";
    public string FooterText      { get; set; } = "© 2026 Tabsan EduSphere";
    public string? LogoImage      { get; set; }
    public string? PrivacyPolicyUrl { get; set; }
    public string? PrivacyPolicyContent { get; set; }
    public string? FontFamily     { get; set; }
    public string? FontSize       { get; set; }
}

public class DashboardSettingsPageModel
{
    public bool   IsConnected      { get; set; }
    public string? Message         { get; set; }
    public PortalBrandingWebModel  Branding { get; set; } = new();
    public Microsoft.AspNetCore.Http.IFormFile? LogoFile { get; set; }
}
