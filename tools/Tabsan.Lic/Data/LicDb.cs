using Microsoft.EntityFrameworkCore;
using Tabsan.Lic.Models;

namespace Tabsan.Lic.Data;

/// <summary>
/// EF Core SQLite DbContext for the Tabsan-Lic local database.
/// Stores all issued VerificationKeys so the operator can audit which keys exist
/// and whether a .tablic file has been generated for each.
/// </summary>
public class LicDb : DbContext
{
    public DbSet<IssuedKey> IssuedKeys => Set<IssuedKey>();

    public LicDb(DbContextOptions<LicDb> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<IssuedKey>(e =>
        {
            e.ToTable("issued_keys");
            e.HasKey(k => k.Id);
            e.Property(k => k.Id).ValueGeneratedOnAdd();
            e.Property(k => k.KeyId).IsRequired();
            e.HasIndex(k => k.KeyId).IsUnique();
            e.Property(k => k.VerificationKeyHash).IsRequired().HasMaxLength(64);
            e.HasIndex(k => k.VerificationKeyHash).IsUnique();
            e.Property(k => k.ExpiryType).HasConversion<string>().IsRequired();
            e.Property(k => k.IssuedAt).IsRequired();
            e.Property(k => k.Label).HasMaxLength(256);
            e.Property(k => k.IncludeSchool).HasDefaultValue(false);
            e.Property(k => k.IncludeCollege).HasDefaultValue(false);
            e.Property(k => k.IncludeUniversity).HasDefaultValue(true);
            // P3-S1-01: Phase 2 constraint fields
            e.Property(k => k.MaxUsers).HasDefaultValue(0);
            e.Property(k => k.AllowedDomain).HasMaxLength(253).IsRequired(false);
        });
    }
}
