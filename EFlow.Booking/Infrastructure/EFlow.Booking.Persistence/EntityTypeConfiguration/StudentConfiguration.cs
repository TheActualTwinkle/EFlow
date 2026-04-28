using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("students");

        builder.HasKey(s => s.Id)
            .HasName("pk_students");

        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,
                value => new StudentId(value))
            .HasColumnName("id");

        builder.Property(s => s.GroupId)
            .HasConversion(
                id => id.Value,
                value => new GroupId(value))
            .HasColumnName("group_id")
            .IsRequired();

        builder.Property(s => s.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(31)
            .IsRequired();

        builder.Property(s => s.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(31);

        builder.Property(s => s.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(31)
            .IsRequired();

        builder.Property(s => s.BirthDate)
            .HasColumnName("birth_date")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
