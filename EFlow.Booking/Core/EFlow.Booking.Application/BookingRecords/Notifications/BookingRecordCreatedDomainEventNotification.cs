using EFlow.Booking.Domain.BookingRecords.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.BookingRecords.Notifications;

public sealed record BookingRecordCreatedDomainEventNotification : IDomainNotification<BookingRecordCreatedDomainEvent>
{
    public required BookingRecordCreatedDomainEvent DomainEvent { get; init; }
}
