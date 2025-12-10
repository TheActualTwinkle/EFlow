using FluentResults;

namespace EFlow.Booking.Application.Common.Errors.Abstractions;

public abstract record ApplicationError : IError
{
    public abstract string? Message { get; init; }

    public abstract Dictionary<string, object>? Metadata { get; init; }

    public abstract List<IError>? Reasons { get; init; }
}

public static class ApplicationErrorBuilder
{
    /// <summary>
    ///     Extends an <see cref="ApplicationError" /> by setting its <see cref="ApplicationError.Message" /> property.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of <see cref="ApplicationError" /> being extended.
    /// </typeparam>
    /// <param name="error">
    ///     The <see cref="ApplicationError" /> instance to modify.
    /// </param>
    /// <param name="message">
    ///     The new message to set for the <see cref="ApplicationError" />.
    /// </param>
    /// <returns>
    ///     The modified <see cref="ApplicationError" /> with the provided message.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="error" /> or <paramref name="message" /> is <c>null</c>.
    /// </exception>
    public static T WithMessage<T>(this T error, string message) where T : ApplicationError
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(message);

        error = error with { Message = message };

        return error;
    }
}