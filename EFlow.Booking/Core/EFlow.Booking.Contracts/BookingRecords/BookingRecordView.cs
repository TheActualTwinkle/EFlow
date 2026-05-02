using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.SubmissionSlots;

namespace EFlow.Booking.Contracts.BookingRecords;

public sealed record BookingRecordView
{
    public required Guid Id { get; init; }

    public required StudentView? Student { get; init; }

    public required SubmissionSlotView? Slot { get; init; }

    public required DateTime CreatedAt { get; init; }
}