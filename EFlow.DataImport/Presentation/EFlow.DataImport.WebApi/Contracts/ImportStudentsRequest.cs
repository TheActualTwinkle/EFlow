using System.ComponentModel;

namespace EFlow.DataImport.WebApi.Contracts;

public sealed record ImportStudentsRequest
{
    public required IFormFile File { get; init; }

    [DefaultValue(new[]
    {
        "LastName",
        "FirstName",
        "MiddleName",
        "Email",
        "UserName",
        "Password",
        "BirthDate"
    })]
    public required string[] Fields { get; init; }

    public bool HasHeaderRow { get; init; }
}
