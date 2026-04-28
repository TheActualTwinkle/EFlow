namespace EFlow.Booking.ApiTests.Infrastructure.Contracts;

/// <summary>
/// Represents the login request payload sent by API tests.
/// </summary>
internal sealed record LoginRequestModel
{
    /// <summary>
    /// Gets the username of the account being authenticated.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the password of the account being authenticated.
    /// </summary>
    public required string Password { get; init; }
}