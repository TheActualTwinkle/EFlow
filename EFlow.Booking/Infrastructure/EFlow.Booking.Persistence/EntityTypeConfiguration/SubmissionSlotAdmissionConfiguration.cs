using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.Admissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class SubmissionSlotAdmissionConfiguration : IEntityTypeConfiguration<SubmissionSlotAdmission>
{
    public void Configure(EntityTypeBuilder<SubmissionSlotAdmission> builder)
    {
        builder.ToTable("submission_slot_admissions");

        builder.HasKey(admission => admission.Id)
            .HasName("pk_submission_slot_admissions");

        builder.Property(admission => admission.Id)
            .HasConversion(id => id.Value, value => new SubmissionSlotAdmissionId(value))
            .HasColumnName("id");

        builder.Property(admission => admission.SubmissionSlotId)
            .HasConversion(id => id.Value, value => new SubmissionSlotId(value))
            .HasColumnName("submission_slot_id")
            .IsRequired();

        builder.Property(admission => admission.StudentId)
            .HasConversion(id => id.Value, value => new StudentId(value))
            .HasColumnName("student_id")
            .IsRequired();

        builder.Property(admission => admission.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<SubmissionSlot>()
            .WithMany(slot => slot.Admissions)
            .HasForeignKey(admission => admission.SubmissionSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Student>()
            .WithMany()
            .HasForeignKey(admission => admission.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
