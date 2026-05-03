using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase11ResultCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentSemesterGpa",
                table: "student_profiles",
                type: "decimal(4,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "ResultType",
                table: "results",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "GradePoint",
                table: "results",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gpa_scale_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GradePoint = table.Column<decimal>(type: "decimal(4,2)", nullable: false),
                    MinimumScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gpa_scale_rules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "result_component_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Weightage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_result_component_rules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gpa_scale_rules_minimum_score",
                table: "gpa_scale_rules",
                column: "MinimumScore",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_result_component_rules_name",
                table: "result_component_rules",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gpa_scale_rules");

            migrationBuilder.DropTable(
                name: "result_component_rules");

            migrationBuilder.DropColumn(
                name: "CurrentSemesterGpa",
                table: "student_profiles");

            migrationBuilder.DropColumn(
                name: "GradePoint",
                table: "results");

            migrationBuilder.AlterColumn<string>(
                name: "ResultType",
                table: "results",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
