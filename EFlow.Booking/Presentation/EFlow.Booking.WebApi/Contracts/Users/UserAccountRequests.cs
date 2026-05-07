using System.ComponentModel.DataAnnotations;

namespace EFlow.Booking.WebApi.Contracts.Users;

public sealed record UpdateUserEmailRequest
{
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }
}

public sealed record UpdateUserPasswordRequest
{
    [MinLength(6)]
    [MaxLength(63)]
    public required string CurrentPassword { get; init; }

    [MinLength(6)]
    [MaxLength(63)]
    public required string NewPassword { get; init; }
}
