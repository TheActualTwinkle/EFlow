using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins.Events;

public sealed class AdminDeletedDomainEvent : DomainEvent
{
    public required AdminId AdminId { get; init; }
}