SET ANSI_NULLS ON;
GO

SET QUOTED_IDENTIFIER ON;
GO

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
    RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting dummy data script.', 16, 1);
    RETURN;
END;
GO

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
    RAISERROR('Roles not found. Run 02-Seed-Core.sql first.', 16, 1);
    ROLLBACK TRANSACTION;
    RETURN;
END;

/* 0) Demo metadata table (custom object requested for demos) */
IF OBJECT_ID(N'[Tabsan-EduSphere]') IS NOT NULL
BEGIN
    INSERT INTO [Tabsan-EduSphere] ([Id], [DemoKey], [DemoValue], [CreatedAt], [UpdatedAt])
    SELECT '10101010-1010-1010-1010-101010101010', N'DemoDatasetVersion', N'FullDummyData-v2', @Now, NULL
    WHERE NOT EXISTS (SELECT 1 FROM [Tabsan-EduSphere] x WHERE x.[DemoKey] = N'DemoDatasetVersion');

    INSERT INTO [Tabsan-EduSphere] ([Id], [DemoKey], [DemoValue], [CreatedAt], [UpdatedAt])
    SELECT '10101010-1010-1010-1010-101010101011', N'DemoSeededAtUtc', CONVERT(NVARCHAR(40), @Now, 127), @Now, NULL
    WHERE NOT EXISTS (SELECT 1 FROM [Tabsan-EduSphere] x WHERE x.[DemoKey] = N'DemoSeededAtUtc');
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
    DepartmentId UNIQUEIDENTIFIER NULL,
    InstitutionType INT NULL
);

INSERT INTO @Users VALUES
('66666666-6666-6666-6666-666666666601', N'superadmin', N'superadmin@demo.local', @RoleSuperAdmin, NULL, NULL),
('66666666-6666-6666-6666-666666666611', N'admin.it', N'admin.it@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111111', NULL),
('66666666-6666-6666-6666-666666666612', N'admin.bus', N'admin.bus@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111112', NULL),
('66666666-6666-6666-6666-666666666613', N'admin.lang', N'admin.lang@demo.local', @RoleAdmin, '11111111-1111-1111-1111-111111111113', NULL),
('66666666-6666-6666-6666-666666666621', N'faculty.it.1', N'faculty.it.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111111', NULL),
('66666666-6666-6666-6666-666666666622', N'faculty.it.2', N'faculty.it.2@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111111', NULL),
('66666666-6666-6666-6666-666666666623', N'faculty.bus.1', N'faculty.bus.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111112', NULL),
('66666666-6666-6666-6666-666666666624', N'faculty.lang.1', N'faculty.lang.1@demo.local', @RoleFaculty, '11111111-1111-1111-1111-111111111113', NULL),
('66666666-6666-6666-6666-666666666631', N'student.it.1', N'student.it.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111111', NULL),
('66666666-6666-6666-6666-666666666632', N'student.it.2', N'student.it.2@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111111', NULL),
('66666666-6666-6666-6666-666666666633', N'student.bus.1', N'student.bus.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111112', NULL),
('66666666-6666-6666-6666-666666666634', N'student.lang.1', N'student.lang.1@demo.local', @RoleStudent, '11111111-1111-1111-1111-111111111113', NULL);

INSERT INTO [users] ([Id], [Username], [Email], [PasswordHash], [RoleId], [DepartmentId], [InstitutionType], [IsActive], [LastLoginAt], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
SELECT u.Id, u.Username, u.Email, @PwdHash, u.RoleId, u.DepartmentId, u.InstitutionType, 1, NULL, @Now, NULL, 0, NULL
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

/* 12) Results */
IF OBJECT_ID(N'[results]') IS NOT NULL
BEGIN
    DECLARE @Results TABLE (
        Id UNIQUEIDENTIFIER,
        StudentProfileId UNIQUEIDENTIFIER,
        CourseOfferingId UNIQUEIDENTIFIER,
        ResultType NVARCHAR(450),
        MarksObtained DECIMAL(8,2),
        MaxMarks DECIMAL(8,2),
        PublishedByUserId UNIQUEIDENTIFIER
    );

    INSERT INTO @Results VALUES
    ('12121212-1212-1212-1212-121212121201', '77777777-7777-7777-7777-777777777731', '55555555-5555-5555-5555-555555555501', N'Final', 84.00, 100.00, '66666666-6666-6666-6666-666666666621'),
    ('12121212-1212-1212-1212-121212121202', '77777777-7777-7777-7777-777777777732', '55555555-5555-5555-5555-555555555501', N'Final', 79.00, 100.00, '66666666-6666-6666-6666-666666666621'),
    ('12121212-1212-1212-1212-121212121203', '77777777-7777-7777-7777-777777777733', '55555555-5555-5555-5555-555555555504', N'Final', 74.00, 100.00, '66666666-6666-6666-6666-666666666623'),
    ('12121212-1212-1212-1212-121212121204', '77777777-7777-7777-7777-777777777734', '55555555-5555-5555-5555-555555555506', N'Final', 81.00, 100.00, '66666666-6666-6666-6666-666666666624');

    INSERT INTO [results] ([Id], [StudentProfileId], [CourseOfferingId], [ResultType], [MarksObtained], [MaxMarks], [IsPublished], [PublishedAt], [PublishedByUserId], [CreatedAt], [UpdatedAt])
    SELECT r.Id, r.StudentProfileId, r.CourseOfferingId, r.ResultType, r.MarksObtained, r.MaxMarks, 1, @Now, r.PublishedByUserId, @Now, NULL
    FROM @Results r
    WHERE NOT EXISTS (SELECT 1 FROM [results] x WHERE x.[Id] = r.Id);
END

/* 13) Quizzes + questions + attempts + answers */
IF OBJECT_ID(N'[quizzes]') IS NOT NULL
BEGIN
    DECLARE @Quizzes TABLE (Id UNIQUEIDENTIFIER, CourseOfferingId UNIQUEIDENTIFIER, Title NVARCHAR(300), CreatedByUserId UNIQUEIDENTIFIER);
    INSERT INTO @Quizzes VALUES
    ('13131313-1313-1313-1313-131313131301', '55555555-5555-5555-5555-555555555501', N'PF Quiz 1', '66666666-6666-6666-6666-666666666621'),
    ('13131313-1313-1313-1313-131313131302', '55555555-5555-5555-5555-555555555504', N'Management Quiz 1', '66666666-6666-6666-6666-666666666623');

    INSERT INTO [quizzes] ([Id], [CourseOfferingId], [Title], [Instructions], [TimeLimitMinutes], [MaxAttempts], [AvailableFrom], [AvailableUntil], [IsPublished], [IsActive], [CreatedByUserId], [CreatedAt], [UpdatedAt])
    SELECT q.Id, q.CourseOfferingId, q.Title, N'Demo quiz instructions', 20, 1, DATEADD(day, -2, @Now), DATEADD(day, 7, @Now), 1, 1, q.CreatedByUserId, @Now, NULL
    FROM @Quizzes q
    WHERE NOT EXISTS (SELECT 1 FROM [quizzes] x WHERE x.[Id] = q.Id);

    DECLARE @QuizQuestions TABLE (Id UNIQUEIDENTIFIER, QuizId UNIQUEIDENTIFIER, [Text] NVARCHAR(2000), [Type] NVARCHAR(20), Marks DECIMAL(8,2), OrderIndex INT);
    INSERT INTO @QuizQuestions VALUES
    ('14141414-1414-1414-1414-141414141401', '13131313-1313-1313-1313-131313131301', N'Which keyword declares a class in C#?', N'MCQ', 5.00, 1),
    ('14141414-1414-1414-1414-141414141402', '13131313-1313-1313-1313-131313131301', N'What is encapsulation?', N'Short', 5.00, 2),
    ('14141414-1414-1414-1414-141414141403', '13131313-1313-1313-1313-131313131302', N'Management is both science and ____?', N'MCQ', 5.00, 1);

    INSERT INTO [quiz_questions] ([Id], [QuizId], [Text], [Type], [Marks], [OrderIndex], [CreatedAt], [UpdatedAt])
    SELECT qq.Id, qq.QuizId, qq.[Text], qq.[Type], qq.Marks, qq.OrderIndex, @Now, NULL
    FROM @QuizQuestions qq
    WHERE NOT EXISTS (SELECT 1 FROM [quiz_questions] x WHERE x.[Id] = qq.Id);

    DECLARE @QuizOptions TABLE (Id UNIQUEIDENTIFIER, QuizQuestionId UNIQUEIDENTIFIER, [Text] NVARCHAR(1000), IsCorrect BIT, OrderIndex INT);
    INSERT INTO @QuizOptions VALUES
    ('15151515-1515-1515-1515-151515151501', '14141414-1414-1414-1414-141414141401', N'class', 1, 1),
    ('15151515-1515-1515-1515-151515151502', '14141414-1414-1414-1414-141414141401', N'struct', 0, 2),
    ('15151515-1515-1515-1515-151515151503', '14141414-1414-1414-1414-141414141403', N'Art', 1, 1),
    ('15151515-1515-1515-1515-151515151504', '14141414-1414-1414-1414-141414141403', N'Luck', 0, 2);

    INSERT INTO [quiz_options] ([Id], [QuizQuestionId], [Text], [IsCorrect], [OrderIndex], [CreatedAt], [UpdatedAt])
    SELECT qo.Id, qo.QuizQuestionId, qo.[Text], qo.IsCorrect, qo.OrderIndex, @Now, NULL
    FROM @QuizOptions qo
    WHERE NOT EXISTS (SELECT 1 FROM [quiz_options] x WHERE x.[Id] = qo.Id);

    DECLARE @QuizAttempts TABLE (Id UNIQUEIDENTIFIER, QuizId UNIQUEIDENTIFIER, StudentProfileId UNIQUEIDENTIFIER, TotalScore DECIMAL(10,2));
    INSERT INTO @QuizAttempts VALUES
    ('16161616-1616-1616-1616-161616161601', '13131313-1313-1313-1313-131313131301', '77777777-7777-7777-7777-777777777731', 8.00),
    ('16161616-1616-1616-1616-161616161602', '13131313-1313-1313-1313-131313131302', '77777777-7777-7777-7777-777777777733', 4.00);

    INSERT INTO [quiz_attempts] ([Id], [QuizId], [StudentProfileId], [StartedAt], [FinishedAt], [Status], [TotalScore], [CreatedAt], [UpdatedAt])
    SELECT qa.Id, qa.QuizId, qa.StudentProfileId, DATEADD(minute, -20, @Now), @Now, N'Completed', qa.TotalScore, @Now, NULL
    FROM @QuizAttempts qa
    WHERE NOT EXISTS (SELECT 1 FROM [quiz_attempts] x WHERE x.[Id] = qa.Id);

    DECLARE @QuizAnswers TABLE (Id UNIQUEIDENTIFIER, QuizAttemptId UNIQUEIDENTIFIER, QuizQuestionId UNIQUEIDENTIFIER, SelectedOptionId UNIQUEIDENTIFIER NULL, TextResponse NVARCHAR(4000) NULL, MarksAwarded DECIMAL(8,2));
    INSERT INTO @QuizAnswers VALUES
    ('17171717-1717-1717-1717-171717171701', '16161616-1616-1616-1616-161616161601', '14141414-1414-1414-1414-141414141401', '15151515-1515-1515-1515-151515151501', NULL, 5.00),
    ('17171717-1717-1717-1717-171717171702', '16161616-1616-1616-1616-161616161601', '14141414-1414-1414-1414-141414141402', NULL, N'Encapsulation means wrapping data and behavior in one unit.', 3.00),
    ('17171717-1717-1717-1717-171717171703', '16161616-1616-1616-1616-161616161602', '14141414-1414-1414-1414-141414141403', '15151515-1515-1515-1515-151515151504', NULL, 0.00);

    INSERT INTO [quiz_answers] ([Id], [QuizAttemptId], [QuizQuestionId], [SelectedOptionId], [TextResponse], [MarksAwarded], [CreatedAt], [UpdatedAt])
    SELECT qa.Id, qa.QuizAttemptId, qa.QuizQuestionId, qa.SelectedOptionId, qa.TextResponse, qa.MarksAwarded, @Now, NULL
    FROM @QuizAnswers qa
    WHERE NOT EXISTS (SELECT 1 FROM [quiz_answers] x WHERE x.[Id] = qa.Id);
END

/* 14) Helpdesk demo tickets */
IF OBJECT_ID(N'[support_tickets]') IS NOT NULL
BEGIN
    DECLARE @SupportTickets TABLE (Id UNIQUEIDENTIFIER, SubmitterId UNIQUEIDENTIFIER, DepartmentId UNIQUEIDENTIFIER, Category INT, Subject NVARCHAR(300), Body NVARCHAR(4000), Status INT, AssignedToId UNIQUEIDENTIFIER NULL);
    INSERT INTO @SupportTickets VALUES
    ('18181818-1818-1818-1818-181818181801', '66666666-6666-6666-6666-666666666631', '11111111-1111-1111-1111-111111111111', 0, N'Cannot open assignment portal', N'Assignment page is not loading from student dashboard.', 0, '66666666-6666-6666-6666-666666666611'),
    ('18181818-1818-1818-1818-181818181802', '66666666-6666-6666-6666-666666666633', '11111111-1111-1111-1111-111111111112', 1, N'Result discrepancy query', N'Requesting review of posted marks.', 1, '66666666-6666-6666-6666-666666666612');

    INSERT INTO [support_tickets] ([Id], [SubmitterId], [DepartmentId], [Category], [Subject], [Body], [Status], [AssignedToId], [ResolvedAt], [ReopenWindowDays], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    SELECT t.Id, t.SubmitterId, t.DepartmentId, t.Category, t.Subject, t.Body, t.Status, t.AssignedToId,
           CASE WHEN t.Status = 1 THEN @Now ELSE NULL END,
           7, @Now, NULL, 0, NULL
    FROM @SupportTickets t
    WHERE NOT EXISTS (SELECT 1 FROM [support_tickets] x WHERE x.[Id] = t.Id);

    IF OBJECT_ID(N'[support_ticket_messages]') IS NOT NULL
    BEGIN
        DECLARE @SupportMessages TABLE (Id UNIQUEIDENTIFIER, TicketId UNIQUEIDENTIFIER, AuthorId UNIQUEIDENTIFIER, Body NVARCHAR(4000), IsInternalNote BIT);
        INSERT INTO @SupportMessages VALUES
        ('19191919-1919-1919-1919-191919191901', '18181818-1818-1818-1818-181818181801', '66666666-6666-6666-6666-666666666631', N'Issue started after latest portal login.', 0),
        ('19191919-1919-1919-1919-191919191902', '18181818-1818-1818-1818-181818181801', '66666666-6666-6666-6666-666666666611', N'Investigating logs and routing.', 1),
        ('19191919-1919-1919-1919-191919191903', '18181818-1818-1818-1818-181818181802', '66666666-6666-6666-6666-666666666612', N'Please share screenshot for result discrepancy.', 0);

        INSERT INTO [support_ticket_messages] ([Id], [TicketId], [AuthorId], [Body], [IsInternalNote], [CreatedAt], [UpdatedAt])
        SELECT m.Id, m.TicketId, m.AuthorId, m.Body, m.IsInternalNote, @Now, NULL
        FROM @SupportMessages m
        WHERE NOT EXISTS (SELECT 1 FROM [support_ticket_messages] x WHERE x.[Id] = m.Id);
    END
END

/* 15) LMS discussions */
IF OBJECT_ID(N'[discussion_threads]') IS NOT NULL
BEGIN
    DECLARE @DiscussionThreads TABLE (Id UNIQUEIDENTIFIER, OfferingId UNIQUEIDENTIFIER, Title NVARCHAR(500), AuthorId UNIQUEIDENTIFIER, IsPinned BIT, IsClosed BIT);
    INSERT INTO @DiscussionThreads VALUES
    ('20202020-2020-2020-2020-202020202001', '55555555-5555-5555-5555-555555555501', N'Week 2 Lab Preparation', '66666666-6666-6666-6666-666666666621', 1, 0),
    ('20202020-2020-2020-2020-202020202002', '55555555-5555-5555-5555-555555555504', N'Case Study References', '66666666-6666-6666-6666-666666666623', 0, 0);

    INSERT INTO [discussion_threads] ([Id], [OfferingId], [Title], [AuthorId], [IsPinned], [IsClosed], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
    SELECT t.Id, t.OfferingId, t.Title, t.AuthorId, t.IsPinned, t.IsClosed, @Now, NULL, 0, NULL
    FROM @DiscussionThreads t
    WHERE NOT EXISTS (SELECT 1 FROM [discussion_threads] x WHERE x.[Id] = t.Id);

    IF OBJECT_ID(N'[discussion_replies]') IS NOT NULL
    BEGIN
        DECLARE @DiscussionReplies TABLE (Id UNIQUEIDENTIFIER, ThreadId UNIQUEIDENTIFIER, AuthorId UNIQUEIDENTIFIER, Body NVARCHAR(MAX));
        INSERT INTO @DiscussionReplies VALUES
        ('21212121-2121-2121-2121-212121212101', '20202020-2020-2020-2020-202020202001', '66666666-6666-6666-6666-666666666631', N'Should we revise loops before the lab?'),
        ('21212121-2121-2121-2121-212121212102', '20202020-2020-2020-2020-202020202001', '66666666-6666-6666-6666-666666666621', N'Yes, focus on arrays and loops.'),
        ('21212121-2121-2121-2121-212121212103', '20202020-2020-2020-2020-202020202002', '66666666-6666-6666-6666-666666666633', N'I found a useful Harvard case study on strategy.');

        INSERT INTO [discussion_replies] ([Id], [ThreadId], [AuthorId], [Body], [CreatedAt], [UpdatedAt], [IsDeleted], [DeletedAt])
        SELECT r.Id, r.ThreadId, r.AuthorId, r.Body, @Now, NULL, 0, NULL
        FROM @DiscussionReplies r
        WHERE NOT EXISTS (SELECT 1 FROM [discussion_replies] x WHERE x.[Id] = r.Id);
    END
END

/* 16) Notifications and recipients */
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
