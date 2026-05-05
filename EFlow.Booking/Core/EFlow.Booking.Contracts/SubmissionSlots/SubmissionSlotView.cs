using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Contracts.Subjects;
using EFlow.Booking.Contracts.Teachers;

namespace EFlow.Booking.Contracts.SubmissionSlots;

public sealed record SubmissionSlotView
{
    public required Guid Id { get; init; }

    public required DateTime StartTime { get; init; }

    public required DateTime EndTime { get; init; }

    public required int MaxStudents { get; init; }

    public required int BookingCount { get; init; }

    public required bool AllowAllGroups { get; init; }

    public string? Location { get; init; }

    public string? Comment { get; init; }
    
    public SubjectView? Subject { get; init; }

    public TeacherView? Teacher { get; init; }
    
    public IReadOnlyCollection<GroupView>? AllowedGroups { get; init; }

    public IReadOnlyCollection<StudentView>? AdmittedStudents { get; init; }

    public IReadOnlyCollection<SubmissionSlotNotificationSettingsView>? NotificationSettings { get; init; }
}
