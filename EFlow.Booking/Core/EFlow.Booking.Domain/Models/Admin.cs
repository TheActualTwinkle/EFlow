namespace EFlow.Booking.Domain.Models;

public sealed class Admin : IEntity
{
    public required Guid Id { get; init; }

    public required DateTime CreatedAt { get; init; }

    public Identity? Identity { get; init; }
}