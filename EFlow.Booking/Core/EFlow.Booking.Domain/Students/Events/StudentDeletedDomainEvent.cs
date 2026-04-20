using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Students.Events;

public sealed class StudentDeletedDomainEvent : DomainEvent
{
    public required StudentId StudentId { get; init; }
}