using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record SubmissionSlotReminderRecipient
{
    public required Guid UserId { get; init; }

    public required string Email { get; init; }
    
    public required SubmissionRemindTimeModel RemindTime { get; init; }
}
