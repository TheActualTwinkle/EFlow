namespace EFlow.Contracts.Messages.Models;

public record SubmissionSlotModel
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }
}