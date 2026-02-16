using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Events;

public sealed class GroupCreatedDomainEvent : DomainEvent
{
    public required GroupId GroupId { get; init; }
}