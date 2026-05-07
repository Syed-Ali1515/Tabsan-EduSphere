-- ============================================================
-- Tabsan EduSphere  —  Full Dummy Data  (Script 2 of 2)
-- ============================================================
-- PURPOSE : Populate the database with rich, realistic test data
--           across all three departments so every portal role
--           and feature can be exercised thoroughly.
--
-- STRUCTURE:
--   Departments  : IT | Business | Languages
--   Programs     : BSCS, BSIT (IT) | BBA, MBA (Business) |
--                  BA-Arabic, BA-English, BA-Chinese (Languages)
--   Semesters    : Fall 2025 (closed) | Spring 2026 (active) |
--                  Fall 2026 (future/upcoming)
--   Roles        : 1 SuperAdmin | 3 Admins (1 per dept) |
--                  9 Faculty | 18 Students (6 per dept)
--   Features     : enrollments, assignments, submissions,
--                  results, attendance, quizzes, FYP projects,
--                  notifications, payment receipts, timetables,
--                  buildings, rooms.
--
-- PASSWORD: Replace PLACEHOLDER before running.
--   Run: .\Scripts\GenerateTestHashes.ps1
--
-- SAFE   : Fully idempotent — safe to re-run.
-- DEPENDS: 0-Schema.sql, 1-MinimalSeed.sql applied first.
-- ============================================================

USE TabsanEduSphere;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

DECLARE @PwdHash NVARCHAR(512) = N'PLACEHOLDER_RUN_GenerateTestHashes.ps1_TO_REPLACE';
DECLARE @Now     DATETIME2     = SYSUTCDATETIME();

-- ═════════════════════════════════════════════════════════════
-- A.  DEPARTMENTS
-- ═════════════════════════════════════════════════════════════
DECLARE @DeptIT   UNIQUEIDENTIFIER = CAST(N'DA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @DeptBiz  UNIQUEIDENTIFIER = CAST(N'DA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @DeptLang UNIQUEIDENTIFIER = CAST(N'DA000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);

INSERT INTO [departments] ([Id],[Name],[Code],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Name], v.[Code], 1, @Now, @Now, 0
FROM (VALUES
    (@DeptIT,   N'Information Technology', N'IT'  ),
    (@DeptBiz,  N'Business',               N'BUS' ),
    (@DeptLang, N'Languages',              N'LANG')
) AS v([Id],[Name],[Code])
WHERE NOT EXISTS (SELECT 1 FROM [departments] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- B.  ACADEMIC PROGRAMS
-- ═════════════════════════════════════════════════════════════
DECLARE @ProgBSCS   UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @ProgBSIT   UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @ProgBBA    UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @ProgMBA    UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER);
DECLARE @ProgArabic UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000005' AS UNIQUEIDENTIFIER);
DECLARE @ProgEng    UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER);
DECLARE @ProgChinese UNIQUEIDENTIFIER = CAST(N'PA000000-0000-0000-0000-000000000007' AS UNIQUEIDENTIFIER);

INSERT INTO [academic_programs] ([Id],[Name],[Code],[DepartmentId],[TotalSemesters],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Name], v.[Code], v.[DeptId], v.[Sems], 1, @Now, @Now, 0
FROM (VALUES
    (@ProgBSCS,   N'BS Computer Science',   N'BSCS',    @DeptIT,   8),
    (@ProgBSIT,   N'BS Information Technology', N'BSIT',@DeptIT,   8),
    (@ProgBBA,    N'Bachelor of Business Administration', N'BBA',  @DeptBiz,  8),
    (@ProgMBA,    N'Master of Business Administration',   N'MBA',  @DeptBiz,  4),
    (@ProgArabic, N'BA Arabic',             N'BA-ARB',  @DeptLang, 6),
    (@ProgEng,    N'BA English',            N'BA-ENG',  @DeptLang, 6),
    (@ProgChinese,N'BA Chinese',            N'BA-CHN',  @DeptLang, 6)
) AS v([Id],[Name],[Code],[DeptId],[Sems])
WHERE NOT EXISTS (SELECT 1 FROM [academic_programs] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- C.  SEMESTERS
-- ═════════════════════════════════════════════════════════════
DECLARE @SemFall25 UNIQUEIDENTIFIER = CAST(N'SA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @SemSpr26  UNIQUEIDENTIFIER = CAST(N'SA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @SemFall26 UNIQUEIDENTIFIER = CAST(N'SA000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);

INSERT INTO [semesters] ([Id],[Name],[StartDate],[EndDate],[IsClosed],[ClosedAt],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Name], v.[Start], v.[End], v.[Closed], v.[ClosedAt], @Now, @Now, 0
FROM (VALUES
    (@SemFall25, N'Fall 2025',   '2025-09-01', '2026-01-31', CAST(1 AS BIT), CAST('2026-02-01 00:00:00' AS DATETIME2)),
    (@SemSpr26,  N'Spring 2026', '2026-02-01', '2026-06-30', CAST(0 AS BIT), CAST(NULL AS DATETIME2)),
    (@SemFall26, N'Fall 2026',   '2026-09-01', '2027-01-31', CAST(0 AS BIT), CAST(NULL AS DATETIME2))
) AS v([Id],[Name],[Start],[End],[Closed],[ClosedAt])
WHERE NOT EXISTS (SELECT 1 FROM [semesters] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- D.  USERS
-- ═════════════════════════════════════════════════════════════
-- SuperAdmin
DECLARE @USuperAdmin UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
-- Admins (one per dept)
DECLARE @UAdminIT    UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @UAdminBiz   UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @UAdminLang  UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER);
-- Faculty — IT (3), Business (3), Languages (3)
DECLARE @UFacAhmed   UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000011' AS UNIQUEIDENTIFIER); -- IT CS
DECLARE @UFacSara    UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000012' AS UNIQUEIDENTIFIER); -- IT IT
DECLARE @UFacAli     UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000013' AS UNIQUEIDENTIFIER); -- IT shared
DECLARE @UFacKhan    UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000014' AS UNIQUEIDENTIFIER); -- Business
DECLARE @UFacNaeem   UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000015' AS UNIQUEIDENTIFIER); -- Business
DECLARE @UFacZara    UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000016' AS UNIQUEIDENTIFIER); -- Business
DECLARE @UFacFatima  UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000017' AS UNIQUEIDENTIFIER); -- Languages
DECLARE @UFacImran   UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000018' AS UNIQUEIDENTIFIER); -- Languages
DECLARE @UFacSana    UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000019' AS UNIQUEIDENTIFIER); -- Languages
-- Students — IT (6), Business (6), Languages (6)
DECLARE @UStud01 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER); -- IT
DECLARE @UStud02 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000102' AS UNIQUEIDENTIFIER);
DECLARE @UStud03 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000103' AS UNIQUEIDENTIFIER);
DECLARE @UStud04 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000104' AS UNIQUEIDENTIFIER);
DECLARE @UStud05 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000105' AS UNIQUEIDENTIFIER);
DECLARE @UStud06 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000106' AS UNIQUEIDENTIFIER);
DECLARE @UStud07 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000107' AS UNIQUEIDENTIFIER); -- Business
DECLARE @UStud08 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000108' AS UNIQUEIDENTIFIER);
DECLARE @UStud09 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000109' AS UNIQUEIDENTIFIER);
DECLARE @UStud10 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000110' AS UNIQUEIDENTIFIER);
DECLARE @UStud11 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000111' AS UNIQUEIDENTIFIER);
DECLARE @UStud12 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000112' AS UNIQUEIDENTIFIER);
DECLARE @UStud13 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000113' AS UNIQUEIDENTIFIER); -- Languages
DECLARE @UStud14 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000114' AS UNIQUEIDENTIFIER);
DECLARE @UStud15 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000115' AS UNIQUEIDENTIFIER);
DECLARE @UStud16 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000116' AS UNIQUEIDENTIFIER);
DECLARE @UStud17 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000117' AS UNIQUEIDENTIFIER);
DECLARE @UStud18 UNIQUEIDENTIFIER = CAST(N'UA000000-0000-0000-0000-000000000118' AS UNIQUEIDENTIFIER);

INSERT INTO [users]
    ([Id],[Username],[Email],[PasswordHash],[RoleId],[DepartmentId],
     [IsActive],[MustChangePassword],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Username], v.[Email], @PwdHash, v.[RoleId], v.[DeptId], 1, 0, @Now, @Now, 0
FROM (VALUES
    -- SuperAdmin
    (@USuperAdmin, N'superadmin',     N'superadmin@edusphere.local',           1, CAST(NULL AS UNIQUEIDENTIFIER)),
    -- Admins
    (@UAdminIT,    N'admin.it',       N'admin.it@edusphere.local',             2, @DeptIT  ),
    (@UAdminBiz,   N'admin.biz',      N'admin.biz@edusphere.local',            2, @DeptBiz ),
    (@UAdminLang,  N'admin.lang',     N'admin.lang@edusphere.local',           2, @DeptLang),
    -- Faculty IT
    (@UFacAhmed,   N'dr.ahmed',       N'ahmed@edusphere.local',                3, @DeptIT  ),
    (@UFacSara,    N'dr.sara',        N'sara@edusphere.local',                 3, @DeptIT  ),
    (@UFacAli,     N'mr.ali',         N'ali@edusphere.local',                  3, @DeptIT  ),
    -- Faculty Business
    (@UFacKhan,    N'prof.khan',      N'khan@edusphere.local',                 3, @DeptBiz ),
    (@UFacNaeem,   N'dr.naeem',       N'naeem@edusphere.local',                3, @DeptBiz ),
    (@UFacZara,    N'ms.zara',        N'zara@edusphere.local',                 3, @DeptBiz ),
    -- Faculty Languages
    (@UFacFatima,  N'dr.fatima',      N'fatima@edusphere.local',               3, @DeptLang),
    (@UFacImran,   N'mr.imran',       N'imran@edusphere.local',                3, @DeptLang),
    (@UFacSana,    N'ms.sana',        N'sana@edusphere.local',                 3, @DeptLang),
    -- Students IT
    (@UStud01,     N's.aslam',        N'aslam@student.edusphere.local',        4, @DeptIT  ),
    (@UStud02,     N's.bilal',        N'bilal@student.edusphere.local',        4, @DeptIT  ),
    (@UStud03,     N's.fatima',       N'fatima.s@student.edusphere.local',     4, @DeptIT  ),
    (@UStud04,     N's.hina',         N'hina@student.edusphere.local',         4, @DeptIT  ),
    (@UStud05,     N's.kamran',       N'kamran@student.edusphere.local',       4, @DeptIT  ),
    (@UStud06,     N's.layla',        N'layla@student.edusphere.local',        4, @DeptIT  ),
    -- Students Business
    (@UStud07,     N's.nadia',        N'nadia@student.edusphere.local',        4, @DeptBiz ),
    (@UStud08,     N's.omar',         N'omar@student.edusphere.local',         4, @DeptBiz ),
    (@UStud09,     N's.rabia',        N'rabia@student.edusphere.local',        4, @DeptBiz ),
    (@UStud10,     N's.saad',         N'saad@student.edusphere.local',         4, @DeptBiz ),
    (@UStud11,     N's.shazia',       N'shazia@student.edusphere.local',       4, @DeptBiz ),
    (@UStud12,     N's.tariq',        N'tariq@student.edusphere.local',        4, @DeptBiz ),
    -- Students Languages
    (@UStud13,     N's.usman',        N'usman@student.edusphere.local',        4, @DeptLang),
    (@UStud14,     N's.yasmin',       N'yasmin@student.edusphere.local',       4, @DeptLang),
    (@UStud15,     N's.zainab',       N'zainab@student.edusphere.local',       4, @DeptLang),
    (@UStud16,     N's.danish',       N'danish@student.edusphere.local',       4, @DeptLang),
    (@UStud17,     N's.amira',        N'amira@student.edusphere.local',        4, @DeptLang),
    (@UStud18,     N's.kabir',        N'kabir@student.edusphere.local',        4, @DeptLang)
) AS v([Id],[Username],[Email],[RoleId],[DeptId])
WHERE NOT EXISTS (SELECT 1 FROM [users] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- E.  ADMIN DEPARTMENT ASSIGNMENTS
-- ═════════════════════════════════════════════════════════════
INSERT INTO [admin_department_assignments]
    ([Id],[AdminUserId],[DepartmentId],[AssignedAt],[RemovedAt],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[Admin], v.[Dept], @Now, NULL, @Now, @Now
FROM (VALUES
    (@UAdminIT,   @DeptIT  ),
    (@UAdminBiz,  @DeptBiz ),
    (@UAdminLang, @DeptLang)
) AS v([Admin],[Dept])
WHERE NOT EXISTS (
    SELECT 1 FROM [admin_department_assignments]
    WHERE [AdminUserId] = v.[Admin] AND [DepartmentId] = v.[Dept] AND [RemovedAt] IS NULL
);

-- ═════════════════════════════════════════════════════════════
-- F.  FACULTY DEPARTMENT ASSIGNMENTS
-- ═════════════════════════════════════════════════════════════
INSERT INTO [faculty_department_assignments]
    ([Id],[FacultyUserId],[DepartmentId],[AssignedAt],[RemovedAt],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[Fac], v.[Dept], @Now, NULL, @Now, @Now
FROM (VALUES
    (@UFacAhmed,  @DeptIT  ), (@UFacSara,   @DeptIT  ), (@UFacAli,    @DeptIT  ),
    (@UFacKhan,   @DeptBiz ), (@UFacNaeem,  @DeptBiz ), (@UFacZara,   @DeptBiz ),
    (@UFacFatima, @DeptLang), (@UFacImran,  @DeptLang), (@UFacSana,   @DeptLang)
) AS v([Fac],[Dept])
WHERE NOT EXISTS (
    SELECT 1 FROM [faculty_department_assignments]
    WHERE [FacultyUserId] = v.[Fac] AND [DepartmentId] = v.[Dept] AND [RemovedAt] IS NULL
);

-- ═════════════════════════════════════════════════════════════
-- G.  COURSES
-- ═════════════════════════════════════════════════════════════
-- IT Department (6 courses)
DECLARE @CIT01 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER);
DECLARE @CIT02 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000102' AS UNIQUEIDENTIFIER);
DECLARE @CIT03 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000103' AS UNIQUEIDENTIFIER);
DECLARE @CIT04 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000104' AS UNIQUEIDENTIFIER);
DECLARE @CIT05 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000105' AS UNIQUEIDENTIFIER);
DECLARE @CIT06 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000106' AS UNIQUEIDENTIFIER);
-- Business Department (6 courses)
DECLARE @CBZ01 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000201' AS UNIQUEIDENTIFIER);
DECLARE @CBZ02 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000202' AS UNIQUEIDENTIFIER);
DECLARE @CBZ03 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000203' AS UNIQUEIDENTIFIER);
DECLARE @CBZ04 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000204' AS UNIQUEIDENTIFIER);
DECLARE @CBZ05 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000205' AS UNIQUEIDENTIFIER);
DECLARE @CBZ06 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000206' AS UNIQUEIDENTIFIER);
-- Languages Department (6 courses)
DECLARE @CLG01 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000301' AS UNIQUEIDENTIFIER);
DECLARE @CLG02 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000302' AS UNIQUEIDENTIFIER);
DECLARE @CLG03 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000303' AS UNIQUEIDENTIFIER);
DECLARE @CLG04 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000304' AS UNIQUEIDENTIFIER);
DECLARE @CLG05 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000305' AS UNIQUEIDENTIFIER);
DECLARE @CLG06 UNIQUEIDENTIFIER = CAST(N'CA000000-0000-0000-0000-000000000306' AS UNIQUEIDENTIFIER);

INSERT INTO [courses] ([Id],[Title],[Code],[CreditHours],[DepartmentId],[IsActive],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[Title], v.[Code], v.[Cr], v.[DeptId], 1, @Now, @Now, 0
FROM (VALUES
    -- IT
    (@CIT01, N'Introduction to Programming',         N'IT101', 3, @DeptIT),
    (@CIT02, N'Data Structures and Algorithms',      N'IT201', 3, @DeptIT),
    (@CIT03, N'Database Management Systems',         N'IT301', 3, @DeptIT),
    (@CIT04, N'Operating Systems',                   N'IT302', 3, @DeptIT),
    (@CIT05, N'Computer Networks',                   N'IT401', 3, @DeptIT),
    (@CIT06, N'Software Engineering',                N'IT402', 3, @DeptIT),
    -- Business
    (@CBZ01, N'Principles of Management',            N'BUS101', 3, @DeptBiz),
    (@CBZ02, N'Financial Accounting',                N'BUS102', 3, @DeptBiz),
    (@CBZ03, N'Marketing Management',                N'BUS201', 3, @DeptBiz),
    (@CBZ04, N'Business Statistics',                 N'BUS202', 3, @DeptBiz),
    (@CBZ05, N'Strategic Management',                N'BUS301', 3, @DeptBiz),
    (@CBZ06, N'Entrepreneurship and Innovation',     N'BUS302', 3, @DeptBiz),
    -- Languages
    (@CLG01, N'Arabic Language I',                   N'LANG101', 3, @DeptLang),
    (@CLG02, N'Arabic Language II',                  N'LANG102', 3, @DeptLang),
    (@CLG03, N'English Composition',                 N'LANG201', 3, @DeptLang),
    (@CLG04, N'English Literature',                  N'LANG202', 3, @DeptLang),
    (@CLG05, N'Chinese Language I',                  N'LANG301', 3, @DeptLang),
    (@CLG06, N'Chinese Language II',                 N'LANG302', 3, @DeptLang)
) AS v([Id],[Title],[Code],[Cr],[DeptId])
WHERE NOT EXISTS (SELECT 1 FROM [courses] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- H.  COURSE OFFERINGS
--     UNIQUE constraint: (CourseId, SemesterId)
--     Fall 2025: courses 1–3 per dept (6 total offerings)
--     Spring 2026: courses 4–6 per dept (6 total offerings — different courses!)
-- ═════════════════════════════════════════════════════════════
-- IT — Fall 2025
DECLARE @OIT01F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER);
DECLARE @OIT02F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000102' AS UNIQUEIDENTIFIER);
DECLARE @OIT03F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000103' AS UNIQUEIDENTIFIER);
-- IT — Spring 2026
DECLARE @OIT04S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000104' AS UNIQUEIDENTIFIER);
DECLARE @OIT05S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000105' AS UNIQUEIDENTIFIER);
DECLARE @OIT06S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000106' AS UNIQUEIDENTIFIER);
-- Business — Fall 2025
DECLARE @OBZ01F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000201' AS UNIQUEIDENTIFIER);
DECLARE @OBZ02F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000202' AS UNIQUEIDENTIFIER);
DECLARE @OBZ03F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000203' AS UNIQUEIDENTIFIER);
-- Business — Spring 2026
DECLARE @OBZ04S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000204' AS UNIQUEIDENTIFIER);
DECLARE @OBZ05S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000205' AS UNIQUEIDENTIFIER);
DECLARE @OBZ06S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000206' AS UNIQUEIDENTIFIER);
-- Languages — Fall 2025
DECLARE @OLG01F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000301' AS UNIQUEIDENTIFIER);
DECLARE @OLG02F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000302' AS UNIQUEIDENTIFIER);
DECLARE @OLG03F25 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000303' AS UNIQUEIDENTIFIER);
-- Languages — Spring 2026
DECLARE @OLG04S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000304' AS UNIQUEIDENTIFIER);
DECLARE @OLG05S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000305' AS UNIQUEIDENTIFIER);
DECLARE @OLG06S26 UNIQUEIDENTIFIER = CAST(N'OA000000-0000-0000-0000-000000000306' AS UNIQUEIDENTIFIER);

INSERT INTO [course_offerings] ([Id],[CourseId],[SemesterId],[FacultyUserId],[MaxEnrollment],[IsOpen],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[CourseId], v.[SemId], v.[FacId], 40, v.[IsOpen], @Now, @Now, 0
FROM (VALUES
    -- IT Fall 2025 (closed)
    (@OIT01F25, @CIT01, @SemFall25, @UFacAhmed, CAST(0 AS BIT)),
    (@OIT02F25, @CIT02, @SemFall25, @UFacSara,  CAST(0 AS BIT)),
    (@OIT03F25, @CIT03, @SemFall25, @UFacAli,   CAST(0 AS BIT)),
    -- IT Spring 2026 (open)
    (@OIT04S26, @CIT04, @SemSpr26,  @UFacAhmed, CAST(1 AS BIT)),
    (@OIT05S26, @CIT05, @SemSpr26,  @UFacSara,  CAST(1 AS BIT)),
    (@OIT06S26, @CIT06, @SemSpr26,  @UFacAli,   CAST(1 AS BIT)),
    -- Business Fall 2025 (closed)
    (@OBZ01F25, @CBZ01, @SemFall25, @UFacKhan,  CAST(0 AS BIT)),
    (@OBZ02F25, @CBZ02, @SemFall25, @UFacNaeem, CAST(0 AS BIT)),
    (@OBZ03F25, @CBZ03, @SemFall25, @UFacZara,  CAST(0 AS BIT)),
    -- Business Spring 2026 (open)
    (@OBZ04S26, @CBZ04, @SemSpr26,  @UFacKhan,  CAST(1 AS BIT)),
    (@OBZ05S26, @CBZ05, @SemSpr26,  @UFacNaeem, CAST(1 AS BIT)),
    (@OBZ06S26, @CBZ06, @SemSpr26,  @UFacZara,  CAST(1 AS BIT)),
    -- Languages Fall 2025 (closed)
    (@OLG01F25, @CLG01, @SemFall25, @UFacFatima, CAST(0 AS BIT)),
    (@OLG02F25, @CLG03, @SemFall25, @UFacImran,  CAST(0 AS BIT)),
    (@OLG03F25, @CLG05, @SemFall25, @UFacSana,   CAST(0 AS BIT)),
    -- Languages Spring 2026 (open)
    (@OLG04S26, @CLG02, @SemSpr26,  @UFacFatima, CAST(1 AS BIT)),
    (@OLG05S26, @CLG04, @SemSpr26,  @UFacImran,  CAST(1 AS BIT)),
    (@OLG06S26, @CLG06, @SemSpr26,  @UFacSana,   CAST(1 AS BIT))
) AS v([Id],[CourseId],[SemId],[FacId],[IsOpen])
WHERE NOT EXISTS (SELECT 1 FROM [course_offerings] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- I.  STUDENT PROFILES
-- ═════════════════════════════════════════════════════════════
-- IT students
DECLARE @SP01 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER);
DECLARE @SP02 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000102' AS UNIQUEIDENTIFIER);
DECLARE @SP03 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000103' AS UNIQUEIDENTIFIER);
DECLARE @SP04 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000104' AS UNIQUEIDENTIFIER);
DECLARE @SP05 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000105' AS UNIQUEIDENTIFIER);
DECLARE @SP06 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000106' AS UNIQUEIDENTIFIER);
-- Business students
DECLARE @SP07 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000107' AS UNIQUEIDENTIFIER);
DECLARE @SP08 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000108' AS UNIQUEIDENTIFIER);
DECLARE @SP09 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000109' AS UNIQUEIDENTIFIER);
DECLARE @SP10 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000110' AS UNIQUEIDENTIFIER);
DECLARE @SP11 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000111' AS UNIQUEIDENTIFIER);
DECLARE @SP12 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000112' AS UNIQUEIDENTIFIER);
-- Languages students
DECLARE @SP13 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000113' AS UNIQUEIDENTIFIER);
DECLARE @SP14 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000114' AS UNIQUEIDENTIFIER);
DECLARE @SP15 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000115' AS UNIQUEIDENTIFIER);
DECLARE @SP16 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000116' AS UNIQUEIDENTIFIER);
DECLARE @SP17 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000117' AS UNIQUEIDENTIFIER);
DECLARE @SP18 UNIQUEIDENTIFIER = CAST(N'SPA00000-0000-0000-0000-000000000118' AS UNIQUEIDENTIFIER);

INSERT INTO [student_profiles]
    ([Id],[UserId],[RegistrationNumber],[ProgramId],[DepartmentId],
     [AdmissionDate],[Cgpa],[CurrentSemesterNumber],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[UserId], v.[RegNo], v.[ProgId], v.[DeptId], v.[Adm], v.[Cgpa], v.[CurSem], @Now, @Now, 0
FROM (VALUES
    -- IT
    (@SP01, @UStud01, N'2024-IT-001', @ProgBSCS, @DeptIT,   '2024-09-01', CAST(3.50 AS DECIMAL(4,2)), 4),
    (@SP02, @UStud02, N'2024-IT-002', @ProgBSCS, @DeptIT,   '2024-09-01', CAST(3.20 AS DECIMAL(4,2)), 4),
    (@SP03, @UStud03, N'2025-IT-001', @ProgBSIT, @DeptIT,   '2025-09-01', CAST(2.90 AS DECIMAL(4,2)), 2),
    (@SP04, @UStud04, N'2025-IT-002', @ProgBSIT, @DeptIT,   '2025-09-01', CAST(3.70 AS DECIMAL(4,2)), 2),
    (@SP05, @UStud05, N'2025-IT-003', @ProgBSCS, @DeptIT,   '2025-09-01', CAST(3.10 AS DECIMAL(4,2)), 2),
    (@SP06, @UStud06, N'2025-IT-004', @ProgBSIT, @DeptIT,   '2025-09-01', CAST(3.40 AS DECIMAL(4,2)), 2),
    -- Business
    (@SP07, @UStud07, N'2024-BZ-001', @ProgBBA,  @DeptBiz,  '2024-09-01', CAST(3.00 AS DECIMAL(4,2)), 4),
    (@SP08, @UStud08, N'2024-BZ-002', @ProgBBA,  @DeptBiz,  '2024-09-01', CAST(2.80 AS DECIMAL(4,2)), 4),
    (@SP09, @UStud09, N'2025-BZ-001', @ProgBBA,  @DeptBiz,  '2025-09-01', CAST(3.60 AS DECIMAL(4,2)), 2),
    (@SP10, @UStud10, N'2025-BZ-002', @ProgMBA,  @DeptBiz,  '2025-09-01', CAST(3.80 AS DECIMAL(4,2)), 2),
    (@SP11, @UStud11, N'2025-BZ-003', @ProgMBA,  @DeptBiz,  '2025-09-01', CAST(3.50 AS DECIMAL(4,2)), 2),
    (@SP12, @UStud12, N'2025-BZ-004', @ProgBBA,  @DeptBiz,  '2025-09-01', CAST(2.70 AS DECIMAL(4,2)), 2),
    -- Languages
    (@SP13, @UStud13, N'2024-LG-001', @ProgArabic, @DeptLang,'2024-09-01', CAST(3.30 AS DECIMAL(4,2)), 4),
    (@SP14, @UStud14, N'2024-LG-002', @ProgEng,    @DeptLang,'2024-09-01', CAST(3.60 AS DECIMAL(4,2)), 4),
    (@SP15, @UStud15, N'2025-LG-001', @ProgArabic, @DeptLang,'2025-09-01', CAST(3.10 AS DECIMAL(4,2)), 2),
    (@SP16, @UStud16, N'2025-LG-002', @ProgEng,    @DeptLang,'2025-09-01', CAST(2.90 AS DECIMAL(4,2)), 2),
    (@SP17, @UStud17, N'2025-LG-003', @ProgChinese,@DeptLang,'2025-09-01', CAST(3.40 AS DECIMAL(4,2)), 2),
    (@SP18, @UStud18, N'2025-LG-004', @ProgChinese,@DeptLang,'2025-09-01', CAST(3.20 AS DECIMAL(4,2)), 2)
) AS v([Id],[UserId],[RegNo],[ProgId],[DeptId],[Adm],[Cgpa],[CurSem])
WHERE NOT EXISTS (SELECT 1 FROM [student_profiles] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- J.  REGISTRATION WHITELIST
-- ═════════════════════════════════════════════════════════════
INSERT INTO [registration_whitelist]
    ([Id],[IdentifierType],[IdentifierValue],[DepartmentId],[ProgramId],
     [IsUsed],[UsedAt],[CreatedUserId],[CreatedAt],[UpdatedAt])
SELECT NEWID(), N'Email', v.[Email], v.[DeptId], v.[ProgId], 1, @Now, @USuperAdmin, @Now, @Now
FROM (VALUES
    (N'aslam@student.edusphere.local',   @DeptIT,   @ProgBSCS),
    (N'bilal@student.edusphere.local',   @DeptIT,   @ProgBSCS),
    (N'fatima.s@student.edusphere.local',@DeptIT,   @ProgBSIT),
    (N'hina@student.edusphere.local',    @DeptIT,   @ProgBSIT),
    (N'kamran@student.edusphere.local',  @DeptIT,   @ProgBSCS),
    (N'layla@student.edusphere.local',   @DeptIT,   @ProgBSIT),
    (N'nadia@student.edusphere.local',   @DeptBiz,  @ProgBBA ),
    (N'omar@student.edusphere.local',    @DeptBiz,  @ProgBBA ),
    (N'rabia@student.edusphere.local',   @DeptBiz,  @ProgBBA ),
    (N'saad@student.edusphere.local',    @DeptBiz,  @ProgMBA ),
    (N'shazia@student.edusphere.local',  @DeptBiz,  @ProgMBA ),
    (N'tariq@student.edusphere.local',   @DeptBiz,  @ProgBBA ),
    (N'usman@student.edusphere.local',   @DeptLang, @ProgArabic),
    (N'yasmin@student.edusphere.local',  @DeptLang, @ProgEng),
    (N'zainab@student.edusphere.local',  @DeptLang, @ProgArabic),
    (N'danish@student.edusphere.local',  @DeptLang, @ProgEng),
    (N'amira@student.edusphere.local',   @DeptLang, @ProgChinese),
    (N'kabir@student.edusphere.local',   @DeptLang, @ProgChinese)
) AS v([Email],[DeptId],[ProgId])
WHERE NOT EXISTS (SELECT 1 FROM [registration_whitelist] WHERE [IdentifierValue] = v.[Email]);

-- ═════════════════════════════════════════════════════════════
-- K.  ENROLLMENTS
--     IT students 01-04 enrolled in Fall 2025 IT offerings (01-03)
--     IT students 01-06 enrolled in Spring 2026 IT offerings (04-06)
--     Business students enrolled similarly
--     Languages students enrolled similarly
-- ═════════════════════════════════════════════════════════════
INSERT INTO [enrollments] ([Id],[StudentProfileId],[CourseOfferingId],[EnrolledAt],[DroppedAt],[Status],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[SpId], v.[OffId], v.[EnrolledAt], NULL, N'Active', @Now, @Now
FROM (VALUES
    -- IT Fall 2025 enrollments (students 01-04)
    (@SP01, @OIT01F25, '2025-09-05'),(@SP01, @OIT02F25, '2025-09-05'),(@SP01, @OIT03F25, '2025-09-05'),
    (@SP02, @OIT01F25, '2025-09-05'),(@SP02, @OIT02F25, '2025-09-05'),(@SP02, @OIT03F25, '2025-09-05'),
    (@SP03, @OIT01F25, '2025-09-05'),(@SP03, @OIT02F25, '2025-09-05'),(@SP03, @OIT03F25, '2025-09-05'),
    (@SP04, @OIT01F25, '2025-09-05'),(@SP04, @OIT02F25, '2025-09-05'),(@SP04, @OIT03F25, '2025-09-05'),
    -- IT Spring 2026 enrollments (students 01-06)
    (@SP01, @OIT04S26, '2026-02-05'),(@SP01, @OIT05S26, '2026-02-05'),(@SP01, @OIT06S26, '2026-02-05'),
    (@SP02, @OIT04S26, '2026-02-05'),(@SP02, @OIT05S26, '2026-02-05'),(@SP02, @OIT06S26, '2026-02-05'),
    (@SP03, @OIT04S26, '2026-02-05'),(@SP03, @OIT05S26, '2026-02-05'),(@SP03, @OIT06S26, '2026-02-05'),
    (@SP04, @OIT04S26, '2026-02-05'),(@SP04, @OIT05S26, '2026-02-05'),(@SP04, @OIT06S26, '2026-02-05'),
    (@SP05, @OIT04S26, '2026-02-05'),(@SP05, @OIT05S26, '2026-02-05'),(@SP05, @OIT06S26, '2026-02-05'),
    (@SP06, @OIT04S26, '2026-02-05'),(@SP06, @OIT05S26, '2026-02-05'),(@SP06, @OIT06S26, '2026-02-05'),
    -- Business Fall 2025
    (@SP07, @OBZ01F25, '2025-09-05'),(@SP07, @OBZ02F25, '2025-09-05'),(@SP07, @OBZ03F25, '2025-09-05'),
    (@SP08, @OBZ01F25, '2025-09-05'),(@SP08, @OBZ02F25, '2025-09-05'),(@SP08, @OBZ03F25, '2025-09-05'),
    (@SP09, @OBZ01F25, '2025-09-05'),(@SP09, @OBZ02F25, '2025-09-05'),(@SP09, @OBZ03F25, '2025-09-05'),
    -- Business Spring 2026
    (@SP07, @OBZ04S26, '2026-02-05'),(@SP07, @OBZ05S26, '2026-02-05'),(@SP07, @OBZ06S26, '2026-02-05'),
    (@SP08, @OBZ04S26, '2026-02-05'),(@SP08, @OBZ05S26, '2026-02-05'),(@SP08, @OBZ06S26, '2026-02-05'),
    (@SP09, @OBZ04S26, '2026-02-05'),(@SP09, @OBZ05S26, '2026-02-05'),(@SP09, @OBZ06S26, '2026-02-05'),
    (@SP10, @OBZ04S26, '2026-02-05'),(@SP10, @OBZ05S26, '2026-02-05'),(@SP10, @OBZ06S26, '2026-02-05'),
    (@SP11, @OBZ04S26, '2026-02-05'),(@SP11, @OBZ05S26, '2026-02-05'),(@SP11, @OBZ06S26, '2026-02-05'),
    (@SP12, @OBZ04S26, '2026-02-05'),(@SP12, @OBZ05S26, '2026-02-05'),(@SP12, @OBZ06S26, '2026-02-05'),
    -- Languages Fall 2025
    (@SP13, @OLG01F25, '2025-09-05'),(@SP13, @OLG02F25, '2025-09-05'),(@SP13, @OLG03F25, '2025-09-05'),
    (@SP14, @OLG01F25, '2025-09-05'),(@SP14, @OLG02F25, '2025-09-05'),(@SP14, @OLG03F25, '2025-09-05'),
    -- Languages Spring 2026
    (@SP13, @OLG04S26, '2026-02-05'),(@SP13, @OLG05S26, '2026-02-05'),(@SP13, @OLG06S26, '2026-02-05'),
    (@SP14, @OLG04S26, '2026-02-05'),(@SP14, @OLG05S26, '2026-02-05'),(@SP14, @OLG06S26, '2026-02-05'),
    (@SP15, @OLG04S26, '2026-02-05'),(@SP15, @OLG05S26, '2026-02-05'),(@SP15, @OLG06S26, '2026-02-05'),
    (@SP16, @OLG04S26, '2026-02-05'),(@SP16, @OLG05S26, '2026-02-05'),(@SP16, @OLG06S26, '2026-02-05'),
    (@SP17, @OLG04S26, '2026-02-05'),(@SP17, @OLG05S26, '2026-02-05'),(@SP17, @OLG06S26, '2026-02-05'),
    (@SP18, @OLG04S26, '2026-02-05'),(@SP18, @OLG05S26, '2026-02-05'),(@SP18, @OLG06S26, '2026-02-05')
) AS v([SpId],[OffId],[EnrolledAt])
WHERE NOT EXISTS (
    SELECT 1 FROM [enrollments] WHERE [StudentProfileId] = v.[SpId] AND [CourseOfferingId] = v.[OffId]
);

-- ═════════════════════════════════════════════════════════════
-- L.  PUBLISHED RESULTS (Fall 2025 — closed semester)
--     ResultType: Midterm | Final   (matching live DB values)
-- ═════════════════════════════════════════════════════════════
INSERT INTO [results]
    ([Id],[StudentProfileId],[CourseOfferingId],[ResultType],
     [MarksObtained],[MaxMarks],[IsPublished],[PublishedAt],[PublishedByUserId],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[SpId], v.[OffId], v.[Type], v.[Obtained], v.[Max], 1, '2026-01-28', v.[PubBy], @Now, @Now
FROM (VALUES
    -- IT — OIT01F25 (Dr. Ahmed)
    (@SP01,@OIT01F25,N'Midterm', CAST(62  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP01,@OIT01F25,N'Final',   CAST(90  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP02,@OIT01F25,N'Midterm', CAST(58  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP02,@OIT01F25,N'Final',   CAST(82  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP03,@OIT01F25,N'Midterm', CAST(50  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP03,@OIT01F25,N'Final',   CAST(72  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP04,@OIT01F25,N'Midterm', CAST(70  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAhmed),
    (@SP04,@OIT01F25,N'Final',   CAST(95  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAhmed),
    -- IT — OIT02F25 (Dr. Sara)
    (@SP01,@OIT02F25,N'Midterm', CAST(55  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSara),
    (@SP01,@OIT02F25,N'Final',   CAST(85  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSara),
    (@SP02,@OIT02F25,N'Midterm', CAST(60  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSara),
    (@SP02,@OIT02F25,N'Final',   CAST(78  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSara),
    (@SP03,@OIT02F25,N'Midterm', CAST(45  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSara),
    (@SP03,@OIT02F25,N'Final',   CAST(68  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSara),
    (@SP04,@OIT02F25,N'Midterm', CAST(72  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSara),
    (@SP04,@OIT02F25,N'Final',   CAST(92  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSara),
    -- IT — OIT03F25 (Mr. Ali)
    (@SP01,@OIT03F25,N'Midterm', CAST(65  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAli),
    (@SP01,@OIT03F25,N'Final',   CAST(88  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAli),
    (@SP02,@OIT03F25,N'Midterm', CAST(55  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAli),
    (@SP02,@OIT03F25,N'Final',   CAST(80  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAli),
    (@SP03,@OIT03F25,N'Midterm', CAST(48  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAli),
    (@SP03,@OIT03F25,N'Final',   CAST(65  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAli),
    (@SP04,@OIT03F25,N'Midterm', CAST(68  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacAli),
    (@SP04,@OIT03F25,N'Final',   CAST(91  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacAli),
    -- Business — OBZ01F25
    (@SP07,@OBZ01F25,N'Midterm', CAST(60  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacKhan),
    (@SP07,@OBZ01F25,N'Final',   CAST(80  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacKhan),
    (@SP08,@OBZ01F25,N'Midterm', CAST(50  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacKhan),
    (@SP08,@OBZ01F25,N'Final',   CAST(70  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacKhan),
    (@SP09,@OBZ01F25,N'Midterm', CAST(65  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacKhan),
    (@SP09,@OBZ01F25,N'Final',   CAST(85  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacKhan),
    -- Business — OBZ02F25
    (@SP07,@OBZ02F25,N'Midterm', CAST(55  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacNaeem),
    (@SP07,@OBZ02F25,N'Final',   CAST(75  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacNaeem),
    (@SP08,@OBZ02F25,N'Midterm', CAST(45  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacNaeem),
    (@SP08,@OBZ02F25,N'Final',   CAST(66  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacNaeem),
    (@SP09,@OBZ02F25,N'Midterm', CAST(70  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacNaeem),
    (@SP09,@OBZ02F25,N'Final',   CAST(88  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacNaeem),
    -- Business — OBZ03F25
    (@SP07,@OBZ03F25,N'Midterm', CAST(62  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacZara),
    (@SP07,@OBZ03F25,N'Final',   CAST(78  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacZara),
    (@SP08,@OBZ03F25,N'Midterm', CAST(52  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacZara),
    (@SP08,@OBZ03F25,N'Final',   CAST(72  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacZara),
    (@SP09,@OBZ03F25,N'Midterm', CAST(68  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacZara),
    (@SP09,@OBZ03F25,N'Final',   CAST(90  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacZara),
    -- Languages — OLG01F25
    (@SP13,@OLG01F25,N'Midterm', CAST(64  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacFatima),
    (@SP13,@OLG01F25,N'Final',   CAST(84  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacFatima),
    (@SP14,@OLG01F25,N'Midterm', CAST(70  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacFatima),
    (@SP14,@OLG01F25,N'Final',   CAST(92  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacFatima),
    -- Languages — OLG02F25
    (@SP13,@OLG02F25,N'Midterm', CAST(58  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacImran),
    (@SP13,@OLG02F25,N'Final',   CAST(79  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacImran),
    (@SP14,@OLG02F25,N'Midterm', CAST(68  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacImran),
    (@SP14,@OLG02F25,N'Final',   CAST(88  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacImran),
    -- Languages — OLG03F25
    (@SP13,@OLG03F25,N'Midterm', CAST(62  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSana),
    (@SP13,@OLG03F25,N'Final',   CAST(82  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSana),
    (@SP14,@OLG03F25,N'Midterm', CAST(72  AS DECIMAL(8,2)),CAST(75 AS DECIMAL(8,2)),@UFacSana),
    (@SP14,@OLG03F25,N'Final',   CAST(93  AS DECIMAL(8,2)),CAST(100 AS DECIMAL(8,2)),@UFacSana)
) AS v([SpId],[OffId],[Type],[Obtained],[Max],[PubBy])
WHERE NOT EXISTS (
    SELECT 1 FROM [results]
    WHERE [StudentProfileId]=v.[SpId] AND [CourseOfferingId]=v.[OffId] AND [ResultType]=v.[Type]
);

-- ═════════════════════════════════════════════════════════════
-- M.  ATTENDANCE RECORDS (Spring 2026 — active semester)
--     3 dates per offering, 4 students per date
-- ═════════════════════════════════════════════════════════════
INSERT INTO [attendance_records]
    ([Id],[StudentProfileId],[CourseOfferingId],[Date],[Status],[MarkedByUserId],[Remarks],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[SpId], v.[OffId], v.[Dt], v.[Status], v.[MarkedBy], NULL, @Now, @Now
FROM (VALUES
    -- IT Spring 2026 — OIT04S26 (Operating Systems)
    (@SP01,@OIT04S26,'2026-02-10',N'Present',@UFacAhmed),
    (@SP02,@OIT04S26,'2026-02-10',N'Present',@UFacAhmed),
    (@SP03,@OIT04S26,'2026-02-10',N'Absent', @UFacAhmed),
    (@SP04,@OIT04S26,'2026-02-10',N'Present',@UFacAhmed),
    (@SP01,@OIT04S26,'2026-02-17',N'Present',@UFacAhmed),
    (@SP02,@OIT04S26,'2026-02-17',N'Late',   @UFacAhmed),
    (@SP03,@OIT04S26,'2026-02-17',N'Present',@UFacAhmed),
    (@SP04,@OIT04S26,'2026-02-17',N'Present',@UFacAhmed),
    (@SP01,@OIT04S26,'2026-02-24',N'Present',@UFacAhmed),
    (@SP02,@OIT04S26,'2026-02-24',N'Present',@UFacAhmed),
    (@SP03,@OIT04S26,'2026-02-24',N'Present',@UFacAhmed),
    (@SP04,@OIT04S26,'2026-02-24',N'Absent', @UFacAhmed),
    -- IT Spring 2026 — OIT05S26 (Computer Networks)
    (@SP01,@OIT05S26,'2026-02-11',N'Present',@UFacSara),
    (@SP02,@OIT05S26,'2026-02-11',N'Present',@UFacSara),
    (@SP05,@OIT05S26,'2026-02-11',N'Absent', @UFacSara),
    (@SP06,@OIT05S26,'2026-02-11',N'Present',@UFacSara),
    (@SP01,@OIT05S26,'2026-02-18',N'Present',@UFacSara),
    (@SP02,@OIT05S26,'2026-02-18',N'Present',@UFacSara),
    (@SP05,@OIT05S26,'2026-02-18',N'Present',@UFacSara),
    (@SP06,@OIT05S26,'2026-02-18',N'Late',   @UFacSara),
    -- Business Spring 2026 — OBZ04S26 (Business Statistics)
    (@SP07,@OBZ04S26,'2026-02-10',N'Present',@UFacKhan),
    (@SP08,@OBZ04S26,'2026-02-10',N'Present',@UFacKhan),
    (@SP09,@OBZ04S26,'2026-02-10',N'Absent', @UFacKhan),
    (@SP10,@OBZ04S26,'2026-02-10',N'Present',@UFacKhan),
    (@SP07,@OBZ04S26,'2026-02-17',N'Present',@UFacKhan),
    (@SP08,@OBZ04S26,'2026-02-17',N'Present',@UFacKhan),
    (@SP09,@OBZ04S26,'2026-02-17',N'Present',@UFacKhan),
    (@SP10,@OBZ04S26,'2026-02-17',N'Late',   @UFacKhan),
    -- Languages Spring 2026 — OLG04S26 (Arabic Language II)
    (@SP13,@OLG04S26,'2026-02-09',N'Present',@UFacFatima),
    (@SP14,@OLG04S26,'2026-02-09',N'Present',@UFacFatima),
    (@SP15,@OLG04S26,'2026-02-09',N'Present',@UFacFatima),
    (@SP16,@OLG04S26,'2026-02-09',N'Absent', @UFacFatima),
    (@SP13,@OLG04S26,'2026-02-16',N'Present',@UFacFatima),
    (@SP14,@OLG04S26,'2026-02-16',N'Late',   @UFacFatima),
    (@SP15,@OLG04S26,'2026-02-16',N'Present',@UFacFatima),
    (@SP16,@OLG04S26,'2026-02-16',N'Present',@UFacFatima)
) AS v([SpId],[OffId],[Dt],[Status],[MarkedBy])
WHERE NOT EXISTS (
    SELECT 1 FROM [attendance_records]
    WHERE [StudentProfileId]=v.[SpId] AND [CourseOfferingId]=v.[OffId] AND [Date]=v.[Dt]
);

-- ═════════════════════════════════════════════════════════════
-- N.  ASSIGNMENTS (Spring 2026 — active semester)
-- ═════════════════════════════════════════════════════════════
DECLARE @Asgn01 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @Asgn02 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @Asgn03 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @Asgn04 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER);
DECLARE @Asgn05 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000005' AS UNIQUEIDENTIFIER);
DECLARE @Asgn06 UNIQUEIDENTIFIER = CAST(N'AE000000-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER);

INSERT INTO [assignments]
    ([Id],[CourseOfferingId],[Title],[Description],[DueDate],[MaxMarks],[IsPublished],[PublishedAt],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[OffId], v.[Title], v.[Desc], v.[Due], v.[Max], 1, '2026-02-10 00:00:00', @Now, @Now, 0
FROM (VALUES
    (@Asgn01, @OIT04S26, N'OS Process Scheduling Analysis',
        N'Analyze and compare FCFS, SJF, Round Robin, and Priority scheduling algorithms with real examples. Submit a report with diagrams.',
        '2026-03-10 23:59:00', CAST(20 AS DECIMAL(8,2))),
    (@Asgn02, @OIT05S26, N'Network Topology Design',
        N'Design a scalable network topology for a medium-sized enterprise with 500 users across three floors. Include IP addressing scheme.',
        '2026-03-12 23:59:00', CAST(20 AS DECIMAL(8,2))),
    (@Asgn03, @OIT06S26, N'Software Requirements Specification',
        N'Write a complete SRS document for a Library Management System following IEEE 830 standard.',
        '2026-03-15 23:59:00', CAST(25 AS DECIMAL(8,2))),
    (@Asgn04, @OBZ04S26, N'Statistical Analysis of Market Data',
        N'Collect monthly sales data of any three companies from Pakistan Stock Exchange for the last year. Apply descriptive and inferential statistics.',
        '2026-03-10 23:59:00', CAST(20 AS DECIMAL(8,2))),
    (@Asgn05, @OLG04S26, N'Arabic Writing Portfolio',
        N'Write five short paragraphs in Arabic on daily life topics using vocabulary covered in weeks 1-4.',
        '2026-03-08 23:59:00', CAST(15 AS DECIMAL(8,2))),
    (@Asgn06, @OBZ05S26, N'Strategic Analysis — Case Study',
        N'Apply SWOT and Porter Five Forces analysis to a company of your choice operating in Pakistan.',
        '2026-03-20 23:59:00', CAST(25 AS DECIMAL(8,2)))
) AS v([Id],[OffId],[Title],[Desc],[Due],[Max])
WHERE NOT EXISTS (SELECT 1 FROM [assignments] WHERE [Id] = v.[Id]);

-- Assignment Submissions + Grading
INSERT INTO [assignment_submissions]
    ([Id],[AssignmentId],[StudentProfileId],[TextContent],[SubmittedAt],[MarksAwarded],[Feedback],[GradedAt],[GradedByUserId],[Status],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[AsgId], v.[SpId],
    N'Submitted assignment content for ' + v.[Note],
    v.[SubAt], v.[Marks], v.[Feedback], v.[GradedAt], v.[GradedBy], v.[Status], @Now, @Now
FROM (VALUES
    (@Asgn01, @SP01, '2026-03-08', CAST(18 AS DECIMAL(8,2)), N'Excellent analysis with clear diagrams.', '2026-03-11', @UFacAhmed, N'Graded', N'SP01 Asgn01'),
    (@Asgn01, @SP02, '2026-03-09', CAST(15 AS DECIMAL(8,2)), N'Good attempt. Missing Round Robin analysis.', '2026-03-11', @UFacAhmed, N'Graded', N'SP02 Asgn01'),
    (@Asgn01, @SP03, '2026-03-10', CAST(12 AS DECIMAL(8,2)), N'Needs more detail in diagrams.', '2026-03-12', @UFacAhmed, N'Graded', N'SP03 Asgn01'),
    (@Asgn01, @SP04, '2026-03-07', CAST(19 AS DECIMAL(8,2)), N'Outstanding work. Excellent examples.', '2026-03-11', @UFacAhmed, N'Graded', N'SP04 Asgn01'),
    (@Asgn02, @SP01, '2026-03-11', CAST(17 AS DECIMAL(8,2)), N'Good topology design. IP scheme is correct.', '2026-03-13', @UFacSara,  N'Graded', N'SP01 Asgn02'),
    (@Asgn02, @SP05, '2026-03-12', CAST(14 AS DECIMAL(8,2)), N'Acceptable. Could improve redundancy planning.', '2026-03-14', @UFacSara, N'Graded', N'SP05 Asgn02'),
    (@Asgn04, @SP07, '2026-03-09', CAST(17 AS DECIMAL(8,2)), N'Thorough analysis of market data.', '2026-03-12', @UFacKhan,  N'Graded', N'SP07 Asgn04'),
    (@Asgn04, @SP08, '2026-03-10', CAST(13 AS DECIMAL(8,2)), N'Inferential statistics section incomplete.', '2026-03-12', @UFacKhan,  N'Graded', N'SP08 Asgn04'),
    (@Asgn05, @SP13, '2026-03-07', CAST(13 AS DECIMAL(8,2)), N'Good vocabulary usage. Some grammar errors.', '2026-03-09', @UFacFatima,N'Graded', N'SP13 Asgn05'),
    (@Asgn05, @SP14, '2026-03-08', CAST(14 AS DECIMAL(8,2)), N'Excellent paragraphs. Very fluent writing.', '2026-03-09', @UFacFatima,N'Graded', N'SP14 Asgn05')
) AS v([AsgId],[SpId],[SubAt],[Marks],[Feedback],[GradedAt],[GradedBy],[Status],[Note])
WHERE NOT EXISTS (
    SELECT 1 FROM [assignment_submissions]
    WHERE [AssignmentId]=v.[AsgId] AND [StudentProfileId]=v.[SpId]
);

-- ═════════════════════════════════════════════════════════════
-- O.  QUIZZES + QUESTIONS + OPTIONS
-- ═════════════════════════════════════════════════════════════
DECLARE @Quiz01 UNIQUEIDENTIFIER = CAST(N'QZ000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @Quiz02 UNIQUEIDENTIFIER = CAST(N'QZ000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);

INSERT INTO [quizzes]
    ([Id],[CourseOfferingId],[Title],[Instructions],[TimeLimitMinutes],[MaxAttempts],
     [AvailableFrom],[AvailableUntil],[IsPublished],[IsActive],[CreatedByUserId],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[OffId], v.[Title], v.[Instr], v.[TL], v.[MA], v.[From], v.[Until], 1, 1, v.[By], @Now, @Now
FROM (VALUES
    (@Quiz01, @OIT04S26, N'OS Quiz 1 — Process Management',
     N'Choose the best answer. Each question is worth equal marks.',
     20, 1, '2026-03-01 08:00:00', '2026-03-07 23:59:00', @UFacAhmed),
    (@Quiz02, @OBZ04S26, N'Business Stats — Descriptive Statistics',
     N'Select the correct answer. No negative marking.',
     25, 1, '2026-03-03 08:00:00', '2026-03-09 23:59:00', @UFacKhan)
) AS v([Id],[OffId],[Title],[Instr],[TL],[MA],[From],[Until],[By])
WHERE NOT EXISTS (SELECT 1 FROM [quizzes] WHERE [Id] = v.[Id]);

-- Quiz Questions
DECLARE @QQ01 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @QQ02 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @QQ03 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @QQ04 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER);
DECLARE @QQ05 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000005' AS UNIQUEIDENTIFIER);
DECLARE @QQ06 UNIQUEIDENTIFIER = CAST(N'QQ000000-0000-0000-0000-000000000006' AS UNIQUEIDENTIFIER);

INSERT INTO [quiz_questions] ([Id],[QuizId],[Text],[Type],[Marks],[OrderIndex],[QuizId1],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[QuizId], v.[Text], N'MCQ', v.[Marks], v.[Ord], v.[QuizId], @Now, @Now
FROM (VALUES
    (@QQ01, @Quiz01, N'Which scheduling algorithm is non-preemptive?',         CAST(2 AS DECIMAL(8,2)), 1),
    (@QQ02, @Quiz01, N'What is the primary goal of Round Robin scheduling?',    CAST(2 AS DECIMAL(8,2)), 2),
    (@QQ03, @Quiz01, N'Which state does a process NOT exist in?',               CAST(2 AS DECIMAL(8,2)), 3),
    (@QQ04, @Quiz02, N'The arithmetic mean is most affected by?',               CAST(2 AS DECIMAL(8,2)), 1),
    (@QQ05, @Quiz02, N'Standard deviation measures?',                           CAST(2 AS DECIMAL(8,2)), 2),
    (@QQ06, @Quiz02, N'Which is a measure of central tendency?',                CAST(2 AS DECIMAL(8,2)), 3)
) AS v([Id],[QuizId],[Text],[Marks],[Ord])
WHERE NOT EXISTS (SELECT 1 FROM [quiz_questions] WHERE [Id] = v.[Id]);

-- Quiz Options
DECLARE @QO0101 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0001-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @QO0102 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0001-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @QO0103 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0001-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @QO0104 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0001-000000000004' AS UNIQUEIDENTIFIER);
DECLARE @QO0201 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0002-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @QO0202 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0002-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @QO0203 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0002-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @QO0204 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0002-000000000004' AS UNIQUEIDENTIFIER);
DECLARE @QO0401 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0004-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @QO0402 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0004-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @QO0403 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0004-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @QO0404 UNIQUEIDENTIFIER = CAST(N'QO000000-0000-0000-0004-000000000004' AS UNIQUEIDENTIFIER);

INSERT INTO [quiz_options] ([Id],[QuizQuestionId],[Text],[IsCorrect],[OrderIndex],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[QQId], v.[Text], v.[IsCorrect], v.[Ord], @Now, @Now
FROM (VALUES
    (@QO0101, @QQ01, N'Round Robin',     CAST(0 AS BIT), 1),
    (@QO0102, @QQ01, N'FCFS',            CAST(1 AS BIT), 2),  -- correct
    (@QO0103, @QQ01, N'Preemptive SJF',  CAST(0 AS BIT), 3),
    (@QO0104, @QQ01, N'Priority (prem)', CAST(0 AS BIT), 4),
    (@QO0201, @QQ02, N'Minimize waiting time',      CAST(0 AS BIT), 1),
    (@QO0202, @QQ02, N'Fair CPU time sharing',       CAST(1 AS BIT), 2),  -- correct
    (@QO0203, @QQ02, N'Maximize throughput',         CAST(0 AS BIT), 3),
    (@QO0204, @QQ02, N'Minimize context switches',   CAST(0 AS BIT), 4),
    (@QO0401, @QQ04, N'Outliers / extreme values',   CAST(1 AS BIT), 1),  -- correct
    (@QO0402, @QQ04, N'Median',                      CAST(0 AS BIT), 2),
    (@QO0403, @QQ04, N'Mode',                        CAST(0 AS BIT), 3),
    (@QO0404, @QQ04, N'Sample size',                 CAST(0 AS BIT), 4)
) AS v([Id],[QQId],[Text],[IsCorrect],[Ord])
WHERE NOT EXISTS (SELECT 1 FROM [quiz_options] WHERE [Id] = v.[Id]);

-- Quiz Attempts + Answers (students who attempted Quiz01)
DECLARE @Att01SP01 UNIQUEIDENTIFIER = CAST(N'QA000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @Att01SP02 UNIQUEIDENTIFIER = CAST(N'QA000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);

INSERT INTO [quiz_attempts]
    ([Id],[QuizId],[StudentProfileId],[StartedAt],[FinishedAt],[Status],[TotalScore],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[QuizId], v.[SpId], v.[Start], v.[Finish], N'Completed', v.[Score], @Now, @Now
FROM (VALUES
    (@Att01SP01, @Quiz01, @SP01, '2026-03-05 10:00:00', '2026-03-05 10:18:00', CAST(4 AS DECIMAL(8,2))),
    (@Att01SP02, @Quiz01, @SP02, '2026-03-06 11:00:00', '2026-03-06 11:20:00', CAST(2 AS DECIMAL(8,2)))
) AS v([Id],[QuizId],[SpId],[Start],[Finish],[Score])
WHERE NOT EXISTS (SELECT 1 FROM [quiz_attempts] WHERE [Id] = v.[Id]);

INSERT INTO [quiz_answers]
    ([Id],[QuizAttemptId],[QuizQuestionId],[SelectedOptionId],[TextResponse],[MarksAwarded],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[AtId], v.[QQId], v.[OptId], NULL, v.[Marks], @Now, @Now
FROM (VALUES
    -- SP01 attempt: Q1 correct (FCFS), Q2 correct (fair sharing)
    (@Att01SP01, @QQ01, @QO0102, CAST(2 AS DECIMAL(8,2))),
    (@Att01SP01, @QQ02, @QO0202, CAST(2 AS DECIMAL(8,2))),
    -- SP02 attempt: Q1 wrong (Round Robin), Q2 correct (fair sharing)
    (@Att01SP02, @QQ01, @QO0101, CAST(0 AS DECIMAL(8,2))),
    (@Att01SP02, @QQ02, @QO0202, CAST(2 AS DECIMAL(8,2)))
) AS v([AtId],[QQId],[OptId],[Marks])
WHERE NOT EXISTS (
    SELECT 1 FROM [quiz_answers] WHERE [QuizAttemptId]=v.[AtId] AND [QuizQuestionId]=v.[QQId]
);

-- ═════════════════════════════════════════════════════════════
-- P.  FYP PROJECTS (IT 4th-semester students SP01/SP02)
-- ═════════════════════════════════════════════════════════════
DECLARE @FYP01 UNIQUEIDENTIFIER = CAST(N'FP000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @FYP02 UNIQUEIDENTIFIER = CAST(N'FP000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);

INSERT INTO [fyp_projects]
    ([Id],[StudentProfileId],[DepartmentId],[Title],[Description],[Status],[SupervisorUserId],
     [CoordinatorRemarks],[IsCompletionRequested],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[SpId], @DeptIT, v.[Title], v.[Desc], v.[Status], v.[Supervisor], v.[Remarks], 0, @Now, @Now
FROM (VALUES
    (@FYP01, @SP01, N'AI-Powered Student Performance Prediction System',
     N'A machine learning system that predicts student academic performance using historical GPA, attendance, and quiz data. Built with Python, FastAPI, and React.',
     N'InProgress', @UFacAhmed, N'Good progress. Needs more data collection.'),
    (@FYP02, @SP02, N'Secure Cloud-Based Document Management System',
     N'A secure document management platform with AES-256 encryption, role-based access, and audit trails. Tech stack: ASP.NET Core, Azure Blob Storage, SQL Server.',
     N'Approved', @UFacSara, N'Approved. Supervisor assigned.')
) AS v([Id],[SpId],[Title],[Desc],[Status],[Supervisor],[Remarks])
WHERE NOT EXISTS (SELECT 1 FROM [fyp_projects] WHERE [Id] = v.[Id]);

-- ═════════════════════════════════════════════════════════════
-- Q.  BUILDINGS + ROOMS + TIMETABLES + TIMETABLE ENTRIES
-- ═════════════════════════════════════════════════════════════
DECLARE @BuildA UNIQUEIDENTIFIER = CAST(N'BLD00000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @BuildB UNIQUEIDENTIFIER = CAST(N'BLD00000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);

IF NOT EXISTS (SELECT 1 FROM [buildings] WHERE [Id] = @BuildA)
BEGIN
    DECLARE @BldCols NVARCHAR(MAX);
    SELECT @BldCols = STRING_AGG(c.name, ', ')
    FROM sys.columns c JOIN sys.tables t ON c.object_id = t.object_id
    WHERE t.name = 'buildings';

    -- Insert via dynamic columns discovery
    INSERT INTO [buildings] ([Id],[Name],[CreatedAt],[UpdatedAt])
    VALUES (@BuildA, N'Block A — Technology', @Now, @Now);
END;
IF NOT EXISTS (SELECT 1 FROM [buildings] WHERE [Id] = @BuildB)
    INSERT INTO [buildings] ([Id],[Name],[CreatedAt],[UpdatedAt])
    VALUES (@BuildB, N'Block B — Business & Languages', @Now, @Now);

DECLARE @Room101 UNIQUEIDENTIFIER = CAST(N'RM000000-0000-0000-0000-000000000101' AS UNIQUEIDENTIFIER);
DECLARE @Room102 UNIQUEIDENTIFIER = CAST(N'RM000000-0000-0000-0000-000000000102' AS UNIQUEIDENTIFIER);
DECLARE @Room201 UNIQUEIDENTIFIER = CAST(N'RM000000-0000-0000-0000-000000000201' AS UNIQUEIDENTIFIER);
DECLARE @Room202 UNIQUEIDENTIFIER = CAST(N'RM000000-0000-0000-0000-000000000202' AS UNIQUEIDENTIFIER);

INSERT INTO [rooms] ([Id],[Name],[BuildingId],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Name], v.[BldId], @Now, @Now
FROM (VALUES
    (@Room101, N'A-101 (CS Lab)',        @BuildA),
    (@Room102, N'A-102 (IT Lab)',        @BuildA),
    (@Room201, N'B-201 (Business Hall)', @BuildB),
    (@Room202, N'B-202 (Language Lab)',  @BuildB)
) AS v([Id],[Name],[BldId])
WHERE NOT EXISTS (SELECT 1 FROM [rooms] WHERE [Id] = v.[Id]);

-- Timetables (one per dept per semester Spring 2026)
DECLARE @TT01 UNIQUEIDENTIFIER = CAST(N'TT000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @TT02 UNIQUEIDENTIFIER = CAST(N'TT000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @TT03 UNIQUEIDENTIFIER = CAST(N'TT000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);

INSERT INTO [timetables] ([Id],[DepartmentId],[SemesterId],[AcademicProgramId],[SemesterNumber],[IsPublished],[PublishedAt],[EffectiveDate],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT v.[Id], v.[DeptId], @SemSpr26, v.[ProgId], 2, 1, '2026-02-01 00:00:00', '2026-02-01', @Now, @Now, 0
FROM (VALUES
    (@TT01, @DeptIT,   @ProgBSCS),
    (@TT02, @DeptBiz,  @ProgBBA ),
    (@TT03, @DeptLang, @ProgEng )
) AS v([Id],[DeptId],[ProgId])
WHERE NOT EXISTS (SELECT 1 FROM [timetables] WHERE [Id] = v.[Id]);

-- Timetable entries
INSERT INTO [timetable_entries]
    ([Id],[TimetableId],[DayOfWeek],[StartTime],[EndTime],[SubjectName],[RoomNumber],[FacultyName],
     [RoomId],[BuildingId],[CourseId],[FacultyUserId],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[TtId], v.[Day], v.[Start], v.[End], v.[Subject], v.[RoomNo], v.[FacName],
    v.[RoomId], v.[BldId], v.[CourseId], v.[FacId], @Now, @Now
FROM (VALUES
    -- IT timetable
    (@TT01, 1, '08:00', '09:30', N'Operating Systems',    N'A-101', N'Dr. Ahmed', @Room101, @BuildA, @CIT04, @UFacAhmed),
    (@TT01, 1, '10:00', '11:30', N'Computer Networks',    N'A-102', N'Dr. Sara',  @Room102, @BuildA, @CIT05, @UFacSara),
    (@TT01, 3, '08:00', '09:30', N'Software Engineering', N'A-101', N'Mr. Ali',   @Room101, @BuildA, @CIT06, @UFacAli),
    (@TT01, 3, '10:00', '11:30', N'Operating Systems',    N'A-101', N'Dr. Ahmed', @Room101, @BuildA, @CIT04, @UFacAhmed),
    -- Business timetable
    (@TT02, 2, '09:00', '10:30', N'Business Statistics',  N'B-201', N'Prof. Khan',@Room201, @BuildB, @CBZ04, @UFacKhan),
    (@TT02, 2, '11:00', '12:30', N'Strategic Management', N'B-201', N'Dr. Naeem', @Room201, @BuildB, @CBZ05, @UFacNaeem),
    (@TT02, 4, '09:00', '10:30', N'Entrepreneurship',     N'B-201', N'Ms. Zara',  @Room201, @BuildB, @CBZ06, @UFacZara),
    -- Language timetable
    (@TT03, 1, '13:00', '14:30', N'English Literature',   N'B-202', N'Mr. Imran', @Room202, @BuildB, @CLG04, @UFacImran),
    (@TT03, 3, '13:00', '14:30', N'Arabic Language II',   N'B-202', N'Dr. Fatima',@Room202, @BuildB, @CLG02, @UFacFatima)
) AS v([TtId],[Day],[Start],[End],[Subject],[RoomNo],[FacName],[RoomId],[BldId],[CourseId],[FacId])
WHERE NOT EXISTS (
    SELECT 1 FROM [timetable_entries]
    WHERE [TimetableId]=v.[TtId] AND [DayOfWeek]=v.[Day] AND [StartTime]=v.[Start]
);

-- ═════════════════════════════════════════════════════════════
-- R.  NOTIFICATIONS
-- ═════════════════════════════════════════════════════════════
DECLARE @Notif01 UNIQUEIDENTIFIER = CAST(N'NF000000-0000-0000-0000-000000000001' AS UNIQUEIDENTIFIER);
DECLARE @Notif02 UNIQUEIDENTIFIER = CAST(N'NF000000-0000-0000-0000-000000000002' AS UNIQUEIDENTIFIER);
DECLARE @Notif03 UNIQUEIDENTIFIER = CAST(N'NF000000-0000-0000-0000-000000000003' AS UNIQUEIDENTIFIER);
DECLARE @Notif04 UNIQUEIDENTIFIER = CAST(N'NF000000-0000-0000-0000-000000000004' AS UNIQUEIDENTIFIER);

INSERT INTO [notifications]
    ([Id],[Title],[Body],[Type],[SenderUserId],[IsSystemGenerated],[IsActive],[CreatedAt],[UpdatedAt])
SELECT v.[Id], v.[Title], v.[Body], v.[Type], v.[Sender], v.[IsSys], 1, @Now, @Now
FROM (VALUES
    (@Notif01,
     N'Spring 2026 Semester Begins',
     N'Welcome to Spring 2026! Classes begin on 1st February. Please check your timetable and enroll in your courses. Contact your department admin for any enrollment issues.',
     N'Academic', @USuperAdmin, CAST(1 AS BIT)),
    (@Notif02,
     N'Assignment Submission Reminder — OS Quiz 1',
     N'This is a reminder that OS Quiz 1 is available from 1st March and closes on 7th March. Please ensure you attempt it before the deadline.',
     N'Assignment', @UFacAhmed, CAST(0 AS BIT)),
    (@Notif03,
     N'Fall 2025 Results Published',
     N'Final results for Fall 2025 semester have been published. You can view your results in the Results section of the portal.',
     N'Results', @USuperAdmin, CAST(1 AS BIT)),
    (@Notif04,
     N'Fee Payment Reminder',
     N'This is a reminder to submit your semester fee payment receipt by 28th February 2026 to avoid late payment charges.',
     N'Finance', @UAdminIT, CAST(0 AS BIT))
) AS v([Id],[Title],[Body],[Type],[Sender],[IsSys])
WHERE NOT EXISTS (SELECT 1 FROM [notifications] WHERE [Id] = v.[Id]);

-- Notification recipients — all students get notifications 1,3; IT students get 2; all get 4
INSERT INTO [notification_recipients]
    ([Id],[NotificationId],[RecipientUserId],[IsRead],[ReadAt],[CreatedAt],[UpdatedAt])
SELECT NEWID(), v.[NotifId], v.[UserId], 0, NULL, @Now, @Now
FROM (VALUES
    -- Notif01: semester announcement — all students
    (@Notif01, @UStud01),(@Notif01, @UStud02),(@Notif01, @UStud03),(@Notif01, @UStud04),
    (@Notif01, @UStud05),(@Notif01, @UStud06),(@Notif01, @UStud07),(@Notif01, @UStud08),
    (@Notif01, @UStud09),(@Notif01, @UStud10),(@Notif01, @UStud11),(@Notif01, @UStud12),
    (@Notif01, @UStud13),(@Notif01, @UStud14),(@Notif01, @UStud15),(@Notif01, @UStud16),
    (@Notif01, @UStud17),(@Notif01, @UStud18),
    -- Notif02: OS quiz reminder — IT students only
    (@Notif02, @UStud01),(@Notif02, @UStud02),(@Notif02, @UStud03),(@Notif02, @UStud04),
    (@Notif02, @UStud05),(@Notif02, @UStud06),
    -- Notif03: results — all students
    (@Notif03, @UStud01),(@Notif03, @UStud02),(@Notif03, @UStud03),(@Notif03, @UStud04),
    (@Notif03, @UStud07),(@Notif03, @UStud08),(@Notif03, @UStud09),
    (@Notif03, @UStud13),(@Notif03, @UStud14),
    -- Notif04: fee reminder — all students
    (@Notif04, @UStud01),(@Notif04, @UStud02),(@Notif04, @UStud07),(@Notif04, @UStud08),
    (@Notif04, @UStud13),(@Notif04, @UStud14)
) AS v([NotifId],[UserId])
WHERE NOT EXISTS (
    SELECT 1 FROM [notification_recipients]
    WHERE [NotificationId]=v.[NotifId] AND [RecipientUserId]=v.[UserId]
);

-- ═════════════════════════════════════════════════════════════
-- S.  PAYMENT RECEIPTS
-- ═════════════════════════════════════════════════════════════
INSERT INTO [payment_receipts]
    ([Id],[StudentProfileId],[CreatedByUserId],[Status],[Amount],[Description],
     [DueDate],[ProofOfPaymentPath],[ProofUploadedAt],[ConfirmedByUserId],[ConfirmedAt],
     [Notes],[CreatedAt],[UpdatedAt],[IsDeleted])
SELECT NEWID(), v.[SpId], @UAdminIT, v.[Status], v.[Amount], v.[Desc],
    v.[DueDate], v.[Proof], v.[UploadedAt], v.[ConfBy], v.[ConfAt], NULL, @Now, @Now, 0
FROM (VALUES
    (@SP01, N'Paid',    CAST(15000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', N'/uploads/receipts/sp01-spr26.jpg', '2026-02-20 10:00:00', @UAdminIT, '2026-02-21 09:00:00'),
    (@SP02, N'Paid',    CAST(15000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', N'/uploads/receipts/sp02-spr26.jpg', '2026-02-22 11:00:00', @UAdminIT, '2026-02-23 09:00:00'),
    (@SP03, N'Pending', CAST(15000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', NULL, NULL, NULL, NULL),
    (@SP07, N'Paid',    CAST(18000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', N'/uploads/receipts/sp07-spr26.jpg', '2026-02-19 14:00:00', @UAdminBiz,'2026-02-20 09:00:00'),
    (@SP08, N'Pending', CAST(18000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', NULL, NULL, NULL, NULL),
    (@SP13, N'Paid',    CAST(12000 AS DECIMAL(10,2)), N'Spring 2026 Tuition Fee', '2026-02-28', N'/uploads/receipts/sp13-spr26.jpg', '2026-02-21 12:00:00', @UAdminLang,'2026-02-22 09:00:00')
) AS v([SpId],[Status],[Amount],[Desc],[DueDate],[Proof],[UploadedAt],[ConfBy],[ConfAt])
WHERE NOT EXISTS (
    SELECT 1 FROM [payment_receipts]
    WHERE [StudentProfileId]=v.[SpId] AND [Description]=v.[Desc]
);

PRINT '✓ Full dummy data seed complete.';
PRINT '';
PRINT '  Departments : IT | Business | Languages';
PRINT '  Programs    : BSCS, BSIT | BBA, MBA | BA-Arabic, BA-English, BA-Chinese';
PRINT '  Semesters   : Fall 2025 (closed) | Spring 2026 (active) | Fall 2026 (future)';
PRINT '  Users       : 1 SuperAdmin + 3 Admins + 9 Faculty + 18 Students';
PRINT '';
PRINT '  Password for ALL accounts: run .\Scripts\GenerateTestHashes.ps1';
PRINT '';
PRINT '  Key login accounts:';
PRINT '    superadmin   / superadmin@edusphere.local';
PRINT '    admin.it     / admin.it@edusphere.local';
PRINT '    admin.biz    / admin.biz@edusphere.local';
PRINT '    admin.lang   / admin.lang@edusphere.local';
PRINT '    dr.ahmed     / ahmed@edusphere.local      (Faculty — IT)';
PRINT '    prof.khan    / khan@edusphere.local       (Faculty — Business)';
PRINT '    dr.fatima    / fatima@edusphere.local     (Faculty — Languages)';
PRINT '    s.aslam      / aslam@student.edusphere.local   (Student — IT)';
PRINT '    s.nadia      / nadia@student.edusphere.local   (Student — Business)';
PRINT '    s.usman      / usman@student.edusphere.local   (Student — Languages)';
GO
