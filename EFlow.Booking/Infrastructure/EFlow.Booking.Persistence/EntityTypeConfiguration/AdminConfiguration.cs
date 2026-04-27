using EFlow.Booking.Domain.Admins;
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
            .HasConversion(id => id.Value, value => new AdminId(value))
            .HasColumnName("id");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
