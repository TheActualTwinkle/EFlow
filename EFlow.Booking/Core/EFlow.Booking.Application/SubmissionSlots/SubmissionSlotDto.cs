namespace EFlow.Booking.Application.SubmissionSlots;

public record SubmissionSlotDto
{
    public required Guid Id { get; init; }

    public required Guid SubjectId { get; init; }

    public required Guid TeacherId { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public string? Location { get; init; }

    public string? Comment { get; init; }
}
