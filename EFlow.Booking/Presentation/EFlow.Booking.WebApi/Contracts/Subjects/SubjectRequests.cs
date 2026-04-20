namespace EFlow.Booking.WebApi.Contracts.Subjects;

public record CreateSubjectRequest
{
    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }

    public required ICollection<Guid> GroupIds { get; init; }
}
