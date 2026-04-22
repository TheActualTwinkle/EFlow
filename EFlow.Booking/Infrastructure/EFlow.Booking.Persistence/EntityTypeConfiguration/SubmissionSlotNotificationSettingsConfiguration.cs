using EFlow.Booking.Domain;
using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFlow.Booking.Persistence.EntityTypeConfiguration;

public class SubmissionSlotNotificationSettingsConfiguration : IEntityTypeConfiguration<SubmissionSlotNotificationSettings>
{
    public void Configure(EntityTypeBuilder<SubmissionSlotNotificationSettings> builder)
    {
        builder.ToTable("submission_slot_notification_settings");

        builder.HasKey(settings => settings.Id)
            .HasName("pk_submission_slot_notification_settings");

        builder.Property(settings => settings.Id)
            .HasConversion(id => id.Value, value => new SubmissionSlotNotificationSettingsId(value))
            .HasColumnName("id");

        builder.Property(settings => settings.SubmissionSlotId)
            .HasConversion(id => id.Value, value => new SubmissionSlotId(value))
            .HasColumnName("submission_slot_id")
            .IsRequired();

        builder.Property(settings => settings.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(settings => settings.ReminderSchedule)
            .HasColumnName("reminder_schedule")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(settings => settings.BookingNotificationMode)
            .HasColumnName("booking_notification_mode")
            .HasConversion<int?>();

        builder.Property(settings => settings.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne<SubmissionSlot>()
            .WithMany(slot => slot.NotificationSettings)
            .HasForeignKey(settings => settings.SubmissionSlotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
