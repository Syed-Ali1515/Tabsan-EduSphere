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
