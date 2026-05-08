using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase19_CourseTypeAndGrading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DurationUnit",
                table: "courses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationValue",
                table: "courses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradingType",
                table: "courses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "GPA");

            migrationBuilder.AddColumn<bool>(
                name: "HasSemesters",
                table: "courses",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalSemesters",
                table: "courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "course_grading_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassThreshold = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    GradingType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GradeRangesJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_grading_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_course_grading_configs_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_grading_configs_courseId",
                table: "course_grading_configs",
                column: "CourseId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_grading_configs");

            migrationBuilder.DropColumn(
                name: "DurationUnit",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "DurationValue",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "GradingType",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "HasSemesters",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "TotalSemesters",
                table: "courses");
        }
    }
}
