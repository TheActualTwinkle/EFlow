namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record SubmissionSlotReminderSnapshot
{
    public required SubmissionSlot SubmissionSlot { get; init; }

    public required IEnumerable<SubmissionSlotReminderRecipient> Recipients { get; init; }
}
