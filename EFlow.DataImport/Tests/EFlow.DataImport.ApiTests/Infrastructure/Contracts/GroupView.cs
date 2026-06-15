namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record GroupView
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }
}
