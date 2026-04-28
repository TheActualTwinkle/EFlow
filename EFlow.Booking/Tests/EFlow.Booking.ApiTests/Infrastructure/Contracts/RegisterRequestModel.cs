using EFlow.Booking.Domain;

namespace EFlow.Booking.ApiTests.Infrastructure.Contracts;

/// <summary>
/// Represents the registration request payload sent by API tests.
/// </summary>
internal sealed record RegisterRequestModel
{
    /// <summary>
    /// Gets the username for the account being created.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// Gets the password for the account being created.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Gets the email address for the account being created.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the role assigned to the account being created.
    /// </summary>
    public required Identity.Role Role { get; init; }
}