using MemoryPack;

namespace EFlow.Common.IntegrationEvents.Booking.Models;

[MemoryPackable]
public sealed partial record SubmissionSlotModel
{
    public required Guid Id { get; init; }
    
    public required string SubjectName { get; init; }
    
    public required string TeacherFullName { get; init; }
    
    public required DateTime StartTime { get; init; }
    
    public required DateTime EndTime { get; init; }
    
    public string? Location { get; init; }
    
    public string? Comment { get; init; }
    
    public required int MaxStudents { get; init; }

    public required bool AllowAllGroups { get; init; }

    public required IEnumerable<string> AllowedGroupNames { get; init; }
}