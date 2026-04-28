using EFlow.Booking.Domain.Notifications;

namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record UpdateSubmissionSlotNotificationSettingsRequest
{
    public required Guid UserId { get; init; }

    public required ICollection<SubmissionRemindTime> SubmissionRemindTimes { get; init; }

    public BookingNotificationMode? BookingNotificationMode { get; init; }
}
