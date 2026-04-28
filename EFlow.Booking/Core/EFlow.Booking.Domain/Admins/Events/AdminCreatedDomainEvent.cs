using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins.Events;

public sealed class AdminCreatedDomainEvent : DomainEvent
{
    public required AdminId AdminId { get; init; }

    public required DateTime CreatedAt { get; init; }
}