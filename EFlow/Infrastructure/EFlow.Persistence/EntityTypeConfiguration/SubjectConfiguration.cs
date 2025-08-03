using EFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Persistence.EntityTypeConfiguration;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id)
            .HasName("pk_subjects");

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(127)
            .IsRequired();

        builder.Property(s => s.TeacherId)
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.Property(s => s.GroupIds)
            .HasColumnName("group_ids")
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.HasOne(s => s.Teacher)
            .WithMany()
            .HasForeignKey(s => s.TeacherId)
            .HasConstraintName("fk_subjects_teachers");

        builder.HasMany(s => s.Groups)
            .WithMany(g => g.Subjects)
            .UsingEntity("group_subject");
    }
}