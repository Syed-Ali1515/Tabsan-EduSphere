-- ============================================================
-- Tabsan EduSphere — Script 1: Minimal Seed Data
-- ============================================================
-- PURPOSE : Minimum test data to exercise EVERY role, module,
--           view, and stored procedure as SuperAdmin.
--
-- PREREQUISITES
--   1. Apply all EF Core migrations:
--        dotnet ef database update ^
--          --project src/Tabsan.EduSphere.Infrastructure ^
--          --startup-project src/Tabsan.EduSphere.API ^
--          --context ApplicationDbContext
--
--   2. Set env vars and run the API once so DatabaseSeeder runs:
--        set TABSAN_SUPER_USERNAME=superadmin
--        set TABSAN_SUPER_PASSWORD=SuperAdmin@1234!
--        set TABSAN_SUPER_EMAIL=superadmin@tabsan.local
--        dotnet run --project src/Tabsan.EduSphere.API
--      (Press Ctrl+C once you see "Application started".)
--      This seeds: roles (id 1-4), modules, SuperAdmin user.
--
-- PASSWORD NOTE
--   All test accounts below use placeholder hashes.
--   AFTER running this script, open the admin panel as SuperAdmin
--   and use "Reset Password" for each account, OR run:
--        Scripts\GenerateTestHashes.ps1
--   to get real Argon2id hashes and paste them in @PwdHash below.
--
-- TEST ACCOUNTS  (after password reset)
--   superadmin    / SuperAdmin@1234!  (role: SuperAdmin)
--   admin.test    / TestPass@1234!    (role: Admin)
--   faculty.test  / TestPass@1234!    (role: Faculty)
--   student.test  / TestPass@1234!    (role: Student)
--
-- DATABASE    : TabsanEduSphere   (auto-created if missing)
-- ============================================================

IF DB_ID(N'TabsanEduSphere') IS NULL
BEGIN
    PRINT 'Database TabsanEduSphere not found. Creating it now...';
    CREATE DATABASE [TabsanEduSphere];
END
GO

USE [TabsanEduSphere];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF OBJECT_ID(N'dbo.roles', N'U') IS NULL
BEGIN
    PRINT 'Schema missing in TabsanEduSphere (table dbo.roles not found).';
    PRINT 'Database has been created successfully.';
    PRINT 'Next step: run EF migrations, then run this script again:';
    PRINT 'dotnet ef database update --project src/Tabsan.EduSphere.Infrastructure --startup-project src/Tabsan.EduSphere.API --context ApplicationDbContext';
    RETURN;
END

SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Now  DATETIME2 = GETUTCDATE();

-- ── Password placeholder ─────────────────────────────────────
-- Replace with output of Scripts\GenerateTestHashes.ps1
DECLARE @PwdHash NVARCHAR(500) =
    N'PLACEHOLDER_RUN_GenerateTestHashes.ps1_TO_REPLACE';
-- ─────────────────────────────────────────────────────────────

-- ═══════════════════════════════════════════════════════════
-- §1  ROLES  (idempotent — DatabaseSeeder inserts these, but
--             we add them here in case the script runs first)
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM roles WHERE Name = N'SuperAdmin')
    INSERT INTO roles (Name, Description, IsSystemRole)
    VALUES (N'SuperAdmin', N'Full platform access — manages license and all settings.', 1);

IF NOT EXISTS (SELECT 1 FROM roles WHERE Name = N'Admin')
    INSERT INTO roles (Name, Description, IsSystemRole)
    VALUES (N'Admin', N'Department-level admin — manages users and courses.', 1);

IF NOT EXISTS (SELECT 1 FROM roles WHERE Name = N'Faculty')
    INSERT INTO roles (Name, Description, IsSystemRole)
    VALUES (N'Faculty', N'Teaches courses and manages academic content.', 1);

IF NOT EXISTS (SELECT 1 FROM roles WHERE Name = N'Student')
    INSERT INTO roles (Name, Description, IsSystemRole)
    VALUES (N'Student', N'Enrolled student — accesses course and academic content.', 1);

DECLARE @RoleSuperAdmin INT = (SELECT TOP 1 Id FROM roles WHERE Name = N'SuperAdmin');
DECLARE @RoleAdmin      INT = (SELECT TOP 1 Id FROM roles WHERE Name = N'Admin');
DECLARE @RoleFaculty    INT = (SELECT TOP 1 Id FROM roles WHERE Name = N'Faculty');
DECLARE @RoleStudent    INT = (SELECT TOP 1 Id FROM roles WHERE Name = N'Student');

-- ═══════════════════════════════════════════════════════════
-- §2  MODULES  (idempotent)
-- ═══════════════════════════════════════════════════════════
DECLARE @ModDefs TABLE ([Key] NVARCHAR(50), Name NVARCHAR(100), IsMandatory BIT);
INSERT INTO @ModDefs VALUES
    (N'authentication',  N'Authentication',           1),
    (N'departments',     N'Departments',              1),
    (N'sis',             N'Student Information',      1),
    (N'courses',         N'Courses',                  0),
    (N'assignments',     N'Assignments',              0),
    (N'quizzes',         N'Quizzes',                  0),
    (N'attendance',      N'Attendance',               0),
    (N'results',         N'Results / Grades',         0),
    (N'notifications',   N'Notifications',            0),
    (N'fyp',             N'Final Year Projects',      0),
    (N'ai_chat',         N'AI Chatbot',               0),
    (N'reports',         N'Reports',                  0),
    (N'themes',          N'UI Themes',                0),
    (N'advanced_audit',  N'Advanced Audit Logging',   0);

INSERT INTO modules (Id, [Key], Name, IsMandatory, CreatedAt)
SELECT NEWID(), d.[Key], d.Name, d.IsMandatory, @Now
FROM @ModDefs d
WHERE NOT EXISTS (SELECT 1 FROM modules m WHERE m.[Key] = d.[Key]);

-- Module status entries (activate ALL modules for easy testing)
INSERT INTO module_status (Id, ModuleId, IsActive, ActivatedAt, Source, CreatedAt)
SELECT NEWID(), m.Id, 1, @Now, N'seed', @Now
FROM modules m
WHERE NOT EXISTS (SELECT 1 FROM module_status ms WHERE ms.ModuleId = m.Id);

-- Activate any modules seeded as inactive
UPDATE ms SET IsActive = 1, ActivatedAt = @Now, Source = N'seed'
FROM module_status ms
WHERE ms.IsActive = 0;

-- ═══════════════════════════════════════════════════════════
-- §3  LICENSE STATE  (required for the system to allow access)
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM license_state WHERE Status = N'Active')
    INSERT INTO license_state (Id, LicenseHash, LicenseType, Status, ActivatedAt, ExpiresAt, CreatedAt)
    VALUES (NEWID(), N'TEST-PERMANENT-LICENSE-DO-NOT-DEPLOY-TO-PROD',
            N'Permanent', N'Active', @Now, NULL, @Now);

-- ═══════════════════════════════════════════════════════════
-- §4  DEPARTMENT
-- ═══════════════════════════════════════════════════════════
DECLARE @DeptCS UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM departments WHERE Code = N'CS')
BEGIN
    INSERT INTO departments (Id, Name, Code, IsActive, CreatedAt, IsDeleted)
    VALUES (@DeptCS, N'Computer Science', N'CS', 1, @Now, 0);
END
ELSE
    SELECT @DeptCS = Id FROM departments WHERE Code = N'CS';

-- ═══════════════════════════════════════════════════════════
-- §5  BUILDING + ROOM
-- ═══════════════════════════════════════════════════════════
DECLARE @Bldg1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Room1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM buildings WHERE Code = N'BLK-A')
BEGIN
    INSERT INTO buildings (Id, Name, Code, IsActive, CreatedAt, IsDeleted)
    VALUES (@Bldg1, N'Block A', N'BLK-A', 1, @Now, 0);

    INSERT INTO rooms (Id, BuildingId, Number, Capacity, IsActive, CreatedAt, IsDeleted)
    VALUES (@Room1, @Bldg1, N'A-101', 60, 1, @Now, 0);
END
ELSE
BEGIN
    SELECT @Bldg1 = Id FROM buildings WHERE Code = N'BLK-A';
    SELECT TOP 1 @Room1 = Id FROM rooms WHERE BuildingId = @Bldg1;
END

-- ═══════════════════════════════════════════════════════════
-- §6  ACADEMIC PROGRAM
-- ═══════════════════════════════════════════════════════════
DECLARE @ProgBSCS UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM academic_programs WHERE Code = N'BSCS')
BEGIN
    INSERT INTO academic_programs (Id, Name, Code, DepartmentId, TotalSemesters, IsActive, CreatedAt, IsDeleted)
    VALUES (@ProgBSCS, N'BS Computer Science', N'BSCS', @DeptCS, 8, 1, @Now, 0);
END
ELSE
    SELECT @ProgBSCS = Id FROM academic_programs WHERE Code = N'BSCS';

-- ═══════════════════════════════════════════════════════════
-- §7  SEMESTER
-- ═══════════════════════════════════════════════════════════
DECLARE @Sem1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM semesters WHERE Name = N'Spring 2026')
BEGIN
    INSERT INTO semesters (Id, Name, StartDate, EndDate, IsClosed, CreatedAt, IsDeleted)
    VALUES (@Sem1, N'Spring 2026', '2026-01-15', '2026-05-31', 0, @Now, 0);
END
ELSE
    SELECT @Sem1 = Id FROM semesters WHERE Name = N'Spring 2026';

-- ═══════════════════════════════════════════════════════════
-- §8  COURSES
-- ═══════════════════════════════════════════════════════════
DECLARE @CourseOOP UNIQUEIDENTIFIER = NEWID();
DECLARE @CourseDB  UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code = N'CS-301' AND DepartmentId = @DeptCS)
BEGIN
    INSERT INTO courses (Id, Title, Code, CreditHours, DepartmentId, IsActive, CreatedAt, IsDeleted)
    VALUES (@CourseOOP, N'Object-Oriented Programming', N'CS-301', 3, @DeptCS, 1, @Now, 0);
END
ELSE
    SELECT @CourseOOP = Id FROM courses WHERE Code = N'CS-301' AND DepartmentId = @DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code = N'CS-302' AND DepartmentId = @DeptCS)
BEGIN
    INSERT INTO courses (Id, Title, Code, CreditHours, DepartmentId, IsActive, CreatedAt, IsDeleted)
    VALUES (@CourseDB, N'Database Systems', N'CS-302', 3, @DeptCS, 1, @Now, 0);
END
ELSE
    SELECT @CourseDB = Id FROM courses WHERE Code = N'CS-302' AND DepartmentId = @DeptCS;

-- ═══════════════════════════════════════════════════════════
-- §9  USERS
-- ═══════════════════════════════════════════════════════════
DECLARE @UserAdmin   UNIQUEIDENTIFIER = NEWID();
DECLARE @UserFaculty UNIQUEIDENTIFIER = NEWID();
DECLARE @UserStudent UNIQUEIDENTIFIER = NEWID();

-- SuperAdmin (may already exist from DatabaseSeeder)
DECLARE @UserSuperAdmin UNIQUEIDENTIFIER;
SELECT @UserSuperAdmin = Id FROM users WHERE Username = N'superadmin';

IF @UserSuperAdmin IS NULL
BEGIN
    SET @UserSuperAdmin = NEWID();
    INSERT INTO users (Id, Username, Email, PasswordHash, RoleId, DepartmentId,
                       IsActive, IsLockedOut, FailedLoginAttempts,
                       CreatedAt, IsDeleted)
    VALUES (@UserSuperAdmin, N'superadmin', N'superadmin@tabsan.local',
            @PwdHash, @RoleSuperAdmin, NULL,
            1, 0, 0, @Now, 0);
END

-- Admin user
IF NOT EXISTS (SELECT 1 FROM users WHERE Username = N'admin.test')
    INSERT INTO users (Id, Username, Email, PasswordHash, RoleId, DepartmentId,
                       IsActive, IsLockedOut, FailedLoginAttempts,
                       CreatedAt, IsDeleted)
    VALUES (@UserAdmin, N'admin.test', N'admin@tabsan.local',
            @PwdHash, @RoleAdmin, @DeptCS,
            1, 0, 0, @Now, 0);
ELSE
    SELECT @UserAdmin = Id FROM users WHERE Username = N'admin.test';

-- Faculty user
IF NOT EXISTS (SELECT 1 FROM users WHERE Username = N'faculty.test')
    INSERT INTO users (Id, Username, Email, PasswordHash, RoleId, DepartmentId,
                       IsActive, IsLockedOut, FailedLoginAttempts,
                       CreatedAt, IsDeleted)
    VALUES (@UserFaculty, N'faculty.test', N'faculty@tabsan.local',
            @PwdHash, @RoleFaculty, @DeptCS,
            1, 0, 0, @Now, 0);
ELSE
    SELECT @UserFaculty = Id FROM users WHERE Username = N'faculty.test';

-- Student user
IF NOT EXISTS (SELECT 1 FROM users WHERE Username = N'student.test')
    INSERT INTO users (Id, Username, Email, PasswordHash, RoleId, DepartmentId,
                       IsActive, IsLockedOut, FailedLoginAttempts,
                       CreatedAt, IsDeleted)
    VALUES (@UserStudent, N'student.test', N'student@tabsan.local',
            @PwdHash, @RoleStudent, @DeptCS,
            1, 0, 0, @Now, 0);
ELSE
    SELECT @UserStudent = Id FROM users WHERE Username = N'student.test';

-- ═══════════════════════════════════════════════════════════
-- §10  STUDENT PROFILE
-- ═══════════════════════════════════════════════════════════
DECLARE @SpStudent UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId = @UserStudent)
    INSERT INTO student_profiles
        (Id, UserId, RegistrationNumber, ProgramId, DepartmentId,
         AdmissionDate, Cgpa, CurrentSemesterNumber, Status,
         CreatedAt, IsDeleted)
    VALUES
        (@SpStudent, @UserStudent, N'2024-CS-0001', @ProgBSCS, @DeptCS,
         '2024-09-01', 0.00, 1, 1,   -- Status 1 = Active
         @Now, 0);
ELSE
    SELECT @SpStudent = Id FROM student_profiles WHERE UserId = @UserStudent;

-- ═══════════════════════════════════════════════════════════
-- §11  FACULTY DEPARTMENT ASSIGNMENT
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM faculty_department_assignments
               WHERE FacultyUserId = @UserFaculty AND DepartmentId = @DeptCS)
    INSERT INTO faculty_department_assignments (Id, FacultyUserId, DepartmentId, AssignedAt, CreatedAt)
    VALUES (NEWID(), @UserFaculty, @DeptCS, @Now, @Now);

-- ═══════════════════════════════════════════════════════════
-- §12  COURSE OFFERINGS
-- ═══════════════════════════════════════════════════════════
DECLARE @OfferOOP UNIQUEIDENTIFIER = NEWID();
DECLARE @OfferDB  UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM course_offerings
               WHERE CourseId = @CourseOOP AND SemesterId = @Sem1)
BEGIN
    INSERT INTO course_offerings
        (Id, CourseId, SemesterId, FacultyUserId, MaxEnrollment, IsOpen, CreatedAt, IsDeleted)
    VALUES
        (@OfferOOP, @CourseOOP, @Sem1, @UserFaculty, 40, 1, @Now, 0);
END
ELSE
    SELECT @OfferOOP = Id FROM course_offerings
    WHERE CourseId = @CourseOOP AND SemesterId = @Sem1;

IF NOT EXISTS (SELECT 1 FROM course_offerings
               WHERE CourseId = @CourseDB AND SemesterId = @Sem1)
BEGIN
    INSERT INTO course_offerings
        (Id, CourseId, SemesterId, FacultyUserId, MaxEnrollment, IsOpen, CreatedAt, IsDeleted)
    VALUES
        (@OfferDB, @CourseDB, @Sem1, @UserFaculty, 40, 1, @Now, 0);
END
ELSE
    SELECT @OfferDB = Id FROM course_offerings
    WHERE CourseId = @CourseDB AND SemesterId = @Sem1;

-- ═══════════════════════════════════════════════════════════
-- §13  ENROLLMENT
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM enrollments
               WHERE StudentProfileId = @SpStudent AND CourseOfferingId = @OfferOOP)
    INSERT INTO enrollments (Id, StudentProfileId, CourseOfferingId, EnrolledAt, Status, CreatedAt)
    VALUES (NEWID(), @SpStudent, @OfferOOP, @Now, N'Active', @Now);

IF NOT EXISTS (SELECT 1 FROM enrollments
               WHERE StudentProfileId = @SpStudent AND CourseOfferingId = @OfferDB)
    INSERT INTO enrollments (Id, StudentProfileId, CourseOfferingId, EnrolledAt, Status, CreatedAt)
    VALUES (NEWID(), @SpStudent, @OfferDB, @Now, N'Active', @Now);

-- ═══════════════════════════════════════════════════════════
-- §14  TIMETABLE + ENTRIES
-- ═══════════════════════════════════════════════════════════
DECLARE @TT1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM timetables WHERE DepartmentId = @DeptCS AND SemesterId = @Sem1)
BEGIN
    INSERT INTO timetables
        (Id, DepartmentId, SemesterId, IsPublished, PublishedAt, CreatedAt, IsDeleted,
         AcademicProgramId, EffectiveDate, SemesterNumber)
    VALUES
        (@TT1, @DeptCS, @Sem1, 1, @Now, @Now, 0,
         @ProgBSCS, CAST('2026-01-15' AS DATE), 1);

    -- Monday OOP  08:00-09:30
    INSERT INTO timetable_entries
        (Id, TimetableId, DayOfWeek, StartTime, EndTime,
         SubjectName, RoomNumber, FacultyName, CreatedAt,
         RoomId, BuildingId, CourseId, FacultyUserId)
    VALUES
        (NEWID(), @TT1, 1, '08:00:00', '09:30:00',
         N'Object-Oriented Programming', N'A-101', N'faculty.test', @Now,
         @Room1, @Bldg1, @CourseOOP, @UserFaculty);

    -- Wednesday DB  10:00-11:30
    INSERT INTO timetable_entries
        (Id, TimetableId, DayOfWeek, StartTime, EndTime,
         SubjectName, RoomNumber, FacultyName, CreatedAt,
         RoomId, BuildingId, CourseId, FacultyUserId)
    VALUES
        (NEWID(), @TT1, 3, '10:00:00', '11:30:00',
         N'Database Systems', N'A-101', N'faculty.test', @Now,
         @Room1, @Bldg1, @CourseDB, @UserFaculty);
END

-- ═══════════════════════════════════════════════════════════
-- §15  SIDEBAR MENU ITEMS + ROLE ACCESSES  (idempotent)
-- ═══════════════════════════════════════════════════════════
DECLARE @SMDashboard    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMTTAdmin      UNIQUEIDENTIFIER = NEWID();
DECLARE @SMTTTeacher    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMTTStudent    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMLookups      UNIQUEIDENTIFIER = NEWID();
DECLARE @SMBuildings    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMRooms        UNIQUEIDENTIFIER = NEWID();
DECLARE @SMSettings     UNIQUEIDENTIFIER = NEWID();
DECLARE @SMReportSet    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMModuleSet    UNIQUEIDENTIFIER = NEWID();
DECLARE @SMSidebarSet   UNIQUEIDENTIFIER = NEWID();
DECLARE @SMThemeSet     UNIQUEIDENTIFIER = NEWID();
DECLARE @SMLicUpd       UNIQUEIDENTIFIER = NEWID();

-- Helper to upsert a menu item and return its ID
-- Using IF NOT EXISTS pattern for each item

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'dashboard')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMDashboard,N'dashboard',N'Dashboard',
            N'Main overview dashboard for all roles',NULL,1,1,1,@Now,0);
ELSE SELECT @SMDashboard = Id FROM sidebar_menu_items WHERE [Key] = N'dashboard';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'timetable_admin')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTAdmin,N'timetable_admin',N'Timetable Admin',
            N'Create and manage department timetables',NULL,2,1,1,@Now,0);
ELSE SELECT @SMTTAdmin = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_admin';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'timetable_teacher')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTTeacher,N'timetable_teacher',N'My Timetable (Faculty)',
            N'View teaching schedule for the faculty member',NULL,3,1,1,@Now,0);
ELSE SELECT @SMTTTeacher = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_teacher';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'timetable_student')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTStudent,N'timetable_student',N'My Timetable (Student)',
            N'View class schedule for the student',NULL,4,1,1,@Now,0);
ELSE SELECT @SMTTStudent = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_student';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'lookups')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMLookups,N'lookups',N'Lookups',
            N'Manage reference data: buildings, rooms',NULL,5,1,1,@Now,0);
ELSE SELECT @SMLookups = Id FROM sidebar_menu_items WHERE [Key] = N'lookups';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'buildings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMBuildings,N'buildings',N'Buildings',
            N'Manage physical buildings on campus',@SMLookups,1,1,1,@Now,0);
ELSE SELECT @SMBuildings = Id FROM sidebar_menu_items WHERE [Key] = N'buildings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'rooms')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMRooms,N'rooms',N'Rooms',
            N'Manage classrooms and labs within buildings',@SMLookups,2,1,1,@Now,0);
ELSE SELECT @SMRooms = Id FROM sidebar_menu_items WHERE [Key] = N'rooms';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'system_settings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMSettings,N'system_settings',N'System Settings',
            N'Platform-level configuration visible to SuperAdmin only',NULL,6,1,1,@Now,0);
ELSE SELECT @SMSettings = Id FROM sidebar_menu_items WHERE [Key] = N'system_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'report_settings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMReportSet,N'report_settings',N'Report Settings',
            N'Control which roles can access which reports',@SMSettings,1,1,1,@Now,0);
ELSE SELECT @SMReportSet = Id FROM sidebar_menu_items WHERE [Key] = N'report_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'module_settings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMModuleSet,N'module_settings',N'Module Settings',
            N'Enable or disable optional feature modules',@SMSettings,2,1,1,@Now,0);
ELSE SELECT @SMModuleSet = Id FROM sidebar_menu_items WHERE [Key] = N'module_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'sidebar_settings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMSidebarSet,N'sidebar_settings',N'Sidebar Settings',
            N'Customise sidebar menu visibility per role',@SMSettings,3,1,1,@Now,0);
ELSE SELECT @SMSidebarSet = Id FROM sidebar_menu_items WHERE [Key] = N'sidebar_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'theme_settings')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMThemeSet,N'theme_settings',N'Theme Settings',
            N'Switch UI colour theme',@SMSettings,4,1,1,@Now,0);
ELSE SELECT @SMThemeSet = Id FROM sidebar_menu_items WHERE [Key] = N'theme_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key] = N'license_update')
    INSERT INTO sidebar_menu_items
        (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMLicUpd,N'license_update',N'License Update',
            N'Upload and activate a new license file',@SMSettings,5,1,1,@Now,0);
ELSE SELECT @SMLicUpd = Id FROM sidebar_menu_items WHERE [Key] = N'license_update';

-- Re-read IDs (needed after the ELSE branches above)
SELECT @SMDashboard  = Id FROM sidebar_menu_items WHERE [Key] = N'dashboard';
SELECT @SMTTAdmin    = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_admin';
SELECT @SMTTTeacher  = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_teacher';
SELECT @SMTTStudent  = Id FROM sidebar_menu_items WHERE [Key] = N'timetable_student';
SELECT @SMLookups    = Id FROM sidebar_menu_items WHERE [Key] = N'lookups';
SELECT @SMBuildings  = Id FROM sidebar_menu_items WHERE [Key] = N'buildings';
SELECT @SMRooms      = Id FROM sidebar_menu_items WHERE [Key] = N'rooms';
SELECT @SMSettings   = Id FROM sidebar_menu_items WHERE [Key] = N'system_settings';
SELECT @SMReportSet  = Id FROM sidebar_menu_items WHERE [Key] = N'report_settings';
SELECT @SMModuleSet  = Id FROM sidebar_menu_items WHERE [Key] = N'module_settings';
SELECT @SMSidebarSet = Id FROM sidebar_menu_items WHERE [Key] = N'sidebar_settings';
SELECT @SMThemeSet   = Id FROM sidebar_menu_items WHERE [Key] = N'theme_settings';
SELECT @SMLicUpd     = Id FROM sidebar_menu_items WHERE [Key] = N'license_update';

-- Sidebar role accesses (use MERGE to avoid unique-index violations)
DECLARE @SidebarRoles TABLE (ItemId UNIQUEIDENTIFIER, RoleName NVARCHAR(100), IsAllowed BIT);
INSERT INTO @SidebarRoles VALUES
    -- Dashboard: all roles
    (@SMDashboard,  N'SuperAdmin', 1), (@SMDashboard,  N'Admin',    1),
    (@SMDashboard,  N'Faculty',    1), (@SMDashboard,  N'Student',  1),
    -- Timetable Admin: SuperAdmin + Admin
    (@SMTTAdmin,    N'SuperAdmin', 1), (@SMTTAdmin,    N'Admin',    1),
    -- Teacher Timetable: Faculty
    (@SMTTTeacher,  N'Faculty',    1),
    -- Student Timetable: Student
    (@SMTTStudent,  N'Student',    1),
    -- Lookups: SuperAdmin + Admin
    (@SMLookups,    N'SuperAdmin', 1), (@SMLookups,    N'Admin',    1),
    (@SMBuildings,  N'SuperAdmin', 1), (@SMBuildings,  N'Admin',    1),
    (@SMRooms,      N'SuperAdmin', 1), (@SMRooms,      N'Admin',    1),
    -- System Settings: SuperAdmin only
    (@SMSettings,   N'SuperAdmin', 1),
    (@SMReportSet,  N'SuperAdmin', 1),
    (@SMModuleSet,  N'SuperAdmin', 1),
    (@SMSidebarSet, N'SuperAdmin', 1),
    -- Theme: all roles
    (@SMThemeSet,   N'SuperAdmin', 1), (@SMThemeSet,   N'Admin',    1),
    (@SMThemeSet,   N'Faculty',    1), (@SMThemeSet,   N'Student',  1),
    -- License: SuperAdmin only
    (@SMLicUpd,     N'SuperAdmin', 1);

INSERT INTO sidebar_menu_role_accesses
    (Id, SidebarMenuItemId, RoleName, IsAllowed, CreatedAt)
SELECT NEWID(), sr.ItemId, sr.RoleName, sr.IsAllowed, @Now
FROM @SidebarRoles sr
WHERE NOT EXISTS (
    SELECT 1 FROM sidebar_menu_role_accesses x
    WHERE x.SidebarMenuItemId = sr.ItemId AND x.RoleName = sr.RoleName
);

-- ═══════════════════════════════════════════════════════════
-- §16  MODULE ROLE ASSIGNMENTS
-- ═══════════════════════════════════════════════════════════
-- Grant all roles access to the modules they need
DECLARE @MRAData TABLE (ModuleKey NVARCHAR(50), RoleName NVARCHAR(50));
INSERT INTO @MRAData VALUES
    -- SuperAdmin — all
    (N'authentication', N'SuperAdmin'),(N'departments',  N'SuperAdmin'),
    (N'sis',            N'SuperAdmin'),(N'courses',      N'SuperAdmin'),
    (N'assignments',    N'SuperAdmin'),(N'quizzes',      N'SuperAdmin'),
    (N'attendance',     N'SuperAdmin'),(N'results',      N'SuperAdmin'),
    (N'notifications',  N'SuperAdmin'),(N'fyp',          N'SuperAdmin'),
    (N'ai_chat',        N'SuperAdmin'),(N'reports',      N'SuperAdmin'),
    (N'themes',         N'SuperAdmin'),(N'advanced_audit',N'SuperAdmin'),
    -- Admin
    (N'departments',    N'Admin'),(N'sis',          N'Admin'),
    (N'courses',        N'Admin'),(N'assignments',  N'Admin'),
    (N'attendance',     N'Admin'),(N'results',      N'Admin'),
    (N'notifications',  N'Admin'),(N'reports',      N'Admin'),
    (N'themes',         N'Admin'),(N'advanced_audit',N'Admin'),
    -- Faculty
    (N'courses',        N'Faculty'),(N'assignments', N'Faculty'),
    (N'quizzes',        N'Faculty'),(N'attendance',  N'Faculty'),
    (N'results',        N'Faculty'),(N'notifications',N'Faculty'),
    (N'fyp',            N'Faculty'),(N'themes',      N'Faculty'),
    (N'ai_chat',        N'Faculty'),
    -- Student
    (N'courses',        N'Student'),(N'assignments', N'Student'),
    (N'quizzes',        N'Student'),(N'attendance',  N'Student'),
    (N'results',        N'Student'),(N'notifications',N'Student'),
    (N'fyp',            N'Student'),(N'themes',      N'Student'),
    (N'ai_chat',        N'Student');

INSERT INTO module_role_assignments (Id, ModuleId, RoleName, CreatedAt)
SELECT NEWID(), m.Id, d.RoleName, @Now
FROM @MRAData d
JOIN modules m ON m.[Key] = d.ModuleKey
WHERE NOT EXISTS (
    SELECT 1 FROM module_role_assignments x
    WHERE x.ModuleId = m.Id AND x.RoleName = d.RoleName
);

-- ═══════════════════════════════════════════════════════════
-- §17  REPORT DEFINITIONS + ROLE ASSIGNMENTS
-- ═══════════════════════════════════════════════════════════
DECLARE @RptAttend  UNIQUEIDENTIFIER = NEWID();
DECLARE @RptResults UNIQUEIDENTIFIER = NEWID();
DECLARE @RptDept    UNIQUEIDENTIFIER = NEWID();
DECLARE @RptSemRes  UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key] = N'attendance-report')
    INSERT INTO report_definitions
        (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptAttend,N'attendance-report',N'Attendance Summary',
            N'Per-student and per-course attendance breakdown',1,@Now,0);
ELSE SELECT @RptAttend = Id FROM report_definitions WHERE [Key] = N'attendance-report';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key] = N'results-report')
    INSERT INTO report_definitions
        (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptResults,N'results-report',N'Student Results',
            N'Midterm and final marks for enrolled students',1,@Now,0);
ELSE SELECT @RptResults = Id FROM report_definitions WHERE [Key] = N'results-report';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key] = N'dept-summary')
    INSERT INTO report_definitions
        (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptDept,N'dept-summary',N'Department Summary',
            N'High-level student and course counts by department',1,@Now,0);
ELSE SELECT @RptDept = Id FROM report_definitions WHERE [Key] = N'dept-summary';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key] = N'semester-results')
    INSERT INTO report_definitions
        (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptSemRes,N'semester-results',N'Semester Results',
            N'All published results for students in a specific semester',1,@Now,0);
ELSE SELECT @RptSemRes = Id FROM report_definitions WHERE [Key] = N'semester-results';

DECLARE @RRAData TABLE (RptId UNIQUEIDENTIFIER, RoleName NVARCHAR(50));
INSERT INTO @RRAData VALUES
    (@RptAttend,  N'Admin'),   (@RptAttend,  N'Faculty'),  (@RptAttend,  N'Student'),
    (@RptResults, N'Admin'),   (@RptResults, N'Faculty'),  (@RptResults, N'Student'),
    (@RptDept,    N'Admin'),   (@RptDept,    N'SuperAdmin'),
    (@RptSemRes,  N'Admin'),   (@RptSemRes,  N'Faculty');

INSERT INTO report_role_assignments (Id, ReportDefinitionId, RoleName, CreatedAt)
SELECT NEWID(), r.RptId, r.RoleName, @Now
FROM @RRAData r
WHERE NOT EXISTS (
    SELECT 1 FROM report_role_assignments x
    WHERE x.ReportDefinitionId = r.RptId AND x.RoleName = r.RoleName
);

-- ═══════════════════════════════════════════════════════════
-- §18  ASSIGNMENT
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM assignments WHERE Title = N'Lab 1 — Classes and Objects'
               AND CourseOfferingId = @OfferOOP)
    INSERT INTO assignments
        (Id, CourseOfferingId, Title, Description, DueDate,
         MaxMarks, IsPublished, PublishedAt, CreatedAt, IsDeleted)
    VALUES
        (NEWID(), @OfferOOP, N'Lab 1 — Classes and Objects',
         N'Implement a simple Bank Account class demonstrating encapsulation.',
         DATEADD(DAY, 14, @Now), 20, 1, @Now, @Now, 0);

-- ═══════════════════════════════════════════════════════════
-- §19  ATTENDANCE RECORDS  (enough rows for the view to work)
-- ═══════════════════════════════════════════════════════════
DECLARE @AttBase DATETIME2 = DATEADD(DAY, -14, @Now);

IF NOT EXISTS (SELECT 1 FROM attendance_records
               WHERE StudentProfileId = @SpStudent AND CourseOfferingId = @OfferOOP
               AND CAST(Date AS DATE) = CAST(@AttBase AS DATE))
BEGIN
    -- 4 Present, 1 Absent for OOP
    INSERT INTO attendance_records
        (Id, StudentProfileId, CourseOfferingId, Date, Status,
         MarkedByUserId, CreatedAt)
    SELECT NEWID(), @SpStudent, @OfferOOP, DATEADD(DAY, v.n, @AttBase),
           CASE WHEN v.n = 7 THEN N'Absent' ELSE N'Present' END,
           @UserFaculty, @Now
    FROM (VALUES(0),(2),(4),(7),(9)) AS v(n);

    -- 5 Present for DB
    INSERT INTO attendance_records
        (Id, StudentProfileId, CourseOfferingId, Date, Status,
         MarkedByUserId, CreatedAt)
    SELECT NEWID(), @SpStudent, @OfferDB, DATEADD(DAY, v.n, @AttBase),
           N'Present',
           @UserFaculty, @Now
    FROM (VALUES(1),(3),(5),(8),(10)) AS v(n);
END

-- ═══════════════════════════════════════════════════════════
-- §20  RESULTS  (exercises vw_student_results_summary +
--               sp_recalculate_student_cgpa)
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM results
               WHERE StudentProfileId = @SpStudent AND CourseOfferingId = @OfferOOP
               AND ResultType = N'Midterm')
    INSERT INTO results
        (Id, StudentProfileId, CourseOfferingId, ResultType,
         MarksObtained, MaxMarks, IsPublished, PublishedAt,
         PublishedByUserId, CreatedAt)
    VALUES
        (NEWID(), @SpStudent, @OfferOOP, N'Midterm',
         72, 100, 1, @Now, @UserFaculty, @Now);

IF NOT EXISTS (SELECT 1 FROM results
               WHERE StudentProfileId = @SpStudent AND CourseOfferingId = @OfferDB
               AND ResultType = N'Midterm')
    INSERT INTO results
        (Id, StudentProfileId, CourseOfferingId, ResultType,
         MarksObtained, MaxMarks, IsPublished, PublishedAt,
         PublishedByUserId, CreatedAt)
    VALUES
        (NEWID(), @SpStudent, @OfferDB, N'Midterm',
         85, 100, 1, @Now, @UserFaculty, @Now);

-- ═══════════════════════════════════════════════════════════
-- §21  QUIZ + QUESTIONS + OPTIONS
-- ═══════════════════════════════════════════════════════════
DECLARE @Quiz1  UNIQUEIDENTIFIER = NEWID();
DECLARE @QQ1    UNIQUEIDENTIFIER = NEWID();
DECLARE @QQ2    UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM quizzes WHERE Title = N'OOP Fundamentals Quiz 1'
               AND CourseOfferingId = @OfferOOP)
BEGIN
    INSERT INTO quizzes
        (Id, CourseOfferingId, Title, Instructions, TimeLimitMinutes,
         MaxAttempts, AvailableFrom, AvailableUntil,
         IsPublished, IsActive, CreatedByUserId, CreatedAt)
    VALUES
        (@Quiz1, @OfferOOP, N'OOP Fundamentals Quiz 1',
         N'Answer all questions. Closed notes.',
         20, 1, @Now, DATEADD(DAY, 7, @Now),
         1, 1, @UserFaculty, @Now);

    INSERT INTO quiz_questions
        (Id, QuizId, [Type], [Text], Marks, OrderIndex, CreatedAt)
    VALUES
        (@QQ1, @Quiz1, N'MultipleChoice',
         N'Which OOP principle hides internal state from outside code?',
         1, 1, @Now),
        (@QQ2, @Quiz1, N'TrueFalse',
         N'Inheritance allows a subclass to inherit from multiple parent classes in Java.',
         1, 2, @Now);

    -- Options for Q1
    INSERT INTO quiz_options
        (Id, QuizQuestionId, [Text], IsCorrect, OrderIndex, CreatedAt)
    VALUES
        (NEWID(), @QQ1, N'Encapsulation', 1, 1, @Now),
        (NEWID(), @QQ1, N'Polymorphism',  0, 2, @Now),
        (NEWID(), @QQ1, N'Inheritance',   0, 3, @Now),
        (NEWID(), @QQ1, N'Abstraction',   0, 4, @Now);

    -- Options for Q2 (True/False)
    INSERT INTO quiz_options
        (Id, QuizQuestionId, [Text], IsCorrect, OrderIndex, CreatedAt)
    VALUES
        (NEWID(), @QQ2, N'True',  0, 1, @Now),
        (NEWID(), @QQ2, N'False', 1, 2, @Now);
END

-- ═══════════════════════════════════════════════════════════
-- §22  NOTIFICATION
-- ═══════════════════════════════════════════════════════════
DECLARE @Notif1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM notifications WHERE Title = N'Welcome to Tabsan EduSphere!')
BEGIN
    INSERT INTO notifications
        (Id, Title, Body, [Type], SenderUserId,
         IsSystemGenerated, IsActive, CreatedAt)
    VALUES
        (@Notif1, N'Welcome to Tabsan EduSphere!',
         N'Your account is active. Log in and explore your dashboard.',
         N'General', @UserSuperAdmin, 1, 1, @Now);

    INSERT INTO notification_recipients
        (Id, NotificationId, RecipientUserId, IsRead, CreatedAt)
    VALUES
        (NEWID(), @Notif1, @UserAdmin,   0, @Now),
        (NEWID(), @Notif1, @UserFaculty, 0, @Now),
        (NEWID(), @Notif1, @UserStudent, 0, @Now);
END

-- ═══════════════════════════════════════════════════════════
-- §23  FYP PROJECT
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM fyp_projects WHERE StudentProfileId = @SpStudent)
    INSERT INTO fyp_projects
        (Id, StudentProfileId, DepartmentId, Title, Description,
         Status, SupervisorUserId, CreatedAt)
    VALUES
        (NEWID(), @SpStudent, @DeptCS,
         N'Automated Attendance System using Face Recognition',
         N'A deep-learning solution to automate classroom attendance tracking.',
         N'Proposed', @UserFaculty, @Now);

-- ═══════════════════════════════════════════════════════════
-- §24  PAYMENT RECEIPT
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM payment_receipts WHERE StudentProfileId = @SpStudent)
    INSERT INTO payment_receipts
        (Id, StudentProfileId, CreatedByUserId, Status,
         Amount, Description, DueDate, CreatedAt, UpdatedAt, IsDeleted)
    VALUES
        (NEWID(), @SpStudent, @UserAdmin, 0,
         45000.00, N'Spring 2026 Semester Fee',
         DATEADD(DAY, 30, @Now), @Now, @Now, 0);

COMMIT TRANSACTION;

-- ═══════════════════════════════════════════════════════════
-- §25  VERIFY — spot-check counts and exercise views/procs
-- ═══════════════════════════════════════════════════════════
PRINT '── Seeded row counts ──────────────────────────────────';
SELECT 'roles'                   AS [Table], COUNT(*) AS Rows FROM roles
UNION ALL SELECT 'modules',              COUNT(*) FROM modules
UNION ALL SELECT 'users',                COUNT(*) FROM users         WHERE IsDeleted = 0
UNION ALL SELECT 'departments',          COUNT(*) FROM departments   WHERE IsDeleted = 0
UNION ALL SELECT 'courses',              COUNT(*) FROM courses        WHERE IsDeleted = 0
UNION ALL SELECT 'course_offerings',     COUNT(*) FROM course_offerings WHERE IsDeleted = 0
UNION ALL SELECT 'enrollments',          COUNT(*) FROM enrollments
UNION ALL SELECT 'attendance_records',   COUNT(*) FROM attendance_records
UNION ALL SELECT 'results',              COUNT(*) FROM results
UNION ALL SELECT 'assignments',          COUNT(*) FROM assignments   WHERE IsDeleted = 0
UNION ALL SELECT 'quizzes',              COUNT(*) FROM quizzes
UNION ALL SELECT 'notifications',        COUNT(*) FROM notifications
UNION ALL SELECT 'sidebar_menu_items',   COUNT(*) FROM sidebar_menu_items WHERE IsDeleted = 0
UNION ALL SELECT 'module_role_assignments', COUNT(*) FROM module_role_assignments
UNION ALL SELECT 'report_definitions',   COUNT(*) FROM report_definitions WHERE IsDeleted = 0
ORDER BY [Table];

PRINT '── vw_student_attendance_summary ──────────────────────';
SELECT * FROM vw_student_attendance_summary;

PRINT '── vw_student_results_summary ─────────────────────────';
SELECT * FROM vw_student_results_summary;

PRINT '── vw_course_enrollment_summary ───────────────────────';
SELECT * FROM vw_course_enrollment_summary;

PRINT '── sp_get_attendance_below_threshold (75%) ────────────';
EXEC sp_get_attendance_below_threshold @ThresholdPercent = 75.0;

PRINT '── sp_recalculate_student_cgpa ────────────────────────';
DECLARE @SpId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM student_profiles);
IF @SpId IS NOT NULL EXEC sp_recalculate_student_cgpa @StudentProfileId = @SpId;
SELECT Cgpa FROM student_profiles WHERE Id = @SpId;

PRINT '';
PRINT '✓ Script 1 complete.';
PRINT '  Log in as superadmin and reset passwords for test accounts.';
PRINT '  Or run Scripts\GenerateTestHashes.ps1 to set real password hashes.';
