-- ============================================================
-- CLEANUP: Remove EduSphere objects accidentally created in master
-- Run this against the MASTER database ONLY.
-- After running, execute 01-Schema-Current.sql from master to
-- create TabsanEduSphere properly.
-- ============================================================

USE master;
GO

PRINT 'Starting cleanup of EduSphere objects from master database...';
GO

-- ------------------------------------------------------------
-- Drop Views
-- ------------------------------------------------------------
DROP VIEW IF EXISTS [vw_course_enrollment_summary];
DROP VIEW IF EXISTS [vw_student_results_summary];
DROP VIEW IF EXISTS [vw_student_attendance_summary];
GO

-- ------------------------------------------------------------
-- Drop Stored Procedures
-- ------------------------------------------------------------
DROP PROCEDURE IF EXISTS [sp_get_attendance_below_threshold];
DROP PROCEDURE IF EXISTS [sp_recalculate_student_cgpa];
GO

-- ------------------------------------------------------------
-- Drop Tables (order respects foreign key dependencies)
-- ------------------------------------------------------------

-- Leaf / junction tables first
DROP TABLE IF EXISTS [rubric_student_grades];
DROP TABLE IF EXISTS [rubric_levels];
DROP TABLE IF EXISTS [rubric_criteria];
DROP TABLE IF EXISTS [rubrics];
DROP TABLE IF EXISTS [quiz_answers];
DROP TABLE IF EXISTS [quiz_attempts];
DROP TABLE IF EXISTS [quiz_options];
DROP TABLE IF EXISTS [quiz_questions];
DROP TABLE IF EXISTS [quizzes];
DROP TABLE IF EXISTS [assignment_submissions];
DROP TABLE IF EXISTS [assignments];
DROP TABLE IF EXISTS [attendance_records];
DROP TABLE IF EXISTS [results];
DROP TABLE IF EXISTS [result_component_rules];
DROP TABLE IF EXISTS [course_grading_configs];
DROP TABLE IF EXISTS [gpa_scale_rules];
DROP TABLE IF EXISTS [institution_grading_profiles];
DROP TABLE IF EXISTS [enrollments];
DROP TABLE IF EXISTS [timetable_entries];
DROP TABLE IF EXISTS [timetables];
DROP TABLE IF EXISTS [rooms];
DROP TABLE IF EXISTS [buildings];
DROP TABLE IF EXISTS [course_announcements];
DROP TABLE IF EXISTS [content_videos];
DROP TABLE IF EXISTS [course_content_modules];
DROP TABLE IF EXISTS [discussion_replies];
DROP TABLE IF EXISTS [discussion_threads];
DROP TABLE IF EXISTS [fyp_meetings];
DROP TABLE IF EXISTS [fyp_panel_members];
DROP TABLE IF EXISTS [fyp_projects];
DROP TABLE IF EXISTS [chat_messages];
DROP TABLE IF EXISTS [chat_conversations];
DROP TABLE IF EXISTS [support_ticket_messages];
DROP TABLE IF EXISTS [support_tickets];
DROP TABLE IF EXISTS [notification_recipients];
DROP TABLE IF EXISTS [notifications];
DROP TABLE IF EXISTS [outbound_email_logs];
DROP TABLE IF EXISTS [report_role_assignments];
DROP TABLE IF EXISTS [report_definitions];
DROP TABLE IF EXISTS [module_role_assignments];
DROP TABLE IF EXISTS [sidebar_menu_role_accesses];
DROP TABLE IF EXISTS [sidebar_menu_items];
DROP TABLE IF EXISTS [module_status];
DROP TABLE IF EXISTS [student_report_cards];
DROP TABLE IF EXISTS [transcript_export_logs];
DROP TABLE IF EXISTS [graduation_application_approvals];
DROP TABLE IF EXISTS [graduation_applications];
DROP TABLE IF EXISTS [bulk_promotion_entries];
DROP TABLE IF EXISTS [bulk_promotion_batches];
DROP TABLE IF EXISTS [teacher_modification_requests];
DROP TABLE IF EXISTS [admin_change_requests];
DROP TABLE IF EXISTS [accreditation_templates];
DROP TABLE IF EXISTS [degree_rule_required_courses];
DROP TABLE IF EXISTS [degree_rules];
DROP TABLE IF EXISTS [study_plan_courses];
DROP TABLE IF EXISTS [study_plans];
DROP TABLE IF EXISTS [student_stream_assignments];
DROP TABLE IF EXISTS [school_streams];
DROP TABLE IF EXISTS [registration_whitelist];
DROP TABLE IF EXISTS [payment_receipts];
DROP TABLE IF EXISTS [password_history];
DROP TABLE IF EXISTS [parent_student_links];
DROP TABLE IF EXISTS [consumed_verification_keys];
DROP TABLE IF EXISTS [academic_deadlines];
DROP TABLE IF EXISTS [course_prerequisites];
DROP TABLE IF EXISTS [course_offerings];
DROP TABLE IF EXISTS [semesters];
DROP TABLE IF EXISTS [courses];
DROP TABLE IF EXISTS [academic_programs];
DROP TABLE IF EXISTS [student_profiles];
DROP TABLE IF EXISTS [faculty_department_assignments];
DROP TABLE IF EXISTS [admin_department_assignments];
DROP TABLE IF EXISTS [user_sessions];
DROP TABLE IF EXISTS [audit_logs];
DROP TABLE IF EXISTS [license_state];
DROP TABLE IF EXISTS [portal_settings];
DROP TABLE IF EXISTS [modules];
DROP TABLE IF EXISTS [roles];
DROP TABLE IF EXISTS [users];
DROP TABLE IF EXISTS [departments];
DROP TABLE IF EXISTS [__EFMigrationsHistory];
GO

PRINT 'Cleanup complete. All EduSphere objects removed from master.';
PRINT 'Now run 01-Schema-Current.sql (connected to master) to create TabsanEduSphere properly.';
GO
