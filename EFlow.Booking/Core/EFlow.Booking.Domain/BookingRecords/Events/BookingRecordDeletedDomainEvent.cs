using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.BookingRecords.Events;

public sealed class BookingRecordDeletedDomainEvent : DomainEvent
{
    public required BookingRecordId BookingRecordId { get; init; }
}