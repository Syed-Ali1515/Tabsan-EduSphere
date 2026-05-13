using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Tabsan.EduSphere.Infrastructure.Persistence;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260513124500_Phase1Stage12ReferentialIntegrityAndIndexes")]
    public partial class Phase1Stage12ReferentialIntegrityAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "enrollments",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.DropIndex(
                name: "IX_academic_programs_code",
                table: "academic_programs");

            migrationBuilder.CreateIndex(
                name: "IX_academic_programs_code_dept",
                table: "academic_programs",
                columns: new[] { "Code", "DepartmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_academic_programs_dept_active",
                table: "academic_programs",
                columns: new[] { "DepartmentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_courses_dept_active",
                table: "courses",
                columns: new[] { "DepartmentId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_semester_open",
                table: "course_offerings",
                columns: new[] { "SemesterId", "IsOpen" });

            migrationBuilder.CreateIndex(
                name: "IX_course_offerings_faculty_open",
                table: "course_offerings",
                columns: new[] { "FacultyUserId", "IsOpen" });

            migrationBuilder.CreateIndex(
                name: "IX_student_profiles_dept_status",
                table: "student_profiles",
                columns: new[] { "DepartmentId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_profiles_program_status",
                table: "student_profiles",
                columns: new[] { "ProgramId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_offering_status",
                table: "enrollments",
                columns: new[] { "CourseOfferingId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_enrollments_student_status",
                table: "enrollments",
                columns: new[] { "StudentProfileId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_faculty_dept_assignments_active_lookup",
                table: "faculty_department_assignments",
                columns: new[] { "FacultyUserId", "RemovedAt", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_admin_dept_assignments_active_lookup",
                table: "admin_department_assignments",
                columns: new[] { "AdminUserId", "RemovedAt", "DepartmentId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_academic_programs_code_dept",
                table: "academic_programs");

            migrationBuilder.DropIndex(
                name: "IX_academic_programs_dept_active",
                table: "academic_programs");

            migrationBuilder.DropIndex(
                name: "IX_courses_dept_active",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_course_offerings_semester_open",
                table: "course_offerings");

            migrationBuilder.DropIndex(
                name: "IX_course_offerings_faculty_open",
                table: "course_offerings");

            migrationBuilder.DropIndex(
                name: "IX_student_profiles_dept_status",
                table: "student_profiles");

            migrationBuilder.DropIndex(
                name: "IX_student_profiles_program_status",
                table: "student_profiles");

            migrationBuilder.DropIndex(
                name: "IX_enrollments_offering_status",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "IX_enrollments_student_status",
                table: "enrollments");

            migrationBuilder.DropIndex(
                name: "IX_faculty_dept_assignments_active_lookup",
                table: "faculty_department_assignments");

            migrationBuilder.DropIndex(
                name: "IX_admin_dept_assignments_active_lookup",
                table: "admin_department_assignments");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "enrollments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "IX_academic_programs_code",
                table: "academic_programs",
                column: "Code",
                unique: true);
        }
    }
}
