using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Events;

public sealed class GroupUpdatedDomainEvent : DomainEvent
{
    public required GroupId GroupId { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
}