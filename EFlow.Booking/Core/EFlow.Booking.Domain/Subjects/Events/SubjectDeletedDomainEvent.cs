using EFlow.Common.Domain;

namespace EFlow.Booking.Subjects.Events;

public sealed class SubjectDeletedDomainEvent : DomainEvent
{
    public required SubjectId SubjectId { get; init; }
}