using EFlow.Booking.Domain.Students;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Events;

public sealed class StudentAddedToGroupDomainEvent : DomainEvent
{
    public required GroupId GroupId { get; init; }
    
    public required StudentId StudentId { get; init; }
}