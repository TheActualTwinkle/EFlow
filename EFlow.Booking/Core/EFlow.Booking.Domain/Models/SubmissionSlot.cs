namespace EFlow.Common.Domain.Models;

public sealed class SubmissionSlot : IEntity
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }
    
    public required bool AllowAllGroups { get; init; }
    
    // [MemberNotNullWhen(false, nameof(AllowedGroupIds))] // TODO: Check if this works as expected in calling code
    public ICollection<Guid>? AllowedGroupIds { get; init; }

    public string? Location { get; init; }

    public Subject? Subject { get; init; }
    
    public ICollection<Group>? AllowedGroups { get; init; }
}