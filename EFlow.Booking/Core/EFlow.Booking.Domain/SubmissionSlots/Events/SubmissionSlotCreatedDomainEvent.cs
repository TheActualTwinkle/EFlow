using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Subjects;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots.Events;

public sealed class SubmissionSlotCreatedDomainEvent : DomainEvent
{
    public required SubmissionSlotId SlotId { get; init; }

    public required SubjectId SubjectId { get; init; }
    
    public required TeacherId TeacherId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
    
    public string? Comment { get; init; }
    
    public required bool AllowAllGroups { get; init; }

    public required IEnumerable<GroupId> AllowedGroupIds { get; init; }
    
    public required DateTime CreatedAt { get; init; }
}