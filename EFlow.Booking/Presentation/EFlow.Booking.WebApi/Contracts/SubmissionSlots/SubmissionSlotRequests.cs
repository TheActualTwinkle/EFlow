namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record CreateSubmissionSlotRequest
{
    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public required bool AllowAllGroups { get; init; }

    public ICollection<Guid>? AllowedGroupIds { get; init; }

    public string? Location { get; init; }
}
