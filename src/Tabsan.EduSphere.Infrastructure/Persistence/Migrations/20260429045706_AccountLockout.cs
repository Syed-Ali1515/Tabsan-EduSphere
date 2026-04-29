using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AccountLockout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsLockedOut",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "FailedLoginAttempts",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_users_is_locked_out",
                table: "users",
                column: "IsLockedOut",
                filter: "[is_locked_out] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_is_locked_out",
                table: "users");

            migrationBuilder.AlterColumn<bool>(
                name: "IsLockedOut",
                table: "users",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "FailedLoginAttempts",
                table: "users",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);
        }
    }
}
