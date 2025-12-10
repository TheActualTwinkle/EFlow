namespace EFlow.Booking.Domain.Models;

public record OutboxMessage : IEntity
{
    public required Guid Id { get; init; }

    public required string Type { get; init; }

    public required byte[] Payload { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }

    public string? ErrorMessage { get; init; }
}