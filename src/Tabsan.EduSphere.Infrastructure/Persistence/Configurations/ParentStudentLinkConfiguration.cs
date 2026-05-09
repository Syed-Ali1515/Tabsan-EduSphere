using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 26 — Stage 26.3

public class ParentStudentLinkConfiguration : IEntityTypeConfiguration<ParentStudentLink>
{
    public void Configure(EntityTypeBuilder<ParentStudentLink> builder)
    {
        builder.ToTable("parent_student_links");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ParentUserId).IsRequired();
        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.Relationship).HasMaxLength(60);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.ParentUserId, x.StudentProfileId })
            .IsUnique()
            .HasDatabaseName("IX_parent_student_links_parent_student");
    }
}
