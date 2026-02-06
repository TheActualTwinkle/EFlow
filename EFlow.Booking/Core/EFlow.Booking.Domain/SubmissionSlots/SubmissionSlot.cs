using System.Diagnostics.CodeAnalysis;
using EFlow.Booking.Domain.Admins;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Subjects;
using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.SubmissionSlots;

public sealed class SubmissionSlot : Entity
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    [MemberNotNullWhen(false, nameof(AllowedGroupIds))]
    public required bool AllowAllGroups { get; init; }
    
    public ICollection<Guid>? AllowedGroupIds { get; init; }

    public string? Location { get; init; }

    public Subject? Subject { get; init; }
    
    public ICollection<Group>? AllowedGroups { get; init; }
}