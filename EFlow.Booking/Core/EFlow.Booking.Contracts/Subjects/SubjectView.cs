using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Teachers;

namespace EFlow.Booking.Contracts.Subjects;

public sealed record SubjectView
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required TeacherView? Teacher { get; init; }
    
    public required IEnumerable<GroupView>? Groups { get; init; }
}