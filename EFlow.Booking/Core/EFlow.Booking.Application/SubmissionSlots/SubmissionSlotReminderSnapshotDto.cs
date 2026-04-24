namespace EFlow.Booking.Application.SubmissionSlots;

public sealed record SubmissionSlotReminderSnapshotDto
{
    public required Guid SlotId { get; init; }

    public required DateTime SlotStartTime { get; init; }

    public required SubmissionSlotReminderRecipientDto[] Recipients { get; init; }
}
