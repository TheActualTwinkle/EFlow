using EFlow.Booking.Domain;

namespace EFlow.Booking.WebApi.Contracts.Auth;

public record RegisterRequest
{
	public required string Username { get; init; }

	public required string Password { get; init; }

	public required Identity.Role Role { get; init; }
}
