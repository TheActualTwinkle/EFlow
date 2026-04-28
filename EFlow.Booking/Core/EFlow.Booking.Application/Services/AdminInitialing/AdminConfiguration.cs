using System.ComponentModel.DataAnnotations;

namespace EFlow.Booking.Application.Services.AdminInitialing;

public sealed record AdminConfiguration
{
    public const string SectionName = "AdminConfiguration";
    
    public required string Username { get; init; }

    public required string Password { get; init; }

    [EmailAddress]
    public required string Email { get; init; }
}