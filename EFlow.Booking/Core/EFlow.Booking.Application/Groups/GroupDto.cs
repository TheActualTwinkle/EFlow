namespace EFlow.Booking.Application.Groups;

public record GroupDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}