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
  RAISERROR('Failed to switch context to [Tabsan-EduSphere]. Aborting maintenance script.', 16, 1);
  RETURN;
END;
GO

/*
  Lightweight maintenance script aligned with current hierarchy.
  Safe to run repeatedly.
  Run this script after major upgrades or bulk imports to ensure all indexes and views are current.
*/

/* 1) Missing supportive indexes */
IF COL_LENGTH('notification_recipients', 'RecipientUserId') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_notification_recipients_recipient' AND object_id = OBJECT_ID('notification_recipients'))
BEGIN
    CREATE INDEX IX_notification_recipients_recipient
    ON notification_recipients (RecipientUserId, IsRead, CreatedAt DESC);
END

IF COL_LENGTH('assignments', 'DueDate') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_assignments_due_date' AND object_id = OBJECT_ID('assignments'))
BEGIN
    CREATE INDEX IX_assignments_due_date
    ON assignments (CourseOfferingId, DueDate);
END

IF COL_LENGTH('student_profiles', 'RegistrationNumber') IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_student_profiles_registration_active' AND object_id = OBJECT_ID('student_profiles'))
BEGIN
    CREATE INDEX IX_student_profiles_registration_active
    ON student_profiles (RegistrationNumber, IsDeleted);
END

  /* Stage 1.3 parity hardening indexes */
  IF COL_LENGTH('departments', 'InstitutionType') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_departments_institution_type' AND object_id = OBJECT_ID('departments'))
  BEGIN
    CREATE INDEX IX_departments_institution_type
    ON departments (InstitutionType);
  END

  IF COL_LENGTH('academic_programs', 'DepartmentId') IS NOT NULL
  AND COL_LENGTH('academic_programs', 'Code') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_academic_programs_code_dept' AND object_id = OBJECT_ID('academic_programs'))
  BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_academic_programs_code' AND object_id = OBJECT_ID('academic_programs'))
      DROP INDEX IX_academic_programs_code ON academic_programs;

    CREATE UNIQUE INDEX IX_academic_programs_code_dept
    ON academic_programs (Code, DepartmentId);
  END

  IF COL_LENGTH('academic_programs', 'DepartmentId') IS NOT NULL
  AND COL_LENGTH('academic_programs', 'IsActive') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_academic_programs_dept_active' AND object_id = OBJECT_ID('academic_programs'))
  BEGIN
    CREATE INDEX IX_academic_programs_dept_active
    ON academic_programs (DepartmentId, IsActive);
  END

  IF COL_LENGTH('courses', 'DepartmentId') IS NOT NULL
  AND COL_LENGTH('courses', 'IsActive') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_courses_dept_active' AND object_id = OBJECT_ID('courses'))
  BEGIN
    CREATE INDEX IX_courses_dept_active
    ON courses (DepartmentId, IsActive);
  END

  IF COL_LENGTH('course_offerings', 'SemesterId') IS NOT NULL
  AND COL_LENGTH('course_offerings', 'IsOpen') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_course_offerings_semester_open' AND object_id = OBJECT_ID('course_offerings'))
  BEGIN
    CREATE INDEX IX_course_offerings_semester_open
    ON course_offerings (SemesterId, IsOpen);
  END

  IF COL_LENGTH('course_offerings', 'FacultyUserId') IS NOT NULL
  AND COL_LENGTH('course_offerings', 'IsOpen') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_course_offerings_faculty_open' AND object_id = OBJECT_ID('course_offerings'))
  BEGIN
    CREATE INDEX IX_course_offerings_faculty_open
    ON course_offerings (FacultyUserId, IsOpen);
  END

  IF COL_LENGTH('student_profiles', 'DepartmentId') IS NOT NULL
  AND COL_LENGTH('student_profiles', 'Status') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_student_profiles_dept_status' AND object_id = OBJECT_ID('student_profiles'))
  BEGIN
    CREATE INDEX IX_student_profiles_dept_status
    ON student_profiles (DepartmentId, Status);
  END

  IF COL_LENGTH('student_profiles', 'ProgramId') IS NOT NULL
  AND COL_LENGTH('student_profiles', 'Status') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_student_profiles_program_status' AND object_id = OBJECT_ID('student_profiles'))
  BEGIN
    CREATE INDEX IX_student_profiles_program_status
    ON student_profiles (ProgramId, Status);
  END

  IF COL_LENGTH('enrollments', 'Status') IS NOT NULL
  BEGIN
    DECLARE @EnrollmentStatusMaxLength smallint = COL_LENGTH('enrollments', 'Status');
    IF @EnrollmentStatusMaxLength IS NULL OR @EnrollmentStatusMaxLength < 32
    BEGIN
      ALTER TABLE enrollments ALTER COLUMN Status nvarchar(32) NOT NULL;
    END
  END

  IF COL_LENGTH('enrollments', 'CourseOfferingId') IS NOT NULL
  AND COL_LENGTH('enrollments', 'Status') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_enrollments_offering_status' AND object_id = OBJECT_ID('enrollments'))
  BEGIN
    CREATE INDEX IX_enrollments_offering_status
    ON enrollments (CourseOfferingId, Status);
  END

  IF COL_LENGTH('enrollments', 'StudentProfileId') IS NOT NULL
  AND COL_LENGTH('enrollments', 'Status') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_enrollments_student_status' AND object_id = OBJECT_ID('enrollments'))
  BEGIN
    CREATE INDEX IX_enrollments_student_status
    ON enrollments (StudentProfileId, Status);
  END

  IF COL_LENGTH('faculty_department_assignments', 'FacultyUserId') IS NOT NULL
  AND COL_LENGTH('faculty_department_assignments', 'RemovedAt') IS NOT NULL
  AND COL_LENGTH('faculty_department_assignments', 'DepartmentId') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_faculty_dept_assignments_active_lookup' AND object_id = OBJECT_ID('faculty_department_assignments'))
  BEGIN
    CREATE INDEX IX_faculty_dept_assignments_active_lookup
    ON faculty_department_assignments (FacultyUserId, RemovedAt, DepartmentId);
  END

  IF COL_LENGTH('admin_department_assignments', 'AdminUserId') IS NOT NULL
  AND COL_LENGTH('admin_department_assignments', 'RemovedAt') IS NOT NULL
  AND COL_LENGTH('admin_department_assignments', 'DepartmentId') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_admin_dept_assignments_active_lookup' AND object_id = OBJECT_ID('admin_department_assignments'))
  BEGIN
    CREATE INDEX IX_admin_dept_assignments_active_lookup
    ON admin_department_assignments (AdminUserId, RemovedAt, DepartmentId);
  END

/* 2) Operational summary views */
IF OBJECT_ID('dbo.vw_ActiveAcademicHierarchy', 'V') IS NULL
BEGIN
    EXEC('CREATE VIEW dbo.vw_ActiveAcademicHierarchy AS
          SELECT
            d.Id AS DepartmentId,
            d.Name AS DepartmentName,
            p.Id AS ProgramId,
            p.Name AS ProgramName,
            c.Id AS CourseId,
            c.Title AS CourseTitle,
            o.Id AS OfferingId,
            s.Name AS SemesterName,
            o.IsOpen
          FROM departments d
          JOIN academic_programs p ON p.DepartmentId = d.Id AND p.IsDeleted = 0
          JOIN courses c ON c.DepartmentId = d.Id AND c.IsDeleted = 0
          LEFT JOIN course_offerings o ON o.CourseId = c.Id AND o.IsDeleted = 0
          LEFT JOIN semesters s ON s.Id = o.SemesterId AND s.IsDeleted = 0
          WHERE d.IsDeleted = 0');
END

IF OBJECT_ID('dbo.vw_NotificationInboxStats', 'V') IS NULL
BEGIN
    EXEC('CREATE VIEW dbo.vw_NotificationInboxStats AS
          SELECT
            nr.RecipientUserId,
            COUNT(1) AS TotalNotifications,
            SUM(CASE WHEN nr.IsRead = 0 THEN 1 ELSE 0 END) AS UnreadNotifications,
            MAX(n.CreatedAt) AS LastNotificationAt
          FROM notification_recipients nr
          INNER JOIN notifications n ON n.Id = nr.NotificationId
          WHERE n.IsActive = 1
          GROUP BY nr.RecipientUserId');
END

PRINT 'Maintenance script completed.';
