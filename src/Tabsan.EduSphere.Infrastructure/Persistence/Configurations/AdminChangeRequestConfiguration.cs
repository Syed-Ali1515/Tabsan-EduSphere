using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.StudentLifecycle;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the AdminChangeRequest entity.
/// Indexes applied on RequestorUserId and Status for query performance.
/// </summary>
public class AdminChangeRequestConfiguration : IEntityTypeConfiguration<AdminChangeRequest>
{
    public void Configure(EntityTypeBuilder<AdminChangeRequest> builder)
    {
        builder.ToTable("admin_change_requests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.RequestorUserId).IsRequired();
        builder.Property(x => x.ReviewedByUserId).IsRequired(false);
        builder.Property(x => x.Status).IsRequired().HasConversion<int>();
        builder.Property(x => x.ChangeDescription).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Reason).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.NewData).IsRequired().HasColumnType("NVARCHAR(MAX)");
        builder.Property(x => x.AdminNotes).IsRequired(false).HasMaxLength(2000);
        builder.Property(x => x.ReviewedAt).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();

        // Foreign keys
        builder.HasOne(x => x.Requestor)
            .WithMany()
            .HasForeignKey(x => x.RequestorUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_acr_requestor_user");

        builder.HasOne(x => x.ReviewedByUser)
            .WithMany()
            .HasForeignKey(x => x.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_acr_reviewer_user");

        // Indexes
        builder.HasIndex(x => x.RequestorUserId).HasDatabaseName("ix_acr_requestor_user_id");
        builder.HasIndex(x => x.Status).HasDatabaseName("ix_acr_status");
        builder.HasIndex(x => new { x.RequestorUserId, x.Status }).HasDatabaseName("ix_acr_requestor_status");
    }
}
