namespace EFlow.Domain.Models;

public sealed class Booking
{
    public required Guid Id { get; init; }

    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Student? Student { get; init; }

    public SubmissionSlot? SubmissionSlot { get; init; }
}