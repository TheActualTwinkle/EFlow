namespace EFlow.Booking.Application.SubmissionSlots;

public sealed record SubmissionSlotReminderRecipientDto
{
    public required Guid UserId { get; init; }

    public string? Email { get; init; }
}
