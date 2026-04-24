using EFlow.Booking.Domain.Common.BusinessRules;
using EFlow.Booking.Domain.Notifications;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;

public sealed class SubmissionSlotNotificationSettings : Entity
{
    public SubmissionSlotNotificationSettingsId Id { get; }

    internal SubmissionSlotId SubmissionSlotId { get; private set; }

    internal Guid UserId { get; private set; }

    internal ReminderSchedule[] ReminderSchedules { get; private set; } = [];

    internal BookingNotificationMode? BookingNotificationMode { get; private set; }

    internal DateTime CreatedAt { get; private set; }

    private SubmissionSlotNotificationSettings() { }

    private SubmissionSlotNotificationSettings(
        SubmissionSlotId submissionSlotId,
        Guid userId,
        ReminderSchedule[] reminderSchedules,
        BookingNotificationMode? bookingNotificationMode,
        DateTime createdAt,
        DateTime utcNow)
    {
        ThrowIfBroken(new CreationTimeMustBeInPastRule(createdAt, utcNow));

        Id = new SubmissionSlotNotificationSettingsId(Guid.CreateVersion7());
        SubmissionSlotId = submissionSlotId;
        UserId = userId;
        ReminderSchedules = reminderSchedules.Distinct().ToArray();
        BookingNotificationMode = bookingNotificationMode;
        CreatedAt = createdAt;
    }

    internal static SubmissionSlotNotificationSettings Create(
        SubmissionSlotId submissionSlotId,
        Guid userId,
        ReminderSchedule[] reminderSchedules,
        BookingNotificationMode? bookingNotificationMode,
        DateTime createdAt,
        DateTime utcNow) =>
        new(submissionSlotId, userId, reminderSchedules, bookingNotificationMode, createdAt, utcNow);
}
