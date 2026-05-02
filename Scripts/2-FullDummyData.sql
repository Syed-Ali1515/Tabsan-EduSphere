-- ============================================================
-- Tabsan EduSphere — Script 2: Full Dummy Data
-- ============================================================
-- PURPOSE : Rich, realistic dummy data covering EVERY table,
--           all 4 roles, all modules, views, and stored procs.
--           Designed for comprehensive QA and demo sessions.
--
-- INCLUDES everything in Script 1 (self-contained — no need
-- to run Script 1 first).
--
-- PREREQUISITES  — same as Script 1.
--   Apply EF migrations + run the API once (sets up schema +
--   DatabaseSeeder seeds roles/modules/SuperAdmin).
--
-- TEST ACCOUNTS  (reset passwords via admin panel or
--                run Scripts\GenerateTestHashes.ps1 first)
--
--   Role       Username           Email
--   ─────────  ─────────────────  ─────────────────────────────
--   SuperAdmin superadmin         superadmin@tabsan.local
--   Admin      admin.cs           admin.cs@tabsan.local
--   Admin      admin.se           admin.se@tabsan.local
--   Admin      admin.it           admin.it@tabsan.local
--   Faculty    dr.ahmed           ahmed@tabsan.local
--   Faculty    dr.sara            sara@tabsan.local
--   Faculty    mr.ali             ali@tabsan.local
--   Faculty    ms.zara            zara@tabsan.local
--   Faculty    prof.khan          khan@tabsan.local
--   Student    s.aslam            aslam@student.tabsan.local
--   Student    s.fatima           fatima@student.tabsan.local
--   Student    s.usman            usman@student.tabsan.local
--   Student    s.hina             hina@student.tabsan.local
--   Student    s.tariq            tariq@student.tabsan.local
--   Student    s.nadia            nadia@student.tabsan.local
--   Student    s.bilal            bilal@student.tabsan.local
--   Student    s.rabia            rabia@student.tabsan.local
--   Student    s.kamran           kamran@student.tabsan.local
--   Student    s.shazia           shazia@student.tabsan.local
--
-- DATABASE : TabsanEduSphere  (auto-created if missing)
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

DECLARE @Now DATETIME2 = GETUTCDATE();

-- ── Password placeholder ─────────────────────────────────────
DECLARE @PwdHash NVARCHAR(500) =
    N'PLACEHOLDER_RUN_GenerateTestHashes.ps1_TO_REPLACE';
-- ─────────────────────────────────────────────────────────────

-- ═══════════════════════════════════════════════════════════
-- §1  ROLES
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
-- §2  MODULES  (all activated)
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

INSERT INTO module_status (Id, ModuleId, IsActive, ActivatedAt, Source, CreatedAt)
SELECT NEWID(), m.Id, 1, @Now, N'seed', @Now
FROM modules m
WHERE NOT EXISTS (SELECT 1 FROM module_status ms WHERE ms.ModuleId = m.Id);

UPDATE ms SET IsActive = 1, ActivatedAt = @Now, Source = N'seed'
FROM module_status ms WHERE ms.IsActive = 0;

-- ═══════════════════════════════════════════════════════════
-- §3  LICENSE STATE
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM license_state WHERE Status = N'Active')
    INSERT INTO license_state (Id, LicenseHash, LicenseType, Status, ActivatedAt, ExpiresAt, CreatedAt)
    VALUES (NEWID(), N'TEST-PERMANENT-LICENSE-DO-NOT-DEPLOY-TO-PROD',
            N'Permanent', N'Active', @Now, NULL, @Now);

-- ═══════════════════════════════════════════════════════════
-- §4  DEPARTMENTS  (3)
-- ═══════════════════════════════════════════════════════════
DECLARE @DeptCS UNIQUEIDENTIFIER = NEWID();
DECLARE @DeptSE UNIQUEIDENTIFIER = NEWID();
DECLARE @DeptIT UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM departments WHERE Code = N'CS')
    INSERT INTO departments (Id,Name,Code,IsActive,CreatedAt,IsDeleted)
    VALUES (@DeptCS,N'Computer Science',N'CS',1,@Now,0);
ELSE SELECT @DeptCS = Id FROM departments WHERE Code = N'CS';

IF NOT EXISTS (SELECT 1 FROM departments WHERE Code = N'SE')
    INSERT INTO departments (Id,Name,Code,IsActive,CreatedAt,IsDeleted)
    VALUES (@DeptSE,N'Software Engineering',N'SE',1,@Now,0);
ELSE SELECT @DeptSE = Id FROM departments WHERE Code = N'SE';

IF NOT EXISTS (SELECT 1 FROM departments WHERE Code = N'IT')
    INSERT INTO departments (Id,Name,Code,IsActive,CreatedAt,IsDeleted)
    VALUES (@DeptIT,N'Information Technology',N'IT',1,@Now,0);
ELSE SELECT @DeptIT = Id FROM departments WHERE Code = N'IT';

-- ═══════════════════════════════════════════════════════════
-- §5  BUILDINGS + ROOMS  (2 buildings, 6 rooms)
-- ═══════════════════════════════════════════════════════════
DECLARE @BlkA UNIQUEIDENTIFIER = NEWID();
DECLARE @BlkB UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM buildings WHERE Code = N'BLK-A')
BEGIN
    INSERT INTO buildings (Id,Name,Code,IsActive,CreatedAt,IsDeleted)
    VALUES (@BlkA,N'Block A',N'BLK-A',1,@Now,0);
    INSERT INTO rooms (Id,BuildingId,Number,Capacity,IsActive,CreatedAt,IsDeleted) VALUES
        (NEWID(),@BlkA,N'A-101',60,1,@Now,0),
        (NEWID(),@BlkA,N'A-102',40,1,@Now,0),
        (NEWID(),@BlkA,N'A-Lab1',30,1,@Now,0);
END
ELSE SELECT @BlkA = Id FROM buildings WHERE Code = N'BLK-A';

IF NOT EXISTS (SELECT 1 FROM buildings WHERE Code = N'BLK-B')
BEGIN
    INSERT INTO buildings (Id,Name,Code,IsActive,CreatedAt,IsDeleted)
    VALUES (@BlkB,N'Block B',N'BLK-B',1,@Now,0);
    INSERT INTO rooms (Id,BuildingId,Number,Capacity,IsActive,CreatedAt,IsDeleted) VALUES
        (NEWID(),@BlkB,N'B-201',80,1,@Now,0),
        (NEWID(),@BlkB,N'B-202',50,1,@Now,0),
        (NEWID(),@BlkB,N'B-Lab2',25,1,@Now,0);
END
ELSE SELECT @BlkB = Id FROM buildings WHERE Code = N'BLK-B';

-- ═══════════════════════════════════════════════════════════
-- §6  ACADEMIC PROGRAMS  (4 programs)
-- ═══════════════════════════════════════════════════════════
DECLARE @ProgBSCS UNIQUEIDENTIFIER = NEWID();
DECLARE @ProgBSSE UNIQUEIDENTIFIER = NEWID();
DECLARE @ProgMSCS UNIQUEIDENTIFIER = NEWID();
DECLARE @ProgBSIT UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM academic_programs WHERE Code = N'BSCS')
    INSERT INTO academic_programs (Id,Name,Code,DepartmentId,TotalSemesters,IsActive,CreatedAt,IsDeleted)
    VALUES (@ProgBSCS,N'BS Computer Science',N'BSCS',@DeptCS,8,1,@Now,0);
ELSE SELECT @ProgBSCS = Id FROM academic_programs WHERE Code = N'BSCS';

IF NOT EXISTS (SELECT 1 FROM academic_programs WHERE Code = N'BSSE')
    INSERT INTO academic_programs (Id,Name,Code,DepartmentId,TotalSemesters,IsActive,CreatedAt,IsDeleted)
    VALUES (@ProgBSSE,N'BS Software Engineering',N'BSSE',@DeptSE,8,1,@Now,0);
ELSE SELECT @ProgBSSE = Id FROM academic_programs WHERE Code = N'BSSE';

IF NOT EXISTS (SELECT 1 FROM academic_programs WHERE Code = N'MSCS')
    INSERT INTO academic_programs (Id,Name,Code,DepartmentId,TotalSemesters,IsActive,CreatedAt,IsDeleted)
    VALUES (@ProgMSCS,N'MS Computer Science',N'MSCS',@DeptCS,4,1,@Now,0);
ELSE SELECT @ProgMSCS = Id FROM academic_programs WHERE Code = N'MSCS';

IF NOT EXISTS (SELECT 1 FROM academic_programs WHERE Code = N'BSIT')
    INSERT INTO academic_programs (Id,Name,Code,DepartmentId,TotalSemesters,IsActive,CreatedAt,IsDeleted)
    VALUES (@ProgBSIT,N'BS Information Technology',N'BSIT',@DeptIT,8,1,@Now,0);
ELSE SELECT @ProgBSIT = Id FROM academic_programs WHERE Code = N'BSIT';

-- ═══════════════════════════════════════════════════════════
-- §7  SEMESTERS  (past + current per department)
-- ═══════════════════════════════════════════════════════════
DECLARE @SemFall25CS   UNIQUEIDENTIFIER = NEWID();
DECLARE @SemSpr26CS    UNIQUEIDENTIFIER = NEWID();
DECLARE @SemSpr26SE    UNIQUEIDENTIFIER = NEWID();
DECLARE @SemSpr26IT    UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM semesters WHERE Name = N'Fall 2025 - CS')
    INSERT INTO semesters (Id,Name,StartDate,EndDate,IsClosed,CreatedAt,IsDeleted)
    VALUES (@SemFall25CS,N'Fall 2025 - CS','2025-09-01','2026-01-10',1,@Now,0);
ELSE SELECT @SemFall25CS = Id FROM semesters WHERE Name = N'Fall 2025 - CS';

IF NOT EXISTS (SELECT 1 FROM semesters WHERE Name = N'Spring 2026 - CS')
    INSERT INTO semesters (Id,Name,StartDate,EndDate,IsClosed,CreatedAt,IsDeleted)
    VALUES (@SemSpr26CS,N'Spring 2026 - CS','2026-01-15','2026-05-31',0,@Now,0);
ELSE SELECT @SemSpr26CS = Id FROM semesters WHERE Name = N'Spring 2026 - CS';

IF NOT EXISTS (SELECT 1 FROM semesters WHERE Name = N'Spring 2026 - SE')
    INSERT INTO semesters (Id,Name,StartDate,EndDate,IsClosed,CreatedAt,IsDeleted)
    VALUES (@SemSpr26SE,N'Spring 2026 - SE','2026-01-15','2026-05-31',0,@Now,0);
ELSE SELECT @SemSpr26SE = Id FROM semesters WHERE Name = N'Spring 2026 - SE';

IF NOT EXISTS (SELECT 1 FROM semesters WHERE Name = N'Spring 2026 - IT')
    INSERT INTO semesters (Id,Name,StartDate,EndDate,IsClosed,CreatedAt,IsDeleted)
    VALUES (@SemSpr26IT,N'Spring 2026 - IT','2026-01-15','2026-05-31',0,@Now,0);
ELSE SELECT @SemSpr26IT = Id FROM semesters WHERE Name = N'Spring 2026 - IT';

-- ═══════════════════════════════════════════════════════════
-- §8  COURSES  (10 courses across 3 departments)
-- ═══════════════════════════════════════════════════════════
DECLARE @COOP   UNIQUEIDENTIFIER = NEWID(); -- CS-301 OOP
DECLARE @CDB    UNIQUEIDENTIFIER = NEWID(); -- CS-302 DB
DECLARE @CDS    UNIQUEIDENTIFIER = NEWID(); -- CS-401 Data Structures
DECLARE @COS    UNIQUEIDENTIFIER = NEWID(); -- CS-402 OS
DECLARE @CSE1   UNIQUEIDENTIFIER = NEWID(); -- SE-301 Software Requirements
DECLARE @CSE2   UNIQUEIDENTIFIER = NEWID(); -- SE-302 Software Design
DECLARE @CNet   UNIQUEIDENTIFIER = NEWID(); -- CS-501 Computer Networks
DECLARE @CAI    UNIQUEIDENTIFIER = NEWID(); -- CS-502 Artificial Intelligence
DECLARE @CIT1   UNIQUEIDENTIFIER = NEWID(); -- IT-301 Web Technologies
DECLARE @CIT2   UNIQUEIDENTIFIER = NEWID(); -- IT-302 Network Admin

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-301' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@COOP,N'Object-Oriented Programming',N'CS-301',3,@DeptCS,1,@Now,0);
ELSE SELECT @COOP = Id FROM courses WHERE Code=N'CS-301' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-302' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CDB,N'Database Systems',N'CS-302',3,@DeptCS,1,@Now,0);
ELSE SELECT @CDB = Id FROM courses WHERE Code=N'CS-302' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-401' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CDS,N'Data Structures and Algorithms',N'CS-401',3,@DeptCS,1,@Now,0);
ELSE SELECT @CDS = Id FROM courses WHERE Code=N'CS-401' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-402' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@COS,N'Operating Systems',N'CS-402',3,@DeptCS,1,@Now,0);
ELSE SELECT @COS = Id FROM courses WHERE Code=N'CS-402' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'SE-301' AND DepartmentId=@DeptSE)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CSE1,N'Software Requirements Engineering',N'SE-301',3,@DeptSE,1,@Now,0);
ELSE SELECT @CSE1 = Id FROM courses WHERE Code=N'SE-301' AND DepartmentId=@DeptSE;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'SE-302' AND DepartmentId=@DeptSE)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CSE2,N'Software Design Patterns',N'SE-302',3,@DeptSE,1,@Now,0);
ELSE SELECT @CSE2 = Id FROM courses WHERE Code=N'SE-302' AND DepartmentId=@DeptSE;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-501' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CNet,N'Computer Networks',N'CS-501',3,@DeptCS,1,@Now,0);
ELSE SELECT @CNet = Id FROM courses WHERE Code=N'CS-501' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'CS-502' AND DepartmentId=@DeptCS)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CAI,N'Artificial Intelligence',N'CS-502',3,@DeptCS,1,@Now,0);
ELSE SELECT @CAI = Id FROM courses WHERE Code=N'CS-502' AND DepartmentId=@DeptCS;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'IT-301' AND DepartmentId=@DeptIT)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CIT1,N'Web Technologies',N'IT-301',3,@DeptIT,1,@Now,0);
ELSE SELECT @CIT1 = Id FROM courses WHERE Code=N'IT-301' AND DepartmentId=@DeptIT;

IF NOT EXISTS (SELECT 1 FROM courses WHERE Code=N'IT-302' AND DepartmentId=@DeptIT)
    INSERT INTO courses (Id,Title,Code,CreditHours,DepartmentId,IsActive,CreatedAt,IsDeleted)
    VALUES (@CIT2,N'Network Administration',N'IT-302',3,@DeptIT,1,@Now,0);
ELSE SELECT @CIT2 = Id FROM courses WHERE Code=N'IT-302' AND DepartmentId=@DeptIT;

-- ═══════════════════════════════════════════════════════════
-- §9  USERS  (3 admins, 5 faculty, 10 students, 1 superadmin)
-- ═══════════════════════════════════════════════════════════
DECLARE @UserSA    UNIQUEIDENTIFIER;
DECLARE @UAdmCS    UNIQUEIDENTIFIER = NEWID();
DECLARE @UAdmSE    UNIQUEIDENTIFIER = NEWID();
DECLARE @UAdmIT    UNIQUEIDENTIFIER = NEWID();
DECLARE @UFDrA     UNIQUEIDENTIFIER = NEWID(); -- dr.ahmed (CS)
DECLARE @UFDrS     UNIQUEIDENTIFIER = NEWID(); -- dr.sara  (CS)
DECLARE @UFMrA     UNIQUEIDENTIFIER = NEWID(); -- mr.ali   (SE)
DECLARE @UFMsZ     UNIQUEIDENTIFIER = NEWID(); -- ms.zara  (IT)
DECLARE @UFProfK   UNIQUEIDENTIFIER = NEWID(); -- prof.khan(CS)
DECLARE @US1       UNIQUEIDENTIFIER = NEWID(); -- s.aslam
DECLARE @US2       UNIQUEIDENTIFIER = NEWID(); -- s.fatima
DECLARE @US3       UNIQUEIDENTIFIER = NEWID(); -- s.usman
DECLARE @US4       UNIQUEIDENTIFIER = NEWID(); -- s.hina
DECLARE @US5       UNIQUEIDENTIFIER = NEWID(); -- s.tariq
DECLARE @US6       UNIQUEIDENTIFIER = NEWID(); -- s.nadia
DECLARE @US7       UNIQUEIDENTIFIER = NEWID(); -- s.bilal
DECLARE @US8       UNIQUEIDENTIFIER = NEWID(); -- s.rabia
DECLARE @US9       UNIQUEIDENTIFIER = NEWID(); -- s.kamran
DECLARE @US10      UNIQUEIDENTIFIER = NEWID(); -- s.shazia

SELECT @UserSA = Id FROM users WHERE Username = N'superadmin';
IF @UserSA IS NULL
BEGIN
    SET @UserSA = NEWID();
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UserSA,N'superadmin',N'superadmin@tabsan.local',@PwdHash,@RoleSuperAdmin,NULL,1,0,0,@Now,0);
END

-- Admin users
IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'admin.cs')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UAdmCS,N'admin.cs',N'admin.cs@tabsan.local',@PwdHash,@RoleAdmin,@DeptCS,1,0,0,@Now,0);
ELSE SELECT @UAdmCS = Id FROM users WHERE Username=N'admin.cs';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'admin.se')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UAdmSE,N'admin.se',N'admin.se@tabsan.local',@PwdHash,@RoleAdmin,@DeptSE,1,0,0,@Now,0);
ELSE SELECT @UAdmSE = Id FROM users WHERE Username=N'admin.se';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'admin.it')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UAdmIT,N'admin.it',N'admin.it@tabsan.local',@PwdHash,@RoleAdmin,@DeptIT,1,0,0,@Now,0);
ELSE SELECT @UAdmIT = Id FROM users WHERE Username=N'admin.it';

-- Faculty users
IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'dr.ahmed')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UFDrA,N'dr.ahmed',N'ahmed@tabsan.local',@PwdHash,@RoleFaculty,@DeptCS,1,0,0,@Now,0);
ELSE SELECT @UFDrA = Id FROM users WHERE Username=N'dr.ahmed';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'dr.sara')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UFDrS,N'dr.sara',N'sara@tabsan.local',@PwdHash,@RoleFaculty,@DeptCS,1,0,0,@Now,0);
ELSE SELECT @UFDrS = Id FROM users WHERE Username=N'dr.sara';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'mr.ali')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UFMrA,N'mr.ali',N'ali@tabsan.local',@PwdHash,@RoleFaculty,@DeptSE,1,0,0,@Now,0);
ELSE SELECT @UFMrA = Id FROM users WHERE Username=N'mr.ali';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'ms.zara')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UFMsZ,N'ms.zara',N'zara@tabsan.local',@PwdHash,@RoleFaculty,@DeptIT,1,0,0,@Now,0);
ELSE SELECT @UFMsZ = Id FROM users WHERE Username=N'ms.zara';

IF NOT EXISTS (SELECT 1 FROM users WHERE Username=N'prof.khan')
    INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
    VALUES (@UFProfK,N'prof.khan',N'khan@tabsan.local',@PwdHash,@RoleFaculty,@DeptCS,1,0,0,@Now,0);
ELSE SELECT @UFProfK = Id FROM users WHERE Username=N'prof.khan';

-- Student users (10)
DECLARE @Students TABLE (VarRef INT, Username NVARCHAR(100), Email NVARCHAR(256), DeptId UNIQUEIDENTIFIER);
INSERT INTO @Students VALUES
    (1, N's.aslam',  N'aslam@student.tabsan.local',  @DeptCS),
    (2, N's.fatima', N'fatima@student.tabsan.local',  @DeptCS),
    (3, N's.usman',  N'usman@student.tabsan.local',   @DeptCS),
    (4, N's.hina',   N'hina@student.tabsan.local',    @DeptSE),
    (5, N's.tariq',  N'tariq@student.tabsan.local',   @DeptSE),
    (6, N's.nadia',  N'nadia@student.tabsan.local',   @DeptCS),
    (7, N's.bilal',  N'bilal@student.tabsan.local',   @DeptIT),
    (8, N's.rabia',  N'rabia@student.tabsan.local',   @DeptIT),
    (9, N's.kamran', N'kamran@student.tabsan.local',  @DeptCS),
    (10,N's.shazia', N'shazia@student.tabsan.local',  @DeptSE);

INSERT INTO users (Id,Username,Email,PasswordHash,RoleId,DepartmentId,IsActive,IsLockedOut,FailedLoginAttempts,CreatedAt,IsDeleted)
SELECT NEWID(), s.Username, s.Email, @PwdHash, @RoleStudent, s.DeptId, 1, 0, 0, @Now, 0
FROM @Students s
WHERE NOT EXISTS (SELECT 1 FROM users u WHERE u.Username = s.Username);

-- Capture student user IDs
SELECT @US1  = Id FROM users WHERE Username = N's.aslam';
SELECT @US2  = Id FROM users WHERE Username = N's.fatima';
SELECT @US3  = Id FROM users WHERE Username = N's.usman';
SELECT @US4  = Id FROM users WHERE Username = N's.hina';
SELECT @US5  = Id FROM users WHERE Username = N's.tariq';
SELECT @US6  = Id FROM users WHERE Username = N's.nadia';
SELECT @US7  = Id FROM users WHERE Username = N's.bilal';
SELECT @US8  = Id FROM users WHERE Username = N's.rabia';
SELECT @US9  = Id FROM users WHERE Username = N's.kamran';
SELECT @US10 = Id FROM users WHERE Username = N's.shazia';

-- ═══════════════════════════════════════════════════════════
-- §10  STUDENT PROFILES  (10 students, 2 semesters in)
-- ═══════════════════════════════════════════════════════════
DECLARE @SP1  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP2  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP3  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP4  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP5  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP6  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP7  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP8  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP9  UNIQUEIDENTIFIER = NEWID();
DECLARE @SP10 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US1)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP1, @US1,N'2026-CS-1001',@ProgBSCS,@DeptCS,'2024-09-01',3.20,2,1,@Now,0);
ELSE SELECT @SP1 = Id FROM student_profiles WHERE UserId=@US1;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US2)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP2, @US2,N'2026-CS-1002',@ProgBSCS,@DeptCS,'2024-09-01',3.50,2,1,@Now,0);
ELSE SELECT @SP2 = Id FROM student_profiles WHERE UserId=@US2;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US3)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP3, @US3,N'2026-CS-1003',@ProgBSCS,@DeptCS,'2024-09-01',2.80,2,1,@Now,0);
ELSE SELECT @SP3 = Id FROM student_profiles WHERE UserId=@US3;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US4)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP4, @US4,N'2026-SE-1001',@ProgBSSE,@DeptSE,'2024-09-01',3.70,2,1,@Now,0);
ELSE SELECT @SP4 = Id FROM student_profiles WHERE UserId=@US4;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US5)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP5, @US5,N'2026-SE-1002',@ProgBSSE,@DeptSE,'2024-09-01',2.60,2,1,@Now,0);
ELSE SELECT @SP5 = Id FROM student_profiles WHERE UserId=@US5;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US6)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP6, @US6,N'2026-CS-1004',@ProgBSCS,@DeptCS,'2024-09-01',3.10,2,1,@Now,0);
ELSE SELECT @SP6 = Id FROM student_profiles WHERE UserId=@US6;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US7)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP7, @US7,N'2026-IT-1001',@ProgBSIT,@DeptIT,'2024-09-01',3.00,2,1,@Now,0);
ELSE SELECT @SP7 = Id FROM student_profiles WHERE UserId=@US7;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US8)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP8, @US8,N'2026-IT-1002',@ProgBSIT,@DeptIT,'2024-09-01',3.40,2,1,@Now,0);
ELSE SELECT @SP8 = Id FROM student_profiles WHERE UserId=@US8;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US9)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP9, @US9,N'2026-CS-1005',@ProgMSCS,@DeptCS,'2024-09-01',3.80,2,1,@Now,0);
ELSE SELECT @SP9 = Id FROM student_profiles WHERE UserId=@US9;

IF NOT EXISTS (SELECT 1 FROM student_profiles WHERE UserId=@US10)
    INSERT INTO student_profiles (Id,UserId,RegistrationNumber,ProgramId,DepartmentId,AdmissionDate,Cgpa,CurrentSemesterNumber,Status,CreatedAt,IsDeleted)
    VALUES (@SP10,@US10,N'2026-SE-1003',@ProgBSSE,@DeptSE,'2024-09-01',2.90,2,1,@Now,0);
ELSE SELECT @SP10 = Id FROM student_profiles WHERE UserId=@US10;

-- ═══════════════════════════════════════════════════════════
-- §11  FACULTY DEPARTMENT ASSIGNMENTS
-- ═══════════════════════════════════════════════════════════
DECLARE @FDAData TABLE (FId UNIQUEIDENTIFIER, DId UNIQUEIDENTIFIER);
INSERT INTO @FDAData VALUES
    (@UFDrA,   @DeptCS), (@UFDrS,   @DeptCS), (@UFProfK, @DeptCS),
    (@UFMrA,   @DeptSE), (@UFMsZ,   @DeptIT);

INSERT INTO faculty_department_assignments (Id,FacultyUserId,DepartmentId,AssignedAt,CreatedAt)
SELECT NEWID(), f.FId, f.DId, @Now, @Now
FROM @FDAData f
WHERE NOT EXISTS (
    SELECT 1 FROM faculty_department_assignments x
    WHERE x.FacultyUserId=f.FId AND x.DepartmentId=f.DId
);

-- ═══════════════════════════════════════════════════════════
-- §12  COURSE OFFERINGS  (Spring 2026 — 8 offerings)
-- ═══════════════════════════════════════════════════════════
DECLARE @OfOOP   UNIQUEIDENTIFIER = NEWID();
DECLARE @OfDB    UNIQUEIDENTIFIER = NEWID();
DECLARE @OfDS    UNIQUEIDENTIFIER = NEWID();
DECLARE @OfOS    UNIQUEIDENTIFIER = NEWID();
DECLARE @OfSE1   UNIQUEIDENTIFIER = NEWID();
DECLARE @OfSE2   UNIQUEIDENTIFIER = NEWID();
DECLARE @OfIT1   UNIQUEIDENTIFIER = NEWID();
DECLARE @OfIT2   UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@COOP AND SemesterId=@SemSpr26CS)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfOOP,@COOP,@SemSpr26CS,@UFDrA,40,1,@Now,0);
ELSE SELECT @OfOOP = Id FROM course_offerings WHERE CourseId=@COOP AND SemesterId=@SemSpr26CS;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CDB AND SemesterId=@SemSpr26CS)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfDB,@CDB,@SemSpr26CS,@UFDrS,35,1,@Now,0);
ELSE SELECT @OfDB = Id FROM course_offerings WHERE CourseId=@CDB AND SemesterId=@SemSpr26CS;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CDS AND SemesterId=@SemSpr26CS)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfDS,@CDS,@SemSpr26CS,@UFProfK,45,1,@Now,0);
ELSE SELECT @OfDS = Id FROM course_offerings WHERE CourseId=@CDS AND SemesterId=@SemSpr26CS;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@COS AND SemesterId=@SemSpr26CS)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfOS,@COS,@SemSpr26CS,@UFDrA,40,1,@Now,0);
ELSE SELECT @OfOS = Id FROM course_offerings WHERE CourseId=@COS AND SemesterId=@SemSpr26CS;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CSE1 AND SemesterId=@SemSpr26SE)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfSE1,@CSE1,@SemSpr26SE,@UFMrA,30,1,@Now,0);
ELSE SELECT @OfSE1 = Id FROM course_offerings WHERE CourseId=@CSE1 AND SemesterId=@SemSpr26SE;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CSE2 AND SemesterId=@SemSpr26SE)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfSE2,@CSE2,@SemSpr26SE,@UFMrA,30,1,@Now,0);
ELSE SELECT @OfSE2 = Id FROM course_offerings WHERE CourseId=@CSE2 AND SemesterId=@SemSpr26SE;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CIT1 AND SemesterId=@SemSpr26IT)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfIT1,@CIT1,@SemSpr26IT,@UFMsZ,35,1,@Now,0);
ELSE SELECT @OfIT1 = Id FROM course_offerings WHERE CourseId=@CIT1 AND SemesterId=@SemSpr26IT;

IF NOT EXISTS (SELECT 1 FROM course_offerings WHERE CourseId=@CIT2 AND SemesterId=@SemSpr26IT)
    INSERT INTO course_offerings (Id,CourseId,SemesterId,FacultyUserId,MaxEnrollment,IsOpen,CreatedAt,IsDeleted)
    VALUES (@OfIT2,@CIT2,@SemSpr26IT,@UFMsZ,30,1,@Now,0);
ELSE SELECT @OfIT2 = Id FROM course_offerings WHERE CourseId=@CIT2 AND SemesterId=@SemSpr26IT;

-- ═══════════════════════════════════════════════════════════
-- §13  ENROLLMENTS
-- ═══════════════════════════════════════════════════════════
DECLARE @EnrlData TABLE (SpId UNIQUEIDENTIFIER, OffId UNIQUEIDENTIFIER);
INSERT INTO @EnrlData VALUES
    -- CS students → OOP + DB + DS + OS
    (@SP1,@OfOOP), (@SP1,@OfDB),  (@SP1,@OfDS),
    (@SP2,@OfOOP), (@SP2,@OfDB),  (@SP2,@OfDS), (@SP2,@OfOS),
    (@SP3,@OfOOP), (@SP3,@OfDB),
    (@SP6,@OfOOP), (@SP6,@OfDS),  (@SP6,@OfOS),
    (@SP9,@OfDS),  (@SP9,@OfOS),
    -- SE students → SE courses
    (@SP4,@OfSE1), (@SP4,@OfSE2),
    (@SP5,@OfSE1), (@SP5,@OfSE2),
    (@SP10,@OfSE1),(@SP10,@OfSE2),
    -- IT students → IT courses
    (@SP7,@OfIT1), (@SP7,@OfIT2),
    (@SP8,@OfIT1), (@SP8,@OfIT2);

INSERT INTO enrollments (Id,StudentProfileId,CourseOfferingId,EnrolledAt,Status,CreatedAt)
SELECT NEWID(), e.SpId, e.OffId, @Now, N'Active', @Now
FROM @EnrlData e
WHERE NOT EXISTS (
    SELECT 1 FROM enrollments x
    WHERE x.StudentProfileId=e.SpId AND x.CourseOfferingId=e.OffId
);

-- ═══════════════════════════════════════════════════════════
-- §14  TIMETABLES + ENTRIES
-- ═══════════════════════════════════════════════════════════
DECLARE @TT_CS  UNIQUEIDENTIFIER = NEWID();
DECLARE @TT_SE  UNIQUEIDENTIFIER = NEWID();
DECLARE @TT_IT  UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM timetables WHERE DepartmentId=@DeptCS AND SemesterId=@SemSpr26CS)
BEGIN
    INSERT INTO timetables (Id,DepartmentId,SemesterId,IsPublished,PublishedAt,CreatedAt,IsDeleted,AcademicProgramId,EffectiveDate,SemesterNumber)
    VALUES (@TT_CS,@DeptCS,@SemSpr26CS,1,@Now,@Now,0,@ProgBSCS,CAST('2026-01-15' AS DATE),2);
    INSERT INTO timetable_entries (Id,TimetableId,DayOfWeek,StartTime,EndTime,SubjectName,RoomNumber,FacultyName,CourseId,FacultyUserId,CreatedAt)
    VALUES
        (NEWID(),@TT_CS,1,'08:00','09:30',N'Object-Oriented Programming',N'A-101',N'dr.ahmed',  @COOP,@UFDrA,@Now),
        (NEWID(),@TT_CS,1,'10:00','11:30',N'Data Structures',            N'A-102',N'prof.khan', @CDS, @UFProfK,@Now),
        (NEWID(),@TT_CS,2,'08:00','09:30',N'Database Systems',           N'A-101',N'dr.sara',   @CDB, @UFDrS,@Now),
        (NEWID(),@TT_CS,3,'08:00','09:30',N'Object-Oriented Programming',N'A-101',N'dr.ahmed',  @COOP,@UFDrA,@Now),
        (NEWID(),@TT_CS,3,'10:00','11:30',N'Operating Systems',          N'A-Lab1',N'dr.ahmed', @COS, @UFDrA,@Now),
        (NEWID(),@TT_CS,4,'08:00','09:30',N'Database Systems',           N'A-101',N'dr.sara',   @CDB, @UFDrS,@Now),
        (NEWID(),@TT_CS,4,'10:00','11:30',N'Data Structures',            N'A-102',N'prof.khan', @CDS, @UFProfK,@Now);
END

IF NOT EXISTS (SELECT 1 FROM timetables WHERE DepartmentId=@DeptSE AND SemesterId=@SemSpr26SE)
BEGIN
    INSERT INTO timetables (Id,DepartmentId,SemesterId,IsPublished,PublishedAt,CreatedAt,IsDeleted,AcademicProgramId,EffectiveDate,SemesterNumber)
    VALUES (@TT_SE,@DeptSE,@SemSpr26SE,1,@Now,@Now,0,@ProgBSSE,CAST('2026-01-15' AS DATE),2);
    INSERT INTO timetable_entries (Id,TimetableId,DayOfWeek,StartTime,EndTime,SubjectName,RoomNumber,FacultyName,CourseId,FacultyUserId,CreatedAt)
    VALUES
        (NEWID(),@TT_SE,1,'08:00','09:30',N'Software Requirements',N'B-201',N'mr.ali',@CSE1,@UFMrA,@Now),
        (NEWID(),@TT_SE,3,'08:00','09:30',N'Software Design Patterns',N'B-201',N'mr.ali',@CSE2,@UFMrA,@Now);
END

IF NOT EXISTS (SELECT 1 FROM timetables WHERE DepartmentId=@DeptIT AND SemesterId=@SemSpr26IT)
BEGIN
    INSERT INTO timetables (Id,DepartmentId,SemesterId,IsPublished,PublishedAt,CreatedAt,IsDeleted,AcademicProgramId,EffectiveDate,SemesterNumber)
    VALUES (@TT_IT,@DeptIT,@SemSpr26IT,1,@Now,@Now,0,@ProgBSIT,CAST('2026-01-15' AS DATE),2);
    INSERT INTO timetable_entries (Id,TimetableId,DayOfWeek,StartTime,EndTime,SubjectName,RoomNumber,FacultyName,CourseId,FacultyUserId,CreatedAt)
    VALUES
        (NEWID(),@TT_IT,2,'10:00','11:30',N'Web Technologies',  N'B-Lab2',N'ms.zara',@CIT1,@UFMsZ,@Now),
        (NEWID(),@TT_IT,4,'10:00','11:30',N'Network Administration',N'B-202',N'ms.zara',@CIT2,@UFMsZ,@Now);
END

-- ═══════════════════════════════════════════════════════════
-- §15  SIDEBAR MENUS + ROLE ACCESSES  (same as Script 1)
-- ═══════════════════════════════════════════════════════════
DECLARE @SMD  UNIQUEIDENTIFIER = NEWID(); DECLARE @SMTTA UNIQUEIDENTIFIER = NEWID();
DECLARE @SMTTF UNIQUEIDENTIFIER = NEWID(); DECLARE @SMTTS UNIQUEIDENTIFIER = NEWID();
DECLARE @SML  UNIQUEIDENTIFIER = NEWID(); DECLARE @SMBL UNIQUEIDENTIFIER = NEWID();
DECLARE @SMRM UNIQUEIDENTIFIER = NEWID(); DECLARE @SMSS UNIQUEIDENTIFIER = NEWID();
DECLARE @SMRS UNIQUEIDENTIFIER = NEWID(); DECLARE @SMMS UNIQUEIDENTIFIER = NEWID();
DECLARE @SMSI UNIQUEIDENTIFIER = NEWID(); DECLARE @SMTS UNIQUEIDENTIFIER = NEWID();
DECLARE @SMLU UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'dashboard')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMD,N'dashboard',N'Dashboard',N'Main overview dashboard',NULL,1,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'timetable_admin')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTA,N'timetable_admin',N'Timetable Admin',N'Manage department timetables',NULL,2,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'timetable_teacher')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTF,N'timetable_teacher',N'My Timetable (Faculty)',N'Faculty teaching schedule',NULL,3,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'timetable_student')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTTS,N'timetable_student',N'My Timetable (Student)',N'Student class schedule',NULL,4,1,1,@Now,0);

SELECT @SMD   = Id FROM sidebar_menu_items WHERE [Key]=N'dashboard';
SELECT @SMTTA = Id FROM sidebar_menu_items WHERE [Key]=N'timetable_admin';
SELECT @SMTTF = Id FROM sidebar_menu_items WHERE [Key]=N'timetable_teacher';
SELECT @SMTTS = Id FROM sidebar_menu_items WHERE [Key]=N'timetable_student';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'lookups')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SML,N'lookups',N'Lookups',N'Reference data management',NULL,5,1,1,@Now,0);
SELECT @SML  = Id FROM sidebar_menu_items WHERE [Key]=N'lookups';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'buildings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMBL,N'buildings',N'Buildings',N'Manage buildings',@SML,1,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'rooms')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMRM,N'rooms',N'Rooms',N'Manage rooms',@SML,2,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'system_settings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMSS,N'system_settings',N'System Settings',N'Platform configuration (SuperAdmin)',NULL,6,1,1,@Now,0);
SELECT @SMSS = Id FROM sidebar_menu_items WHERE [Key]=N'system_settings';

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'report_settings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMRS,N'report_settings',N'Report Settings',N'Report access control',@SMSS,1,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'module_settings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMMS,N'module_settings',N'Module Settings',N'Enable/disable modules',@SMSS,2,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'sidebar_settings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMSI,N'sidebar_settings',N'Sidebar Settings',N'Customise sidebar per role',@SMSS,3,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'theme_settings')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMTS,N'theme_settings',N'Theme Settings',N'UI colour theme',@SMSS,4,1,1,@Now,0);

IF NOT EXISTS (SELECT 1 FROM sidebar_menu_items WHERE [Key]=N'license_update')
    INSERT INTO sidebar_menu_items (Id,[Key],Name,Purpose,ParentId,DisplayOrder,IsActive,IsSystemMenu,CreatedAt,IsDeleted)
    VALUES (@SMLU,N'license_update',N'License Update',N'Upload license file',@SMSS,5,1,1,@Now,0);

-- Re-read all item IDs
SELECT @SMD  =Id FROM sidebar_menu_items WHERE [Key]=N'dashboard';
SELECT @SMTTA=Id FROM sidebar_menu_items WHERE [Key]=N'timetable_admin';
SELECT @SMTTF=Id FROM sidebar_menu_items WHERE [Key]=N'timetable_teacher';
SELECT @SMTTS=Id FROM sidebar_menu_items WHERE [Key]=N'timetable_student';
SELECT @SML  =Id FROM sidebar_menu_items WHERE [Key]=N'lookups';
SELECT @SMBL =Id FROM sidebar_menu_items WHERE [Key]=N'buildings';
SELECT @SMRM =Id FROM sidebar_menu_items WHERE [Key]=N'rooms';
SELECT @SMSS =Id FROM sidebar_menu_items WHERE [Key]=N'system_settings';
SELECT @SMRS =Id FROM sidebar_menu_items WHERE [Key]=N'report_settings';
SELECT @SMMS =Id FROM sidebar_menu_items WHERE [Key]=N'module_settings';
SELECT @SMSI =Id FROM sidebar_menu_items WHERE [Key]=N'sidebar_settings';
SELECT @SMTS =Id FROM sidebar_menu_items WHERE [Key]=N'theme_settings';
SELECT @SMLU =Id FROM sidebar_menu_items WHERE [Key]=N'license_update';

DECLARE @SRA TABLE (ItemId UNIQUEIDENTIFIER, RoleName NVARCHAR(100), IsAllowed BIT);
INSERT INTO @SRA VALUES
    (@SMD, N'SuperAdmin',1),(@SMD, N'Admin',1),(@SMD, N'Faculty',1),(@SMD, N'Student',1),
    (@SMTTA,N'SuperAdmin',1),(@SMTTA,N'Admin',1),
    (@SMTTF,N'Faculty',1),
    (@SMTTS,N'Student',1),
    (@SML, N'SuperAdmin',1),(@SML, N'Admin',1),
    (@SMBL,N'SuperAdmin',1),(@SMBL,N'Admin',1),
    (@SMRM,N'SuperAdmin',1),(@SMRM,N'Admin',1),
    (@SMSS,N'SuperAdmin',1),
    (@SMRS,N'SuperAdmin',1),
    (@SMMS,N'SuperAdmin',1),
    (@SMSI,N'SuperAdmin',1),
    (@SMTS,N'SuperAdmin',1),(@SMTS,N'Admin',1),(@SMTS,N'Faculty',1),(@SMTS,N'Student',1),
    (@SMLU,N'SuperAdmin',1);

INSERT INTO sidebar_menu_role_accesses (Id,SidebarMenuItemId,RoleName,IsAllowed,CreatedAt)
SELECT NEWID(),r.ItemId,r.RoleName,r.IsAllowed,@Now FROM @SRA r
WHERE NOT EXISTS (SELECT 1 FROM sidebar_menu_role_accesses x
                  WHERE x.SidebarMenuItemId=r.ItemId AND x.RoleName=r.RoleName);

-- ═══════════════════════════════════════════════════════════
-- §16  MODULE ROLE ASSIGNMENTS
-- ═══════════════════════════════════════════════════════════
DECLARE @MRAd TABLE (ModKey NVARCHAR(50), Role NVARCHAR(50));
INSERT INTO @MRAd VALUES
    (N'authentication',N'SuperAdmin'),(N'departments',N'SuperAdmin'),(N'sis',N'SuperAdmin'),
    (N'courses',N'SuperAdmin'),(N'assignments',N'SuperAdmin'),(N'quizzes',N'SuperAdmin'),
    (N'attendance',N'SuperAdmin'),(N'results',N'SuperAdmin'),(N'notifications',N'SuperAdmin'),
    (N'fyp',N'SuperAdmin'),(N'ai_chat',N'SuperAdmin'),(N'reports',N'SuperAdmin'),
    (N'themes',N'SuperAdmin'),(N'advanced_audit',N'SuperAdmin'),
    (N'departments',N'Admin'),(N'sis',N'Admin'),(N'courses',N'Admin'),
    (N'assignments',N'Admin'),(N'attendance',N'Admin'),(N'results',N'Admin'),
    (N'notifications',N'Admin'),(N'reports',N'Admin'),(N'themes',N'Admin'),(N'advanced_audit',N'Admin'),
    (N'courses',N'Faculty'),(N'assignments',N'Faculty'),(N'quizzes',N'Faculty'),
    (N'attendance',N'Faculty'),(N'results',N'Faculty'),(N'notifications',N'Faculty'),
    (N'fyp',N'Faculty'),(N'themes',N'Faculty'),(N'ai_chat',N'Faculty'),
    (N'courses',N'Student'),(N'assignments',N'Student'),(N'quizzes',N'Student'),
    (N'attendance',N'Student'),(N'results',N'Student'),(N'notifications',N'Student'),
    (N'fyp',N'Student'),(N'themes',N'Student'),(N'ai_chat',N'Student');

INSERT INTO module_role_assignments (Id,ModuleId,RoleName,CreatedAt)
SELECT NEWID(),m.Id,d.Role,@Now
FROM @MRAd d JOIN modules m ON m.[Key]=d.ModKey
WHERE NOT EXISTS (SELECT 1 FROM module_role_assignments x WHERE x.ModuleId=m.Id AND x.RoleName=d.Role);

-- ═══════════════════════════════════════════════════════════
-- §17  REPORT DEFINITIONS + ROLE ASSIGNMENTS
-- ═══════════════════════════════════════════════════════════
DECLARE @RptAtt UNIQUEIDENTIFIER = NEWID(); DECLARE @RptRes UNIQUEIDENTIFIER = NEWID();
DECLARE @RptDep UNIQUEIDENTIFIER = NEWID(); DECLARE @RptTrn UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key]=N'attendance-report')
    INSERT INTO report_definitions (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptAtt,N'attendance-report',N'Attendance Summary',N'Attendance breakdown by student/course',1,@Now,0);
ELSE SELECT @RptAtt = Id FROM report_definitions WHERE [Key]=N'attendance-report';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key]=N'results-report')
    INSERT INTO report_definitions (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptRes,N'results-report',N'Student Results',N'Midterm and final marks',1,@Now,0);
ELSE SELECT @RptRes = Id FROM report_definitions WHERE [Key]=N'results-report';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key]=N'dept-summary')
    INSERT INTO report_definitions (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptDep,N'dept-summary',N'Department Summary',N'Department-level statistics',1,@Now,0);
ELSE SELECT @RptDep = Id FROM report_definitions WHERE [Key]=N'dept-summary';

IF NOT EXISTS (SELECT 1 FROM report_definitions WHERE [Key]=N'transcript')
    INSERT INTO report_definitions (Id,[Key],Name,Purpose,IsActive,CreatedAt,IsDeleted)
    VALUES (@RptTrn,N'transcript',N'Student Transcript',N'Official academic transcript',1,@Now,0);
ELSE SELECT @RptTrn = Id FROM report_definitions WHERE [Key]=N'transcript';

DECLARE @RRAd TABLE (RId UNIQUEIDENTIFIER, Role NVARCHAR(50));
INSERT INTO @RRAd VALUES
    (@RptAtt,N'Admin'),(@RptAtt,N'Faculty'),(@RptAtt,N'Student'),
    (@RptRes,N'Admin'),(@RptRes,N'Faculty'),(@RptRes,N'Student'),
    (@RptDep,N'Admin'),(@RptDep,N'SuperAdmin'),
    (@RptTrn,N'Admin'),(@RptTrn,N'Student');

INSERT INTO report_role_assignments (Id,ReportDefinitionId,RoleName,CreatedAt)
SELECT NEWID(),r.RId,r.Role,@Now FROM @RRAd r
WHERE NOT EXISTS (SELECT 1 FROM report_role_assignments x WHERE x.ReportDefinitionId=r.RId AND x.RoleName=r.Role);

-- ═══════════════════════════════════════════════════════════
-- §18  ASSIGNMENTS (4 assignments across 4 offerings)
-- ═══════════════════════════════════════════════════════════
DECLARE @Asgn1 UNIQUEIDENTIFIER = NEWID(); -- OOP Lab 1
DECLARE @Asgn2 UNIQUEIDENTIFIER = NEWID(); -- OOP Lab 2
DECLARE @Asgn3 UNIQUEIDENTIFIER = NEWID(); -- DB Assignment 1
DECLARE @Asgn4 UNIQUEIDENTIFIER = NEWID(); -- SE Requirements Doc

IF NOT EXISTS (SELECT 1 FROM assignments WHERE Title=N'OOP Lab 1 — Classes and Objects' AND CourseOfferingId=@OfOOP)
    INSERT INTO assignments (Id,CourseOfferingId,Title,Description,DueDate,MaxMarks,IsPublished,PublishedAt,CreatedAt,IsDeleted)
    VALUES (@Asgn1,@OfOOP,N'OOP Lab 1 — Classes and Objects',
            N'Implement BankAccount class with deposit, withdraw, and balance methods.',
            DATEADD(DAY,-10,@Now),20,1,DATEADD(DAY,-25,@Now),@Now,0);
ELSE SELECT @Asgn1 = Id FROM assignments WHERE Title=N'OOP Lab 1 — Classes and Objects' AND CourseOfferingId=@OfOOP;

IF NOT EXISTS (SELECT 1 FROM assignments WHERE Title=N'OOP Lab 2 — Inheritance' AND CourseOfferingId=@OfOOP)
    INSERT INTO assignments (Id,CourseOfferingId,Title,Description,DueDate,MaxMarks,IsPublished,PublishedAt,CreatedAt,IsDeleted)
    VALUES (@Asgn2,@OfOOP,N'OOP Lab 2 — Inheritance',
            N'Extend BankAccount to SavingsAccount and CurrentAccount using inheritance.',
            DATEADD(DAY,7,@Now),20,1,@Now,@Now,0);
ELSE SELECT @Asgn2 = Id FROM assignments WHERE Title=N'OOP Lab 2 — Inheritance' AND CourseOfferingId=@OfOOP;

IF NOT EXISTS (SELECT 1 FROM assignments WHERE Title=N'DB Assignment 1 — ER Diagram' AND CourseOfferingId=@OfDB)
    INSERT INTO assignments (Id,CourseOfferingId,Title,Description,DueDate,MaxMarks,IsPublished,PublishedAt,CreatedAt,IsDeleted)
    VALUES (@Asgn3,@OfDB,N'DB Assignment 1 — ER Diagram',
            N'Design an ER diagram for a hospital management system.',
            DATEADD(DAY,-5,@Now),25,1,DATEADD(DAY,-20,@Now),@Now,0);
ELSE SELECT @Asgn3 = Id FROM assignments WHERE Title=N'DB Assignment 1 — ER Diagram' AND CourseOfferingId=@OfDB;

IF NOT EXISTS (SELECT 1 FROM assignments WHERE Title=N'SE Assignment 1 — SRS Document' AND CourseOfferingId=@OfSE1)
    INSERT INTO assignments (Id,CourseOfferingId,Title,Description,DueDate,MaxMarks,IsPublished,PublishedAt,CreatedAt,IsDeleted)
    VALUES (@Asgn4,@OfSE1,N'SE Assignment 1 — SRS Document',
            N'Write a complete Software Requirements Specification for a library management system.',
            DATEADD(DAY,14,@Now),30,1,@Now,@Now,0);
ELSE SELECT @Asgn4 = Id FROM assignments WHERE Title=N'SE Assignment 1 — SRS Document' AND CourseOfferingId=@OfSE1;

-- ═══════════════════════════════════════════════════════════
-- §19  ASSIGNMENT SUBMISSIONS
-- ═══════════════════════════════════════════════════════════
DECLARE @SubData TABLE (AsgnId UNIQUEIDENTIFIER, SpId UNIQUEIDENTIFIER,
                        TextC NVARCHAR(500), Marks DECIMAL(8,2), Feedback NVARCHAR(500));
INSERT INTO @SubData VALUES
    (@Asgn1,@SP1,N'Implemented BankAccount with proper encapsulation and validation.',   18,'Good work. Consider adding transaction history.'),
    (@Asgn1,@SP2,N'Full implementation with JUnit tests and edge-case handling.',        20,'Excellent! Thorough test coverage.'),
    (@Asgn1,@SP3,N'Basic implementation. Missing withdraw validation.',                   14,'Needs improvement. Review exception handling.'),
    (@Asgn1,@SP6,N'Implemented with extra features: overdraft protection.',               19,'Very good. Clean code style.'),
    (@Asgn3,@SP1,N'ER diagram covers all entities with correct cardinalities.',           22,'Well structured. Minor notation issues.'),
    (@Asgn3,@SP2,N'Comprehensive ER with weak entities and derived attributes.',          25,'Perfect marks. Excellent understanding.'),
    (@Asgn4,@SP4,N'SRS document follows IEEE 830 standard with 30 requirements.',        27,'Professional quality document.'),
    (@Asgn4,@SP5,N'Partial SRS. Missing non-functional requirements section.',            18,'See feedback in comments for revision guide.');

INSERT INTO assignment_submissions
    (Id,AssignmentId,StudentProfileId,TextContent,SubmittedAt,MarksAwarded,Feedback,GradedAt,GradedByUserId,Status,CreatedAt)
SELECT NEWID(),s.AsgnId,s.SpId,s.TextC,DATEADD(DAY,-2,@Now),s.Marks,s.Feedback,@Now,NULL,N'Graded',@Now
FROM @SubData s
WHERE NOT EXISTS (
    SELECT 1 FROM assignment_submissions x
    WHERE x.AssignmentId=s.AsgnId AND x.StudentProfileId=s.SpId
);

-- ═══════════════════════════════════════════════════════════
-- §20  ATTENDANCE RECORDS  (exercises all 3 views)
-- ═══════════════════════════════════════════════════════════
-- Generate attendance for 10 sessions per offering per enrolled student
-- Using a numbers table approach
DECLARE @AttBase DATETIME2 = DATEADD(DAY,-20,@Now);

-- SP1 (s.aslam) — OOP: 9/10 Present, DB: 10/10, DS: 8/10
INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP1,@OfOOP,DATEADD(DAY,v.n,@AttBase),
       CASE WHEN v.n=14 THEN N'Absent' ELSE N'Present' END, @UFDrA, @Now
FROM (VALUES(0),(2),(4),(6),(8),(10),(12),(14),(16),(18)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP1 AND CourseOfferingId=@OfOOP);

INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP1,@OfDB,DATEADD(DAY,v.n,@AttBase),N'Present',@UFDrS,@Now
FROM (VALUES(1),(3),(5),(7),(9),(11),(13),(15),(17),(19)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP1 AND CourseOfferingId=@OfDB);

-- SP2 (s.fatima) — OOP: 10/10, DB: 10/10, DS: 9/10
INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP2,@OfOOP,DATEADD(DAY,v.n,@AttBase),N'Present',@UFDrA,@Now
FROM (VALUES(0),(2),(4),(6),(8),(10),(12),(14),(16),(18)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP2 AND CourseOfferingId=@OfOOP);

INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP2,@OfDB,DATEADD(DAY,v.n,@AttBase),N'Present',@UFDrS,@Now
FROM (VALUES(1),(3),(5),(7),(9),(11),(13),(15),(17),(19)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP2 AND CourseOfferingId=@OfDB);

-- SP3 (s.usman) — OOP: 6/10 (low — will appear in sp_get_attendance_below_threshold)
INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP3,@OfOOP,DATEADD(DAY,v.n,@AttBase),
       CASE WHEN v.n IN (8,12,16,18) THEN N'Absent' ELSE N'Present' END, @UFDrA, @Now
FROM (VALUES(0),(2),(4),(6),(8),(10),(12),(14),(16),(18)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP3 AND CourseOfferingId=@OfOOP);

-- SP4 and SP5 — SE courses
INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP4,@OfSE1,DATEADD(DAY,v.n,@AttBase),N'Present',@UFMrA,@Now
FROM (VALUES(0),(2),(4),(6),(8),(10),(12),(14),(16),(18)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP4 AND CourseOfferingId=@OfSE1);

INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP5,@OfSE1,DATEADD(DAY,v.n,@AttBase),
       CASE WHEN v.n IN (4,8,12) THEN N'Absent' ELSE N'Present' END, @UFMrA, @Now
FROM (VALUES(0),(2),(4),(6),(8),(10),(12),(14),(16),(18)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP5 AND CourseOfferingId=@OfSE1);

-- SP7 and SP8 — IT courses
INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP7,@OfIT1,DATEADD(DAY,v.n,@AttBase),N'Present',@UFMsZ,@Now
FROM (VALUES(1),(3),(5),(7),(9),(11),(13),(15),(17),(19)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP7 AND CourseOfferingId=@OfIT1);

INSERT INTO attendance_records (Id,StudentProfileId,CourseOfferingId,Date,Status,MarkedByUserId,CreatedAt)
SELECT NEWID(),@SP8,@OfIT1,DATEADD(DAY,v.n,@AttBase),N'Present',@UFMsZ,@Now
FROM (VALUES(1),(3),(5),(7),(9),(11),(13),(15),(17),(19)) AS v(n)
WHERE NOT EXISTS (SELECT 1 FROM attendance_records WHERE StudentProfileId=@SP8 AND CourseOfferingId=@OfIT1);

-- ═══════════════════════════════════════════════════════════
-- §21  RESULTS  (exercises vw_student_results_summary)
-- ═══════════════════════════════════════════════════════════
DECLARE @ResData TABLE(SpId UNIQUEIDENTIFIER,OffId UNIQUEIDENTIFIER,
                       RT NVARCHAR(20),Marks DECIMAL(8,2),Max DECIMAL(8,2),PubBy UNIQUEIDENTIFIER);
INSERT INTO @ResData VALUES
    -- OOP midterms
    (@SP1,@OfOOP,N'Midterm',72,100,@UFDrA), (@SP2,@OfOOP,N'Midterm',88,100,@UFDrA),
    (@SP3,@OfOOP,N'Midterm',55,100,@UFDrA), (@SP6,@OfOOP,N'Midterm',78,100,@UFDrA),
    -- OOP finals
    (@SP1,@OfOOP,N'Final',  76,100,@UFDrA), (@SP2,@OfOOP,N'Final',  91,100,@UFDrA),
    (@SP3,@OfOOP,N'Final',  60,100,@UFDrA), (@SP6,@OfOOP,N'Final',  82,100,@UFDrA),
    -- DB midterms
    (@SP1,@OfDB, N'Midterm',80,100,@UFDrS), (@SP2,@OfDB, N'Midterm',92,100,@UFDrS),
    (@SP3,@OfDB, N'Midterm',68,100,@UFDrS),
    -- DB finals
    (@SP1,@OfDB, N'Final',  85,100,@UFDrS), (@SP2,@OfDB, N'Final',  95,100,@UFDrS),
    -- SE results
    (@SP4,@OfSE1,N'Midterm',88,100,@UFMrA), (@SP5,@OfSE1,N'Midterm',62,100,@UFMrA),
    (@SP4,@OfSE1,N'Final',  90,100,@UFMrA), (@SP5,@OfSE1,N'Final',  70,100,@UFMrA),
    -- IT results
    (@SP7,@OfIT1,N'Midterm',75,100,@UFMsZ), (@SP8,@OfIT1,N'Midterm',83,100,@UFMsZ),
    (@SP7,@OfIT1,N'Final',  78,100,@UFMsZ), (@SP8,@OfIT1,N'Final',  87,100,@UFMsZ);

INSERT INTO results (Id,StudentProfileId,CourseOfferingId,ResultType,
                     MarksObtained,MaxMarks,IsPublished,PublishedAt,PublishedByUserId,CreatedAt)
SELECT NEWID(),r.SpId,r.OffId,r.RT,r.Marks,r.Max,1,@Now,r.PubBy,@Now
FROM @ResData r
WHERE NOT EXISTS (
    SELECT 1 FROM results x
    WHERE x.StudentProfileId=r.SpId AND x.CourseOfferingId=r.OffId AND x.ResultType=r.RT
);

-- ═══════════════════════════════════════════════════════════
-- §22  QUIZZES + QUESTIONS + OPTIONS + ATTEMPTS
-- ═══════════════════════════════════════════════════════════
DECLARE @Q1 UNIQUEIDENTIFIER = NEWID(); DECLARE @QQ1 UNIQUEIDENTIFIER = NEWID();
DECLARE @QQ2 UNIQUEIDENTIFIER = NEWID(); DECLARE @QO1 UNIQUEIDENTIFIER = NEWID();
DECLARE @QO2 UNIQUEIDENTIFIER = NEWID(); DECLARE @QO3 UNIQUEIDENTIFIER = NEWID();
DECLARE @QO4 UNIQUEIDENTIFIER = NEWID(); DECLARE @QO5 UNIQUEIDENTIFIER = NEWID();
DECLARE @QO6 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM quizzes WHERE Title=N'OOP Quiz 1 — Fundamentals' AND CourseOfferingId=@OfOOP)
BEGIN
    INSERT INTO quizzes (Id,CourseOfferingId,Title,Instructions,TimeLimitMinutes,MaxAttempts,
                         AvailableFrom,AvailableUntil,IsPublished,IsActive,CreatedByUserId,CreatedAt)
    VALUES (@Q1,@OfOOP,N'OOP Quiz 1 — Fundamentals',N'Read each question carefully. Closed notes.',
            20,1,DATEADD(DAY,-5,@Now),DATEADD(DAY,5,@Now),1,1,@UFDrA,@Now);

    INSERT INTO quiz_questions (Id,QuizId,[Type],[Text],Marks,OrderIndex,CreatedAt)
    VALUES
        (@QQ1,@Q1,N'MultipleChoice',N'Which principle hides object internals from the outside world?',1,1,@Now),
        (@QQ2,@Q1,N'TrueFalse',N'A child class cannot override a parent class method in Java.',1,2,@Now);

    SET @QO1=NEWID(); SET @QO2=NEWID(); SET @QO3=NEWID(); SET @QO4=NEWID();
    INSERT INTO quiz_options (Id,QuizQuestionId,[Text],IsCorrect,OrderIndex,CreatedAt)
    VALUES
        (@QO1,@QQ1,N'Encapsulation',1,1,@Now),(@QO2,@QQ1,N'Polymorphism',0,2,@Now),
        (@QO3,@QQ1,N'Inheritance', 0,3,@Now),(@QO4,@QQ1,N'Abstraction', 0,4,@Now);

    SET @QO5=NEWID(); SET @QO6=NEWID();
    INSERT INTO quiz_options (Id,QuizQuestionId,[Text],IsCorrect,OrderIndex,CreatedAt)
    VALUES (@QO5,@QQ2,N'True',0,1,@Now),(@QO6,@QQ2,N'False',1,2,@Now);
END
ELSE
BEGIN
    SELECT @Q1  = Id FROM quizzes WHERE Title=N'OOP Quiz 1 — Fundamentals' AND CourseOfferingId=@OfOOP;
    SELECT TOP 1 @QQ1 = Id FROM quiz_questions WHERE QuizId=@Q1 AND OrderIndex=1;
    SELECT TOP 1 @QQ2 = Id FROM quiz_questions WHERE QuizId=@Q1 AND OrderIndex=2;
    SELECT @QO1  = Id FROM quiz_options WHERE QuizQuestionId=@QQ1 AND IsCorrect=1;
END

-- Quiz attempt by SP1 (all correct)
IF NOT EXISTS (SELECT 1 FROM quiz_attempts WHERE QuizId=@Q1 AND StudentProfileId=@SP1)
BEGIN
    DECLARE @Att1 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO quiz_attempts (Id,QuizId,StudentProfileId,StartedAt,FinishedAt,
                                TotalScore,Status,CreatedAt)
    VALUES (@Att1,@Q1,@SP1,DATEADD(MINUTE,-25,@Now),DATEADD(MINUTE,-5,@Now),2,N'Submitted',@Now);

    INSERT INTO quiz_answers (Id,QuizAttemptId,QuizQuestionId,SelectedOptionId,MarksAwarded,CreatedAt)
    VALUES
        (NEWID(),@Att1,@QQ1,@QO1,1,@Now),
        (NEWID(),@Att1,@QQ2,@QO6,1,@Now);
END

-- Quiz attempt by SP3 (scored 1/2)
IF NOT EXISTS (SELECT 1 FROM quiz_attempts WHERE QuizId=@Q1 AND StudentProfileId=@SP3)
BEGIN
    DECLARE @Att3 UNIQUEIDENTIFIER = NEWID();
    INSERT INTO quiz_attempts (Id,QuizId,StudentProfileId,StartedAt,FinishedAt,
                                TotalScore,Status,CreatedAt)
    VALUES (@Att3,@Q1,@SP3,DATEADD(MINUTE,-20,@Now),DATEADD(MINUTE,-3,@Now),1,N'Submitted',@Now);

    INSERT INTO quiz_answers (Id,QuizAttemptId,QuizQuestionId,SelectedOptionId,MarksAwarded,CreatedAt)
    VALUES
        (NEWID(),@Att3,@QQ1,@QO1,1,@Now),  -- correct
        (NEWID(),@Att3,@QQ2,@QO5,0,@Now);  -- wrong
END

-- ═══════════════════════════════════════════════════════════
-- §23  FYP PROJECTS + PANEL MEMBERS + MEETINGS
-- ═══════════════════════════════════════════════════════════
DECLARE @FYP1 UNIQUEIDENTIFIER = NEWID();
DECLARE @FYP2 UNIQUEIDENTIFIER = NEWID();
DECLARE @FYP3 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM fyp_projects WHERE StudentProfileId=@SP2)
BEGIN
    INSERT INTO fyp_projects (Id,StudentProfileId,DepartmentId,Title,Description,
                               Status,SupervisorUserId,CreatedAt)
    VALUES (@FYP1,@SP2,@DeptCS,
            N'AI-based Plagiarism Detection for Academic Submissions',
            N'Uses NLP and machine learning to identify plagiarised text in assignment submissions.',
            N'InProgress',@UFDrS,@Now);

        INSERT INTO fyp_panel_members (Id,FypProjectId,UserId,[Role],CreatedAt)
        VALUES (NEWID(),@FYP1,@UFDrS,  N'Supervisor',  @Now),
            (NEWID(),@FYP1,@UFDrA,  N'CoSupervisor',@Now),
            (NEWID(),@FYP1,@UFProfK,N'Examiner',    @Now);

        INSERT INTO fyp_meetings (Id,FypProjectId,ScheduledAt,Venue,Agenda,Status,OrganiserUserId,Minutes,CreatedAt)
        VALUES (NEWID(),@FYP1,DATEADD(DAY,-14,@Now),N'CS Dept. Room 301',
            N'Project scope finalisation',N'Completed',@UFDrS,
            N'Project scope finalised. Dataset collection plan approved.',@Now),
           (NEWID(),@FYP1,DATEADD(DAY,-7,@Now),N'CS Dept. Room 301',
            N'Prototype progress review',N'Completed',@UFDrS,
            N'Initial prototype demo. Accuracy at 72%. Target 85%.',@Now);
END

IF NOT EXISTS (SELECT 1 FROM fyp_projects WHERE StudentProfileId=@SP1)
    INSERT INTO fyp_projects (Id,StudentProfileId,DepartmentId,Title,Description,
                               Status,SupervisorUserId,CreatedAt)
    VALUES (@FYP2,@SP1,@DeptCS,
            N'Smart Campus Navigation using IoT Sensors',
            N'Real-time indoor navigation system using BLE beacons.',
            N'Proposed',@UFProfK,@Now);

IF NOT EXISTS (SELECT 1 FROM fyp_projects WHERE StudentProfileId=@SP4)
    INSERT INTO fyp_projects (Id,StudentProfileId,DepartmentId,Title,Description,
                               Status,SupervisorUserId,CreatedAt)
    VALUES (@FYP3,@SP4,@DeptSE,
            N'Automated Test Case Generation using LLMs',
            N'Leverages GPT-style models to auto-generate unit tests from source code.',
            N'Approved',@UFMrA,@Now);

-- ═══════════════════════════════════════════════════════════
-- §24  NOTIFICATIONS  (various types for all roles)
-- ═══════════════════════════════════════════════════════════
DECLARE @N1 UNIQUEIDENTIFIER = NEWID(); DECLARE @N2 UNIQUEIDENTIFIER = NEWID();
DECLARE @N3 UNIQUEIDENTIFIER = NEWID(); DECLARE @N4 UNIQUEIDENTIFIER = NEWID();
DECLARE @N5 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM notifications WHERE Title=N'Welcome to EduSphere!')
BEGIN
    INSERT INTO notifications (Id,Title,Body,[Type],SenderUserId,IsSystemGenerated,IsActive,CreatedAt)
    VALUES
    (@N1,N'Welcome to EduSphere!',
         N'Your account has been created. Explore your dashboard and modules.',
         N'General',@UserSA,1,1,@Now),
    (@N2,N'Spring 2026 Registration Open',
         N'Enrollment for Spring 2026 semester is now open. Register by Jan 10.',
         N'Announcement',@UAdmCS,0,1,DATEADD(DAY,-30,@Now)),
    (@N3,N'OOP Lab 1 Results Published',
         N'Marks for OOP Lab 1 — Classes and Objects have been released.',
         N'Result',@UFDrA,0,1,DATEADD(DAY,-2,@Now)),
    (@N4,N'Attendance Alert: CS-301 (OOP)',
         N'Student s.usman attendance has fallen below 75% in OOP.',
         N'AttendanceAlert',NULL,1,1,@Now),
    (@N5,N'FYP Supervisor Assigned',
         N'Your FYP supervisor has been assigned. Check your FYP portal.',
         N'System',NULL,1,1,@Now);
END
ELSE
BEGIN
    SELECT @N1=Id FROM notifications WHERE Title=N'Welcome to EduSphere!';
    SELECT @N2=Id FROM notifications WHERE Title=N'Spring 2026 Registration Open';
    SELECT @N3=Id FROM notifications WHERE Title=N'OOP Lab 1 Results Published';
    SELECT @N4=Id FROM notifications WHERE Title=N'Attendance Alert: CS-301 (OOP)';
    SELECT @N5=Id FROM notifications WHERE Title=N'FYP Supervisor Assigned';
END

-- Notification recipients
DECLARE @NRData TABLE (NId UNIQUEIDENTIFIER, UId UNIQUEIDENTIFIER);
INSERT INTO @NRData VALUES
    (@N1,@UAdmCS),(@N1,@UAdmSE),(@N1,@UAdmIT),
    (@N1,@UFDrA),(@N1,@UFDrS),(@N1,@UFMrA),(@N1,@UFMsZ),(@N1,@UFProfK),
    (@N1,@US1),(@N1,@US2),(@N1,@US3),(@N1,@US4),(@N1,@US5),
    (@N1,@US6),(@N1,@US7),(@N1,@US8),(@N1,@US9),(@N1,@US10),
    (@N2,@US1),(@N2,@US2),(@N2,@US3),(@N2,@US4),(@N2,@US5),
    (@N3,@US1),(@N3,@US2),(@N3,@US3),(@N3,@US6),
    (@N4,@UAdmCS),(@N4,@UFDrA),
    (@N5,@US1),(@N5,@US2),(@N5,@US4);

INSERT INTO notification_recipients (Id,NotificationId,RecipientUserId,IsRead,CreatedAt)
SELECT NEWID(),nr.NId,nr.UId,0,@Now FROM @NRData nr
WHERE NOT EXISTS (
    SELECT 1 FROM notification_recipients x WHERE x.NotificationId=nr.NId AND x.RecipientUserId=nr.UId
);

-- Mark some notifications as read
UPDATE notification_recipients SET IsRead=1, ReadAt=@Now
WHERE RecipientUserId=@US2 AND NotificationId IN (@N1,@N2);

-- ═══════════════════════════════════════════════════════════
-- §25  PAYMENT RECEIPTS
-- ═══════════════════════════════════════════════════════════
DECLARE @PRData TABLE(SpId UNIQUEIDENTIFIER, Amt DECIMAL(10,2), [Description] NVARCHAR(200),
                      Status INT, Due DATETIME2);
INSERT INTO @PRData VALUES
    (@SP1, 45000, N'Spring 2026 Semester Fee', 2, DATEADD(DAY,-20,@Now)),
    (@SP2, 45000, N'Spring 2026 Semester Fee', 2, DATEADD(DAY,-20,@Now)),
    (@SP3, 45000, N'Spring 2026 Semester Fee', 0, DATEADD(DAY,10,@Now)),
    (@SP4, 42000, N'Spring 2026 Semester Fee', 2, DATEADD(DAY,-15,@Now)),
    (@SP5, 42000, N'Spring 2026 Semester Fee', 1, DATEADD(DAY,5,@Now)),
    (@SP7, 38000, N'Spring 2026 Semester Fee', 0, DATEADD(DAY,15,@Now)),
    (@SP8, 38000, N'Spring 2026 Semester Fee', 2, DATEADD(DAY,-10,@Now));

INSERT INTO payment_receipts (Id,StudentProfileId,CreatedByUserId,Status,Amount,Description,DueDate,CreatedAt,UpdatedAt,IsDeleted)
SELECT NEWID(),p.SpId,@UAdmCS,p.Status,p.Amt,p.[Description],p.Due,@Now,@Now,0
FROM @PRData p
WHERE NOT EXISTS (
    SELECT 1 FROM payment_receipts x
    WHERE x.StudentProfileId=p.SpId AND x.Description=p.[Description]
);

-- ═══════════════════════════════════════════════════════════
-- §26  ADMIN CHANGE REQUESTS
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM admin_change_requests WHERE RequestorUserId=@UAdmCS
               AND ChangeDescription=N'Semester registration deadline extension')
    INSERT INTO admin_change_requests
        (Id,RequestorUserId,ReviewedByUserId,Status,ChangeDescription,Reason,
         NewData,AdminNotes,ReviewedAt,CreatedAt,UpdatedAt,IsDeleted)
    VALUES
        (NEWID(),@UAdmCS,@UserSA,1,
         N'Semester registration deadline extension',
         N'Many students could not register due to the fee payment system outage.',
         N'{"ExtendedDeadline":"2026-01-20"}',
         N'Approved. System outage confirmed. Students notified.',
         DATEADD(DAY,-2,@Now),DATEADD(DAY,-5,@Now),DATEADD(DAY,-5,@Now),0);

IF NOT EXISTS (SELECT 1 FROM admin_change_requests WHERE RequestorUserId=@UAdmCS
               AND ChangeDescription=N'Update student registration number correction')
    INSERT INTO admin_change_requests
        (Id,RequestorUserId,ReviewedByUserId,Status,ChangeDescription,Reason,
         NewData,AdminNotes,ReviewedAt,CreatedAt,UpdatedAt,IsDeleted)
    VALUES
        (NEWID(),@UAdmCS,NULL,0,
         N'Update student registration number correction',
         N'Typographical error in registration number for student 2024-CS-0003.',
         N'{"OldRegNo":"2024-CS-0003","NewRegNo":"2024-CS-0003-A"}',
         NULL, NULL, @Now, @Now, 0);

-- ═══════════════════════════════════════════════════════════
-- §27  TEACHER MODIFICATION REQUESTS
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM teacher_modification_requests WHERE TeacherUserId=@UFDrA
               AND Reason LIKE N'Student was present%')
    INSERT INTO teacher_modification_requests
        (Id,TeacherUserId,ModificationType,RecordId,Reason,Status,
         ReviewedByUserId,ReviewedAt,AdminNotes,ProposedData,CreatedAt,UpdatedAt,IsDeleted)
    VALUES
        (NEWID(),@UFDrA,0,
         (SELECT TOP 1 Id FROM attendance_records
          WHERE StudentProfileId=@SP3 AND Status=N'Absent' AND CourseOfferingId=@OfOOP),
         N'Student was present but marked absent due to register error.',
         1,@UAdmCS,@Now,N'Verified with CCTV footage. Corrected.',N'{"Status":"Present"}',@Now,@Now,0);

-- ═══════════════════════════════════════════════════════════
-- §28  AI CHAT CONVERSATIONS + MESSAGES
-- ═══════════════════════════════════════════════════════════
DECLARE @Conv1 UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM chat_conversations WHERE UserId=@US1)
BEGIN
    INSERT INTO chat_conversations (Id,UserId,UserRole,DepartmentId,StartedAt)
    VALUES (@Conv1,@US1,N'Student',@DeptCS,@Now);

    INSERT INTO chat_messages (Id,ConversationId,[Role],Content,SentAt,TokensUsed)
    VALUES
        (NEWID(),@Conv1,N'user',
         N'Can you explain the difference between an interface and an abstract class?',
         DATEADD(MINUTE,-10,@Now),15),
        (NEWID(),@Conv1,N'assistant',
         N'An interface defines a contract (all methods are abstract by default), while an abstract class can have both abstract and concrete methods. Use interfaces for unrelated classes sharing behaviour; use abstract classes for closely related classes sharing implementation.',
         DATEADD(MINUTE,-9,@Now),82),
        (NEWID(),@Conv1,N'user',
         N'Thanks! Can you give me a Java code example?',
         DATEADD(MINUTE,-8,@Now),12),
        (NEWID(),@Conv1,N'assistant',
         N'Sure! Here''s a quick example: interface Drawable { void draw(); } abstract class Shape { abstract double area(); void describe() { System.out.println("Shape"); } } class Circle extends Shape implements Drawable { public void draw() {...} public double area() {...} }',
         DATEADD(MINUTE,-7,@Now),75);
END

-- ═══════════════════════════════════════════════════════════
-- §29  OUTBOUND EMAIL LOGS  (sample delivery records)
-- ═══════════════════════════════════════════════════════════
IF NOT EXISTS (SELECT 1 FROM outbound_email_logs WHERE ToAddress=N'aslam@student.tabsan.local')
    INSERT INTO outbound_email_logs (Id,ToAddress,Subject,Status,ErrorMessage,AttemptedAt)
    VALUES
        (NEWID(),N'aslam@student.tabsan.local',  N'OOP Lab 1 Marks Released',N'Sent',NULL,     DATEADD(DAY,-2,@Now)),
        (NEWID(),N'fatima@student.tabsan.local', N'OOP Lab 1 Marks Released',N'Sent',NULL,     DATEADD(DAY,-2,@Now)),
        (NEWID(),N'usman@student.tabsan.local',  N'Attendance Alert',        N'Sent',NULL,     DATEADD(HOUR,-3,@Now)),
        (NEWID(),N'invalid@broken.local',        N'Welcome Email',           N'Failed',
         N'SMTP error 550: Mailbox not found',@Now);

COMMIT TRANSACTION;

-- ═══════════════════════════════════════════════════════════
-- §30  VERIFY — counts, views, stored procedures
-- ═══════════════════════════════════════════════════════════
PRINT '─────────────────────────────────────────────────';
PRINT ' Seeded row counts';
PRINT '─────────────────────────────────────────────────';
SELECT 'roles'                      AS [Table], COUNT(*) AS Rows FROM roles
UNION ALL SELECT 'modules',              COUNT(*) FROM modules
UNION ALL SELECT 'module_status',        COUNT(*) FROM module_status
UNION ALL SELECT 'users',                COUNT(*) FROM users          WHERE IsDeleted=0
UNION ALL SELECT 'departments',          COUNT(*) FROM departments    WHERE IsDeleted=0
UNION ALL SELECT 'academic_programs',    COUNT(*) FROM academic_programs WHERE IsDeleted=0
UNION ALL SELECT 'semesters',            COUNT(*) FROM semesters      WHERE IsDeleted=0
UNION ALL SELECT 'courses',              COUNT(*) FROM courses        WHERE IsDeleted=0
UNION ALL SELECT 'course_offerings',     COUNT(*) FROM course_offerings WHERE IsDeleted=0
UNION ALL SELECT 'student_profiles',     COUNT(*) FROM student_profiles WHERE IsDeleted=0
UNION ALL SELECT 'enrollments',          COUNT(*) FROM enrollments
UNION ALL SELECT 'attendance_records',   COUNT(*) FROM attendance_records
UNION ALL SELECT 'results',              COUNT(*) FROM results
UNION ALL SELECT 'assignments',          COUNT(*) FROM assignments    WHERE IsDeleted=0
UNION ALL SELECT 'assignment_submissions',COUNT(*) FROM assignment_submissions
UNION ALL SELECT 'quizzes',              COUNT(*) FROM quizzes
UNION ALL SELECT 'quiz_questions',       COUNT(*) FROM quiz_questions
UNION ALL SELECT 'quiz_options',         COUNT(*) FROM quiz_options
UNION ALL SELECT 'quiz_attempts',        COUNT(*) FROM quiz_attempts
UNION ALL SELECT 'quiz_answers',         COUNT(*) FROM quiz_answers
UNION ALL SELECT 'fyp_projects',         COUNT(*) FROM fyp_projects
UNION ALL SELECT 'fyp_panel_members',    COUNT(*) FROM fyp_panel_members
UNION ALL SELECT 'fyp_meetings',         COUNT(*) FROM fyp_meetings
UNION ALL SELECT 'notifications',        COUNT(*) FROM notifications
UNION ALL SELECT 'notification_recipients',COUNT(*) FROM notification_recipients
UNION ALL SELECT 'payment_receipts',     COUNT(*) FROM payment_receipts  WHERE IsDeleted=0
UNION ALL SELECT 'admin_change_requests',COUNT(*) FROM admin_change_requests WHERE IsDeleted=0
UNION ALL SELECT 'teacher_mod_requests', COUNT(*) FROM teacher_modification_requests WHERE IsDeleted=0
UNION ALL SELECT 'chat_conversations',   COUNT(*) FROM chat_conversations
UNION ALL SELECT 'chat_messages',        COUNT(*) FROM chat_messages
UNION ALL SELECT 'outbound_email_logs',  COUNT(*) FROM outbound_email_logs
UNION ALL SELECT 'timetables',           COUNT(*) FROM timetables     WHERE IsDeleted=0
UNION ALL SELECT 'timetable_entries',    COUNT(*) FROM timetable_entries
UNION ALL SELECT 'sidebar_menu_items',   COUNT(*) FROM sidebar_menu_items WHERE IsDeleted=0
UNION ALL SELECT 'module_role_assignments',COUNT(*) FROM module_role_assignments
UNION ALL SELECT 'report_definitions',   COUNT(*) FROM report_definitions WHERE IsDeleted=0
ORDER BY [Table];

PRINT '';
PRINT '─────────────────────────────────────────────────';
PRINT ' vw_student_attendance_summary';
PRINT '─────────────────────────────────────────────────';
SELECT sp.RegistrationNumber,
       c.Code AS CourseCode,
       v.TotalSessions,
       v.AttendedSessions,
       v.AttendancePercentage
FROM vw_student_attendance_summary v
JOIN student_profiles sp ON sp.Id = v.StudentProfileId
JOIN course_offerings co ON co.Id = v.CourseOfferingId
JOIN courses c ON c.Id = co.CourseId
ORDER BY sp.RegistrationNumber, c.Code;

PRINT '';
PRINT '─────────────────────────────────────────────────';
PRINT ' vw_student_results_summary';
PRINT '─────────────────────────────────────────────────';
SELECT sp.RegistrationNumber,
       v.CourseCode,
       v.CourseTitle,
       v.ResultType,
       v.MarksObtained,
       v.MaxMarks,
       v.Percentage
FROM vw_student_results_summary v
JOIN student_profiles sp ON sp.Id = v.StudentProfileId
ORDER BY sp.RegistrationNumber, v.CourseCode, v.ResultType;

PRINT '';
PRINT '─────────────────────────────────────────────────';
PRINT ' vw_course_enrollment_summary';
PRINT '─────────────────────────────────────────────────';
SELECT v.CourseCode, v.CourseTitle,
       v.MaxEnrollment, v.EnrolledCount, v.AvailableSeats
FROM vw_course_enrollment_summary v
ORDER BY v.CourseCode;

PRINT '';
PRINT '─────────────────────────────────────────────────';
PRINT ' sp_get_attendance_below_threshold (75%)';
PRINT ' Expected: s.usman (OOP ~60%) and sp5 (SE ~70%)';
PRINT '─────────────────────────────────────────────────';
EXEC sp_get_attendance_below_threshold @ThresholdPercent = 75.0;

PRINT '';
PRINT '─────────────────────────────────────────────────';
PRINT ' sp_recalculate_student_cgpa — all students';
PRINT '─────────────────────────────────────────────────';
DECLARE @CgpaUpdate CURSOR;
SET @CgpaUpdate = CURSOR FOR SELECT Id FROM student_profiles;
OPEN @CgpaUpdate;
DECLARE @SpId UNIQUEIDENTIFIER;
FETCH NEXT FROM @CgpaUpdate INTO @SpId;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC sp_recalculate_student_cgpa @StudentProfileId = @SpId;
    FETCH NEXT FROM @CgpaUpdate INTO @SpId;
END
CLOSE @CgpaUpdate;
DEALLOCATE @CgpaUpdate;

SELECT sp.RegistrationNumber, sp.Cgpa AS UpdatedCgpa
FROM student_profiles sp
ORDER BY sp.RegistrationNumber;

PRINT '';
PRINT '✓ Script 2 complete.';
PRINT '  Reset passwords via Admin UI or run Scripts\GenerateTestHashes.ps1.';
