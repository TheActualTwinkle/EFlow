namespace EFlow.Notifications.Templates.Models;

public sealed record SubmissionSlotDeletedEmailTemplateModel
{
    public required string Title { get; init; }

    public required string Lead { get; init; }

    public required string SubjectName { get; init; }

    public required string TeacherFullName { get; init; }

    public required string TimeRange { get; init; }

    public required string Location { get; init; }

    public required string Comment { get; init; }
}
