using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Quizzes;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>EF Core configuration for the <see cref="Quiz"/> entity.</summary>
internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("quizzes");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.RowVersion).IsRowVersion();

        builder.Property(q => q.Title).HasMaxLength(300).IsRequired();
        builder.Property(q => q.Instructions).HasMaxLength(4000);

        // Store IsActive; filter on active quizzes by default
        builder.HasQueryFilter(q => q.IsActive);

        builder.HasIndex(q => q.CourseOfferingId);
        builder.HasIndex(q => new { q.CourseOfferingId, q.IsPublished });
    }
}

/// <summary>EF Core configuration for the <see cref="QuizQuestion"/> entity.</summary>
internal sealed class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder.ToTable("quiz_questions");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.RowVersion).IsRowVersion();

        builder.Property(q => q.Text).HasMaxLength(2000).IsRequired();
        builder.Property(q => q.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(q => q.Marks).HasColumnType("decimal(8,2)");

        builder.HasOne(q => q.Quiz)
               .WithMany()
               .HasForeignKey(q => q.QuizId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(q => new { q.QuizId, q.OrderIndex });
    }
}

/// <summary>EF Core configuration for the <see cref="QuizOption"/> entity.</summary>
internal sealed class QuizOptionConfiguration : IEntityTypeConfiguration<QuizOption>
{
    public void Configure(EntityTypeBuilder<QuizOption> builder)
    {
        builder.ToTable("quiz_options");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.Property(o => o.Text).HasMaxLength(1000).IsRequired();

        builder.HasOne(o => o.Question)
               .WithMany(q => q.Options)
               .HasForeignKey(o => o.QuizQuestionId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => new { o.QuizQuestionId, o.OrderIndex });
    }
}

/// <summary>EF Core configuration for the <see cref="QuizAttempt"/> entity.</summary>
internal sealed class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
{
    public void Configure(EntityTypeBuilder<QuizAttempt> builder)
    {
        builder.ToTable("quiz_attempts");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.TotalScore).HasColumnType("decimal(10,2)");

        builder.HasOne(a => a.Quiz)
               .WithMany()
               .HasForeignKey(a => a.QuizId)
               .OnDelete(DeleteBehavior.Restrict);

        // One in-progress attempt per student per quiz
        builder.HasIndex(a => new { a.QuizId, a.StudentProfileId, a.Status });
        builder.HasIndex(a => a.StudentProfileId);
    }
}

/// <summary>EF Core configuration for the <see cref="QuizAnswer"/> entity.</summary>
internal sealed class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
{
    public void Configure(EntityTypeBuilder<QuizAnswer> builder)
    {
        builder.ToTable("quiz_answers");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.RowVersion).IsRowVersion();

        builder.Property(a => a.TextResponse).HasMaxLength(4000);
        builder.Property(a => a.MarksAwarded).HasColumnType("decimal(8,2)");

        builder.HasOne(a => a.Attempt)
               .WithMany(at => at.Answers)
               .HasForeignKey(a => a.QuizAttemptId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Question)
               .WithMany()
               .HasForeignKey(a => a.QuizQuestionId)
               .OnDelete(DeleteBehavior.Restrict);

        // One answer per question per attempt
        builder.HasIndex(a => new { a.QuizAttemptId, a.QuizQuestionId }).IsUnique();
    }
}
