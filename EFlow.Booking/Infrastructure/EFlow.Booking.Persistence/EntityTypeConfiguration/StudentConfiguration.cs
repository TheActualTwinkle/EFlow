using EFlow.Common.Domain.Models;
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
            .HasColumnName("id");

        builder.Property(s => s.GroupId)
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

        builder.HasOne(s => s.Identity)
            .WithOne()
            .HasForeignKey<Student>(s => s.Id)
            .HasConstraintName("fk_students_identity");

        builder.HasOne(s => s.Group)
            .WithMany(g => g.Students)
            .HasForeignKey(s => s.GroupId)
            .HasConstraintName("fk_students_groups");
    }
}