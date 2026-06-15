namespace EFlow.DataImport.Application.Models.Students;

public sealed record StudentImportProxyRequest
{
    public required Guid GroupId { get; init; }

    public required Stream FileStream { get; init; }

    public required string FileName { get; init; }

    public string? ContentType { get; init; }

    public required IReadOnlyList<string> Fields { get; init; }

    public bool HasHeaderRow { get; init; }
}
