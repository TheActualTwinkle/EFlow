namespace EFlow.Application.Bookings;

public record BookingDto
{
    public required Guid Id { get; init; }

    public required Guid StudentId { get; init; }

    public required Guid SlotId { get; init; }

    public required DateTime CreatedAt { get; init; }
}