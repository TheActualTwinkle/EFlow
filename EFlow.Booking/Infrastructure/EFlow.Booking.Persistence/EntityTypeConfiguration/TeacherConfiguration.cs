using EFlow.Booking.Domain.Teachers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.HasKey(t => t.Id)
            .HasName("pk_teachers");

        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, value => new TeacherId(value))
            .HasColumnName("id");

        builder.Property(t => t.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(31) // TODO: взять из IBussinessRule
            .IsRequired();

        builder.Property(t => t.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(31);

        builder.Property(t => t.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(31)
            .IsRequired();

        builder.Property(t => t.BirthDate)
            .HasColumnName("birth_date")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
