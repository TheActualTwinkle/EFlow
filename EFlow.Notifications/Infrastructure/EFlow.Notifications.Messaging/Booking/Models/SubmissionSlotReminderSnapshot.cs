using EFlow.Common.IntegrationEvents.Booking.Models;

namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record SubmissionSlotReminderSnapshot
{
    public required SubmissionSlotModel SubmissionSlot { get; init; }

    public required IEnumerable<SubmissionSlotReminderRecipient> Recipients { get; init; }
}
