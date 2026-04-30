using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core fluent configuration for OutboundEmailLog.
/// Append-only table; no updates or deletes are expected.
/// </summary>
public class OutboundEmailLogConfiguration : IEntityTypeConfiguration<OutboundEmailLog>
{
    public void Configure(EntityTypeBuilder<OutboundEmailLog> builder)
    {
        builder.ToTable("outbound_email_logs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ToAddress)
               .IsRequired()
               .HasMaxLength(256);

        builder.Property(e => e.Subject)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(e => e.Status)
               .IsRequired()
               .HasMaxLength(20);

        builder.Property(e => e.ErrorMessage)
               .HasMaxLength(2000);

        builder.Property(e => e.AttemptedAt)
               .IsRequired();

        // Index for querying recent send history by status.
        builder.HasIndex(e => new { e.Status, e.AttemptedAt })
               .HasDatabaseName("IX_outbound_email_logs_status_attempted");
    }
}
