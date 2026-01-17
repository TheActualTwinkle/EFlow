namespace EFlow.Common.Domain.Models;

public sealed class Group : IEntity
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public ICollection<Student>? Students { get; init; }

    public ICollection<Subject>? Subjects { get; init; }

    public ICollection<SubmissionSlot>? SubmissionSlots { get; init; }
}