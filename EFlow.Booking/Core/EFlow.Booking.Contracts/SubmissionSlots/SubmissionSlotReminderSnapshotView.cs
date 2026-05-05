namespace EFlow.Booking.Contracts.SubmissionSlots;

public sealed record SubmissionSlotReminderSnapshotView
{
    public required SubmissionSlotView SubmissionSlot { get; init; }

    public required SubmissionSlotReminderRecipientView[] Recipients { get; init; }
}
