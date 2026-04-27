using EFlow.Booking.Domain.Groups;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(g => g.Id)
            .HasName("pk_groups");

        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, value => new GroupId(value))
            .HasColumnName("id");

        builder.Property(g => g.Name)
            .HasColumnName("name")
            .HasMaxLength(64)
            .IsRequired();
    }
}