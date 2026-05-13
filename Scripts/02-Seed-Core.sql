SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_ID(N'Tabsan-EduSphere') IS NULL
BEGIN
    RAISERROR('Database [Tabsan-EduSphere] does not exist. Run 01-Schema-Current.sql first.', 16, 1);
    RETURN;
END;
GO

USE [Tabsan-EduSphere];
GO

IF DB_NAME() <> N'Tabsan-EduSphere'
BEGIN
    RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting seed script.', 16, 1);
    RETURN;
END;
GO

BEGIN TRANSACTION;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();

/* 1) System roles */
MERGE INTO [roles] AS tgt
USING (
    SELECT N'SuperAdmin' AS [Name], N'Full platform access - manages license and all settings.' AS [Description], CAST(1 AS bit) AS [IsSystemRole]
    UNION ALL SELECT N'Admin', N'Department-level admin - manages users and courses.', CAST(1 AS bit)
    UNION ALL SELECT N'Faculty', N'Teaches courses and manages academic content.', CAST(1 AS bit)
    UNION ALL SELECT N'Student', N'Enrolled student - accesses course and academic content.', CAST(1 AS bit)
) AS src
ON tgt.[Name] = src.[Name]
WHEN MATCHED THEN
    UPDATE SET tgt.[Description] = src.[Description], tgt.[IsSystemRole] = src.[IsSystemRole]
WHEN NOT MATCHED THEN
    INSERT ([Name], [Description], [IsSystemRole])
    VALUES (src.[Name], src.[Description], src.[IsSystemRole]);

/* 2) Modules + default module_status */
DECLARE @Modules TABLE ([Key] NVARCHAR(50), [Name] NVARCHAR(100), [IsMandatory] bit);
INSERT INTO @Modules ([Key], [Name], [IsMandatory]) VALUES
(N'authentication', N'Authentication', 1),
(N'departments', N'Departments', 1),
(N'sis', N'Student Information', 1),
(N'courses', N'Courses', 0),
(N'assignments', N'Assignments', 0),
(N'quizzes', N'Quizzes', 0),
(N'attendance', N'Attendance', 0),
(N'results', N'Results / Grades', 0),
(N'notifications', N'Notifications', 0),
(N'fyp', N'Final Year Projects', 0),
(N'ai-chat', N'AI Chatbot', 0),
(N'reports', N'Reports', 0),
(N'theming', N'UI Themes', 0),
(N'advanced-audit', N'Advanced Audit Logging', 0);

INSERT INTO [modules] ([Id], [Key], [Name], [IsMandatory], [CreatedAt], [UpdatedAt])
SELECT NEWID(), m.[Key], m.[Name], m.[IsMandatory], @Now, NULL
FROM @Modules m
WHERE NOT EXISTS (SELECT 1 FROM [modules] x WHERE x.[Key] = m.[Key]);

INSERT INTO [module_status] ([Id], [ModuleId], [IsActive], [ActivatedAt], [Source], [ChangedBy], [CreatedAt], [UpdatedAt])
SELECT NEWID(), m.[Id], CASE WHEN m.[IsMandatory] = 1 THEN 1 ELSE 0 END,
       CASE WHEN m.[IsMandatory] = 1 THEN @Now ELSE NULL END,
       CASE WHEN m.[IsMandatory] = 1 THEN N'mandatory' ELSE N'seed' END,
       NULL,
       @Now,
       NULL
FROM [modules] m
WHERE NOT EXISTS (SELECT 1 FROM [module_status] s WHERE s.[ModuleId] = m.[Id]);

/* 3) Portal settings */
MERGE INTO [portal_settings] AS tgt
USING (
    SELECT N'portal.universityName' AS [Key], N'Tabsan EduSphere University' AS [Value]
    UNION ALL SELECT N'portal.brandInitials', N'TE'
    UNION ALL SELECT N'portal.theme', N'default'
    UNION ALL SELECT N'portal.timeZone', N'UTC'
    UNION ALL SELECT N'institution_include_school', N'true'
    UNION ALL SELECT N'institution_include_college', N'true'
    UNION ALL SELECT N'institution_include_university', N'true'
) AS src
ON tgt.[Key] = src.[Key]
WHEN MATCHED THEN
    UPDATE SET tgt.[Value] = src.[Value], tgt.[UpdatedAt] = @Now
WHEN NOT MATCHED THEN
    INSERT ([Id], [Key], [Value], [CreatedAt], [UpdatedAt])
    VALUES (NEWID(), src.[Key], src.[Value], @Now, NULL);

/* 4) Baseline institute-aware departments (School, College, University) */
DECLARE @CoreDepartments TABLE ([Id] UNIQUEIDENTIFIER, [Name] NVARCHAR(200), [Code] NVARCHAR(20), [InstitutionType] INT);
INSERT INTO @CoreDepartments ([Id], [Name], [Code], [InstitutionType]) VALUES
(CAST('21000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), N'Core University Department', N'CORE-UNI', 0),
(CAST('21000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), N'Core College Department', N'CORE-COL', 1),
(CAST('21000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), N'Core School Department', N'CORE-SCH', 2);

INSERT INTO [departments] ([Id], [Name], [Code], [InstitutionType], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT d.[Id], d.[Name], d.[Code], d.[InstitutionType], 1, @Now, NULL, 0, NULL
FROM @CoreDepartments d
WHERE NOT EXISTS (SELECT 1 FROM [departments] x WHERE x.[Id] = d.[Id]);

/* 5) Baseline report definitions + role assignments */
-- Normalize legacy report keys from older seed scripts to current underscore keys.
UPDATE [report_definitions]
SET [Key] = N'academic_transcript', [UpdatedAt] = @Now
WHERE [Key] = N'academic-transcript'
    AND NOT EXISTS (SELECT 1 FROM [report_definitions] x WHERE x.[Key] = N'academic_transcript');

UPDATE [report_definitions]
SET [Key] = N'attendance_summary', [UpdatedAt] = @Now
WHERE [Key] = N'attendance-summary'
    AND NOT EXISTS (SELECT 1 FROM [report_definitions] x WHERE x.[Key] = N'attendance_summary');

UPDATE [report_definitions]
SET [Key] = N'result_summary', [UpdatedAt] = @Now
WHERE [Key] = N'result-sheet'
    AND NOT EXISTS (SELECT 1 FROM [report_definitions] x WHERE x.[Key] = N'result_summary');

DECLARE @Reports TABLE ([Key] NVARCHAR(100), [Name] NVARCHAR(150), [Purpose] NVARCHAR(500));
INSERT INTO @Reports ([Key], [Name], [Purpose]) VALUES
(N'attendance_summary', N'Attendance Summary', N'Per-student attendance percentage per course offering, filterable by semester and department.'),
(N'result_summary', N'Result Summary', N'All published result entries with marks and percentage, filterable by semester, offering, or student.'),
(N'gpa_report', N'GPA & CGPA Report', N'Per-student current semester GPA and cumulative CGPA, filterable by department and program.'),
(N'enrollment_summary', N'Enrollment Summary', N'Course offering seat utilisation showing enrolled count versus maximum capacity.'),
(N'semester_results', N'Semester Results', N'Full published result set for a selected semester with optional department filter.'),
(N'student_transcript', N'Student Transcript', N'Full academic record for a selected student including all result components.'),
(N'low_attendance_warning', N'Low Attendance Warning', N'Students whose attendance falls below a configurable threshold.'),
(N'fyp_status', N'FYP Status Report', N'Final Year Project status overview filterable by department and project status.');

INSERT INTO [report_definitions] ([Id], [Name], [Purpose], [Key], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT NEWID(), r.[Name], r.[Purpose], r.[Key], 1, @Now, NULL, 0, NULL
FROM @Reports r
WHERE NOT EXISTS (SELECT 1 FROM [report_definitions] d WHERE d.[Key] = r.[Key]);

INSERT INTO [report_role_assignments] ([Id], [ReportDefinitionId], [RoleName], [CreatedAt], [UpdatedAt])
SELECT NEWID(), d.[Id], rr.[RoleName], @Now, NULL
FROM [report_definitions] d
CROSS APPLY (VALUES (N'SuperAdmin'), (N'Admin'), (N'Faculty')) rr([RoleName])
WHERE d.[Key] IN (
        N'attendance_summary',
        N'result_summary',
        N'gpa_report',
        N'enrollment_summary',
        N'semester_results',
        N'student_transcript',
        N'low_attendance_warning',
        N'fyp_status')
  AND NOT EXISTS (
      SELECT 1
      FROM [report_role_assignments] x
      WHERE x.[ReportDefinitionId] = d.[Id] AND x.[RoleName] = rr.[RoleName]
  );

INSERT INTO [report_role_assignments] ([Id], [ReportDefinitionId], [RoleName], [CreatedAt], [UpdatedAt])
SELECT NEWID(), d.[Id], N'Student', @Now, NULL
FROM [report_definitions] d
WHERE d.[Key] = N'student_transcript'
    AND NOT EXISTS (
            SELECT 1
            FROM [report_role_assignments] x
            WHERE x.[ReportDefinitionId] = d.[Id] AND x.[RoleName] = N'Student'
    );

/* 6) Baseline sidebar menus */
DECLARE @DashboardId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [sidebar_menu_items] WHERE [Key] = N'dashboard');
IF @DashboardId IS NULL
BEGIN
    SET @DashboardId = NEWID();
    INSERT INTO [sidebar_menu_items] ([Id], [Name], [Purpose], [Key], [ParentId], [DisplayOrder], [IsActive], [IsSystemMenu], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    VALUES (@DashboardId, N'Dashboard', N'Main landing page', N'dashboard', NULL, 1, 1, 1, @Now, NULL, 0, NULL);
END

DECLARE @AcademicId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [sidebar_menu_items] WHERE [Key] = N'academic');
IF @AcademicId IS NULL
BEGIN
    SET @AcademicId = NEWID();
    INSERT INTO [sidebar_menu_items] ([Id], [Name], [Purpose], [Key], [ParentId], [DisplayOrder], [IsActive], [IsSystemMenu], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    VALUES (@AcademicId, N'Academic', N'Core academic operations', N'academic', NULL, 2, 1, 0, @Now, NULL, 0, NULL);
END

DECLARE @CoursesMenuId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [sidebar_menu_items] WHERE [Key] = N'courses');
IF @CoursesMenuId IS NULL
BEGIN
    SET @CoursesMenuId = NEWID();
    INSERT INTO [sidebar_menu_items] ([Id], [Name], [Purpose], [Key], [ParentId], [DisplayOrder], [IsActive], [IsSystemMenu], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    VALUES (@CoursesMenuId, N'Courses', N'Course catalog and offerings', N'courses', @AcademicId, 1, 1, 0, @Now, NULL, 0, NULL);
END

DECLARE @AttendanceMenuId UNIQUEIDENTIFIER = (SELECT TOP 1 [Id] FROM [sidebar_menu_items] WHERE [Key] = N'attendance');
IF @AttendanceMenuId IS NULL
BEGIN
    SET @AttendanceMenuId = NEWID();
    INSERT INTO [sidebar_menu_items] ([Id], [Name], [Purpose], [Key], [ParentId], [DisplayOrder], [IsActive], [IsSystemMenu], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    VALUES (@AttendanceMenuId, N'Attendance', N'Attendance marking and views', N'attendance', @AcademicId, 2, 1, 0, @Now, NULL, 0, NULL);
END

INSERT INTO [sidebar_menu_role_accesses] ([Id], [SidebarMenuItemId], [RoleName], [IsAllowed], [CreatedAt], [UpdatedAt])
SELECT NEWID(), m.[Id], ra.[RoleName], ra.[IsAllowed], @Now, NULL
FROM [sidebar_menu_items] m
JOIN (VALUES
    (N'dashboard', N'SuperAdmin', 1),
    (N'dashboard', N'Admin', 1),
    (N'dashboard', N'Faculty', 1),
    (N'dashboard', N'Student', 1),
    (N'academic', N'SuperAdmin', 1),
    (N'academic', N'Admin', 1),
    (N'courses', N'SuperAdmin', 1),
    (N'courses', N'Admin', 1),
    (N'courses', N'Faculty', 1),
    (N'attendance', N'SuperAdmin', 1),
    (N'attendance', N'Admin', 1),
    (N'attendance', N'Faculty', 1),
    (N'attendance', N'Student', 1)
) ra([MenuKey], [RoleName], [IsAllowed]) ON ra.[MenuKey] = m.[Key]
WHERE NOT EXISTS (
    SELECT 1
    FROM [sidebar_menu_role_accesses] x
    WHERE x.[SidebarMenuItemId] = m.[Id] AND x.[RoleName] = ra.[RoleName]
);

COMMIT TRANSACTION;
PRINT 'Core seed completed.';
