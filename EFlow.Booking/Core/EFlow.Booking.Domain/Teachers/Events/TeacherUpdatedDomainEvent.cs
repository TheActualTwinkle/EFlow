using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Events;

public sealed class TeacherUpdatedDomainEvent : DomainEvent
{
    public required TeacherId TeacherId { get; init; }
    
    public required DateTime UpdatedAt { get; init; }
}