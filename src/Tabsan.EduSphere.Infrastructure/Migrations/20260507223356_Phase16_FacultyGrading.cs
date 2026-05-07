using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase16_FacultyGrading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rubric_student_grades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentSubmissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RubricCriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RubricLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    GradedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubric_student_grades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rubrics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rubric_criteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RubricId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MaxPoints = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubric_criteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rubric_criteria_rubrics_RubricId",
                        column: x => x.RubricId,
                        principalTable: "rubrics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rubric_levels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PointsAwarded = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    RubricCriterionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rubric_levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rubric_levels_rubric_criteria_RubricCriterionId",
                        column: x => x.RubricCriterionId,
                        principalTable: "rubric_criteria",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_rubric_criteria_rubric_id",
                table: "rubric_criteria",
                column: "RubricId");

            migrationBuilder.CreateIndex(
                name: "IX_rubric_levels_criterion_id",
                table: "rubric_levels",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_rubric_levels_RubricCriterionId",
                table: "rubric_levels",
                column: "RubricCriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_rubric_student_grades_submission_criterion",
                table: "rubric_student_grades",
                columns: new[] { "AssignmentSubmissionId", "RubricCriterionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rubric_student_grades_submission_id",
                table: "rubric_student_grades",
                column: "AssignmentSubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_rubrics_assignment_active",
                table: "rubrics",
                columns: new[] { "AssignmentId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rubric_levels");

            migrationBuilder.DropTable(
                name: "rubric_student_grades");

            migrationBuilder.DropTable(
                name: "rubric_criteria");

            migrationBuilder.DropTable(
                name: "rubrics");
        }
    }
}
