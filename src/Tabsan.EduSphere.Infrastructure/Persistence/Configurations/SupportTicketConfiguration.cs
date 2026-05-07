using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Helpdesk;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table mapping for SupportTicket (Phase 14 — Helpdesk).</summary>
public class SupportTicketConfiguration : IEntityTypeConfiguration<SupportTicket>
{
    public void Configure(EntityTypeBuilder<SupportTicket> builder)
    {
        builder.ToTable("support_tickets");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Subject).IsRequired().HasMaxLength(300);
        builder.Property(t => t.Body).IsRequired().HasMaxLength(4000);
        builder.Property(t => t.Category).IsRequired();
        builder.Property(t => t.Status).IsRequired();
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasMany(t => t.Messages)
               .WithOne()
               .HasForeignKey(m => m.TicketId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.SubmitterId)
               .HasDatabaseName("IX_support_tickets_submitter");

        builder.HasIndex(t => t.DepartmentId)
               .HasDatabaseName("IX_support_tickets_department");

        builder.HasIndex(t => t.Status)
               .HasDatabaseName("IX_support_tickets_status");

        builder.HasIndex(t => t.AssignedToId)
               .HasDatabaseName("IX_support_tickets_assigned");

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
