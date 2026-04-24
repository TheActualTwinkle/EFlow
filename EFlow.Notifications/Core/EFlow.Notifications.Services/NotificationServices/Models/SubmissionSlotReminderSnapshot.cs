namespace EFlow.Notifications.Services.NotificationServices.Models;

public sealed record SubmissionSlotReminderSnapshot
{
    public required Guid SlotId { get; init; }

    public required DateTime SlotStartTime { get; init; }

    public required SubmissionSlotReminderRecipient[] Recipients { get; init; }
}
