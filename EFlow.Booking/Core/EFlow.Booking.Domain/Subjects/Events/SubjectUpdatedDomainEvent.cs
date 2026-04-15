using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects.Events;

public sealed class SubjectUpdatedDomainEvent : DomainEvent
{
    public required SubjectId SubjectId { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
}