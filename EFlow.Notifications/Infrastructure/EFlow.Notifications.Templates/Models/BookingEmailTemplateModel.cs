namespace EFlow.Notifications.Templates.Models;

public sealed record BookingEmailTemplateModel
{
    public required string Title { get; init; }

    public required string Lead { get; init; }

    public required string AccentLabel { get; init; }

    public required string SubjectName { get; init; }

    public required string TeacherName { get; init; }

    public required string StudentFullName { get; init; }

    public required string TimeRange { get; init; }

    public required string Location { get; init; }

    public required string Footer { get; init; }
}
