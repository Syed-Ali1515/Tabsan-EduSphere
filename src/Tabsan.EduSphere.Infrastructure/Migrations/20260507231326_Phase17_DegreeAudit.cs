using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase17_DegreeAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseType",
                table: "courses",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "degree_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AcademicProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MinTotalCredits = table.Column<int>(type: "int", nullable: false),
                    MinCoreCredits = table.Column<int>(type: "int", nullable: false),
                    MinElectiveCredits = table.Column<int>(type: "int", nullable: false),
                    MinGpa = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_degree_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_degree_rules_academic_programs_AcademicProgramId",
                        column: x => x.AcademicProgramId,
                        principalTable: "academic_programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "degree_rule_required_courses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DegreeRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_degree_rule_required_courses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_degree_rule_required_courses_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_degree_rule_required_courses_degree_rules_DegreeRuleId",
                        column: x => x.DegreeRuleId,
                        principalTable: "degree_rules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_degree_rule_required_courses_CourseId",
                table: "degree_rule_required_courses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_degree_rule_required_courses_rule_course",
                table: "degree_rule_required_courses",
                columns: new[] { "DegreeRuleId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_degree_rules_program",
                table: "degree_rules",
                column: "AcademicProgramId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "degree_rule_required_courses");

            migrationBuilder.DropTable(
                name: "degree_rules");

            migrationBuilder.DropColumn(
                name: "CourseType",
                table: "courses");
        }
    }
}
