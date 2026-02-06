using EFlow.Booking.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        builder.ToTable("admins");

        builder.HasKey(a => a.Id)
            .HasName("pk_admins");

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(a => a.Identity)
            .WithOne()
            .HasForeignKey<Admin>(a => a.Id)
            .HasConstraintName("fk_admins_identity");
    }
}