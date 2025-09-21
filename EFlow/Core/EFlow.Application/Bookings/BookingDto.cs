using EFlow.Application.Students;
using EFlow.Application.SubmissionSlots;

namespace EFlow.Application.Bookings;

public record BookingDto
{
    public required Guid Id { get; init; }

    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public StudentDto? Student { get; init; }

    public SubmissionSlotDto? SubmissionSlot { get; init; }
}