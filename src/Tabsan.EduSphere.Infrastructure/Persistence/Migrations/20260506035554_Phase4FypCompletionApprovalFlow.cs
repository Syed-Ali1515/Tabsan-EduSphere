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
            migrationBuilder.Sql(@"
IF COL_LENGTH('license_state', 'ActivatedDomain') IS NULL
BEGIN
    ALTER TABLE [license_state] ADD [ActivatedDomain] nvarchar(253) NULL;
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('license_state', 'MaxUsers') IS NULL
BEGIN
    ALTER TABLE [license_state] ADD [MaxUsers] int NOT NULL CONSTRAINT [DF_license_state_MaxUsers] DEFAULT (0);
END
");

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
            migrationBuilder.Sql(@"
IF COL_LENGTH('license_state', 'ActivatedDomain') IS NOT NULL
BEGIN
    ALTER TABLE [license_state] DROP COLUMN [ActivatedDomain];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('license_state', 'MaxUsers') IS NOT NULL
BEGIN
    DECLARE @dfName nvarchar(128);
    SELECT @dfName = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
    INNER JOIN sys.tables t ON t.object_id = c.object_id
    WHERE t.name = 'license_state' AND c.name = 'MaxUsers';

    IF @dfName IS NOT NULL
        EXEC('ALTER TABLE [license_state] DROP CONSTRAINT [' + @dfName + ']');

    ALTER TABLE [license_state] DROP COLUMN [MaxUsers];
END
");

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
