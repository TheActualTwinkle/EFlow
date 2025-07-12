namespace EFlow.Domain.Models;

public sealed class Subject : IEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }

    public Teacher? Teacher { get; init; }
}