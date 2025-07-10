using EFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Persistence.EntityTypeConfiguration;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");

        builder.HasKey(a => a.IdentityId)
            .HasName("pk_admins");

        builder.Property(a => a.IdentityId)
            .HasColumnName("identity_id");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(a => a.Identity)
            .WithOne()
            .HasForeignKey<Admin>(a => a.IdentityId)
            .HasConstraintName("fk_admins_identity");
    }
}