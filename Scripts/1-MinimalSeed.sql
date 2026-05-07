-- ============================================================
-- Tabsan EduSphere  —  Minimal Seed  (Script 1 of 2)
-- ============================================================
-- PURPOSE : Bootstrap a fresh database with all required system
--           data (license, modules, portal, sidebar, reports,
--           GPA / result rules) plus four essential test accounts.
--
-- PASSWORD: Replace the PLACEHOLDER below before running.
--           From the repository root, run:
--               .\Scripts\GenerateTestHashes.ps1
--           This patches the placeholder automatically.
--
-- TEST ACCOUNTS
--   Role        Username      Email
--   ----------- ------------- --------------------------------
--   SuperAdmin  superadmin    superadmin@edusphere.local
--   Admin       admin.it      admin.it@edusphere.local
--   Faculty     dr.ahmed      ahmed@edusphere.local
--   Student     s.aslam       aslam@student.edusphere.local
--
-- SAFE   : Fully idempotent — safe to re-run on any database.
-- DEPENDS: 0-Schema.sql must have been applied first.
-- ============================================================

USE TabsanEduSphere;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @PwdHash NVARCHAR(512) = N'PLACEHOLDER_RUN_GenerateTestHashes.ps1_TO_REPLACE';
DECLARE @Now     DATETIME2     = SYSUTCDATETIME();

-- ─────────────────────────────────────────────────────────────
-- 1. LICENSE STATE
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [license_state])
BEGIN
    INSERT INTO [license_state]
        ([Id],[LicenseHash],[LicenseType],[Status],[ActivatedAt],[ExpiresAt],
         [MaxUsers],[ActivatedDomain],[CreatedAt],[UpdatedAt])
    VALUES
        (NEWID(),
         N'DEMO-LICENSE-HASH-000000000000000000000000000000000000000000000000000000000000000000000',
         N'Standard', N'Active',
         DATEADD(MONTH,-1,@Now),
         DATEADD(YEAR,1,@Now),
         500, N'edusphere.local',
         @Now, @Now);
END;

-- ─────────────────────────────────────────────────────────────
-- 2. MODULES (fixed GUIDs matching live DB)
-- ─────────────────────────────────────────────────────────────
INSERT INTO [modules] ([Id],[Key],[Name],[IsMandatory],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Key], v.[Name], v.[IsMandatory], @Now, NULL
FROM (VALUES
    (CAST(N'3A0EE170-EEB9-4F86-9C1A-F90AD20E8B8B' AS UNIQUEIDENTIFIER), N'authentication',  N'Authentication',          1),
    (CAST(N'1826E827-F968-48EA-9D77-9061F8E70372' AS UNIQUEIDENTIFIER), N'departments',     N'Departments',             1),
    (CAST(N'F7BF7154-A3F4-4FF5-9CF7-FC80AB791206' AS UNIQUEIDENTIFIER), N'courses',         N'Courses',                 1),
    (CAST(N'419A0CA9-8AED-40B1-A89D-0CD69CE4E30C' AS UNIQUEIDENTIFIER), N'sis',             N'Student Information',     1),
    (CAST(N'0AE5AE4C-AE60-4356-9D24-17395930702E' AS UNIQUEIDENTIFIER), N'assignments',     N'Assignments',             0),
    (CAST(N'0D4D3736-3BE1-4E9D-BF58-3F6D0E6E3182' AS UNIQUEIDENTIFIER), N'attendance',      N'Attendance',              0),
    (CAST(N'4C475357-68B8-435D-AA47-2ABDCC84439B' AS UNIQUEIDENTIFIER), N'results',         N'Results / Grades',        0),
    (CAST(N'1501DC20-ACD9-4FE2-A99F-4CD0C4EEC0FD' AS UNIQUEIDENTIFIER), N'quizzes',         N'Quizzes',                 0),
    (CAST(N'48740CC0-C769-4F5E-9AB7-4F29F2D7BDC0' AS UNIQUEIDENTIFIER), N'fyp',             N'Final Year Projects',     0),
    (CAST(N'2228406F-0A43-4028-ADF6-8487F37ADB7D' AS UNIQUEIDENTIFIER), N'notifications',   N'Notifications',           0),
    (CAST(N'5F2B426E-B512-4DF4-92A4-1ADFEBF74DA0' AS UNIQUEIDENTIFIER), N'reports',         N'Reports',                 0),
    (CAST(N'722E6611-556E-489F-8FDD-875122BBC3D1' AS UNIQUEIDENTIFIER), N'ai_chat',         N'AI Chatbot',              0),
    (CAST(N'9A2E61E4-8F2D-4F2F-AA93-1629F3E92645' AS UNIQUEIDENTIFIER), N'themes',          N'UI Themes',               0),
    (CAST(N'3B90884B-64D3-4955-80C8-0CEE37BF2536' AS UNIQUEIDENTIFIER), N'advanced_audit',  N'Advanced Audit Logging',  0)
) AS v([Id],[Key],[Name],[IsMandatory])
WHERE NOT EXISTS (SELECT 1 FROM [modules] m WHERE m.[Id] = v.[Id]);

-- ─────────────────────────────────────────────────────────────
-- 3. MODULE STATUS (activate all modules)
-- ─────────────────────────────────────────────────────────────
INSERT INTO [module_status] ([Id],[ModuleId],[IsActive],[ActivatedAt],[Source],[ChangedBy],[CreatedAt],[UpdatedAt])
SELECT NEWID(), m.[Id], 1, @Now, N'Seed', NULL, @Now, NULL
FROM [modules] m
WHERE NOT EXISTS (SELECT 1 FROM [module_status] ms WHERE ms.[ModuleId] = m.[Id]);

-- ─────────────────────────────────────────────────────────────
-- 4. MODULE ROLE ASSIGNMENTS
-- ─────────────────────────────────────────────────────────────
;WITH ModRoles AS (
    SELECT m.[Id] AS ModuleId, r.RoleName
    FROM [modules] m
    CROSS JOIN (VALUES (N'SuperAdmin'),(N'Admin'),(N'Faculty'),(N'Student')) r(RoleName)
    WHERE m.[Key] IN (N'authentication',N'courses',N'sis',N'assignments',
                       N'attendance',N'results',N'quizzes',N'fyp',N'notifications',
                       N'reports',N'ai_chat')
    UNION ALL
    -- departments: SuperAdmin + Admin only
    SELECT m.[Id], r.RoleName FROM [modules] m
    CROSS JOIN (VALUES (N'SuperAdmin'),(N'Admin')) r(RoleName)
    WHERE m.[Key] = N'departments'
    UNION ALL
    -- themes: SuperAdmin + Admin only
    SELECT m.[Id], r.RoleName FROM [modules] m
    CROSS JOIN (VALUES (N'SuperAdmin'),(N'Admin')) r(RoleName)
    WHERE m.[Key] = N'themes'
    UNION ALL
    -- advanced_audit: SuperAdmin only
    SELECT m.[Id], N'SuperAdmin' FROM [modules] m WHERE m.[Key] = N'advanced_audit'
)
INSERT INTO [module_role_assignments] ([Id],[ModuleId],[RoleName],[CreatedAt],[UpdatedAt])
SELECT NEWID(), mr.ModuleId, mr.RoleName, @Now, NULL
FROM ModRoles mr
WHERE NOT EXISTS (
    SELECT 1 FROM [module_role_assignments] ex
    WHERE ex.[ModuleId] = mr.ModuleId AND ex.[RoleName] = mr.RoleName
);

-- ─────────────────────────────────────────────────────────────
-- 5. GPA SCALE RULES (standard 4-point scale)
-- ─────────────────────────────────────────────────────────────
INSERT INTO [gpa_scale_rules] ([Id],[GradePoint],[MinimumScore],[DisplayOrder],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[GP], v.[MinScore], v.[Ord], @Now, NULL
FROM (VALUES
    (CAST(N'AA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), 4.00, 90.00, 1),
    (CAST(N'AA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), 3.70, 85.00, 2),
    (CAST(N'AA000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), 3.30, 80.00, 3),
    (CAST(N'AA000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER), 3.00, 75.00, 4),
    (CAST(N'AA000000-0000-0000-0000-000000000005' AS UNIQUEIDENTIFIER), 2.70, 70.00, 5),
    (CAST(N'AA000000-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER), 2.30, 65.00, 6),
    (CAST(N'AA000000-0000-0000-0000-000000000007' AS UNIQUEIDENTIFIER), 2.00, 60.00, 7),
    (CAST(N'AA000000-0000-0000-0000-000000000008' AS UNIQUEIDENTIFIER), 1.70, 55.00, 8),
    (CAST(N'AA000000-0000-0000-0000-000000000009' AS UNIQUEIDENTIFIER), 1.30, 50.00, 9),
    (CAST(N'AA000000-0000-0000-0000-000000000010' AS UNIQUEIDENTIFIER), 1.00, 45.00, 10),
    (CAST(N'AA000000-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER), 0.00,  0.00, 11)
) AS v([Id],[GP],[MinScore],[Ord])
WHERE NOT EXISTS (SELECT 1 FROM [gpa_scale_rules] WHERE [Id] = v.[Id]);

-- ─────────────────────────────────────────────────────────────
-- 6. RESULT COMPONENT RULES
-- ─────────────────────────────────────────────────────────────
INSERT INTO [result_component_rules] ([Id],[Name],[Weightage],[DisplayOrder],[IsActive],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Name], v.[Wt], v.[Ord], 1, @Now, NULL
FROM (VALUES
    (CAST(N'AB000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER), N'Midterm',    30.00, 1),
    (CAST(N'AB000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER), N'Final',      50.00, 2),
    (CAST(N'AB000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER), N'Assignment', 10.00, 3),
    (CAST(N'AB000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER), N'Quiz',       10.00, 4)
) AS v([Id],[Name],[Wt],[Ord])
WHERE NOT EXISTS (SELECT 1 FROM [result_component_rules] WHERE [Id] = v.[Id]);

-- ─────────────────────────────────────────────────────────────
-- 7. PORTAL SETTINGS (branding defaults — skip keys that exist)
-- ─────────────────────────────────────────────────────────────
INSERT INTO [portal_settings] ([Id],[Key],[Value],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[Key], v.[Value], @Now, NULL
FROM (VALUES
    (N'university_name',        N'Tabsan EduSphere'),
    (N'portal_subtitle',        N'Campus Management Portal'),
    (N'footer_text',            N'© 2026 Tabsan EduSphere. All rights reserved.'),
    (N'logo_url',               N''),
    (N'logo_image',             N''),
    (N'font_family',            N''),
    (N'font_size',              N''),
    (N'brand_initials',         N'TE'),
    (N'privacy_policy_url',     N''),
    (N'privacy_policy_content', N'Your privacy matters. Student data is used only for academic workflows within this campus portal.')
) AS v([Key],[Value])
WHERE NOT EXISTS (SELECT 1 FROM [portal_settings] WHERE [Key] = v.[Key]);

-- ─────────────────────────────────────────────────────────────
-- 8. REPORT DEFINITIONS + ROLE ASSIGNMENTS
-- ─────────────────────────────────────────────────────────────
INSERT INTO [report_definitions] ([Id],[Name],[Purpose],[Key],[IsActive],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Name], v.[Purpose], v.[Key], 1, @Now, NULL
FROM (VALUES
    (CAST(N'E870DE31-9FDA-4008-A2EA-01643FB30B45' AS UNIQUEIDENTIFIER), N'Attendance Summary',     N'Per-student attendance percentage per course offering, filterable by semester and department.',        N'attendance_summary'),
    (CAST(N'D234B5BD-DBC8-4F77-8C88-3B9A2FDFAE8F' AS UNIQUEIDENTIFIER), N'Enrollment Summary',     N'Course offering seat utilisation showing enrolled count versus maximum capacity.',                      N'enrollment_summary'),
    (CAST(N'9A407E16-0AA7-4782-9C8D-5AD5B594DF20' AS UNIQUEIDENTIFIER), N'FYP Status Report',      N'Final Year Project status overview filterable by department and project status.',                       N'fyp_status'),
    (CAST(N'56995606-1936-4163-BB8D-61BF2242C5BC' AS UNIQUEIDENTIFIER), N'GPA & CGPA Report',      N'Per-student current semester GPA and cumulative CGPA, filterable by department and program.',          N'gpa_report'),
    (CAST(N'84BA2F4B-D804-4CF5-AA2F-621CB7D47493' AS UNIQUEIDENTIFIER), N'Low Attendance Warning', N'Students whose attendance falls below a configurable threshold.',                                       N'low_attendance_warning'),
    (CAST(N'FF67A92D-4154-458C-8EE3-A01055340412' AS UNIQUEIDENTIFIER), N'Result Summary',         N'All published result entries with marks and percentage, filterable by semester, offering, or student.', N'result_summary'),
    (CAST(N'613116DF-C7DB-4A7A-A9C9-6D30D093F36C' AS UNIQUEIDENTIFIER), N'Semester Results',       N'Full published result set for a selected semester with optional department filter.',                    N'semester_results'),
    (CAST(N'E32BC054-EA26-4160-AE13-C4DB2D9DD1ED' AS UNIQUEIDENTIFIER), N'Student Transcript',     N'Full academic record for a selected student including all result components.',                          N'student_transcript')
) AS v([Id],[Name],[Purpose],[Key])
WHERE NOT EXISTS (SELECT 1 FROM [report_definitions] WHERE [Id] = v.[Id]);

;WITH ReportRoles AS (
    SELECT v.[ReportId], v.[RoleName]
    FROM (VALUES
        (CAST(N'E870DE31-9FDA-4008-A2EA-01643FB30B45' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'E870DE31-9FDA-4008-A2EA-01643FB30B45' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'E870DE31-9FDA-4008-A2EA-01643FB30B45' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'D234B5BD-DBC8-4F77-8C88-3B9A2FDFAE8F' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'D234B5BD-DBC8-4F77-8C88-3B9A2FDFAE8F' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'D234B5BD-DBC8-4F77-8C88-3B9A2FDFAE8F' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'9A407E16-0AA7-4782-9C8D-5AD5B594DF20' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'9A407E16-0AA7-4782-9C8D-5AD5B594DF20' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'9A407E16-0AA7-4782-9C8D-5AD5B594DF20' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'56995606-1936-4163-BB8D-61BF2242C5BC' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'56995606-1936-4163-BB8D-61BF2242C5BC' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'56995606-1936-4163-BB8D-61BF2242C5BC' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'56995606-1936-4163-BB8D-61BF2242C5BC' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'84BA2F4B-D804-4CF5-AA2F-621CB7D47493' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'84BA2F4B-D804-4CF5-AA2F-621CB7D47493' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'84BA2F4B-D804-4CF5-AA2F-621CB7D47493' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'84BA2F4B-D804-4CF5-AA2F-621CB7D47493' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'FF67A92D-4154-458C-8EE3-A01055340412' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'FF67A92D-4154-458C-8EE3-A01055340412' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'FF67A92D-4154-458C-8EE3-A01055340412' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'FF67A92D-4154-458C-8EE3-A01055340412' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'613116DF-C7DB-4A7A-A9C9-6D30D093F36C' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'613116DF-C7DB-4A7A-A9C9-6D30D093F36C' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'613116DF-C7DB-4A7A-A9C9-6D30D093F36C' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'E32BC054-EA26-4160-AE13-C4DB2D9DD1ED' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'E32BC054-EA26-4160-AE13-C4DB2D9DD1ED' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'E32BC054-EA26-4160-AE13-C4DB2D9DD1ED' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'E32BC054-EA26-4160-AE13-C4DB2D9DD1ED' AS UNIQUEIDENTIFIER), N'Student')
    ) AS v([ReportId],[RoleName])
)
INSERT INTO [report_role_assignments] ([Id],[ReportDefinitionId],[RoleName],[CreatedAt],[UpdatedAt])
SELECT NEWID(), rr.ReportId, rr.RoleName, @Now, NULL
FROM ReportRoles rr
WHERE NOT EXISTS (
    SELECT 1 FROM [report_role_assignments] ex
    WHERE ex.[ReportDefinitionId] = rr.ReportId AND ex.[RoleName] = rr.RoleName
);

-- ─────────────────────────────────────────────────────────────
-- 9. SIDEBAR MENU ITEMS
-- ─────────────────────────────────────────────────────────────
INSERT INTO [sidebar_menu_items] ([Id],[Name],[Purpose],[Key],[ParentId],[DisplayOrder],[IsActive],[IsSystemMenu],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Name], v.[Purpose], v.[Key], NULL, v.[Ord], 1, 1, @Now, NULL
FROM (VALUES
    (CAST(N'30A1025F-BAC8-4847-AE59-F0D1BAD8F506' AS UNIQUEIDENTIFIER), N'Dashboard',              N'Main overview dashboard',                     N'dashboard',           1),
    (CAST(N'AFFC4796-6040-4FB4-BE44-BF49410F6F15' AS UNIQUEIDENTIFIER), N'Timetable Admin',        N'Manage department timetables',                N'timetable_admin',     2),
    (CAST(N'ACD69922-A686-4E1E-B8DE-2DDB76810E97' AS UNIQUEIDENTIFIER), N'My Timetable (Faculty)', N'Faculty teaching schedule',                   N'timetable_teacher',   3),
    (CAST(N'946AE4D5-9894-4D34-B0D9-F0ACDEF9BCB6' AS UNIQUEIDENTIFIER), N'My Timetable (Student)', N'Student class schedule',                      N'timetable_student',   4),
    (CAST(N'72BFB9B9-5DF7-4CCB-BD52-EA3A6B626BF4' AS UNIQUEIDENTIFIER), N'Lookups',                N'Reference data management',                   N'lookups',             5),
    (CAST(N'35CEAEB0-A6DF-43A8-A41D-D6225696B924' AS UNIQUEIDENTIFIER), N'System Settings',        N'Platform configuration (SuperAdmin)',          N'system_settings',     6),
    (CAST(N'662BBCF7-B1CF-4AF6-829C-C6314534620B' AS UNIQUEIDENTIFIER), N'Result Calculation',     N'Configure GPA scale and assessment weights',  N'result_calculation',  7),
    (CAST(N'CEFE1A96-905C-405B-8ED6-D4902C531A43' AS UNIQUEIDENTIFIER), N'Notifications',          N'View system and academic notifications',       N'notifications',       8),
    (CAST(N'A062C9DF-45A2-4C4C-AD97-CC763C8A200C' AS UNIQUEIDENTIFIER), N'Students',               N'Manage student profiles',                     N'students',            9),
    (CAST(N'3DA9193D-BA9C-4E25-916E-D0C9A12DAF65' AS UNIQUEIDENTIFIER), N'Departments',            N'Manage academic departments',                 N'departments',        10),
    (CAST(N'B521C9BD-F1F3-4C18-B976-A55F29DF82FA' AS UNIQUEIDENTIFIER), N'Courses',                N'Manage courses and offerings',                N'courses',            11),
    (CAST(N'36DC3134-B3EC-484C-BF81-5E4EAD7AE670' AS UNIQUEIDENTIFIER), N'Assignments',            N'Manage and submit assignments',               N'assignments',        12),
    (CAST(N'C29CDA68-D18B-4885-9F7F-160F4C7FD9EA' AS UNIQUEIDENTIFIER), N'Attendance',             N'Record and view attendance',                  N'attendance',         13),
    (CAST(N'42AAE8E1-F4DA-49CB-A185-6960833B5514' AS UNIQUEIDENTIFIER), N'Results',                N'View and publish academic results',           N'results',            14),
    (CAST(N'49CB94D3-A88F-43C1-8D40-BDDC5415675E' AS UNIQUEIDENTIFIER), N'Quizzes',                N'Manage and attempt quizzes',                  N'quizzes',            15),
    (CAST(N'65C242B5-7BE1-40AD-9AD4-50E86092055D' AS UNIQUEIDENTIFIER), N'FYP',                    N'Final Year Projects management',              N'fyp',                16),
    (CAST(N'70C7D869-8ECA-41CF-AC13-91E2C0375E89' AS UNIQUEIDENTIFIER), N'Analytics',              N'Academic analytics and dashboards',           N'analytics',          17),
    (CAST(N'8B3239A2-6FA5-4CF4-96FC-F25163551180' AS UNIQUEIDENTIFIER), N'AI Chat',                N'AI-powered academic assistant',               N'ai_chat',            18),
    (CAST(N'BF4DDC33-FBC7-4256-9DA4-0D3879CE3CE2' AS UNIQUEIDENTIFIER), N'Student Lifecycle',      N'Manage promotions, holds and withdrawals',    N'student_lifecycle',  19),
    (CAST(N'13963044-779D-4E60-868D-FBFD4614CD7B' AS UNIQUEIDENTIFIER), N'Payments',               N'Manage and view fee payment records',         N'payments',           20),
    (CAST(N'18D1697A-1093-4C7D-BE5C-2FB5AE9853D0' AS UNIQUEIDENTIFIER), N'Enrollments',            N'Manage course enrollments and rosters',       N'enrollments',        21),
    (CAST(N'78409D30-E31D-42D0-AE44-A6CAFA45C02A' AS UNIQUEIDENTIFIER), N'Report Center',          N'Generate and export academic reports',        N'report_center',      22),
    (CAST(N'7DB67169-FE1B-40C7-81B4-D1CF57D77B61' AS UNIQUEIDENTIFIER), N'Module Settings',        N'Enable/disable modules',                      N'module_settings',    23),
    (CAST(N'C2AD83E2-601F-466D-81B8-C0D1C8D5A67F' AS UNIQUEIDENTIFIER), N'Sidebar Settings',       N'Customise sidebar per role',                  N'sidebar_settings',   24),
    (CAST(N'6577D2CC-9FF2-4A78-8770-FC419ED28C10' AS UNIQUEIDENTIFIER), N'Theme Settings',         N'UI colour theme',                             N'theme_settings',     25),
    (CAST(N'3E97C851-A163-4B2D-90BA-28548E68E00E' AS UNIQUEIDENTIFIER), N'License Update',         N'Upload license file',                         N'license_update',     26),
    (CAST(N'55F6615E-D38F-4825-BA54-2603B3D7E539' AS UNIQUEIDENTIFIER), N'Dashboard Settings',     N'Customise portal branding and name',          N'dashboard_settings', 27),
    (CAST(N'C16DBE1D-7A79-4BC8-B6DA-1A4B72DE6987' AS UNIQUEIDENTIFIER), N'Report Settings',        N'Report access control',                       N'report_settings',    28),
    (CAST(N'A55B6BEF-A288-4ABB-9ED6-65C319B67C1B' AS UNIQUEIDENTIFIER), N'Buildings',              N'Manage buildings',                            N'buildings',          29),
    (CAST(N'1C2312DD-1C86-4FD0-A9F1-54026BE5A0FB' AS UNIQUEIDENTIFIER), N'Rooms',                  N'Manage rooms',                                N'rooms',              30)
) AS v([Id],[Name],[Purpose],[Key],[Ord])
WHERE NOT EXISTS (SELECT 1 FROM [sidebar_menu_items] WHERE [Id] = v.[Id]);

-- Sidebar role accesses
;WITH SidebarAccess AS (
    SELECT v.[MenuId], v.[RoleName]
    FROM (VALUES
        -- SuperAdmin gets all menu items
        (CAST(N'30A1025F-BAC8-4847-AE59-F0D1BAD8F506' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'AFFC4796-6040-4FB4-BE44-BF49410F6F15' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'ACD69922-A686-4E1E-B8DE-2DDB76810E97' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'946AE4D5-9894-4D34-B0D9-F0ACDEF9BCB6' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'72BFB9B9-5DF7-4CCB-BD52-EA3A6B626BF4' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'35CEAEB0-A6DF-43A8-A41D-D6225696B924' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'662BBCF7-B1CF-4AF6-829C-C6314534620B' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'CEFE1A96-905C-405B-8ED6-D4902C531A43' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'A062C9DF-45A2-4C4C-AD97-CC763C8A200C' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'3DA9193D-BA9C-4E25-916E-D0C9A12DAF65' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'B521C9BD-F1F3-4C18-B976-A55F29DF82FA' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'36DC3134-B3EC-484C-BF81-5E4EAD7AE670' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'C29CDA68-D18B-4885-9F7F-160F4C7FD9EA' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'42AAE8E1-F4DA-49CB-A185-6960833B5514' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'49CB94D3-A88F-43C1-8D40-BDDC5415675E' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'65C242B5-7BE1-40AD-9AD4-50E86092055D' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'70C7D869-8ECA-41CF-AC13-91E2C0375E89' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'8B3239A2-6FA5-4CF4-96FC-F25163551180' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'BF4DDC33-FBC7-4256-9DA4-0D3879CE3CE2' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'13963044-779D-4E60-868D-FBFD4614CD7B' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'18D1697A-1093-4C7D-BE5C-2FB5AE9853D0' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'78409D30-E31D-42D0-AE44-A6CAFA45C02A' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'7DB67169-FE1B-40C7-81B4-D1CF57D77B61' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'C2AD83E2-601F-466D-81B8-C0D1C8D5A67F' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'6577D2CC-9FF2-4A78-8770-FC419ED28C10' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'3E97C851-A163-4B2D-90BA-28548E68E00E' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'55F6615E-D38F-4825-BA54-2603B3D7E539' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'C16DBE1D-7A79-4BC8-B6DA-1A4B72DE6987' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'A55B6BEF-A288-4ABB-9ED6-65C319B67C1B' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        (CAST(N'1C2312DD-1C86-4FD0-A9F1-54026BE5A0FB' AS UNIQUEIDENTIFIER), N'SuperAdmin'),
        -- Admin
        (CAST(N'30A1025F-BAC8-4847-AE59-F0D1BAD8F506' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'AFFC4796-6040-4FB4-BE44-BF49410F6F15' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'72BFB9B9-5DF7-4CCB-BD52-EA3A6B626BF4' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'CEFE1A96-905C-405B-8ED6-D4902C531A43' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'A062C9DF-45A2-4C4C-AD97-CC763C8A200C' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'3DA9193D-BA9C-4E25-916E-D0C9A12DAF65' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'B521C9BD-F1F3-4C18-B976-A55F29DF82FA' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'36DC3134-B3EC-484C-BF81-5E4EAD7AE670' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'C29CDA68-D18B-4885-9F7F-160F4C7FD9EA' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'42AAE8E1-F4DA-49CB-A185-6960833B5514' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'49CB94D3-A88F-43C1-8D40-BDDC5415675E' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'65C242B5-7BE1-40AD-9AD4-50E86092055D' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'70C7D869-8ECA-41CF-AC13-91E2C0375E89' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'BF4DDC33-FBC7-4256-9DA4-0D3879CE3CE2' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'13963044-779D-4E60-868D-FBFD4614CD7B' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'18D1697A-1093-4C7D-BE5C-2FB5AE9853D0' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'78409D30-E31D-42D0-AE44-A6CAFA45C02A' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'A55B6BEF-A288-4ABB-9ED6-65C319B67C1B' AS UNIQUEIDENTIFIER), N'Admin'),
        (CAST(N'1C2312DD-1C86-4FD0-A9F1-54026BE5A0FB' AS UNIQUEIDENTIFIER), N'Admin'),
        -- Faculty
        (CAST(N'30A1025F-BAC8-4847-AE59-F0D1BAD8F506' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'ACD69922-A686-4E1E-B8DE-2DDB76810E97' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'CEFE1A96-905C-405B-8ED6-D4902C531A43' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'B521C9BD-F1F3-4C18-B976-A55F29DF82FA' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'36DC3134-B3EC-484C-BF81-5E4EAD7AE670' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'C29CDA68-D18B-4885-9F7F-160F4C7FD9EA' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'42AAE8E1-F4DA-49CB-A185-6960833B5514' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'49CB94D3-A88F-43C1-8D40-BDDC5415675E' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'65C242B5-7BE1-40AD-9AD4-50E86092055D' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'8B3239A2-6FA5-4CF4-96FC-F25163551180' AS UNIQUEIDENTIFIER), N'Faculty'),
        (CAST(N'78409D30-E31D-42D0-AE44-A6CAFA45C02A' AS UNIQUEIDENTIFIER), N'Faculty'),
        -- Student
        (CAST(N'30A1025F-BAC8-4847-AE59-F0D1BAD8F506' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'946AE4D5-9894-4D34-B0D9-F0ACDEF9BCB6' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'CEFE1A96-905C-405B-8ED6-D4902C531A43' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'36DC3134-B3EC-484C-BF81-5E4EAD7AE670' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'C29CDA68-D18B-4885-9F7F-160F4C7FD9EA' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'42AAE8E1-F4DA-49CB-A185-6960833B5514' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'49CB94D3-A88F-43C1-8D40-BDDC5415675E' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'65C242B5-7BE1-40AD-9AD4-50E86092055D' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'8B3239A2-6FA5-4CF4-96FC-F25163551180' AS UNIQUEIDENTIFIER), N'Student'),
        (CAST(N'13963044-779D-4E60-868D-FBFD4614CD7B' AS UNIQUEIDENTIFIER), N'Student')
    ) AS v([MenuId],[RoleName])
)
INSERT INTO [sidebar_menu_role_accesses] ([Id],[SidebarMenuItemId],[RoleName],[IsAllowed],[CreatedAt],[UpdatedAt])
SELECT NEWID(), sa.MenuId, sa.RoleName, 1, @Now, NULL
FROM SidebarAccess sa
WHERE NOT EXISTS (
    SELECT 1 FROM [sidebar_menu_role_accesses] ex
    WHERE ex.[SidebarMenuItemId] = sa.MenuId AND ex.[RoleName] = sa.RoleName
);

-- ─────────────────────────────────────────────────────────────
-- 10. DEPARTMENT
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [departments] WHERE [Id] = CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER))
BEGIN
    INSERT INTO [departments] ([Id],[Name],[Code],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
    VALUES (CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
            N'Information Technology', N'IT', 1, @Now, @Now, 0);
END;

-- ─────────────────────────────────────────────────────────────
-- 11. ACADEMIC PROGRAM
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [academic_programs] WHERE [Id] = CAST(N'P0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER))
BEGIN
    INSERT INTO [academic_programs] ([Id],[Name],[Code],[DepartmentId],[TotalSemesters],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
    VALUES (CAST(N'P0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
            N'BS Information Technology', N'BSIT',
            CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
            8, 1, @Now, @Now, 0);
END;

-- ─────────────────────────────────────────────────────────────
-- 12. SEMESTERS
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [semesters] WHERE [Id] = CAST(N'S0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER))
    INSERT INTO [semesters] ([Id],[Name],[StartDate],[EndDate],[IsClosed],[ClosedAt],[CreatedAt],[UpdatedAt],[IsDeleted])
    VALUES (CAST(N'S0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
            N'Fall 2025', '2025-09-01', '2026-01-31', 1, '2026-02-01 00:00:00', @Now, @Now, 0);

IF NOT EXISTS (SELECT 1 FROM [semesters] WHERE [Id] = CAST(N'S0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER))
    INSERT INTO [semesters] ([Id],[Name],[StartDate],[EndDate],[IsClosed],[ClosedAt],[CreatedAt],[UpdatedAt],[IsDeleted])
    VALUES (CAST(N'S0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
            N'Spring 2026', '2026-02-01', '2026-06-30', 0, NULL, @Now, @Now, 0);

-- ─────────────────────────────────────────────────────────────
-- 13. USERS (4 minimal test accounts)
-- ─────────────────────────────────────────────────────────────
INSERT INTO [users]
    ([Id],[Username],[Email],[PasswordHash],[RoleId],[DepartmentId],
     [IsActive],[MustChangePassword],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Username], v.[Email], @PwdHash, v.[RoleId], v.[DeptId], 1, 0, @Now, @Now, 0
FROM (VALUES
    (CAST(N'U0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
     N'superadmin', N'superadmin@edusphere.local',  1,
     CAST(NULL AS UNIQUEIDENTIFIER)),
    (CAST(N'U0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
     N'admin.it',   N'admin.it@edusphere.local',    2,
     CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER)),
    (CAST(N'U0000000-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER),
     N'dr.ahmed',   N'ahmed@edusphere.local',       3,
     CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER)),
    (CAST(N'U0000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER),
     N's.aslam',    N'aslam@student.edusphere.local', 4,
     CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER))
) AS v([Id],[Username],[Email],[RoleId],[DeptId])
WHERE NOT EXISTS (SELECT 1 FROM [users] WHERE [Id] = v.[Id]);

-- ─────────────────────────────────────────────────────────────
-- 14. ADMIN DEPARTMENT ASSIGNMENT
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM [admin_department_assignments]
    WHERE [AdminUserId]  = CAST(N'U0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER)
      AND [DepartmentId] = CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER)
      AND [RemovedAt] IS NULL
)
INSERT INTO [admin_department_assignments]
    ([Id],[AdminUserId],[DepartmentId],[AssignedAt],[RemovedAt],[CreatedAt],[UpdatedAt])
VALUES (NEWID(),
    CAST(N'U0000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER),
    CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
    @Now, NULL, @Now, @Now);

-- ─────────────────────────────────────────────────────────────
-- 15. FACULTY DEPARTMENT ASSIGNMENT
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM [faculty_department_assignments]
    WHERE [FacultyUserId] = CAST(N'U0000000-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER)
      AND [DepartmentId]  = CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER)
      AND [RemovedAt] IS NULL
)
INSERT INTO [faculty_department_assignments]
    ([Id],[FacultyUserId],[DepartmentId],[AssignedAt],[RemovedAt],[CreatedAt],[UpdatedAt])
VALUES (NEWID(),
    CAST(N'U0000000-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER),
    CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
    @Now, NULL, @Now, @Now);

-- ─────────────────────────────────────────────────────────────
-- 16. REGISTRATION WHITELIST (student s.aslam)
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [registration_whitelist] WHERE [IdentifierValue] = N'aslam@student.edusphere.local')
    INSERT INTO [registration_whitelist]
        ([Id],[IdentifierType],[IdentifierValue],[DepartmentId],[ProgramId],
         [IsUsed],[UsedAt],[CreatedUserId],[CreatedAt],[UpdatedAt])
    VALUES (NEWID(), N'Email', N'aslam@student.edusphere.local',
        CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
        CAST(N'P0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
        1, @Now,
        CAST(N'U0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
        @Now, @Now);

-- ─────────────────────────────────────────────────────────────
-- 17. STUDENT PROFILE
-- ─────────────────────────────────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM [student_profiles] WHERE [Id] = CAST(N'SP000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER))
    INSERT INTO [student_profiles]
        ([Id],[UserId],[RegistrationNumber],[ProgramId],[DepartmentId],
         [AdmissionDate],[Cgpa],[CurrentSemesterNumber],[CreatedAt],[UpdatedAt],[IsDeleted])
    VALUES (
        CAST(N'SP000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER),
        CAST(N'U0000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER),
        N'2025-IT-001',
        CAST(N'P0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
        CAST(N'D0000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER),
        '2025-09-01', 0.00, 1, @Now, @Now, 0);

PRINT '✓ Minimal seed complete.';
PRINT '  Accounts created (all share the same hashed password):';
PRINT '    SuperAdmin : superadmin       / superadmin@edusphere.local';
PRINT '    Admin      : admin.it         / admin.it@edusphere.local';
PRINT '    Faculty    : dr.ahmed         / ahmed@edusphere.local';
PRINT '    Student    : s.aslam          / aslam@student.edusphere.local';
PRINT '';
PRINT '  Run .\Scripts\GenerateTestHashes.ps1 to set the password hash.';
GO
