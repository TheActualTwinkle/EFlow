namespace EFlow.Notifications.Templates.Models;

public sealed record ReminderEmailTemplateModel
{
    public required string Title { get; init; }

    public required string Lead { get; init; }

    public required string SubjectName { get; init; }

    public required string TeacherName { get; init; }

    public required string TimeRange { get; init; }

    public required string Location { get; init; }

    public required string Audience { get; init; }
}
