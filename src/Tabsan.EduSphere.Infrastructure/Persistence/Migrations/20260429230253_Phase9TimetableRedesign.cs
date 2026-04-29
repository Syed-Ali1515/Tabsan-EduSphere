using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase9TimetableRedesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_timetables_dept_semester",
                table: "timetables");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "timetables");

            migrationBuilder.RenameColumn(
                name: "CourseOfferingId",
                table: "timetable_entries",
                newName: "RoomId");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicProgramId",
                table: "timetables",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "timetables",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SemesterNumber",
                table: "timetables",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "timetable_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "timetable_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FacultyUserId",
                table: "timetable_entries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuildingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rooms_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_timetables_AcademicProgramId",
                table: "timetables",
                column: "AcademicProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_timetables_dept_program_semester",
                table: "timetables",
                columns: new[] { "DepartmentId", "AcademicProgramId", "SemesterId" });

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_BuildingId",
                table: "timetable_entries",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_CourseId",
                table: "timetable_entries",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_faculty_user",
                table: "timetable_entries",
                column: "FacultyUserId");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_RoomId",
                table: "timetable_entries",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_buildings_code",
                table: "buildings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_building_number",
                table: "rooms",
                columns: new[] { "BuildingId", "Number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_buildings_BuildingId",
                table: "timetable_entries",
                column: "BuildingId",
                principalTable: "buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_courses_CourseId",
                table: "timetable_entries",
                column: "CourseId",
                principalTable: "courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_rooms_RoomId",
                table: "timetable_entries",
                column: "RoomId",
                principalTable: "rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_users_FacultyUserId",
                table: "timetable_entries",
                column: "FacultyUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetables_academic_programs_AcademicProgramId",
                table: "timetables",
                column: "AcademicProgramId",
                principalTable: "academic_programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_buildings_BuildingId",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_courses_CourseId",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_rooms_RoomId",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_users_FacultyUserId",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetables_academic_programs_AcademicProgramId",
                table: "timetables");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "buildings");

            migrationBuilder.DropIndex(
                name: "IX_timetables_AcademicProgramId",
                table: "timetables");

            migrationBuilder.DropIndex(
                name: "IX_timetables_dept_program_semester",
                table: "timetables");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_BuildingId",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_CourseId",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_faculty_user",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_RoomId",
                table: "timetable_entries");

            migrationBuilder.DropColumn(
                name: "AcademicProgramId",
                table: "timetables");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "timetables");

            migrationBuilder.DropColumn(
                name: "SemesterNumber",
                table: "timetables");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "timetable_entries");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "timetable_entries");

            migrationBuilder.DropColumn(
                name: "FacultyUserId",
                table: "timetable_entries");

            migrationBuilder.RenameColumn(
                name: "RoomId",
                table: "timetable_entries",
                newName: "CourseOfferingId");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "timetables",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_timetables_dept_semester",
                table: "timetables",
                columns: new[] { "DepartmentId", "SemesterId" });
        }
    }
}
