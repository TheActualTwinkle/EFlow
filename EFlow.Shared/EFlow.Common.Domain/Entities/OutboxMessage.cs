namespace EFlow.Common.Domain.Entities;

public record OutboxMessage : IEntity
{
    public required string Type { get; init; }

    public required byte[] Payload { get; init; }

    public required DateTime CreatedAt { get; init; }

    public DateTime? ProcessedAt { get; init; }

    public string? ErrorMessage { get; init; }

    public required Guid Id { get; init; }
}