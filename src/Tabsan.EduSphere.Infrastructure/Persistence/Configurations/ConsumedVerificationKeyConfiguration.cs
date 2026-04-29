using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tabsan.EduSphere.Domain.Licensing;

namespace Tabsan.EduSphere.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the ConsumedVerificationKey table.
/// Each row records the hash of a VerificationKey that has been used to activate
/// a license on this EduSphere installation.  Rows are never deleted.
/// </summary>
public class ConsumedVerificationKeyConfiguration
    : IEntityTypeConfiguration<ConsumedVerificationKey>
{
    public void Configure(EntityTypeBuilder<ConsumedVerificationKey> builder)
    {
        builder.ToTable("consumed_verification_keys");

        builder.HasKey(k => k.Id);

        builder.Property(k => k.KeyHash)
               .IsRequired()
               .HasMaxLength(64);

        builder.HasIndex(k => k.KeyHash)
               .IsUnique();

        builder.Property(k => k.ConsumedAt)
               .IsRequired();
    }
}
