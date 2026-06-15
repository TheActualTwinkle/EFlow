namespace EFlow.DataImport.Messaging.Booking.Models;

public sealed record CurrentUserView
{
    public required IReadOnlyList<string> Roles { get; init; }
}
