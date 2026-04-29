using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the Notification entity.</summary>
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Title).IsRequired().HasMaxLength(300);
        builder.Property(n => n.Body).IsRequired().HasMaxLength(4000);
        builder.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);

        // Index for fetching recent active notifications by sender.
        builder.HasIndex(n => n.SenderUserId)
               .HasDatabaseName("IX_notifications_sender_id");

        // Index for filtering by type (e.g. system alerts).
        builder.HasIndex(n => n.Type)
               .HasDatabaseName("IX_notifications_type");

        // Notifications are never soft-deleted — deactivated via IsActive flag.
        // No global query filter here; service layer filters by IsActive as needed.
    }
}

/// <summary>EF Core configuration for the NotificationRecipient entity.</summary>
public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable("notification_recipients");
        builder.HasKey(r => r.Id);

        // Each user receives a notification at most once.
        builder.HasIndex(r => new { r.NotificationId, r.RecipientUserId })
               .IsUnique()
               .HasDatabaseName("IX_notification_recipients_notification_user");

        // Fast unread-count and inbox queries per user.
        builder.HasIndex(r => new { r.RecipientUserId, r.IsRead })
               .HasDatabaseName("IX_notification_recipients_user_read");

        builder.HasOne(r => r.Notification)
               .WithMany()
               .HasForeignKey(r => r.NotificationId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
