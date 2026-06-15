using System.Net;

namespace EFlow.DataImport.Application.Models.Students;

public sealed record StudentImportProxyResult
{
    public required HttpStatusCode StatusCode { get; init; }

    public string? Body { get; init; }

    public string? ContentType { get; init; }

    public static StudentImportProxyResult FromStatus(HttpStatusCode statusCode, string? body = null) =>
        new()
        {
            StatusCode = statusCode,
            Body = body
        };
}
