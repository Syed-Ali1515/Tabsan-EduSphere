-- ============================================================
-- Tabsan EduSphere — Script 0: Full Schema
-- ============================================================
-- PURPOSE  : Creates the TabsanEduSphere database (if absent)
--            and applies all EF Core migrations idempotently:
--            tables, indexes, views, and stored procedures.
--
-- RUN THIS FIRST, then run 1-MinimalSeed.sql or 2-FullDummyData.sql.
--
-- SAFE TO RE-RUN — every statement is guarded by migration history
-- checks, so no data is lost and no errors are thrown on repeat runs.
-- ============================================================

-- ── 1. Create database if missing ────────────────────────────
IF DB_ID(N'TabsanEduSphere') IS NULL
BEGIN
    PRINT 'Creating database TabsanEduSphere...';
    CREATE DATABASE [TabsanEduSphere];
    PRINT 'Database created.';
END
GO

USE [TabsanEduSphere];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ── 2. EF Core migration history + all DDL ───────────────────

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [audit_logs] (
        [Id] bigint NOT NULL IDENTITY,
        [ActorUserId] uniqueidentifier NULL,
        [Action] nvarchar(100) NOT NULL,
        [EntityName] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NULL,
        [OldValuesJson] nvarchar(max) NULL,
        [NewValuesJson] nvarchar(max) NULL,
        [OccurredAt] datetime2 NOT NULL,
        [IpAddress] nvarchar(64) NULL,
        CONSTRAINT [PK_audit_logs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [departments] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_departments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [license_state] (
        [Id] uniqueidentifier NOT NULL,
        [LicenseHash] nvarchar(128) NOT NULL,
        [LicenseType] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [ActivatedAt] datetime2 NOT NULL,
        [ExpiresAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_license_state] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [modules] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(50) NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [IsMandatory] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_modules] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(256) NULL,
        [IsSystemRole] bit NOT NULL,
        CONSTRAINT [PK_roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [module_status] (
        [Id] uniqueidentifier NOT NULL,
        [ModuleId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [ActivatedAt] datetime2 NULL,
        [Source] nvarchar(20) NOT NULL,
        [ChangedBy] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_module_status] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_module_status_modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [modules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [users] (
        [Id] uniqueidentifier NOT NULL,
        [Username] nvarchar(100) NOT NULL,
        [Email] nvarchar(256) NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [RoleId] int NOT NULL,
        [DepartmentId] uniqueidentifier NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_users_roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [roles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE TABLE [user_sessions] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RefreshTokenHash] nvarchar(512) NOT NULL,
        [DeviceInfo] nvarchar(512) NULL,
        [IpAddress] nvarchar(64) NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [RevokedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_user_sessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_user_sessions_users_UserId] FOREIGN KEY ([UserId]) REFERENCES [users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_audit_logs_actor] ON [audit_logs] ([ActorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_audit_logs_occurred_at] ON [audit_logs] ([OccurredAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_departments_code] ON [departments] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_status_module_id] ON [module_status] ([ModuleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_modules_key] ON [modules] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_roles_name] ON [roles] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_user_sessions_token_hash] ON [user_sessions] ([RefreshTokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_user_sessions_user_id] ON [user_sessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_users_email] ON [users] ([Email]) WHERE [email] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_users_RoleId] ON [users] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_users_username] ON [users] ([Username]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429002542_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429002542_InitialCreate', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [academic_programs] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [TotalSemesters] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_academic_programs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_academic_programs_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [courses] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [CreditHours] int NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_courses_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [faculty_department_assignments] (
        [Id] uniqueidentifier NOT NULL,
        [FacultyUserId] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [AssignedAt] datetime2 NOT NULL,
        [RemovedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_faculty_department_assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_faculty_department_assignments_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [registration_whitelist] (
        [Id] uniqueidentifier NOT NULL,
        [IdentifierType] nvarchar(max) NOT NULL,
        [IdentifierValue] nvarchar(256) NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [ProgramId] uniqueidentifier NOT NULL,
        [IsUsed] bit NOT NULL,
        [UsedAt] datetime2 NULL,
        [CreatedUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_registration_whitelist] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [semesters] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsClosed] bit NOT NULL,
        [ClosedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_semesters] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [student_profiles] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RegistrationNumber] nvarchar(50) NOT NULL,
        [ProgramId] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [AdmissionDate] datetime2 NOT NULL,
        [Cgpa] decimal(4,2) NOT NULL,
        [CurrentSemesterNumber] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_student_profiles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_student_profiles_academic_programs_ProgramId] FOREIGN KEY ([ProgramId]) REFERENCES [academic_programs] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_student_profiles_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [course_offerings] (
        [Id] uniqueidentifier NOT NULL,
        [CourseId] uniqueidentifier NOT NULL,
        [SemesterId] uniqueidentifier NOT NULL,
        [FacultyUserId] uniqueidentifier NULL,
        [MaxEnrollment] int NOT NULL,
        [IsOpen] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_course_offerings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_course_offerings_courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_course_offerings_semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [semesters] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE TABLE [enrollments] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [CourseOfferingId] uniqueidentifier NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [DroppedAt] datetime2 NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_enrollments_course_offerings_CourseOfferingId] FOREIGN KEY ([CourseOfferingId]) REFERENCES [course_offerings] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_enrollments_student_profiles_StudentProfileId] FOREIGN KEY ([StudentProfileId]) REFERENCES [student_profiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_academic_programs_code] ON [academic_programs] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_academic_programs_DepartmentId] ON [academic_programs] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_course_offerings_course_semester] ON [course_offerings] ([CourseId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_course_offerings_SemesterId] ON [course_offerings] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_courses_code_dept] ON [courses] ([Code], [DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_courses_DepartmentId] ON [courses] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_enrollments_CourseOfferingId] ON [enrollments] ([CourseOfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_enrollments_student_offering] ON [enrollments] ([StudentProfileId], [CourseOfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_faculty_department_assignments_DepartmentId] ON [faculty_department_assignments] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_faculty_dept_assignments_faculty_dept] ON [faculty_department_assignments] ([FacultyUserId], [DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_registration_whitelist_identifier] ON [registration_whitelist] ([IdentifierValue]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_student_profiles_DepartmentId] ON [student_profiles] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE INDEX [IX_student_profiles_ProgramId] ON [student_profiles] ([ProgramId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_student_profiles_reg_no] ON [student_profiles] ([RegistrationNumber]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    CREATE UNIQUE INDEX [IX_student_profiles_user_id] ON [student_profiles] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429004340_AcademicCore'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429004340_AcademicCore', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE TABLE [assignments] (
        [Id] uniqueidentifier NOT NULL,
        [CourseOfferingId] uniqueidentifier NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [DueDate] datetime2 NOT NULL,
        [MaxMarks] decimal(8,2) NOT NULL,
        [IsPublished] bit NOT NULL,
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_assignments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE TABLE [results] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [CourseOfferingId] uniqueidentifier NOT NULL,
        [ResultType] nvarchar(450) NOT NULL,
        [MarksObtained] decimal(8,2) NOT NULL,
        [MaxMarks] decimal(8,2) NOT NULL,
        [IsPublished] bit NOT NULL,
        [PublishedAt] datetime2 NULL,
        [PublishedByUserId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_results] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE TABLE [transcript_export_logs] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [RequestedByUserId] uniqueidentifier NOT NULL,
        [ExportedAt] datetime2 NOT NULL,
        [DocumentUrl] nvarchar(2048) NULL,
        [Format] nvarchar(10) NOT NULL,
        [IpAddress] nvarchar(45) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_transcript_export_logs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE TABLE [assignment_submissions] (
        [Id] uniqueidentifier NOT NULL,
        [AssignmentId] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [FileUrl] nvarchar(2048) NULL,
        [TextContent] nvarchar(max) NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [MarksAwarded] decimal(8,2) NULL,
        [Feedback] nvarchar(2000) NULL,
        [GradedAt] datetime2 NULL,
        [GradedByUserId] uniqueidentifier NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_assignment_submissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_assignment_submissions_assignments_AssignmentId] FOREIGN KEY ([AssignmentId]) REFERENCES [assignments] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE UNIQUE INDEX [IX_assignment_submissions_assignment_student] ON [assignment_submissions] ([AssignmentId], [StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE INDEX [IX_assignments_offering_id] ON [assignments] ([CourseOfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE INDEX [IX_results_offering_id] ON [results] ([CourseOfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE UNIQUE INDEX [IX_results_student_offering_type] ON [results] ([StudentProfileId], [CourseOfferingId], [ResultType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    CREATE INDEX [IX_transcript_export_logs_student_id] ON [transcript_export_logs] ([StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429005740_AssignmentsAndResults'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429005740_AssignmentsAndResults', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE TABLE [attendance_records] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [CourseOfferingId] uniqueidentifier NOT NULL,
        [Date] datetime2 NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [MarkedByUserId] uniqueidentifier NOT NULL,
        [Remarks] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_attendance_records] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE TABLE [notifications] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Body] nvarchar(4000) NOT NULL,
        [Type] nvarchar(50) NOT NULL,
        [SenderUserId] uniqueidentifier NULL,
        [IsSystemGenerated] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_notifications] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE TABLE [notification_recipients] (
        [Id] uniqueidentifier NOT NULL,
        [NotificationId] uniqueidentifier NOT NULL,
        [RecipientUserId] uniqueidentifier NOT NULL,
        [IsRead] bit NOT NULL,
        [ReadAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_notification_recipients] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_notification_recipients_notifications_NotificationId] FOREIGN KEY ([NotificationId]) REFERENCES [notifications] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE INDEX [IX_attendance_offering_date] ON [attendance_records] ([CourseOfferingId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE INDEX [IX_attendance_student_id] ON [attendance_records] ([StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_attendance_student_offering_date] ON [attendance_records] ([StudentProfileId], [CourseOfferingId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE UNIQUE INDEX [IX_notification_recipients_notification_user] ON [notification_recipients] ([NotificationId], [RecipientUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE INDEX [IX_notification_recipients_user_read] ON [notification_recipients] ([RecipientUserId], [IsRead]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE INDEX [IX_notifications_sender_id] ON [notifications] ([SenderUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    CREATE INDEX [IX_notifications_type] ON [notifications] ([Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429011542_NotificationsAndAttendance'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429011542_NotificationsAndAttendance', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [fyp_projects] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [Title] nvarchar(500) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SupervisorUserId] uniqueidentifier NULL,
        [CoordinatorRemarks] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_fyp_projects] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [quizzes] (
        [Id] uniqueidentifier NOT NULL,
        [CourseOfferingId] uniqueidentifier NOT NULL,
        [Title] nvarchar(300) NOT NULL,
        [Instructions] nvarchar(4000) NULL,
        [TimeLimitMinutes] int NULL,
        [MaxAttempts] int NOT NULL,
        [AvailableFrom] datetime2 NULL,
        [AvailableUntil] datetime2 NULL,
        [IsPublished] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_quizzes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [fyp_meetings] (
        [Id] uniqueidentifier NOT NULL,
        [FypProjectId] uniqueidentifier NOT NULL,
        [ScheduledAt] datetime2 NOT NULL,
        [Venue] nvarchar(500) NOT NULL,
        [Agenda] nvarchar(4000) NULL,
        [Status] nvarchar(20) NOT NULL,
        [OrganiserUserId] uniqueidentifier NOT NULL,
        [Minutes] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_fyp_meetings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_fyp_meetings_fyp_projects_FypProjectId] FOREIGN KEY ([FypProjectId]) REFERENCES [fyp_projects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [fyp_panel_members] (
        [Id] uniqueidentifier NOT NULL,
        [FypProjectId] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_fyp_panel_members] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_fyp_panel_members_fyp_projects_FypProjectId] FOREIGN KEY ([FypProjectId]) REFERENCES [fyp_projects] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [quiz_attempts] (
        [Id] uniqueidentifier NOT NULL,
        [QuizId] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [StartedAt] datetime2 NOT NULL,
        [FinishedAt] datetime2 NULL,
        [Status] nvarchar(20) NOT NULL,
        [TotalScore] decimal(10,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_quiz_attempts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_quiz_attempts_quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [quizzes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [quiz_questions] (
        [Id] uniqueidentifier NOT NULL,
        [QuizId] uniqueidentifier NOT NULL,
        [Text] nvarchar(2000) NOT NULL,
        [Type] nvarchar(20) NOT NULL,
        [Marks] decimal(8,2) NOT NULL,
        [OrderIndex] int NOT NULL,
        [QuizId1] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_quiz_questions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_quiz_questions_quizzes_QuizId] FOREIGN KEY ([QuizId]) REFERENCES [quizzes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_quiz_questions_quizzes_QuizId1] FOREIGN KEY ([QuizId1]) REFERENCES [quizzes] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [quiz_answers] (
        [Id] uniqueidentifier NOT NULL,
        [QuizAttemptId] uniqueidentifier NOT NULL,
        [QuizQuestionId] uniqueidentifier NOT NULL,
        [SelectedOptionId] uniqueidentifier NULL,
        [TextResponse] nvarchar(4000) NULL,
        [MarksAwarded] decimal(8,2) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_quiz_answers] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_quiz_answers_quiz_attempts_QuizAttemptId] FOREIGN KEY ([QuizAttemptId]) REFERENCES [quiz_attempts] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_quiz_answers_quiz_questions_QuizQuestionId] FOREIGN KEY ([QuizQuestionId]) REFERENCES [quiz_questions] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE TABLE [quiz_options] (
        [Id] uniqueidentifier NOT NULL,
        [QuizQuestionId] uniqueidentifier NOT NULL,
        [Text] nvarchar(1000) NOT NULL,
        [IsCorrect] bit NOT NULL,
        [OrderIndex] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_quiz_options] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_quiz_options_quiz_questions_QuizQuestionId] FOREIGN KEY ([QuizQuestionId]) REFERENCES [quiz_questions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_meetings_FypProjectId_ScheduledAt] ON [fyp_meetings] ([FypProjectId], [ScheduledAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_meetings_OrganiserUserId_Status] ON [fyp_meetings] ([OrganiserUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_fyp_panel_members_FypProjectId_UserId_Role] ON [fyp_panel_members] ([FypProjectId], [UserId], [Role]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_panel_members_UserId] ON [fyp_panel_members] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_projects_DepartmentId_Status] ON [fyp_projects] ([DepartmentId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_projects_StudentProfileId] ON [fyp_projects] ([StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_fyp_projects_SupervisorUserId] ON [fyp_projects] ([SupervisorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE UNIQUE INDEX [IX_quiz_answers_QuizAttemptId_QuizQuestionId] ON [quiz_answers] ([QuizAttemptId], [QuizQuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_answers_QuizQuestionId] ON [quiz_answers] ([QuizQuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_attempts_QuizId_StudentProfileId_Status] ON [quiz_attempts] ([QuizId], [StudentProfileId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_attempts_StudentProfileId] ON [quiz_attempts] ([StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_options_QuizQuestionId_OrderIndex] ON [quiz_options] ([QuizQuestionId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_questions_QuizId_OrderIndex] ON [quiz_questions] ([QuizId], [OrderIndex]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quiz_questions_QuizId1] ON [quiz_questions] ([QuizId1]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quizzes_CourseOfferingId] ON [quizzes] ([CourseOfferingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    CREATE INDEX [IX_quizzes_CourseOfferingId_IsPublished] ON [quizzes] ([CourseOfferingId], [IsPublished]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429013621_QuizzesAndFyp'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429013621_QuizzesAndFyp', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE TABLE [chat_conversations] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [UserRole] nvarchar(50) NOT NULL,
        [DepartmentId] uniqueidentifier NULL,
        [StartedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_chat_conversations] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE TABLE [chat_messages] (
        [Id] uniqueidentifier NOT NULL,
        [ConversationId] uniqueidentifier NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [SentAt] datetime2 NOT NULL,
        [TokensUsed] int NOT NULL,
        CONSTRAINT [PK_chat_messages] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_chat_messages_chat_conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [chat_conversations] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE INDEX [IX_chat_conversations_UserId] ON [chat_conversations] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE INDEX [IX_chat_conversations_UserId_StartedAt] ON [chat_conversations] ([UserId], [StartedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE INDEX [IX_chat_messages_ConversationId] ON [chat_messages] ([ConversationId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    CREATE INDEX [IX_chat_messages_ConversationId_SentAt] ON [chat_messages] ([ConversationId], [SentAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429035351_AiAndAnalytics'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429035351_AiAndAnalytics', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429041941_VerificationKeys'
)
BEGIN
    CREATE TABLE [consumed_verification_keys] (
        [Id] uniqueidentifier NOT NULL,
        [KeyHash] nvarchar(64) NOT NULL,
        [ConsumedAt] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_consumed_verification_keys] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429041941_VerificationKeys'
)
BEGIN
    CREATE UNIQUE INDEX [IX_consumed_verification_keys_KeyHash] ON [consumed_verification_keys] ([KeyHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429041941_VerificationKeys'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429041941_VerificationKeys', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    ALTER TABLE [users] ADD [FailedLoginAttempts] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    ALTER TABLE [users] ADD [IsLockedOut] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    ALTER TABLE [users] ADD [LockedOutUntil] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    ALTER TABLE [student_profiles] ADD [GraduatedDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    ALTER TABLE [student_profiles] ADD [Status] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE TABLE [admin_change_requests] (
        [Id] uniqueidentifier NOT NULL,
        [RequestorUserId] uniqueidentifier NOT NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [Status] int NOT NULL,
        [ChangeDescription] nvarchar(500) NOT NULL,
        [Reason] nvarchar(2000) NULL,
        [NewData] NVARCHAR(MAX) NOT NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [ReviewedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_admin_change_requests] PRIMARY KEY ([Id]),
        CONSTRAINT [fk_acr_requestor_user] FOREIGN KEY ([RequestorUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_acr_reviewer_user] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE TABLE [payment_receipts] (
        [Id] uniqueidentifier NOT NULL,
        [StudentProfileId] uniqueidentifier NOT NULL,
        [CreatedByUserId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [Amount] decimal(10,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [DueDate] datetime2 NOT NULL,
        [ProofOfPaymentPath] nvarchar(500) NULL,
        [ProofUploadedAt] datetime2 NULL,
        [ConfirmedByUserId] uniqueidentifier NULL,
        [ConfirmedAt] datetime2 NULL,
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_payment_receipts] PRIMARY KEY ([Id]),
        CONSTRAINT [fk_pr_confirmed_by_user] FOREIGN KEY ([ConfirmedByUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_pr_created_by_user] FOREIGN KEY ([CreatedByUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_pr_student_profile] FOREIGN KEY ([StudentProfileId]) REFERENCES [student_profiles] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE TABLE [teacher_modification_requests] (
        [Id] uniqueidentifier NOT NULL,
        [TeacherUserId] uniqueidentifier NOT NULL,
        [ReviewedByUserId] uniqueidentifier NULL,
        [ModificationType] int NOT NULL,
        [RecordId] uniqueidentifier NOT NULL,
        [Status] int NOT NULL,
        [Reason] nvarchar(2000) NOT NULL,
        [ProposedData] NVARCHAR(MAX) NOT NULL,
        [AdminNotes] nvarchar(2000) NULL,
        [ReviewedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_teacher_modification_requests] PRIMARY KEY ([Id]),
        CONSTRAINT [fk_tmr_reviewer_user] FOREIGN KEY ([ReviewedByUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_tmr_teacher_user] FOREIGN KEY ([TeacherUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_acr_requestor_status] ON [admin_change_requests] ([RequestorUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_acr_requestor_user_id] ON [admin_change_requests] ([RequestorUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_acr_status] ON [admin_change_requests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [IX_admin_change_requests_ReviewedByUserId] ON [admin_change_requests] ([ReviewedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [IX_payment_receipts_ConfirmedByUserId] ON [payment_receipts] ([ConfirmedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [IX_payment_receipts_CreatedByUserId] ON [payment_receipts] ([CreatedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_pr_due_date] ON [payment_receipts] ([DueDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_pr_status] ON [payment_receipts] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_pr_student_profile_id] ON [payment_receipts] ([StudentProfileId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_pr_student_status] ON [payment_receipts] ([StudentProfileId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [IX_teacher_modification_requests_ReviewedByUserId] ON [teacher_modification_requests] ([ReviewedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_tmr_modification_type] ON [teacher_modification_requests] ([ModificationType]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_tmr_record_id] ON [teacher_modification_requests] ([RecordId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_tmr_status] ON [teacher_modification_requests] ([Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_tmr_teacher_status] ON [teacher_modification_requests] ([TeacherUserId], [Status]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    CREATE INDEX [ix_tmr_teacher_user_id] ON [teacher_modification_requests] ([TeacherUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429043652_StudentLifecycle'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429043652_StudentLifecycle', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429045706_AccountLockout'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[users]') AND [c].[name] = N'IsLockedOut');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [users] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [users] ADD DEFAULT CAST(0 AS bit) FOR [IsLockedOut];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429045706_AccountLockout'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[users]') AND [c].[name] = N'FailedLoginAttempts');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [users] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [users] ADD DEFAULT 0 FOR [FailedLoginAttempts];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429045706_AccountLockout'
)
BEGIN
    EXEC(N'CREATE INDEX [IX_users_is_locked_out] ON [users] ([IsLockedOut]) WHERE [IsLockedOut] = 1');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429045706_AccountLockout'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429045706_AccountLockout', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    ALTER TABLE [users] ADD [ThemeKey] nvarchar(50) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE TABLE [module_role_assignments] (
        [Id] uniqueidentifier NOT NULL,
        [ModuleId] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_module_role_assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_module_role_assignments_modules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [modules] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE TABLE [report_definitions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Purpose] nvarchar(500) NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_report_definitions] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE TABLE [timetables] (
        [Id] uniqueidentifier NOT NULL,
        [DepartmentId] uniqueidentifier NOT NULL,
        [SemesterId] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [IsPublished] bit NOT NULL DEFAULT CAST(0 AS bit),
        [PublishedAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_timetables] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_timetables_departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [departments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_timetables_semesters_SemesterId] FOREIGN KEY ([SemesterId]) REFERENCES [semesters] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE TABLE [report_role_assignments] (
        [Id] uniqueidentifier NOT NULL,
        [ReportDefinitionId] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(50) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_report_role_assignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_report_role_assignments_report_definitions_ReportDefinitionId] FOREIGN KEY ([ReportDefinitionId]) REFERENCES [report_definitions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE TABLE [timetable_entries] (
        [Id] uniqueidentifier NOT NULL,
        [TimetableId] uniqueidentifier NOT NULL,
        [DayOfWeek] int NOT NULL,
        [StartTime] time NOT NULL,
        [EndTime] time NOT NULL,
        [SubjectName] nvarchar(200) NOT NULL,
        [RoomNumber] nvarchar(50) NULL,
        [FacultyName] nvarchar(200) NULL,
        [CourseOfferingId] uniqueidentifier NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_timetable_entries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_timetable_entries_timetables_TimetableId] FOREIGN KEY ([TimetableId]) REFERENCES [timetables] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_module_role_assignments_unique] ON [module_role_assignments] ([ModuleId], [RoleName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_report_definitions_key] ON [report_definitions] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_report_role_assignments_unique] ON [report_role_assignments] ([ReportDefinitionId], [RoleName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE INDEX [IX_timetable_entries_timetable_day] ON [timetable_entries] ([TimetableId], [DayOfWeek]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE INDEX [IX_timetables_dept_semester] ON [timetables] ([DepartmentId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    CREATE INDEX [IX_timetables_SemesterId] ON [timetables] ([SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429223425_Phase9DashboardSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429223425_Phase9DashboardSettings', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    DROP INDEX [IX_timetables_dept_semester] ON [timetables];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[timetables]') AND [c].[name] = N'Title');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [timetables] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [timetables] DROP COLUMN [Title];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    EXEC sp_rename N'[timetable_entries].[CourseOfferingId]', N'RoomId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetables] ADD [AcademicProgramId] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetables] ADD [EffectiveDate] date NOT NULL DEFAULT '0001-01-01';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetables] ADD [SemesterNumber] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD [BuildingId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD [CourseId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD [FacultyUserId] uniqueidentifier NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE TABLE [buildings] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_buildings] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE TABLE [rooms] (
        [Id] uniqueidentifier NOT NULL,
        [Number] nvarchar(50) NOT NULL,
        [BuildingId] uniqueidentifier NOT NULL,
        [Capacity] int NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_rooms] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_rooms_buildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [buildings] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetables_AcademicProgramId] ON [timetables] ([AcademicProgramId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetables_dept_program_semester] ON [timetables] ([DepartmentId], [AcademicProgramId], [SemesterId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetable_entries_BuildingId] ON [timetable_entries] ([BuildingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetable_entries_CourseId] ON [timetable_entries] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetable_entries_faculty_user] ON [timetable_entries] ([FacultyUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE INDEX [IX_timetable_entries_RoomId] ON [timetable_entries] ([RoomId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE UNIQUE INDEX [IX_buildings_code] ON [buildings] ([Code]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    CREATE UNIQUE INDEX [IX_rooms_building_number] ON [rooms] ([BuildingId], [Number]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD CONSTRAINT [FK_timetable_entries_buildings_BuildingId] FOREIGN KEY ([BuildingId]) REFERENCES [buildings] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD CONSTRAINT [FK_timetable_entries_courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [courses] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD CONSTRAINT [FK_timetable_entries_rooms_RoomId] FOREIGN KEY ([RoomId]) REFERENCES [rooms] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetable_entries] ADD CONSTRAINT [FK_timetable_entries_users_FacultyUserId] FOREIGN KEY ([FacultyUserId]) REFERENCES [users] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    ALTER TABLE [timetables] ADD CONSTRAINT [FK_timetables_academic_programs_AcademicProgramId] FOREIGN KEY ([AcademicProgramId]) REFERENCES [academic_programs] ([Id]) ON DELETE NO ACTION;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260429230253_Phase9TimetableRedesign'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260429230253_Phase9TimetableRedesign', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    CREATE TABLE [sidebar_menu_items] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Purpose] nvarchar(500) NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [ParentId] uniqueidentifier NULL,
        [DisplayOrder] int NOT NULL,
        [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
        [IsSystemMenu] bit NOT NULL DEFAULT CAST(0 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [DeletedAt] datetime2 NULL,
        CONSTRAINT [PK_sidebar_menu_items] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_sidebar_menu_items_sidebar_menu_items_ParentId] FOREIGN KEY ([ParentId]) REFERENCES [sidebar_menu_items] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    CREATE TABLE [sidebar_menu_role_accesses] (
        [Id] uniqueidentifier NOT NULL,
        [SidebarMenuItemId] uniqueidentifier NOT NULL,
        [RoleName] nvarchar(100) NOT NULL,
        [IsAllowed] bit NOT NULL DEFAULT CAST(1 AS bit),
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_sidebar_menu_role_accesses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_sidebar_menu_role_accesses_sidebar_menu_items_SidebarMenuItemId] FOREIGN KEY ([SidebarMenuItemId]) REFERENCES [sidebar_menu_items] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_sidebar_menu_items_key] ON [sidebar_menu_items] ([Key]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    CREATE INDEX [IX_sidebar_menu_items_ParentId] ON [sidebar_menu_items] ([ParentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    CREATE UNIQUE INDEX [IX_sidebar_menu_role_accesses_item_role] ON [sidebar_menu_role_accesses] ([SidebarMenuItemId], [RoleName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430000234_Phase9SidebarSettings'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430000234_Phase9SidebarSettings', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430045628_Phase10PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_audit_logs_entity_occurred_at] ON [audit_logs] ([EntityName], [OccurredAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430045628_Phase10PerformanceIndexes'
)
BEGIN
    CREATE INDEX [IX_assignments_offering_published] ON [assignments] ([CourseOfferingId], [IsPublished]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430045628_Phase10PerformanceIndexes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430045628_Phase10PerformanceIndexes', N'8.0.8');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430141918_Phase10SecurityTables'
)
BEGIN
    CREATE TABLE [outbound_email_logs] (
        [Id] uniqueidentifier NOT NULL,
        [ToAddress] nvarchar(256) NOT NULL,
        [Subject] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [ErrorMessage] nvarchar(2000) NULL,
        [AttemptedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_outbound_email_logs] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430141918_Phase10SecurityTables'
)
BEGIN
    CREATE TABLE [password_history] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [PasswordHash] nvarchar(512) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_password_history] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430141918_Phase10SecurityTables'
)
BEGIN
    CREATE INDEX [IX_outbound_email_logs_status_attempted] ON [outbound_email_logs] ([Status], [AttemptedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430141918_Phase10SecurityTables'
)
BEGIN
    CREATE INDEX [IX_password_history_user_created] ON [password_history] ([UserId], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430141918_Phase10SecurityTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430141918_Phase10SecurityTables', N'8.0.8');
END;
GO

COMMIT;
GO

-- ── Phase10StoredProcedures ──────────────────────────────────
-- CREATE OR ALTER PROCEDURE must be the first statement in its batch (after GO).
-- The migration-history guard below keeps the INSERT idempotent.
-- The procedures themselves are idempotent via CREATE OR ALTER.

CREATE OR ALTER PROCEDURE [dbo].[sp_get_attendance_below_threshold]
    @ThresholdPercent DECIMAL(5,2) = 75.0,
    @CourseOfferingId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ar.StudentProfileId,
        ar.CourseOfferingId,
        COUNT(*) AS TotalSessions,
        SUM(CASE WHEN ar.Status = 'Present' THEN 1 ELSE 0 END) AS AttendedSessions,
        CAST(
            CASE WHEN COUNT(*) = 0 THEN 0.0
                 ELSE (SUM(CASE WHEN ar.Status = 'Present' THEN 1.0 ELSE 0.0 END) / COUNT(*)) * 100.0
            END AS DECIMAL(5,2)
        ) AS AttendancePercentage
    FROM attendance_records ar
    WHERE (@CourseOfferingId IS NULL OR ar.CourseOfferingId = @CourseOfferingId)
    GROUP BY ar.StudentProfileId, ar.CourseOfferingId
    HAVING
        CASE WHEN COUNT(*) = 0 THEN 0.0
             ELSE (SUM(CASE WHEN ar.Status = 'Present' THEN 1.0 ELSE 0.0 END) / COUNT(*)) * 100.0
        END < @ThresholdPercent;
END;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_recalculate_student_cgpa]
    @StudentProfileId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TotalWeightedMarks DECIMAL(18,4) = 0;
    DECLARE @TotalMaxMarks DECIMAL(18,4) = 0;
    DECLARE @NewCgpa DECIMAL(4,2) = 0;

    SELECT
        @TotalWeightedMarks = SUM(CAST(r.MarksObtained AS DECIMAL(18,4))),
        @TotalMaxMarks = SUM(CAST(r.MaxMarks AS DECIMAL(18,4)))
    FROM results r
    WHERE r.StudentProfileId = @StudentProfileId
      AND r.IsPublished = 1
      AND r.MaxMarks > 0;

    IF @TotalMaxMarks > 0
    BEGIN
        -- Convert percentage to 4.0 GPA scale (proportional mapping: 100% -> 4.0)
        SET @NewCgpa = CAST((@TotalWeightedMarks / @TotalMaxMarks) * 4.0 AS DECIMAL(4,2));
        IF @NewCgpa > 4.0 SET @NewCgpa = 4.0;
    END

    UPDATE student_profiles
    SET Cgpa = @NewCgpa
    WHERE Id = @StudentProfileId;

    SELECT @NewCgpa AS NewCgpa;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430142338_Phase10StoredProcedures'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430142338_Phase10StoredProcedures', N'8.0.8');
END;
GO

-- ── Phase10SqlViews ──────────────────────────────────────────
-- CREATE VIEW must be the first statement in its batch (after GO).
-- DROP VIEW IF EXISTS in the preceding batch makes this idempotent.

DROP VIEW IF EXISTS [dbo].[vw_student_attendance_summary];
GO

CREATE VIEW [dbo].[vw_student_attendance_summary] AS
SELECT
    ar.StudentProfileId,
    ar.CourseOfferingId,
    COUNT(*) AS TotalSessions,
    SUM(CASE WHEN ar.Status = 'Present' THEN 1 ELSE 0 END) AS AttendedSessions,
    CAST(
        CASE WHEN COUNT(*) = 0 THEN 0.0
             ELSE (SUM(CASE WHEN ar.Status = 'Present' THEN 1.0 ELSE 0.0 END) / COUNT(*)) * 100.0
        END AS decimal(5,2)
    ) AS AttendancePercentage
FROM attendance_records ar
GROUP BY ar.StudentProfileId, ar.CourseOfferingId;
GO

DROP VIEW IF EXISTS [dbo].[vw_student_results_summary];
GO

CREATE VIEW [dbo].[vw_student_results_summary] AS
SELECT
    r.StudentProfileId,
    r.CourseOfferingId,
    r.ResultType,
    r.MarksObtained,
    r.MaxMarks,
    CAST(
        CASE WHEN r.MaxMarks = 0 THEN 0.0
             ELSE (CAST(r.MarksObtained AS decimal(10,2)) / r.MaxMarks) * 100.0
        END AS decimal(5,2)
    ) AS Percentage,
    r.PublishedAt,
    co.CourseId,
    c.Code AS CourseCode,
    c.Title AS CourseTitle,
    co.SemesterId
FROM results r
INNER JOIN course_offerings co ON co.Id = r.CourseOfferingId
INNER JOIN courses c ON c.Id = co.CourseId
WHERE r.IsPublished = 1;
GO

DROP VIEW IF EXISTS [dbo].[vw_course_enrollment_summary];
GO

CREATE VIEW [dbo].[vw_course_enrollment_summary] AS
SELECT
    co.Id AS CourseOfferingId,
    co.CourseId,
    c.Code AS CourseCode,
    c.Title AS CourseTitle,
    co.SemesterId,
    co.MaxEnrollment,
    COUNT(e.Id) AS EnrolledCount,
    co.MaxEnrollment - COUNT(e.Id) AS AvailableSeats
FROM course_offerings co
INNER JOIN courses c ON c.Id = co.CourseId
LEFT JOIN enrollments e ON e.CourseOfferingId = co.Id AND e.Status = 'Active'
WHERE co.IsOpen = 1
GROUP BY co.Id, co.CourseId, c.Code, c.Title, co.SemesterId, co.MaxEnrollment;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430143000_Phase10SqlViews'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430143000_Phase10SqlViews', N'8.0.8');
END;
GO

PRINT '✓ Schema script complete. All tables, views, and stored procedures are ready.';
GO

