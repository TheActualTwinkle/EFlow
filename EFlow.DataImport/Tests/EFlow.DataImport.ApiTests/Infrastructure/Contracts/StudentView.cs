namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record StudentView
{
    public required Guid Id { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public string? MiddleName { get; init; }

    public required DateOnly BirthDate { get; init; }

    public GroupView? Group { get; init; }
}
