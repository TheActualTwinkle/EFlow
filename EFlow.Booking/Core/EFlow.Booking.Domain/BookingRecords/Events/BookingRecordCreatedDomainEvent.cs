using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.BookingRecords.Events;

public sealed class BookingRecordCreatedDomainEvent : DomainEvent
{
    public required BookingRecordId BookingRecordId { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}