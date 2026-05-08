using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase21_StudyPlanner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxCreditLoadPerSemester",
                table: "academic_programs",
                type: "int",
                nullable: false,
                defaultValue: 18);

            migrationBuilder.CreateTable(
                name: "study_plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlannedSemesterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AdvisorStatus = table.Column<int>(type: "int", nullable: false),
                    AdvisorNotes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plans_student_profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalTable: "student_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_plan_courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_plan_courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_study_plan_courses_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_study_plan_courses_study_plans_StudyPlanId",
                        column: x => x.StudyPlanId,
                        principalTable: "study_plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_plan_courses_CourseId",
                table: "study_plan_courses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "UQ_study_plan_courses_plan_course",
                table: "study_plan_courses",
                columns: new[] { "StudyPlanId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_plans_StudentProfileId",
                table: "study_plans",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_plan_courses");

            migrationBuilder.DropTable(
                name: "study_plans");

            migrationBuilder.DropColumn(
                name: "MaxCreditLoadPerSemester",
                table: "academic_programs");
        }
    }
}
