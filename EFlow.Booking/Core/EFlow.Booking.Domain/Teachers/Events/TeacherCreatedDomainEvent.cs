using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Events;

public sealed class TeacherCreatedDomainEvent : DomainEvent
{
    public required TeacherId TeacherId { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}