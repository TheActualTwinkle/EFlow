using EFlow.Application.Common.Errors.Abstractions;
using FluentResults;

namespace EFlow.Application.Common.Errors;

public record NotFoundError : ApplicationError
{
    public override string? Message { get; init; }

    public override Dictionary<string, object>? Metadata { get; init; }

    public override List<IError>? Reasons { get; init; }
}

public static class NotFoundErrorBuilder
{
    /// <summary>
    ///     Extends a <see cref="NotFoundError" /> by adding the specified ID to its metadata.
    ///     ID represents a unique identifier of the resource that was not found.
    /// </summary>
    /// <param name="error">
    ///     The <see cref="NotFoundError" /> instance to modify.
    /// </param>
    /// <param name="id">
    ///     The <see cref="Guid" /> ID of the resource that was not found.
    ///     This will be stored under the "Id" key in the error's metadata.
    /// </param>
    /// <returns>
    ///     The modified <see cref="NotFoundError" /> with the ID added to its metadata.
    ///     If the original metadata was null, returns a new record instance with an initialized dictionary.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="error" /> is <c>null</c>.
    /// </exception>
    public static NotFoundError WithId(this NotFoundError error, Guid id)
    {
        ArgumentNullException.ThrowIfNull(error);

        if (error.Metadata is null)
            error = error with { Metadata = new Dictionary<string, object>() };

        error.Metadata["Id"] = id;

        return error;
    }
}