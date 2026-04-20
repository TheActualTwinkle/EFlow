namespace EFlow.Booking.WebApi.Contracts.Auth;

public record LoginRequest
{
	public required string Username { get; init; }

	public required string Password { get; init; }
}
