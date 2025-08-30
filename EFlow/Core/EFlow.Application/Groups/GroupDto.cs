using EFlow.Application.Students;

namespace EFlow.Application.Groups;

public record GroupDto
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required ICollection<StudentDto> Students { get; init; }
}