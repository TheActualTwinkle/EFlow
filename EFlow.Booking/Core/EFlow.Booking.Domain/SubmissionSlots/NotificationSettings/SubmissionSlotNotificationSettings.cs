using EFlow.Booking.Domain.Notifications;
using EFlow.Booking.Domain.SubmissionSlots.Rules;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;

public sealed class SubmissionSlotNotificationSettings : Entity
{
    public SubmissionSlotNotificationSettingsId Id { get; }

    internal SubmissionSlotId SubmissionSlotId { get; private set; }

    internal Guid UserId { get; private set; }

    internal ICollection<SubmissionRemindTime> SubmissionRemindTimes { get; private set; } = [];

    internal BookingNotificationMode? BookingNotificationMode { get; private set; }
    
    private SubmissionSlotNotificationSettings() { }

    private SubmissionSlotNotificationSettings(
        SubmissionSlotId submissionSlotId,
        Guid userId,
        ICollection<Guid> usersWithoutNotifications,
        ICollection<SubmissionRemindTime> submissionRemindTimes,
        BookingNotificationMode? bookingNotificationMode)
    {
        ThrowIfBroken(new UserMustNotBeInUsersWithoutNotificationsRule(userId, usersWithoutNotifications));

        Id = new SubmissionSlotNotificationSettingsId(Guid.CreateVersion7());
        SubmissionSlotId = submissionSlotId;
        UserId = userId;
        SubmissionRemindTimes = submissionRemindTimes.Distinct().ToArray();
        BookingNotificationMode = bookingNotificationMode;
    }

    internal static SubmissionSlotNotificationSettings Create(
        SubmissionSlotId submissionSlotId,
        Guid userId,
        ICollection<Guid> usersWithoutNotifications,
        ICollection<SubmissionRemindTime> submissionRemindTime,
        BookingNotificationMode? bookingNotificationMode) =>
        new(
            submissionSlotId,
            userId,
            usersWithoutNotifications,
            submissionRemindTime,
            bookingNotificationMode);
}
