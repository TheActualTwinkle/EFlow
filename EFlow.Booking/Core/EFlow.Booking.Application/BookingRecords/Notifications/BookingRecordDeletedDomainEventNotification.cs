using EFlow.Booking.Domain.BookingRecords.Events;
using EFlow.Common.Infrastructure;

namespace EFlow.Booking.Application.BookingRecords.Notifications;

public sealed record BookingRecordDeletedDomainEventNotification : IDomainNotification<BookingRecordDeletedDomainEvent>
{
    public required BookingRecordDeletedDomainEvent DomainEvent { get; init; }
}
