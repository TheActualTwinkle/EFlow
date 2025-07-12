namespace EFlow.Application.Students;

public record StudentDto
{
    public required Guid Id { get; init; }

    public required Guid GroupId { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public required DateTime CreatedAt { get; init; }
}