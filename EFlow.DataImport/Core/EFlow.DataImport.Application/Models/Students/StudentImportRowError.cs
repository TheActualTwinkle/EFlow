namespace EFlow.DataImport.Application.Models.Students;

public sealed record StudentImportRowError
{
    public required int RowNumber { get; init; }

    public required string Message { get; init; }
}
