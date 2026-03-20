using EFlow.Booking.Subjects;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups.Events;

public sealed class SubjectRemovedFromGroupDomainEvent : DomainEvent
{
    public required GroupId GroupId { get; init; }
    
    public required SubjectId SubjectId { get; init; }
}