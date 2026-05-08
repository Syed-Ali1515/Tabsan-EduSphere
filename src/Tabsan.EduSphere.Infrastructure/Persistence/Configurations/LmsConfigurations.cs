using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Lms;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Final-Touches Phase 20 Stage 20.1 — EF configuration for CourseContentModule

/// <summary>EF Core configuration for the CourseContentModule entity.</summary>
public class CourseContentModuleConfiguration : IEntityTypeConfiguration<CourseContentModule>
{
    public void Configure(EntityTypeBuilder<CourseContentModule> builder)
    {
        builder.ToTable("course_content_modules");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Title).IsRequired().HasMaxLength(300);
        builder.Property(m => m.Body).HasMaxLength(50_000);
        builder.Property(m => m.RowVersion).IsRowVersion();

        builder.HasOne<Domain.Academic.CourseOffering>()
               .WithMany()
               .HasForeignKey(m => m.OfferingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(m => m.Videos)
               .WithOne(v => v.Module)
               .HasForeignKey(v => v.ModuleId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}

// Final-Touches Phase 20 Stage 20.2 — EF configuration for ContentVideo

/// <summary>EF Core configuration for the ContentVideo entity.</summary>
public class ContentVideoConfiguration : IEntityTypeConfiguration<ContentVideo>
{
    public void Configure(EntityTypeBuilder<ContentVideo> builder)
    {
        builder.ToTable("content_videos");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Title).IsRequired().HasMaxLength(300);
        builder.Property(v => v.StorageUrl).HasMaxLength(1000);
        builder.Property(v => v.EmbedUrl).HasMaxLength(1000);
        builder.Property(v => v.RowVersion).IsRowVersion();

        builder.HasQueryFilter(v => !v.IsDeleted);
    }
}

// Final-Touches Phase 20 Stage 20.3 — EF configurations for DiscussionThread and DiscussionReply

/// <summary>EF Core configuration for the DiscussionThread entity.</summary>
public class DiscussionThreadConfiguration : IEntityTypeConfiguration<DiscussionThread>
{
    public void Configure(EntityTypeBuilder<DiscussionThread> builder)
    {
        builder.ToTable("discussion_threads");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Title).IsRequired().HasMaxLength(500);
        builder.Property(t => t.RowVersion).IsRowVersion();

        builder.HasOne<Domain.Academic.CourseOffering>()
               .WithMany()
               .HasForeignKey(t => t.OfferingId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Replies)
               .WithOne(r => r.Thread)
               .HasForeignKey(r => r.ThreadId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

/// <summary>EF Core configuration for the DiscussionReply entity.</summary>
public class DiscussionReplyConfiguration : IEntityTypeConfiguration<DiscussionReply>
{
    public void Configure(EntityTypeBuilder<DiscussionReply> builder)
    {
        builder.ToTable("discussion_replies");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Body).IsRequired().HasMaxLength(10_000);
        builder.Property(r => r.RowVersion).IsRowVersion();

        builder.HasQueryFilter(r => !r.IsDeleted);
    }
}

// Final-Touches Phase 20 Stage 20.4 — EF configuration for CourseAnnouncement

/// <summary>EF Core configuration for the CourseAnnouncement entity.</summary>
public class CourseAnnouncementConfiguration : IEntityTypeConfiguration<CourseAnnouncement>
{
    public void Configure(EntityTypeBuilder<CourseAnnouncement> builder)
    {
        builder.ToTable("course_announcements");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Title).IsRequired().HasMaxLength(300);
        builder.Property(a => a.Body).IsRequired().HasMaxLength(10_000);
        builder.Property(a => a.RowVersion).IsRowVersion();

        // Optional FK — null means a department-wide announcement not tied to a specific offering
        builder.HasOne<Domain.Academic.CourseOffering>()
               .WithMany()
               .HasForeignKey(a => a.OfferingId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
