namespace EFlow.Notifications.Templates.Models;

public sealed record SubmissionSlotUpdatedEmailTemplateModel
{
    public required string Title { get; init; }

    public required string Lead { get; init; }

    public required string SubjectName { get; init; }

    public required string TeacherName { get; init; }

    public required string CurrentTimeRange { get; init; }

    public required ChangedItem[] Changes { get; init; }
    
    public sealed record ChangedItem
    {
        public required string Label { get; init; }

        public required string OldValue { get; init; }

        public required string NewValue { get; init; }
    }
}