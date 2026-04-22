using EFlow.Booking.Domain.Notifications;

namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record UpdateSubmissionSlotNotificationSettingsRequest
{
    public required Guid UserId { get; init; }

    public required ReminderSchedule ReminderSchedule { get; init; }

    public BookingNotificationMode? BookingNotificationMode { get; init; }
}
