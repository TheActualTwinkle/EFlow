using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Events;

public sealed class StudentCreatedDomainEvent : DomainEvent
{
    public required StudentId StudentId { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}