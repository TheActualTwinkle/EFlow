using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects.Events;

public sealed class SubjectDeletedDomainEvent : DomainEvent
{
    public required SubjectId SubjectId { get; init; }
}