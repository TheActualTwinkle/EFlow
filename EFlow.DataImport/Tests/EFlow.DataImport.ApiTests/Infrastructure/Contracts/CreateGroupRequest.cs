namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record CreateGroupRequest
{
    public required string Name { get; init; }
}
