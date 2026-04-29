using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class StudentLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedOut",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedOutUntil",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GraduatedDate",
                table: "student_profiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "student_profiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "admin_change_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestorUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ChangeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NewData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_change_requests", x => x.Id);
                    table.ForeignKey(
                        name: "fk_acr_requestor_user",
                        column: x => x.RequestorUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_acr_reviewer_user",
                        column: x => x.ReviewedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_receipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProofOfPaymentPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ProofUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_receipts", x => x.Id);
                    table.ForeignKey(
                        name: "fk_pr_confirmed_by_user",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pr_created_by_user",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_pr_student_profile",
                        column: x => x.StudentProfileId,
                        principalTable: "student_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_modification_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModificationType = table.Column<int>(type: "int", nullable: false),
                    RecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ProposedData = table.Column<string>(type: "NVARCHAR(MAX)", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_modification_requests", x => x.Id);
                    table.ForeignKey(
                        name: "fk_tmr_reviewer_user",
                        column: x => x.ReviewedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_tmr_teacher_user",
                        column: x => x.TeacherUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_acr_requestor_status",
                table: "admin_change_requests",
                columns: new[] { "RequestorUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_acr_requestor_user_id",
                table: "admin_change_requests",
                column: "RequestorUserId");

            migrationBuilder.CreateIndex(
                name: "ix_acr_status",
                table: "admin_change_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_admin_change_requests_ReviewedByUserId",
                table: "admin_change_requests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_receipts_ConfirmedByUserId",
                table: "payment_receipts",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_payment_receipts_CreatedByUserId",
                table: "payment_receipts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "ix_pr_due_date",
                table: "payment_receipts",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "ix_pr_status",
                table: "payment_receipts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_pr_student_profile_id",
                table: "payment_receipts",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "ix_pr_student_status",
                table: "payment_receipts",
                columns: new[] { "StudentProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_teacher_modification_requests_ReviewedByUserId",
                table: "teacher_modification_requests",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "ix_tmr_modification_type",
                table: "teacher_modification_requests",
                column: "ModificationType");

            migrationBuilder.CreateIndex(
                name: "ix_tmr_record_id",
                table: "teacher_modification_requests",
                column: "RecordId");

            migrationBuilder.CreateIndex(
                name: "ix_tmr_status",
                table: "teacher_modification_requests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "ix_tmr_teacher_status",
                table: "teacher_modification_requests",
                columns: new[] { "TeacherUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "ix_tmr_teacher_user_id",
                table: "teacher_modification_requests",
                column: "TeacherUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_change_requests");

            migrationBuilder.DropTable(
                name: "payment_receipts");

            migrationBuilder.DropTable(
                name: "teacher_modification_requests");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IsLockedOut",
                table: "users");

            migrationBuilder.DropColumn(
                name: "LockedOutUntil",
                table: "users");

            migrationBuilder.DropColumn(
                name: "GraduatedDate",
                table: "student_profiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "student_profiles");
        }
    }
}
