using EFlow.Booking.Domain.Subjects;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Events;

public sealed class SubmissionSlotCreatedDomainEvent : DomainEvent
{
    public required SubmissionSlotId SlotId { get; init; }

    public required SubjectId SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}