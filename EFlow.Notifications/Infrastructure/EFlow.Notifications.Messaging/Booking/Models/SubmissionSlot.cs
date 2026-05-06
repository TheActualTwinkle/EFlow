namespace EFlow.Notifications.Messaging.Booking.Models;

public sealed record SubmissionSlot
{
    public required Guid Id { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public int? BookingCount { get; init; }

    public required bool AllowAllGroups { get; init; }

    public required ICollection<Group> AllowedGroups { get; init; }
    
    public string? Location { get; init; }

    public string? Comment { get; init; }
    
    public required Subject Subject { get; init; }

    public required Teacher Teacher { get; init; }
}