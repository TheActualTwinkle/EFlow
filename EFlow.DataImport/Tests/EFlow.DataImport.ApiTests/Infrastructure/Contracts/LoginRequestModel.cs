namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record LoginRequestModel
{
    public required string Username { get; init; }

    public required string Password { get; init; }
}
