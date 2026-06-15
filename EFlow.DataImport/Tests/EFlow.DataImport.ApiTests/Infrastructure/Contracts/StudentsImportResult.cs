namespace EFlow.DataImport.ApiTests.Infrastructure.Contracts;

public sealed record StudentsImportResult
{
    public required int TotalCount { get; init; }

    public required int ImportedCount { get; init; }

    public required int FailedCount { get; init; }

    public required IReadOnlyList<StudentImportRowError> Errors { get; init; }
}
