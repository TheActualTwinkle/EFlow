using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Events;

public sealed class StudentChangedGroupDomainEvent : DomainEvent
{
    public required StudentId StudentId { get; init; }
    
    public required GroupId OldGroupId { get; init; }
    
    public required GroupId NewGroupId { get; init; }
    
    public required DateTime ChangedAt { get; init; }
}