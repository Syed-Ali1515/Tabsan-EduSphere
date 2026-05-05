using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2LicenseConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActivatedDomain",
                table: "license_state",
                type: "nvarchar(253)",
                maxLength: 253,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxUsers",
                table: "license_state",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivatedDomain",
                table: "license_state");

            migrationBuilder.DropColumn(
                name: "MaxUsers",
                table: "license_state");
        }
    }
}
