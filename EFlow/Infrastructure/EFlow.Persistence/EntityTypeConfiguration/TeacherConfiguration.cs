using EFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Persistence.EntityTypeConfiguration;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.ToTable("teachers");

        builder.HasKey(t => t.IdentityId)
            .HasName("pk_teachers");

        builder.Property(t => t.IdentityId)
            .HasColumnName("identity_id");

        builder.Property(t => t.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(31)
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
            .HasColumnName("created_at");

        builder.HasOne(t => t.Identity)
            .WithOne()
            .HasForeignKey<Teacher>(t => t.IdentityId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_teachers_identity");
    }
}