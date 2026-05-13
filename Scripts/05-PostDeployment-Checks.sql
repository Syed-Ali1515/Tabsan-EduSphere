SET NOCOUNT ON;

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
	RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting check script.', 16, 1);
	RETURN;
END;
GO

PRINT 'Running post-deployment checks...';

SELECT 'SchemaVersionCount' AS [CheckName], COUNT(1) AS [Value]
FROM __EFMigrationsHistory;

SELECT 'RoleCount' AS [CheckName], COUNT(1) AS [Value]
FROM roles;

SELECT 'ModuleCount' AS [CheckName], COUNT(1) AS [Value]
FROM modules;

SELECT 'ModuleStatusCount' AS [CheckName], COUNT(1) AS [Value]
FROM module_status;

SELECT 'DepartmentCount' AS [CheckName], COUNT(1) AS [Value]
FROM departments;

SELECT 'ProgramCount' AS [CheckName], COUNT(1) AS [Value]
FROM academic_programs;

SELECT 'CourseCount' AS [CheckName], COUNT(1) AS [Value]
FROM courses;

SELECT 'CourseOfferingCount' AS [CheckName], COUNT(1) AS [Value]
FROM course_offerings;

SELECT 'StudentProfileCount' AS [CheckName], COUNT(1) AS [Value]
FROM student_profiles;

SELECT 'NotificationCount' AS [CheckName], COUNT(1) AS [Value]
FROM notifications;

IF OBJECT_ID(N'[Tabsan-EduSphere]') IS NOT NULL
BEGIN
	SELECT 'TabsanEduSphereMetaCount' AS [CheckName], COUNT(1) AS [Value]
	FROM [Tabsan-EduSphere];
END;

SELECT 'ResultCount' AS [CheckName], COUNT(1) AS [Value]
FROM results;

SELECT 'QuizCount' AS [CheckName], COUNT(1) AS [Value]
FROM quizzes;

SELECT 'QuizQuestionCount' AS [CheckName], COUNT(1) AS [Value]
FROM quiz_questions;

SELECT 'QuizAttemptCount' AS [CheckName], COUNT(1) AS [Value]
FROM quiz_attempts;

SELECT 'SupportTicketCount' AS [CheckName], COUNT(1) AS [Value]
FROM support_tickets;

SELECT 'DiscussionThreadCount' AS [CheckName], COUNT(1) AS [Value]
FROM discussion_threads;

SELECT 'PortalSettingsCount' AS [CheckName], COUNT(1) AS [Value]
FROM portal_settings;

SELECT
	'UsersInstitutionTypeColumnExists' AS [CheckName],
	CASE WHEN COL_LENGTH('users', 'InstitutionType') IS NULL THEN 0 ELSE 1 END AS [Value];

SELECT 'UsersInstitutionTypeAssignedCount' AS [CheckName], COUNT(1) AS [Value]
FROM users
WHERE InstitutionType IS NOT NULL;

SELECT
	'DepartmentsInstitutionTypeColumnExists' AS [CheckName],
	CASE WHEN COL_LENGTH('departments', 'InstitutionType') IS NULL THEN 0 ELSE 1 END AS [Value];

SELECT
	'EnrollmentsStatusMaxLength' AS [CheckName],
	ISNULL(COL_LENGTH('enrollments', 'Status'), 0) AS [Value];

SELECT 'IndexExists_IX_departments_institution_type' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_departments_institution_type' AND object_id = OBJECT_ID('departments')) THEN 1 ELSE 0 END AS [Value];

SELECT 'IndexExists_IX_academic_programs_code_dept' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_academic_programs_code_dept' AND object_id = OBJECT_ID('academic_programs')) THEN 1 ELSE 0 END AS [Value];

SELECT 'IndexExists_IX_enrollments_offering_status' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_enrollments_offering_status' AND object_id = OBJECT_ID('enrollments')) THEN 1 ELSE 0 END AS [Value];

SELECT 'IndexExists_IX_enrollments_student_status' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_enrollments_student_status' AND object_id = OBJECT_ID('enrollments')) THEN 1 ELSE 0 END AS [Value];

SELECT 'MigrationExists_Stage11_DepartmentInstitutionType' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260513121000_Phase1Stage11DepartmentInstitutionType') THEN 1 ELSE 0 END AS [Value];

SELECT 'MigrationExists_Stage12_ReferentialIntegrityAndIndexes' AS [CheckName],
	CASE WHEN EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '20260513124500_Phase1Stage12ReferentialIntegrityAndIndexes') THEN 1 ELSE 0 END AS [Value];

SELECT TOP 20 [MigrationId], [ProductVersion]
FROM __EFMigrationsHistory
ORDER BY [MigrationId] DESC;

PRINT 'Post-deployment checks completed.';
