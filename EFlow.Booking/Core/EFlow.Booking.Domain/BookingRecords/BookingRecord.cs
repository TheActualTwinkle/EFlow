using EFlow.Booking.Domain.SubmissionSlots;
using EFlow.Common.Domain;
using EFlow.Common.Domain.Students;

namespace EFlow.Booking.Domain.BookingRecords;

public sealed class BookingRecord : Entity
{
    public required Guid Id { get; init; }

    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Student? Student { get; init; }

    public SubmissionSlot? SubmissionSlot { get; init; }
}