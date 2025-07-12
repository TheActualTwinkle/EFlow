using EFlow.Application.Common.Errors.Abstractions;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace EFlow.Application.Common.Errors.Identity;

public record IdentityInternalError : UnprocessableEntityError
{
    public override string? Message { get; init; }

    public override Dictionary<string, object>? Metadata { get; init; }

    public override List<IError>? Reasons { get; init; }
}

public static class IdentityInternalErrorBuilder
{
    /// <summary>
    ///     Extends an <see cref="IdentityInternalError" /> by adding a collection of <see cref="IdentityError" /> objects
    ///     to its <see cref="IdentityInternalError.Reasons" /> property. Each <see cref="IdentityError" /> is converted into a
    ///     <see cref="FluentResults.IError" />.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of <see cref="IdentityInternalError" /> being extended.
    /// </typeparam>
    /// <param name="error">
    ///     The <see cref="IdentityInternalError" /> instance to modify.
    /// </param>
    /// <param name="identityErrors">
    ///     A collection of <see cref="IdentityError" /> objects to add as reasons.
    /// </param>
    /// <returns>
    ///     The modified <see cref="IdentityInternalError" /> with the provided errors added to its
    ///     <see cref="IdentityInternalError.Reasons" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="error" /> or <paramref name="identityErrors" /> is <c>null</c>.
    /// </exception>
    public static T WithIdentityErrors<T>(this T error, IEnumerable<IdentityError> identityErrors) where T : IdentityInternalError
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(identityErrors);

        error = error with
        {
            Reasons = (error.Reasons ?? [])
            .Concat(identityErrors.Select(e => new Error(e.Description)))
            .ToList()
        };

        return error;
    }
}