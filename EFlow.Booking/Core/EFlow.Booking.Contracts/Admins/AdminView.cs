namespace EFlow.Booking.Contracts.Admins;

public sealed record AdminView
{
    public required Guid Id { get; init; }

    public required DateTime CreatedAt { get; init; }
}