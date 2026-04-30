using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tabsan.EduSphere.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Phase10PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_occurred_at",
                table: "audit_logs",
                columns: new[] { "EntityName", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_assignments_offering_published",
                table: "assignments",
                columns: new[] { "CourseOfferingId", "IsPublished" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_logs_entity_occurred_at",
                table: "audit_logs");

            migrationBuilder.DropIndex(
                name: "IX_assignments_offering_published",
                table: "assignments");
        }
    }
}
