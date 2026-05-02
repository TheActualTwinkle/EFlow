using EFlow.Booking.Domain.Teachers;
using EFlow.Booking.Domain.Subjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class SubjectConfiguration : IEntityTypeConfiguration<Subject>
{
    private const string GroupIdValuePropertyName = "Value";

    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("subjects");

        builder.HasKey(s => s.Id)
            .HasName("pk_subjects");

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SubjectId(value))
            .HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(127)
            .IsRequired();

        builder.Property(s => s.TeacherId)
            .HasConversion(
                id => id.Value,
                value => new TeacherId(value))
            .HasColumnName("teacher_id")
            .IsRequired();

        builder.OwnsMany(
            s => s.GroupIds,
            groupsBuilder =>
            {
                groupsBuilder.ToTable("subject_groups");

                groupsBuilder.WithOwner()
                    .HasForeignKey("subject_id");

                groupsBuilder.Property(groupId => groupId.Value)
                    .HasColumnName("group_id")
                    .IsRequired();

                groupsBuilder.HasKey("subject_id", GroupIdValuePropertyName)
                    .HasName("pk_subject_groups");

                groupsBuilder.HasIndex(GroupIdValuePropertyName)
                    .HasDatabaseName("ix_subject_groups_group_id");
            });

        builder.HasOne<Teacher>()
            .WithMany()
            .HasForeignKey(s => s.TeacherId);
    }
}
