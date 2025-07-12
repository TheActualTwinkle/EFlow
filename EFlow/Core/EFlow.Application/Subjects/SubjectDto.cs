namespace EFlow.Application.Subjects;

public record SubjectDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }
}