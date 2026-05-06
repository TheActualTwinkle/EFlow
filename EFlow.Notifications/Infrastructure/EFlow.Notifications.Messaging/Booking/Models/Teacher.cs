namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record Teacher
{
    public required Guid Id { get; init; }

    public required string FirstName { get; init; }
    
    public required string LastName { get; init; }
    
    public string? MiddleName { get; init; }
}