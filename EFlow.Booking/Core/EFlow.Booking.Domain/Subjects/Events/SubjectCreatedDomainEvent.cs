using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Domain.Teachers;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Subjects.Events;

public class SubjectCreatedDomainEvent : DomainEvent
{
    public required SubjectId SubjectId { get; init; }
    
    public required TeacherId TeacherId { get; init; }
    
    public required ICollection<GroupId> GroupIds { get; init; }
}