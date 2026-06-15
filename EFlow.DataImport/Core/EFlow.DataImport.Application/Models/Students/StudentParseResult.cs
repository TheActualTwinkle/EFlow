namespace EFlow.DataImport.Application.Models.Students;

internal sealed record StudentParseResult
{
    public int TotalCount { get; init; }

    public IReadOnlyList<ImportedStudent> Students { get; init; } = [];

    public IReadOnlyList<StudentImportRowError> Errors { get; init; } = [];

    public string? InvalidMappingMessage { get; init; }
}
