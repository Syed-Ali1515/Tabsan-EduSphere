using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

// Phase 26 — Stage 26.1

public class StudentStreamAssignmentConfiguration : IEntityTypeConfiguration<StudentStreamAssignment>
{
    public void Configure(EntityTypeBuilder<StudentStreamAssignment> builder)
    {
        builder.ToTable("student_stream_assignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentProfileId).IsRequired();
        builder.Property(x => x.SchoolStreamId).IsRequired();
        builder.Property(x => x.AssignedByUserId).IsRequired();
        builder.Property(x => x.AssignedAt).IsRequired();

        builder.HasOne(x => x.Stream)
            .WithMany()
            .HasForeignKey(x => x.SchoolStreamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.StudentProfileId)
            .IsUnique()
            .HasDatabaseName("IX_student_stream_assignments_student");
    }
}
