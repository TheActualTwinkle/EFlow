using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Events;

public sealed class StudentUpdatedDomainEvent : DomainEvent
{
    public required StudentId StudentId { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
}