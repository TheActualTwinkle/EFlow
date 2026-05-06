namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record Subject
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}