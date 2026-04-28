using EFlow.Booking.Domain.Notifications;

namespace EFlow.Booking.Domain.SubmissionSlots.NotificationSettings;

public sealed record SubmissionSlotNotificationRecipient(
    Guid UserId,
    IEnumerable<SubmissionRemindTime> SubmissionRemindTimes,
    BookingNotificationMode? BookingNotificationMode);
