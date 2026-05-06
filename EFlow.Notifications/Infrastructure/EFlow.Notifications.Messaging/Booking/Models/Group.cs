namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record Group
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}