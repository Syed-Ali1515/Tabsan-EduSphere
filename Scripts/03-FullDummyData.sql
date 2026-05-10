SET NOCOUNT ON;
SET XACT_ABORT ON;

/*
  Full dummy data script for demos.
  Prerequisite: run 01-Schema-Current.sql and 02-Seed-Core.sql first.

  NOTE:
  - Replace @PwdHash with a valid hash produced by your app hasher.
  - This script is idempotent (NOT EXISTS checks + stable GUID keys).
*/

BEGIN TRANSACTION;

DECLARE @Now DATETIME2 = SYSUTCDATETIME();
DECLARE @PwdHash NVARCHAR(512) = N'REPLACE_WITH_VALID_HASH';

DECLARE @RoleSuperAdmin INT = (SELECT TOP 1 [Id] FROM [roles] WHERE [Name] = N'SuperAdmin');
DECLARE @RoleAdmin INT = (SELECT TOP 1 [Id] FROM [roles] WHERE [Name] = N'Admin');
DECLARE @RoleFaculty INT = (SELECT TOP 1 [Id] FROM [roles] WHERE [Name] = N'Faculty');
DECLARE @RoleStudent INT = (SELECT TOP 1 [Id] FROM [roles] WHERE [Name] = N'Student');

IF @RoleSuperAdmin IS NULL OR @RoleAdmin IS NULL OR @RoleFaculty IS NULL OR @RoleStudent IS NULL
BEGIN
    THROW 51000, 'Roles not found. Run 02-Seed-Core.sql first.', 1;
END

/* 1) Departments */
DECLARE @Departments TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(200), Code NVARCHAR(20));
INSERT INTO @Departments (Id, Name, Code) VALUES
('11111111-1111-1111-1111-111111111111', N'Information Technology', N'IT'),
('11111111-1111-1111-1111-111111111112', N'Business', N'BUS'),
('11111111-1111-1111-1111-111111111113', N'Languages', N'LANG');

INSERT INTO [departments] ([Id], [Name], [Code], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT d.Id, d.Name, d.Code, 1, @Now, NULL, 0, NULL
FROM @Departments d
WHERE NOT EXISTS (SELECT 1 FROM [departments] x WHERE x.[Id] = d.Id);

/* 2) Programs */
DECLARE @Programs TABLE (Id UNIQUEIDENTIFIER, DepartmentId UNIQUEIDENTIFIER, Name NVARCHAR(200), Code NVARCHAR(20), TotalSemesters INT);
INSERT INTO @Programs VALUES
('22222222-2222-2222-2222-222222222211', '11111111-1111-1111-1111-111111111111', N'BS Computer Science', N'BSCS', 8),
('22222222-2222-2222-2222-222222222212', '11111111-1111-1111-1111-111111111111', N'BS Information Technology', N'BSIT', 8),
('22222222-2222-2222-2222-222222222213', '11111111-1111-1111-1111-111111111112', N'BBA', N'BBA', 8),
('22222222-2222-2222-2222-222222222214', '11111111-1111-1111-1111-111111111112', N'MBA', N'MBA', 4),
('22222222-2222-2222-2222-222222222215', '11111111-1111-1111-1111-111111111113', N'BA English', N'BAENG', 8),
('22222222-2222-2222-2222-222222222216', '11111111-1111-1111-1111-111111111113', N'BA Arabic', N'BAARB', 8);

INSERT INTO [academic_programs] ([Id], [Name], [Code], [DepartmentId], [TotalSemesters], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT p.Id, p.Name, p.Code, p.DepartmentId, p.TotalSemesters, 1, @Now, NULL, 0, NULL
FROM @Programs p
WHERE NOT EXISTS (SELECT 1 FROM [academic_programs] x WHERE x.[Id] = p.Id);

/* 3) Semesters */
DECLARE @Semesters TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(100), StartDate DATETIME2, EndDate DATETIME2, IsClosed BIT);
INSERT INTO @Semesters VALUES
('33333333-3333-3333-3333-333333333331', N'Fall 2025', '2025-08-15', '2025-12-31', 1),
('33333333-3333-3333-3333-333333333332', N'Spring 2026', '2026-01-15', '2026-06-15', 0),
('33333333-3333-3333-3333-333333333333', N'Fall 2026', '2026-08-15', '2026-12-31', 0);

INSERT INTO [semesters] ([Id], [Name], [StartDate], [EndDate], [IsClosed], [ClosedAt], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT s.Id, s.Name, s.StartDate, s.EndDate, s.IsClosed,
       CASE WHEN s.IsClosed = 1 THEN DATEADD(day, 1, s.EndDate) ELSE NULL END,
       @Now, NULL, 0, NULL
FROM @Semesters s
WHERE NOT EXISTS (SELECT 1 FROM [semesters] x WHERE x.[Id] = s.Id);

/* 4) Users (SuperAdmin/Admin/Faculty/Student) */
DECLARE @Users TABLE (
    Id UNIQUEIDENTIFIER,
    Username NVARCHAR(100),
    Email NVARCHAR(256),
    RoleId INT,
    DepartmentId UNIQUEIDENTIFIER NULL
);

INSERT INTO @Users VALUES
('66666666-6666-6666-6666-666666666601', N'superadmin', N'superadmin@demo.local', @RoleSuperAdmin, NULL),
('66666666-6666-6666-6666-666666666611', N'admin.it', N'admin.it@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111111'),
('66666666-6666-6666-6666-666666666612', N'admin.bus', N'admin.bus@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111112'),
('66666666-6666-6666-6666-666666666613', N'admin.lang', N'admin.lang@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111113'),
('66666666-6666-6666-6666-666666666621', N'faculty.it.1', N'faculty.it.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111111'),
('66666666-6666-6666-6666-666666666622', N'faculty.it.2', N'faculty.it.2@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111111'),
('66666666-6666-6666-6666-666666666623', N'faculty.bus.1', N'faculty.bus.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111112'),
('66666666-6666-6666-6666-666666666624', N'faculty.lang.1', N'faculty.lang.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111113'),
('66666666-6666-6666-6666-666666666631', N'student.it.1', N'student.it.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111111'),
('66666666-6666-6666-6666-666666666632', N'student.it.2', N'student.it.2@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111111'),
('66666666-6666-6666-6666-666666666633', N'student.bus.1', N'student.bus.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111112'),
('66666666-6666-6666-6666-666666666634', N'student.lang.1', N'student.lang.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111113');

INSERT INTO [users] ([Id], [Username], [Email], [PasswordHash], [RoleId], [DepartmentId], [IsActive], [LastLoginAt], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT u.Id, u.Username, u.Email, @PwdHash, u.RoleId, u.DepartmentId, 1, NULL, @Now, NULL, 0, NULL
FROM @Users u
WHERE NOT EXISTS (SELECT 1 FROM [users] x WHERE x.[Id] = u.Id);

/* 5) Student profiles */
DECLARE @StudentProfiles TABLE (
    Id UNIQUEIDENTIFIER,
    UserId UNIQUEIDENTIFIER,
    RegistrationNumber NVARCHAR(50),
    ProgramId UNIQUEIDENTIFIER,
    DepartmentId UNIQUEIDENTIFIER,
    CurrentSemesterNumber INT
);

INSERT INTO @StudentProfiles VALUES
('77777777-7777-7777-7777-777777777731', '66666666-6666-6666-6666-666666666631', N'2026-IT-0001', '22222222-2222-2222-2222-222222222211', '11111111-1111-1111-1111-111111111111', 2),
('77777777-7777-7777-7777-777777777732', '66666666-6666-6666-6666-666666666632', N'2026-IT-0002', '22222222-2222-2222-2222-222222222212', '11111111-1111-1111-1111-111111111111', 2),
('77777777-7777-7777-7777-777777777733', '66666666-6666-6666-6666-666666666633', N'2026-BUS-0001', '22222222-2222-2222-2222-222222222213', '11111111-1111-1111-1111-111111111112', 2),
('77777777-7777-7777-7777-777777777734', '66666666-6666-6666-6666-666666666634', N'2026-LANG-0001', '22222222-2222-2222-2222-222222222215', '11111111-1111-1111-1111-111111111113', 2);

INSERT INTO [student_profiles] ([Id], [UserId], [RegistrationNumber], [ProgramId], [DepartmentId], [AdmissionDate], [Cgpa], [CurrentSemesterNumber], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT sp.Id, sp.UserId, sp.RegistrationNumber, sp.ProgramId, sp.DepartmentId, '2026-01-15', 3.20, sp.CurrentSemesterNumber, @Now, NULL, 0, NULL
FROM @StudentProfiles sp
WHERE NOT EXISTS (SELECT 1 FROM [student_profiles] x WHERE x.[Id] = sp.Id);

/* 6) Courses */
DECLARE @Courses TABLE (Id UNIQUEIDENTIFIER, DepartmentId UNIQUEIDENTIFIER, Title NVARCHAR(200), Code NVARCHAR(20), CreditHours INT);
INSERT INTO @Courses VALUES
('44444444-4444-4444-4444-444444444401', '11111111-1111-1111-1111-111111111111', N'Programming Fundamentals', N'CS-101', 3),
('44444444-4444-4444-4444-444444444402', '11111111-1111-1111-1111-111111111111', N'Data Structures', N'CS-201', 3),
('44444444-4444-4444-4444-444444444403', '11111111-1111-1111-1111-111111111111', N'Software Engineering', N'CS-301', 3),
('44444444-4444-4444-4444-444444444404', '11111111-1111-1111-1111-111111111112', N'Principles of Management', N'BUS-101', 3),
('44444444-4444-4444-4444-444444444405', '11111111-1111-1111-1111-111111111112', N'Marketing Fundamentals', N'BUS-201', 3),
('44444444-4444-4444-4444-444444444406', '11111111-1111-1111-1111-111111111112', N'Financial Accounting', N'BUS-301', 3),
('44444444-4444-4444-4444-444444444407', '11111111-1111-1111-1111-111111111113', N'English Composition', N'ENG-101', 3),
('44444444-4444-4444-4444-444444444408', '11111111-1111-1111-1111-111111111113', N'Applied Linguistics', N'ENG-201', 3),
('44444444-4444-4444-4444-444444444409', '11111111-1111-1111-1111-111111111113', N'Arabic Literature', N'ARB-301', 3);

INSERT INTO [courses] ([Id], [Title], [Code], [CreditHours], [DepartmentId], [IsActive], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT c.Id, c.Title, c.Code, c.CreditHours, c.DepartmentId, 1, @Now, NULL, 0, NULL
FROM @Courses c
WHERE NOT EXISTS (SELECT 1 FROM [courses] x WHERE x.[Id] = c.Id);

/* 7) Course offerings (Spring 2026) */
DECLARE @SpringSemester UNIQUEIDENTIFIER = '33333333-3333-3333-3333-333333333332';
DECLARE @Offerings TABLE (Id UNIQUEIDENTIFIER, CourseId UNIQUEIDENTIFIER, FacultyUserId UNIQUEIDENTIFIER, MaxEnrollment INT);
INSERT INTO @Offerings VALUES
('55555555-5555-5555-5555-555555555501', '44444444-4444-4444-4444-444444444401', '66666666-6666-6666-6666-666666666621', 60),
('55555555-5555-5555-5555-555555555502', '44444444-4444-4444-4444-444444444402', '66666666-6666-6666-6666-666666666622', 60),
('55555555-5555-5555-5555-555555555503', '44444444-4444-4444-4444-444444444403', '66666666-6666-6666-6666-666666666621', 50),
('55555555-5555-5555-5555-555555555504', '44444444-4444-4444-4444-444444444404', '66666666-6666-6666-6666-666666666623', 55),
('55555555-5555-5555-5555-555555555505', '44444444-4444-4444-4444-444444444405', '66666666-6666-6666-6666-666666666623', 55),
('55555555-5555-5555-5555-555555555506', '44444444-4444-4444-4444-444444444407', '66666666-6666-6666-6666-666666666624', 45);

INSERT INTO [course_offerings] ([Id], [CourseId], [SemesterId], [FacultyUserId], [MaxEnrollment], [IsOpen], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT o.Id, o.CourseId, @SpringSemester, o.FacultyUserId, o.MaxEnrollment, 1, @Now, NULL, 0, NULL
FROM @Offerings o
WHERE NOT EXISTS (SELECT 1 FROM [course_offerings] x WHERE x.[Id] = o.Id);

/* 8) Enrollments */
DECLARE @Enrollments TABLE (Id UNIQUEIDENTIFIER, StudentProfileId UNIQUEIDENTIFIER, CourseOfferingId UNIQUEIDENTIFIER, Status NVARCHAR(50));
INSERT INTO @Enrollments VALUES
('88888888-8888-8888-8888-888888888801', '77777777-7777-7777-7777-777777777731', '55555555-5555-5555-5555-555555555501', N'Enrolled'),
('88888888-8888-8888-8888-888888888802', '77777777-7777-7777-7777-777777777731', '55555555-5555-5555-5555-555555555502', N'Enrolled'),
('88888888-8888-8888-8888-888888888803', '77777777-7777-7777-7777-777777777732', '55555555-5555-5555-5555-555555555501', N'Enrolled'),
('88888888-8888-8888-8888-888888888804', '77777777-7777-7777-7777-777777777732', '55555555-5555-5555-5555-555555555503', N'Enrolled'),
('88888888-8888-8888-8888-888888888805', '77777777-7777-7777-7777-777777777733', '55555555-5555-5555-5555-555555555504', N'Enrolled'),
('88888888-8888-8888-8888-888888888806', '77777777-7777-7777-7777-777777777733', '55555555-5555-5555-5555-555555555505', N'Enrolled'),
('88888888-8888-8888-8888-888888888807', '77777777-7777-7777-7777-777777777734', '55555555-5555-5555-5555-555555555506', N'Enrolled');

INSERT INTO [enrollments] ([Id], [StudentProfileId], [CourseOfferingId], [EnrolledAt], [DroppedAt], [Status], [CreatedAt], [UpdatedAt])
SELECT e.Id, e.StudentProfileId, e.CourseOfferingId, @Now, NULL, e.Status, @Now, NULL
FROM @Enrollments e
WHERE NOT EXISTS (SELECT 1 FROM [enrollments] x WHERE x.[Id] = e.Id);

/* 9) Assignments */
DECLARE @Assignments TABLE (Id UNIQUEIDENTIFIER, CourseOfferingId UNIQUEIDENTIFIER, Title NVARCHAR(300), DueDate DATETIME2, MaxMarks DECIMAL(8,2));
INSERT INTO @Assignments VALUES
('99999999-9999-9999-9999-999999999901', '55555555-5555-5555-5555-555555555501', N'PF Assignment 1', DATEADD(day, 10, @Now), 20.00),
('99999999-9999-9999-9999-999999999902', '55555555-5555-5555-5555-555555555502', N'DS Assignment 1', DATEADD(day, 12, @Now), 25.00),
('99999999-9999-9999-9999-999999999903', '55555555-5555-5555-5555-555555555504', N'Mgmt Case Study', DATEADD(day, 14, @Now), 30.00),
('99999999-9999-9999-9999-999999999904', '55555555-5555-5555-5555-555555555506', N'Essay Draft', DATEADD(day, 9, @Now), 15.00);

INSERT INTO [assignments] ([Id], [CourseOfferingId], [Title], [Description], [DueDate], [MaxMarks], [IsPublished], [PublishedAt], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT a.Id, a.CourseOfferingId, a.Title, N'Demo assignment data', a.DueDate, a.MaxMarks, 1, @Now, @Now, NULL, 0, NULL
FROM @Assignments a
WHERE NOT EXISTS (SELECT 1 FROM [assignments] x WHERE x.[Id] = a.Id);

/* 10) Assignment submissions */
DECLARE @Submissions TABLE (Id UNIQUEIDENTIFIER, AssignmentId UNIQUEIDENTIFIER, StudentProfileId UNIQUEIDENTIFIER, Marks DECIMAL(8,2), GradedByUserId UNIQUEIDENTIFIER);
INSERT INTO @Submissions VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa01', '99999999-9999-9999-9999-999999999901', '77777777-7777-7777-7777-777777777731', 17.00, '66666666-6666-6666-6666-666666666621'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa02', '99999999-9999-9999-9999-999999999901', '77777777-7777-7777-7777-777777777732', 16.00, '66666666-6666-6666-6666-666666666621'),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaa03', '99999999-9999-9999-9999-999999999903', '77777777-7777-7777-7777-777777777733', 24.00, '66666666-6666-6666-6666-666666666623');

INSERT INTO [assignment_submissions] ([Id], [AssignmentId], [StudentProfileId], [FileUrl], [TextContent], [SubmittedAt], [MarksAwarded], [Feedback], [GradedAt], [GradedByUserId], [Status], [CreatedAt], [UpdatedAt])
SELECT s.Id, s.AssignmentId, s.StudentProfileId, NULL, N'Demo answer content', @Now, s.Marks, N'Good work', @Now, s.GradedByUserId, N'Graded', @Now, NULL
FROM @Submissions s
WHERE NOT EXISTS (SELECT 1 FROM [assignment_submissions] x WHERE x.[Id] = s.Id);

/* 11) Attendance records */
DECLARE @Attendance TABLE (Id UNIQUEIDENTIFIER, StudentProfileId UNIQUEIDENTIFIER, CourseOfferingId UNIQUEIDENTIFIER, [Date] DATETIME2, [Status] NVARCHAR(20), MarkedByUserId UNIQUEIDENTIFIER);
INSERT INTO @Attendance VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb01', '77777777-7777-7777-7777-777777777731', '55555555-5555-5555-5555-555555555501', CAST(@Now AS date), N'Present', '66666666-6666-6666-6666-666666666621'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb02', '77777777-7777-7777-7777-777777777732', '55555555-5555-5555-5555-555555555501', CAST(@Now AS date), N'Late', '66666666-6666-6666-6666-666666666621'),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbb03', '77777777-7777-7777-7777-777777777733', '55555555-5555-5555-5555-555555555504', CAST(@Now AS date), N'Present', '66666666-6666-6666-6666-666666666623');

INSERT INTO [attendance_records] ([Id], [StudentProfileId], [CourseOfferingId], [Date], [Status], [MarkedByUserId], [Remarks], [CreatedAt], [UpdatedAt])
SELECT a.Id, a.StudentProfileId, a.CourseOfferingId, a.[Date], a.[Status], a.MarkedByUserId, N'Demo attendance', @Now, NULL
FROM @Attendance a
WHERE NOT EXISTS (SELECT 1 FROM [attendance_records] x WHERE x.[Id] = a.Id);

/* 12) Notifications and recipients */
DECLARE @Notifications TABLE (Id UNIQUEIDENTIFIER, Title NVARCHAR(300), Body NVARCHAR(4000), [Type] NVARCHAR(50), SenderUserId UNIQUEIDENTIFIER);
INSERT INTO @Notifications VALUES
('cccccccc-cccc-cccc-cccc-cccccccccc01', N'Welcome to Spring 2026', N'Classes are now active in the portal.', N'Academic', '66666666-6666-6666-6666-666666666611'),
('cccccccc-cccc-cccc-cccc-cccccccccc02', N'Assignment Reminder', N'Submit assignments before due date.', N'Reminder', '66666666-6666-6666-6666-666666666621'),
('cccccccc-cccc-cccc-cccc-cccccccccc03', N'Attendance Policy', N'Attendance below threshold will be flagged.', N'Policy', '66666666-6666-6666-6666-666666666612');

INSERT INTO [notifications] ([Id], [Title], [Body], [Type], [SenderUserId], [IsSystemGenerated], [IsActive], [CreatedAt], [UpdatedAt])
SELECT n.Id, n.Title, n.Body, n.[Type], n.SenderUserId, 0, 1, @Now, NULL
FROM @Notifications n
WHERE NOT EXISTS (SELECT 1 FROM [notifications] x WHERE x.[Id] = n.Id);

DECLARE @Recipients TABLE (Id UNIQUEIDENTIFIER, NotificationId UNIQUEIDENTIFIER, RecipientUserId UNIQUEIDENTIFIER, IsRead BIT);
INSERT INTO @Recipients VALUES
('dddddddd-dddd-dddd-dddd-dddddddddd01', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '66666666-6666-6666-6666-666666666631', 0),
('dddddddd-dddd-dddd-dddd-dddddddddd02', 'cccccccc-cccc-cccc-cccc-cccccccccc01', '66666666-6666-6666-6666-666666666632', 0),
('dddddddd-dddd-dddd-dddd-dddddddddd03', 'cccccccc-cccc-cccc-cccc-cccccccccc02', '66666666-6666-6666-6666-666666666631', 1),
('dddddddd-dddd-dddd-dddd-dddddddddd04', 'cccccccc-cccc-cccc-cccc-cccccccccc03', '66666666-6666-6666-6666-666666666633', 0),
('dddddddd-dddd-dddd-dddd-dddddddddd05', 'cccccccc-cccc-cccc-cccc-cccccccccc03', '66666666-6666-6666-6666-666666666634', 0);

INSERT INTO [notification_recipients] ([Id], [NotificationId], [RecipientUserId], [IsRead], [ReadAt], [CreatedAt], [UpdatedAt])
SELECT r.Id, r.NotificationId, r.RecipientUserId, r.IsRead,
       CASE WHEN r.IsRead = 1 THEN @Now ELSE NULL END,
       @Now, NULL
FROM @Recipients r
WHERE NOT EXISTS (SELECT 1 FROM [notification_recipients] x WHERE x.[Id] = r.Id);

COMMIT TRANSACTION;

PRINT 'Full dummy demo data seeding completed.';
