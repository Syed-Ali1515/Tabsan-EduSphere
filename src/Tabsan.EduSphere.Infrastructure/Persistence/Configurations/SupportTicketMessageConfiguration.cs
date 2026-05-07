using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Helpdesk;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core table mapping for SupportTicketMessage (Phase 14 — Helpdesk).</summary>
public class SupportTicketMessageConfiguration : IEntityTypeConfiguration<SupportTicketMessage>
{
    public void Configure(EntityTypeBuilder<SupportTicketMessage> builder)
    {
        builder.ToTable("support_ticket_messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Body).IsRequired().HasMaxLength(4000);

        builder.HasIndex(m => m.TicketId)
               .HasDatabaseName("IX_support_ticket_messages_ticket");

        builder.HasIndex(m => m.AuthorId)
               .HasDatabaseName("IX_support_ticket_messages_author");
    }
}
