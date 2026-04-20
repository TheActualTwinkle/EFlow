namespace EFlow.Booking.WebApi.Contracts.SubmissionSlots;

public record UpdateSubmissionSlotRequest
{
    public Guid? SubjectId { get; init; }

    public DateTime? StartTime { get; init; }

    public DateTime? EndTime { get; init; }

    public int? MaxStudents { get; init; }

    public string? Location { get; init; }
}
