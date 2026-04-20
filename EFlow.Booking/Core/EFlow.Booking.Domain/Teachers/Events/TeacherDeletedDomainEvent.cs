using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Teachers.Events;

public sealed class TeacherDeletedDomainEvent : DomainEvent
{
    public required TeacherId TeacherId { get; init; }
}