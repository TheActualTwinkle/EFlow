using EFlow.Booking.Domain.Notifications;

namespace EFlow.Booking.Contracts.SubmissionSlots;

public sealed record SubmissionSlotNotificationSettingsView
{
    public required Guid UserId { get; init; }

    public required IReadOnlyCollection<SubmissionRemindTime> SubmissionRemindTimes { get; init; }

    public BookingNotificationMode? BookingNotificationMode { get; init; }
}
