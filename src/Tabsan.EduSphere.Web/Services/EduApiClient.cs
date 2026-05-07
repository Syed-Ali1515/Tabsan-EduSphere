using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tabsan.EduSphere.Application.DTOs.Analytics;
using Tabsan.EduSphere.Web.Models.Portal;

namespace Tabsan.EduSphere.Web.Services;

public interface IEduApiClient
{
    bool IsConnected();
    bool IsForcePasswordChangeRequired();
    void SetForcePasswordChangeRequired(bool required);
    ApiConnectionModel GetConnection();
    void SaveConnection(ApiConnectionModel model);
    SessionIdentity? GetSessionIdentity();
    Task ForceChangePasswordAsync(string newPassword, CancellationToken ct);
    Task<StudentProfileSummaryItem?> GetMyStudentProfileAsync(CancellationToken ct);

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

    // Result Calculation
    Task<ResultCalculationSettingsPageModel> GetResultCalculationSettingsAsync(CancellationToken ct);
    Task SaveResultCalculationSettingsAsync(ResultCalculationSettingsPageModel model, CancellationToken ct);

    // Phase 12: Reports
    Task<List<ReportCatalogItem>> GetReportCatalogAsync(CancellationToken ct);
    Task<AttendanceSummaryWebModel?> GetAttendanceSummaryReportAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<ResultSummaryWebModel?> GetResultSummaryReportAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<AssignmentSummaryWebModel?> GetAssignmentSummaryReportAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<QuizSummaryWebModel?> GetQuizSummaryReportAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<GpaReportWebModel?> GetGpaReportAsync(Guid? departmentId, Guid? programId, CancellationToken ct);
    Task<EnrollmentSummaryWebModel?> GetEnrollmentSummaryReportAsync(Guid? semesterId, Guid? departmentId, CancellationToken ct);
    Task<SemesterResultsWebModel?> GetSemesterResultsReportAsync(Guid semesterId, Guid? departmentId, CancellationToken ct);
    Task<byte[]> ExportAttendanceSummaryAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportResultSummaryAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportAttendanceSummaryCsvAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportAttendanceSummaryPdfAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportResultSummaryCsvAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportResultSummaryPdfAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportAssignmentSummaryAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportQuizSummaryAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportAssignmentSummaryCsvAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportAssignmentSummaryPdfAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportQuizSummaryCsvAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportQuizSummaryPdfAsync(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct);
    Task<byte[]> ExportGpaReportAsync(Guid? departmentId, Guid? programId, CancellationToken ct);

    // Stage 4.2: Additional Reports
    Task<TranscriptWebModel?> GetStudentTranscriptReportAsync(Guid studentProfileId, CancellationToken ct);
    Task<LowAttendanceWebModel?> GetLowAttendanceReportAsync(decimal threshold, Guid? departmentId, Guid? courseOfferingId, CancellationToken ct);
    Task<FypStatusWebModel?> GetFypStatusReportAsync(Guid? departmentId, string? status, CancellationToken ct);
    Task<byte[]> ExportStudentTranscriptAsync(Guid studentProfileId, CancellationToken ct);

    // Notifications
    Task<List<NotificationItem>> GetNotificationsAsync(CancellationToken ct);
    Task<int> GetUnreadNotificationCountAsync(CancellationToken ct);
    Task MarkNotificationReadAsync(Guid id, CancellationToken ct);
    Task MarkAllNotificationsReadAsync(CancellationToken ct);

    // Students
    Task<List<StudentItem>> GetStudentsAsync(Guid? departmentId, CancellationToken ct);

    // Departments
    Task<List<DepartmentItem>> GetDepartmentDetailsAsync(CancellationToken ct);
    Task CreateDepartmentAsync(string name, string code, CancellationToken ct);
    Task UpdateDepartmentAsync(Guid id, string newName, CancellationToken ct);
    Task DeactivateDepartmentAsync(Guid id, CancellationToken ct);
    Task<UserImportResultItem> ImportUsersCsvAsync(Stream fileStream, string fileName, CancellationToken ct);
    Task<List<AdminUserLookupItem>> GetAdminUsersAsync(CancellationToken ct);
    Task<Guid> CreateAdminUserAsync(string username, string? email, string password, CancellationToken ct);
    Task UpdateAdminUserAsync(Guid userId, string? email, bool isActive, string? newPassword, CancellationToken ct);
    Task<List<Guid>> GetAdminDepartmentIdsAsync(Guid adminUserId, CancellationToken ct);
    Task AssignAdminToDepartmentAsync(Guid adminUserId, Guid departmentId, CancellationToken ct);
    Task RemoveAdminFromDepartmentAsync(Guid adminUserId, Guid departmentId, CancellationToken ct);

    // Courses / Offerings
    Task<List<CourseItem>> GetCourseDetailsAsync(Guid? departmentId, CancellationToken ct);
    Task<List<CourseOfferingItem>> GetCourseOfferingsAsync(Guid? departmentId, CancellationToken ct);
    Task<List<LookupItem>> GetMyOfferingsAsync(CancellationToken ct);
    Task CreateCourseAsync(string code, string title, int creditHours, Guid departmentId, CancellationToken ct);
    Task CreateOfferingAsync(Guid courseId, Guid semesterId, int maxEnrollment, Guid? facultyUserId, CancellationToken ct);
    Task DeactivateCourseAsync(Guid id, CancellationToken ct);
    Task DeleteOfferingAsync(Guid id, CancellationToken ct);

    // Assignments
    Task<List<AssignmentItem>> GetMyAssignmentsAsync(CancellationToken ct);
    Task<List<MyAssignmentSubmissionItem>> GetMyAssignmentSubmissionsAsync(CancellationToken ct);
    Task<List<AssignmentItem>> GetAssignmentsByOfferingAsync(Guid offeringId, CancellationToken ct);
    Task<List<SubmissionItem>> GetSubmissionsForAssignmentAsync(Guid assignmentId, CancellationToken ct);
    Task SubmitAssignmentAsync(Guid assignmentId, string? fileUrl, string? textContent, CancellationToken ct);
    Task<Guid> CreateAssignmentAsync(Guid courseOfferingId, string title, string? description, DateTime dueDate, decimal maxMarks, CancellationToken ct);
    Task UpdateAssignmentAsync(Guid id, string title, string? description, DateTime dueDate, decimal maxMarks, CancellationToken ct);
    Task PublishAssignmentAsync(Guid id, CancellationToken ct);
    Task DeleteAssignmentAsync(Guid id, CancellationToken ct);
    Task GradeSubmissionAsync(Guid assignmentId, Guid studentProfileId, decimal marksAwarded, string? feedback, CancellationToken ct);

    // Attendance
    Task<List<AttendanceSummaryItem>> GetMyAttendanceSummaryAsync(CancellationToken ct);
    Task<List<AttendanceRecordItem>> GetAttendanceByOfferingAsync(Guid offeringId, CancellationToken ct);
    Task BulkMarkAttendanceAsync(Guid offeringId, DateTime date, IEnumerable<(Guid StudentProfileId, string Status)> entries, CancellationToken ct);
    Task CorrectAttendanceAsync(Guid studentProfileId, Guid courseOfferingId, DateTime date, string newStatus, string? remarks, CancellationToken ct);

    // Results
    Task<List<ResultItem>> GetMyResultsAsync(CancellationToken ct);
    Task<List<ResultItem>> GetResultsByOfferingAsync(Guid offeringId, CancellationToken ct);
    Task CreateResultAsync(Guid studentProfileId, Guid courseOfferingId, string resultType, decimal marksObtained, decimal maxMarks, CancellationToken ct);
    Task CorrectResultAsync(Guid studentProfileId, Guid courseOfferingId, string resultType, decimal newMarksObtained, decimal newMaxMarks, CancellationToken ct);
    Task PublishAllResultsAsync(Guid courseOfferingId, CancellationToken ct);

    // Quizzes
    Task<List<QuizItem>> GetQuizzesByOfferingAsync(Guid offeringId, CancellationToken ct);
    Task<List<QuizAttemptItem>> GetMyAttemptsAsync(CancellationToken ct);
    Task<Guid> CreateQuizAsync(Guid courseOfferingId, string title, string? instructions, int? timeLimitMinutes, int maxAttempts, CancellationToken ct);
    Task UpdateQuizAsync(Guid id, string title, string? instructions, int? timeLimitMinutes, int maxAttempts, CancellationToken ct);
    Task PublishQuizAsync(Guid id, CancellationToken ct);
    Task DeleteQuizAsync(Guid id, CancellationToken ct);

    // FYP
    Task<List<FypProjectItem>> GetMyFypProjectsAsync(CancellationToken ct);
    Task<List<FypProjectItem>> GetAllFypProjectsAsync(CancellationToken ct);
    Task<List<FypProjectItem>> GetFypByDepartmentAsync(Guid departmentId, CancellationToken ct);
    Task<List<FypProjectItem>> GetMySupervisedProjectsAsync(CancellationToken ct);
    Task<List<FypMeetingItem>> GetUpcomingMeetingsAsync(CancellationToken ct);
    Task<Guid> ProposeFypProjectAsync(Guid departmentId, string title, string description, CancellationToken ct);
    Task<Guid> CreateFypProjectAsync(Guid studentProfileId, Guid departmentId, string title, string description, CancellationToken ct);
    Task UpdateFypProjectAsync(Guid id, string title, string description, CancellationToken ct);
    Task ApproveFypProjectAsync(Guid id, string? remarks, CancellationToken ct);
    Task RejectFypProjectAsync(Guid id, string remarks, CancellationToken ct);
    Task AssignFypSupervisorAsync(Guid id, Guid supervisorUserId, CancellationToken ct);
    Task CompleteFypProjectAsync(Guid id, CancellationToken ct);
    Task RequestFypCompletionAsync(Guid id, CancellationToken ct);
    Task ApproveFypCompletionAsync(Guid id, CancellationToken ct);

    // Analytics — Final-Touches Phase 6 Stage 6.2: typed return instead of raw JSON strings
    Task<DepartmentPerformanceReport?> GetPerformanceAnalyticsAsync(CancellationToken ct);
    Task<DepartmentAttendanceReport?> GetAttendanceAnalyticsAsync(CancellationToken ct);
    Task<AssignmentStatsReport?> GetAssignmentAnalyticsAsync(CancellationToken ct);

    // AI Chat
    Task<List<AiChatConversationItem>> GetChatConversationsAsync(CancellationToken ct);
    Task<List<AiChatMessageItem>> GetChatMessagesAsync(Guid conversationId, CancellationToken ct);
    Task<AiChatMessageItem?> SendChatMessageAsync(Guid? conversationId, string message, CancellationToken ct);

    // Student Lifecycle
    Task<List<GraduationCandidateItem>> GetGraduationCandidatesAsync(Guid departmentId, CancellationToken ct);
    Task GraduateStudentAsync(Guid studentId, CancellationToken ct);
    Task GraduateStudentsBatchAsync(List<Guid> studentIds, CancellationToken ct);
    Task<List<StudentItem>> GetStudentsBySemesterAsync(Guid departmentId, int semesterNumber, CancellationToken ct);
    Task PromoteStudentAsync(Guid studentId, CancellationToken ct);

    // Payments
    Task<List<PaymentReceiptItem>> GetPaymentsByStudentAsync(Guid studentId, CancellationToken ct);
    // Final-Touches Phase 7 — admin all-receipts, student own, create, confirm, cancel, submit proof
    Task<List<PaymentReceiptItem>> GetAllPaymentsAsync(CancellationToken ct);
    Task<List<PaymentReceiptItem>> GetMyPaymentsAsync(CancellationToken ct);
    Task CreatePaymentAsync(Guid studentProfileId, decimal amount, string description, DateTime dueDate, CancellationToken ct);
    Task ConfirmPaymentAsync(Guid receiptId, CancellationToken ct);
    Task CancelPaymentAsync(Guid receiptId, CancellationToken ct);
    Task SubmitProofAsync(Guid receiptId, string proofNote, CancellationToken ct);

    // Enrollments
    Task<List<EnrollmentRosterItem>> GetEnrollmentRosterAsync(Guid offeringId, CancellationToken ct);
    // Final-Touches Phase 8 Stage 8.1+8.2 — student my-courses, admin enroll/drop, student enroll/drop
    Task<List<MyEnrollmentItem>> GetMyEnrollmentsAsync(CancellationToken ct);
    Task AdminEnrollStudentAsync(Guid studentProfileId, Guid courseOfferingId, CancellationToken ct);
    Task AdminDropEnrollmentAsync(Guid enrollmentId, CancellationToken ct);
    Task StudentEnrollAsync(Guid courseOfferingId, CancellationToken ct);
    Task StudentDropEnrollmentAsync(Guid courseOfferingId, CancellationToken ct);

    // Portal / Dashboard Settings
    Task<PortalBrandingWebModel> GetPortalBrandingAsync(CancellationToken ct);
    Task SavePortalBrandingAsync(PortalBrandingWebModel model, CancellationToken ct);
    Task<string?> UploadLogoAsync(Stream fileStream, string fileName, CancellationToken ct);

    // Phase 12: Academic Calendar & Deadlines
    Task<List<DeadlineWebItem>> GetCalendarDeadlinesAsync(Guid? semesterId, CancellationToken ct);
    Task<DeadlineWebItem?> GetCalendarDeadlineByIdAsync(Guid id, CancellationToken ct);
    Task CreateCalendarDeadlineAsync(DeadlineFormModel form, CancellationToken ct);
    Task UpdateCalendarDeadlineAsync(Guid id, DeadlineFormModel form, CancellationToken ct);
    Task DeleteCalendarDeadlineAsync(Guid id, CancellationToken ct);

    // Phase 13: Global Search
    Task<SearchWebResponse> SearchAsync(string term, int limit, CancellationToken ct);

    // Phase 14: Helpdesk
    Task<List<TicketSummaryItem>> GetTicketsAsync(TicketStatusWeb? status, CancellationToken ct);
    Task<TicketDetailItem?> GetTicketDetailAsync(Guid ticketId, CancellationToken ct);
    Task<Guid> CreateTicketAsync(Guid? departmentId, TicketCategoryWeb category, string subject, string body, CancellationToken ct);
    Task<Guid> AddTicketMessageAsync(Guid ticketId, string body, bool isInternalNote, CancellationToken ct);
    Task AssignTicketAsync(Guid ticketId, Guid assignedToId, CancellationToken ct);
    Task ResolveTicketAsync(Guid ticketId, CancellationToken ct);
    Task CloseTicketAsync(Guid ticketId, CancellationToken ct);
    Task ReopenTicketAsync(Guid ticketId, CancellationToken ct);
}

public class EduApiClient : IEduApiClient
{
    private const string ApiUrlKey    = "ApiBaseUrl";
    private const string ApiTokenKey  = "ApiAccessToken";
    private const string DepartmentKey = "DefaultDepartmentId";
    private const string IdentityKey  = "SessionIdentityJson";
    private const string ForcePasswordChangeKey = "ForcePasswordChangeRequired";

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

    public bool IsForcePasswordChangeRequired()
    {
        var raw = GetSession().GetString(ForcePasswordChangeKey);
        return bool.TryParse(raw, out var required) && required;
    }

    public void SetForcePasswordChangeRequired(bool required)
    {
        var session = GetSession();
        session.SetString(ForcePasswordChangeKey, required.ToString());

        var identity = GetSessionIdentity() ?? new SessionIdentity();
        identity.MustChangePassword = required;
        session.SetString(IdentityKey, JsonSerializer.Serialize(identity, _jsonOptions));
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
        session.Remove(ForcePasswordChangeKey);
    }

    public SessionIdentity? GetSessionIdentity()
    {
        var raw = GetSession().GetString(IdentityKey);
        if (string.IsNullOrWhiteSpace(raw)) return null;
        try { return JsonSerializer.Deserialize<SessionIdentity>(raw, _jsonOptions); }
        catch { return null; }
    }

    public async Task<StudentProfileSummaryItem?> GetMyStudentProfileAsync(CancellationToken ct)
    {
        var raw = await GetAsync<StudentProfileApiDto>("api/v1/student/profile", ct);
        if (raw is null) return null;

        return new StudentProfileSummaryItem
        {
            Id = raw.Id,
            DepartmentId = raw.DepartmentId,
            DepartmentName = raw.DeptName ?? "",
            CurrentSemesterNumber = raw.CurrentSemesterNumber
        };
    }

    public Task ForceChangePasswordAsync(string newPassword, CancellationToken ct)
        => PostAsync<object, object>("api/v1/auth/force-change-password", new { newPassword }, ct);

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

    private async Task DeleteAsync(string path, CancellationToken ct)
    {
        using var request  = CreateRequest(HttpMethod.Delete, path);
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);
    }

    private async Task DeleteWithBodyAsync(string path, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        using var request = CreateRequest(HttpMethod.Delete, path);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);
    }

    // Dedicated byte-array downloader for binary responses (Excel exports).
    // Cannot reuse GetAsync<T> here because that method reads the body as JSON
    // and attempts to deserialize it, which corrupts binary xlsx content.
    private async Task<byte[]> GetBytesAsync(string path, CancellationToken ct)
    {
        using var request  = CreateRequest(HttpMethod.Get, path);
        using var response = await CreateClient().SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            throw BuildException(response.StatusCode, body);
        }
        return await response.Content.ReadAsByteArrayAsync(ct);
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

            // Role claim may be emitted as `role` or the standard ClaimTypes.Role URI.
            if (TryReadRoleClaims(root, out var roles))
            {
                identity.Roles.AddRange(roles);
            }
        }
        catch { /* ignore decode errors â€“ identity stays default */ }

        static bool TryReadRoleClaims(JsonElement root, out List<string> roles)
        {
            roles = new List<string>();
            if (TryAppendRoles(root, "role", roles)) return true;
            return TryAppendRoles(root, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", roles);
        }

        static bool TryAppendRoles(JsonElement root, string propertyName, List<string> roles)
        {
            if (!root.TryGetProperty(propertyName, out var roleProp))
                return false;

            if (roleProp.ValueKind == JsonValueKind.Array)
            {
                foreach (var r in roleProp.EnumerateArray())
                {
                    if (r.GetString() is string value && !string.IsNullOrWhiteSpace(value))
                        roles.Add(value);
                }
            }
            else if (roleProp.GetString() is string singleRole && !string.IsNullOrWhiteSpace(singleRole))
            {
                roles.Add(singleRole);
            }

            return roles.Count > 0;
        }

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

    public async Task<ResultCalculationSettingsPageModel> GetResultCalculationSettingsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<ResultCalculationSettingsApiDto>("api/v1/result-calculation", ct);
        var model = new ResultCalculationSettingsPageModel();
        if (raw is null) return model;

        model.GpaRules = raw.GpaScaleRules.Select((r, index) => new ResultCalculationGpaRuleItem
        {
            Id = r.Id,
            GradePoint = r.GradePoint,
            MinimumScore = r.MinimumScore,
            DisplayOrder = r.DisplayOrder == 0 ? index + 1 : r.DisplayOrder
        }).ToList();

        model.ComponentRules = raw.ComponentRules.Select((r, index) => new ResultCalculationComponentRuleItem
        {
            Id = r.Id,
            Name = r.Name ?? string.Empty,
            Weightage = r.Weightage,
            DisplayOrder = r.DisplayOrder == 0 ? index + 1 : r.DisplayOrder,
            IsActive = r.IsActive
        }).ToList();

        return model;
    }

    public Task SaveResultCalculationSettingsAsync(ResultCalculationSettingsPageModel model, CancellationToken ct)
    {
        var payload = new
        {
            gpaScaleRules = model.GpaRules.Select((r, index) => new
            {
                r.Id,
                gradePoint = r.GradePoint,
                minimumScore = r.MinimumScore,
                displayOrder = index + 1
            }).ToList(),
            componentRules = model.ComponentRules.Select((r, index) => new
            {
                r.Id,
                name = r.Name,
                weightage = r.Weightage,
                displayOrder = index + 1,
                isActive = r.IsActive
            }).ToList()
        };

        return PostAsync<object, object>("api/v1/result-calculation", payload, ct);
    }

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

    private sealed class ResultCalculationSettingsApiDto
    {
        public List<ResultCalculationGpaRuleApiDto> GpaScaleRules { get; set; } = new();
        public List<ResultCalculationComponentRuleApiDto> ComponentRules { get; set; } = new();
    }

    private sealed class ResultCalculationGpaRuleApiDto
    {
        public Guid? Id { get; set; }
        public decimal GradePoint { get; set; }
        public decimal MinimumScore { get; set; }
        public int DisplayOrder { get; set; }
    }

    private sealed class ResultCalculationComponentRuleApiDto
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public decimal Weightage { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
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

    // ── Notifications ─────────────────────────────────────────────────────────

    public async Task<List<NotificationItem>> GetNotificationsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<NotificationApiDto>>("api/v1/notification/inbox", ct) ?? new();
        return raw.Select(n => new NotificationItem
        {
            Id               = n.Id,
            Title            = n.Title ?? "",
            Body             = n.Body,
            NotificationType = n.NotificationType ?? "",
            IsRead           = n.IsRead,
            CreatedAt        = n.CreatedAt
        }).ToList();
    }

    public async Task<int> GetUnreadNotificationCountAsync(CancellationToken ct)
    {
        var dto = await GetAsync<BadgeDto>("api/v1/notification/badge", ct);
        return dto?.UnreadCount ?? 0;
    }

    public Task MarkNotificationReadAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/notification/{id}/read", new { }, ct);

    public Task MarkAllNotificationsReadAsync(CancellationToken ct)
        => PostAsync<object, object>("api/v1/notification/read-all", new { }, ct);

    private sealed class NotificationApiDto
    {
        public Guid     Id               { get; set; }
        public string?  Title            { get; set; }
        public string?  Body             { get; set; }
        public string?  NotificationType { get; set; }
        public bool     IsRead           { get; set; }
        public DateTime CreatedAt        { get; set; }
    }

    private sealed class BadgeDto { public int UnreadCount { get; set; } }

    // ── Students ──────────────────────────────────────────────────────────────

    public async Task<List<StudentItem>> GetStudentsAsync(Guid? departmentId, CancellationToken ct)
    {
        var path = departmentId.HasValue
            ? $"api/v1/student?departmentId={departmentId.Value}"
            : "api/v1/student";
        var raw = await GetAsync<List<StudentApiDto>>(path, ct) ?? new();
        return raw.Select(MapStudent).ToList();
    }

    private static StudentItem MapStudent(StudentApiDto s) => new()
    {
        // Final-Touches Phase 1 Stage 1.5 — semester-students endpoint returns StudentProfileId.
        Id                 = s.StudentProfileId != Guid.Empty ? s.StudentProfileId : s.Id,
        RegistrationNumber = s.RegistrationNumber ?? "",
        FullName           = s.FullName ?? s.StudentName ?? s.UserName ?? s.Email ?? s.RegistrationNumber ?? "Student",
        Email              = s.Email,
        DepartmentName     = s.DepartmentName ?? "",
        ProgramName        = s.ProgramName ?? "",
        SemesterNumber     = s.SemesterNumber > 0 ? s.SemesterNumber : s.CurrentSemesterNumber,
        Status             = s.Status ?? "Active"
    };

    private sealed class StudentApiDto
    {
        public Guid    Id                 { get; set; }
        public Guid    StudentProfileId   { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? FullName           { get; set; }
        public string? StudentName        { get; set; }
        public string? UserName           { get; set; }
        public string? Email              { get; set; }
        public string? DepartmentName     { get; set; }
        public string? ProgramName        { get; set; }
        public int     SemesterNumber     { get; set; }
        public int     CurrentSemesterNumber { get; set; }
        public string? Status             { get; set; }
    }

    // ── Departments (detail) ──────────────────────────────────────────────────

    public async Task<List<DepartmentItem>> GetDepartmentDetailsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<DeptDetailDto>>("api/v1/department", ct) ?? new();
        return raw.Select(d => new DepartmentItem
        {
            Id = d.Id, Name = d.Name ?? "", Code = d.Code ?? "", IsActive = d.IsActive
        }).ToList();
    }

    private sealed class DeptDetailDto
    {
        public Guid    Id       { get; set; }
        public string? Name     { get; set; }
        public string? Code     { get; set; }
        public bool    IsActive { get; set; }
    }

    private sealed class AdminUserApiDto
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class AdminDepartmentAssignmentApiDto
    {
        public Guid AdminUserId { get; set; }
        public Guid DepartmentId { get; set; }
    }

    private sealed class UserImportResultApiDto
    {
        public int TotalRows { get; set; }
        public int Imported { get; set; }
        public int Duplicates { get; set; }
        public int Errors { get; set; }
        public List<string>? ErrorDetails { get; set; }
    }

    public Task CreateDepartmentAsync(string name, string code, CancellationToken ct)
        => PostAsync<object, object>("api/v1/department", new { name, code }, ct);

    public Task UpdateDepartmentAsync(Guid id, string newName, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/department/{id}", new { newName }, ct);

    public Task DeactivateDepartmentAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/department/{id}", ct);

    public async Task<UserImportResultItem> ImportUsersCsvAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);

        using var request = CreateRequest(HttpMethod.Post, "api/v1/user-import/csv");
        request.Content = content;

        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) throw BuildException(response.StatusCode, body);

        var raw = string.IsNullOrWhiteSpace(body)
            ? null
            : JsonSerializer.Deserialize<UserImportResultApiDto>(body, _jsonOptions);

        return new UserImportResultItem
        {
            TotalRows = raw?.TotalRows ?? 0,
            Imported = raw?.Imported ?? 0,
            Duplicates = raw?.Duplicates ?? 0,
            Errors = raw?.Errors ?? 0,
            ErrorDetails = raw?.ErrorDetails ?? new List<string>()
        };
    }

    public async Task<List<AdminUserLookupItem>> GetAdminUsersAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<AdminUserApiDto>>("api/v1/admin-user", ct) ?? new();
        return raw.Select(a => new AdminUserLookupItem
        {
            Id = a.Id,
            UserName = a.Username ?? string.Empty,
            Email = a.Email,
            IsActive = a.IsActive
        }).ToList();
    }

    public async Task<Guid> CreateAdminUserAsync(string username, string? email, string password, CancellationToken ct)
    {
        var created = await PostAsync<object, AdminUserApiDto>("api/v1/admin-user", new { username, email, password }, ct)
            ?? throw new InvalidOperationException("Admin create API returned no body.");
        return created.Id;
    }

    public Task UpdateAdminUserAsync(Guid userId, string? email, bool isActive, string? newPassword, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/admin-user/{userId}", new { email, isActive, newPassword }, ct);

    public async Task<List<Guid>> GetAdminDepartmentIdsAsync(Guid adminUserId, CancellationToken ct)
    {
        var raw = await GetAsync<List<AdminDepartmentAssignmentApiDto>>($"api/v1/department/admin-assignment/{adminUserId}", ct) ?? new();
        return raw.Select(x => x.DepartmentId).Distinct().ToList();
    }

    public Task AssignAdminToDepartmentAsync(Guid adminUserId, Guid departmentId, CancellationToken ct)
        => PostAsync<object, object>("api/v1/department/admin-assignment", new { adminUserId, departmentId }, ct);

    public Task RemoveAdminFromDepartmentAsync(Guid adminUserId, Guid departmentId, CancellationToken ct)
        => DeleteWithBodyAsync("api/v1/department/admin-assignment", new { adminUserId, departmentId }, ct);

    // ── Courses ───────────────────────────────────────────────────────────────

    public async Task<List<CourseItem>> GetCourseDetailsAsync(Guid? departmentId, CancellationToken ct)
    {
        var path = departmentId.HasValue
            ? $"api/v1/course?departmentId={departmentId.Value}"
            : "api/v1/course";
        var raw = await GetAsync<List<CourseDetailDto>>(path, ct) ?? new();
        return raw.Select(c => new CourseItem
        {
            Id             = c.Id,
            Title          = c.Title ?? "",
            Code           = c.Code ?? "",
            DepartmentName = c.DepartmentName ?? "",
            CreditHours    = c.CreditHours
        }).ToList();
    }

    public async Task<List<CourseOfferingItem>> GetCourseOfferingsAsync(Guid? departmentId, CancellationToken ct)
    {
        var path = departmentId.HasValue
            ? $"api/v1/course/offerings?departmentId={departmentId.Value}"
            : "api/v1/course/offerings";
        var raw = await GetAsync<List<OfferingApiDto>>(path, ct) ?? new();
        return raw.Select(o => new CourseOfferingItem
        {
            Id           = o.Id,
            CourseTitle  = o.CourseTitle ?? "",
            CourseCode   = o.CourseCode ?? "",
            FacultyName  = o.FacultyName ?? "",
            SemesterName = o.SemesterName ?? "",
            IsActive     = o.IsActive
        }).ToList();
    }

    public async Task<List<LookupItem>> GetMyOfferingsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<MyOfferingApiDto>>("api/v1/course/offerings/my", ct) ?? new();
        return raw.Select(o => new LookupItem
        {
            Id = o.Id,
            Name = string.IsNullOrWhiteSpace(o.SemesterName)
                ? o.CourseTitle ?? "Offering"
                : $"{o.CourseTitle ?? "Offering"} ({o.SemesterName})"
        }).ToList();
    }

    public Task CreateCourseAsync(string code, string title, int creditHours, Guid departmentId, CancellationToken ct)
        => PostAsync<object, object>("api/v1/course", new { code, title, creditHours, departmentId }, ct);

    public Task CreateOfferingAsync(Guid courseId, Guid semesterId, int maxEnrollment, Guid? facultyUserId, CancellationToken ct)
        => PostAsync<object, object>("api/v1/course/offerings", new { courseId, semesterId, maxEnrollment, facultyUserId }, ct);

    public Task DeactivateCourseAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/course/{id}", ct);

    public Task DeleteOfferingAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/course/offerings/{id}", ct);

    private sealed class CourseDetailDto
    {
        public Guid    Id             { get; set; }
        public string? Title          { get; set; }
        public string? Code           { get; set; }
        public string? DepartmentName { get; set; }
        public int     CreditHours    { get; set; }
    }

    private sealed class OfferingApiDto
    {
        public Guid    Id           { get; set; }
        public string? CourseTitle  { get; set; }
        public string? CourseCode   { get; set; }
        public string? FacultyName  { get; set; }
        public string? SemesterName { get; set; }
        public bool    IsActive     { get; set; }
    }

    private sealed class MyOfferingApiDto
    {
        public Guid Id { get; set; }
        public string? CourseTitle { get; set; }
        public string? SemesterName { get; set; }
    }

    private sealed class StudentProfileApiDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public string? DeptName { get; set; }
        public int CurrentSemesterNumber { get; set; }
    }

    // ── Assignments ───────────────────────────────────────────────────────────

    public async Task<List<AssignmentItem>> GetMyAssignmentsAsync(CancellationToken ct)
    {
        // Student endpoint returns submissions, so map it into assignment-like rows for the default view.
        var raw = await GetAsync<List<MySubmissionApiDto>>("api/v1/assignment/my-submissions", ct) ?? new();
        return raw.Select(s => new AssignmentItem
        {
            Id                  = s.AssignmentId,
            Title               = s.AssignmentTitle ?? "",
            Description         = null,
            DueDate             = null,
            TotalMarks          = 0,
            IsPublished         = true,
            CourseOfferingTitle = "",
            SubmissionCount     = 0,
            IsSubmitted         = true,
            MarksAwarded        = s.MarksAwarded
        }).ToList();
    }

    public async Task<List<MyAssignmentSubmissionItem>> GetMyAssignmentSubmissionsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<MySubmissionApiDto>>("api/v1/assignment/my-submissions", ct) ?? new();
        return raw.Select(s => new MyAssignmentSubmissionItem
        {
            AssignmentId = s.AssignmentId,
            Status = s.Status ?? "",
            MarksAwarded = s.MarksAwarded,
            SubmittedAt = s.SubmittedAt
        }).ToList();
    }

    public async Task<List<AssignmentItem>> GetAssignmentsByOfferingAsync(Guid offeringId, CancellationToken ct)
    {
        var raw = await GetAsync<List<AssignmentApiDto>>($"api/v1/assignment/by-offering/{offeringId}", ct) ?? new();
        return raw.Select(MapAssignment).ToList();
    }

    private static AssignmentItem MapAssignment(AssignmentApiDto a) => new()
    {
        Id                  = a.Id,
        Title               = a.Title ?? "",
        Description         = a.Description,
        DueDate             = a.DueDate,
        TotalMarks          = a.MaxMarks,
        IsPublished         = a.IsPublished,
        CourseOfferingTitle = a.CourseOfferingTitle ?? "",
        SubmissionCount     = a.SubmissionCount,
        IsSubmitted         = a.IsSubmitted,
        MarksAwarded        = a.MarksAwarded
    };

    // ── Assignment write methods ──────────────────────────────────────────────

    public Task<Guid> CreateAssignmentAsync(Guid courseOfferingId, string title, string? description,
        DateTime dueDate, decimal maxMarks, CancellationToken ct)
    {
        var payload = new { courseOfferingId, title, description, dueDate, maxMarks };
        return PostAsync<object, AssignmentCreateResponse>("api/v1/assignment", payload, ct)
            .ContinueWith(t => t.Result?.Id ?? Guid.Empty, ct);
    }

    public Task UpdateAssignmentAsync(Guid id, string title, string? description,
        DateTime dueDate, decimal maxMarks, CancellationToken ct)
    {
        var payload = new { title, description, dueDate, maxMarks };
        return PutAsync<object, object>($"api/v1/assignment/{id}", payload, ct);
    }

    public Task PublishAssignmentAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/assignment/{id}/publish", new { }, ct);

    public Task DeleteAssignmentAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/assignment/{id}", ct);

    public Task GradeSubmissionAsync(Guid assignmentId, Guid studentProfileId, decimal marksAwarded,
        string? feedback, CancellationToken ct)
    {
        var payload = new { assignmentId, studentProfileId, marksAwarded, feedback };
        return PutAsync<object, object>("api/v1/assignment/submissions/grade", payload, ct);
    }

    private sealed class AssignmentCreateResponse { public Guid Id { get; set; } }

    public async Task<List<SubmissionItem>> GetSubmissionsForAssignmentAsync(Guid assignmentId, CancellationToken ct)
    {
        var raw = await GetAsync<List<SubmissionApiDto>>($"api/v1/assignment/{assignmentId}/submissions", ct) ?? new();
        return raw.Select(s => new SubmissionItem
        {
            Id                  = s.Id,
            StudentName         = s.StudentName ?? "",
            RegistrationNumber  = s.RegistrationNumber ?? "",
            Comments            = s.Comments,
            SubmittedAt         = s.SubmittedAt,
            IsGraded            = s.IsGraded,
            MarksAwarded        = s.MarksAwarded,
            FeedbackFromFaculty = s.FeedbackFromFaculty
        }).ToList();
    }

    public Task SubmitAssignmentAsync(Guid assignmentId, string? fileUrl, string? textContent, CancellationToken ct)
    {
        var payload = new { assignmentId, fileUrl, textContent };
        return PostAsync<object, object>("api/v1/assignment/submit", payload, ct);
    }

    private sealed class AssignmentApiDto
    {
        public Guid      Id                   { get; set; }
        public string?   Title                { get; set; }
        public string?   Description          { get; set; }
        public DateTime? DueDate              { get; set; }
        [JsonPropertyName("maxMarks")]
        public decimal   MaxMarks             { get; set; }
        public bool      IsPublished          { get; set; }
        public string?   CourseOfferingTitle  { get; set; }
        public int       SubmissionCount      { get; set; }
        public bool      IsSubmitted          { get; set; }
        public decimal?  MarksAwarded         { get; set; }
    }

    private sealed class SubmissionApiDto
    {
        public Guid     Id                   { get; set; }
        public string?  StudentName          { get; set; }
        public string?  RegistrationNumber   { get; set; }
        public string?  Comments             { get; set; }
        public DateTime SubmittedAt          { get; set; }
        public bool     IsGraded             { get; set; }
        public decimal? MarksAwarded         { get; set; }
        public string?  FeedbackFromFaculty  { get; set; }
    }

    private sealed class MySubmissionApiDto
    {
        public Guid      Id               { get; set; }
        public Guid      AssignmentId     { get; set; }
        public string?   AssignmentTitle  { get; set; }
        public Guid      StudentProfileId { get; set; }
        public string?   FileUrl          { get; set; }
        public string?   TextContent      { get; set; }
        public DateTime  SubmittedAt      { get; set; }
        public string?   Status           { get; set; }
        public decimal?  MarksAwarded     { get; set; }
        public string?   Feedback         { get; set; }
        public DateTime? GradedAt         { get; set; }
    }

    // ── Attendance ────────────────────────────────────────────────────────────

    public async Task<List<AttendanceSummaryItem>> GetMyAttendanceSummaryAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<MyAttendanceApiDto>>("api/v1/attendance/my-attendance", ct) ?? new();
        return raw.Select(s => new AttendanceSummaryItem
        {
            StudentId            = s.StudentId,
            StudentName          = s.StudentName ?? "",
            RegistrationNumber   = s.RegistrationNumber ?? "",
            CourseName           = s.CourseName ?? "",
            TotalClasses         = s.TotalClasses,
            PresentCount         = s.PresentCount,
            AttendancePercentage = s.AttendancePercentage
        }).ToList();
    }

    public async Task<List<AttendanceRecordItem>> GetAttendanceByOfferingAsync(Guid offeringId, CancellationToken ct)
    {
        var raw = await GetAsync<List<AttendanceRecordApiDto>>($"api/v1/attendance/by-offering/{offeringId}", ct) ?? new();
        return raw.Select(r => new AttendanceRecordItem
        {
            Id                 = r.Id,
            StudentProfileId   = r.StudentProfileId,
            StudentName        = r.StudentName ?? "",
            RegistrationNumber = r.RegistrationNumber ?? "",
            Date               = r.Date,
            Status             = r.Status ?? "",
            IsCorrected        = r.IsCorrected
        }).ToList();
    }

    private sealed class MyAttendanceApiDto
    {
        public Guid   StudentId            { get; set; }
        public string? StudentName         { get; set; }
        public string? RegistrationNumber  { get; set; }
        public string? CourseName          { get; set; }
        public int    TotalClasses         { get; set; }
        public int    PresentCount         { get; set; }
        public double AttendancePercentage { get; set; }
    }

    private sealed class AttendanceRecordApiDto
    {
        public Guid     Id                 { get; set; }
        public Guid     StudentProfileId   { get; set; }
        public string?  StudentName        { get; set; }
        public string?  RegistrationNumber { get; set; }
        public DateTime Date               { get; set; }
        public string?  Status             { get; set; }
        public bool     IsCorrected        { get; set; }
    }

    // ── Attendance write methods ──────────────────────────────────────────────

    public Task BulkMarkAttendanceAsync(Guid offeringId, DateTime date,
        IEnumerable<(Guid StudentProfileId, string Status)> entries, CancellationToken ct)
    {
        var payload = new
        {
            courseOfferingId = offeringId,
            date,
            entries = entries.Select(e => new { studentProfileId = e.StudentProfileId, status = e.Status }).ToList()
        };
        return PostAsync<object, object>("api/v1/attendance/bulk", payload, ct);
    }

    public Task CorrectAttendanceAsync(Guid studentProfileId, Guid courseOfferingId, DateTime date,
        string newStatus, string? remarks, CancellationToken ct)
    {
        var payload = new { studentProfileId, courseOfferingId, date, newStatus, remarks };
        return PutAsync<object, object>("api/v1/attendance/correct", payload, ct);
    }

    // ── Results ───────────────────────────────────────────────────────────────

    public async Task<List<ResultItem>> GetMyResultsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<ResultApiDto>>("api/v1/result/my-results", ct) ?? new();
        return raw.Select(MapResult).ToList();
    }

    public async Task<List<ResultItem>> GetResultsByOfferingAsync(Guid offeringId, CancellationToken ct)
    {
        var raw = await GetAsync<List<ResultApiDto>>($"api/v1/result/by-offering/{offeringId}", ct) ?? new();
        return raw.Select(MapResult).ToList();
    }

    // Maps the raw API DTO to the view-facing ResultItem.
    // StudentProfileId is included so the Results table can render
    // the per-row Promote button with the correct student identity.
    private static ResultItem MapResult(ResultApiDto r) => new()
    {
        Id                 = r.Id,
        StudentProfileId   = r.StudentProfileId,
        CourseOfferingId   = r.CourseOfferingId,
        ResultType         = r.ResultType ?? "",
        CourseName         = r.CourseName ?? "",
        CourseCode         = r.CourseCode ?? "",
        MarksObtained      = r.MarksObtained,
        TotalMarks         = r.TotalMarks,
        LetterGrade        = r.LetterGrade,
        IsPublished        = r.IsPublished,
        SemesterName       = r.SemesterName ?? "",
        StudentName        = r.StudentName ?? "",
        RegistrationNumber = r.RegistrationNumber ?? ""
    };

    private sealed class ResultApiDto
    {
        public Guid    Id                 { get; set; }
        public Guid    StudentProfileId   { get; set; }
        public Guid    CourseOfferingId   { get; set; }
        public string? ResultType         { get; set; }
        public string? CourseName         { get; set; }
        public string? CourseCode         { get; set; }
        public decimal? MarksObtained     { get; set; }
        public int     TotalMarks         { get; set; }
        public string? LetterGrade        { get; set; }
        public bool    IsPublished        { get; set; }
        public string? SemesterName       { get; set; }
        public string? StudentName        { get; set; }
        public string? RegistrationNumber { get; set; }
    }

    // ── Result write methods ──────────────────────────────────────────────────

    public Task CreateResultAsync(Guid studentProfileId, Guid courseOfferingId,
        string resultType, decimal marksObtained, decimal maxMarks, CancellationToken ct)
    {
        var payload = new { studentProfileId, courseOfferingId, resultType, marksObtained, maxMarks };
        return PostAsync<object, object>("api/v1/result", payload, ct);
    }

    public Task CorrectResultAsync(Guid studentProfileId, Guid courseOfferingId, string resultType,
        decimal newMarksObtained, decimal newMaxMarks, CancellationToken ct)
    {
        var payload = new { newMarksObtained, newMaxMarks };
        return PutAsync<object, object>($"api/v1/result/correct?studentProfileId={studentProfileId}&courseOfferingId={courseOfferingId}&resultType={Uri.EscapeDataString(resultType)}", payload, ct);
    }

    public Task PublishAllResultsAsync(Guid courseOfferingId, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/result/publish-all?courseOfferingId={courseOfferingId}", new { }, ct);

    // ── Quizzes ───────────────────────────────────────────────────────────────

    public async Task<List<QuizItem>> GetQuizzesByOfferingAsync(Guid offeringId, CancellationToken ct)
    {
        var raw = await GetAsync<List<QuizApiDto>>($"api/v1/quiz/by-offering/{offeringId}", ct) ?? new();
        return raw.Select(q => new QuizItem
        {
            Id                  = q.Id,
            Title               = q.Title ?? "",
            Description         = q.Description,
            IsPublished         = q.IsPublished,
            IsActive            = q.IsActive,
            AvailableFrom       = q.AvailableFrom,
            AvailableTo         = q.AvailableTo,
            MaxAttempts         = q.MaxAttempts,
            TimeLimitMinutes    = q.TimeLimitMinutes,
            QuestionCount       = q.QuestionCount,
            CourseOfferingTitle = q.CourseOfferingTitle ?? ""
        }).ToList();
    }

    public async Task<List<QuizAttemptItem>> GetMyAttemptsAsync(CancellationToken ct)
    {
        // Fetch attempts across all quizzes student has accessed
        var raw = await GetAsync<List<QuizAttemptApiDto>>("api/v1/quiz/my-attempts", ct) ?? new();
        return raw.Select(a => new QuizAttemptItem
        {
            Id          = a.Id,
            QuizTitle   = a.QuizTitle ?? "",
            StartedAt   = a.StartedAt,
            SubmittedAt = a.SubmittedAt,
            Status      = a.Status ?? "",
            TotalScore  = a.TotalScore,
            MaxScore    = a.MaxScore
        }).ToList();
    }

    private sealed class QuizApiDto
    {
        public Guid      Id                  { get; set; }
        public string?   Title               { get; set; }
        public string?   Description         { get; set; }
        public bool      IsPublished         { get; set; }
        public bool      IsActive            { get; set; }
        public DateTime? AvailableFrom       { get; set; }
        public DateTime? AvailableTo         { get; set; }
        public int       MaxAttempts         { get; set; }
        public int?      TimeLimitMinutes    { get; set; }
        public int       QuestionCount       { get; set; }
        public string?   CourseOfferingTitle { get; set; }
    }

    private sealed class QuizAttemptApiDto
    {
        public Guid      Id          { get; set; }
        public string?   QuizTitle   { get; set; }
        public DateTime  StartedAt   { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string?   Status      { get; set; }
        public decimal?  TotalScore  { get; set; }
        public int       MaxScore    { get; set; }
    }

    // ── Quiz write methods ────────────────────────────────────────────────────

    public Task<Guid> CreateQuizAsync(Guid courseOfferingId, string title, string? instructions,
        int? timeLimitMinutes, int maxAttempts, CancellationToken ct)
    {
        var payload = new { courseOfferingId, title, instructions, timeLimitMinutes, maxAttempts };
        return PostAsync<object, QuizCreateResponse>("api/v1/quiz", payload, ct)
            .ContinueWith(t => t.Result?.QuizId ?? Guid.Empty, ct);
    }

    public Task UpdateQuizAsync(Guid id, string title, string? instructions,
        int? timeLimitMinutes, int maxAttempts, CancellationToken ct)
    {
        var payload = new
        {
            title,
            instructions,
            timeLimitMinutes,
            maxAttempts,
            availableFrom = (DateTime?)null,
            availableUntil = (DateTime?)null
        };
        return PutAsync<object, object>($"api/v1/quiz/{id}", payload, ct);
    }

    public Task PublishQuizAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/quiz/{id}/publish", new { }, ct);

    public Task DeleteQuizAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/quiz/{id}", ct);

    private sealed class QuizCreateResponse { public Guid QuizId { get; set; } }

    // ── FYP ───────────────────────────────────────────────────────────────────

    public async Task<List<FypProjectItem>> GetMyFypProjectsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<FypApiDto>>("api/v1/fyp/my-projects", ct) ?? new();
        return raw.Select(MapFyp).ToList();
    }

    public async Task<List<FypProjectItem>> GetAllFypProjectsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<FypApiDto>>("api/v1/fyp/all", ct) ?? new();
        return raw.Select(MapFyp).ToList();
    }

    public async Task<List<FypProjectItem>> GetFypByDepartmentAsync(Guid departmentId, CancellationToken ct)
    {
        var raw = await GetAsync<List<FypApiDto>>($"api/v1/fyp/by-department/{departmentId}", ct) ?? new();
        return raw.Select(MapFyp).ToList();
    }

    public async Task<List<FypProjectItem>> GetMySupervisedProjectsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<FypApiDto>>("api/v1/fyp/my-supervised", ct) ?? new();
        return raw.Select(MapFyp).ToList();
    }

    public async Task<List<FypMeetingItem>> GetUpcomingMeetingsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<FypMeetingApiDto>>("api/v1/fyp/meeting/upcoming", ct) ?? new();
        return raw.Select(m => new FypMeetingItem
        {
            Id           = m.Id,
            Title        = m.Title ?? "",
            ScheduledAt  = m.ScheduledAt,
            Status       = m.Status ?? "",
            Location     = m.Location,
            Notes        = m.Notes,
            ProjectTitle = m.ProjectTitle ?? ""
        }).ToList();
    }

    // ── FYP write methods ─────────────────────────────────────────────────────

    public Task<Guid> ProposeFypProjectAsync(Guid departmentId, string title, string description, CancellationToken ct)
    {
        var payload = new { departmentId, title, description };
        return PostAsync<object, FypCreateResponse>("api/v1/fyp", payload, ct)
            .ContinueWith(t => t.Result?.ProjectId ?? Guid.Empty, ct);
    }

    public Task<Guid> CreateFypProjectAsync(Guid studentProfileId, Guid departmentId, string title, string description, CancellationToken ct)
    {
        var payload = new { studentProfileId, departmentId, title, description };
        return PostAsync<object, FypCreateResponse>("api/v1/fyp/admin-create", payload, ct)
            .ContinueWith(t => t.Result?.ProjectId ?? Guid.Empty, ct);
    }

    public Task UpdateFypProjectAsync(Guid id, string title, string description, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/fyp/{id}", new { title, description }, ct);

    public Task ApproveFypProjectAsync(Guid id, string? remarks, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/approve", new { remarks }, ct);

    public Task RejectFypProjectAsync(Guid id, string remarks, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/reject", new { remarks }, ct);

    public Task AssignFypSupervisorAsync(Guid id, Guid supervisorUserId, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/assign-supervisor", new { supervisorUserId }, ct);

    public Task CompleteFypProjectAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/complete", new { }, ct);

    public Task RequestFypCompletionAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/request-completion", new { }, ct);

    public Task ApproveFypCompletionAsync(Guid id, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/fyp/{id}/approve-completion", new { }, ct);

    private sealed class FypCreateResponse { public Guid ProjectId { get; set; } }

    private static FypProjectItem MapFyp(FypApiDto f) => new()
    {
        Id             = f.ProjectId != Guid.Empty ? f.ProjectId : f.Id,
        Title          = f.Title ?? "",
        Description    = f.Description,
        Status         = f.Status ?? "",
        StudentName    = f.StudentName ?? "",
        SupervisorName = f.SupervisorName,
        DepartmentName = f.DepartmentName ?? "",
        MeetingCount   = f.MeetingCount,
        IsCompletionRequested = f.IsCompletionRequested,
        CompletionApprovalCount = f.CompletionApprovalCount,
        RequiredApprovalCount = f.RequiredApprovalCount,
        CompletionApprovedByUserIds = f.CompletionApprovedByUserIds ?? new()
    };

    private sealed class FypApiDto
    {
        public Guid    ProjectId      { get; set; }
        public Guid    Id             { get; set; }
        public string? Title          { get; set; }
        public string? Description    { get; set; }
        public string? Status         { get; set; }
        public string? StudentName    { get; set; }
        public string? SupervisorName { get; set; }
        public string? DepartmentName { get; set; }
        public int     MeetingCount   { get; set; }
        public bool    IsCompletionRequested { get; set; }
        public int     CompletionApprovalCount { get; set; }
        public int     RequiredApprovalCount { get; set; }
        public List<Guid>? CompletionApprovedByUserIds { get; set; }
    }

    private sealed class FypMeetingApiDto
    {
        public Guid     Id           { get; set; }
        public string?  Title        { get; set; }
        public DateTime ScheduledAt  { get; set; }
        public string?  Status       { get; set; }
        public string?  Location     { get; set; }
        public string?  Notes        { get; set; }
        public string?  ProjectTitle { get; set; }
    }

    // ── Analytics ─────────────────────────────────────────────────────────────

    // Final-Touches Phase 6 Stage 6.2 — replaced raw JSON fetch with typed GetAsync<T> deserialization
    public Task<DepartmentPerformanceReport?> GetPerformanceAnalyticsAsync(CancellationToken ct)
        => GetAsync<DepartmentPerformanceReport>("api/analytics/performance", ct);

    public Task<DepartmentAttendanceReport?> GetAttendanceAnalyticsAsync(CancellationToken ct)
        => GetAsync<DepartmentAttendanceReport>("api/analytics/attendance", ct);

    public Task<AssignmentStatsReport?> GetAssignmentAnalyticsAsync(CancellationToken ct)
        => GetAsync<AssignmentStatsReport>("api/analytics/assignments", ct);

    // ── AI Chat ───────────────────────────────────────────────────────────────

    public async Task<List<AiChatConversationItem>> GetChatConversationsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<ConversationApiDto>>("api/ai/conversations", ct) ?? new();
        return raw.Select(c => new AiChatConversationItem
        {
            Id            = c.Id,
            Title         = c.Title ?? "Conversation",
            CreatedAt     = c.CreatedAt,
            LastMessageAt = c.LastMessageAt
        }).ToList();
    }

    public async Task<List<AiChatMessageItem>> GetChatMessagesAsync(Guid conversationId, CancellationToken ct)
    {
        var raw = await GetAsync<List<MessageApiDto>>($"api/ai/conversations/{conversationId}", ct) ?? new();
        return raw.Select(m => new AiChatMessageItem
        {
            Id        = m.Id,
            Role      = m.Role ?? "user",
            Content   = m.Content ?? "",
            CreatedAt = m.CreatedAt
        }).ToList();
    }

    public async Task<AiChatMessageItem?> SendChatMessageAsync(Guid? conversationId, string message, CancellationToken ct)
    {
        var payload = new { conversationId, message };
        var raw = await PostAsync<object, MessageApiDto>("api/ai/message", payload, ct);
        if (raw is null) return null;
        return new AiChatMessageItem
        {
            Id        = raw.Id,
            Role      = raw.Role ?? "assistant",
            Content   = raw.Content ?? "",
            CreatedAt = raw.CreatedAt
        };
    }

    private sealed class ConversationApiDto
    {
        public Guid      Id            { get; set; }
        public string?   Title         { get; set; }
        public DateTime  CreatedAt     { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    private sealed class MessageApiDto
    {
        public Guid     Id        { get; set; }
        public string?  Role      { get; set; }
        public string?  Content   { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Student Lifecycle ─────────────────────────────────────────────────────

    public async Task<List<GraduationCandidateItem>> GetGraduationCandidatesAsync(Guid departmentId, CancellationToken ct)
    {
        var raw = await GetAsync<List<GradCandidateApiDto>>($"api/v1/student-lifecycle/graduation-candidates/{departmentId}", ct) ?? new();
        return raw.Select(g => new GraduationCandidateItem
        {
            Id                 = g.Id,
            FullName           = g.FullName ?? "",
            RegistrationNumber = g.RegistrationNumber ?? "",
            ProgramName        = g.ProgramName ?? "",
            SemesterNumber     = g.SemesterNumber,
            Cgpa               = g.Cgpa
        }).ToList();
    }

    public Task GraduateStudentAsync(Guid studentId, CancellationToken ct)
        => PostAsync<object, object>("api/v1/student-lifecycle/graduate", new { studentProfileId = studentId }, ct);

    public Task GraduateStudentsBatchAsync(List<Guid> studentIds, CancellationToken ct)
        => PostAsync<object, object>("api/v1/student-lifecycle/graduate/batch", new { studentProfileIds = studentIds }, ct);

    public async Task<List<StudentItem>> GetStudentsBySemesterAsync(Guid departmentId, int semesterNumber, CancellationToken ct)
    {
        var raw = await GetAsync<List<StudentApiDto>>($"api/v1/student-lifecycle/semester-students/{departmentId}/{semesterNumber}", ct) ?? new();
        return raw.Select(MapStudent).ToList();
    }

    public Task PromoteStudentAsync(Guid studentId, CancellationToken ct)
        => PostAsync<object, object>($"api/v1/student-lifecycle/{studentId}/promote", new { }, ct);

    private sealed class GradCandidateApiDto
    {
        public Guid    Id                 { get; set; }
        public string? FullName           { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? ProgramName        { get; set; }
        public int     SemesterNumber     { get; set; }
        public double? Cgpa               { get; set; }
    }

    // ── Payments ──────────────────────────────────────────────────────────────

    public async Task<List<PaymentReceiptItem>> GetPaymentsByStudentAsync(Guid studentId, CancellationToken ct)
    {
        var raw = await GetAsync<List<PaymentApiDto>>($"api/v1/payments/student/{studentId}", ct) ?? new();
        return raw.Select(MapPayment).ToList();
    }

    private static PaymentReceiptItem MapPayment(PaymentApiDto p) => new()
    {
        Id                 = p.Id,
        StudentProfileId   = p.StudentProfileId,
        StudentName        = p.StudentName ?? "",
        RegistrationNumber = p.RegistrationNumber ?? "",
        Amount             = p.Amount,
        FeeType            = p.Description ?? p.FeeType ?? "",
        Status             = p.Status ?? "",
        DueDate            = p.DueDate,
        PaidDate           = p.PaidDate,
        ProofOfPaymentPath = p.ProofOfPaymentPath,
        Notes              = p.Notes
    };

    // Final-Touches Phase 7 — admin and student payment actions
    public async Task<List<PaymentReceiptItem>> GetAllPaymentsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<PaymentApiDto>>("api/v1/payments", ct) ?? new();
        return raw.Select(MapPayment).ToList();
    }

    public async Task<List<PaymentReceiptItem>> GetMyPaymentsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<PaymentApiDto>>("api/v1/payments/mine", ct) ?? new();
        return raw.Select(MapPayment).ToList();
    }

    public Task CreatePaymentAsync(Guid studentProfileId, decimal amount, string description, DateTime dueDate, CancellationToken ct)
        => PostAsync<object, object>("api/v1/payments", new { studentProfileId, amount, description, dueDate }, ct);

    public Task ConfirmPaymentAsync(Guid receiptId, CancellationToken ct)
        => PostAsync<string, object>($"api/v1/payments/{receiptId}/confirm", string.Empty, ct);

    public Task CancelPaymentAsync(Guid receiptId, CancellationToken ct)
        => PostAsync<string, object>($"api/v1/payments/{receiptId}/cancel", string.Empty, ct);

    public Task SubmitProofAsync(Guid receiptId, string proofNote, CancellationToken ct)
        => PostAsync<string, object>($"api/v1/payments/{receiptId}/mark-submitted", proofNote, ct);

    private sealed class PaymentApiDto
    {
        public Guid     Id                 { get; set; }
        public Guid     StudentProfileId   { get; set; }
        public string?  StudentName        { get; set; }
        public string?  RegistrationNumber { get; set; }
        public decimal  Amount             { get; set; }
        public string?  FeeType            { get; set; }
        public string?  Description        { get; set; }
        public string?  Status             { get; set; }
        public DateTime DueDate            { get; set; }
        public DateTime? PaidDate          { get; set; }
        public string?  ProofOfPaymentPath { get; set; }
        public string?  Notes              { get; set; }
    }

    // ── Enrollments ───────────────────────────────────────────────────────────

    public async Task<List<EnrollmentRosterItem>> GetEnrollmentRosterAsync(Guid offeringId, CancellationToken ct)
    {
        var raw = await GetAsync<List<RosterApiDto>>($"api/v1/enrollment/roster/{offeringId}", ct) ?? new();
        return raw.Select(r => new EnrollmentRosterItem
        {
            Id                 = r.Id,
            StudentName        = r.StudentName ?? "",
            RegistrationNumber = r.RegistrationNumber ?? "",
            ProgramName        = r.ProgramName ?? "",
            SemesterNumber     = r.SemesterNumber
        }).ToList();
    }

    // Final-Touches Phase 8 Stage 8.1+8.2 — student my-courses, admin enroll/drop, student enroll/drop
    public async Task<List<MyEnrollmentItem>> GetMyEnrollmentsAsync(CancellationToken ct)
    {
        var raw = await GetAsync<List<MyCourseApiDto>>("api/v1/enrollment/my-courses", ct) ?? new();
        return raw.Select(e => new MyEnrollmentItem
        {
            EnrollmentId     = e.Id,
            CourseOfferingId = e.CourseOfferingId,
            CourseCode       = e.CourseCode ?? "",
            CourseTitle      = e.CourseTitle ?? "",
            SemesterName     = e.Semester ?? "",
            Status           = e.Status ?? "",
            EnrolledAt       = e.EnrolledAt
        }).ToList();
    }

    public async Task AdminEnrollStudentAsync(Guid studentProfileId, Guid courseOfferingId, CancellationToken ct)
        => await PostAsync<object, object>("api/v1/enrollment/admin",
               new { StudentProfileId = studentProfileId, CourseOfferingId = courseOfferingId }, ct);

    public async Task AdminDropEnrollmentAsync(Guid enrollmentId, CancellationToken ct)
        => await DeleteAsync($"api/v1/enrollment/admin/{enrollmentId}", ct);

    public async Task StudentEnrollAsync(Guid courseOfferingId, CancellationToken ct)
        => await PostAsync<object, object>("api/v1/enrollment",
               new { CourseOfferingId = courseOfferingId }, ct);

    public async Task StudentDropEnrollmentAsync(Guid courseOfferingId, CancellationToken ct)
        => await DeleteAsync($"api/v1/enrollment/{courseOfferingId}", ct);

    private sealed class MyCourseApiDto
    {
        public Guid     Id               { get; set; }
        public Guid     CourseOfferingId { get; set; }
        public string?  CourseTitle      { get; set; }
        public string?  CourseCode       { get; set; }
        public string?  Semester         { get; set; }
        public string?  Status           { get; set; }
        public DateTime EnrolledAt       { get; set; }
    }

    private sealed class RosterApiDto
    {
        public Guid    Id                 { get; set; }
        public string? StudentName        { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? ProgramName        { get; set; }
        public int     SemesterNumber     { get; set; }
    }

    // ── Phase 12: Report methods ───────────────────────────────────────────────

    public async Task<List<ReportCatalogItem>> GetReportCatalogAsync(CancellationToken ct)
    {
        var raw = await GetAsync<ReportCatalogApiDto>("api/v1/reports", ct);
        if (raw?.Reports is null) return new();
        return raw.Reports.Select(r => new ReportCatalogItem
        {
            Id           = r.Id,
            Key          = r.Key ?? "",
            Name         = r.Name ?? "",
            Purpose      = r.Purpose ?? "",
            AllowedRoles = r.AllowedRoles ?? new()
        }).ToList();
    }

    public async Task<AttendanceSummaryWebModel?> GetAttendanceSummaryReportAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        var raw = await GetAsync<AttendanceSummaryApiDto>($"api/v1/reports/attendance-summary{qs}", ct);
        if (raw is null) return null;
        return new AttendanceSummaryWebModel
        {
            TotalStudents = raw.TotalStudents,
            GeneratedAt   = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new AttendanceSummaryRowItem
            {
                RegistrationNumber   = r.RegistrationNumber ?? "",
                StudentName          = r.StudentName ?? "",
                CourseCode           = r.CourseCode ?? "",
                CourseTitle          = r.CourseTitle ?? "",
                TotalSessions        = r.TotalSessions,
                AttendedSessions     = r.AttendedSessions,
                AttendancePercentage = r.AttendancePercentage
            }).ToList() ?? new()
        };
    }

    public async Task<ResultSummaryWebModel?> GetResultSummaryReportAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        var raw = await GetAsync<ResultSummaryApiDto>($"api/v1/reports/result-summary{qs}", ct);
        if (raw is null) return null;
        return new ResultSummaryWebModel
        {
            TotalRecords = raw.TotalRecords,
            GeneratedAt  = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new ResultSummaryRowItem
            {
                RegistrationNumber = r.RegistrationNumber ?? "",
                StudentName        = r.StudentName ?? "",
                CourseCode         = r.CourseCode ?? "",
                CourseTitle        = r.CourseTitle ?? "",
                ResultType         = r.ResultType ?? "",
                MarksObtained      = r.MarksObtained,
                MaxMarks           = r.MaxMarks,
                Percentage         = r.Percentage,
                PublishedAt        = r.PublishedAt
            }).ToList() ?? new()
        };
    }

    public async Task<AssignmentSummaryWebModel?> GetAssignmentSummaryReportAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        var raw = await GetAsync<AssignmentSummaryApiDto>($"api/v1/reports/assignment-summary{qs}", ct);
        if (raw is null) return null;
        return new AssignmentSummaryWebModel
        {
            TotalSubmissions = raw.TotalSubmissions,
            GeneratedAt      = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new AssignmentSummaryRowItem
            {
                RegistrationNumber = r.RegistrationNumber ?? "",
                StudentName        = r.StudentName ?? "",
                CourseCode         = r.CourseCode ?? "",
                CourseTitle        = r.CourseTitle ?? "",
                AssignmentTitle    = r.AssignmentTitle ?? "",
                DueDate            = r.DueDate,
                SubmittedAt        = r.SubmittedAt,
                Status             = r.Status ?? "",
                MarksAwarded       = r.MarksAwarded
            }).ToList() ?? new()
        };
    }

    public async Task<QuizSummaryWebModel?> GetQuizSummaryReportAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        var raw = await GetAsync<QuizSummaryApiDto>($"api/v1/reports/quiz-summary{qs}", ct);
        if (raw is null) return null;
        return new QuizSummaryWebModel
        {
            TotalAttempts = raw.TotalAttempts,
            GeneratedAt   = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new QuizSummaryRowItem
            {
                RegistrationNumber = r.RegistrationNumber ?? "",
                StudentName        = r.StudentName ?? "",
                CourseCode         = r.CourseCode ?? "",
                CourseTitle        = r.CourseTitle ?? "",
                QuizTitle          = r.QuizTitle ?? "",
                StartedAt          = r.StartedAt,
                FinishedAt         = r.FinishedAt,
                AttemptStatus      = r.AttemptStatus ?? "",
                TotalScore         = r.TotalScore
            }).ToList() ?? new()
        };
    }

    public async Task<GpaReportWebModel?> GetGpaReportAsync(
        Guid? departmentId, Guid? programId, CancellationToken ct)
    {
        var parts = new List<string>();
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId.Value}");
        if (programId.HasValue)    parts.Add($"programId={programId.Value}");
        var qs = parts.Any() ? "?" + string.Join("&", parts) : "";
        var raw = await GetAsync<GpaReportApiDto>($"api/v1/reports/gpa-report{qs}", ct);
        if (raw is null) return null;
        return new GpaReportWebModel
        {
            AverageCgpa   = raw.AverageCgpa,
            TotalStudents = raw.TotalStudents,
            GeneratedAt   = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new GpaReportRowItem
            {
                RegistrationNumber = r.RegistrationNumber ?? "",
                StudentName        = r.StudentName ?? "",
                ProgramName        = r.ProgramName ?? "",
                DepartmentName     = r.DepartmentName ?? "",
                CurrentSemester    = r.CurrentSemester,
                Cgpa               = r.Cgpa,
                CurrentSemesterGpa = r.CurrentSemesterGpa
            }).ToList() ?? new()
        };
    }

    public async Task<EnrollmentSummaryWebModel?> GetEnrollmentSummaryReportAsync(
        Guid? semesterId, Guid? departmentId, CancellationToken ct)
    {
        var parts = new List<string>();
        if (semesterId.HasValue)   parts.Add($"semesterId={semesterId.Value}");
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId.Value}");
        var qs = parts.Any() ? "?" + string.Join("&", parts) : "";
        var raw = await GetAsync<EnrollmentSummaryApiDto>($"api/v1/reports/enrollment-summary{qs}", ct);
        if (raw is null) return null;
        return new EnrollmentSummaryWebModel
        {
            TotalOfferings = raw.TotalOfferings,
            GeneratedAt    = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new EnrollmentSummaryRowItem
            {
                CourseCode      = r.CourseCode ?? "",
                CourseTitle     = r.CourseTitle ?? "",
                SemesterName    = r.SemesterName ?? "",
                MaxEnrollment   = r.MaxEnrollment,
                EnrolledCount   = r.EnrolledCount,
                AvailableSeats  = r.AvailableSeats
            }).ToList() ?? new()
        };
    }

    private static string BuildReportQuery(Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId)
    {
        var parts = new List<string>();
        if (semesterId.HasValue)   parts.Add($"semesterId={semesterId.Value}");
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId.Value}");
        if (offeringId.HasValue)   parts.Add($"courseOfferingId={offeringId.Value}");
        if (studentId.HasValue)    parts.Add($"studentProfileId={studentId.Value}");
        return parts.Any() ? "?" + string.Join("&", parts) : "";
    }

    // semesterId is required by the API; departmentId is optional and narrows the result set
    // to a single department when provided.
    public async Task<SemesterResultsWebModel?> GetSemesterResultsReportAsync(
        Guid semesterId, Guid? departmentId, CancellationToken ct)
    {
        var parts = new List<string> { $"semesterId={semesterId}" };
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId.Value}");
        var raw = await GetAsync<SemesterResultsApiDto>($"api/v1/reports/semester-results?{string.Join("&", parts)}", ct);
        if (raw is null) return null;
        return new SemesterResultsWebModel
        {
            TotalStudents = raw.TotalStudents,
            GeneratedAt   = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new SemesterResultsRowItem
            {
                RegistrationNumber = r.RegistrationNumber ?? "",
                StudentName        = r.StudentName ?? "",
                CourseCode         = r.CourseCode ?? "",
                CourseTitle        = r.CourseTitle ?? "",
                ResultType         = r.ResultType ?? "",
                MarksObtained      = r.MarksObtained,
                MaxMarks           = r.MaxMarks,
                Percentage         = r.Percentage
            }).ToList() ?? new()
        };
    }

    // All three export methods delegate to GetBytesAsync because the API returns a
    // binary .xlsx file, not JSON. BuildReportQuery assembles the shared filter params.

    public Task<byte[]> ExportAttendanceSummaryAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/attendance-summary/export{qs}", ct);
    }

    public Task<byte[]> ExportAttendanceSummaryCsvAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/attendance-summary/export/csv{qs}", ct);
    }

    public Task<byte[]> ExportAttendanceSummaryPdfAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/attendance-summary/export/pdf{qs}", ct);
    }

    public Task<byte[]> ExportResultSummaryAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/result-summary/export{qs}", ct);
    }

    public Task<byte[]> ExportResultSummaryCsvAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/result-summary/export/csv{qs}", ct);
    }

    public Task<byte[]> ExportResultSummaryPdfAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/result-summary/export/pdf{qs}", ct);
    }

    public Task<byte[]> ExportAssignmentSummaryAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/assignment-summary/export{qs}", ct);
    }

    public Task<byte[]> ExportAssignmentSummaryCsvAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/assignment-summary/export/csv{qs}", ct);
    }

    public Task<byte[]> ExportAssignmentSummaryPdfAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/assignment-summary/export/pdf{qs}", ct);
    }

    public Task<byte[]> ExportQuizSummaryAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/quiz-summary/export{qs}", ct);
    }

    public Task<byte[]> ExportQuizSummaryCsvAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/quiz-summary/export/csv{qs}", ct);
    }

    public Task<byte[]> ExportQuizSummaryPdfAsync(
        Guid? semesterId, Guid? departmentId, Guid? offeringId, Guid? studentId, CancellationToken ct)
    {
        var qs = BuildReportQuery(semesterId, departmentId, offeringId, studentId);
        return GetBytesAsync($"api/v1/reports/quiz-summary/export/pdf{qs}", ct);
    }

    // GPA report uses department + program filters only (no per-offering or per-student scope).
    public Task<byte[]> ExportGpaReportAsync(Guid? departmentId, Guid? programId, CancellationToken ct)
    {
        var parts = new List<string>();
        if (departmentId.HasValue) parts.Add($"departmentId={departmentId.Value}");
        if (programId.HasValue)    parts.Add($"programId={programId.Value}");
        var qs = parts.Any() ? "?" + string.Join("&", parts) : "";
        return GetBytesAsync($"api/v1/reports/gpa-report/export{qs}", ct);
    }

    // ── Stage 4.2: Additional Report Proxy Methods ─────────────────────────────

    public async Task<TranscriptWebModel?> GetStudentTranscriptReportAsync(
        Guid studentProfileId, CancellationToken ct)
    {
        var raw = await GetAsync<TranscriptApiDto>(
            $"api/v1/reports/student-transcript?studentProfileId={studentProfileId}", ct);
        if (raw is null) return null;
        return new TranscriptWebModel
        {
            StudentProfileId   = raw.StudentProfileId,
            RegistrationNumber = raw.RegistrationNumber ?? "",
            StudentName        = raw.StudentName ?? "",
            ProgramName        = raw.ProgramName ?? "",
            DepartmentName     = raw.DepartmentName ?? "",
            Cgpa               = raw.Cgpa,
            GeneratedAt        = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new TranscriptRowItem
            {
                CourseCode    = r.CourseCode ?? "",
                CourseTitle   = r.CourseTitle ?? "",
                SemesterName  = r.SemesterName ?? "",
                ResultType    = r.ResultType ?? "",
                MarksObtained = r.MarksObtained,
                MaxMarks      = r.MaxMarks,
                Percentage    = r.Percentage,
                GradePoint    = r.GradePoint,
                PublishedAt   = r.PublishedAt
            }).ToList() ?? new()
        };
    }

    public async Task<LowAttendanceWebModel?> GetLowAttendanceReportAsync(
        decimal threshold, Guid? departmentId, Guid? courseOfferingId, CancellationToken ct)
    {
        var parts = new List<string> { $"threshold={threshold}" };
        if (departmentId.HasValue)     parts.Add($"departmentId={departmentId.Value}");
        if (courseOfferingId.HasValue) parts.Add($"courseOfferingId={courseOfferingId.Value}");
        var raw = await GetAsync<LowAttendanceApiDto>(
            $"api/v1/reports/low-attendance?{string.Join("&", parts)}", ct);
        if (raw is null) return null;
        return new LowAttendanceWebModel
        {
            ThresholdPercent    = raw.ThresholdPercent,
            TotalStudentsAtRisk = raw.TotalStudentsAtRisk,
            GeneratedAt         = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new LowAttendanceRowItem
            {
                RegistrationNumber   = r.RegistrationNumber ?? "",
                StudentName          = r.StudentName ?? "",
                CourseCode           = r.CourseCode ?? "",
                CourseTitle          = r.CourseTitle ?? "",
                SemesterName         = r.SemesterName ?? "",
                DepartmentName       = r.DepartmentName ?? "",
                TotalSessions        = r.TotalSessions,
                AttendedSessions     = r.AttendedSessions,
                AttendancePercentage = r.AttendancePercentage
            }).ToList() ?? new()
        };
    }

    public async Task<FypStatusWebModel?> GetFypStatusReportAsync(
        Guid? departmentId, string? status, CancellationToken ct)
    {
        var parts = new List<string>();
        if (departmentId.HasValue)         parts.Add($"departmentId={departmentId.Value}");
        if (!string.IsNullOrEmpty(status)) parts.Add($"status={Uri.EscapeDataString(status)}");
        var qs = parts.Any() ? "?" + string.Join("&", parts) : "";
        var raw = await GetAsync<FypStatusApiDto>($"api/v1/reports/fyp-status{qs}", ct);
        if (raw is null) return null;
        return new FypStatusWebModel
        {
            TotalProjects = raw.TotalProjects,
            GeneratedAt   = raw.GeneratedAt,
            Rows = raw.Rows?.Select(r => new FypStatusRowItem
            {
                Title              = r.Title ?? "",
                StudentName        = r.StudentName ?? "",
                RegistrationNumber = r.RegistrationNumber ?? "",
                DepartmentName     = r.DepartmentName ?? "",
                SupervisorName     = r.SupervisorName,
                Status             = r.Status ?? "",
                ProposedAt         = r.ProposedAt,
                MeetingCount       = r.MeetingCount
            }).ToList() ?? new()
        };
    }

    public Task<byte[]> ExportStudentTranscriptAsync(Guid studentProfileId, CancellationToken ct)
        => GetBytesAsync($"api/v1/reports/student-transcript/export?studentProfileId={studentProfileId}", ct);

    // Private API DTOs for Phase 12
    private sealed class ReportCatalogApiDto
    {
        public List<ReportCatalogItemApiDto>? Reports { get; set; }
    }
    private sealed class ReportCatalogItemApiDto
    {
        public Guid    Id           { get; set; }
        public string? Key          { get; set; }
        public string? Name         { get; set; }
        public string? Purpose      { get; set; }
        public bool    IsActive     { get; set; }
        public List<string> AllowedRoles { get; set; } = new();
    }
    private sealed class AttendanceSummaryApiDto
    {
        public int      TotalStudents { get; set; }
        public DateTime GeneratedAt   { get; set; }
        public List<AttendanceSummaryRowApiDto>? Rows { get; set; }
    }
    private sealed class AttendanceSummaryRowApiDto
    {
        public string? RegistrationNumber   { get; set; }
        public string? StudentName          { get; set; }
        public string? CourseCode           { get; set; }
        public string? CourseTitle          { get; set; }
        public int     TotalSessions        { get; set; }
        public int     AttendedSessions     { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
    private sealed class ResultSummaryApiDto
    {
        public int      TotalRecords { get; set; }
        public DateTime GeneratedAt  { get; set; }
        public List<ResultSummaryRowApiDto>? Rows { get; set; }
    }
    private sealed class ResultSummaryRowApiDto
    {
        public string?   RegistrationNumber { get; set; }
        public string?   StudentName        { get; set; }
        public string?   CourseCode         { get; set; }
        public string?   CourseTitle        { get; set; }
        public string?   ResultType         { get; set; }
        public decimal   MarksObtained      { get; set; }
        public decimal   MaxMarks           { get; set; }
        public decimal   Percentage         { get; set; }
        public DateTime? PublishedAt        { get; set; }
    }
    private sealed class AssignmentSummaryApiDto
    {
        public int      TotalSubmissions { get; set; }
        public DateTime GeneratedAt      { get; set; }
        public List<AssignmentSummaryRowApiDto>? Rows { get; set; }
    }
    private sealed class AssignmentSummaryRowApiDto
    {
        public string?   RegistrationNumber { get; set; }
        public string?   StudentName        { get; set; }
        public string?   CourseCode         { get; set; }
        public string?   CourseTitle        { get; set; }
        public string?   AssignmentTitle    { get; set; }
        public DateTime  DueDate            { get; set; }
        public DateTime  SubmittedAt        { get; set; }
        public string?   Status             { get; set; }
        public decimal?  MarksAwarded       { get; set; }
    }
    private sealed class QuizSummaryApiDto
    {
        public int      TotalAttempts { get; set; }
        public DateTime GeneratedAt   { get; set; }
        public List<QuizSummaryRowApiDto>? Rows { get; set; }
    }
    private sealed class QuizSummaryRowApiDto
    {
        public string?   RegistrationNumber { get; set; }
        public string?   StudentName        { get; set; }
        public string?   CourseCode         { get; set; }
        public string?   CourseTitle        { get; set; }
        public string?   QuizTitle          { get; set; }
        public DateTime  StartedAt          { get; set; }
        public DateTime? FinishedAt         { get; set; }
        public string?   AttemptStatus      { get; set; }
        public decimal?  TotalScore         { get; set; }
    }
    private sealed class GpaReportApiDto
    {
        public decimal  AverageCgpa   { get; set; }
        public int      TotalStudents { get; set; }
        public DateTime GeneratedAt   { get; set; }
        public List<GpaReportRowApiDto>? Rows { get; set; }
    }
    private sealed class GpaReportRowApiDto
    {
        public string? RegistrationNumber { get; set; }
        public string? StudentName        { get; set; }
        public string? ProgramName        { get; set; }
        public string? DepartmentName     { get; set; }
        public int     CurrentSemester    { get; set; }
        public decimal Cgpa               { get; set; }
        public decimal CurrentSemesterGpa { get; set; }
    }
    private sealed class EnrollmentSummaryApiDto
    {
        public int      TotalOfferings { get; set; }
        public DateTime GeneratedAt    { get; set; }
        public List<EnrollmentSummaryRowApiDto>? Rows { get; set; }
    }
    private sealed class EnrollmentSummaryRowApiDto
    {
        public string? CourseCode     { get; set; }
        public string? CourseTitle    { get; set; }
        public string? SemesterName   { get; set; }
        public int     MaxEnrollment  { get; set; }
        public int     EnrolledCount  { get; set; }
        public int     AvailableSeats { get; set; }
    }
    // Internal DTOs for the Semester Results report endpoint.
    // These are private to EduApiClient; the web layer uses SemesterResultsWebModel instead.
    private sealed class SemesterResultsApiDto
    {
        public int      TotalStudents { get; set; }
        public DateTime GeneratedAt   { get; set; }
        public List<SemesterResultsRowApiDto>? Rows { get; set; }
    }
    private sealed class SemesterResultsRowApiDto
    {
        public string? RegistrationNumber { get; set; }
        public string? StudentName        { get; set; }
        public string? CourseCode         { get; set; }
        public string? CourseTitle        { get; set; }
        public string? ResultType         { get; set; }
        public decimal MarksObtained      { get; set; }
        public decimal MaxMarks           { get; set; }
        public decimal Percentage         { get; set; }
    }

    // Stage 4.2 private DTOs
    private sealed class TranscriptApiDto
    {
        public Guid    StudentProfileId   { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? StudentName        { get; set; }
        public string? ProgramName        { get; set; }
        public string? DepartmentName     { get; set; }
        public decimal Cgpa               { get; set; }
        public DateTime GeneratedAt       { get; set; }
        public List<TranscriptRowApiDto>? Rows { get; set; }
    }
    private sealed class TranscriptRowApiDto
    {
        public string?   CourseCode    { get; set; }
        public string?   CourseTitle   { get; set; }
        public string?   SemesterName  { get; set; }
        public string?   ResultType    { get; set; }
        public decimal   MarksObtained { get; set; }
        public decimal   MaxMarks      { get; set; }
        public decimal   Percentage    { get; set; }
        public decimal?  GradePoint    { get; set; }
        public DateTime? PublishedAt   { get; set; }
    }
    private sealed class LowAttendanceApiDto
    {
        public decimal  ThresholdPercent    { get; set; }
        public int      TotalStudentsAtRisk { get; set; }
        public DateTime GeneratedAt         { get; set; }
        public List<LowAttendanceRowApiDto>? Rows { get; set; }
    }
    private sealed class LowAttendanceRowApiDto
    {
        public string? RegistrationNumber   { get; set; }
        public string? StudentName          { get; set; }
        public string? CourseCode           { get; set; }
        public string? CourseTitle          { get; set; }
        public string? SemesterName         { get; set; }
        public string? DepartmentName       { get; set; }
        public int     TotalSessions        { get; set; }
        public int     AttendedSessions     { get; set; }
        public decimal AttendancePercentage { get; set; }
    }
    private sealed class FypStatusApiDto
    {
        public int      TotalProjects { get; set; }
        public DateTime GeneratedAt   { get; set; }
        public List<FypStatusRowApiDto>? Rows { get; set; }
    }
    private sealed class FypStatusRowApiDto
    {
        public string?   Title              { get; set; }
        public string?   StudentName        { get; set; }
        public string?   RegistrationNumber { get; set; }
        public string?   DepartmentName     { get; set; }
        public string?   SupervisorName     { get; set; }
        public string?   Status             { get; set; }
        public DateTime  ProposedAt         { get; set; }
        public int       MeetingCount       { get; set; }
    }

    // ── Portal / Dashboard Settings ───────────────────────────────────────────

    public async Task<PortalBrandingWebModel> GetPortalBrandingAsync(CancellationToken ct)
    {
        var raw = await GetAsync<PortalBrandingApiDto>("api/v1/portal-settings", ct);
        return new PortalBrandingWebModel
        {
            UniversityName   = raw?.UniversityName   ?? "Tabsan EduSphere",
            PortalSubtitle   = raw?.PortalSubtitle   ?? "Campus Portal",
            FooterText       = raw?.FooterText       ?? "© 2026 Tabsan EduSphere",
            LogoImage        = raw?.LogoImage,
            PrivacyPolicyUrl = raw?.PrivacyPolicyUrl,
            PrivacyPolicyContent = raw?.PrivacyPolicyContent,
            FontFamily       = raw?.FontFamily,
            FontSize         = raw?.FontSize
        };
    }

    public async Task SavePortalBrandingAsync(PortalBrandingWebModel model, CancellationToken ct)
    {
        var payload = new
        {
            universityName   = model.UniversityName,
            portalSubtitle   = model.PortalSubtitle,
            footerText       = model.FooterText,
            logoImage        = model.LogoImage,
            privacyPolicyUrl = model.PrivacyPolicyUrl,
            privacyPolicyContent = model.PrivacyPolicyContent,
            fontFamily       = model.FontFamily,
            fontSize         = model.FontSize
        };
        await PostAsync<object, object>("api/v1/portal-settings", payload, ct);
    }

    public async Task<string?> UploadLogoAsync(Stream fileStream, string fileName, CancellationToken ct)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StreamContent(fileStream), "file", fileName);
        using var request  = CreateRequest(HttpMethod.Post, "api/v1/portal-settings/logo");
        request.Content    = content;
        using var response = await CreateClient().SendAsync(request, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode) return null;
        var json = JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body, _jsonOptions);
        return json.TryGetProperty("url", out var urlProp) ? NormalizeApiAssetUrl(urlProp.GetString()) : null;
    }

    private string? NormalizeApiAssetUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl)) return rawUrl;
        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out _)) return rawUrl;

        var baseUrl = GetConnection().ApiBaseUrl?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl)) return rawUrl;

        return rawUrl.StartsWith('/') ? $"{baseUrl}{rawUrl}" : $"{baseUrl}/{rawUrl}";
    }

    private sealed class PortalBrandingApiDto
    {
        public string? UniversityName   { get; set; }
        public string? PortalSubtitle   { get; set; }
        public string? FooterText       { get; set; }
        public string? LogoImage        { get; set; }
        public string? PrivacyPolicyUrl { get; set; }
        public string? PrivacyPolicyContent { get; set; }
        public string? FontFamily       { get; set; }
        public string? FontSize         { get; set; }
    }

    // ── Phase 12: Academic Calendar & Deadlines ──────────────────────────────

    public async Task<List<DeadlineWebItem>> GetCalendarDeadlinesAsync(Guid? semesterId, CancellationToken ct)
    {
        var path = semesterId.HasValue
            ? $"api/v1/calendar/deadlines/by-semester/{semesterId.Value}"
            : "api/v1/calendar/deadlines";
        return await GetAsync<List<DeadlineWebItem>>(path, ct) ?? new();
    }

    public Task<DeadlineWebItem?> GetCalendarDeadlineByIdAsync(Guid id, CancellationToken ct)
        => GetAsync<DeadlineWebItem>($"api/v1/calendar/deadlines/{id}", ct);

    public async Task CreateCalendarDeadlineAsync(DeadlineFormModel form, CancellationToken ct)
    {
        var payload = new
        {
            semesterId         = form.SemesterId,
            title              = form.Title,
            description        = form.Description,
            deadlineDate       = form.DeadlineDate,
            reminderDaysBefore = form.ReminderDaysBefore
        };
        await PostAsync<object, object>("api/v1/calendar/deadlines", payload, ct);
    }

    public async Task UpdateCalendarDeadlineAsync(Guid id, DeadlineFormModel form, CancellationToken ct)
    {
        var payload = new
        {
            title              = form.Title,
            description        = form.Description,
            deadlineDate       = form.DeadlineDate,
            reminderDaysBefore = form.ReminderDaysBefore,
            isActive           = form.IsActive
        };
        await PutAsync<object, object>($"api/v1/calendar/deadlines/{id}", payload, ct);
    }

    public Task DeleteCalendarDeadlineAsync(Guid id, CancellationToken ct)
        => DeleteAsync($"api/v1/calendar/deadlines/{id}", ct);

    // ── Phase 13: Global Search ───────────────────────────────────────────────

    public async Task<SearchWebResponse> SearchAsync(string term, int limit, CancellationToken ct)
    {
        var encoded = Uri.EscapeDataString(term);
        var raw = await GetAsync<SearchApiDto>($"api/v1/search?q={encoded}&limit={limit}", ct);
        if (raw is null) return new SearchWebResponse(term, 0, new());
        return new SearchWebResponse(
            raw.Term ?? term,
            raw.TotalHits,
            raw.Results?.Select(r => new SearchWebItem
            {
                Type     = r.Type     ?? "",
                Id       = r.Id,
                Label    = r.Label    ?? "",
                SubLabel = r.SubLabel ?? "",
                Url      = r.Url      ?? ""
            }).ToList() ?? new());
    }

    // ── Phase 13 API DTOs (private) ───────────────────────────────────────────
    private sealed class SearchApiDto
    {
        public string?                  Term      { get; set; }
        public int                      TotalHits { get; set; }
        public List<SearchResultApiDto>? Results  { get; set; }
    }
    private sealed class SearchResultApiDto
    {
        public string? Type     { get; set; }
        public Guid    Id       { get; set; }
        public string? Label    { get; set; }
        public string? SubLabel { get; set; }
        public string? Url      { get; set; }
    }

    // ── Phase 14: Helpdesk / Support Ticketing ────────────────────────────────

    public async Task<List<TicketSummaryItem>> GetTicketsAsync(TicketStatusWeb? status, CancellationToken ct)
    {
        var url = status.HasValue
            ? $"api/v1/helpdesk/tickets?status={(int)status.Value}"
            : "api/v1/helpdesk/tickets";
        var raw = await GetAsync<List<TicketSummaryApiDto>>(url, ct);
        return raw?.Select(MapSummary).ToList() ?? new();
    }

    public async Task<TicketDetailItem?> GetTicketDetailAsync(Guid ticketId, CancellationToken ct)
    {
        var raw = await GetAsync<TicketDetailApiDto>($"api/v1/helpdesk/tickets/{ticketId}", ct);
        if (raw is null) return null;
        return new TicketDetailItem
        {
            Id               = raw.Id,
            Subject          = raw.Subject ?? "",
            Body             = raw.Body ?? "",
            Category         = (TicketCategoryWeb)raw.Category,
            Status           = (TicketStatusWeb)raw.Status,
            SubmitterId      = raw.SubmitterId,
            SubmitterName    = raw.SubmitterName ?? "",
            AssignedToId     = raw.AssignedToId,
            AssigneeName     = raw.AssigneeName,
            DepartmentId     = raw.DepartmentId,
            CreatedAt        = raw.CreatedAt,
            ResolvedAt       = raw.ResolvedAt,
            ReopenWindowDays = raw.ReopenWindowDays,
            CanReopen        = raw.CanReopen,
            Messages         = raw.Messages?.Select(m => new TicketMessageItem
            {
                Id             = m.Id,
                AuthorId       = m.AuthorId,
                AuthorName     = m.AuthorName ?? "",
                Body           = m.Body ?? "",
                IsInternalNote = m.IsInternalNote,
                CreatedAt      = m.CreatedAt
            }).ToList() ?? new()
        };
    }

    public async Task<Guid> CreateTicketAsync(Guid? departmentId, TicketCategoryWeb category,
        string subject, string body, CancellationToken ct)
    {
        var req = new { departmentId, category = (int)category, subject, body };
        var res = await PostAsync<object, TicketIdApiDto>("api/v1/helpdesk/tickets", req, ct);
        return res?.Id ?? Guid.Empty;
    }

    public async Task<Guid> AddTicketMessageAsync(Guid ticketId, string body, bool isInternalNote, CancellationToken ct)
    {
        var req = new { body, isInternalNote };
        var res = await PostAsync<object, TicketIdApiDto>($"api/v1/helpdesk/tickets/{ticketId}/messages", req, ct);
        return res?.Id ?? Guid.Empty;
    }

    public Task AssignTicketAsync(Guid ticketId, Guid assignedToId, CancellationToken ct)
    {
        var req = new { assignedToId };
        return PutAsync<object, object>($"api/v1/helpdesk/tickets/{ticketId}/assign", req, ct);
    }

    public Task ResolveTicketAsync(Guid ticketId, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/helpdesk/tickets/{ticketId}/resolve", new { }, ct);

    public Task CloseTicketAsync(Guid ticketId, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/helpdesk/tickets/{ticketId}/close", new { }, ct);

    public Task ReopenTicketAsync(Guid ticketId, CancellationToken ct)
        => PutAsync<object, object>($"api/v1/helpdesk/tickets/{ticketId}/reopen", new { }, ct);

    // ── Phase 14 API DTOs (private) ───────────────────────────────────────────

    private static TicketSummaryItem MapSummary(TicketSummaryApiDto d) => new()
    {
        Id            = d.Id,
        Subject       = d.Subject       ?? "",
        Category      = (TicketCategoryWeb)d.Category,
        Status        = (TicketStatusWeb)d.Status,
        SubmitterId   = d.SubmitterId,
        SubmitterName = d.SubmitterName ?? "",
        AssignedToId  = d.AssignedToId,
        AssigneeName  = d.AssigneeName,
        DepartmentId  = d.DepartmentId,
        CreatedAt     = d.CreatedAt,
        ResolvedAt    = d.ResolvedAt,
        MessageCount  = d.MessageCount
    };

    private sealed class TicketIdApiDto         { public Guid Id { get; set; } }
    private sealed class TicketSummaryApiDto
    {
        public Guid    Id            { get; set; }
        public string? Subject       { get; set; }
        public int     Category      { get; set; }
        public int     Status        { get; set; }
        public Guid    SubmitterId   { get; set; }
        public string? SubmitterName { get; set; }
        public Guid?   AssignedToId  { get; set; }
        public string? AssigneeName  { get; set; }
        public Guid?   DepartmentId  { get; set; }
        public DateTime CreatedAt   { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public int     MessageCount  { get; set; }
    }
    private sealed class TicketDetailApiDto
    {
        public Guid    Id               { get; set; }
        public string? Subject          { get; set; }
        public string? Body             { get; set; }
        public int     Category         { get; set; }
        public int     Status           { get; set; }
        public Guid    SubmitterId      { get; set; }
        public string? SubmitterName    { get; set; }
        public Guid?   AssignedToId     { get; set; }
        public string? AssigneeName     { get; set; }
        public Guid?   DepartmentId     { get; set; }
        public DateTime  CreatedAt      { get; set; }
        public DateTime? ResolvedAt     { get; set; }
        public int     ReopenWindowDays { get; set; }
        public bool    CanReopen        { get; set; }
        public List<TicketMessageApiDto>? Messages { get; set; }
    }
    private sealed class TicketMessageApiDto
    {
        public Guid    Id             { get; set; }
        public Guid    AuthorId       { get; set; }
        public string? AuthorName     { get; set; }
        public string? Body           { get; set; }
        public bool    IsInternalNote { get; set; }
        public DateTime CreatedAt    { get; set; }
    }
}

