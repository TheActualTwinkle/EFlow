using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Events;

public sealed class GroupDeletedDomainEvent : DomainEvent
{
    public required GroupId GroupId { get; init; }
}