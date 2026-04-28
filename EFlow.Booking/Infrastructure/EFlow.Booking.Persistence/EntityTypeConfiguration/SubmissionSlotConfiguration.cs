using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

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
            .HasConversion(id => id.Value, value => new SubmissionSlotId(value))
            .HasColumnName("id");

        builder.Property(s => s.SubjectId)
            .HasConversion(id => id.Value, value => new SubjectId(value))
            .HasColumnName("subject_id")
            .IsRequired();

        builder.Property(s => s.TeacherId)
            .HasConversion(id => id.Value, value => new TeacherId(value))
            .HasColumnName("teacher_id")
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

        builder.Property(s => s.Comment)
            .HasColumnName("comment")
            .HasMaxLength(1023);

        builder.Property(s => s.AllowAllGroups)
            .HasColumnName("allow_all_groups")
            .IsRequired();

        builder.Property(s => s.AllowedGroupIds)
            .HasConversion(
                ids => ids.Select(x => x.Value).ToArray(),
                values => values
                    .Select(value => new GroupId(value))
                    .ToArray())
            .Metadata.SetValueComparer(
                new ValueComparer<ICollection<GroupId>>(
                    (left, right) => left!.SequenceEqual(right!),
                    ids => ids.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.Value.GetHashCode())),
                    ids => ids.ToArray()));
        
        builder.Property(s => s.AllowedGroupIds)
            .HasColumnName("allowed_group_ids")
            .HasColumnType("uuid[]");
    }
}
