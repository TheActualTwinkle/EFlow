namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record SubmissionSlotReminderSnapshot
{
    public required Guid SlotId { get; init; }

    public required DateTime SlotStartTime { get; init; }

    public required IEnumerable<SubmissionSlotReminderRecipient> Recipients { get; init; }
}
