using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase4FypCompletionApprovalFlow : Migration
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

            migrationBuilder.AddColumn<string>(
                name: "CompletionApprovedByUserIdsCsv",
                table: "fyp_projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionRequestedAt",
                table: "fyp_projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompletionRequestedByStudentProfileId",
                table: "fyp_projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompletionRequested",
                table: "fyp_projects",
                type: "bit",
                nullable: false,
                defaultValue: false);
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

            migrationBuilder.DropColumn(
                name: "CompletionApprovedByUserIdsCsv",
                table: "fyp_projects");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedAt",
                table: "fyp_projects");

            migrationBuilder.DropColumn(
                name: "CompletionRequestedByStudentProfileId",
                table: "fyp_projects");

            migrationBuilder.DropColumn(
                name: "IsCompletionRequested",
                table: "fyp_projects");
        }
    }
}
