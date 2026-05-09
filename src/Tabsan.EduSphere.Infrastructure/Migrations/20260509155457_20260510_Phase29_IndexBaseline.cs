using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _20260510_Phase29_IndexBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_graduation_applications_StudentProfileId",
                table: "graduation_applications");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_user_created_at",
                table: "user_sessions",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_assigned_created_at",
                table: "support_tickets",
                columns: new[] { "AssignedToId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_department_status_created_at",
                table: "support_tickets",
                columns: new[] { "DepartmentId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_support_tickets_submitter_created_at",
                table: "support_tickets",
                columns: new[] { "SubmitterId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_quiz_student_started_at",
                table: "quiz_attempts",
                columns: new[] { "QuizId", "StudentProfileId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_quiz_attempts_student_started_at",
                table: "quiz_attempts",
                columns: new[] { "StudentProfileId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "ix_pr_status_due_date",
                table: "payment_receipts",
                columns: new[] { "Status", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "ix_pr_student_created_at",
                table: "payment_receipts",
                columns: new[] { "StudentProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_recipients_user_created_at",
                table: "notification_recipients",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_graduation_applications_status_created_at",
                table: "graduation_applications",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_graduation_applications_student_created_at",
                table: "graduation_applications",
                columns: new[] { "StudentProfileId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_sessions_user_created_at",
                table: "user_sessions");

            migrationBuilder.DropIndex(
                name: "IX_support_tickets_assigned_created_at",
                table: "support_tickets");

            migrationBuilder.DropIndex(
                name: "IX_support_tickets_department_status_created_at",
                table: "support_tickets");

            migrationBuilder.DropIndex(
                name: "IX_support_tickets_submitter_created_at",
                table: "support_tickets");

            migrationBuilder.DropIndex(
                name: "IX_quiz_attempts_quiz_student_started_at",
                table: "quiz_attempts");

            migrationBuilder.DropIndex(
                name: "IX_quiz_attempts_student_started_at",
                table: "quiz_attempts");

            migrationBuilder.DropIndex(
                name: "ix_pr_status_due_date",
                table: "payment_receipts");

            migrationBuilder.DropIndex(
                name: "ix_pr_student_created_at",
                table: "payment_receipts");

            migrationBuilder.DropIndex(
                name: "IX_notification_recipients_user_created_at",
                table: "notification_recipients");

            migrationBuilder.DropIndex(
                name: "IX_graduation_applications_status_created_at",
                table: "graduation_applications");

            migrationBuilder.DropIndex(
                name: "IX_graduation_applications_student_created_at",
                table: "graduation_applications");

            migrationBuilder.CreateIndex(
                name: "IX_graduation_applications_StudentProfileId",
                table: "graduation_applications",
                column: "StudentProfileId");
        }
    }
}
