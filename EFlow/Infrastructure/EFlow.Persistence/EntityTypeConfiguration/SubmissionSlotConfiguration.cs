using EFlow.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Persistence.EntityTypeConfiguration;

public class SubmissionSlotConfiguration : IEntityTypeConfiguration<SubmissionSlot>
{
    public void Configure(EntityTypeBuilder<SubmissionSlot> builder)
    {
        builder.ToTable(
            "submission_slots",
            t =>
            {
                t.HasCheckConstraint(
                    "CK_SubmissionSlots_ValidTimeRange",
                    "start_time < end_time");
            });

        builder.HasKey(s => s.Id)
            .HasName("pk_submission_slots");

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.SubjectId)
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(s => s.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(s => s.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(s => s.MaxStudents)
            .HasColumnName("max_students")
            .IsRequired();

        builder.Property(s => s.Location)
            .HasColumnName("location")
            .HasMaxLength(127);

        builder.Property(s => s.IsForAllGroups)
            .HasColumnName("is_for_all_groups")
            .IsRequired();

        builder.Property(s => s.AllowedGroupIds)
            .HasColumnName("allowed_group_ids")
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.HasOne(s => s.Subject)
            .WithMany()
            .HasForeignKey(s => s.SubjectId)
            .HasConstraintName("fk_submission_slots_subjects");

        builder.HasMany(s => s.AllowedGroups)
            .WithMany(g => g.SubmissionSlots)
            .UsingEntity("group_submission_slot");
    }
}