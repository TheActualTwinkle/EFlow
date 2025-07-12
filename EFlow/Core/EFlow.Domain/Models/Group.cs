namespace EFlow.Domain.Models;

public sealed class Group : IEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public ICollection<Student>? Students { get; init; }
}