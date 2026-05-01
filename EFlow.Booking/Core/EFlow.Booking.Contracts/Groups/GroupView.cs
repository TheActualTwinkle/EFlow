namespace EFlow.Booking.Contracts.Groups;

public sealed record GroupView
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}