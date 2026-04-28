using EFlow.Common.Domain;
using EFlow.Booking.Domain.Students;
using EFlow.Booking.Domain.SubmissionSlots;

namespace EFlow.Booking.Domain.BookingRecords.Events;

public sealed class BookingRecordDeletedDomainEvent : DomainEvent
{
    public required BookingRecordId BookingRecordId { get; init; }

    public required StudentId StudentId { get; init; }

    public required SubmissionSlotId SlotId { get; init; }

    public required DateTime CancelledAt { get; init; }
}
