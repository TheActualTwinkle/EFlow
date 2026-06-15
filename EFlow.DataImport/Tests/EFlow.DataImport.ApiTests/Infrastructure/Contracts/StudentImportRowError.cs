namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record StudentImportRowError
{
    public required int RowNumber { get; init; }

    public required string Message { get; init; }
}
